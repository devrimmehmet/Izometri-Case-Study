# Çalıştırma ve Ortam Rehberi

Bu doküman projenin local geliştirme ortamında nasıl çalıştırılacağını, nasıl deneneceğini ve prod benzeri ortamda hangi ayarlarla hazırlanacağını anlatır.

## 1. Ön Koşullar

Local geliştirme için gerekli araçlar:

- .NET 10 SDK
- Docker Desktop
- Git
- İsteğe bağlı: PostgreSQL istemcisi, RabbitMQ Management UI

Portlar:

- Expense API: `5001`
- Notification API: `5002`
- RabbitMQ AMQP: `5673`
- RabbitMQ Management: `15673`
- Expense DB: `15433`
- Notification DB: `15434`
- Keycloak: `18080`
- Mailpit UI: `8025`
- Mailpit SMTP: `1025`

Bu portlar mevcut local servislerle çakışmayı azaltmak için varsayılan Docker portlarından farklı seçilmiştir.

## 2. Local Başlatma

Repository kök dizininde çalıştırın:

```bash
dotnet build Izometri.CaseStudy.slnx
dotnet test Izometri.CaseStudy.slnx
docker compose up -d --build
```

Servisleri kontrol edin:

```bash
docker compose ps
```

Swagger adresleri:

- Expense API: `http://localhost:5001/swagger`
- Notification API: `http://localhost:5002/swagger`
- RabbitMQ UI: `http://localhost:15673`
- Mailpit UI: `http://localhost:8025`

RabbitMQ kullanıcı bilgileri:

- Kullanıcı adı: `izometri`
- Şifre: `Izometri2026!`

API containerları açılışta migrationları otomatik uygular. Bu nedenle localde ayrıca `database update` komutu çalıştırmak zorunlu değildir.

## 3. Local Smoke Test

Önce Personel kullanıcısı ile login olun:

```http
POST http://localhost:5001/api/auth/login
Content-Type: application/json
```

```json
{
  "email": "devrimmehmet@msn.com",
  "password": "Pass123!",
  "tenantCode": "test1"
}
```

Token ile harcama oluşturun:

```http
POST http://localhost:5001/api/expenses
Authorization: Bearer {PersonelToken}
X-Correlation-Id: local-smoke-001
Content-Type: application/json
```

```json
{
  "category": "Travel",
  "currency": "TRY",
  "amount": 3500,
  "description": "İstanbul müşteri toplantısı için ulaşım gideri"
}
```

Harcama kaydını submit edin:

```http
PUT http://localhost:5001/api/expenses/{expenseId}/submit
Authorization: Bearer {PersonelToken}
X-Correlation-Id: local-smoke-001
```

HR kullanıcısı ile login olun:

```json
{
  "email": "devrimmehmet@gmail.com",
  "password": "Pass123!",
  "tenantCode": "test1"
}
```

HR token ile onaylayın:

```http
PUT http://localhost:5001/api/expenses/{expenseId}/approve
Authorization: Bearer {hrToken}
X-Correlation-Id: local-smoke-001
```

Bildirimleri kontrol edin:

```http
GET http://localhost:5002/api/notifications
```

Beklenen sonuç:

- Expense status `Approved` olur.
- ExpenseService outbox mesajı RabbitMQ'ya publish eder.
- NotificationService `expense.approved` eventini consume eder.
- Notification DB içinde ilgili bildirim kaydı oluşur.

## 4. Admin Kullanıcı ve Rol Yönetimi

Admin kullanıcısı ile login olun:

```json
{
  "email": "pattabanoglu@devrimmehmet.com",
  "password": "Pass123!",
  "tenantCode": "test1"
}
```

Tenant kullanıcılarını listeleme:

```http
GET http://localhost:5001/api/admin/users
Authorization: Bearer {adminToken}
```

Yeni kullanıcı oluşturma:

```http
POST http://localhost:5001/api/admin/users
Authorization: Bearer {adminToken}
Content-Type: application/json
```

```json
{
  "email": "new.user@test1.com",
  "displayName": "New User",
  "password": "Pass123!",
  "roles": ["Personel"]
}
```

Rol güncelleme:

```http
PUT http://localhost:5001/api/admin/users/{userId}/roles
Authorization: Bearer {adminToken}
Content-Type: application/json
```

```json
{
  "roles": ["Personel", "HR"]
}
```

Bu endpointler sadece Admin rolü ile kullanılabilir ve yalnızca token içindeki tenant üzerinde işlem yapar.

## 5. Test Komutları

Tüm unit ve canlı Docker entegrasyon testlerini çalıştırma:

```bash
dotnet test Izometri.CaseStudy.slnx
```

Test kapsamı:

- Auth controller
- Expense controller
- Admin user controller
- Notification controller
- Validator kuralları
- Approval threshold kuralları
- Notification event handler
- Canlı Docker akışı: API + PostgreSQL + RabbitMQ + Notification consumer

Canlı entegrasyon testi için `docker compose up -d --build` ile servislerin açık olması gerekir.

## 6. OAuth2 / Keycloak Modu

Docker akışında kullanıcı authentication kaynağı Keycloak'tur. API'ler kullanıcı tokenı üretmez; JWT Bearer access token doğrular. Local JWT login endpointi sadece geliştirme/test fallback'i olarak kodda durur ve Docker'da kapalıdır.

Tum sistemi baslatma:

```bash
docker compose up -d --build
```

Keycloak UI:

- Adres: `http://localhost:18080`
- Kullanıcı adı: `admin`
- Şifre: `admin`

Expense API ve Notification API'nin dış IdP tokenlarını doğrulaması için prod veya local override ortam değişkenleri:

```bash
Jwt__Authority=http://keycloak:8080/realms/izometri
Jwt__PublicAuthority=http://localhost:18080/realms/izometri
Jwt__Audience=expense-service
Jwt__RequireHttpsMetadata=false
Authentication__EnableLocalLogin=false
```

`Jwt__Authority` container icinden metadata almak, `Jwt__PublicAuthority` hosttan alinan token issuer degerini kabul etmek icindir. Keycloak import dosyası Expense DB seed kullanıcılarının tamamını aynı `UserId`, `TenantId`, email ve rol değerleriyle içerir. Token içinde `UserId`, `TenantId`, `role` ve `aud=expense-service` claimleri bulunur.

## 7. Prod Benzeri Çalıştırma

Prod ortamında aşağıdaki ayarlar environment variable veya secret manager üzerinden verilmelidir:

```bash
ConnectionStrings__ExpenseDb=Host=expense-db;Port=5432;Database=expense_db;Username=...;Password=...
ConnectionStrings__NotificationDb=Host=notification-db;Port=5432;Database=notification_db;Username=...;Password=...
RabbitMq__HostName=rabbitmq
RabbitMq__Port=5672
RabbitMq__UserName=...
RabbitMq__Password=...
Jwt__Issuer=...
Jwt__Audience=...
Jwt__Secret=...
ExpenseService__BaseUrl=http://expense-api:8080
Mail__FromName=Izometri Expense
Mail__FromEmail=...
Mail__Host=mail.devrimmehmet.com
Mail__Port=587
Mail__UserName=...
Mail__Password=...
Mail__UseSsl=true
Mail__UsePickupFolder=false
Mail__IgnoreCertificateErrors=true
Mail__PickupFolderPath=App_Data/mail-drop
Netgsm__UserCode=...
Netgsm__Password=...
Netgsm__MsgHeader=...
Netgsm__BaseUrl=https://api.netgsm.com.tr
Netgsm__Encoding=TR
Netgsm__AppName=
Netgsm__UseOtpEndpoint=false
```

Prod önerileri:

- `Jwt__Secret` kesinlikle repository içinde tutulmamalıdır.
- PostgreSQL ve RabbitMQ kullanıcı/şifreleri secret olarak yönetilmelidir.
- API containerları arkasına reverse proxy veya ingress konulmalıdır.
- HTTPS dış katmanda zorunlu olmalıdır.
- SMTP için üretimde geçerli TLS sertifikası kullanılmalı, mümkünse `Mail__IgnoreCertificateErrors=false` kalmalıdır.
- Netgsm SMS gönderimi REST v2 JSON POST ile yapılır. `Netgsm__MsgHeader` Netgsm hesabında tanımlı aktif gönderici başlığı olmalıdır; başlık tanımlı değilse canlı API `40 invalidHeader/header problem` döndürür.
- Migration uygulama stratejisi ekip politikasına göre otomatik startup migration veya ayrı deployment job olarak belirlenmelidir.
- RabbitMQ ve PostgreSQL volume yedekleme stratejisi tanımlanmalıdır.
- Loglar merkezi bir log sistemine yönlendirilmelidir.
- Local Docker ortamındaki Mailpit sadece geliştirme içindir; prod ortamında gerçek SMTP host ve secret değerleri verilmelidir.

## 8. Durdurma ve Temizlik

Containerları durdurma:

```bash
docker compose down
```

Volume dahil tamamen temizleme:

```bash
docker compose down -v
```

`down -v` komutu veritabanı verilerini siler.
