# Project Plan

## Solution Structure

- `src/Shared/ExpenseManagement.Contracts`: Shared event contractlari.
- `src/Services/ExpenseService/ExpenseService.Domain`: Expense domain entities/enums.
- `src/Services/ExpenseService/ExpenseService.Application`: DTOs, validators, interfaces, use-case services.
- `src/Services/ExpenseService/ExpenseService.Infrastructure`: EF Core, repositories, UnitOfWork, seed, migrations, JWT, outbox publisher.
- `src/Services/ExpenseService/ExpenseService.Api`: Controllers, auth, Swagger, middleware.
- `src/Services/NotificationService/NotificationService.Domain`: Notification entities.
- `src/Services/NotificationService/NotificationService.Application`: Consumer handler and query contracts.
- `src/Services/NotificationService/NotificationService.Infrastructure`: EF Core, migrations, RabbitMQ consumer, Expense HTTP client.
- `src/Services/NotificationService/NotificationService.Api`: Swagger and notification inspection endpoint.

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

Shared base:

- `BaseEntity`
- `TenantEntity`

## Endpoint Listesi

ExpenseService:

- `POST /api/auth/login`
- `POST /api/expenses`
- `GET /api/expenses`
- `GET /api/expenses/{id}`
- `PUT /api/expenses/{id}/submit`
- `PUT /api/expenses/{id}/approve`
- `PUT /api/expenses/{id}/reject`
- `DELETE /api/expenses/{id}`

NotificationService:

- `GET /api/notifications`

## Database Tablolari

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

## Event Contractlari

- `ExpenseCreatedEvent`
- `ExpenseApprovedEvent`
- `ExpenseRejectedEvent`

RabbitMQ:

- Exchange: `expense.events`
- Queue: `notification.expense-events`
- Routing keys: `expense.created`, `expense.approved`, `expense.rejected`

## Implementation Phase Listesi

1. Solution ve project scaffold.
2. Shared contracts.
3. Expense domain/application/infrastructure/API.
4. JWT auth, tenant/current user context.
5. EF Core filters, seed, repository, UnitOfWork, migrations.
6. Expense business rules and endpoints.
7. Outbox pattern and RabbitMQ publisher.
8. Notification DB, RabbitMQ consumer, mock notification persistence.
9. Docker Compose and Dockerfiles.
10. README, TODO and verification.

## Bonus / Arti Puan Kapsami

- Outbox Pattern uygulandi.
- Docker Compose ile RabbitMQ, iki PostgreSQL DB ve iki API eklendi.
- Swagger/OpenAPI iki API'de aktif.
- Correlation ID header, event payload ve servisler arasi HTTP request boyunca tasiniyor.
- NotificationService HTTP client'i standard resilience/retry policy kullaniyor.
- xUnit + Moq test projesi eklendi.
- OAuth2/Keycloak entegrasyonu, mevcut JWT login'i bozmamak icin opsiyonel roadmap maddesi olarak ayrildi.

## Local Portlar

- Expense API: `5001`
- Notification API: `5002`
- RabbitMQ AMQP: `5673`
- RabbitMQ Management: `15673`
- Expense DB: `15433`
- Notification DB: `15434`
