# İzometri Case Study Frontend

Bu frontend, backend case'in zorunlu parçası değil; mikroservis akışını demo sırasında görünür ve test edilebilir hale getiren ek bir arayüzdür. Backend'e ait iş kuralları değiştirilmeden Personel, HR ve Admin rollerinin harcama onay süreci, bildirim üretimi, Mailpit e-postaları ve Jaeger trace akışı tek yerden izlenebilir.

## Amaç

- Backend case'in uçtan uca davranışını ürün benzeri bir arayüzle göstermek.
- Rol bazlı aksiyonların backend kurallarıyla uyumlu çalıştığını doğrulamak.
- `X-Correlation-Id`, NotificationService, Mailpit ve Jaeger görünürlüğünü demo akışına taşımak.
- Admin operasyonları için dead-letter ve probe email kontrollerini arayüzden erişilebilir yapmak.

## Teknoloji

- Vue 3
- Quasar 2
- Pinia
- Axios
- TypeScript
- PWA build
- Nginx reverse proxy

## Çalıştırma

Backend servisleriyle birlikte tam demo:

```bash
docker compose up -d --build
```

Frontend:

```text
http://localhost:3000
```

Lokal geliştirme:

```bash
cd frontend
npm install
npm run dev
```

Build doğrulama:

```bash
cd frontend
npm run build
```

## Demo Kullanıcıları

| Rol | Tenant | E-posta | Şifre |
| --- | --- | --- | --- |
| Personel | `test1` | `devrimmehmet@msn.com` | `Pass123!` |
| HR | `test1` | `devrimmehmet@gmail.com` | `Pass123!` |
| Admin | `test1` | `pattabanoglu@devrimmehmet.com` | `Pass123!` |

## Demo Akışı

1. Personel kullanıcısıyla giriş yap.
2. Yeni harcama oluştur. Açıklama en az 20 karakter olmalıdır.
3. Harcamayı onaya gönder.
4. HR kullanıcısıyla giriş yap.
5. Pending harcamayı onayla veya en az 10 karakterlik red nedeni ile reddet.
6. 5.000 TL üstü harcamalarda Admin onayı gerektiğini göster.
7. Bildirimler ekranında event kaydını, e-posta durumunu ve correlation id değerini kontrol et.
8. Mailpit üzerinde e-postayı doğrula: `http://localhost:8025`.
9. Jaeger üzerinde trace akışını doğrula: `http://localhost:16686`.

## Backend Endpointleri

| Alan | Method | Endpoint |
| --- | --- | --- |
| Auth | POST | `/api/auth/login` |
| Harcamalar | GET | `/api/expenses` |
| Harcama detayı | GET | `/api/expenses/{id}` |
| Harcama oluşturma | POST | `/api/expenses` |
| Harcama güncelleme | PUT | `/api/expenses/{id}` |
| Submit | PUT | `/api/expenses/{id}/submit` |
| Onay | PUT | `/api/expenses/{id}/approve` |
| Red | PUT | `/api/expenses/{id}/reject` |
| Silme | DELETE | `/api/expenses/{id}` |
| Kullanıcılar | GET/POST | `/api/admin/users` |
| Rol güncelleme | PUT | `/api/admin/users/{id}/roles` |
| Döviz kurları | GET/PUT | `/api/settings/exchange-rates` |
| Outbox dead-letter | GET | `/api/admin/outbox/dead-letters` |
| Bildirimler | GET | `/notify-api/notifications` |
| Notification dead-letter | GET | `/notify-api/admin/notifications/dead-letters` |
| Probe email | POST | `/notify-api/admin/notifications/probe-email` |

## Görünürlük

- Her frontend isteği için `X-Correlation-Id` üretilir.
- API response header'ındaki correlation id yakalanır.
- Hata toast'larında correlation id kopyalanabilir şekilde gösterilir.
- Bildirim ve admin operasyon tablolarında uzun id değerleri kopyalama butonuyla gösterilir.
- Mailpit ve Jaeger bağlantıları demo akışında açıkça doğrulanır.

## Son Smoke Sonucu

2026-04-30 tarihinde doğrulandı:

- `npm run build` başarılı.
- `docker compose up -d --build` başarılı.
- Frontend `http://localhost:3000` üzerinden HTTP 200 döndü.
- Personel ile harcama oluşturuldu ve submit edildi.
- HR ile harcama onaylandı.
- Notification ekranında event kaydı görüldü.
- Mailpit üzerinde e-posta kaydı görüldü.
- Jaeger üzerinde `expense-service` için trace kaydı döndü.

Kanıt değerleri:

- Expense ID: `3651bbec-c811-49b4-81bd-10b0ce93cd68`
- Notification Event ID: `02318e2a-a398-4b9a-85b8-b2ba409dfff6`
- Correlation ID: `1d783cf0-36a6-4636-a0fd-c7a3daf96fcc`
- Mailpit son konu: `Yönetici Onayı Bekleyen Harcama Talebi: 3651bbec-c811-49b4-81bd-10b0ce93cd68`
- Jaeger servisleri: `expense-service`, `notification-service`, `jaeger-all-in-one`
