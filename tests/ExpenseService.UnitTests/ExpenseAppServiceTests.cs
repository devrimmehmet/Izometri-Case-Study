using System.Collections;
using System.Linq.Expressions;
using System.Text.Json;
using ExpenseManagement.Contracts;
using ExpenseService.Application.Abstractions;
using ExpenseService.Application.DTOs;
using ExpenseService.Application.Services;
using ExpenseService.Domain.Common;
using ExpenseService.Domain.Entities;
using ExpenseService.Domain.Enums;
using Microsoft.EntityFrameworkCore.Query;
using Moq;

namespace ExpenseService.Tests;

public sealed class ExpenseAppServiceTests
{
    [Fact]
    public async Task CreateAsync_creates_expense_and_outbox_message_in_transaction()
    {
        var tenantId = Guid.NewGuid();
        var requesterId = Guid.NewGuid();
        var addedExpenses = new List<Expense>();
        var addedOutboxMessages = new List<OutboxMessage>();
        var users = new List<User>
        {
            User(tenantId, Guid.NewGuid(), "hr@test1.com", Roles.HR),
            User(tenantId, Guid.NewGuid(), "admin@test1.com", Roles.Admin)
        };

        var service = CreateService(
            tenantId,
            requesterId,
            roles: new[] { Roles.Personel },
            users: users,
            expenses: Array.Empty<Expense>(),
            addedExpenses: addedExpenses,
            addedOutboxMessages: addedOutboxMessages,
            exchangeRate: 35m,
            correlationId: "corr-create");

        var response = await service.CreateAsync(
            new CreateExpenseRequest(ExpenseCategory.Travel, ExpenseCurrency.USD, 100, null, "International travel expense description"),
            CancellationToken.None);

        var expense = Assert.Single(addedExpenses);
        Assert.Equal(response.Id, expense.Id);
        Assert.Equal(tenantId, expense.TenantId);
        Assert.Equal(requesterId, expense.RequestedByUserId);
        Assert.Equal(35m, expense.ExchangeRate);
        Assert.Equal(ExpenseStatus.Draft, expense.Status);

        var outbox = Assert.Single(addedOutboxMessages);
        Assert.Equal(ExpenseEventNames.ExpenseCreated, outbox.RoutingKey);
        Assert.Equal("corr-create", outbox.CorrelationId);

        using var document = JsonDocument.Parse(outbox.Payload);
        Assert.Equal("corr-create", document.RootElement.GetProperty("CorrelationId").GetString());
        Assert.Equal(expense.Id, document.RootElement.GetProperty("ExpenseId").GetGuid());
        Assert.Contains("hr@test1.com", document.RootElement.GetProperty("RecipientEmail").GetString());
        Assert.Contains("admin@test1.com", document.RootElement.GetProperty("RecipientEmail").GetString());
    }

    [Fact]
    public async Task GetListAsync_limits_personel_to_own_expenses()
    {
        var tenantId = Guid.NewGuid();
        var requesterId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();
        var ownExpense = Expense(tenantId, requesterId, ExpenseStatus.Draft, 100);
        var otherExpense = Expense(tenantId, otherUserId, ExpenseStatus.Draft, 200);

        var service = CreateService(
            tenantId,
            requesterId,
            roles: new[] { Roles.Personel },
            users: Array.Empty<User>(),
            expenses: new[] { ownExpense, otherExpense });

        var response = await service.GetListAsync(new ExpenseQuery(null, null, null, null), CancellationToken.None);

        var item = Assert.Single(response.Items);
        Assert.Equal(ownExpense.Id, item.Id);
        Assert.Equal(1, response.TotalCount);
    }

    [Fact]
    public async Task GetListAsync_allows_hr_to_see_tenant_expenses()
    {
        var tenantId = Guid.NewGuid();
        var hrId = Guid.NewGuid();
        var firstExpense = Expense(tenantId, Guid.NewGuid(), ExpenseStatus.Pending, 100);
        var secondExpense = Expense(tenantId, Guid.NewGuid(), ExpenseStatus.Approved, 200);

        var service = CreateService(
            tenantId,
            hrId,
            roles: new[] { Roles.HR },
            users: Array.Empty<User>(),
            expenses: new[] { firstExpense, secondExpense });

        var response = await service.GetListAsync(new ExpenseQuery(null, null, null, null), CancellationToken.None);

        Assert.Equal(2, response.TotalCount);
        Assert.Contains(response.Items, x => x.Id == firstExpense.Id);
        Assert.Contains(response.Items, x => x.Id == secondExpense.Id);
    }

    [Fact]
    public async Task SubmitAsync_rejects_non_requester()
    {
        var tenantId = Guid.NewGuid();
        var requesterId = Guid.NewGuid();
        var currentUserId = Guid.NewGuid();
        var expense = Expense(tenantId, requesterId, ExpenseStatus.Draft, 100);

        var service = CreateService(
            tenantId,
            currentUserId,
            roles: new[] { Roles.Personel },
            users: Array.Empty<User>(),
            expenses: new[] { expense });

        var exception = await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => service.SubmitAsync(expense.Id, CancellationToken.None));

        Assert.Equal("Only requester can submit the expense.", exception.Message);
        Assert.Equal(ExpenseStatus.Draft, expense.Status);
    }

    [Fact]
    public async Task ApproveAsync_hr_approval_for_high_amount_creates_admin_approval_notification()
    {
        var tenantId = Guid.NewGuid();
        var hrId = Guid.NewGuid();
        var requesterId = Guid.NewGuid();
        var expense = Expense(tenantId, requesterId, ExpenseStatus.Pending, 6000);
        var addedApprovals = new List<ExpenseApproval>();
        var addedOutboxMessages = new List<OutboxMessage>();
        var users = new List<User>
        {
            User(tenantId, requesterId, "personel@test1.com", Roles.Personel),
            User(tenantId, Guid.NewGuid(), "admin@test1.com", Roles.Admin)
        };

        var service = CreateService(
            tenantId,
            hrId,
            roles: new[] { Roles.HR },
            users: users,
            expenses: new[] { expense },
            addedApprovals: addedApprovals,
            addedOutboxMessages: addedOutboxMessages,
            correlationId: "corr-approve");

        var response = await service.ApproveAsync(expense.Id, CancellationToken.None);

        Assert.Equal(ExpenseStatus.Pending, response.Status);
        Assert.True(response.HrApproved);
        Assert.False(response.AdminApproved);

        var approval = Assert.Single(addedApprovals);
        Assert.Equal(ApprovalStep.HR, approval.Step);
        Assert.Equal(ApprovalDecision.Approved, approval.Decision);
        Assert.Equal(hrId, approval.ApproverUserId);

        var outbox = Assert.Single(addedOutboxMessages);
        Assert.Equal(ExpenseEventNames.ExpenseRequiresAdminApproval, outbox.RoutingKey);
        Assert.Equal("corr-approve", outbox.CorrelationId);
        Assert.Contains("admin@test1.com", outbox.Payload);
    }

    private static ExpenseAppService CreateService(
        Guid tenantId,
        Guid userId,
        IReadOnlyCollection<string> roles,
        IReadOnlyCollection<User> users,
        IReadOnlyCollection<Expense> expenses,
        List<Expense>? addedExpenses = null,
        List<ExpenseApproval>? addedApprovals = null,
        List<OutboxMessage>? addedOutboxMessages = null,
        decimal exchangeRate = 1m,
        string correlationId = "corr-unit")
    {
        var currentUser = new Mock<ICurrentUserContext>();
        currentUser.Setup(x => x.UserId).Returns(userId);
        currentUser.Setup(x => x.TenantId).Returns(tenantId);
        currentUser.Setup(x => x.Roles).Returns(roles);
        currentUser.Setup(x => x.IsInRole(It.IsAny<string>()))
            .Returns<string>(role => roles.Contains(role, StringComparer.OrdinalIgnoreCase));

        var correlation = new Mock<ICorrelationContext>();
        correlation.Setup(x => x.CorrelationId).Returns(correlationId);

        var exchangeRateService = new Mock<IExchangeRateService>();
        exchangeRateService.Setup(x => x.GetExchangeRateAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(exchangeRate);

        var userRepository = Repository(users, added: (List<User>?)null);
        var expenseRepository = Repository(expenses, addedExpenses);
        var approvalRepository = Repository(Array.Empty<ExpenseApproval>(), addedApprovals);
        var outboxRepository = Repository(Array.Empty<OutboxMessage>(), addedOutboxMessages);

        var unitOfWork = new Mock<IUnitOfWork>();
        unitOfWork.Setup(x => x.Repository<User>()).Returns(userRepository.Object);
        unitOfWork.Setup(x => x.Repository<Expense>()).Returns(expenseRepository.Object);
        unitOfWork.Setup(x => x.Repository<ExpenseApproval>()).Returns(approvalRepository.Object);
        unitOfWork.Setup(x => x.Repository<OutboxMessage>()).Returns(outboxRepository.Object);
        unitOfWork.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
        unitOfWork.Setup(x => x.ExecuteInTransactionAsync(It.IsAny<Func<CancellationToken, Task>>(), It.IsAny<CancellationToken>()))
            .Returns<Func<CancellationToken, Task>, CancellationToken>((action, cancellationToken) => action(cancellationToken));

        return new ExpenseAppService(unitOfWork.Object, currentUser.Object, correlation.Object, exchangeRateService.Object);
    }

    private static Mock<IRepository<T>> Repository<T>(IReadOnlyCollection<T> items, List<T>? added)
        where T : BaseEntity
    {
        var repository = new Mock<IRepository<T>>();
        repository.Setup(x => x.Query()).Returns(items.AsAsyncQueryable());
        repository.Setup(x => x.AddAsync(It.IsAny<T>(), It.IsAny<CancellationToken>()))
            .Returns<T, CancellationToken>((entity, _) =>
            {
                added?.Add(entity);
                return Task.CompletedTask;
            });
        repository.Setup(x => x.Delete(It.IsAny<T>()));
        repository.Setup(x => x.Update(It.IsAny<T>()));
        return repository;
    }

    private static User User(Guid tenantId, Guid userId, string email, string role)
    {
        return new User
        {
            Id = userId,
            TenantId = tenantId,
            Email = email,
            DisplayName = email,
            PasswordHash = "hash",
            Roles = new List<UserRole>
            {
                new()
                {
                    Id = Guid.NewGuid(),
                    TenantId = tenantId,
                    UserId = userId,
                    Role = role
                }
            }
        };
    }

    private static Expense Expense(Guid tenantId, Guid requesterId, ExpenseStatus status, decimal amount)
    {
        return new Expense
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            RequestedByUserId = requesterId,
            Category = ExpenseCategory.Travel,
            Currency = ExpenseCurrency.TRY,
            Amount = amount,
            ExchangeRate = 1m,
            Description = "Unit test expense description",
            Status = status,
            CreatedAt = DateTime.UtcNow
        };
    }
}

internal static class AsyncQueryableExtensions
{
    public static IQueryable<T> AsAsyncQueryable<T>(this IEnumerable<T> source)
    {
        return new TestAsyncEnumerable<T>(source);
    }
}

internal sealed class TestAsyncEnumerable<T> : EnumerableQuery<T>, IAsyncEnumerable<T>, IQueryable<T>
{
    public TestAsyncEnumerable(IEnumerable<T> enumerable)
        : base(enumerable)
    {
    }

    public TestAsyncEnumerable(Expression expression)
        : base(expression)
    {
    }

    IQueryProvider IQueryable.Provider => new TestAsyncQueryProvider<T>(this);

    public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default)
    {
        return new TestAsyncEnumerator<T>(this.AsEnumerable().GetEnumerator());
    }
}

internal sealed class TestAsyncEnumerator<T> : IAsyncEnumerator<T>
{
    private readonly IEnumerator<T> _inner;

    public TestAsyncEnumerator(IEnumerator<T> inner)
    {
        _inner = inner;
    }

    public T Current => _inner.Current;

    public ValueTask DisposeAsync()
    {
        _inner.Dispose();
        return ValueTask.CompletedTask;
    }

    public ValueTask<bool> MoveNextAsync()
    {
        return new ValueTask<bool>(_inner.MoveNext());
    }
}

internal sealed class TestAsyncQueryProvider<TEntity> : IAsyncQueryProvider
{
    private readonly IQueryProvider _inner;

    public TestAsyncQueryProvider(IQueryProvider inner)
    {
        _inner = inner;
    }

    public IQueryable CreateQuery(Expression expression)
    {
        return new TestAsyncEnumerable<TEntity>(expression);
    }

    public IQueryable<TElement> CreateQuery<TElement>(Expression expression)
    {
        return new TestAsyncEnumerable<TElement>(expression);
    }

    public object? Execute(Expression expression)
    {
        return _inner.Execute(expression);
    }

    public TResult Execute<TResult>(Expression expression)
    {
        return _inner.Execute<TResult>(expression);
    }

    public TResult ExecuteAsync<TResult>(Expression expression, CancellationToken cancellationToken = default)
    {
        var expectedResultType = typeof(TResult).GetGenericArguments()[0];
        var executionResult = typeof(IQueryProvider)
            .GetMethod(nameof(IQueryProvider.Execute), genericParameterCount: 1, types: new[] { typeof(Expression) })!
            .MakeGenericMethod(expectedResultType)
            .Invoke(_inner, new object[] { expression });

        return (TResult)typeof(Task)
            .GetMethod(nameof(Task.FromResult))!
            .MakeGenericMethod(expectedResultType)
            .Invoke(null, new[] { executionResult })!;
    }
}
