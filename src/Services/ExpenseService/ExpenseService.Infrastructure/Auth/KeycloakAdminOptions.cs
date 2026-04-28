namespace ExpenseService.Infrastructure.Auth;

/// <summary>
/// Keycloak Admin API bağlantı ayarları.
/// appsettings.json → "Keycloak" bölümünden okunur.
/// </summary>
public sealed class KeycloakAdminOptions
{
    /// <summary>Keycloak sunucu URL'si (ör. http://keycloak:8080)</summary>
    public string? BaseUrl { get; set; }

    /// <summary>Hedef realm adı (ör. izometri)</summary>
    public string Realm { get; set; } = "izometri";

    /// <summary>service-account-roles sahibi client id (ör. expense-service)</summary>
    public string ClientId { get; set; } = "expense-service";

    /// <summary>Client secret</summary>
    public string? ClientSecret { get; set; }

    /// <summary>Keycloak entegrasyonu aktif mi? false ise kullanıcı sadece uygulama DB'sine yazılır.</summary>
    public bool Enabled { get; set; }
}
