# Proje Planı

## Solution Yapısı

- `src/Shared/ExpenseManagement.Contracts`: Ortak event contractları.
- `src/Services/ExpenseService/ExpenseService.Domain`: Expense domain entity ve enumları.
- `src/Services/ExpenseService/ExpenseService.Application`: DTO, validator, interface ve use-case servisleri.
- `src/Services/ExpenseService/ExpenseService.Infrastructure`: EF Core, repository, UnitOfWork, seed, migration, JWT, outbox publisher.
- `src/Services/ExpenseService/ExpenseService.Api`: Controller, auth, Swagger ve middleware.
- `src/Services/NotificationService/NotificationService.Domain`: Notification entityleri.
- `src/Services/NotificationService/NotificationService.Application`: Consumer handler ve query contractları.
- `src/Services/NotificationService/NotificationService.Infrastructure`: EF Core, migration, RabbitMQ consumer ve Expense HTTP client.
- `src/Services/NotificationService/NotificationService.Api`: Swagger ve notification görüntüleme endpointi.

## Entity Listesi

ExpenseService:

- `Tenant`
- `User`
- `UserRole`
- `Expense`
- `ExpenseApproval`
- `OutboxMessage`

NotificationService:

- `Notification`
- `ProcessedMessage`

Ortak base tipler:

- `BaseEntity`
- `TenantEntity`

## Endpoint Listesi

ExpenseService:

- `POST /api/auth/login`
- `GET /api/admin/users`
- `POST /api/admin/users`
- `PUT /api/admin/users/{userId}/roles`
- `POST /api/expenses`
- `GET /api/expenses`
- `GET /api/expenses/{id}`
- `PUT /api/expenses/{id}/submit`
- `PUT /api/expenses/{id}/approve`
- `PUT /api/expenses/{id}/reject`
- `DELETE /api/expenses/{id}`

NotificationService:

- `GET /api/notifications`

## Veritabanı Tabloları

Expense DB:

- `Tenants`
- `Users`
- `UserRoles`
- `Expenses`
- `ExpenseApprovals`
- `OutboxMessages`

Notification DB:

- `Notifications`
- `ProcessedMessages`

## Event Contractları

- `ExpenseCreatedEvent`
- `ExpenseApprovedEvent`
- `ExpenseRejectedEvent`

RabbitMQ:

- Exchange: `expense.events`
- Queue: `notification.expense-events`
- Routing key değerleri: `expense.created`, `expense.approved`, `expense.rejected`

## Bonus / Artı Puan Kapsamı

- Outbox Pattern uygulandı.
- Outbox mesajları 10 başarısız publish denemesinden sonra dead-letter durumuna alınır.
- Docker Compose ile RabbitMQ, iki PostgreSQL DB, iki API ve opsiyonel Keycloak profili eklendi.
- Swagger/OpenAPI iki API'de aktif.
- Correlation ID header, event payload ve servisler arası HTTP request boyunca taşınıyor.
- NotificationService HTTP client'ı standard resilience/retry policy kullanıyor.
- xUnit + Moq test projesi eklendi.
- Canlı Docker entegrasyon testi API + PostgreSQL + RabbitMQ akışını doğruluyor.
- OAuth2/Keycloak modu `Jwt:Authority` ayarı ve `oauth` compose profili ile destekleniyor.

## Local Portlar

- Expense API: `5001`
- Notification API: `5002`
- RabbitMQ AMQP: `5673`
- RabbitMQ Management: `15673`
- Expense DB: `15433`
- Notification DB: `15434`
- Keycloak opsiyonel profil: `18080`
