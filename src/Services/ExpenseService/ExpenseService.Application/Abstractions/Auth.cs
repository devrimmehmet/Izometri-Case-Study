using ExpenseService.Domain.Entities;

namespace ExpenseService.Application.Abstractions;

public interface IJwtTokenService
{
    string CreateToken(User user, IReadOnlyCollection<string> roles);
    string CreateServiceToken(Guid tenantId, string correlationId);
}
