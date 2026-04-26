# 📧 CASE STUDY: Senior .NET Developer Pozisyonu

---

## İzometri Bilişim
**Pozisyon:** Backend Developer

---

## 📌 Genel Bakış

Değerli Aday,

Teknik yetkinliklerinizi ve mimari tasarım becerilerinizi değerlendirmek için gerçek iş senaryosuna dayalı bir case study hazırladık. **Mikroservis mimarisi**, **multi-tenant SaaS** ve **asenkron iletişim** konularında deneyiminizi görmek istiyoruz.

---

## 🎯 İş Senaryosu

**Senaryo:** Cloud-based SaaS modelinde çalışan farazi bir e-onay platformuna **Kurumsal Harcama Yönetim Sistemi** eklenmesi simüle edilecektir. Platform gerçek değil, teknik yetkinlikleri değerlendirmek için kurgulanmış bir senaryodur.

**İş Akışı:**
1. Personel harcama talebi oluşturur
2. HR departmanı talebi inceler ve onaylar/reddeder
3. Onaylanan talepler için bildirim gönderilir (async)
4. İlgili taraflara sistem üzerinden bildirim iletilir

**Platform Özellikleri:**
- **Multi-Tenant:** Her şirket kendi izole ortamında çalışır
- **Static Roles:** Sistemde önceden tanımlı roller kullanılır (Admin, HR, Personel)
- **Mikroservis:** En az 2 bağımsız API
- **Email Uniqueness:** Email adresi uygulama genelinde değil, tenant bazında tekil olmalıdır

---

## 📋 İŞ GEREKSİNİMLERİ

### BR-1: Multi-Tenant İzolasyon ⭐
- Her tenant (şirket) sadece kendi verilerini görebilir
- Tenant A'nın kullanıcısı Tenant B'nin verilerini hiçbir şekilde görememeli
- Her entity TenantId içermeli, otomatik filtreleme yapılmalı

### BR-2: Rol Tabanlı Yetkilendirme ⭐
Sistemde önceden tanımlanmış statik roller bulunur:

**Admin:**
- Tenant içindeki tüm verilere erişim
- Kullanıcı ve rol yönetimi
- Harcama kayıtlarını görebilir

**HR (İnsan Kaynakları):**
- Harcama taleplerini görüntüleme ve onaylama/reddetme
- Kendi tenant'ındaki tüm harcamaları görebilir

**Personel (Personel):**
- Kendi harcama taleplerini oluşturma
- Sadece kendi taleplerini görüntüleme

**İş Kuralları:**
- Bir kullanıcı birden fazla role sahip olabilir
- Roller sistemde statik olarak tanımlıdır, her tenant aynı rolleri kullanır

### BR-3: Harcama Talebi Oluşturma
- Personel harcama talebi oluşturabilir
- Kategori: Seyahat, Malzeme, Eğitim, Diğer
- Tutar ve para birimi (TRY, USD, EUR)
- Açıklama (min. 20 karakter)
- Durum: Draft → Pending → Approved / Rejected

**Validasyonlar:**
- Kategori zorunlu
- Personnel sadece kendi taleplerini görebilir
- HR ve Admin tüm talepleri görebilir

### BR-4: Onay Süreci
- **5.000 TL ve altı:** Sadece HR onayı
- **5.000 TL üzeri:** HR + Admin onayı (sıralı)
- Red durumunda red nedeni zorunlu (min. 10 karakter)
- Sadece HR ve Admin onaylayabilir

### BR-5: Bildirim Sistemi (Asenkron) 📨
RabbitMQ ile bildirimler gönderilir:
- Talep oluşturuldu → HR'a bildirim
- Talep onaylandı → Personele bildirim
- Talep reddedildi → Personele bildirim

> **Not:** Notification gönderimini mock edebilirsiniz (gerçek email/SMS gönderimi gerekmez).

### BR-6: Sorgulama
- Personnel: Sadece kendi talepleri
- HR/Admin: Tenant'taki tüm talepler
- Filtreleme: Tarih, statü, kategori
- Sayfalama (pagination) desteği

---

## 🔧 TEKNİK GEREKSINIMLER

### ZORUNLU

#### TR-1: Mikroservis Mimarisi ⭐
- **En az 2 bağımsız API:** Örneğin Expense Service + Notification Service
- Her servis kendi database'i (Database per Service pattern)


#### TR-2: Multi-Tenancy ⭐⭐
- TenantId ile veri izolasyonu
- EF Core global query filter (TenantId)
- JWT'den TenantId çözümleme
- **Not:** User - Tenant ilişkisi 1-N'dir (bir user sadece bir tenant'a aittir)

#### TR-3: Soft Delete ⭐
- **Hard delete kullanılmamalıdır**
- Tüm entity'ler soft delete desteklemeli (IsDeleted, DeletedAt, DeletedBy)
- EF Core global query filter ile silinmiş kayıtlar otomatik filtrelenmeli
- Delete işlemleri sadece flag günceller, fiziksel silme yapılmaz
- Audit trail için silinme bilgileri saklanmalı

#### TR-4: Role-Based Authorization ⭐
- JWT token içinde roller (claims: UserId, TenantId, Roles[])
- Endpoint'lerde rol kontrolü

#### TR-5: Onion Architecture
- Entity'ler, DTOs, Services, Validators

#### TR-6: Repository & Unit of Work Pattern ⭐
- Generic Repository
- UnitOfWork ile transaction yönetimi

#### TR-7: Servisler Arası İletişim ⭐⭐

**Asenkron (RabbitMQ):**
- Publisher/Consumer pattern
- Exchange, Queue, Routing configuration
- JSON serialization
- Event-driven architecture
- Events: `ExpenseCreated`, `ExpenseApproved`, `ExpenseRejected`

**Senkron (HTTP):**
- Notification Service → Expense Service: Harcama detayı sorgulama (GET endpoint)
- Retry policy (Polly - opsiyonel)

> **Senaryo:** Notification Service, bildirim gönderirken detaylı bilgi için Expense Service'e HTTP çağrısı yapabilir.

#### TR-8: Validasyon
- Input DTO validasyonları
- Business rule validations

#### TR-9: ORM & Database
- Entity Framework Core
- Code-First + Migrations
- Global query filters (TenantId + IsDeleted)

#### TR-10: Authentication
- JWT Bearer token
- Login endpoint (OAuth2.0 kullanılmaz ise)
- Token içinde UserId, TenantId, Roles claims'leri

---

### BONUS (Artı Puan)

#### TB-1: Outbox Pattern ⭐⭐⭐
- Database transaction ile mesaj gönderiminin atomik olması

#### TB-2: OAuth 2.0 Entegrasyonu ⭐⭐⭐
- Keycloak, Auth0 veya IdentityServer entegrasyonu

#### TB-3: Unit Testing ⭐
- xUnit + Moq

#### TB-4: Docker Support ⭐
- docker-compose.yml (RabbitMQ + DB + APIs)

#### TB-5: API Documentation
- Swagger/OpenAPI

#### TB-6: Logging
- Correlation ID

---

## 📦 Teslim Edilecekler

1. **Git Repository** (public veya invite)
2. **README.md:**
   - Projeyi çalıştırma kılavuzu
   - Test kullanıcıları ve rolleri
   - Topoloji
3. **docker-compose.yml** (Opsiyonel)
4. **EF Migrations**
5. **Seed Data** (örnek tenant, user, role)
