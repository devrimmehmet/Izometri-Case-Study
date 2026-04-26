using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace NotificationService.Infrastructure.Auth;

public sealed class ServiceTokenFactory
{
    private readonly JwtOptions _options;

    public ServiceTokenFactory(IOptions<JwtOptions> options)
    {
        _options = options.Value;
    }

    public string Create(Guid tenantId, string correlationId)
    {
        var claims = new[]
        {
            new Claim("UserId", Guid.Empty.ToString()),
            new Claim("TenantId", tenantId.ToString()),
            new Claim(ClaimTypes.Role, "Service"),
            new Claim("CorrelationId", correlationId),
            new Claim(JwtRegisteredClaimNames.Sub, "notification-service"),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var token = new JwtSecurityToken(
            _options.Issuer,
            _options.Audience,
            claims,
            expires: DateTime.UtcNow.AddMinutes(_options.ExpirationMinutes),
            signingCredentials: new SigningCredentials(
                new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.Secret)),
                SecurityAlgorithms.HmacSha256));

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
