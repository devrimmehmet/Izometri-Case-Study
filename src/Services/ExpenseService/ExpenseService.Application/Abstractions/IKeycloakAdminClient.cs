namespace ExpenseService.Application.Abstractions;

/// <summary>
/// Keycloak Admin REST API ile kullanıcı yönetimi yapan servis sözleşmesi.
/// Infrastructure katmanında implement edilir.
/// </summary>
public interface IKeycloakAdminClient
{
    /// <summary>
    /// Keycloak'ta yeni bir kullanıcı oluşturur ve belirtilen realm rollerini atar.
    /// </summary>
    /// <param name="userId">Uygulama tarafında oluşturulan kullanıcı Id'si (Keycloak user attribute olarak saklanır).</param>
    /// <param name="tenantId">Kullanıcının ait olduğu tenant Id'si (Keycloak user attribute olarak saklanır).</param>
    /// <param name="email">Kullanıcının e-posta adresi (aynı zamanda username olarak kullanılır).</param>
    /// <param name="displayName">Kullanıcının görünen adı. İlk boşluktan öncesi firstName, sonrası lastName olarak atanır.</param>
    /// <param name="password">Kullanıcının plain-text parolası. Keycloak credential olarak set edilir.</param>
    /// <param name="roles">Atanacak realm rolleri (Admin, HR, Personel).</param>
    /// <param name="cancellationToken">İptal belirteci.</param>
    Task CreateUserAsync(
        Guid userId,
        Guid tenantId,
        string email,
        string displayName,
        string password,
        IReadOnlyCollection<string> roles,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Keycloak'ta mevcut bir kullanıcının rollerini günceller (eskileri siler, yenileri ekler).
    /// </summary>
    /// <param name="email">Kullanıcının e-posta adresi.</param>
    /// <param name="roles">Atanacak yeni realm rolleri.</param>
    /// <param name="cancellationToken">İptal belirteci.</param>
    Task SyncUserRolesAsync(
        string email,
        IReadOnlyCollection<string> roles,
        CancellationToken cancellationToken = default);
}
