# Gereksinim Uyumluluk Matrisi

Bu doküman `Docs/backend_case.md` içindeki gereksinimlerin projede nerede karşılandığını gösterir.

## İş Gereksinimleri

| Kod | Gereksinim | Durum | Karşılık |
| --- | --- | --- | --- |
| BR-1 | Multi-tenant izolasyon | Tamamlandı | JWT `TenantId`, `ICurrentUserContext`, EF Core global query filter |
| BR-2 | Rol tabanlı yetkilendirme | Tamamlandı | Admin, HR, Personnel rolleri ve endpoint authorization |
| BR-3 | Harcama talebi oluşturma | Tamamlandı | `POST /api/expenses`, kategori, para birimi, açıklama validasyonu |
| BR-4 | Onay süreci | Tamamlandı | HR/Admin sıralı onay, ret açıklaması zorunluluğu |
| BR-5 | Asenkron bildirim sistemi | Tamamlandı | RabbitMQ eventleri ve NotificationService consumer |
| BR-6 | Sorgulama | Tamamlandı | Filtreleme, pagination, role göre görünürlük |

## Teknik Gereksinimler

| Kod | Gereksinim | Durum | Karşılık |
| --- | --- | --- | --- |
| TR-1 | Mikroservis mimarisi | Tamamlandı | `ExpenseService.Api`, `NotificationService.Api` |
| TR-2 | Multi-tenancy | Tamamlandı | TenantId claim, query filter, tenant-scoped entityler |
| TR-3 | Soft delete | Tamamlandı | `BaseEntity`, `IsDeleted`, `DeletedAt`, `DeletedBy`, global filter |
| TR-4 | Role-based authorization | Tamamlandı | JWT role claimleri, `[Authorize]`, Admin endpointleri |
| TR-5 | Onion Architecture | Tamamlandı | Domain, Application, Infrastructure, Api, Shared/Contracts |
| TR-6 | Repository & UnitOfWork | Tamamlandı | Generic repository ve UnitOfWork |
| TR-7 | Servisler arası iletişim | Tamamlandı | RabbitMQ async, HTTP sync, retry policy |
| TR-8 | Validasyon | Tamamlandı | FluentValidation ve business rule kontrolleri |
| TR-9 | ORM & Database | Tamamlandı | EF Core, Code First migration, PostgreSQL |
| TR-10 | Authentication | Tamamlandı | JWT login endpointi, UserId/TenantId/role claimleri |

## Bonus Maddeler

| Kod | Bonus | Durum | Karşılık |
| --- | --- | --- | --- |
| TB-1 | Outbox Pattern | Tamamlandı | `OutboxMessages`, publisher worker, retry/dead-letter |
| TB-2 | OAuth 2.0 entegrasyonu | Tamamlandı / opsiyonel mod | `Jwt:Authority` desteği, Keycloak compose profili ve realm/client/role/user import dosyası var |
| TB-3 | Unit Testing | Tamamlandı | xUnit + Moq, 34 test |
| TB-4 | Docker Support | Tamamlandı | `docker-compose.yml`, Dockerfile dosyaları |
| TB-5 | API Documentation | Tamamlandı | Swagger/OpenAPI |
| TB-6 | Logging | Tamamlandı | Correlation ID, Serilog, OpenTelemetry/Jaeger trace export, mock notification logları |

## Güvenlik ve Mimari Düzeltmeler

| Alan | Düzeltme |
| --- | --- |
| NotificationsController auth | `[Authorize]` eklendi; endpoint artık JWT Bearer gerektiriyor |
| Global exception middleware | Controller `Execute()` wrapper'ları kaldırıldı; tüm hatalar `ApiExceptionMiddleware` üzerinden `application/problem+json` formatında dönüyor |
| Onion Architecture | `SettingsController` `IExchangeRateAdminService` application servisi üzerinden çalışıyor; API katmanında `IUnitOfWork`/domain entity referansı yok |
| UpdateExpenseRequest validasyon | `UpdateExpenseRequestValidator` eklendi |
| ExpenseResponse DTO | `RequiresAdminApproval` alanı eklendi |

## Bilinçli Bırakılan Geliştirme Alanları

- Local e-posta teslimi Docker Compose içindeki Mailpit ile test edilir. Gerçek SMTP teslimi sunucunun kimlik doğrulama ve sertifika yapılandırmasına bağlıdır. Uygulama teslim sonucunu `EmailStatus` ve `EmailError` alanlarıyla görünür kılar.
- Üretim ortamında `Mail:IgnoreCertificateErrors=false` tutulmalı ve geçerli TLS sertifikası kullanılmalıdır.
- SMS gönderimi Netgsm REST v2 JSON POST ile desteklenir. Canlı testte bakiye API’si başarılı dönmüş, SMS gönderimi ise hesapta aktif gönderici başlığı olmadığı için `40 invalidHeader/header problem` sonucu vermiştir.
