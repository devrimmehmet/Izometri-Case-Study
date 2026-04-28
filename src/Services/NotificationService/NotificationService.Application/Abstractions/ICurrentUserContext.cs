namespace NotificationService.Application.Abstractions;

/// <summary>
/// Mevcut istek yapan kullanıcının kimlik ve yetki bilgilerine erişim sağlar.
/// </summary>
public interface ICurrentUserContext
{
    Guid? UserId { get; }
    Guid? TenantId { get; }
    IReadOnlyCollection<string> Roles { get; }
    bool IsAuthenticated { get; }
    bool IsInRole(string role);
}
