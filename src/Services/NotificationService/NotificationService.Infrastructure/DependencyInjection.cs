using NotificationService.Application.Abstractions;
using NotificationService.Application.Services;
using NotificationService.Infrastructure.Auth;
using NotificationService.Infrastructure.Contexts;
using NotificationService.Infrastructure.Messaging;
using NotificationService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace NotificationService.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddNotificationInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<RabbitMqOptions>(configuration.GetSection("RabbitMq"));
        services.Configure<JwtOptions>(configuration.GetSection("Jwt"));
        // ExpenseServiceOptions kaldırıldı: ExpenseDetailsClient artık kullanılmıyor.
        services.Configure<SmtpOptions>(options =>
        {
            configuration.GetSection("Smtp").Bind(options);
            configuration.GetSection("Mail").Bind(options);
        });
        services.Configure<NetgsmOptions>(configuration.GetSection("Netgsm"));

        services.AddDbContext<NotificationDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("NotificationDb")));

        services.AddSingleton<DatabaseMigrationState>();
        services.AddScoped<INotificationStore, NotificationStore>();
        services.AddScoped<NotificationDeadLetterStore>();
        services.AddScoped<INotificationQueryService, NotificationQueryService>();
        services.AddScoped<INotificationDeadLetterAdminService, NotificationDeadLetterQueryService>();
        // ServiceTokenFactory ve ExpenseDetailsClient kaldırıldı:
        // event'ler artık Amount/Currency/RecipientRole taşıdığından inter-service HTTP çağrısı gerekmez.
        services.AddTransient<IEmailSender, SmtpEmailSender>();
        services.AddHttpClient<ISmsService, NetgsmSmsSender>();
        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUserContext, HttpCurrentUserContext>();

        services.AddHostedService<DatabaseMigrationHostedService>();
        services.AddHostedService<RabbitMqConsumerWorker>();
        services.AddHostedService<RetentionWorker>();
        return services;
    }
}
