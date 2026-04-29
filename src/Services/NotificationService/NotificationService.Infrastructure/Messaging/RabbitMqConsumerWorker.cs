using System.Diagnostics;
using System.Text;
using System.Text.Json;
using ExpenseManagement.Contracts;
using NotificationService.Application.Abstractions;
using NotificationService.Infrastructure.Persistence;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace NotificationService.Infrastructure.Messaging;

public sealed class RabbitMqConsumerWorker : BackgroundService
{
    private static readonly ActivitySource ActivitySource = new("NotificationService.Messaging");
    private readonly ILogger<RabbitMqConsumerWorker> _logger;
    private readonly DatabaseMigrationState _migrationState;
    private readonly RabbitMqOptions _options;
    private readonly IServiceScopeFactory _scopeFactory;

    public RabbitMqConsumerWorker(IServiceScopeFactory scopeFactory, IOptions<RabbitMqOptions> options, ILogger<RabbitMqConsumerWorker> logger, DatabaseMigrationState migrationState)
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
                await ConsumeAsync(stoppingToken);
            }
            catch (OperationCanceledException)
            {
                return;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "RabbitMQ consumer stopped unexpectedly.");
                await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
            }
        }
    }

    private async Task ConsumeAsync(CancellationToken cancellationToken)
    {
        var factory = new ConnectionFactory
        {
            HostName = _options.HostName,
            Port = _options.Port,
            UserName = _options.UserName,
            Password = _options.Password
        };

        await using var connection = await factory.CreateConnectionAsync(cancellationToken);
        await using var channel = await connection.CreateChannelAsync(cancellationToken: cancellationToken);

        await channel.ExchangeDeclareAsync(ExpenseEventNames.Exchange, ExchangeType.Topic, durable: true, cancellationToken: cancellationToken);
        await channel.QueueDeclareAsync(ExpenseEventNames.NotificationQueue, durable: true, exclusive: false, autoDelete: false, cancellationToken: cancellationToken);
        await channel.QueueBindAsync(ExpenseEventNames.NotificationQueue, ExpenseEventNames.Exchange, ExpenseEventNames.ExpenseCreated, cancellationToken: cancellationToken);
        await channel.QueueBindAsync(ExpenseEventNames.NotificationQueue, ExpenseEventNames.Exchange, ExpenseEventNames.ExpenseApproved, cancellationToken: cancellationToken);
        await channel.QueueBindAsync(ExpenseEventNames.NotificationQueue, ExpenseEventNames.Exchange, ExpenseEventNames.ExpenseRejected, cancellationToken: cancellationToken);
        await channel.QueueBindAsync(ExpenseEventNames.NotificationQueue, ExpenseEventNames.Exchange, ExpenseEventNames.ExpenseRequiresAdminApproval, cancellationToken: cancellationToken);

        var consumer = new AsyncEventingBasicConsumer(channel);
        consumer.ReceivedAsync += async (_, ea) =>
        {
            var payload = Encoding.UTF8.GetString(ea.Body.ToArray());
            var correlationId = ResolveCorrelationId(ea, payload);
            using var activity = ActivitySource.StartActivity("event.consume", ActivityKind.Consumer);
            activity?.SetTag("messaging.system", "rabbitmq");
            activity?.SetTag("messaging.destination", ea.RoutingKey);
            activity?.SetTag("messaging.message.id", ea.BasicProperties.MessageId);
            activity?.SetTag("correlation.id", correlationId);
            using var logScope = _logger.BeginScope(new Dictionary<string, object> { ["CorrelationId"] = correlationId });
            try
            {
                await using var serviceScope = _scopeFactory.CreateAsyncScope();
                var handler = serviceScope.ServiceProvider.GetRequiredService<INotificationEventHandler>();
                await handler.HandleAsync(ea.RoutingKey, payload, cancellationToken);
                await channel.BasicAckAsync(ea.DeliveryTag, multiple: false, cancellationToken);
                activity?.SetStatus(ActivityStatusCode.Ok);
            }
            catch (Exception ex)
            {
                activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
                _logger.LogError(ex, "Failed to process RabbitMQ event {RoutingKey}.", ea.RoutingKey);
                await using var serviceScope = _scopeFactory.CreateAsyncScope();
                var deadLetterStore = serviceScope.ServiceProvider.GetRequiredService<NotificationDeadLetterStore>();
                var retryCount = await deadLetterStore.RecordFailureAsync(ea.RoutingKey, payload, ex.Message, cancellationToken);
                if (NotificationDeadLetterStore.ShouldDeadLetter(retryCount))
                {
                    _logger.LogError("RabbitMQ event {RoutingKey} moved to notification dead-letter table after {RetryCount} retries.", ea.RoutingKey, retryCount);
                    await channel.BasicAckAsync(ea.DeliveryTag, multiple: false, cancellationToken);
                    return;
                }

                await channel.BasicNackAsync(ea.DeliveryTag, multiple: false, requeue: true, cancellationToken);
            }
        };

        await channel.BasicConsumeAsync(ExpenseEventNames.NotificationQueue, autoAck: false, consumer, cancellationToken);
        await Task.Delay(Timeout.InfiniteTimeSpan, cancellationToken);
    }

    private static string ResolveCorrelationId(BasicDeliverEventArgs ea, string payload)
    {
        if (!string.IsNullOrWhiteSpace(ea.BasicProperties.CorrelationId))
        {
            return ea.BasicProperties.CorrelationId;
        }

        if (ea.BasicProperties.Headers?.TryGetValue("X-Correlation-Id", out var headerValue) == true)
        {
            return headerValue switch
            {
                null => string.Empty,
                byte[] bytes => Encoding.UTF8.GetString(bytes),
                string text => text,
                _ => headerValue.ToString() ?? string.Empty
            };
        }

        try
        {
            using var document = JsonDocument.Parse(payload);
            if (document.RootElement.TryGetProperty("CorrelationId", out var correlationProperty))
            {
                return correlationProperty.GetString() ?? string.Empty;
            }
        }
        catch (JsonException)
        {
            return string.Empty;
        }

        return string.Empty;
    }
}
