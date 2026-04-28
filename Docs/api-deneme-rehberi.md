# API Deneme Rehberi

Bu rehber, local Docker ortamı açıkken API akışlarını manuel denemek için hazırlanmıştır.

## 1. Servis Adresleri

- Frontend: `http://localhost:3000`
- Expense API: `http://localhost:5001`
- Notification API: `http://localhost:5002`
- Keycloak: `http://localhost:18080`
- Expense Swagger: `http://localhost:5001/swagger`
- Notification Swagger: `http://localhost:5002/swagger`
- Mailpit: `http://localhost:8025`

## 2. Keycloak Token Alma

Docker akışında kullanıcı tokenını API üretmez. Access token Keycloak üzerinden alınır.

```http
POST http://localhost:18080/realms/izometri/protocol/openid-connect/token
Content-Type: application/x-www-form-urlencoded
```

Body:

```text
client_id=expense-service
client_secret=expense-service-client-secret
grant_type=password
username=devrimmehmet@msn.com
password=Pass123!
```

Beklenen token claimleri:

- `aud`: `expense-service`
- `TenantId`: kullanıcının tenant kimliği
- `UserId`: uygulama DB seed kullanıcısı ile aynı kimlik
- `role`: `Admin`, `HR` veya `Personel`

> `POST /api/auth/login` endpointi Docker akışında kapalıdır ve `404 Local login disabled` döner. Kodda sadece geliştirme/test fallback'i olarak durur.

## 3. Personel Harcama Oluşturma

```http
POST http://localhost:5001/api/expenses
Authorization: Bearer {personelToken}
X-Correlation-Id: manual-test-001
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

Beklenen durum:

- Kayıt `Draft` olarak oluşur.
- Tenant bilgisi token içindeki `TenantId` claiminden alınır.
- Personel yalnızca kendi harcamalarını görür.

## 4. Submit

```http
PUT http://localhost:5001/api/expenses/{expenseId}/submit
Authorization: Bearer {personelToken}
X-Correlation-Id: manual-test-001
```

Beklenen durum:

- Expense status `Pending` olur.
- `expense.created` integration event'i outbox tablosuna yazılır.
- Outbox worker RabbitMQ'ya publish eder.
- NotificationService HR/Admin alıcıları için notification kaydı oluşturur.

## 5. HR Onayı

HR kullanıcısı için Keycloak token alın:

```text
username=devrimmehmet@gmail.com
password=Pass123!
```

Onay:

```http
PUT http://localhost:5001/api/expenses/{expenseId}/approve
Authorization: Bearer {hrToken}
X-Correlation-Id: manual-test-001
```

`3500 TRY` için beklenen durum:

- Expense status `Approved` olur.
- `expense.approved` event'i outbox tablosuna yazılır.
- NotificationService personele notification kaydı oluşturur.

## 6. Admin Onayı Gereken Akış

`5000 TRY` üzeri bir harcama oluşturun:

```json
{
  "category": "Equipment",
  "currency": "TRY",
  "amount": 7500,
  "description": "Yeni ekipman satın alma talebi için masraf kaydı"
}
```

Beklenen akış:

1. Personel harcamayı oluşturur.
2. Personel harcamayı submit eder.
3. HR approve eder; kayıt `Pending` kalır, `hrApproved=true`, `requiresAdminApproval=true` olur.
4. Admin approve eder; kayıt `Approved` olur.

## 7. Reject

```http
PUT http://localhost:5001/api/expenses/{expenseId}/reject
Authorization: Bearer {hrOrAdminToken}
Content-Type: application/json
```

```json
{
  "reason": "Belge bilgisi eksik olduğu için reddedildi"
}
```

Beklenen durum:

- Expense status `Rejected` olur.
- Red sebebi kaydedilir.
- `expense.rejected` event'i publish edilir.
- NotificationService personele notification kaydı oluşturur.

## 8. Admin Kullanıcı Yönetimi

Admin kullanıcısı için Keycloak token alın:

```text
username=pattabanoglu@devrimmehmet.com
password=Pass123!
```

Kullanıcı listeleme:

```http
GET http://localhost:5001/api/admin/users
Authorization: Bearer {adminToken}
```

Kullanıcı oluşturma:

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

Not: Bu endpointler uygulama DB'sindeki tenant kullanıcılarını yönetir. Keycloak kullanıcı seed'i demo başlangıç verisi olarak import edilir.

## 9. Tenant İzolasyonu Denemesi

1. `test1` tenantındaki Personel kullanıcısı ile harcama oluşturun.
2. `test2` tenant kullanıcısı (`personel@test2.com`) ile token alın.
3. `GET /api/expenses/{test1ExpenseId}` çağırın.

Beklenen sonuç:

- `404 Not Found`
- Diğer tenant verisi görüntülenmez.

## 10. Notification Kontrolü

```http
GET http://localhost:5002/api/notifications
Authorization: Bearer {anyValidToken}
```

Tenant filtresi opsiyoneldir. Gönderilirse token tenantı ile aynı olmalıdır:

```http
GET http://localhost:5002/api/notifications?tenantId={tenantId}
Authorization: Bearer {anyValidToken}
```

Yanıtta e-posta kontrolü için şu alanlara bakılır:

- `recipientEmail`: Gönderim denenmiş e-posta adresleri.
- `emailStatus`: `Sent`, `Failed` veya `Skipped`.
- `emailError`: SMTP hatası varsa kısa hata açıklaması.

`test1` tenantında Personel harcama oluşturduğunda `expense.created` bildirimi için beklenen alıcılar:

```text
pattabanoglu@devrimmehmet.com,devrimmehmet@gmail.com
```

Docker local ortamında gönderilen e-postalar Mailpit arayüzünden kontrol edilir:

```text
http://localhost:8025
```
