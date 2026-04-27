# İzometri Case Study - Multi-Tenant Expense Management

![CI](https://github.com/devrimmehmet/Izometri-Case-Study/actions/workflows/ci.yml/badge.svg)

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

Servis adresleri (`docker compose up` sonrası):

| Servis | Adres |
| --- | --- |
| **Frontend (tam sistem)** | `http://localhost:3000` |
| Expense API Swagger | `http://localhost:5001/swagger` |
| Notification API Swagger | `http://localhost:5002/swagger` |
| RabbitMQ Management | `http://localhost:15673` (`izometri` / `Izometri2026!`) |
| Mailpit UI | `http://localhost:8025` |
| Jaeger UI | `http://localhost:16686` |
| Expense PostgreSQL | `localhost:15433` |
| Notification PostgreSQL | `localhost:15434` |

**Sadece frontend geliştirmek için** (Docker backend'ler çalışırken):

```bash
cd frontend
npm run dev          # http://localhost:9000 — quasar dev server
```

`quasar dev` (port 9000) proxy'si `localhost:5001` ve `localhost:5002`'ye yönlenir.
Backend'ler `dotnet run` ile de çalıştırılabilir; `launchSettings.json` portları Docker ile aynıdır (5001/5002).

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
| `test1` | `pattabanoglu@devrimmehmet.com` | Admin |
| `test1` | `devrimmehmet@gmail.com` | HR |
| `test1` | `devrimmehmet@msn.com` | Personnel |
| `test1` | `personel2@test1.com` | Personnel |
| `test2` | `admin@test2.com` | Admin |
| `test2` | `hr@test2.com` | HR |
| `test2` | `personel@test2.com` | Personnel |
| `izometri` | `admin@izometri.com` | Admin |
| `izometri` | `hr@izometri.com` | HR |
| `izometri` | `personel@izometri.com` | Personnel |

Aynı e-posta farklı tenantlarda kullanılabilir. Benzersizlik kuralı `(TenantId, Email)` üzerindedir.

## Örnek İstekler

Login:

```http
POST /api/auth/login
```

```json
{
  "email": "devrimmehmet@msn.com",
  "password": "Pass123!",
  "tenantCode": "test1"
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
- `PUT /api/expenses/{id}`
- `GET /api/expenses?dateFrom=&dateTo=&status=&category=&pageNumber=&pageSize=`
- `GET /api/expenses/{id}`
- `PUT /api/expenses/{id}/submit`
- `PUT /api/expenses/{id}/approve`
- `PUT /api/expenses/{id}/reject`
- `DELETE /api/expenses/{id}`
- `GET /api/settings/exchange-rates`
- `PUT /api/settings/exchange-rates`
- `GET /health`

NotificationService:

- `GET /api/notifications`
- `GET /api/notifications?tenantId={tenantId}`
- `GET /api/admin/notifications/dead-letters`
- `POST /api/admin/notifications/probe-email`
- `GET /health`

## İş Kuralları

- Personnel harcama oluşturabilir ve yalnızca kendi kayıtlarını görebilir.
- HR/Admin kendi tenantındaki tüm harcamaları görebilir.
- `5000 TRY` ve altı için HR onayı yeterlidir.
- `5000 TRY` üzeri için önce HR, sonra Admin onayı gerekir. Onay eşiği döviz kuru üzerinden TRY karşılığı ile hesaplanır.
- Ret işleminde en az 10 karakterlik açıklama zorunludur.
- Harcama açıklaması en az 20 karakter olmalıdır.
- Delete işlemleri fiziksel silme yapmaz; soft delete uygulanır. Personnel yalnızca kendi harcamalarını silebilir. HR ve Admin, görünürlük kapsamlarındaki tüm harcamaları silebilir.

## Bonus Kapsamı

- TB-1 Outbox Pattern: Harcama transactionı içinde outbox mesajı yazılır, background worker RabbitMQ'ya publish eder.
- TB-2 OAuth2: Basit JWT login korunur; `Jwt:Authority` verilirse Keycloak/Auth0/IdentityServer gibi dış IdP tokenları doğrulanabilir. Docker Compose içinde Keycloak için `oauth` profili vardır.
- TB-3 Unit Testing: `tests/ExpenseService.Tests` içinde xUnit + Moq altyapısı, controller, validator, event handler ve canlı Docker entegrasyon testleri vardır.
- TB-4 Docker Support: RabbitMQ, iki PostgreSQL DB ve iki API `docker-compose.yml` ile çalışır.
- TB-5 API Documentation: İki API'de Swagger/OpenAPI aktiftir.
- TB-6 Logging/Correlation ID: `X-Correlation-Id` request/response header olarak taşınır, event payloadlarına yazılır ve servisler arası HTTP isteğinde devam eder.
- TR-7 HTTP Retry: NotificationService -> ExpenseService HTTP client'ı standard resilience/retry pipeline kullanır.
- TB-8 Operational Readiness: Healthcheck endpointleri, Docker Compose healthcheck tanımları, Serilog console logging, standart hata formatı ve CI pipeline eklendi.

## E-posta Kontrolü

Personel Acme tenantında harcama oluşturduğunda HR/Admin alıcıları `devrimmehmet@gmail.com,devrimmehmet@msn.com` olarak notification kaydına yazılır. Docker local ortamında SMTP hedefi Mailpit’tir; mailler `http://localhost:8025` üzerinden görülebilir. `GET /api/notifications` yanıtında `recipientEmail`, `emailStatus` ve `emailError` alanlarıyla gönderim sonucu da izlenir.

Gerçek SMTP için `Mail` configuration section’ı kullanılır: `FromName`, `FromEmail`, `Host`, `Port`, `UserName`, `Password`, `UseSsl`, `UsePickupFolder`, `IgnoreCertificateErrors`, `PickupFolderPath`.

SMS gönderimi Netgsm REST v2 JSON POST API ile yapılır. Local Docker ortamında gerçek SMS gönderimi kapalıdır; prod veya manuel test için `Netgsm__UserCode`, `Netgsm__Password`, `Netgsm__MsgHeader`, `Netgsm__BaseUrl`, `Netgsm__Encoding`, `Netgsm__AppName` ve gerekiyorsa `Netgsm__UseOtpEndpoint` değerleri ortam değişkeni olarak verilmelidir. Netgsm tarafında aktif gönderici başlığı yoksa API `40 invalidHeader/header problem` döndürür.

## Secret Yönetimi

Gerçek şifreler, API anahtarları ve production bağlantı bilgileri commitlenmemelidir. Repository içinde sadece örnek dosyalar tutulur:

- `.env.example`
- `src/Services/ExpenseService/ExpenseService.Api/appsettings.Local.example.json`
- `src/Services/NotificationService/NotificationService.Api/appsettings.Local.example.json`

Gerçek local değerler için `.env`, `.env.local`, `appsettings.Local.json`, `appsettings.Production.json` veya `docker-compose.override.yml` kullanılmalıdır. Bu dosyalar `.gitignore` ile Git dışında bırakılır.

## Doğrulanan Komutlar

- `dotnet build Izometri.CaseStudy.slnx`
- `dotnet test Izometri.CaseStudy.slnx`
- `docker compose config`
- `docker compose up -d --build`
- Canlı local akış: login, admin kullanıcı/rol yönetimi, harcama oluşturma, submit, HR approve, outbox publish, RabbitMQ consume ve notification kaydı.
