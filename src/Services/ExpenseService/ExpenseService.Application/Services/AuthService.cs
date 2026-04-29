using ExpenseService.Application.Abstractions;
using ExpenseService.Application.DTOs;
using ExpenseService.Domain.Entities;
using Microsoft.EntityFrameworkCore; // Include(), ToListAsync() vb. async LINQ — pragmatik kabul

namespace ExpenseService.Application.Services;

public interface IAuthService
{
    Task<LoginResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken);
}

public sealed class AuthService : IAuthService
{
    private readonly IJwtTokenService _jwtTokenService;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IRepository<Tenant> _tenants;
    private readonly IRepository<User> _users;

    public AuthService(IUnitOfWork unitOfWork, IPasswordHasher passwordHasher, IJwtTokenService jwtTokenService)
    {
        _passwordHasher = passwordHasher;
        _jwtTokenService = jwtTokenService;
        _tenants = unitOfWork.Repository<Tenant>();
        _users = unitOfWork.Repository<User>();
    }

    public async Task<LoginResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken)
    {
        // Login sırasında JWT yok → _currentUser.TenantId == null → global filter
        // zaten tüm tenant kayıtlarını geçirir; IgnoreQueryFilters() gereksiz.
        var tenant = await _tenants.Query()
            .FirstOrDefaultAsync(x => x.Name == request.TenantCode, cancellationToken)
            ?? throw new UnauthorizedAccessException("Invalid credentials.");

        var user = await _users.Query()
            .Include(x => x.Roles)
            .FirstOrDefaultAsync(x => x.TenantId == tenant.Id && x.Email == request.Email.ToLower(), cancellationToken)
            ?? throw new UnauthorizedAccessException("Invalid credentials.");

        if (!_passwordHasher.Verify(request.Password, user.PasswordHash))
        {
            throw new UnauthorizedAccessException("Invalid credentials.");
        }

        var roles = user.Roles.Where(x => !x.IsDeleted).Select(x => x.Role).ToArray();
        var token = _jwtTokenService.CreateToken(user, roles);

        return new LoginResponse(token, user.Id, user.TenantId, user.Email, user.DisplayName, roles);
    }
}
