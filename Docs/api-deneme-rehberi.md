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

```json
{
  "email": "personel@demo.com",
  "password": "Pass123!",
  "tenantCode": "acme"
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

HR login:

```json
{
  "email": "devrimmehmet@msn.com",
  "password": "Pass123!",
  "tenantCode": "acme"
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
3. HR approve eder; kayıt hâlâ `Pending` kalır.
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

Admin login:

```json
{
  "email": "devrimmehmet@gmail.com",
  "password": "Pass123!",
  "tenantCode": "acme"
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
  "email": "new.user@acme.com",
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

1. `acme` tenant ile harcama oluşturun.
2. `globex` tenant kullanıcısı ile login olun.
3. `GET /api/expenses/{acmeExpenseId}` çağırın.

Beklenen sonuç:

- `404 Not Found`
- Veri başka tenant tarafından görüntülenmez.

## 10. Notification Kontrolü

```http
GET http://localhost:5002/api/notifications
```

Tenant filtresi:

```http
GET http://localhost:5002/api/notifications?tenantId={tenantId}
```

Yanıtta e-posta kontrolü için şu alanlara bakılır:

- `recipientEmail`: Gönderim denenmiş e-posta adresleri.
- `emailStatus`: `Sent`, `Failed` veya `Skipped`.
- `emailError`: SMTP hatası varsa kısa hata açıklaması.

Personel Acme tenantında harcama oluşturduğunda `expense.created` bildirimi için beklenen alıcılar:

```text
devrimmehmet@gmail.com,devrimmehmet@msn.com
```

Docker local ortamında gönderilen e-postalar Mailpit arayüzünden kontrol edilir:

```text
http://localhost:8025
```
