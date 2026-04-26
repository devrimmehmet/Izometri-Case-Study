namespace ExpenseService.Infrastructure.Auth;

public sealed class JwtOptions
{
    public string Issuer { get; set; } = "Izometri.CaseStudy";
    public string Audience { get; set; } = "Izometri.CaseStudy";
    public string Secret { get; set; } = "change-me-development-secret-at-least-32-characters";
    public int ExpirationMinutes { get; set; } = 120;
}
