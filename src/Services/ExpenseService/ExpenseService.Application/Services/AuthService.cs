using ExpenseService.Application.Abstractions;
using ExpenseService.Application.DTOs;
using ExpenseService.Domain.Entities;
using Microsoft.EntityFrameworkCore;

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
        var tenant = await _tenants.Query()
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(x => x.Name == request.TenantCode && !x.IsDeleted, cancellationToken)
            ?? throw new UnauthorizedAccessException("Invalid credentials.");

        var user = await _users.Query()
            .IgnoreQueryFilters()
            .Include(x => x.Roles)
            .FirstOrDefaultAsync(x => x.TenantId == tenant.Id && x.Email == request.Email.ToLower() && !x.IsDeleted, cancellationToken)
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
