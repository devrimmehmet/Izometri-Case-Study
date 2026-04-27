# Case Üzerinden Eklenen Sonraki İyileştirmeler

Bu doküman, temel case gereksinimlerinden sonra ek puan için tamamlanan iyileştirmeleri özetler.

## Kimlik Doğrulama Genişletmesi

- Keycloak için opsiyonel `oauth` Docker Compose profili bulunur.
- `deploy/keycloak/izometri-realm.json` dosyası realm, client, rol ve demo kullanıcı import eder.
- Detaylı kullanım: [Keycloak Import Rehberi](keycloak-import-rehberi.md)

## Operasyonel Dayanıklılık

- ExpenseService outbox mesajlarında retry ve dead-letter alanları bulunur.
- Admin kullanıcılar dead-letter outbox kayıtlarını `GET /api/admin/outbox/dead-letters` endpointinden görebilir.
- NotificationService başarısız event işlemelerini `NotificationDeadLetters` tablosunda takip eder.
- Notification consumer aynı event için retry sayısını artırır ve 10 deneme sonrasında mesajı kuyrukta sonsuz döngüye sokmadan dead-letter tablosunda bırakır.

## Standart API Davranışı

- İki API için global exception middleware eklendi.
- Beklenmeyen hatalar `application/problem+json` formatında döner.
- Problem response içinde `traceId`, `status`, `title`, `detail` ve `instance` alanları bulunur.

## Gözlemlenebilirlik

- İki API Serilog console sink ile yapılandırıldı.
- Correlation ID middleware’i ExpenseService tarafında request/response header ve event payload akışını destekler.
- OpenTelemetry AspNetCore, HttpClient ve messaging ActivitySource instrumentation eklendi.
- Docker Compose içinde Jaeger UI `http://localhost:16686` adresinden açılır.

## E-posta Teslim Durumu

- Acme Admin seed e-postası `devrimmehmet@gmail.com` olarak güncellendi.
- Acme HR seed e-postası `devrimmehmet@msn.com` olarak güncellendi.
- Personel harcama oluşturduğunda `expense.created` bildirimi HR/Admin için iki adrese gönderilmeye çalışılır.
- Notification kayıtlarında `recipientEmail`, `emailStatus` ve `emailError` alanları görünür.
- Docker local ortamında SMTP hedefi Mailpit’tir; mailler `http://localhost:8025` adresinden incelenir.
- Gerçek SMTP ayarı için `Mail` section’ı desteklenir: `FromName`, `FromEmail`, `Host`, `Port`, `UserName`, `Password`, `UseSsl`, `UsePickupFolder`, `IgnoreCertificateErrors`, `PickupFolderPath`.
- Gerçek SMTP kullanıldığında hata oluşursa `EmailStatus=Failed` kaydedilir; event tüketimi devam eder.

## Sağlık Kontrolleri

- İki API için `GET /health` endpointi eklendi.
- Docker Compose içinde RabbitMQ, iki PostgreSQL servisi ve iki API için healthcheck tanımlandı.
- API container image içine healthcheck için `curl` eklendi.

## Teslim ve Deneme Artifactleri

- GitHub Actions CI pipeline eklendi: restore, build, compose config, compose startup ve test.
- Postman koleksiyonu eklendi: [IzometriCaseStudy.postman_collection.json](IzometriCaseStudy.postman_collection.json)
- OpenAPI JSON artifactleri servislerden üretilecek şekilde teslim listesine alındı:
  - `Docs/openapi-expense.json`
  - `Docs/openapi-notification.json`
