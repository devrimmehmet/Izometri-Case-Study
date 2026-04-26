using ExpenseService.Domain.Entities;
using ExpenseService.Domain.Enums;

namespace ExpenseService.Tests;

public sealed class ExpenseRuleTests
{
    [Theory]
    [InlineData(5000, false)]
    [InlineData(5000.01, true)]
    public void Try_expense_above_5000_requires_admin_approval(decimal amount, bool expected)
    {
        var expense = new Expense
        {
            Amount = amount,
            Currency = ExpenseCurrency.TRY
        };

        Assert.Equal(expected, expense.RequiresAdminApproval);
    }

    [Fact]
    public void Non_try_expense_does_not_trigger_try_threshold_rule()
    {
        var expense = new Expense
        {
            Amount = 9000m,
            Currency = ExpenseCurrency.USD
        };

        Assert.False(expense.RequiresAdminApproval);
    }
}
