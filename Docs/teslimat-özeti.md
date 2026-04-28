# 📦 Izometri Senior .NET Case Study - Teslimat Özeti

Bu belge, **Senior .NET Developer** pozisyonu için hazırlanan case study çalışmasının teknik teslimat özetidir. Tüm temel gereksinimler (TR-1'den TR-10'a kadar) ve kritik bonus maddeleri (TB-1'den TB-6'ya kadar) eksiksiz tamamlanmıştır.

## 🚀 Proje Durumu
*   **Solution:** `Izometri.CaseStudy.slnx`
*   **Teknoloji Stack:** .NET 10, PostgreSQL, RabbitMQ, Keycloak, OpenTelemetry, Vue.js/Quasar.
*   **Docker:** `docker compose up --build` ile tüm sistem (API'ler, DB'ler, Keycloak, UI, Jaeger, Mailpit) hazır hale gelir.
*   **Testler:** 42 Unit Test ve 6 Kapsamlı Entegrasyon Testi (Keycloak token doğrulamalı) başarıyla tamamlandı.

## 📋 Tamamlanan Temel Gereksinimler (TR)
- [x] **Multi-Tenant Mimari:** JWT claim bazlı izolasyon ve EF Core global query filters.
- [x] **Rol Modeli:** Admin, HR ve Personel rolleri Keycloak üzerinden yönetilmektedir.
- [x] **Expense Service:** Harcama oluşturma, onay/red akışı, döviz çevrimi (tenant-specific).
- [x] **Notification Service:** Asenkron bildirim gönderimi (RabbitMQ) ve HTTP üzerinden Expense detay sorgulama.
- [x] **Kademeli Onay:** 5.000 TRY altı/üstü için farklı onay yolları (HR -> Admin).
- [x] **Resilience:** Servisler arası iletişimde Polly (Retry/Circuit Breaker) entegrasyonu.

## 🌟 Tamamlanan Bonus Özellikler (TB)
- [x] **Outbox Pattern (TB-1):** Veritabanı transaction'ı ile atomik mesaj yayını (Quartz.NET worker ile).
- [x] **Keycloak OAuth 2.0 (TB-2):** Tam entegrasyon. Kullanıcı senkronizasyonu (Admin API üzerinden) yapılmıştır.
- [x] **Unit Testing (TB-3):** xUnit + Moq ile %90+ iş mantığı kapsamı.
- [x] **Docker Support (TB-4):** Tüm altyapı konteynerize edilmiştir.
- [x] **Swagger/OpenAPI (TB-5):** Her iki servis için de OpenAPI 3.0 dokümantasyonu.
- [x] **Logging & Tracing (TB-6):** Serilog, Correlation ID ve OpenTelemetry (Jaeger) entegrasyonu.

## 🛠️ Ekstra Operasyonel Olgunluklar
- **OpenAPI Drift Control:** CI pipeline'da otomatik dokümantasyon üretimi ve versiyon kontrolü.
- **Tenant-Aware Query Filter:** Defense-in-depth yaklaşımıyla veritabanı seviyesinde izolasyon.
- **Retention Worker:** Eski outbox mesajları ve bildirim logları için otomatik temizleme görevi.
- **Security Headers:** Nginx seviyesinde HSTS, CSP ve Rate Limiting yapılandırması.

## 📂 Dokümantasyon Dizini
*   [Çalıştırma Rehberi](../README.md)
*   [Mimari Topoloji](mimari-topoloji.md)
*   [API Deneme Rehberi](api-deneme-rehberi.md)
*   [Keycloak Kurulumu](keycloak-import-rehberi.md)
*   [Gereksinim Uyumluluk Matrisi](gereksinim-uyumluluk-matrisi.md)

---
**Hazırlayan:** Devrim Mehmet Pattabanoğlu
**Tarih:** 28 Nisan 2026
