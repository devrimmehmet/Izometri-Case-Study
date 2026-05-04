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

        // Her event tipi için strateji kaydedilir.
        // Yeni event = sadece buraya yeni satır; handler'a dokunulmaz (OCP).
        services.AddScoped<IExpenseNotificationStrategy, ExpenseCreatedStrategy>();
        services.AddScoped<IExpenseNotificationStrategy, ExpenseApprovedStrategy>();
        services.AddScoped<IExpenseNotificationStrategy, ExpenseRejectedStrategy>();
        services.AddScoped<IExpenseNotificationStrategy, ExpenseRequiresAdminApprovalStrategy>();

        return services;
    }
}
