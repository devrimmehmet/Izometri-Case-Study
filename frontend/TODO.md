# Frontend TODO — İzometri Case Study

## ✅ Tamamlananlar

- [x] Quasar Vue 3 projesi oluşturuldu (Pinia, i18n, SCSS, ESLint, Prettier)
- [x] Axios HTTP client kuruldu
- [x] Gate/Lock ekranı — `D19M23P` parolası ile giriş
- [x] JWT Auth — Login sayfası (tenant + email + password)
- [x] Auth store (Pinia) — token, user, roles yönetimi
- [x] API service katmanı — axios interceptor, baseURL, token injection
- [x] Expense store — CRUD, submit, approve, reject
- [x] Gösterge Paneli — özet kartlar, son harcamalar
- [x] Harcama listesi — filtreleme, pagination, durum renkleri
- [x] Harcama detay — tam bilgi görüntüleme
- [x] Yeni harcama — form, validasyon, kategori/para birimi seçimi
- [x] Onay/Red akışı — İK/Yönetici approve/reject butonları
- [x] Kullanıcı yönetimi — Yönetici: kullanıcı listesi, oluşturma, rol güncelleme
- [x] Bildirimler — Bildirim Servisi API'den çekme
- [x] Dokümantasyon hub — tüm local servis linkleri
- [x] MainLayout — sidebar, header, role-based navigation
- [x] Route guard — auth + gate koruması
- [x] Dark mode premium tema
- [x] Quasar Notify plugin entegrasyonu
- [x] Dev server proxy — CORS çözümü
- [x] Frontend tamamen Türkçe
- [x] Enum'lar Türkçe (Kategori, Durum, Roller)
- [x] Merkezi çeviri modülü (utils/tr.ts)
- [x] 3 Tenant: İzometri, TEST1, TEST2
- [x] Her tenant'ta Admin/İK/Personel seed kullanıcıları
- [x] EF Migration: UpdateSeedThreeTenants

## 🏗️ Mimari Kararlar

- **Gate Ekranı**: Dışarıdan bakıldığında sıradan bir sayfa. `D19M23P` parolası ile açılır, `sessionStorage` ile saklanır.
- **Auth Flow**: Login → JWT token → Pinia store → axios interceptor → tüm API çağrıları otomatik auth
- **Multi-Tenant**: Login'de tenantCode seçimi, token'dan tenant bilgisi okunur
- **Role-Based UI**: Yönetici/İK/Personel rollerine göre menü ve butonlar koşullu gösterilir
- **Proxy**: Quasar dev server üzerinden `/api` ve `/notify-api` proxy ile CORS sorunsuz backend bağlantısı
- **Türkçeleştirme**: Merkezi `utils/tr.ts` dosyası tüm enum/status/role çevirilerini yönetir

## 🏢 Seed Verileri (Tüm şifreler: Pass123!)

### İzometri
| Rol | E-posta |
|-----|---------|
| Yönetici | admin@izometri.com |
| İK | hr@izometri.com |
| Personel | personel@izometri.com |

### TEST1 (Test ortamı)
| Rol | E-posta |
|-----|---------|
| Yönetici | pattabanoglu@devrimmehmet.com |
| İK | devrimmehmet@gmail.com |
| Personel | devrimmehmet@msn.com |

### TEST2
| Rol | E-posta |
|-----|---------|
| Yönetici | admin@test2.com |
| İK | hr@test2.com |
| Personel | personel@test2.com |
