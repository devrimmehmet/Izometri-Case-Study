# Test ve Dogrulama

## Komutlar

Build:

```bash
dotnet build Izometri.CaseStudy.slnx
```

Normal test paketi:

```bash
dotnet test Izometri.CaseStudy.slnx
```

Frontend build:

```bash
cd frontend
npm run build
```

Docker Compose dogrulama:

```bash
docker compose config --quiet
docker compose up -d --build
docker compose ps
```

## Test Kapsami

`tests/ExpenseService.Tests` projesi xUnit + Moq kullanir.

Kapsanan alanlar:

- Auth controller
- Local login disabled fallback
- Expense controller
- Admin user controller
- Notification controller
- Notification tenant isolation negatif senaryolari
- FluentValidation kurallari
- Approval threshold domain kurali
- JWT token claim contract
- Notification event handler
- Netgsm SMS sender payload ve hata yonetimi
- Testcontainers tabanli izole integration testi
- Canli local health, Mailpit e-posta ve opsiyonel Netgsm SMS testi

## Testcontainers Integration Testi

`TestcontainersIntegrationTests` izole PostgreSQL ve RabbitMQ containerlari uzerinde su akisi dogrular:

1. Expense API ve Notification API erisilebilir mi?
2. Admin, Personnel, HR ve farkli tenant kullanicisi login olabilir mi?
3. Admin yeni kullanici olusturabilir mi?
4. Admin kullanici rollerini guncelleyebilir mi?
5. Yeni kullanici guncellenen coklu rollerle login olabilir mi?
6. Personnel harcama olusturabilir mi?
7. Farkli tenant kullanicisi bu harcamayi goremiyor mu?
8. Personnel submit yapabilir mi?
9. HR approve yapabilir mi?
10. Outbox mesaji RabbitMQ'ya publish ediliyor mu?
11. NotificationService event consume edip notification kaydi olusturuyor mu?

Bu test icin local Docker engine calisir durumda olmalidir.

## Canli Local Testler

Canli local testler varsayilan test paketini kirmamak icin ortam degiskeniyle acilir:

```powershell
$env:RUN_LIVE_DELIVERY_TESTS="1"
$env:LIVE_NETGSM_SEND_SMS="0"
dotnet test Izometri.CaseStudy.slnx --no-build --filter "FullyQualifiedName~LiveDeliveryAndHealthTests"
```

Bu komut su kontrolleri yapar:

- Expense API `/health`
- Notification API `/health`
- RabbitMQ Management API
- Mailpit API
- Expense DB portu
- Notification DB portu
- RabbitMQ AMQP portu
- `Devrimmehmet@gmail.com` adresine `testtir` icerikli Mailpit e-posta gonderimi

Gercek Netgsm SMS testi ayri acilir:

```powershell
$env:RUN_LIVE_DELIVERY_TESTS="1"
$env:LIVE_NETGSM_SEND_SMS="1"
$env:LIVE_NETGSM_USERCODE="..."
$env:LIVE_NETGSM_PASSWORD="..."
$env:LIVE_NETGSM_MSGHEADER="..."
dotnet test Izometri.CaseStudy.slnx --no-build --filter "FullyQualifiedName~LiveDeliveryAndHealthTests.Send_test_sms_to_configured_phone"
```

Bu test `5438194976` numarasina `test sms` mesaji gondermeyi dener.

## Son Dogrulama Durumu

Son dogrulamada:

- `dotnet build Izometri.CaseStudy.slnx` gecti; 0 warning, 0 error.
- `dotnet test Izometri.CaseStudy.slnx --no-build` gecti.
- Normal test sayisi: 41.
- `npm run build` gecti; frontend build warning uretmedi.
- `docker compose config --quiet` gecti.
- `docker compose up -d --build` ile Keycloak dahil tum sistem otomatik kalkti.
- Expense API, Notification API, PostgreSQL, RabbitMQ, Keycloak, Mailpit, Jaeger ve frontend containerlari calisir durumda.
- Docker akışında `/api/auth/login` local fallback'i kapalıdır.
- Hosttan alinan Keycloak OAuth2 access tokeni ile Expense API sorgusu basarili dondu.
- Canli health + Mailpit e-posta testleri daha once gecti.
- Netgsm bakiye API'si daha once basarili dondu ve hesapta 1000 OTP SMS bakiyesi goruldu.
- Netgsm canli SMS testi provider'a ulasti fakat `40 invalidHeader` ile reddedildi. Bu, hesapta aktif gonderici basligi tanimli olmadigi anlamina gelir; kod veya endpoint hatasi degildir.

## Manuel Kontrol Noktalari

- Frontend: `http://localhost:3000`
- Expense Swagger: `http://localhost:5001/swagger`
- Notification Swagger: `http://localhost:5002/swagger`
- RabbitMQ UI: `http://localhost:15673`
- Keycloak UI: `http://localhost:18080`
- Mailpit UI: `http://localhost:8025`
- Jaeger UI: `http://localhost:16686`

RabbitMQ UI giris bilgisi:

- Kullanici adi: `izometri`
- Sifre: `Izometri2026!`
