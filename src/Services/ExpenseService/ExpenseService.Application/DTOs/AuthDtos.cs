namespace ExpenseService.Application.DTOs;

public sealed record LoginRequest(string Email, string Password, string TenantCode);

public sealed record LoginResponse(
    string AccessToken,
    Guid UserId,
    Guid TenantId,
    string Email,
    string DisplayName,
    IReadOnlyCollection<string> Roles);
