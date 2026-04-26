# TODO Yol Haritası

## Doğrulanmış Tamamlananlar

- [x] .NET 10 solution ve Onion Architecture proje yapısı oluşturuldu.
- [x] `ExpenseService.Api` ve `NotificationService.Api` bağımsız API olarak eklendi.
- [x] Servis başına ayrı PostgreSQL veritabanı modeli kuruldu.
- [x] Ortak integration event contractları eklendi.
- [x] ExpenseService domain entity ve enumları eklendi.
- [x] NotificationService domain entityleri eklendi.
- [x] EF Core DbContext, Code First migration, global query filter, audit alanları ve soft delete davranışı eklendi.
- [x] Generic Repository ve UnitOfWork eklendi.
- [x] İki tenant ve Admin/HR/Personnel seed kullanıcıları eklendi.
- [x] E-posta benzersizliği tenant bazlı `(TenantId, Email)` olarak tanımlandı.
- [x] JWT login endpointi ve `UserId`, `TenantId`, role claimleri eklendi.
- [x] Current user, tenant context ve correlation context eklendi.
- [x] FluentValidation request validatorları eklendi.
- [x] ExpenseService endpointleri eklendi: login, admin user list/create, role update, create, list, detail, submit, approve, reject, delete.
- [x] NotificationService endpointi eklendi: `GET /api/notifications`.
- [x] Personnel ile HR/Admin görünürlük kuralları eklendi.
- [x] `5000 TRY` ve altı ile `5000 TRY` üzeri approval flow eklendi.
- [x] Ret açıklaması validasyonu eklendi.
- [x] Expense create/approve/reject transactionları içinde outbox mesajı oluşturma eklendi.
- [x] RabbitMQ outbox publisher worker eklendi.
- [x] Outbox mesajları için retry ve dead-letter davranışı eklendi.
- [x] NotificationService RabbitMQ consumer eklendi.
- [x] Notification persistence ve processed-message idempotency eklendi.
- [x] Mock notification loglama eklendi.
- [x] NotificationService -> ExpenseService HTTP client eklendi.
- [x] NotificationService -> ExpenseService HTTP çağrısı için resilience/retry policy eklendi.
- [x] Swagger/OpenAPI iki API için aktif edildi.
- [x] Dockerfile ve Docker Compose eklendi.
- [x] Local port çakışmalarını önlemek için RabbitMQ `5673/15673`, PostgreSQL `15433/15434` olarak ayarlandı.
- [x] OAuth2/Keycloak opsiyonel doğrulama modu eklendi; basit JWT login korunuyor.
- [x] Docker Compose `oauth` profiline Keycloak servisi eklendi.
- [x] Keycloak realm/client/role/demo user import dosyası eklendi: `deploy/keycloak/izometri-realm.json`.
- [x] Keycloak import rehberi eklendi: `Docs/keycloak-import-rehberi.md`.
- [x] Outbox dead-letter mesajlarını görüntülemek için admin endpointi eklendi: `GET /api/admin/outbox/dead-letters`.
- [x] NotificationService için `NotificationDeadLetters` tablosu ve consumer retry/dead-letter stratejisi eklendi.
- [x] API response modeline standart `application/problem+json` hata formatı ve global exception middleware eklendi.
- [x] Serilog console logging eklendi.
- [x] Health check endpointleri eklendi: `GET /health`.
- [x] Docker Compose healthcheck tanımları eklendi.
- [x] GitHub Actions CI pipeline eklendi.
- [x] Postman koleksiyonu eklendi: `Docs/IzometriCaseStudy.postman_collection.json`.
- [x] Swagger/OpenAPI JSON çıktıları teslim artifacti olarak eklendi.
- [x] xUnit + Moq test projesi eklendi.
- [x] ExpenseService API controller unit testleri eklendi.
- [x] Admin user/role controller unit testleri eklendi.
- [x] Admin outbox controller unit testi eklendi.
- [x] NotificationService API controller unit testi eklendi.
- [x] Notification event handler unit testleri eklendi.
- [x] Validator ve approval-threshold unit testleri eklendi.
- [x] Canlı Docker integration testi eklendi: API + PostgreSQL + RabbitMQ + Notification consumer.
- [x] Tenant-crossing negative test canlı integration akışında doğrulandı.
- [x] Multi-role kullanıcı oluşturma ve rol güncelleme canlı integration akışında doğrulandı.
- [x] README güncellendi.
- [x] `Docs/project-plan.md` güncellendi.
- [x] `Docs/çalıştırma-ve-ortamlar.md` eklendi.
- [x] `Docs/case-sonraki-iyileştirmeler.md` eklendi.

## Doğrulanan Komutlar

- [x] `dotnet build Izometri.CaseStudy.slnx`
- [x] `dotnet test Izometri.CaseStudy.slnx`
- [x] `docker compose config --quiet`
- [x] `docker compose up -d --build`
- [x] `docker compose ps`
- [x] Swagger/OpenAPI artifact üretimi:
  - `Docs/openapi-expense.json`
  - `Docs/openapi-notification.json`
- [x] Canlı local akış: login, admin kullanıcı/rol yönetimi, harcama oluşturma, submit, HR approve, outbox publish, RabbitMQ consume ve notification kaydı.

## Case Üzerinden Eklenebilecek Sonraki İyileştirmeler

- [ ] Canlı Docker integration testini tamamen izole Testcontainers altyapısına taşı. Mevcut canlı integration testi çalışıyor ve CI compose ile destekleniyor; Testcontainers geçişi ayrı teknik iş olarak kaldı.
- [ ] OpenTelemetry collector, trace export ve dağıtık trace görselleştirme ekle. Serilog console logging tamamlandı; tam trace backend entegrasyonu opsiyonel kaldı.
- [ ] Notification dead-letter kayıtlarını listelemek için NotificationService içinde admin/inspection endpointi ekle. Tablo ve consumer stratejisi tamamlandı; dışarıdan görüntüleme endpointi henüz eklenmedi.
