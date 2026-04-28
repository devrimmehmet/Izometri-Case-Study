# Gereksinim Uyumluluk Matrisi

Bu dokuman `Docs/backend_case.md` icindeki gereksinimlerin projede nerede
karsilandigini gosterir.

## Is Gereksinimleri

| Kod  | Gereksinim                | Durum      | Karsilik                                                           |
| ---- | ------------------------- | ---------- | ------------------------------------------------------------------ |
| BR-1 | Multi-tenant izolasyon    | Tamamlandi | JWT `TenantId`, `ICurrentUserContext`, EF Core global query filter |
| BR-2 | Rol tabanli yetkilendirme | Tamamlandi | Admin, HR, Personel rolleri ve endpoint authorization              |
| BR-3 | Harcama talebi olusturma  | Tamamlandi | `POST /api/expenses`, kategori, para birimi, aciklama validasyonu  |
| BR-4 | Onay sureci               | Tamamlandi | HR/Admin sirali onay, ret aciklamasi zorunlulugu                   |
| BR-5 | Asenkron bildirim sistemi | Tamamlandi | RabbitMQ eventleri ve NotificationService consumer                 |
| BR-6 | Sorgulama                 | Tamamlandi | Filtreleme, pagination, role gore gorunurluk                       |

## Teknik Gereksinimler

| Kod   | Gereksinim               | Durum      | Karsilik                                                                                                 |
| ----- | ------------------------ | ---------- | -------------------------------------------------------------------------------------------------------- |
| TR-1  | Mikroservis mimarisi     | Tamamlandi | `ExpenseService.Api`, `NotificationService.Api`                                                          |
| TR-2  | Multi-tenancy            | Tamamlandi | TenantId claim, query filter, tenant-scoped entityler                                                    |
| TR-3  | Soft delete              | Tamamlandi | `BaseEntity`, `IsDeleted`, `DeletedAt`, `DeletedBy`, global filter                                       |
| TR-4  | Role-based authorization | Tamamlandi | JWT `role` claimleri, `[Authorize]`, Admin endpointleri                                                  |
| TR-5  | Onion Architecture       | Tamamlandi | Domain, Application, Infrastructure, Api, Shared/Contracts                                               |
| TR-6  | Repository & UnitOfWork  | Tamamlandi | Generic repository ve UnitOfWork                                                                         |
| TR-7  | Servisler arasi iletisim | Tamamlandi | RabbitMQ async, HTTP sync, retry policy                                                                  |
| TR-8  | Validasyon               | Tamamlandi | FluentValidation ve business rule kontrolleri                                                            |
| TR-9  | ORM & Database           | Tamamlandi | EF Core, Code First migration, PostgreSQL                                                                |
| TR-10 | Authentication           | Tamamlandi | Keycloak access token, JWT Bearer dogrulama, UserId/TenantId/role claimleri; local login sadece fallback |

## Bonus Maddeler

| Kod  | Bonus                  | Durum      | Karsilik                                                                                                                 |
| ---- | ---------------------- | ---------- | ------------------------------------------------------------------------------------------------------------------------ |
| TB-1 | Outbox Pattern         | Tamamlandi | `OutboxMessages`, publisher worker, retry/dead-letter                                                                    |
| TB-2 | OAuth 2.0 entegrasyonu | Tamamlandi | Keycloak ana Docker Compose akisinda otomatik baslar; kullanici tokenlarini Keycloak uretir, API'ler JWT Bearer dogrular |
| TB-3 | Unit Testing           | Tamamlandi | xUnit + Moq, controller/validator/token/event/integration testleri                                                       |
| TB-4 | Docker Support         | Tamamlandi | `docker-compose.yml`, Dockerfile dosyalari                                                                               |
| TB-5 | API Documentation      | Tamamlandi | Swagger/OpenAPI                                                                                                          |
| TB-6 | Logging                | Tamamlandi | Correlation ID, Serilog, OpenTelemetry/Jaeger trace export, mock notification loglari                                    |

## Guvenlik ve Mimari Duzeltmeler

| Alan                            | Duzeltme                                                                                                                                 |
| ------------------------------- | ---------------------------------------------------------------------------------------------------------------------------------------- |
| Keycloak/OAuth2                 | Authentication Keycloak tarafinda, authorization API tarafinda JWT claimleriyle yapilir                                                  |
| Local login                     | Docker/prod akista kapali; gelistirme ve test fallback'i olarak tutulur                                                                  |
| NotificationsController auth    | `[Authorize]` eklendi; endpoint JWT Bearer ve token tenant izolasyonu gerektiriyor                                                       |
| Global exception middleware     | Controller `Execute()` wrapper'lari kaldirildi; hatalar `ApiExceptionMiddleware` uzerinden `application/problem+json` formatinda donuyor |
| Onion Architecture              | `SettingsController` `IExchangeRateAdminService` application servisi uzerinden calisiyor                                                 |
| UpdateExpenseRequest validasyon | `UpdateExpenseRequestValidator` eklendi                                                                                                  |
| ExpenseResponse DTO             | `RequiresAdminApproval` alani eklendi                                                                                                    |

## Bilincli Birakilan Gelistirme Alanlari

- Local e-posta teslimi Docker Compose icindeki Mailpit ile test edilir.
- Uretim ortaminda `Mail:IgnoreCertificateErrors=false` tutulmali ve gecerli TLS
  sertifikasi kullanilmalidir.
- SMS gonderimi Netgsm REST v2 JSON POST ile desteklenir. Canli testte bakiye
  API'si basarili donmus, SMS gonderimi ise hesapta aktif gonderici basligi
  olmadigi icin `40 invalidHeader/header problem` sonucu vermistir. 1 Nisan'dan
  itibaren Netgsm SMS gonderiminde MCP entegrasyonu zorunlu hale gelmistir. Yeni
  entegrasyonun gercek donanim ile test edilmesi gerekir.
