# Izometri Case Study - Multi-Tenant Expense Management

Senior .NET Backend case icin hazirlanmis multi-tenant expense management sistemi. Cozum iki bagimsiz API, database-per-service, RabbitMQ async event akisi, EF Core Code First, JWT auth, soft delete, global tenant filter ve outbox pattern icerir.

## Mimari Topoloji

- `ExpenseService.Api`: Auth, expense lifecycle, tenant izolasyonu, outbox publisher.
- `NotificationService.Api`: RabbitMQ consumer, notification kaydi, mock notification loglama, ExpenseService HTTP detayi okuma.
- `ExpenseManagement.Contracts`: Ortak integration event contractlari.
- PostgreSQL database per service:
  - `expense_db`
  - `notification_db`
- RabbitMQ:
  - Exchange: `expense.events`
  - Queue: `notification.expense-events`
  - Routing keys: `expense.created`, `expense.approved`, `expense.rejected`

## Calistirma

Prerequisites:

- .NET 10 SDK
- Docker Desktop

Build:

```bash
dotnet build Izometri.CaseStudy.slnx
```

Docker Compose:

```bash
docker compose up --build
```

Servisler:

- Expense API Swagger: `http://localhost:5001/swagger`
- Notification API Swagger: `http://localhost:5002/swagger`
- RabbitMQ AMQP: `localhost:5673`
- RabbitMQ Management: `http://localhost:15673` (`guest` / `guest`)
- Expense PostgreSQL: `localhost:15433`
- Notification PostgreSQL: `localhost:15434`

API containerlari startup sirasinda migrationlari otomatik uygular. Host portlari localde mevcut RabbitMQ/PostgreSQL servisleriyle cakismayacak sekilde secildi.

Local dogrulama:

- `dotnet build Izometri.CaseStudy.slnx`: passed
- `dotnet test Izometri.CaseStudy.slnx --no-build`: passed
- `docker compose up -d --build`: passed
- Login, expense create, submit, HR approve ve RabbitMQ notification flow: passed

## Migration Komutlari

Expense DB:

```bash
dotnet ef database update --project src/Services/ExpenseService/ExpenseService.Infrastructure/ExpenseService.Infrastructure.csproj --startup-project src/Services/ExpenseService/ExpenseService.Api/ExpenseService.Api.csproj
```

Notification DB:

```bash
dotnet ef database update --project src/Services/NotificationService/NotificationService.Infrastructure/NotificationService.Infrastructure.csproj --startup-project src/Services/NotificationService/NotificationService.Api/NotificationService.Api.csproj
```

## Test Kullanicilari

Tum kullanicilar icin sifre: `Pass123!`

| Tenant | Email | Roller |
| --- | --- | --- |
| `acme` | `admin@acme.com` | Admin |
| `acme` | `hr@acme.com` | HR |
| `acme` | `personel@demo.com` | Personnel |
| `globex` | `admin@globex.com` | Admin |
| `globex` | `hr@globex.com` | HR |
| `globex` | `personel@demo.com` | Personnel |

Ayni email (`personel@demo.com`) iki farkli tenant icinde kullanilir; unique constraint `(TenantId, Email)` uzerindedir.

## Ornek Login

```json
{
  "email": "personel@demo.com",
  "password": "Pass123!",
  "tenantCode": "acme"
}
```

Endpoint:

```http
POST /api/auth/login
```

JWT claims:

- `UserId`
- `TenantId`
- `role`

## Ornek Expense Olusturma

```json
{
  "category": "Travel",
  "currency": "TRY",
  "amount": 3500,
  "description": "Istanbul musteri toplantisi icin ulasim gideri"
}
```

Endpoint:

```http
POST /api/expenses
Authorization: Bearer {token}
```

## Approval Flow

- Personnel expense olusturur. Ilk status `Draft`.
- Personnel `PUT /api/expenses/{id}/submit` ile `Pending` yapar.
- `5000 TRY` ve alti icin HR approval yeterlidir ve status `Approved` olur.
- `5000 TRY` uzeri icin once HR, sonra Admin approval gerekir.
- Reject icin reason zorunludur ve minimum 10 karakterdir.
- Sadece HR ve Admin approve/reject yapabilir.

## API Endpointleri

ExpenseService:

- `POST /api/auth/login`
- `POST /api/expenses`
- `GET /api/expenses?dateFrom=&dateTo=&status=&category=&pageNumber=&pageSize=`
- `GET /api/expenses/{id}`
- `PUT /api/expenses/{id}/submit`
- `PUT /api/expenses/{id}/approve`
- `PUT /api/expenses/{id}/reject`
- `DELETE /api/expenses/{id}`

NotificationService:

- `GET /api/notifications`
- `GET /api/notifications?tenantId={tenantId}`

## Bonus Kapsami

- TB-1 Outbox Pattern: Expense transaction icinde outbox mesaji yazilir, background worker RabbitMQ'ya publish eder.
- TB-3 Unit Testing: `tests/ExpenseService.Tests` icinde xUnit + Moq altyapisi ve temel rule/validator testleri vardir.
- TB-4 Docker Support: RabbitMQ, iki PostgreSQL DB ve iki API `docker-compose.yml` ile calisir.
- TB-5 API Documentation: Iki API'de Swagger/OpenAPI aktiftir.
- TB-6 Logging/Correlation ID: `X-Correlation-Id` request/response header olarak tasinir, event payload'larina yazilir ve HTTP client akisi bu id ile devam eder.
- TR-7 HTTP Retry: NotificationService -> ExpenseService HTTP client'i standard resilience/retry pipeline kullanir.
- TB-2 OAuth2: Basit JWT login case gereksinimini karsilar; Keycloak/Auth0 entegrasyonu mevcut mimariyi bozmadan opsiyonel dis IdP modu olarak eklenebilir.

## RabbitMQ Event Akisi

Expense create/approve/reject islemleri ayni transaction icinde `OutboxMessages` tablosuna event yazar.

`OutboxPublisherWorker` periyodik olarak islenmemis mesajlari RabbitMQ `expense.events` exchange'ine publish eder. `NotificationService` bu eventleri `notification.expense-events` queue'sundan consume eder, notification kaydini DB'ye yazar ve mock notification mesajini loglar.

Eventler:

- `ExpenseCreatedEvent`
- `ExpenseApprovedEvent`
- `ExpenseRejectedEvent`

Her event `EventId`, `CorrelationId`, `OccurredAt`, `TenantId`, `ExpenseId` alanlarini tasir.

## Multi-Tenant Izolasyon

- Tenant bilgisi JWT `TenantId` claim'inden okunur.
- Tenant-scoped entity'ler `TenantId` tasir.
- EF Core global query filter tenant verisini otomatik filtreler.
- Personnel sadece kendi expense kayitlarini gorur.
- HR/Admin sadece kendi tenantindaki tum kayitlari gorur.
- Tenant izolasyonu controller icinde manuel uygulanmaz; Application + DbContext filter katmaninda merkezilesir.

## Soft Delete

Hard delete yoktur. `BaseEntity` alanlari:

- `Id`
- `CreatedAt`, `CreatedBy`
- `UpdatedAt`, `UpdatedBy`
- `IsDeleted`
- `DeletedAt`, `DeletedBy`

Repository `Delete` fiziksel silme yerine EF SaveChanges sirasinda soft delete flaglerini set eder. Global query filter `IsDeleted = false` uygular.

## Eksik / Bonus Kalan Noktalar

- OAuth2/Keycloak entegrasyonu eklenmedi; case kapsaminda basit JWT login kullanildi.
- Unit test kapsami temel validator/domain rule seviyesindedir; integration testler roadmap'tedir.
- Notification gonderimi mock DB kaydi ve log davranisi olarak tasarlandi; gercek email/SMS yoktur.
