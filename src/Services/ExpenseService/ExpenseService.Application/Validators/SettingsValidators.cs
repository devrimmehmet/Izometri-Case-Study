using ExpenseService.Application.DTOs;
using FluentValidation;

namespace ExpenseService.Application.Validators;

public sealed class UpdateRatesRequestValidator : AbstractValidator<UpdateRatesRequest>
{
    public UpdateRatesRequestValidator()
    {
        RuleFor(x => x.FixedUsdRate)
            .GreaterThan(0)
            .When(x => x.FixedUsdRate.HasValue)
            .WithMessage("USD kuru 0'dan büyük olmalıdır.");

        RuleFor(x => x.FixedEurRate)
            .GreaterThan(0)
            .When(x => x.FixedEurRate.HasValue)
            .WithMessage("EUR kuru 0'dan büyük olmalıdır.");
    }
}
