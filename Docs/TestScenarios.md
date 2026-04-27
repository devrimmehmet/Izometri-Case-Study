# Devrim Mehmet Pattabanoğlu / İzometri Case Study - Test Senaryoları ve Uçtan Uca Doğrulama Planı

Bu belge, İzometri Harcama Yönetim Sistemi için kurgulanan iş kurallarının
(Tenant izolasyonu, yetkilendirme, kur yönetimi, dinamik onay mekanizması ve
bildirim süreçleri) uçtan uca doğrulanması amacıyla hazırlanmış test
senaryolarını içermektedir.

## 1. Kullanıcı Rolleri ve Tenant (Şirket) İzolasyon Testleri

### Senaryo 1.1: Personeller Arası Veri İzolasyonu

- **Önkoşul:** Sisteme "TEST1" şirketinin "Personel 1" (devrimmehmet@msn.com)
  hesabıyla giriş yapılır.
- **Adım 1:** Yeni bir harcama talebi oluşturulur.
- **Adım 2:** Hesaptan çıkış yapılıp aynı şirketin "Personel 2"
  (personel2@test1.com) hesabıyla giriş yapılır.
- **Beklenen Sonuç:** Personel 2, `Harcamalar` listesinde Personel 1'e ait
  hiçbir harcamayı **görememelidir**.

### Senaryo 1.2: HR ve Admin Veri Erişimi

- **Önkoşul:** Sisteme "TEST1" şirketinin "İK" (devrimmehmet@gmail.com) veya
  "Yönetici" (Admin) hesabıyla giriş yapılır.
- **Adım 1:** `Harcamalar` sayfasına gidilir.
- **Beklenen Sonuç:** Hem Personel 1 hem de Personel 2 tarafından oluşturulan
  tüm taslak ve onaya gönderilmiş harcamalar (ilgili status filtrelerine göre)
  listede eksiksiz **görülebilmelidir**.

### Senaryo 1.3: Çapraz Şirket (Cross-Tenant) İzolasyonu

- **Önkoşul:** Sisteme "İzometri" şirketinin Yönetici hesabı
  (admin@izometri.com) ile giriş yapılır.
- **Adım 1:** Harcamalar veya Kullanıcılar sekmesine gidilir.
- **Beklenen Sonuç:** Sadece İzometri şirketine ait veriler dönmelidir.
  Veritabanında TEST1'e ait `5000 USD` gibi talepler olmasına rağmen, İzometri
  admini diğer şirketin hiçbir verisine (kullanıcı veya harcama)
  **erişememelidir**.

---

## 2. Taslak (Draft) Düzenleme ve Silme Testleri

### Senaryo 2.1: Kendi Taslağını Düzenleme

- **Önkoşul:** Personel 1 hesabıyla giriş yapılır.
- **Adım 1:** "Yeni Harcama" oluşturulur, durumu otomatik olarak `Draft`
  (Taslak) olur.
- **Adım 2:** Tablodaki ilgili satırın yanındaki "Düzenle" (Kalem) butonuna
  tıklanır. Tutar ve Açıklama güncellenip kaydedilir.
- **Beklenen Sonuç:** Değişiklikler başarıyla kaydedilmeli, Notification veya
  Event fırlatılmamalı, tablo anında güncellenmelidir.

### Senaryo 2.2: Onaya Gönderilmiş Talebin Kilitlenmesi

- **Önkoşul:** Personel 1 hesabıyla giriş yapılır.
- **Adım 1:** `Draft` statüsündeki bir harcama "Onaya Gönder" butonuyla
  `Pending` statüsüne alınır.
- **Adım 2:** Tablo satırı incelenir.
- **Beklenen Sonuç:** "Düzenle" ve "Sil" butonları kaybolmalıdır. Onaya giden
  işlem **düzenlenememelidir**.

### Senaryo 2.3: HR'ın Personel Taslağını Düzenlemeye Çalışması (Yetki Reddi)

- **Önkoşul:** Personel 1 bir "Taslak" bırakır. Sisteme HR hesabıyla giriş
  yapılır.
- **Adım 1:** HR, listelenen taslaklar arasında "Düzenle" butonunu görmemelidir.
- **Adım 2:** HR, API üzerinden doğrudan `PUT /api/v1/expenses/{id}` isteği
  atarsa;
- **Beklenen Sonuç:** API `403 Forbidden` veya
  `UnauthorizedAccessException ("Only requester can update the expense")` hatası
  dönmelidir.

---

## 3. Kur Ayarları ve TCMB Entegrasyon Testleri

### Senaryo 3.1: TCMB Canlı Kur Üzerinden Limit Hesaplaması (Sabit Kur Yok)

- **Önkoşul:** Admin hesabı ile `/admin/settings` ekranından "Sabit USD Kur"
  alanı **boş** bırakılır.
- **Adım 1:** Personel hesabıyla `150 USD` tutarında bir işlem onaya gönderilir.
- **Adım 2:** (Senaryo Günü USD Kuru ~33 TL ise, `150 * 33 = 4950 TL`)
- **Beklenen Sonuç:** İşlem 5000 TL altında hesaplanacağı için **Sadece HR
  onayına** ihtiyaç duymalıdır. `RequiresAdminApproval` arka planda `false`
  dönmelidir.

### Senaryo 3.2: Sabit Kur Üzerinden Limit Hesaplaması (Admin Kur Sabitleme)

- **Önkoşul:** Admin hesabı ile `Sistem Ayarları` ekranından "Sabit USD Kur"
  `35.00` olarak kaydedilir.
- **Adım 1:** Personel hesabıyla `145 USD` tutarında bir işlem onaya gönderilir.
- **Adım 2:** Arka planda `145 * 35 = 5075 TL` olarak hesaplanır.
- **Beklenen Sonuç:** `5075 TL > 5000 TL` olduğu için bu harcama
  `RequiresAdminApproval = true` olarak işaretlenmelidir.

---

## 4. Onay Akışı (Workflow) ve Bildirim Testleri

### Senaryo 4.1: 5000 TL Altı Harcama (Sadece İK Onayı)

- **Adım 1:** Personel `2000 TRY` değerinde bir seyahat harcaması oluşturup
  onaya gönderir. `expense.created` bildirimi HR ve Admine (veya sadece HR'a)
  gider.
- **Adım 2:** HR hesabıyla girilip ilgili harcama "Onayla" butonuna basılarak
  onaylanır.
- **Beklenen Sonuç:** İşlem durumu anında `Approved` (Onaylandı) olmalıdır.
  İlgili personele "`expense.approved` (Harcama Talebiniz Onaylandı)" maili ve
  bildirimi gitmelidir. Yönetici onayına gerek kalmamalıdır.

### Senaryo 4.2: 5000 TL Üstü Harcama (Sıralı HR -> Admin Onayı)

- **Adım 1:** Personel `6000 TRY` değerinde harcama oluşturur ve onaya gönderir.
- **Adım 2:** HR sisteme girer, harcamayı onaylar. (UI'da `Onayla` yerine
  `Yönetici Onayına Gönder` butonu gözükebilir).
- **Beklenen Sonuç 1:** İşlemin statüsü `Pending` kalmaya devam eder ancak UI'da
  "Yönetici Onayı Bekliyor" (`hrApproved=true`) rozeti görünür.
- **Beklenen Sonuç 2:** Sistemin arka planı `ExpenseRequiresAdminApprovalEvent`
  fırlatır. Notification API bunu yakalayıp **Admin kullanıcısına** "Yönetici
  Onayı Bekleyen Harcama Talebi: 6000 TRY" başlıklı bir e-posta gönderir
  (`emailStatus: Sent`).

### Senaryo 4.3: Yönetici Nihai Onayı

- **Önkoşul:** Senaryo 4.2'deki harcama HR tarafından onaylanmış halde
  beklemektedir.
- **Adım 1:** Admin (Yönetici) sisteme girer. `Yönetici Onayı Bekliyor` etiketli
  işlemleri görür.
- **Adım 2:** İlgili harcamaya "Onayla" tıklar.
- **Beklenen Sonuç:** İşlem `Approved` statüsüne geçer. `adminApproved=true`
  olur. Personele "`expense.approved`" bildirimi gider.

### Senaryo 4.4: Reddedilme (Rejection) Akışı

- **Adım 1:** İster HR ister Admin, `Pending` durumundaki harcamaya "Reddet"
  tıklar ve "Fatura okunamıyor" gibi bir sebep yazar.
- **Beklenen Sonuç:** İşlem statüsü `Rejected` olur. Personele
  `expense.rejected` (Harcama Talebiniz Reddedildi) e-postası ve SMS'i gider.
  İşlem kilitlenir.

---

## 5. İletişim Altyapısı Testleri (Mailpit & RabbitMQ & Jaeger)

### Senaryo 5.1: RabbitMQ Event Doğrulaması

- **Adım 1:** `http://localhost:15672` üzerinden RabbitMQ konsoluna girilir.
- **Adım 2:** Sisteme yeni bir harcama talebi eklenir.
- **Beklenen Sonuç:** `expense.events` Exchange'inden mesajın
  `notification.expense-events` Queue'suna başarıyla route edildiği ve
  Notification API tarafından anında tüketildiği (`Unacked` ve `Ready` = 0)
  görülmelidir.

### Senaryo 5.2: Mailpit Doğrulaması

- **Adım 1:** `http://localhost:8025` adresine girilir.
- **Beklenen Sonuç:** RabbitMQ'dan düşen tüm `expense.created`,
  `expense.requires_admin_approval`, `expense.approved` maillerinin Türkçe
  içeriklerle (ID, Tutar, Mesaj) Inbox'ta sorunsuz oluştuğu doğrulanmalıdır.

### Senaryo 5.3: Jaeger İzlenebilirliği (Distributed Tracing)

- **Adım 1:** `http://localhost:16686` adresinden Jaeger UI'a girilir.
- **Adım 2:** Service olarak `ExpenseService` veya `NotificationService` seçilip
  "Find Traces" tıklanır.
- **Beklenen Sonuç:** Controller katmanından (Create) -> EF Core Veritabanı
  kaydı -> MassTransit Publish -> RabbitMQ -> Notification Consume -> Email Send
  adımlarının tek bir `TraceID` altında kopukluk olmadan (Span olarak)
  izlenebildiği doğrulanmalıdır.
