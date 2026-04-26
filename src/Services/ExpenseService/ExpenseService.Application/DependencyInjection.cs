using ExpenseService.Application.Services;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace ExpenseService.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddExpenseApplication(this IServiceCollection services)
    {
        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IExpenseAppService, ExpenseAppService>();
        return services;
    }
}
