using System.Text.Json;
using ExpenseManagement.Contracts;
using ExpenseService.Application.Abstractions;
using ExpenseService.Application.DTOs;
using ExpenseService.Domain.Entities;
using ExpenseService.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace ExpenseService.Application.Services;

public interface IExpenseAppService
{
    Task<ExpenseResponse> CreateAsync(CreateExpenseRequest request, CancellationToken cancellationToken);
    Task<ExpenseResponse> UpdateAsync(Guid id, UpdateExpenseRequest request, CancellationToken cancellationToken);
    Task<PagedResponse<ExpenseResponse>> GetListAsync(ExpenseQuery query, CancellationToken cancellationToken);
    Task<ExpenseResponse> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task SubmitAsync(Guid id, CancellationToken cancellationToken);
    Task<ExpenseResponse> ApproveAsync(Guid id, CancellationToken cancellationToken);
    Task<ExpenseResponse> RejectAsync(Guid id, RejectExpenseRequest request, CancellationToken cancellationToken);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken);
}

public sealed class ExpenseAppService : IExpenseAppService
{
    private readonly ICorrelationContext _correlationContext;
    private readonly ICurrentUserContext _currentUser;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IExchangeRateService _exchangeRateService;

    public ExpenseAppService(IUnitOfWork unitOfWork, ICurrentUserContext currentUser, ICorrelationContext correlationContext, IExchangeRateService exchangeRateService)
    {
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
        _correlationContext = correlationContext;
        _exchangeRateService = exchangeRateService;
    }

    public async Task<ExpenseResponse> CreateAsync(CreateExpenseRequest request, CancellationToken cancellationToken)
    {
        EnsureRole(Roles.Personel);
        var tenantId = RequiredTenantId();
        var userId = RequiredUserId();

        var notificationContacts = await _unitOfWork.Repository<User>()
            .Query()
            .Where(u => u.Roles.Any(r => r.Role == Roles.HR || r.Role == Roles.Admin))
            .Select(u => new { u.Email, u.Phone })
            .ToListAsync(cancellationToken);

        var exchangeRate = request.ExchangeRate ?? await _exchangeRateService.GetExchangeRateAsync(request.Currency.ToString(), cancellationToken);

        var expense = new Expense
        {
            TenantId = tenantId,
            RequestedByUserId = userId,
            Category = request.Category,
            Currency = request.Currency,
            Amount = request.Amount,
            ExchangeRate = exchangeRate,
            Description = request.Description,
            Status = ExpenseStatus.Draft
        };

        await _unitOfWork.ExecuteInTransactionAsync(async ct =>
        {
            await _unitOfWork.Repository<Expense>().AddAsync(expense, ct);
            await AddOutboxAsync(new ExpenseCreatedEvent(
                Guid.NewGuid(),
                _correlationContext.CorrelationId,
                DateTime.UtcNow,
                tenantId,
                expense.Id,
                userId,
                expense.Amount,
                expense.Currency.ToString())
            {
                RecipientEmail = string.Join(",", notificationContacts.Select(u => u.Email)),
                RecipientPhone = notificationContacts.Select(u => u.Phone).FirstOrDefault(p => !string.IsNullOrEmpty(p)) ?? string.Empty,
                // Harcama oluşturulduğunda HR onaylayacak; bildirim alıcısı HR rolündedir.
                RecipientRole = "HR"
            }, ExpenseEventNames.ExpenseCreated, ct);
        }, cancellationToken);

        return Map(expense);
    }

    public async Task<ExpenseResponse> UpdateAsync(Guid id, UpdateExpenseRequest request, CancellationToken cancellationToken)
    {
        var userId = RequiredUserId();
        var expense = await _unitOfWork.Repository<Expense>().Query().FirstOrDefaultAsync(x => x.Id == id, cancellationToken)
            ?? throw new KeyNotFoundException("Expense not found.");

        if (expense.RequestedByUserId != userId)
            throw new UnauthorizedAccessException("Only requester can update the expense.");

        if (expense.Status != ExpenseStatus.Draft)
            throw new InvalidOperationException("Only draft expenses can be updated.");

        var exchangeRate = request.ExchangeRate ?? await _exchangeRateService.GetExchangeRateAsync(request.Currency.ToString(), cancellationToken);

        expense.Category = request.Category;
        expense.Currency = request.Currency;
        expense.Amount = request.Amount;
        expense.ExchangeRate = exchangeRate;
        expense.Description = request.Description;

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Map(expense);
    }

    public async Task<PagedResponse<ExpenseResponse>> GetListAsync(ExpenseQuery query, CancellationToken cancellationToken)
    {
        var pageNumber = Math.Max(query.PageNumber, 1);
        var pageSize = Math.Clamp(query.PageSize, 1, 100);
        var expenses = VisibleExpenses();

        if (query.DateFrom.HasValue) expenses = expenses.Where(x => x.CreatedAt >= DateTime.SpecifyKind(query.DateFrom.Value, DateTimeKind.Utc));
        if (query.DateTo.HasValue) expenses = expenses.Where(x => x.CreatedAt <= DateTime.SpecifyKind(query.DateTo.Value, DateTimeKind.Utc));
        if (query.Status.HasValue) expenses = expenses.Where(x => x.Status == query.Status.Value);
        if (query.Category.HasValue) expenses = expenses.Where(x => x.Category == query.Category.Value);

        var total = await expenses.CountAsync(cancellationToken);
        var items = await expenses
            .OrderByDescending(x => x.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(x => Map(x))
            .ToListAsync(cancellationToken);

        return new PagedResponse<ExpenseResponse>(items, pageNumber, pageSize, total);
    }

    public async Task<ExpenseResponse> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var expense = await VisibleExpenses().FirstOrDefaultAsync(x => x.Id == id, cancellationToken)
            ?? throw new KeyNotFoundException("Expense not found.");

        return Map(expense);
    }

    public async Task SubmitAsync(Guid id, CancellationToken cancellationToken)
    {
        var userId = RequiredUserId();
        var expense = await _unitOfWork.Repository<Expense>().Query().FirstOrDefaultAsync(x => x.Id == id, cancellationToken)
            ?? throw new KeyNotFoundException("Expense not found.");

        if (expense.RequestedByUserId != userId)
        {
            throw new UnauthorizedAccessException("Only requester can submit the expense.");
        }

        if (expense.Status != ExpenseStatus.Draft)
        {
            throw new InvalidOperationException("Only draft expenses can be submitted.");
        }

        expense.Status = ExpenseStatus.Pending;
        expense.SubmittedAt = DateTime.UtcNow;
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task<ExpenseResponse> ApproveAsync(Guid id, CancellationToken cancellationToken)
    {
        var userId = RequiredUserId();
        EnsureAnyRole(Roles.HR, Roles.Admin);
        var expense = await LoadExpenseForDecisionAsync(id, cancellationToken);

        if (expense.Status != ExpenseStatus.Pending)
        {
            throw new InvalidOperationException("Only pending expenses can be approved.");
        }

        var isHr = _currentUser.IsInRole(Roles.HR);
        var isAdmin = _currentUser.IsInRole(Roles.Admin);
        ExpenseApproval? approval = null;

        if (!expense.HrApproved)
        {
            if (!isHr)
            {
                throw new UnauthorizedAccessException("HR approval is required first.");
            }

            expense.HrApproved = true;
            approval = Approval(expense, userId, ApprovalStep.HR, ApprovalDecision.Approved);
        }
        else
        {
            if (!expense.RequiresAdminApproval)
            {
                throw new InvalidOperationException("Expense is already approved by HR.");
            }

            if (!isAdmin)
            {
                throw new UnauthorizedAccessException("Admin approval is required.");
            }

            expense.AdminApproved = true;
            approval = Approval(expense, userId, ApprovalStep.Admin, ApprovalDecision.Approved);
        }

        if (!expense.RequiresAdminApproval || expense.AdminApproved)
        {
            expense.Status = ExpenseStatus.Approved;
            expense.ApprovedAt = DateTime.UtcNow;
        }

        var requesterContact = await _unitOfWork.Repository<User>()
            .Query()
            .Where(u => u.Id == expense.RequestedByUserId)
            .Select(u => new { u.Email, u.Phone })
            .FirstOrDefaultAsync(cancellationToken);

        var adminContacts = expense.RequiresAdminApproval && !expense.AdminApproved ? await _unitOfWork.Repository<User>()
            .Query()
            .Where(u => u.TenantId == expense.TenantId && u.Roles.Any(r => r.Role == Roles.Admin))
            .Select(u => new { u.Email, u.Phone })
            .ToListAsync(cancellationToken) : null;

        await _unitOfWork.ExecuteInTransactionAsync(async ct =>
        {
            if (approval is not null)
            {
                await _unitOfWork.Repository<ExpenseApproval>().AddAsync(approval, ct);
            }

            if (expense.Status == ExpenseStatus.Approved)
            {
                await AddOutboxAsync(new ExpenseApprovedEvent(
                    Guid.NewGuid(),
                    _correlationContext.CorrelationId,
                    DateTime.UtcNow,
                    expense.TenantId,
                    expense.Id,
                    userId,
                    expense.Status.ToString(),
                    // Tutar event'e gömülür; NotificationService ayrı HTTP çağrısı yapmaz.
                    expense.Amount,
                    expense.Currency.ToString())
                {
                    RecipientEmail = requesterContact?.Email ?? string.Empty,
                    RecipientPhone = requesterContact?.Phone ?? string.Empty,
                    // Onay tamamlandığında harcamayı açan personel bilgilendirilir.
                    RecipientRole = "Personel"
                }, ExpenseEventNames.ExpenseApproved, ct);
            }
            else if (expense.HrApproved && !expense.AdminApproved && adminContacts != null)
            {
                // Send notification to Admin that an expense > 5000 is waiting
                await AddOutboxAsync(new ExpenseRequiresAdminApprovalEvent(
                    Guid.NewGuid(),
                    _correlationContext.CorrelationId,
                    DateTime.UtcNow,
                    expense.TenantId,
                    expense.Id,
                    userId,
                    expense.Amount,
                    expense.Currency.ToString())
                {
                    RecipientEmail = string.Join(",", adminContacts.Select(u => u.Email)),
                    RecipientPhone = adminContacts.Select(u => u.Phone).FirstOrDefault(p => !string.IsNullOrEmpty(p)) ?? string.Empty,
                    // HR onayladı, sıra Admin'de; bildirim Admin rolüne gider.
                    RecipientRole = "Admin"
                }, ExpenseEventNames.ExpenseRequiresAdminApproval, ct);
            }
        }, cancellationToken);

        return Map(expense);
    }

    public async Task<ExpenseResponse> RejectAsync(Guid id, RejectExpenseRequest request, CancellationToken cancellationToken)
    {
        EnsureAnyRole(Roles.HR, Roles.Admin);
        var userId = RequiredUserId();
        var expense = await LoadExpenseForDecisionAsync(id, cancellationToken);

        if (expense.Status != ExpenseStatus.Pending)
        {
            throw new InvalidOperationException("Only pending expenses can be rejected.");
        }

        var step = expense.HrApproved ? ApprovalStep.Admin : ApprovalStep.HR;
        if (step == ApprovalStep.HR && !_currentUser.IsInRole(Roles.HR))
        {
            throw new UnauthorizedAccessException("HR rejection is required first.");
        }

        if (step == ApprovalStep.Admin && !_currentUser.IsInRole(Roles.Admin))
        {
            throw new UnauthorizedAccessException("Admin rejection is required.");
        }

        expense.Status = ExpenseStatus.Rejected;
        expense.RejectionReason = request.Reason;
        expense.RejectedAt = DateTime.UtcNow;
        var approval = Approval(expense, userId, step, ApprovalDecision.Rejected, request.Reason);

        var requesterContact = await _unitOfWork.Repository<User>()
            .Query()
            .Where(u => u.Id == expense.RequestedByUserId)
            .Select(u => new { u.Email, u.Phone })
            .FirstOrDefaultAsync(cancellationToken);

        await _unitOfWork.ExecuteInTransactionAsync(async ct =>
        {
            await _unitOfWork.Repository<ExpenseApproval>().AddAsync(approval, ct);
            await AddOutboxAsync(new ExpenseRejectedEvent(
                Guid.NewGuid(),
                _correlationContext.CorrelationId,
                DateTime.UtcNow,
                expense.TenantId,
                expense.Id,
                userId,
                request.Reason,
                // Tutar event'e gömülür; NotificationService ayrı HTTP çağrısı yapmaz.
                expense.Amount,
                expense.Currency.ToString())
            {
                RecipientEmail = requesterContact?.Email ?? string.Empty,
                RecipientPhone = requesterContact?.Phone ?? string.Empty,
                // Red kararı harcamayı açan personele bildirilir.
                RecipientRole = "Personel"
            }, ExpenseEventNames.ExpenseRejected, ct);
        }, cancellationToken);

        return Map(expense);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        var expense = await VisibleExpenses().FirstOrDefaultAsync(x => x.Id == id, cancellationToken)
            ?? throw new KeyNotFoundException("Expense not found.");
        _unitOfWork.Repository<Expense>().Delete(expense);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private IQueryable<Expense> VisibleExpenses()
    {
        var query = _unitOfWork.Repository<Expense>().Query();
        if (_currentUser.IsInRole(Roles.Admin) || _currentUser.IsInRole(Roles.HR) || _currentUser.IsInRole(Roles.Service))
        {
            return query;
        }

        return query.Where(x => x.RequestedByUserId == RequiredUserId());
    }

    private async Task<Expense> LoadExpenseForDecisionAsync(Guid id, CancellationToken cancellationToken)
    {
        return await _unitOfWork.Repository<Expense>().Query()
            .Include(x => x.Approvals)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken)
            ?? throw new KeyNotFoundException("Expense not found.");
    }

    private async Task AddOutboxAsync<T>(T integrationEvent, string routingKey, CancellationToken cancellationToken)
        where T : ExpenseIntegrationEvent
    {
        await _unitOfWork.Repository<OutboxMessage>().AddAsync(new OutboxMessage
        {
            TenantId = integrationEvent.TenantId,
            EventType = typeof(T).Name,
            RoutingKey = routingKey,
            Payload = JsonSerializer.Serialize(integrationEvent),
            CorrelationId = integrationEvent.CorrelationId
        }, cancellationToken);
    }

    private static ExpenseApproval Approval(Expense expense, Guid approverId, ApprovalStep step, ApprovalDecision decision, string? reason = null)
    {
        return new ExpenseApproval
        {
            TenantId = expense.TenantId,
            ExpenseId = expense.Id,
            ApproverUserId = approverId,
            Step = step,
            Decision = decision,
            Reason = reason,
            DecidedAt = DateTime.UtcNow
        };
    }

    private Guid RequiredUserId() => _currentUser.UserId ?? throw new UnauthorizedAccessException("UserId claim is missing.");
    private Guid RequiredTenantId() => _currentUser.TenantId ?? throw new UnauthorizedAccessException("TenantId claim is missing.");
    private void EnsureRole(string role)
    {
        if (!_currentUser.IsInRole(role))
        {
            throw new UnauthorizedAccessException($"Role '{role}' is required.");
        }
    }

    private void EnsureAnyRole(params string[] roles)
    {
        if (!roles.Any(_currentUser.IsInRole))
        {
            throw new UnauthorizedAccessException("Required role is missing.");
        }
    }

    private static ExpenseResponse Map(Expense x) => new(
        x.Id,
        x.TenantId,
        x.RequestedByUserId,
        x.Category,
        x.Currency,
        x.Amount,
        x.ExchangeRate,
        x.Amount * x.ExchangeRate,
        x.Description,
        x.Status,
        x.HrApproved,
        x.AdminApproved,
        x.RequiresAdminApproval,
        x.RejectionReason,
        x.SubmittedAt,
        x.ApprovedAt,
        x.RejectedAt,
        x.CreatedAt);
}
