namespace ExpenseService.Application.DTOs;

public sealed record CreateUserRequest(
    string Email,
    string DisplayName,
    string Password,
    IReadOnlyCollection<string> Roles,
    string? Phone = null);

public sealed record UpdateUserRolesRequest(IReadOnlyCollection<string> Roles);

public sealed record UserResponse(
    Guid Id,
    Guid TenantId,
    string Email,
    string DisplayName,
    IReadOnlyCollection<string> Roles,
    DateTime CreatedAt);
