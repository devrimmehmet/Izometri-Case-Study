using ExpenseService.Application.DTOs;
using ExpenseService.Application.Validators;
using ExpenseService.Domain.Enums;

namespace ExpenseService.Tests;

public sealed class ExpenseValidatorTests
{
    [Fact]
    public void CreateExpenseRequest_requires_minimum_description_length()
    {
        var validator = new CreateExpenseRequestValidator();
        var request = new CreateExpenseRequest(ExpenseCategory.Travel, ExpenseCurrency.TRY, 100m, null, "short");

        var result = validator.Validate(request);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, x => x.PropertyName == nameof(CreateExpenseRequest.Description));
    }

    [Fact]
    public void RejectExpenseRequest_requires_minimum_reason_length()
    {
        var validator = new RejectExpenseRequestValidator();
        var request = new RejectExpenseRequest("too short");

        var result = validator.Validate(request);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, x => x.PropertyName == nameof(RejectExpenseRequest.Reason));
    }
}
