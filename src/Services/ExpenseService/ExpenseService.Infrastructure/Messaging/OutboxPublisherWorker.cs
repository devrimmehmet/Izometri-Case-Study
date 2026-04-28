using System.Diagnostics;
using System.Text;
using ExpenseManagement.Contracts;
using ExpenseService.Domain.Entities;
using ExpenseService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;

namespace ExpenseService.Infrastructure.Messaging;

public sealed class OutboxPublisherWorker : BackgroundService
{
    private static readonly ActivitySource ActivitySource = new("ExpenseService.Messaging");
    private readonly ILogger<OutboxPublisherWorker> _logger;
    private readonly DatabaseMigrationState _migrationState;
    private readonly RabbitMqOptions _options;
    private readonly IServiceScopeFactory _scopeFactory;

    // Persistent connection — yeni TCP bağlantısı her poll'da açılmaz.
    private IConnection? _connection;
    private IChannel? _channel;

    public OutboxPublisherWorker(IServiceScopeFactory scopeFactory, IOptions<RabbitMqOptions> options, ILogger<OutboxPublisherWorker> logger, DatabaseMigrationState migrationState)
    {
        _scopeFactory = scopeFactory;
        _options = options.Value;
        _logger = logger;
        _migrationState = migrationState;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await _migrationState.Ready.WaitAsync(stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await EnsureChannelAsync(stoppingToken);
                await PublishPendingAsync(stoppingToken);
            }
            catch (OperationCanceledException)
            {
                return;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Outbox publishing failed. Will reconnect on next cycle.");
                await DisposeChannelAsync();
            }

            await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
        }
    }

    private async Task EnsureChannelAsync(CancellationToken cancellationToken)
    {
        if (_channel is { IsOpen: true })
            return;

        await DisposeChannelAsync();

        var factory = new ConnectionFactory
        {
            HostName = _options.HostName,
            Port = _options.Port,
            UserName = _options.UserName,
            Password = _options.Password
        };

        _connection = await factory.CreateConnectionAsync(cancellationToken);
        _channel = await _connection.CreateChannelAsync(cancellationToken: cancellationToken);
        await _channel.ExchangeDeclareAsync(ExpenseEventNames.Exchange, ExchangeType.Topic, durable: true, cancellationToken: cancellationToken);

        _logger.LogInformation("Outbox publisher connected to RabbitMQ.");
    }

    private async Task PublishPendingAsync(CancellationToken cancellationToken)
    {
        await using var scope = _scopeFactory.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ExpenseDbContext>();

        // SKIP LOCKED: birden fazla publisher instance çalışıyorsa (horizontal scale),
        // PostgreSQL satır kilidi bu mesajların başka bir worker tarafından çift publish
        // edilmesini önler. FOR UPDATE SKIP LOCKED kilitsiz satırları atlar; böylece
        // worker'lar birbirini bloklamaz.
        await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);

        var messages = await dbContext.OutboxMessages
            .FromSqlRaw(
                """
                SELECT * FROM "OutboxMessages"
                WHERE NOT "IsDeleted"
                  AND "ProcessedAt" IS NULL
                  AND "DeadLetteredAt" IS NULL
                ORDER BY "CreatedAt"
                LIMIT 20
                FOR UPDATE SKIP LOCKED
                """)
            .ToListAsync(cancellationToken);

        if (messages.Count == 0)
        {
            await transaction.RollbackAsync(cancellationToken);
            return;
        }

        foreach (var message in messages)
        {
            using var activity = ActivitySource.StartActivity("outbox.publish", ActivityKind.Producer);
            activity?.SetTag("messaging.system", "rabbitmq");
            activity?.SetTag("messaging.destination", message.RoutingKey);
            activity?.SetTag("messaging.message.id", message.Id.ToString());
            try
            {
                var body = Encoding.UTF8.GetBytes(message.Payload);
                await _channel!.BasicPublishAsync(
                    exchange: ExpenseEventNames.Exchange,
                    routingKey: message.RoutingKey,
                    mandatory: false,
                    body: body,
                    cancellationToken: cancellationToken);

                message.ProcessedAt = DateTime.UtcNow;
                message.Error = null;
                activity?.SetStatus(ActivityStatusCode.Ok);
            }
            catch (Exception ex)
            {
                message.RetryCount++;
                message.Error = ex.Message;
                if (message.RetryCount >= 10)
                    message.DeadLetteredAt = DateTime.UtcNow;

                activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
                _logger.LogError(ex, "Failed to publish outbox message {MessageId}.", message.Id);
            }
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
    }

    private async Task DisposeChannelAsync()
    {
        try
        {
            if (_channel is not null) { await _channel.DisposeAsync(); _channel = null; }
            if (_connection is not null) { await _connection.DisposeAsync(); _connection = null; }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error disposing RabbitMQ connection.");
        }
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        await base.StopAsync(cancellationToken);
        await DisposeChannelAsync();
    }
}
