# Test ve Doğrulama

## Komutlar

Build:

```bash
dotnet build Izometri.CaseStudy.slnx
```

Normal test paketi:

```bash
dotnet test Izometri.CaseStudy.slnx
```

Docker Compose doğrulama:

```bash
docker compose config --quiet
docker compose down
docker compose up -d --build
docker compose ps
```

## Test Kapsamı

`tests/ExpenseService.Tests` projesi xUnit + Moq kullanır.

Kapsanan alanlar:

- Auth controller
- Expense controller
- Admin user controller
- Notification controller
- FluentValidation kuralları
- Approval threshold domain kuralı
- Notification event handler
- Netgsm SMS sender payload ve hata yönetimi
- Testcontainers tabanlı izole integration testi
- Canlı local health, Mailpit e-posta ve opsiyonel Netgsm SMS testi

## Testcontainers Integration Testi

`TestcontainersIntegrationTests` izole PostgreSQL ve RabbitMQ containerları üzerinde şu akışı doğrular:

1. Expense API ve Notification API erişilebilir mi?
2. Admin, Personnel, HR ve farklı tenant kullanıcısı login olabilir mi?
3. Admin yeni kullanıcı oluşturabilir mi?
4. Admin kullanıcı rollerini güncelleyebilir mi?
5. Yeni kullanıcı güncellenen çoklu rollerle login olabilir mi?
6. Personnel harcama oluşturabilir mi?
7. Farklı tenant kullanıcısı bu harcamayı göremiyor mu?
8. Personnel submit yapabilir mi?
9. HR approve yapabilir mi?
10. Outbox mesajı RabbitMQ’ya publish ediliyor mu?
11. NotificationService event consume edip notification kaydı oluşturuyor mu?

Bu test için local Docker engine çalışır durumda olmalıdır.

## Canlı Local Testler

Canlı local testler varsayılan test paketini kırmamak için ortam değişkeniyle açılır:

```powershell
$env:RUN_LIVE_DELIVERY_TESTS="1"
$env:LIVE_NETGSM_SEND_SMS="0"
dotnet test Izometri.CaseStudy.slnx --no-build --filter "FullyQualifiedName~LiveDeliveryAndHealthTests"
```

Bu komut şu kontrolleri yapar:

- Expense API `/health`
- Notification API `/health`
- RabbitMQ Management API
- Mailpit API
- Expense DB portu
- Notification DB portu
- RabbitMQ AMQP portu
- `Devrimmehmet@gmail.com` adresine `testtir` içerikli Mailpit e-posta gönderimi

Gerçek Netgsm SMS testi ayrı açılır:

```powershell
$env:RUN_LIVE_DELIVERY_TESTS="1"
$env:LIVE_NETGSM_SEND_SMS="1"
$env:LIVE_NETGSM_USERCODE="..."
$env:LIVE_NETGSM_PASSWORD="..."
$env:LIVE_NETGSM_MSGHEADER="..."
dotnet test Izometri.CaseStudy.slnx --no-build --filter "FullyQualifiedName~LiveDeliveryAndHealthTests.Send_test_sms_to_configured_phone"
```

Bu test `5438194976` numarasına `test sms` mesajı göndermeyi dener.

## Son Doğrulama Durumu

Son doğrulamada:

- `dotnet build Izometri.CaseStudy.slnx` geçti.
- `dotnet test Izometri.CaseStudy.slnx --no-build` geçti.
- Normal test sayısı: 34.
- `docker compose down` ile ortam indirildi.
- `docker compose up -d --build` ile ortam yeniden kaldırıldı.
- Expense API, Notification API, PostgreSQL, RabbitMQ, Mailpit ve Jaeger containerları çalışır durumda.
- Canlı health + Mailpit e-posta testleri geçti.
- Netgsm bakiye API’si daha önce başarılı döndü ve hesapta 1000 OTP SMS bakiyesi görüldü.
- Netgsm canlı SMS testi provider’a ulaştı fakat `40 invalidHeader` ile reddedildi. Bu, hesapta aktif gönderici başlığı tanımlı olmadığı anlamına gelir; kod veya endpoint hatası değildir.

## Manuel Kontrol Noktaları

- Expense Swagger: `http://localhost:5001/swagger`
- Notification Swagger: `http://localhost:5002/swagger`
- RabbitMQ UI: `http://localhost:15673`
- Mailpit UI: `http://localhost:8025`
- Jaeger UI: `http://localhost:16686`

RabbitMQ UI giriş bilgisi:

- Kullanıcı adı: `izometri`
- Şifre: `Izometri2026!`
