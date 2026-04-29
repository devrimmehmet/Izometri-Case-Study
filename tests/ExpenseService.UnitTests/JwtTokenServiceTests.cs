using System.IdentityModel.Tokens.Jwt;
using ExpenseService.Domain.Entities;
using ExpenseService.Domain.Enums;
using ExpenseService.Infrastructure.Auth;
using Microsoft.Extensions.Options;

namespace ExpenseService.Tests;

public sealed class JwtTokenServiceTests
{
    [Fact]
    public void CreateToken_emits_keycloak_compatible_claim_contract()
    {
        var tenantId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var service = new JwtTokenService(Options.Create(new JwtOptions
        {
            Issuer = "Izometri.CaseStudy",
            Audience = "expense-service",
            Secret = "unit-test-secret-at-least-32-characters",
            ExpirationMinutes = 30
        }));

        var token = service.CreateToken(
            new User
            {
                Id = userId,
                TenantId = tenantId,
                Email = "personel@test1.com",
                DisplayName = "Personel",
                PasswordHash = "hash"
            },
            new[] { Roles.Personel, Roles.HR });

        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);

        Assert.Equal("Izometri.CaseStudy", jwt.Issuer);
        Assert.Contains("expense-service", jwt.Audiences);
        Assert.Contains(jwt.Claims, x => x.Type == "UserId" && x.Value == userId.ToString());
        Assert.Contains(jwt.Claims, x => x.Type == "TenantId" && x.Value == tenantId.ToString());
        Assert.Contains(jwt.Claims, x => x.Type == "role" && x.Value == Roles.Personel);
        Assert.Contains(jwt.Claims, x => x.Type == "role" && x.Value == Roles.HR);
    }

    [Fact]
    public void CreateServiceToken_emits_service_role_and_correlation()
    {
        var tenantId = Guid.NewGuid();
        var service = new JwtTokenService(Options.Create(new JwtOptions
        {
            Issuer = "Izometri.CaseStudy",
            Audience = "expense-service",
            Secret = "unit-test-secret-at-least-32-characters",
            ExpirationMinutes = 30
        }));

        var token = service.CreateServiceToken(tenantId, "corr-123");
        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);

        Assert.Contains(jwt.Claims, x => x.Type == "TenantId" && x.Value == tenantId.ToString());
        Assert.Contains(jwt.Claims, x => x.Type == "role" && x.Value == Roles.Service);
        Assert.Contains(jwt.Claims, x => x.Type == "CorrelationId" && x.Value == "corr-123");
    }
}
