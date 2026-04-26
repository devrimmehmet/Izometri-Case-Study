using System.Text;
using ExpenseManagement.Contracts;
using NotificationService.Application.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace NotificationService.Infrastructure.Messaging;

public sealed class RabbitMqConsumerWorker : BackgroundService
{
    private readonly ILogger<RabbitMqConsumerWorker> _logger;
    private readonly RabbitMqOptions _options;
    private readonly IServiceScopeFactory _scopeFactory;

    public RabbitMqConsumerWorker(IServiceScopeFactory scopeFactory, IOptions<RabbitMqOptions> options, ILogger<RabbitMqConsumerWorker> logger)
    {
        _scopeFactory = scopeFactory;
        _options = options.Value;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
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

        var consumer = new AsyncEventingBasicConsumer(channel);
        consumer.ReceivedAsync += async (_, ea) =>
        {
            var payload = Encoding.UTF8.GetString(ea.Body.ToArray());
            try
            {
                await using var scope = _scopeFactory.CreateAsyncScope();
                var handler = scope.ServiceProvider.GetRequiredService<INotificationEventHandler>();
                await handler.HandleAsync(ea.RoutingKey, payload, cancellationToken);
                await channel.BasicAckAsync(ea.DeliveryTag, multiple: false, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to process RabbitMQ event {RoutingKey}.", ea.RoutingKey);
                await channel.BasicNackAsync(ea.DeliveryTag, multiple: false, requeue: true, cancellationToken);
            }
        };

        await channel.BasicConsumeAsync(ExpenseEventNames.NotificationQueue, autoAck: false, consumer, cancellationToken);
        await Task.Delay(Timeout.InfiniteTimeSpan, cancellationToken);
    }
}
