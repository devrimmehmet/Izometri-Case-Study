using ExpenseService.Domain.Enums;

namespace ExpenseService.Application.DTOs;

public sealed record CreateExpenseRequest(
    ExpenseCategory Category,
    ExpenseCurrency Currency,
    decimal Amount,
    decimal? ExchangeRate,
    string Description);

public sealed record UpdateExpenseRequest(
    ExpenseCategory Category,
    ExpenseCurrency Currency,
    decimal Amount,
    decimal? ExchangeRate,
    string Description);

public sealed record RejectExpenseRequest(string Reason);

public sealed record ExpenseQuery(
    DateTime? DateFrom,
    DateTime? DateTo,
    ExpenseStatus? Status,
    ExpenseCategory? Category,
    int PageNumber = 1,
    int PageSize = 20);

public sealed record ExpenseResponse(
    Guid Id,
    Guid TenantId,
    Guid RequestedByUserId,
    ExpenseCategory Category,
    ExpenseCurrency Currency,
    decimal Amount,
    decimal ExchangeRate,
    decimal AmountInTry,
    string Description,
    ExpenseStatus Status,
    bool HrApproved,
    bool AdminApproved,
    string? RejectionReason,
    DateTime? SubmittedAt,
    DateTime? ApprovedAt,
    DateTime? RejectedAt,
    DateTime CreatedAt);

public sealed record PagedResponse<T>(
    IReadOnlyCollection<T> Items,
    int PageNumber,
    int PageSize,
    int TotalCount);
