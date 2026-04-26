using ExpenseService.Application.Abstractions;
using ExpenseService.Application.DTOs;
using ExpenseService.Domain.Entities;
using ExpenseService.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace ExpenseService.Application.Services;

public interface IOutboxAdminService
{
    Task<IReadOnlyCollection<OutboxMessageResponse>> GetDeadLettersAsync(CancellationToken cancellationToken);
}

public sealed class OutboxAdminService : IOutboxAdminService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserContext _currentUser;

    public OutboxAdminService(IUnitOfWork unitOfWork, ICurrentUserContext currentUser)
    {
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
    }

    public async Task<IReadOnlyCollection<OutboxMessageResponse>> GetDeadLettersAsync(CancellationToken cancellationToken)
    {
        if (!_currentUser.IsInRole(Roles.Admin))
        {
            throw new UnauthorizedAccessException("Admin role is required.");
        }

        return await _unitOfWork.Repository<OutboxMessage>()
            .Query()
            .Where(x => x.DeadLetteredAt != null)
            .OrderByDescending(x => x.DeadLetteredAt)
            .Select(x => new OutboxMessageResponse(
                x.Id,
                x.EventType,
                x.RoutingKey,
                x.CorrelationId,
                x.RetryCount,
                x.Error,
                x.CreatedAt,
                x.DeadLetteredAt))
            .ToListAsync(cancellationToken);
    }
}
