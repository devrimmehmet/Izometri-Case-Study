using NotificationService.Application.Abstractions;
using NotificationService.Application.Services;
using NotificationService.Infrastructure.Auth;
using NotificationService.Infrastructure.Clients;
using NotificationService.Infrastructure.Messaging;
using NotificationService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace NotificationService.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddNotificationInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<RabbitMqOptions>(configuration.GetSection("RabbitMq"));
        services.Configure<JwtOptions>(configuration.GetSection("Jwt"));
        services.Configure<ExpenseServiceOptions>(configuration.GetSection("ExpenseService"));

        services.AddDbContext<NotificationDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("NotificationDb")));

        services.AddScoped<INotificationStore, NotificationStore>();
        services.AddScoped<NotificationDeadLetterStore>();
        services.AddScoped<INotificationQueryService, NotificationQueryService>();
        services.AddScoped<INotificationDeadLetterAdminService, NotificationDeadLetterQueryService>();
        services.AddSingleton<ServiceTokenFactory>();
        services.AddHttpClient<IExpenseDetailsClient, ExpenseDetailsClient>((sp, client) =>
        {
            var options = configuration.GetSection("ExpenseService").Get<ExpenseServiceOptions>() ?? new ExpenseServiceOptions();
            client.BaseAddress = new Uri(options.BaseUrl);
        }).AddStandardResilienceHandler();
        services.AddHostedService<DatabaseMigrationHostedService>();
        services.AddHostedService<RabbitMqConsumerWorker>();
        return services;
    }
}
