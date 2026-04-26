using NotificationService.Application.Abstractions;
using NotificationService.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace NotificationService.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddNotificationApplication(this IServiceCollection services)
    {
        services.AddScoped<INotificationEventHandler, NotificationEventHandler>();
        return services;
    }
}
