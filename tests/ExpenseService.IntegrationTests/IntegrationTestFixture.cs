using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using NotificationService.Application.Abstractions;
using Testcontainers.PostgreSql;
using Testcontainers.RabbitMq;

namespace ExpenseService.Tests;

public sealed class IntegrationTestFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer _expenseDb = new PostgreSqlBuilder("postgres:16")
        .WithDatabase("expense_db")
        .WithUsername("postgres")
        .WithPassword("postgres")
        .Build();

    private readonly PostgreSqlContainer _notificationDb = new PostgreSqlBuilder("postgres:16")
        .WithDatabase("notification_db")
        .WithUsername("postgres")
        .WithPassword("postgres")
        .Build();

    private readonly RabbitMqContainer _rabbitMq = new RabbitMqBuilder("rabbitmq:3-management")
        .WithUsername("izometri")
        .WithPassword("Izometri2026!")
        .Build();

    private WebApplicationFactory<ExpenseService.Api.ExpenseApiMarker> _expenseFactory = null!;
    private WebApplicationFactory<NotificationService.Api.NotificationApiMarker> _notificationFactory = null!;

    public HttpClient ExpenseClient { get; private set; } = null!;
    public HttpClient NotificationClient { get; private set; } = null!;

    public async Task InitializeAsync()
    {
        await Task.WhenAll(
            _expenseDb.StartAsync(),
            _notificationDb.StartAsync(),
            _rabbitMq.StartAsync());

        var rabbitHost = _rabbitMq.Hostname;
        var rabbitPort = _rabbitMq.GetMappedPublicPort(5672);

        _expenseFactory = new WebApplicationFactory<ExpenseService.Api.ExpenseApiMarker>()
            .WithWebHostBuilder(builder =>
            {
                builder.UseSetting("ConnectionStrings:ExpenseDb", _expenseDb.GetConnectionString());
                builder.UseSetting("RabbitMq:HostName", rabbitHost);
                builder.UseSetting("RabbitMq:Port", rabbitPort.ToString());
                builder.UseSetting("RabbitMq:UserName", "izometri");
                builder.UseSetting("RabbitMq:Password", "Izometri2026!");
            });

        ExpenseClient = _expenseFactory.CreateClient();

        // ExpenseDetailsClient kaldırıldı: NotificationService artık event payload'ından
        // Amount/Currency/RecipientRole okur; ExpenseService'e HTTP çağrısı yapmaz.
        _notificationFactory = new WebApplicationFactory<NotificationService.Api.NotificationApiMarker>()
            .WithWebHostBuilder(builder =>
            {
                builder.UseSetting("ConnectionStrings:NotificationDb", _notificationDb.GetConnectionString());
                builder.UseSetting("RabbitMq:HostName", rabbitHost);
                builder.UseSetting("RabbitMq:Port", rabbitPort.ToString());
                builder.UseSetting("RabbitMq:UserName", "izometri");
                builder.UseSetting("RabbitMq:Password", "Izometri2026!");

                builder.ConfigureTestServices(services =>
                {
                    services.AddTransient<IEmailSender, NoopEmailSender>();
                    services.AddTransient<ISmsService, NoopSmsService>();
                });
            });

        NotificationClient = _notificationFactory.CreateClient();

        // Allow background services (RabbitMQ consumer, outbox publisher) time to connect
        await Task.Delay(TimeSpan.FromSeconds(3));
    }

    public async Task DisposeAsync()
    {
        await _expenseFactory.DisposeAsync();
        await _notificationFactory.DisposeAsync();
        await Task.WhenAll(
            _expenseDb.DisposeAsync().AsTask(),
            _notificationDb.DisposeAsync().AsTask(),
            _rabbitMq.DisposeAsync().AsTask());
    }

    private sealed class NoopEmailSender : IEmailSender
    {
        public Task SendAsync(string toEmail, string subject, string body, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }

    private sealed class NoopSmsService : ISmsService
    {
        public Task SendAsync(string toPhone, string message, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
