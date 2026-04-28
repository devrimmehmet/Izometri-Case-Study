# TODO Yol Haritası

## Doğrulanmış Tamamlananlar

- [x] .NET 10 solution ve Onion Architecture proje yapısı oluşturuldu.
- [x] `ExpenseService.Api` ve `NotificationService.Api` bağımsız API olarak
      eklendi.
- [x] Servis başına ayrı PostgreSQL veritabanı modeli kuruldu.
- [x] Ortak integration event contractları eklendi.
- [x] ExpenseService domain entity ve enumları eklendi.
- [x] NotificationService domain entityleri eklendi.
- [x] EF Core DbContext, Code First migration, global query filter, audit
      alanları ve soft delete davranışı eklendi.
- [x] Generic Repository ve UnitOfWork eklendi.
- [x] İki tenant ve Admin/HR/Personel seed kullanıcıları eklendi.
- [x] E-posta benzersizliği tenant bazlı `(TenantId, Email)` olarak tanımlandı.
- [x] JWT login endpointi ve `UserId`, `TenantId`, role claimleri eklendi.
- [x] Current user, tenant context ve correlation context eklendi.
- [x] FluentValidation request validatorları eklendi.
- [x] ExpenseService endpointleri eklendi: login, admin user list/create, role
      update, create, list, detail, submit, approve, reject, delete.
- [x] NotificationService endpointi eklendi: `GET /api/notifications`.
- [x] Personel ile HR/Admin görünürlük kuralları eklendi.
- [x] `5000 TRY` ve altı ile `5000 TRY` üzeri approval flow eklendi.
- [x] Ret açıklaması validasyonu eklendi.
- [x] Expense create/approve/reject transactionları içinde outbox mesajı
      oluşturma eklendi.
- [x] RabbitMQ outbox publisher worker eklendi.
- [x] Outbox mesajları için retry ve dead-letter davranışı eklendi.
- [x] NotificationService RabbitMQ consumer eklendi.
- [x] Notification persistence ve processed-message idempotency eklendi.
- [x] Mock notification loglama eklendi.
- [x] NotificationService -> ExpenseService HTTP client eklendi.
- [x] NotificationService -> ExpenseService HTTP çağrısı için resilience/retry
      policy eklendi.
- [x] Swagger/OpenAPI iki API için aktif edildi.
- [x] Dockerfile ve Docker Compose eklendi.
- [x] Local port çakışmalarını önlemek için RabbitMQ `5673/15673`, PostgreSQL
      `15433/15434` olarak ayarlandı.
- [x] OAuth2/Keycloak opsiyonel doğrulama modu eklendi; basit JWT login
      korunuyor.
- [x] Docker Compose `oauth` profiline Keycloak servisi eklendi.
- [x] Keycloak realm/client/role/demo user import dosyası eklendi:
      `deploy/keycloak/izometri-realm.json`.
- [x] Keycloak import rehberi eklendi: `Docs/keycloak-import-rehberi.md`.
- [x] Outbox dead-letter mesajlarını görüntülemek için admin endpointi eklendi:
      `GET /api/admin/outbox/dead-letters`.
- [x] NotificationService için `NotificationDeadLetters` tablosu ve consumer
      retry/dead-letter stratejisi eklendi.
- [x] Global exception middleware eklendi (`ApiExceptionMiddleware`, iki API'de
      de mevcut).
- [x] Serilog console logging eklendi.
- [x] Health check endpointleri eklendi: `GET /health`.
- [x] Docker Compose healthcheck tanımları eklendi.
- [x] GitHub Actions CI pipeline eklendi.
- [x] Postman koleksiyonu eklendi:
      `Docs/IzometriCaseStudy.postman_collection.json`.
- [x] Swagger/OpenAPI JSON çıktıları teslim artifacti olarak eklendi.
- [x] xUnit + Moq test projesi eklendi.
- [x] ExpenseService API controller unit testleri eklendi.
- [x] Admin user/role controller unit testleri eklendi.
- [x] Admin outbox controller unit testi eklendi.
- [x] NotificationService API controller unit testi eklendi.
- [x] Notification event handler unit testleri eklendi.
- [x] E-posta teslim durumu `EmailStatus` ve `EmailError` alanlarıyla
      notification kayıtlarına eklendi.
- [x] Local e-posta testi için Docker Compose'a Mailpit eklendi.
- [x] Netgsm SMS gönderimi REST v2 JSON POST formatına taşındı.
- [x] Netgsm SMS gönderici için unit testler eklendi.
- [x] Canlı local health testi eklendi.
- [x] Canlı Mailpit e-posta testi eklendi.
- [x] Canlı Netgsm SMS testi eklendi; provider `40 invalidHeader` döndürdü
      (hesap sorunu, kod hatası değil).
- [x] Validator ve approval-threshold unit testleri eklendi.
- [x] Canlı Docker integration testi tamamen izole Testcontainers altyapısına
      taşındı.
- [x] Tenant-crossing negative test canlı integration akışında doğrulandı.
- [x] Multi-role kullanıcı oluşturma ve rol güncelleme canlı integration
      akışında doğrulandı.
- [x] OpenTelemetry collector, trace export ve dağıtık trace görselleştirme
      eklendi; Jaeger UI `localhost:16686`.
- [x] Notification dead-letter kayıtlarını listelemek için
      `GET /api/admin/notifications/dead-letters` endpointi eklendi.

## Gönderim Öncesi Zorunlu Düzeltmeler

- [x] **README test kullanıcıları ve tenant tablosu güncellendi.**
      `acme`/`globex` kaldırıldı; seed data'daki gerçek `test1`, `test2`,
      `izometri` tenant kodları ve kullanıcılar eklendi. Giriş örneği de
      düzeltildi.
- [x] **`NotificationsController` üzerindeki `[Authorize]` eksikliği
      giderildi.** `GET /api/notifications` artık JWT Bearer gerektiriyor;
      integration testi de token gönderecek şekilde güncellendi.
- [x] **Controller'lardaki `Execute()` try/catch kaldırıldı.**
      `ExpensesController` ve `AdminUsersController` artık exception'ları
      `ApiExceptionMiddleware`'e iletiyor; tüm hata yanıtları tutarlı
      `application/problem+json` formatında. İlgili unit testler
      `Assert.ThrowsAsync` ile güncellendi.
- [x] **`AdminNotificationTestController` → `AdminEmailProbeController` olarak
      yeniden adlandırıldı.** Hardcode kişisel e-posta ve `"testtir"` subject
      kaldırıldı; endpoint `POST /api/admin/notifications/probe-email` oldu ve
      tüm alanlar zorunlu hale getirildi.
- [x] **`UpdateExpenseRequestValidator` eklendi.** Kategori, para birimi, tutar
      ve açıklama (min. 20 karakter) kuralları uygulandı.
- [x] **`ExpenseResponse` DTO'suna `RequiresAdminApproval` alanı eklendi.**
      İstemci artık onay akışını DTO'dan okuyabiliyor.
- [x] **`SettingsController` Onion Architecture uyumuna getirildi.**
      `IExchangeRateAdminService` application servisi oluşturuldu; controller
      artık yalnızca bu servisi çağırıyor, `IUnitOfWork`/domain entity referansı
      API katmanından kaldırıldı.
- [x] **README API Scope bölümüne eksik endpointler eklendi.**
      `PUT /api/expenses/{id}`, `GET/PUT /api/settings/exchange-rates` ve
      `POST /api/admin/notifications/probe-email` belgelendi.

## Profesyonel Kalite İyileştirmeleri

- [x] **Tüm controller action'larına `[ProducesResponseType]` ve
      `[Produces("application/json")]` attribute'ları eklendi.** Her endpoint
      başarı ve hata durum kodlarını Swagger'da doğru şekilde raporluyor.
- [x] **`AuthController.Login` `ProblemDetails` formatına getirildi.** Geçersiz
      kimlik bilgisi 401 + `ProblemDetails` döndürüyor; login hatası artık tüm
      API ile tutarlı format kullanıyor.
- [x] **`DELETE /api/expenses/{id}` davranışı README İş Kuralları bölümüne
      belgelendi.** Personel yalnızca kendi harcamalarını, HR/Admin görünürlük
      kapsamlarındaki tüm harcamaları silebilir.
- [x] **README'ye GitHub Actions CI badge eklendi.**
- [x] **`Docs/gereksinim-uyumluluk-matrisi.md` güncellendi.** Auth ve mimari
      düzeltmeler, test sayısı (34) yansıtıldı.
- [x] **`Docs/api-deneme-rehberi.md` seed veri ile senkronize edildi.**
      `acme`/`globex` ve var olmayan kullanıcılar kaldırıldı; `test1`/`test2`
      gerçek tenant kodları ve kullanıcıları eklendi. Notification endpoint auth
      gereksinimi de belgelendi.

## Frontend Düzeltmeleri

- [x] **`notifyApi` axios instance'ına JWT Bearer interceptor eklendi.**
      `GET /api/notifications` artık `[Authorize]` gerektirdiğinden `notifyApi`
      de localStorage'dan token okuyarak Authorization header gönderiyor.
- [x] **`ExpenseDto` type'ına `requiresAdminApproval` ve `rejectionReason`
      eklendi.**
- [x] **`PendingAdminApproval` sahte status türü kaldırıldı.** `types/index.ts`,
      `utils/tr.ts`, `ExpensesPage.vue` ve `DashboardPage.vue` güncellendi;
      filtre ve dashboard istatistiği gerçek backend enum değerlerini
      kullanıyor.
- [x] **`canApproveRow`, `statusClass`, `statusLabel` ve onay butonu logic'i
      `requiresAdminApproval` DTO alanını kullanacak şekilde güncellendi.**
      Önceki `currency === 'TRY' && amount > 5000` kontrolü exchange rate'i
      dikkate almıyordu.
- [x] **Harcama detail dialog'una `rejectionReason` alanı eklendi.**

## Outbox Pattern ve Bonus Geliştirmeleri

- [x] **[BUG FIX] `ExpenseRequiresAdminApproval` routing key'i RabbitMQ
      kuyruğuna bind edilmedi.** `RabbitMqConsumerWorker` üç event için binding
      yapıyordu; HR onayından geçen >5000 TRY harcamalar için Admin'e gönderilen
      `expense.requires_admin_approval` event'i hiç consume edilmiyordu. Binding
      eklendi; canlı E2E testte doğrulandı.
- [x] **OutboxPublisherWorker — persistent RabbitMQ connection.** Eski kod her 5
      saniyede yeni TCP bağlantısı açıp kapıyordu (12 connection/dakika).
      Bağlantı worker lifecycle'ına taşındı; kanal açık olduğu sürece yeniden
      kullanılıyor, hata durumunda reconnect yapılıyor.
- [x] **OutboxPublisherWorker — PostgreSQL `SELECT FOR UPDATE SKIP LOCKED`.**
      Birden fazla publisher instance çalışırken (horizontal scale) aynı mesajın
      çift publish edilmesini önler. Her poll, `BeginTransactionAsync` + raw SQL
      ile row-level lock alır; kilitsiz satırları atlar, diğer worker'ları
      bloklamaz.
- [x] **Swagger JWT SecurityDefinition eklendi (TB-5 tamamlandı).**
      `AddSwaggerGen` içinde `AddSecurityDefinition("Bearer")` ve
      `AddSecurityRequirement` yapılandırıldı. Swagger UI'da "Authorize" butonu
      aktif; değerlendirici login token'ını girerek tüm endpoint'leri doğrudan
      test edebilir. Microsoft.OpenApi 2.4.1 API uyumluluğu sağlandı.

## Doğrulanan Komutlar

- [x] `dotnet build Izometri.CaseStudy.slnx`
- [x] `dotnet test Izometri.CaseStudy.slnx`
- [x] `docker compose config --quiet`
- [x] `docker compose up -d --build`
- [x] `docker compose ps`
- [x] Swagger/OpenAPI artifact üretimi:
  - `Docs/openapi-expense.json`
  - `Docs/openapi-notification.json`
- [x] Canlı local akış: login, admin kullanıcı/rol yönetimi, harcama oluşturma,
      submit, HR approve, outbox publish, RabbitMQ consume, notification kaydı
      ve e-posta teslim durumu kontrolü.

## Artı Puan İçin Ek TODO Önerileri

- [x] **Keycloak admin API ile kullanıcı oluşturma senkronizasyonu.**
      `POST /api/admin/users` artık uygulama DB'sine kullanıcı açtıktan sonra
      Keycloak Admin REST API ile aynı kullanıcıyı Keycloak'ta da oluşturuyor.
      `IKeycloakAdminClient` abstraction'ı Application katmanında,
      `KeycloakAdminClient` implementasyonu Infrastructure'da.
      `Keycloak:Enabled=false` iken no-op; Docker Compose ortamında otomatik
      aktif. Service-account `client_credentials` token'ı ile `manage-users`
      rolüyle çalışır.
- [x] **Contract/integration testleri Keycloak tokenlarıyla genişletme.**
      `KeycloakTokenIntegrationTests` eklendi: `RUN_KEYCLOAK_TESTS=1` ile
      çalışır. 6 test: token kabul, claim doğrulaması (`aud=expense-service`,
      `TenantId`, `UserId`, `role`), RBAC (Personel→403), Keycloak user sync
      E2E, geçersiz token ret, tenant izolasyonu.
- [x] **OpenAPI artifactlerini build pipeline'da otomatik üretme.**
      `Docs/openapi-expense.json` ve `Docs/openapi-notification.json` manuel
      artifact yerine CI adımında üretilip drift kontrolü yapılabilir.
- [x] **NotificationService için tenant-aware query filter.** Controller token
      tenant kontrolü yapıyor; defense-in-depth için Notification DB tarafında
      da current tenant context + global query filter eklenebilir.
- [x] **Outbox cleanup/retention job.** Başarıyla publish edilmiş eski
      `OutboxMessages` ve eski `ProcessedMessages` kayıtları için retention
      policy eklemek operasyonel olgunluk sağlar.
- [x] **Rate limiting ve security headers.** Public API gateway/nginx katmanında
      rate limit, HSTS/CSP ve request size limitleri eklenebilir.
- [x] **Readiness/liveness ayrımı.** `/health` yanında DB, RabbitMQ ve Keycloak
      bağımlılıklarını ayrı readiness check olarak raporlamak
      Kubernetes/production anlatısını güçlendirir.
