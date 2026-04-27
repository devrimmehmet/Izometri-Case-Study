# Mimari ve Topoloji

## Genel Yaklaşım

Proje, iki bağımsız API ve servis başına ayrı veritabanı yaklaşımıyla tasarlanmıştır. ExpenseService harcama ve kullanıcı verisinin sahibidir. NotificationService yalnızca bildirim verisini saklar ve ihtiyaç duyduğunda ExpenseService üzerinden HTTP ile harcama detayını okur.

## Servisler

### ExpenseService.Api

Sorumluluklar:

- JWT login
- Tenant bazlı kullanıcı ve rol yönetimi
- Harcama oluşturma, listeleme, detay okuma, submit, approve, reject ve delete
- Soft delete
- Outbox mesajı üretme
- RabbitMQ publish işlemi

Veritabanı:

- `expense_db`

Ana tablolar:

- `Tenants`
- `Users`
- `UserRoles`
- `Expenses`
- `ExpenseApprovals`
- `OutboxMessages`

### NotificationService.Api

Sorumluluklar:

- RabbitMQ consumer olarak expense eventlerini dinleme
- Notification kaydı oluşturma
- Mock bildirim loglama
- Duplicate event işlemeyi engelleme
- ExpenseService üzerinden HTTP ile harcama detayı okuma

Veritabanı:

- `notification_db`

Ana tablolar:

- `Notifications`
- `ProcessedMessages`

## Katmanlar

Her servis Onion Architecture yaklaşımına uygun ayrılmıştır:

- `Domain`: Entity, enum ve domain modelleri.
- `Application`: DTO, validator, interface ve use-case servisleri.
- `Infrastructure`: EF Core, repository, UnitOfWork, messaging, HTTP client, auth yardımcıları.
- `Api`: Controller, authentication/authorization, Swagger ve middleware.
- `Shared/Contracts`: Ortak integration event contractları.

## Asenkron İletişim

ExpenseService create/approve/reject akışlarında eventleri doğrudan RabbitMQ'ya göndermek yerine aynı veritabanı transactionı içinde `OutboxMessages` tablosuna yazar.

`OutboxPublisherWorker` işlenmemiş mesajları RabbitMQ'ya publish eder.

RabbitMQ yapılandırması:

- Exchange: `expense.events`
- Queue: `notification.expense-events`
- Routing key değerleri:
  - `expense.created`
  - `expense.approved`
  - `expense.rejected`

NotificationService bu queue'yu consume eder ve notification kaydı oluşturur.

## Senkron İletişim

NotificationService, event aldıktan sonra detaylı harcama bilgisi için ExpenseService endpointini çağırır:

```http
GET /api/expenses/{id}
```

Bu çağrıda internal service JWT kullanılır. `X-Correlation-Id` header olarak taşınır. HTTP client standard resilience/retry pipeline kullanır.

## Multi-Tenant İzolasyon

- Her tenant entity `TenantId` taşır.
- `TenantId` JWT claim içinden okunur.
- `ICurrentUserContext` merkezi kullanıcı/tenant bilgisini sağlar.
- EF Core global query filter tenant ve soft delete filtresini otomatik uygular.
- Controller içinde tenant filtresi elle uygulanmaz.

## Soft Delete

Hard delete yoktur. `BaseEntity` şu alanları içerir:

- `Id`
- `CreatedAt`, `CreatedBy`
- `UpdatedAt`, `UpdatedBy`
- `IsDeleted`
- `DeletedAt`, `DeletedBy`

Repository `Delete` çağrısı EF SaveChanges sırasında soft delete'e çevrilir.

## Güvenlik

- Varsayılan mod basit JWT login endpointidir.
- Token içinde `UserId`, `TenantId` ve role claimleri bulunur.
- Admin endpointleri `[Authorize(Roles = "Admin")]` ile korunur.
- OAuth2/Keycloak ana Docker Compose akışına dahildir. Authentication Keycloak tarafında, authorization API tarafında JWT claimleriyle yapılır. API kullanıcı tokenı üretmez.
