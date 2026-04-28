using ExpenseService.Application.Abstractions;
using ExpenseService.Infrastructure.Auth;
using ExpenseService.Infrastructure.Contexts;
using ExpenseService.Infrastructure.Messaging;
using ExpenseService.Infrastructure.Persistence;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ExpenseService.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddExpenseInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<JwtOptions>(configuration.GetSection("Jwt"));
        services.Configure<RabbitMqOptions>(configuration.GetSection("RabbitMq"));
        services.Configure<KeycloakAdminOptions>(configuration.GetSection("Keycloak"));

        services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
        services.AddScoped<ICurrentUserContext, HttpCurrentUserContext>();
        services.AddScoped<ICorrelationContext, CorrelationContext>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddSingleton<IPasswordHasher, PasswordHasher>();
        services.AddSingleton<IJwtTokenService, JwtTokenService>();
        
        services.AddHttpClient<IExchangeRateService, ExpenseService.Infrastructure.Services.TcmbExchangeRateService>();

        // Keycloak Admin API client — BaseUrl opsiyonel; Enabled=false ise no-op.
        var keycloakBaseUrl = configuration["Keycloak:BaseUrl"];
        services.AddHttpClient<IKeycloakAdminClient, KeycloakAdminClient>(client =>
        {
            if (!string.IsNullOrWhiteSpace(keycloakBaseUrl))
            {
                client.BaseAddress = new Uri(keycloakBaseUrl.TrimEnd('/'));
            }
        });

        services.AddDbContext<ExpenseDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("ExpenseDb")));

        services.AddSingleton<DatabaseMigrationState>();
        services.AddHostedService<DatabaseMigrationHostedService>();
        services.AddHostedService<OutboxPublisherWorker>();
        services.AddHostedService<ExpenseService.Infrastructure.Services.ExchangeRateInitializerHostedService>();
        return services;
    }
}
