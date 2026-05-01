using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace NotificationService.Api;

public sealed class ValidationActionFilter : IAsyncActionFilter
{
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var validationErrors = new Dictionary<string, string[]>();

        foreach (var argument in context.ActionArguments.Values.Where(x => x is not null))
        {
            var argumentType = argument!.GetType();
            var validatorType = typeof(IValidator<>).MakeGenericType(argumentType);
            if (context.HttpContext.RequestServices.GetService(validatorType) is not IValidator validator)
            {
                continue;
            }

            var validationContextType = typeof(ValidationContext<>).MakeGenericType(argumentType);
            var validationContext = (IValidationContext)Activator.CreateInstance(validationContextType, argument)!;
            var result = await validator.ValidateAsync(validationContext, context.HttpContext.RequestAborted);

            foreach (var group in result.Errors.Where(x => x is not null).GroupBy(x => x.PropertyName))
            {
                validationErrors[group.Key] = group.Select(x => x.ErrorMessage).ToArray();
            }
        }

        if (validationErrors.Count > 0)
        {
            context.Result = new BadRequestObjectResult(new ValidationProblemDetails(validationErrors)
            {
                Title = "Validation failed.",
                Status = StatusCodes.Status400BadRequest
            });
            return;
        }

        await next();
    }
}
