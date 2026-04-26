using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using ExpenseService.Application.Abstractions;
using ExpenseService.Domain.Entities;
using ExpenseService.Domain.Enums;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace ExpenseService.Infrastructure.Auth;

public sealed class JwtTokenService : IJwtTokenService
{
    private readonly JwtOptions _options;

    public JwtTokenService(IOptions<JwtOptions> options)
    {
        _options = options.Value;
    }

    public string CreateToken(User user, IReadOnlyCollection<string> roles)
    {
        return Create(user.Id.ToString(), user.TenantId, roles);
    }

    public string CreateServiceToken(Guid tenantId, string correlationId)
    {
        return Create("notification-service", tenantId, new[] { Roles.Service }, correlationId);
    }

    private string Create(string userId, Guid tenantId, IReadOnlyCollection<string> roles, string? correlationId = null)
    {
        var claims = new List<Claim>
        {
            new("UserId", userId),
            new("TenantId", tenantId.ToString()),
            new(JwtRegisteredClaimNames.Sub, userId),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        if (!string.IsNullOrWhiteSpace(correlationId))
        {
            claims.Add(new Claim("CorrelationId", correlationId));
        }

        claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.Secret));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(
            _options.Issuer,
            _options.Audience,
            claims,
            expires: DateTime.UtcNow.AddMinutes(_options.ExpirationMinutes),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
