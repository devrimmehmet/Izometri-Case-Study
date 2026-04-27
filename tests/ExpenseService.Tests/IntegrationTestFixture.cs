using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using NotificationService.Application.Abstractions;
using NotificationService.Infrastructure.Auth;
using NotificationService.Infrastructure.Clients;
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
        .WithUsername("guest")
        .WithPassword("guest")
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
                builder.UseSetting("RabbitMq:UserName", "guest");
                builder.UseSetting("RabbitMq:Password", "guest");
            });

        ExpenseClient = _expenseFactory.CreateClient();

        var expenseHandler = _expenseFactory.Server.CreateHandler();

        _notificationFactory = new WebApplicationFactory<NotificationService.Api.NotificationApiMarker>()
            .WithWebHostBuilder(builder =>
            {
                builder.UseSetting("ConnectionStrings:NotificationDb", _notificationDb.GetConnectionString());
                builder.UseSetting("RabbitMq:HostName", rabbitHost);
                builder.UseSetting("RabbitMq:Port", rabbitPort.ToString());
                builder.UseSetting("RabbitMq:UserName", "guest");
                builder.UseSetting("RabbitMq:Password", "guest");
                builder.UseSetting("ExpenseService:BaseUrl", "http://expense-test/");

                builder.ConfigureTestServices(services =>
                {
                    // Route NotificationService → ExpenseService HTTP calls through the in-process test server
                    services.AddTransient<IExpenseDetailsClient>(sp =>
                    {
                        var httpClient = new HttpClient(expenseHandler, disposeHandler: false)
                        {
                            BaseAddress = new Uri("http://localhost/")
                        };
                        var tokenFactory = sp.GetRequiredService<ServiceTokenFactory>();
                        return new ExpenseDetailsClient(httpClient, tokenFactory);
                    });
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
