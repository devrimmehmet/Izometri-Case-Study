# 🏗️ Mimari Topoloji ve Sistem Tasarımı

Bu belge, **Izometri Harcama Yönetim Sistemi**'nin teknik mimarisini, veri akışını ve bileşenler arası iletişim modellerini detaylandırmaktadır.

## 📐 Sistem Mimarisi

Sistem, modern bulut tabanlı SaaS prensiplerine uygun olarak **Mikroservis Mimarisi** ve **Event-Driven Architecture (EDA)** yaklaşımlarıyla tasarlanmıştır.

```mermaid
graph TB
    subgraph "Frontend Layer"
        UI["Vue.js / Quasar SPA"]
    end

    subgraph "Gateway / Proxy"
        NGX["Nginx Gateway"]
    end

    subgraph "Identity Provider"
        KC["Keycloak (OIDC/OAuth2)"]
    end

    subgraph "Service Layer"
        subgraph "Expense Service (BC1)"
            EA["Expense.Api"]
            ED["Expense.Domain"]
            EI["Expense.Infrastructure"]
            EPB["Outbox Publisher Worker"]
            EDB[(PostgreSQL - expense_db)]
        end

        subgraph "Notification Service (BC2)"
            NA["Notification.Api"]
            NI["Notification.Infrastructure"]
            NCW["RabbitMQ Consumer Worker"]
            RW["Retention Worker"]
            NDB[(PostgreSQL - notification_db)]
        end
    end

    subgraph "Messaging & Monitoring"
        RMQ["RabbitMQ (Message Broker)"]
        JGR["Jaeger (Distributed Tracing)"]
        MP["Mailpit (SMTP Testing)"]
    end

    %% Flow
    UI --> NGX
    NGX --> KC
    NGX -- "/api/*" --> EA
    NGX -- "/notify-api/*" --> NA

    EA -- "Save + Outbox" --> EDB
    EPB -- "Poll" --> EDB
    EPB -- "Publish Events" --> RMQ

    RMQ -- "Consume" --> NCW
    NCW -- "Save Logs" --> NDB
    NCW -- "Sync Call" --> EA
    NCW -- "Send Email" --> MP

    EA & NA -- "Traces (OTLP)" --> JGR
```

## 🛠️ Teknik Stack

| Bileşen | Teknoloji | Açıklama |
| :--- | :--- | :--- |
| **Backend** | .NET 10 (C#) | Onion Architecture, DDD, CQRS |
| **Frontend** | Vue 3 + Quasar | SPA, Tailwind, Vite |
| **Auth** | Keycloak | OIDC, RBAC, Multi-Tenancy |
| **Veritabanı** | PostgreSQL 16 | Her servis için izole DB (Schema-per-service) |
| **Mesajlaşma** | RabbitMQ | Event-driven asenkron iletişim |
| **ORM** | EF Core 10 | Code-First, Global Query Filters |
| **Resilience** | Polly | Retry & Circuit Breaker |
| **Tracing** | OpenTelemetry | Dağıtık izleme (Jaeger) |

## 🛡️ Multi-Tenancy Yaklaşımı

Sistem **"Shared Database, Isolated Schema"** prensibiyle çalışır (bu case study'de basitlik için tek DB içinde tenant kolonları kullanılmıştır).

*   **Tenant İzolasyonu:** Her veritabanı sorgusunda `TenantId` bazlı global query filter uygulanır.
*   **Defense-in-Depth:** Hem API katmanında (JWT Claim) hem de veritabanı seviyesinde (Query Filter) tenant doğrulaması yapılır.

## 🔄 Kritik İş Akışları

### 1. Kullanıcı Senkronizasyonu (Keycloak Admin API)
Kullanıcı yönetimi işlemleri sırasında sistem, PostgreSQL veritabanı ile Keycloak arasında tam senkronizasyon sağlar.

```mermaid
sequenceDiagram
    participant Client as Frontend / Admin
    participant API as AdminUsersController
    participant Service as UserAdminService
    participant DB as PostgreSQL
    participant KC as Keycloak Admin API

    Client->>API: POST /api/admin/users
    API->>Service: CreateUserAsync()
    Service->>DB: BEGIN TX → User + Roles → COMMIT
    Note over Service,DB: DB commit başarılı
    Service->>KC: POST /admin/realms/izometri/users
    KC-->>Service: 201 Created
    Service->>KC: GET /admin/realms/izometri/users?email=...
    KC-->>Service: Keycloak user ID
    Service->>KC: POST /users/{id}/role-mappings/realm
    KC-->>Service: 204 No Content
    Service-->>API: UserResponse
    API-->>Client: 201 Created
```

### 2. Harcama Oluşturma ve Onay (Outbox Pattern)
Harcama kaydedildiğinde, aynı transaction içinde bir `OutboxMessage` oluşturulur. `OutboxPublisherWorker` bu mesajları asenkron olarak RabbitMQ'ya güvenli bir şekilde iletir. Bu sayede DB işlemi ile mesaj gönderimi arasında tutarlılık sağlanır.

### 2. Bildirim Gönderimi
`NotificationService`, RabbitMQ'dan gelen eventleri dinler. Bildirim içeriğini zenginleştirmek için `ExpenseService`'e senkron bir HTTP çağrısı yapar (resilience policy ile korunur) ve ardından E-posta/SMS gönderimini gerçekleştirir.

---
> [!NOTE]
> Bu mimari, yatayda ölçeklenebilir ve hata toleransı (fault-tolerance) yüksek bir yapı sunar.
