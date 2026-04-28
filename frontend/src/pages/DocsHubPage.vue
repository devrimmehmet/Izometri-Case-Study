<template>
  <q-page class="q-pa-lg docs-page">
    <div class="row items-start q-mb-lg">
      <div>
        <div class="text-h5 text-weight-bold q-mb-xs">Dokümantasyon & Servisler</div>
        <div class="text-grey-5 text-body2">
          Keycloak OAuth2, JWT Bearer authorization, tenant izolasyonu ve güncel servis topolojisi
        </div>
      </div>
      <q-space />
      <q-badge color="positive" outline class="q-mt-xs">Docker Compose Ready</q-badge>
    </div>

    <div class="text-h6 text-weight-bold q-mb-md">
      <q-icon name="dns" class="q-mr-sm" />Çalışan Servisler
    </div>
    <div class="row q-col-gutter-md q-mb-xl">
      <div
        v-for="svc in services"
        :key="svc.title"
        class="col-12 col-sm-6 col-md-4"
      >
        <div class="doc-link-card full-height" @click="openUrl(svc.url)">
          <q-icon :name="svc.icon" :color="svc.color" class="doc-icon" />
          <div class="doc-title text-white">{{ svc.title }}</div>
          <div class="doc-desc">{{ svc.desc }}</div>
          <div class="text-primary text-caption q-mt-sm doc-url">{{ svc.url }}</div>

          <div
            v-if="svc.credentials"
            class="q-mt-sm text-caption text-grey-4 bg-dark q-pa-xs rounded-borders credential-box"
          >
            <q-icon name="key" size="xs" class="q-mr-xs" /> {{ svc.credentials }}
          </div>

          <q-badge
            v-if="svc.healthKey"
            :color="healthStatus[svc.healthKey] === 'ok' ? 'positive' : 'negative'"
            class="q-mt-sm"
          >
            {{ healthStatus[svc.healthKey] === 'ok' ? '● Çevrimiçi' : '● Çevrimdışı' }}
          </q-badge>
        </div>
      </div>
    </div>

    <div class="text-h6 text-weight-bold q-mb-md">
      <q-icon name="description" class="q-mr-sm" />Case Dokümanları
    </div>
    <div class="glass-card q-mb-xl">
      <q-list dark bordered separator class="rounded-borders">
        <q-expansion-item
          v-for="doc in documents"
          :key="doc.title"
          group="docs"
          :icon="doc.icon"
          :label="doc.title"
          :caption="doc.desc"
          header-class="text-weight-bold"
        >
          <q-card class="bg-transparent">
            <q-card-section class="text-grey-4">
              <div v-html="doc.content"></div>
            </q-card-section>
          </q-card>
        </q-expansion-item>
      </q-list>
    </div>

    <div class="text-h6 text-weight-bold q-mb-md">
      <q-icon name="architecture" class="q-mr-sm" />Mimari Genel Bakış
    </div>
    <div class="glass-card q-mb-xl">
      <q-list dark bordered separator class="rounded-borders">
        <q-expansion-item
          icon="verified_user"
          label="Kimlik Doğrulama ve Yetkilendirme"
          caption="Keycloak OAuth2 access token + API tarafında JWT claim authorization"
          header-class="text-info text-weight-bold"
        >
          <q-card class="bg-transparent">
            <q-card-section>
              <div class="architecture-note q-mb-md">
                Authentication Keycloak tarafından yapılır. API'ler kullanıcı tokenı üretmez; sadece JWT Bearer
                access token doğrular. Authorization, tenant izolasyonu ve rol kontrolleri API tarafında
                <code>UserId</code>, <code>TenantId</code>, <code>role</code> ve <code>aud</code> claimleriyle yapılır.
              </div>
              <endpoint-list :items="authEndpoints" color="info" />
            </q-card-section>
          </q-card>
        </q-expansion-item>

        <q-expansion-item
          icon="api"
          label="Harcama Servisi (ExpenseService)"
          caption="Port 5001 - ana domain API, tenant scoped data ve outbox publisher"
          header-class="text-primary text-weight-bold"
        >
          <q-card class="bg-transparent">
            <q-card-section>
              <endpoint-list :items="expenseEndpoints" color="primary" />
            </q-card-section>
          </q-card>
        </q-expansion-item>

        <q-expansion-item
          icon="notifications"
          label="Bildirim Servisi (NotificationService)"
          caption="Port 5002 - RabbitMQ consumer, bildirim logları ve e-posta probe"
          header-class="text-accent text-weight-bold"
        >
          <q-card class="bg-transparent">
            <q-card-section>
              <endpoint-list :items="notificationEndpoints" color="accent" />
            </q-card-section>
          </q-card>
        </q-expansion-item>
      </q-list>
    </div>
  </q-page>
</template>

<script setup lang="ts">
import { reactive, onMounted, defineComponent, h, type PropType } from 'vue';
import axios from 'axios';

interface ServiceLink {
  title: string;
  desc: string;
  url: string;
  icon: string;
  color: string;
  healthKey?: string;
  credentials?: string;
}

interface EndpointInfo {
  method: string;
  path: string;
  auth: string;
  desc: string;
}

const EndpointList = defineComponent({
  name: 'EndpointList',
  props: {
    items: {
      type: Array as PropType<EndpointInfo[]>,
      required: true,
    },
    color: {
      type: String,
      default: 'primary',
    },
  },
  setup(props) {
    return () =>
      h(
        'div',
        { class: 'endpoint-list' },
        props.items.map((endpoint) =>
          h('div', { class: 'endpoint-row', key: `${endpoint.method}-${endpoint.path}` }, [
            h('div', { class: 'endpoint-main' }, [
              h('span', { class: `endpoint-method method-${endpoint.method.toLowerCase()}` }, endpoint.method),
              h('code', { class: 'endpoint-path' }, endpoint.path),
            ]),
            h('div', { class: 'endpoint-meta' }, [
              h('span', { class: `endpoint-auth text-${props.color}` }, endpoint.auth),
              h('span', { class: 'endpoint-desc' }, endpoint.desc),
            ]),
          ]),
        ),
      );
  },
});

const services: ServiceLink[] = [
  {
    title: 'Frontend',
    desc: 'Quasar PWA arayüzü ve nginx reverse proxy',
    url: 'http://localhost:3000',
    icon: 'web',
    color: 'primary',
  },
  {
    title: 'Keycloak',
    desc: 'OAuth2/OIDC realm, client, user, tenant ve role seed kaynağı',
    url: 'http://localhost:18080',
    icon: 'admin_panel_settings',
    color: 'info',
    credentials: 'Admin: admin / admin',
  },
  {
    title: 'Harcama API Swagger',
    desc: 'ExpenseService OpenAPI dokümantasyonu',
    url: 'http://localhost:5001/swagger',
    icon: 'code',
    color: 'primary',
    healthKey: 'expense',
  },
  {
    title: 'Bildirim API Swagger',
    desc: 'NotificationService OpenAPI dokümantasyonu',
    url: 'http://localhost:5002/swagger',
    icon: 'notifications',
    color: 'accent',
    healthKey: 'notification',
  },
  {
    title: 'RabbitMQ Yönetimi',
    desc: 'Outbox ile yayınlanan integration event kuyrukları',
    url: 'http://localhost:15673',
    icon: 'hub',
    color: 'warning',
    credentials: 'Kullanıcı: izometri / Şifre: Izometri2026!',
  },
  {
    title: 'Mailpit',
    desc: 'Yerel e-posta test arayüzü',
    url: 'http://localhost:8025',
    icon: 'email',
    color: 'info',
    credentials: 'Şifre gerektirmez',
  },
  {
    title: 'Jaeger İzleme',
    desc: 'OpenTelemetry trace ve correlation akışı',
    url: 'http://localhost:16686',
    icon: 'timeline',
    color: 'secondary',
    credentials: 'Şifre gerektirmez',
  },
];

const documents = [
  {
    title: 'BR-1 & BR-2: Multi-Tenant ve Rol Modeli',
    desc: 'Tenant izolasyonu JWT claimleriyle başlar, EF Core global filter ile uygulanır',
    icon: 'corporate_fare',
    content: `
      <div class="doc-callout doc-callout-primary">
        <h4>Tenant izolasyonu</h4>
        <p>Her kullanıcı tek bir tenant'a aittir. <code>TenantId</code> claim'i Keycloak access token içinde gelir, API tarafındaki current user context tarafından okunur ve EF Core global query filter ile tüm sorgulara uygulanır.</p>
      </div>
      <p><strong>Roller Keycloak üzerinde statik tanımlıdır:</strong> <code>Admin</code>, <code>HR</code>, <code>Personel</code>.</p>
      <ul>
        <li><strong>Admin:</strong> Tenant içindeki kullanıcı, ayar, outbox ve tüm harcama kayıtlarını yönetir.</li>
        <li><strong>HR:</strong> Tenant içindeki harcamaları görüntüler, HR onay/red adımını yürütür.</li>
        <li><strong>Personel:</strong> Kendi harcamalarını oluşturur, günceller, submit eder ve takip eder.</li>
      </ul>
    `,
  },
  {
    title: 'OAuth2 / Keycloak Entegrasyonu',
    desc: 'Authentication Keycloak, authorization API tarafında JWT Bearer claimleriyle yapılır',
    icon: 'verified_user',
    content: `
      <div class="doc-callout doc-callout-info">
        <h4>Güncel karar</h4>
        <p>Docker/prod akışında <code>/api/auth/login</code> kullanıcı tokenı üretmez. Frontend Keycloak token endpointinden access token alır; API'ler bu tokenı <code>Jwt__Authority</code> ve <code>Jwt__PublicAuthority</code> ayarlarıyla doğrular.</p>
      </div>
      <ul>
        <li><strong>Realm:</strong> <code>izometri</code></li>
        <li><strong>Client:</strong> <code>expense-service</code>, audience <code>expense-service</code></li>
        <li><strong>Claims:</strong> <code>UserId</code>, <code>TenantId</code>, <code>role</code>, <code>aud</code>, <code>email</code></li>
        <li><strong>Seed ayrımı:</strong> uygulama DB seed'i tenant, expense ve notification verilerini; Keycloak seed'i realm, client, user ve role verilerini yükler.</li>
        <li><strong>Local fallback:</strong> kodda geliştirme/test için durur; Docker'da <code>Authentication__EnableLocalLogin=false</code>.</li>
      </ul>
    `,
  },
  {
    title: 'BR-3 & BR-4: Harcama ve Onay Akışı',
    desc: 'Draft, Pending, Approved, Rejected durumları ve tutara bağlı kademeli onay',
    icon: 'price_check',
    content: `
      <p>Personel Seyahat, Malzeme, Eğitim ve Diğer kategorilerinde TRY, USD veya EUR para birimiyle harcama oluşturabilir. Dövizli harcamalar tenant bazlı kur ayarlarıyla TRY karşılığına çevrilir.</p>
      <div class="flow-grid">
        <div class="flow-card flow-ok"><strong>≤ 5.000 TL</strong><span>HR onayı yeterlidir.</span></div>
        <div class="flow-card flow-warn"><strong>&gt; 5.000 TL</strong><span>Önce HR, sonra Admin onayı gerekir.</span></div>
      </div>
      <p>Red işleminde minimum 10 karakterlik gerekçe zorunludur. Delete işlemleri hard delete yapmaz; soft delete ve audit alanları korunur.</p>
    `,
  },
  {
    title: 'TR: Servis Mimarisi ve Patternler',
    desc: 'Onion Architecture, Repository, Unit of Work, Soft Delete, OpenTelemetry',
    icon: 'account_tree',
    content: `
      <ul>
        <li><strong>Database per Service:</strong> ExpenseService ve NotificationService ayrı PostgreSQL veritabanları kullanır.</li>
        <li><strong>Onion Architecture:</strong> Domain, Application, Infrastructure ve Api katmanları ayrıdır.</li>
        <li><strong>Repository & Unit of Work:</strong> transaction sınırları application servislerinde kontrollü tutulur.</li>
        <li><strong>Soft Delete:</strong> tüm ana entity'lerde <code>IsDeleted</code>, audit ve global filter uygulanır.</li>
        <li><strong>Observability:</strong> Serilog, correlation id middleware ve OpenTelemetry/Jaeger izleme akışı bulunur.</li>
      </ul>
    `,
  },
  {
    title: 'BR-5 & TB-1: Async Bildirim ve Outbox',
    desc: 'RabbitMQ, MassTransit, Quartz worker ve güvenilir event yayını',
    icon: 'move_to_inbox',
    content: `
      <div class="doc-callout doc-callout-warning">
        <h4>Outbox Pattern</h4>
        <p>Domain işleminden çıkan integration event önce Expense DB içindeki <code>OutboxMessages</code> tablosuna yazılır. Quartz worker mesajı RabbitMQ'ya taşır; başarısız kayıtlar dead-letter görünümünden izlenir.</p>
      </div>
      <p>NotificationService RabbitMQ eventlerini tüketir, kendi veritabanına notification log yazar ve Mailpit üzerinden e-posta simülasyonu yapar. Gerekli detaylar için ExpenseService'e internal service JWT ile HTTP çağrısı yapabilir.</p>
    `,
  },
];

const authEndpoints: EndpointInfo[] = [
  {
    method: 'POST',
    path: 'http://localhost:18080/realms/izometri/protocol/openid-connect/token',
    auth: 'Public OAuth2',
    desc: 'Frontend Keycloak password grant ile access token alır.',
  },
  {
    method: 'POST',
    path: '/api/auth/login',
    auth: 'Dev fallback',
    desc: 'Docker akışında kapalıdır; 404 Local login disabled döner.',
  },
];

const expenseEndpoints: EndpointInfo[] = [
  { method: 'GET', path: '/api/expenses', auth: 'JWT', desc: 'Tenant scoped listeleme; tarih, statü, kategori ve sayfalama destekler.' },
  { method: 'GET', path: '/api/expenses/{id}', auth: 'JWT', desc: 'Tenant ve rol kurallarına göre tek harcama detayı.' },
  { method: 'POST', path: '/api/expenses', auth: 'Personel', desc: 'Yeni draft harcama oluşturur.' },
  { method: 'PUT', path: '/api/expenses/{id}', auth: 'Owner', desc: 'Draft harcamayı günceller.' },
  { method: 'PUT', path: '/api/expenses/{id}/submit', auth: 'Owner', desc: 'Draft harcamayı Pending durumuna taşır.' },
  { method: 'PUT', path: '/api/expenses/{id}/approve', auth: 'HR/Admin', desc: 'Tutar eşiğine göre HR ve Admin onay adımlarını uygular.' },
  { method: 'PUT', path: '/api/expenses/{id}/reject', auth: 'HR/Admin', desc: 'Gerekçeli red işlemi yapar.' },
  { method: 'DELETE', path: '/api/expenses/{id}', auth: 'Owner/Admin', desc: 'Soft delete uygular.' },
  { method: 'GET', path: '/api/admin/users', auth: 'Admin', desc: 'Token tenantındaki kullanıcıları listeler.' },
  { method: 'POST', path: '/api/admin/users', auth: 'Admin', desc: 'Tenant içinde kullanıcı oluşturur; body alanı displayName kullanır.' },
  { method: 'PUT', path: '/api/admin/users/{userId}/roles', auth: 'Admin', desc: 'Statik rolleri günceller.' },
  { method: 'GET', path: '/api/settings/exchange-rates', auth: 'Admin', desc: 'Tenant bazlı USD/EUR kur ayarlarını getirir.' },
  { method: 'PUT', path: '/api/settings/exchange-rates', auth: 'Admin', desc: 'Tenant bazlı kur ayarlarını günceller.' },
  { method: 'GET', path: '/api/admin/outbox/dead-letters', auth: 'Admin', desc: 'Outbox dead-letter kayıtlarını listeler.' },
  { method: 'GET', path: '/health', auth: 'Public', desc: 'Container healthcheck endpointi.' },
];

const notificationEndpoints: EndpointInfo[] = [
  { method: 'GET', path: '/api/notifications', auth: 'JWT', desc: 'TenantId token claiminden okunur; frontend tenant query göndermez.' },
  { method: 'GET', path: '/api/notifications?tenantId={tenantId}', auth: 'JWT', desc: 'Opsiyonel geriye uyumluluk; query token tenantı ile aynı değilse 403 döner.' },
  { method: 'GET', path: '/api/admin/notifications/dead-letters', auth: 'Admin', desc: 'Notification dead-letter kayıtlarını token tenantı için listeler.' },
  { method: 'POST', path: '/api/admin/notifications/probe-email', auth: 'Admin', desc: 'Mailpit/SMTP test e-postası gönderir.' },
  { method: 'GET', path: '/health', auth: 'Public', desc: 'Container healthcheck endpointi.' },
];

const healthStatus = reactive<Record<string, string>>({});

function openUrl(url: string) {
  window.open(url, '_blank');
}

async function checkHealth(key: string, url: string) {
  try {
    await axios.get(url, { timeout: 3000 });
    healthStatus[key] = 'ok';
  } catch {
    healthStatus[key] = 'down';
  }
}

onMounted(() => {
  void checkHealth('expense', '/health/expense');
  void checkHealth('notification', '/health/notification');
});
</script>

<style scoped>
.docs-page :deep(code) {
  color: #93c5fd;
  background: rgba(148, 163, 184, 0.12);
  border: 1px solid rgba(148, 163, 184, 0.18);
  border-radius: 4px;
  padding: 1px 5px;
}

.doc-link-card {
  cursor: pointer;
  transition: all 0.2s ease;
}

.doc-link-card:hover {
  transform: translateY(-2px);
  border-color: rgba(99, 102, 241, 0.35);
}

.doc-url {
  overflow-wrap: anywhere;
}

.credential-box {
  border: 1px solid rgba(255, 255, 255, 0.1);
}

.architecture-note {
  color: #cbd5e1;
  border-left: 3px solid rgba(56, 189, 248, 0.8);
  background: rgba(56, 189, 248, 0.08);
  border-radius: 4px;
  padding: 10px 12px;
  line-height: 1.55;
}

.endpoint-list {
  display: grid;
  gap: 8px;
}

.endpoint-row {
  display: grid;
  grid-template-columns: minmax(280px, 0.9fr) minmax(220px, 1.1fr);
  gap: 12px;
  align-items: center;
  padding: 10px 12px;
  border: 1px solid rgba(148, 163, 184, 0.14);
  border-radius: 6px;
  background: rgba(15, 23, 42, 0.42);
}

.endpoint-main {
  display: flex;
  align-items: center;
  min-width: 0;
  gap: 8px;
}

.endpoint-method {
  width: 54px;
  flex: 0 0 54px;
  text-align: center;
  border-radius: 4px;
  padding: 2px 0;
  font-size: 11px;
  font-weight: 700;
  letter-spacing: 0;
}

.method-get {
  color: #93c5fd;
  background: rgba(59, 130, 246, 0.16);
}

.method-post {
  color: #86efac;
  background: rgba(34, 197, 94, 0.16);
}

.method-put {
  color: #fde68a;
  background: rgba(234, 179, 8, 0.16);
}

.method-delete {
  color: #fca5a5;
  background: rgba(239, 68, 68, 0.16);
}

.endpoint-path {
  white-space: normal;
  overflow-wrap: anywhere;
}

.endpoint-meta {
  display: grid;
  grid-template-columns: 96px 1fr;
  gap: 10px;
  color: #cbd5e1;
  font-size: 12px;
  line-height: 1.45;
}

.endpoint-auth {
  font-weight: 700;
}

.endpoint-desc {
  color: #cbd5e1;
}

.docs-page :deep(.doc-callout) {
  padding: 10px 12px;
  border-radius: 6px;
  margin-bottom: 14px;
}

.docs-page :deep(.doc-callout h4) {
  margin: 0 0 8px;
  color: #fff;
  font-size: 14px;
}

.docs-page :deep(.doc-callout p) {
  margin: 0;
}

.docs-page :deep(.doc-callout-primary) {
  background: rgba(99, 102, 241, 0.1);
  border-left: 4px solid #6366f1;
}

.docs-page :deep(.doc-callout-info) {
  background: rgba(56, 189, 248, 0.1);
  border-left: 4px solid #38bdf8;
}

.docs-page :deep(.doc-callout-warning) {
  background: rgba(234, 179, 8, 0.1);
  border-left: 4px solid #eab308;
}

.docs-page :deep(ul) {
  padding-left: 20px;
  margin: 0;
  line-height: 1.65;
}

.docs-page :deep(.flow-grid) {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(180px, 1fr));
  gap: 10px;
  margin: 14px 0;
}

.docs-page :deep(.flow-card) {
  display: grid;
  gap: 4px;
  padding: 10px 12px;
  border-radius: 6px;
}

.docs-page :deep(.flow-ok) {
  background: rgba(34, 197, 94, 0.14);
  border: 1px solid rgba(34, 197, 94, 0.28);
}

.docs-page :deep(.flow-warn) {
  background: rgba(239, 68, 68, 0.12);
  border: 1px solid rgba(239, 68, 68, 0.26);
}

@media (max-width: 760px) {
  .endpoint-row {
    grid-template-columns: 1fr;
  }

  .endpoint-meta {
    grid-template-columns: 1fr;
    gap: 2px;
  }
}
</style>
