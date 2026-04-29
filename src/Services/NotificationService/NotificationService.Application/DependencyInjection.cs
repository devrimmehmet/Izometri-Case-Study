using FluentValidation;
using NotificationService.Application.Abstractions;
using NotificationService.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace NotificationService.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddNotificationApplication(this IServiceCollection services)
    {
        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);
        services.AddScoped<INotificationEventHandler, NotificationEventHandler>();
        return services;
    }
}
