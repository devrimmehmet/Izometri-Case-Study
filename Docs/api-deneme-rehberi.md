# API Deneme Rehberi

Bu rehber local Docker ortamı açıkken API akışlarını manuel denemek için hazırlanmıştır.

## 1. Servis Adresleri

- Expense API: `http://localhost:5001`
- Notification API: `http://localhost:5002`
- Expense Swagger: `http://localhost:5001/swagger`
- Notification Swagger: `http://localhost:5002/swagger`

## 2. Login

```http
POST http://localhost:5001/api/auth/login
Content-Type: application/json
```

Personnel girişi (`test1` tenant):

```json
{
  "email": "devrimmehmet@msn.com",
  "password": "Pass123!",
  "tenantCode": "test1"
}
```

Dönen `accessToken` sonraki isteklerde kullanılır.

## 3. Harcama Oluşturma

```http
POST http://localhost:5001/api/expenses
Authorization: Bearer {personnelToken}
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
- `ExpenseCreatedEvent` outbox tablosuna yazılır.
- Outbox worker RabbitMQ'ya publish eder.
- NotificationService HR için mock notification kaydı oluşturur.

## 4. Submit

```http
PUT http://localhost:5001/api/expenses/{expenseId}/submit
Authorization: Bearer {personnelToken}
X-Correlation-Id: manual-test-001
```

Beklenen durum:

- Expense status `Pending` olur.

## 5. HR Onayı

HR login (`test1` tenant):

```json
{
  "email": "devrimmehmet@gmail.com",
  "password": "Pass123!",
  "tenantCode": "test1"
}
```

Onay:

```http
PUT http://localhost:5001/api/expenses/{expenseId}/approve
Authorization: Bearer {hrToken}
X-Correlation-Id: manual-test-001
```

`3500 TRY` için beklenen durum:

- Expense status `Approved` olur.
- `ExpenseApprovedEvent` outbox tablosuna yazılır.
- NotificationService Personnel için notification kaydı oluşturur.

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

1. Personnel create eder.
2. Personnel submit eder.
3. HR approve eder; kayıt hâlâ `Pending` kalır (`hrApproved: true`, `requiresAdminApproval: true`).
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
- `ExpenseRejectedEvent` publish edilir.
- NotificationService Personnel için notification kaydı oluşturur.

## 8. Admin Kullanıcı Yönetimi

Admin login (`test1` tenant):

```json
{
  "email": "pattabanoglu@devrimmehmet.com",
  "password": "Pass123!",
  "tenantCode": "test1"
}
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
  "roles": ["Personnel"]
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
  "roles": ["Personnel", "HR"]
}
```

## 9. Tenant İzolasyonu Denemesi

1. `test1` tenant ile harcama oluşturun.
2. `test2` tenant kullanıcısı (`personel@test2.com`) ile login olun.
3. `GET /api/expenses/{test1ExpenseId}` çağırın.

Beklenen sonuç:

- `404 Not Found`
- Veri başka tenant tarafından görüntülenmez.

## 10. Notification Kontrolü

`GET /api/notifications` artık JWT Bearer gerektirir:

```http
GET http://localhost:5002/api/notifications
Authorization: Bearer {anyValidToken}
```

Tenant filtresi:

```http
GET http://localhost:5002/api/notifications?tenantId={tenantId}
Authorization: Bearer {anyValidToken}
```

Yanıtta e-posta kontrolü için şu alanlara bakılır:

- `recipientEmail`: Gönderim denenmiş e-posta adresleri.
- `emailStatus`: `Sent`, `Failed` veya `Skipped`.
- `emailError`: SMTP hatası varsa kısa hata açıklaması.

`test1` tenantında Personnel harcama oluşturduğunda `expense.created` bildirimi için beklenen alıcılar:

```text
pattabanoglu@devrimmehmet.com,devrimmehmet@gmail.com
```

Docker local ortamında gönderilen e-postalar Mailpit arayüzünden kontrol edilir:

```text
http://localhost:8025
```
