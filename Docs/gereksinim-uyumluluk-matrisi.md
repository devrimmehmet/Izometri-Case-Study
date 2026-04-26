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
| TB-2 | OAuth 2.0 entegrasyonu | Kısmi / opsiyonel mod | `Jwt:Authority` desteği, Keycloak compose profili ve realm/client/role/user import dosyası var |
| TB-3 | Unit Testing | Tamamlandı | xUnit + Moq, 23 test |
| TB-4 | Docker Support | Tamamlandı | `docker-compose.yml`, Dockerfile dosyaları |
| TB-5 | API Documentation | Tamamlandı | Swagger/OpenAPI |
| TB-6 | Logging | Tamamlandı | Correlation ID, mock notification logları |

## Bilinçli Bırakılan Geliştirme Alanları

- Testcontainers ile canlı integration testleri CI ortamında tamamen izole hale getirilebilir.
- OpenTelemetry collector ve trace backend entegrasyonu ile gözlemlenebilirlik güçlendirilebilir.
- CI ortamında Testcontainers tabanlı izole integration testleri eklenebilir.
- Standart hata response modeli ve global exception middleware eklenebilir.
