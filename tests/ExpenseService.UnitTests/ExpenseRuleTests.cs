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
            ExchangeRate = 1,
            Currency = ExpenseCurrency.TRY
        };

        Assert.Equal(expected, expense.RequiresAdminApproval);
    }

    [Fact]
    public void Non_try_expense_triggers_threshold_rule_based_on_exchange_rate()
    {
        var expense1 = new Expense
        {
            Amount = 145m,
            ExchangeRate = 33m,
            Currency = ExpenseCurrency.USD
        };
        Assert.False(expense1.RequiresAdminApproval);

        var expense2 = new Expense
        {
            Amount = 145m,
            ExchangeRate = 35m,
            Currency = ExpenseCurrency.USD
        };
        Assert.True(expense2.RequiresAdminApproval);
    }
}
