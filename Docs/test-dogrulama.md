# Test ve Doğrulama

## Komutlar

Build:

```bash
dotnet build Izometri.CaseStudy.slnx
```

Test:

```bash
dotnet test Izometri.CaseStudy.slnx
```

Docker Compose doğrulama:

```bash
docker compose config
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
- Canlı Docker integration testi

## Canlı Docker Integration Testi

`LiveDockerIntegrationTests` şu akışı gerçek servisler üzerinde doğrular:

1. Expense API ve Notification API erişilebilir mi?
2. Admin, Personnel, HR ve farklı tenant kullanıcısı login olabilir mi?
3. Admin yeni kullanıcı oluşturabilir mi?
4. Admin kullanıcı rollerini güncelleyebilir mi?
5. Yeni kullanıcı güncellenen çoklu rollerle login olabilir mi?
6. Personnel harcama oluşturabilir mi?
7. Farklı tenant kullanıcısı bu harcamayı göremiyor mu?
8. Personnel submit yapabilir mi?
9. HR approve yapabilir mi?
10. Outbox mesajı RabbitMQ'ya publish ediliyor mu?
11. NotificationService event consume edip notification kaydı oluşturuyor mu?

Bu test için servislerin çalışır durumda olması gerekir:

```bash
docker compose up -d --build
dotnet test Izometri.CaseStudy.slnx
```

## Son Doğrulama Durumu

Son doğrulamada:

- Build geçti.
- Testler geçti.
- Toplam test sayısı: 23.
- Docker Compose config geçerli.
- API, PostgreSQL ve RabbitMQ containerları çalışır durumda.

## Manuel Kontrol Noktaları

- Expense Swagger: `http://localhost:5001/swagger`
- Notification Swagger: `http://localhost:5002/swagger`
- RabbitMQ UI: `http://localhost:15673`

RabbitMQ UI giriş bilgisi:

- Kullanıcı adı: `guest`
- Şifre: `guest`
