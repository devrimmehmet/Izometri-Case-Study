# 💎 Izometri Harcama Yönetim Sistemi (EMS)

![CI](https://github.com/devrimmehmet/Izometri-Case-Study/actions/workflows/ci.yml/badge.svg)
![Status](https://img.shields.io/badge/Status-Production--Ready-brightgreen)
![Version](https://img.shields.io/badge/.NET-10.0-blue)

**Izometri EMS**, modern kurumsal ihtiyaçlar için tasarlanmış, **Multi-Tenant SaaS** mimarisine sahip, yüksek ölçeklenebilir ve güvenli bir harcama yönetim platformudur.

---

## 🎨 Mimari Topoloji

Sistem, **Domain-Driven Design (DDD)** ve **Event-Driven Architecture (EDA)** prensipleri üzerine inşa edilmiştir.

![Mimari Topoloji](topology.png)

> [!TIP]
> Detaylı mimari dökümantasyon için [Mimari Topoloji Rehberi](Docs/mimari-topoloji.md) sayfasını ziyaret edebilirsiniz.

---

## 🚀 Hızlı Başlangıç (Docker Compose)

Tek bir komutla tüm ekosistemi (API'ler, Veritabanları, Keycloak, UI, İzleme) ayağa kaldırın:

```bash
# 1. Projeyi derle ve testleri çalıştır
dotnet build Izometri.CaseStudy.slnx
dotnet test Izometri.CaseStudy.slnx

# 2. Docker altyapısını başlat
docker compose up -d --build
```

### 🔗 Servis Hub

| Servis | Adres | Kimlik Bilgileri |
| :--- | :--- | :--- |
| **🌐 Frontend UI** | `http://localhost:3000` | Giriş Sayfası |
| **🔐 Keycloak** | `http://localhost:18080` | `admin` / `admin` |
| **📖 Docs & API Hub** | `http://localhost:3000/#/docs` | Proje Paneli |
| **📧 Mailpit** | `http://localhost:8025` | SMTP Simülatörü |
| **📉 Jaeger** | `http://localhost:16686` | Distributed Tracing |
| **🐰 RabbitMQ** | `http://localhost:15673` | `izometri` / `Izometri2026!` |

---

## 📂 Proje Dökümantasyonu

Case değerlendirmesi için gereken tüm teknik detaylar modüler dökümanlarda toplanmıştır:

| Döküman | İçerik |
| :--- | :--- |
| **📦 [Teslimat Özeti](Docs/teslimat-özeti.md)** | **Hızlı Değerlendirme Özeti** |
| **🏗️ [Mimari Topoloji](Docs/mimari-topoloji.md)** | Teknik Tasarım ve Veri Akışı |
| **📡 [API Deneme Rehberi](Docs/api-deneme-reberi.md)** | Örnek İstekler ve Postman |
| **🧪 [Test ve Doğrulama](Docs/test-dogrulama.md)** | Test Kapsamı ve Sonuçları |
| **⚙️ [Çalıştırma Rehberi](Docs/çalıştırma-ve-ortamlar.md)** | Local ve Prod Kurulumu |

---

## 👥 Test Kullanıcıları ve Senaryolar

Tüm kullanıcılar için ortak şifre: `Pass123!`

| Tenant | E-posta | Rol | Senaryo |
| :--- | :--- | :--- | :--- |
| `test1` | `pattabanoglu@devrimmehmet.com` | **Admin** | Kullanıcı/Rol Yönetimi |
| `test1` | `devrimmehmet@gmail.com` | **HR** | Harcama Onay/Red |
| `test1` | `devrimmehmet@msn.com` | **Personel** | Harcama Girişi |
| `izometri` | `admin@izometri.com` | **Admin** | Cross-Tenant İzolasyon Testi |

---

## ✨ Öne Çıkan Özellikler

*   🛡️ **Multi-Tenancy:** JWT Claim + EF Core Global Query Filter ile tam veri izolasyonu.
*   🔄 **Outbox Pattern:** Transactional mesaj gönderimi (Guaranteed Delivery).
*   🔑 **Keycloak SSO:** OAuth2/OIDC tabanlı merkezi kimlik yönetimi.
*   📈 **Observability:** OpenTelemetry, Jaeger ve Serilog ile tam görünürlük.
*   🚀 **Resilience:** Polly ile servisler arası hata toleransı.
*   🤖 **Auto-OpenAPI:** CI pipeline entegre otomatik dökümantasyon ve drift check.

---
**Hazırlayan:** Izometri Case Study Adayı
**İletişim:** [devrimmehmet@gmail.com](mailto:devrimmehmet@gmail.com)
