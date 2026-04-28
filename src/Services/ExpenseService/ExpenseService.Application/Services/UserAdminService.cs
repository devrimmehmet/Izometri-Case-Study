using ExpenseService.Application.Abstractions;
using ExpenseService.Application.DTOs;
using ExpenseService.Domain.Entities;
using ExpenseService.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ExpenseService.Application.Services;

public interface IUserAdminService
{
    Task<IReadOnlyCollection<UserResponse>> GetUsersAsync(CancellationToken cancellationToken);
    Task<UserResponse> CreateUserAsync(CreateUserRequest request, CancellationToken cancellationToken);
    Task<UserResponse> UpdateRolesAsync(Guid userId, UpdateUserRolesRequest request, CancellationToken cancellationToken);
}

public sealed class UserAdminService : IUserAdminService
{
    private readonly ICurrentUserContext _currentUser;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IKeycloakAdminClient _keycloakAdminClient;
    private readonly ILogger<UserAdminService> _logger;

    public UserAdminService(
        IUnitOfWork unitOfWork,
        ICurrentUserContext currentUser,
        IPasswordHasher passwordHasher,
        IKeycloakAdminClient keycloakAdminClient,
        ILogger<UserAdminService> logger)
    {
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
        _passwordHasher = passwordHasher;
        _keycloakAdminClient = keycloakAdminClient;
        _logger = logger;
    }

    public async Task<IReadOnlyCollection<UserResponse>> GetUsersAsync(CancellationToken cancellationToken)
    {
        EnsureAdmin();

        return await _unitOfWork.Repository<User>().Query()
            .Include(x => x.Roles)
            .OrderBy(x => x.Email)
            .Select(x => Map(x))
            .ToListAsync(cancellationToken);
    }

    public async Task<UserResponse> CreateUserAsync(CreateUserRequest request, CancellationToken cancellationToken)
    {
        EnsureAdmin();
        var tenantId = RequiredTenantId();
        var normalizedEmail = request.Email.Trim().ToLowerInvariant();

        var exists = await _unitOfWork.Repository<User>().Query()
            .AnyAsync(x => x.Email == normalizedEmail, cancellationToken);

        if (exists)
        {
            throw new InvalidOperationException("Email already exists in this tenant.");
        }

        var normalizedRoles = NormalizeRoles(request.Roles);

        var user = new User
        {
            TenantId = tenantId,
            Email = normalizedEmail,
            DisplayName = request.DisplayName.Trim(),
            PasswordHash = _passwordHasher.Hash(request.Password),
            Phone = request.Phone?.Trim()
        };

        var roles = normalizedRoles
            .Select(role => new UserRole
            {
                TenantId = tenantId,
                UserId = user.Id,
                Role = role
            })
            .ToArray();

        await _unitOfWork.ExecuteInTransactionAsync(async ct =>
        {
            await _unitOfWork.Repository<User>().AddAsync(user, ct);
            foreach (var role in roles)
            {
                await _unitOfWork.Repository<UserRole>().AddAsync(role, ct);
            }
        }, cancellationToken);

        // Keycloak senkronizasyonu — DB commit başarılı olduktan sonra.
        // Keycloak çağrısı başarısız olursa kullanıcı DB'de kalır; hata loglanır.
        try
        {
            await _keycloakAdminClient.CreateUserAsync(
                user.Id,
                tenantId,
                normalizedEmail,
                request.DisplayName.Trim(),
                request.Password, // plain-text — Keycloak kendi hash'ini yapar
                normalizedRoles,
                cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Keycloak sync failed for user {Email} (DB user {UserId} was created successfully). " +
                "Manual sync may be required.",
                normalizedEmail, user.Id);
        }

        user.Roles = roles;
        return Map(user);
    }

    public async Task<UserResponse> UpdateRolesAsync(Guid userId, UpdateUserRolesRequest request, CancellationToken cancellationToken)
    {
        EnsureAdmin();
        var tenantId = RequiredTenantId();
        var user = await _unitOfWork.Repository<User>().Query()
            .Include(x => x.Roles)
            .FirstOrDefaultAsync(x => x.Id == userId, cancellationToken)
            ?? throw new KeyNotFoundException("User not found.");

        var requestedRoles = NormalizeRoles(request.Roles).ToArray();
        var currentRoles = user.Roles.Where(x => !x.IsDeleted).ToArray();
        var roleRepository = _unitOfWork.Repository<UserRole>();

        foreach (var role in currentRoles.Where(x => !requestedRoles.Contains(x.Role, StringComparer.OrdinalIgnoreCase)))
        {
            roleRepository.Delete(role);
        }

        foreach (var role in requestedRoles.Where(role => currentRoles.All(x => !x.Role.Equals(role, StringComparison.OrdinalIgnoreCase))))
        {
            await roleRepository.AddAsync(new UserRole
            {
                TenantId = tenantId,
                UserId = user.Id,
                Role = role
            }, cancellationToken);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Keycloak senkronizasyonu
        try
        {
            await _keycloakAdminClient.SyncUserRolesAsync(user.Email, requestedRoles, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Keycloak role sync failed for user {Email}", user.Email);
        }

        user.Roles = await _unitOfWork.Repository<UserRole>().Query()
            .Where(x => x.UserId == user.Id)
            .ToListAsync(cancellationToken);

        return Map(user);
    }

    private static IReadOnlyCollection<string> NormalizeRoles(IEnumerable<string> roles)
    {
        return roles
            .Select(x => x.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Select(x => x.Equals(Roles.Admin, StringComparison.OrdinalIgnoreCase) ? Roles.Admin :
                x.Equals(Roles.HR, StringComparison.OrdinalIgnoreCase) ? Roles.HR : Roles.Personel)
            .ToArray();
    }

    private void EnsureAdmin()
    {
        if (!_currentUser.IsInRole(Roles.Admin))
        {
            throw new UnauthorizedAccessException("Admin role is required.");
        }
    }

    private Guid RequiredTenantId() => _currentUser.TenantId ?? throw new UnauthorizedAccessException("TenantId claim is missing.");

    private static UserResponse Map(User user) => new(
        user.Id,
        user.TenantId,
        user.Email,
        user.DisplayName,
        user.Roles.Where(x => !x.IsDeleted).Select(x => x.Role).OrderBy(x => x).ToArray(),
        user.CreatedAt);
}
