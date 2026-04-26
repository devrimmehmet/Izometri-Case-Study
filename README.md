# İzometri Case Study - Multi-Tenant Expense Management

Senior .NET Backend case çalışması için hazırlanmış çok kiracılı harcama yönetim sistemi. Çözüm iki bağımsız API, servis başına ayrı veritabanı, RabbitMQ ile asenkron iletişim, EF Core Code First, JWT kimlik doğrulama, soft delete, global tenant filtresi, outbox pattern ve Docker Compose desteği içerir.

## Mimari Topoloji

- `ExpenseService.Api`: Kimlik doğrulama, harcama yaşam döngüsü, tenant izolasyonu, kullanıcı/rol yönetimi ve outbox publisher.
- `NotificationService.Api`: RabbitMQ consumer, bildirim kaydı, mock bildirim loglama ve ExpenseService üzerinden HTTP ile harcama detayı okuma.
- `ExpenseManagement.Contracts`: Ortak integration event contractları.
- PostgreSQL servis veritabanları:
  - `expense_db`
  - `notification_db`
- RabbitMQ:
  - Exchange: `expense.events`
  - Queue: `notification.expense-events`
  - Routing key değerleri: `expense.created`, `expense.approved`, `expense.rejected`

## Hızlı Başlangıç

Gereksinimler:

- .NET 10 SDK
- Docker Desktop

```bash
dotnet build Izometri.CaseStudy.slnx
dotnet test Izometri.CaseStudy.slnx
docker compose up -d --build
```

Servis adresleri:

- Expense API Swagger: `http://localhost:5001/swagger`
- Notification API Swagger: `http://localhost:5002/swagger`
- RabbitMQ AMQP: `localhost:5673`
- RabbitMQ Management: `http://localhost:15673` (`guest` / `guest`)
- Expense PostgreSQL: `localhost:15433`
- Notification PostgreSQL: `localhost:15434`

Detaylı local/prod çalıştırma ve deneme rehberi: [Docs/çalıştırma-ve-ortamlar.md](Docs/çalıştırma-ve-ortamlar.md)

## Teslim Dokümanları

Case değerlendirmesi için detaylar README yerine Docs altında ayrı dosyalara bölünmüştür:

- [Teslimat Özeti](Docs/teslimat-özeti.md)
- [Mimari ve Topoloji](Docs/mimari-topoloji.md)
- [API Deneme Rehberi](Docs/api-deneme-rehberi.md)
- [Migration ve Seed Bilgileri](Docs/migration-seed.md)
- [Test ve Doğrulama](Docs/test-dogrulama.md)
- [Gereksinim Uyumluluk Matrisi](Docs/gereksinim-uyumluluk-matrisi.md)
- [Çalıştırma ve Ortam Rehberi](Docs/çalıştırma-ve-ortamlar.md)
- [Case Üzerinden Eklenen Sonraki İyileştirmeler](Docs/case-sonraki-iyileştirmeler.md)
- [Keycloak Import Rehberi](Docs/keycloak-import-rehberi.md)
- [Postman Koleksiyonu](Docs/IzometriCaseStudy.postman_collection.json)
- [ExpenseService OpenAPI JSON](Docs/openapi-expense.json)
- [NotificationService OpenAPI JSON](Docs/openapi-notification.json)
- [Proje Planı](Docs/project-plan.md)

## Test Kullanıcıları

Tüm kullanıcılar için şifre: `Pass123!`

| Tenant | E-posta | Roller |
| --- | --- | --- |
| `acme` | `admin@acme.com` | Admin |
| `acme` | `hr@acme.com` | HR |
| `acme` | `personel@demo.com` | Personnel |
| `globex` | `admin@globex.com` | Admin |
| `globex` | `hr@globex.com` | HR |
| `globex` | `personel@demo.com` | Personnel |

Aynı e-posta (`personel@demo.com`) farklı tenantlarda kullanılabilir. Benzersizlik kuralı `(TenantId, Email)` üzerindedir.

## Örnek İstekler

Login:

```http
POST /api/auth/login
```

```json
{
  "email": "personel@demo.com",
  "password": "Pass123!",
  "tenantCode": "acme"
}
```

Harcama oluşturma:

```http
POST /api/expenses
Authorization: Bearer {token}
```

```json
{
  "category": "Travel",
  "currency": "TRY",
  "amount": 3500,
  "description": "İstanbul müşteri toplantısı için ulaşım gideri"
}
```

## API Kapsamı

ExpenseService:

- `POST /api/auth/login`
- `GET /api/admin/users`
- `POST /api/admin/users`
- `PUT /api/admin/users/{userId}/roles`
- `GET /api/admin/outbox/dead-letters`
- `POST /api/expenses`
- `GET /api/expenses?dateFrom=&dateTo=&status=&category=&pageNumber=&pageSize=`
- `GET /api/expenses/{id}`
- `PUT /api/expenses/{id}/submit`
- `PUT /api/expenses/{id}/approve`
- `PUT /api/expenses/{id}/reject`
- `DELETE /api/expenses/{id}`
- `GET /health`

NotificationService:

- `GET /api/notifications`
- `GET /api/notifications?tenantId={tenantId}`
- `GET /health`

## İş Kuralları

- Personnel harcama oluşturabilir ve yalnızca kendi kayıtlarını görebilir.
- HR/Admin kendi tenantındaki tüm harcamaları görebilir.
- `5000 TRY` ve altı için HR onayı yeterlidir.
- `5000 TRY` üzeri için önce HR, sonra Admin onayı gerekir.
- Ret işleminde en az 10 karakterlik açıklama zorunludur.
- Delete işlemleri fiziksel silme yapmaz; soft delete uygulanır.

## Bonus Kapsamı

- TB-1 Outbox Pattern: Harcama transactionı içinde outbox mesajı yazılır, background worker RabbitMQ'ya publish eder.
- TB-2 OAuth2: Basit JWT login korunur; `Jwt:Authority` verilirse Keycloak/Auth0/IdentityServer gibi dış IdP tokenları doğrulanabilir. Docker Compose içinde Keycloak için `oauth` profili vardır.
- TB-3 Unit Testing: `tests/ExpenseService.Tests` içinde xUnit + Moq altyapısı, controller, validator, event handler ve canlı Docker entegrasyon testleri vardır.
- TB-4 Docker Support: RabbitMQ, iki PostgreSQL DB ve iki API `docker-compose.yml` ile çalışır.
- TB-5 API Documentation: İki API'de Swagger/OpenAPI aktiftir.
- TB-6 Logging/Correlation ID: `X-Correlation-Id` request/response header olarak taşınır, event payloadlarına yazılır ve servisler arası HTTP isteğinde devam eder.
- TR-7 HTTP Retry: NotificationService -> ExpenseService HTTP client'ı standard resilience/retry pipeline kullanır.
- TB-8 Operational Readiness: Healthcheck endpointleri, Docker Compose healthcheck tanımları, Serilog console logging, standart hata formatı ve CI pipeline eklendi.

## Doğrulanan Komutlar

- `dotnet build Izometri.CaseStudy.slnx`
- `dotnet test Izometri.CaseStudy.slnx`
- `docker compose config`
- `docker compose up -d --build`
- Canlı local akış: login, admin kullanıcı/rol yönetimi, harcama oluşturma, submit, HR approve, outbox publish, RabbitMQ consume ve notification kaydı.
