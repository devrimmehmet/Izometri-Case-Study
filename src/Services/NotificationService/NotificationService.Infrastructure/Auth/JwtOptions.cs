namespace NotificationService.Infrastructure.Auth;

public sealed class JwtOptions
{
    public string Issuer { get; set; } = "Izometri.CaseStudy";
    public string Audience { get; set; } = "Izometri.CaseStudy";
    public string Secret { get; set; } = "expense-service-development-secret-key-please-change";
    public int ExpirationMinutes { get; set; } = 30;
}
