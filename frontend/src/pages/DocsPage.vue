<template>
  <q-page class="q-pa-lg docs-page">
    <!-- ── Başlık ──────────────────────────────────────────────────────── -->
    <div class="row items-center q-mb-xl">
      <div class="brand-icon-wrap q-mr-md">
        <q-icon name="menu_book" size="28px" color="white" />
      </div>
      <div>
        <div class="text-h4 text-weight-bold text-white">Sistem Dökümantasyonu</div>
        <div class="text-grey-5 text-body2 q-mt-xs">
          İzometri Harcama Yönetimi &mdash; Geliştirici &amp; Admin Referansı
        </div>
      </div>
    </div>

    <!-- ── BÖLÜM 1: Altyapı Servisleri ───────────────────────────────── -->
    <SectionHeader
      icon="dns"
      title="Altyapı Servisleri"
      subtitle="Yerel Docker Compose ortamındaki tüm servislerin bağlantı ve kimlik bilgileri — kartlara tıklayarak açabilirsiniz"
    />

    <div class="text-overline text-grey-6 q-mb-sm q-mt-lg">WEB ARAYÜZLERİ</div>
    <div class="row q-col-gutter-md q-mb-lg">
      <div v-for="svc in webServices" :key="svc.name" class="col-12 col-sm-6 col-md-4">
        <ServiceCard :svc="svc" @open="openService(svc)" @copy="copy" />
      </div>
    </div>

    <div class="text-overline text-grey-6 q-mb-sm">API SERVİSLERİ</div>
    <div class="row q-col-gutter-md q-mb-lg">
      <div v-for="svc in apiServices" :key="svc.name" class="col-12 col-sm-6 col-md-4">
        <ServiceCard :svc="svc" @open="openService(svc)" @copy="copy" />
      </div>
    </div>

    <div class="text-overline text-grey-6 q-mb-sm">VERİTABANLARI</div>
    <div class="row q-col-gutter-md q-mb-xl">
      <div v-for="svc in dbServices" :key="svc.name" class="col-12 col-sm-6 col-md-4">
        <ServiceCard :svc="svc" @open="openService(svc)" @copy="copy" />
      </div>
    </div>

    <!-- ── BÖLÜM 2: Demo Hesapları ────────────────────────────────────── -->
    <SectionHeader
      icon="manage_accounts"
      title="Demo Hesapları"
      subtitle="Test ortamındaki hazır kullanıcı hesapları — tüm hesapların şifresi Pass123!"
    />

    <div class="row q-col-gutter-md q-mb-xl">
      <div v-for="tenant in demoTenants" :key="tenant.code" class="col-12 col-md-4">
        <div class="tenant-card">
          <div
            class="tenant-header q-px-md q-py-sm row items-center"
            :style="{
              background: `linear-gradient(135deg, ${tenant.color}22, ${tenant.color}0a)`,
              borderBottom: `1px solid ${tenant.color}33`,
            }"
          >
            <q-icon name="domain" size="18px" class="q-mr-sm" :style="{ color: tenant.color }" />
            <span class="text-subtitle2 text-weight-bold text-white">{{ tenant.label }}</span>
            <q-space />
            <q-badge
              outline
              class="text-caption"
              :style="{ color: tenant.color, borderColor: tenant.color }"
            >
              {{ tenant.code }}
            </q-badge>
          </div>
          <div class="q-pa-md">
            <div v-for="acc in tenant.accounts" :key="acc.email" class="account-row q-mb-sm">
              <div class="row items-center q-mb-xs">
                <q-icon
                  :name="roleIcon(acc.role)"
                  :color="roleColor(acc.role)"
                  size="14px"
                  class="q-mr-xs"
                />
                <span
                  class="text-caption text-weight-medium"
                  :class="`text-${roleColor(acc.role)}`"
                  >{{ acc.role }}</span
                >
              </div>
              <div class="cred-box row items-center justify-between q-px-sm q-py-xs q-mb-xs">
                <span class="text-caption text-mono text-grey-4 ellipsis">{{ acc.email }}</span>
                <q-btn
                  flat
                  dense
                  round
                  icon="content_copy"
                  size="xs"
                  color="grey-6"
                  @click.stop="copy(acc.email)"
                >
                  <q-tooltip>E-postayı kopyala</q-tooltip>
                </q-btn>
              </div>
              <div class="cred-box row items-center justify-between q-px-sm q-py-xs">
                <span class="text-caption text-mono text-grey-4">Pass123!</span>
                <q-btn
                  flat
                  dense
                  round
                  icon="content_copy"
                  size="xs"
                  color="grey-6"
                  @click.stop="copy('Pass123!')"
                >
                  <q-tooltip>Şifreyi kopyala</q-tooltip>
                </q-btn>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>

    <!-- ── BÖLÜM 3: Demo Akışı ────────────────────────────────────────── -->
    <SectionHeader
      icon="play_circle"
      title="Demo Akışı"
      subtitle="Sistemi uçtan uca test etmek için adım adım rehber"
    />

    <div class="row q-col-gutter-md q-mb-xl">
      <div class="col-12 col-md-6">
        <div class="glass-card q-pa-lg h-full">
          <q-timeline color="primary" dark>
            <q-timeline-entry
              v-for="step in demoSteps"
              :key="step.title"
              :title="step.title"
              :subtitle="step.subtitle"
              :icon="step.icon"
              :color="step.color ?? 'primary'"
            >
              <div v-if="step.detail" class="text-grey-5 text-caption">{{ step.detail }}</div>
            </q-timeline-entry>
          </q-timeline>
        </div>
      </div>
      <div class="col-12 col-md-6 column q-gutter-md">
        <div class="glass-card q-pa-lg col">
          <div class="text-subtitle2 text-weight-bold text-white q-mb-md">
            <q-icon name="rule" color="primary" class="q-mr-xs" />Onay Eşiği Kuralı
          </div>
          <div class="rule-block q-pa-sm q-mb-sm row items-center">
            <q-icon name="check_circle" color="positive" size="22px" class="q-mr-sm" />
            <div>
              <div class="text-caption text-weight-bold text-white">≤ 5.000 TL</div>
              <div class="text-caption text-grey-5">Sadece HR onayı → tamamlandı</div>
            </div>
          </div>
          <div class="rule-block q-pa-sm row items-center">
            <q-icon name="warning" color="warning" size="22px" class="q-mr-sm" />
            <div>
              <div class="text-caption text-weight-bold text-white">5.000 TL</div>
              <div class="text-caption text-grey-5">HR onayı + Admin onayı → çift aşamalı</div>
            </div>
          </div>
        </div>
        <div class="glass-card q-pa-lg col">
          <div class="text-subtitle2 text-weight-bold text-white q-mb-md">
            <q-icon name="notifications" color="primary" class="q-mr-xs" />Bildirim Tetikleyicileri
          </div>
          <q-list dark dense>
            <q-item v-for="t in notificationTriggers" :key="t.event" dense class="q-px-none">
              <q-item-section avatar style="min-width: 28px">
                <q-icon :name="t.icon" :color="t.color" size="16px" />
              </q-item-section>
              <q-item-section>
                <q-item-label class="text-caption text-white">{{ t.event }}</q-item-label>
                <q-item-label caption class="text-grey-5">{{ t.recipient }}</q-item-label>
              </q-item-section>
            </q-item>
          </q-list>
        </div>
      </div>
    </div>

    <!-- ── BÖLÜM 4: Sistem Mimarisi ──────────────────────────────────── -->
    <SectionHeader
      icon="architecture"
      title="Sistem Mimarisi"
      subtitle="Microservice tabanlı harcama yönetim platformu"
    />

    <div class="row q-col-gutter-md q-mb-xl">
      <div class="col-12 col-md-6">
        <div class="glass-card q-pa-lg h-full">
          <div class="row items-center q-mb-sm">
            <q-icon name="layers" color="indigo-4" size="20px" class="q-mr-sm" />
            <span class="text-subtitle1 text-weight-bold text-white">ExpenseService</span>
            <q-badge color="indigo-4" outline class="q-ml-sm text-caption">:5001</q-badge>
          </div>
          <p class="text-grey-4 text-body2 q-mb-md">
            Harcama taleplerinin oluşturulması, onay/ret işlemleri ve kullanıcı/tenant yönetimini
            üstlenen ana iş servisidir.
          </p>
          <div
            v-for="item in expenseServiceFeatures"
            :key="item"
            class="feature-item row items-start q-mb-xs"
          >
            <q-icon
              name="chevron_right"
              color="indigo-4"
              size="14px"
              class="q-mt-xs q-mr-xs"
              style="flex-shrink: 0"
            />
            <span class="text-caption text-grey-3">{{ item }}</span>
          </div>
        </div>
      </div>
      <div class="col-12 col-md-6">
        <div class="glass-card q-pa-lg h-full">
          <div class="row items-center q-mb-sm">
            <q-icon name="notifications_active" color="teal-4" size="20px" class="q-mr-sm" />
            <span class="text-subtitle1 text-weight-bold text-white">NotificationService</span>
            <q-badge color="teal-4" outline class="q-ml-sm text-caption">:5002</q-badge>
          </div>
          <p class="text-grey-4 text-body2 q-mb-md">
            RabbitMQ üzerinden integration event'leri tüketen; e-posta, SMS gönderimi ve bildirim
            kayıtlarını yöneten bağımsız servistir.
          </p>
          <div
            v-for="item in notificationServiceFeatures"
            :key="item"
            class="feature-item row items-start q-mb-xs"
          >
            <q-icon
              name="chevron_right"
              color="teal-4"
              size="14px"
              class="q-mt-xs q-mr-xs"
              style="flex-shrink: 0"
            />
            <span class="text-caption text-grey-3">{{ item }}</span>
          </div>
        </div>
      </div>
      <div class="col-12">
        <div class="glass-card q-pa-lg">
          <div class="text-subtitle2 text-weight-bold text-white q-mb-md">
            <q-icon name="swap_horiz" color="primary" class="q-mr-xs" />Servisler Arası İletişim
          </div>
          <div class="row q-col-gutter-md">
            <div
              v-for="comm in communicationPatterns"
              :key="comm.title"
              class="col-12 col-sm-6 col-md-3"
            >
              <div class="comm-block q-pa-md text-center">
                <q-icon :name="comm.icon" :color="comm.color" size="30px" class="q-mb-xs" />
                <div class="text-caption text-weight-bold text-white">{{ comm.title }}</div>
                <div class="text-caption text-grey-5 q-mt-xs">{{ comm.desc }}</div>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>

    <!-- ── BÖLÜM 5: Teknoloji Yığını ────────────────────────────────── -->
    <SectionHeader
      icon="code"
      title="Teknoloji Yığını"
      subtitle="Projede kullanılan framework, kütüphane ve altyapı bileşenleri"
    />

    <div class="row q-col-gutter-md q-mb-xl">
      <div v-for="stack in techStacks" :key="stack.category" class="col-12 col-sm-6 col-md-3">
        <div class="stack-card q-pa-md">
          <div class="row items-center q-mb-md">
            <div
              class="stack-icon-wrap q-mr-sm"
              :style="{ background: `${stack.accent}22`, border: `1px solid ${stack.accent}44` }"
            >
              <q-icon :name="stack.icon" size="16px" :style="{ color: stack.accent }" />
            </div>
            <span class="text-subtitle2 text-weight-bold text-white">{{ stack.category }}</span>
          </div>
          <div class="row wrap q-gutter-xs">
            <q-chip
              v-for="tech in stack.items"
              :key="tech"
              dense
              outline
              class="text-caption tech-chip"
              :style="{ borderColor: `${stack.accent}55`, color: stack.accent }"
            >
              {{ tech }}
            </q-chip>
          </div>
        </div>
      </div>
    </div>
  </q-page>
</template>

<script setup lang="ts">
import { h } from 'vue';
import { useQuasar } from 'quasar';

const $q = useQuasar();

function copy(text: string) {
  void navigator.clipboard.writeText(text);
  $q.notify({ type: 'positive', message: 'Panoya kopyalandı', timeout: 1200 });
}

function openService(svc: ServiceDef) {
  if (svc.openUrl) {
    window.open(svc.openUrl, '_blank');
  } else if (svc.url.startsWith('http')) {
    window.open(svc.url, '_blank');
  }
}

function roleColor(role: string) {
  if (role === 'Admin') return 'purple';
  if (role === 'HR') return 'blue';
  return 'teal';
}
function roleIcon(role: string) {
  if (role === 'Admin') return 'admin_panel_settings';
  if (role === 'HR') return 'badge';
  return 'person';
}

// ── Tip tanımı ─────────────────────────────────────────────────────
interface ServiceDef {
  name: string;
  url: string;
  openUrl?: string;
  icon: string;
  accent: string;
  desc: string;
  port?: string;
  credentials?: { label: string; value: string }[];
  badge?: string;
  badgeColor?: string;
}

// ── Altyapı verileri ───────────────────────────────────────────────
const webServices: ServiceDef[] = [
  {
    name: 'İzometri Frontend',
    url: 'http://localhost:3000',
    icon: 'web',
    accent: '#6366f1',
    port: '3000',
    desc: 'Vue 3 + Quasar SPA — ana uygulama',
    badge: 'Giriş gerekmez',
    badgeColor: 'positive',
  },
  {
    name: 'Keycloak Yönetim',
    url: 'http://localhost:18080',
    openUrl: 'http://localhost:18080/admin',
    icon: 'lock',
    accent: '#f97316',
    port: '18080',
    desc: 'Kimlik ve oturum yönetimi (OIDC)',
    credentials: [
      { label: 'Kullanıcı', value: 'admin' },
      { label: 'Şifre', value: 'admin' },
    ],
  },
  {
    name: 'RabbitMQ Yönetim',
    url: 'http://localhost:15673',
    icon: 'hub',
    accent: '#ff6600',
    port: '15673',
    desc: 'Mesaj kuyruğu — Exchange, Queue, Binding',
    credentials: [
      { label: 'Kullanıcı', value: 'izometri' },
      { label: 'Şifre', value: 'Izometri2026!' },
    ],
  },
  {
    name: 'Mailpit',
    url: 'http://localhost:8025',
    icon: 'mark_email_read',
    accent: '#06b6d4',
    port: '8025',
    desc: 'Test e-posta yakalama arayüzü',
    badge: 'Giriş gerekmez',
    badgeColor: 'positive',
  },
  {
    name: 'Jaeger Tracing',
    url: 'http://localhost:16686',
    icon: 'timeline',
    accent: '#60a5fa',
    port: '16686',
    desc: 'Dağıtık OpenTelemetry izleme',
    badge: 'Giriş gerekmez',
    badgeColor: 'positive',
  },
];

const apiServices: ServiceDef[] = [
  {
    name: 'ExpenseService API',
    url: 'http://localhost:5001',
    openUrl: 'http://localhost:5001/swagger',
    icon: 'receipt_long',
    accent: '#818cf8',
    port: '5001',
    desc: 'Harcama, kullanıcı ve tenant yönetimi',
    credentials: [
      { label: 'Swagger', value: 'http://localhost:5001/swagger' },
      { label: 'Auth', value: 'Bearer JWT' },
    ],
  },
  {
    name: 'NotificationService API',
    url: 'http://localhost:5002',
    openUrl: 'http://localhost:5002/swagger',
    icon: 'notifications',
    accent: '#2dd4bf',
    port: '5002',
    desc: 'Bildirim kayıtları ve dead-letter yönetimi',
    credentials: [
      { label: 'Swagger', value: 'http://localhost:5002/swagger' },
      { label: 'Auth', value: 'Bearer JWT' },
    ],
  },
];

const dbServices: ServiceDef[] = [
  {
    name: 'Harcama Veritabanı',
    url: 'localhost:15433',
    icon: 'storage',
    accent: '#94a3b8',
    port: '15433',
    desc: 'PostgreSQL 16 — expense_db',
    credentials: [
      { label: 'Host', value: 'localhost:15433' },
      { label: 'Kullanıcı', value: 'postgres' },
      { label: 'Şifre', value: 'postgres' },
      { label: 'Veritabanı', value: 'expense_db' },
    ],
  },
  {
    name: 'Bildirim Veritabanı',
    url: 'localhost:15434',
    icon: 'storage',
    accent: '#94a3b8',
    port: '15434',
    desc: 'PostgreSQL 16 — notification_db',
    credentials: [
      { label: 'Host', value: 'localhost:15434' },
      { label: 'Kullanıcı', value: 'postgres' },
      { label: 'Şifre', value: 'postgres' },
      { label: 'Veritabanı', value: 'notification_db' },
    ],
  },
];

// ── Demo hesapları ─────────────────────────────────────────────────
const demoTenants = [
  {
    code: 'test1',
    label: 'TEST1 Tenant',
    color: '#8b5cf6',
    accounts: [
      { role: 'Admin', email: 'pattabanoglu@devrimmehmet.com' },
      { role: 'HR', email: 'devrimmehmet@gmail.com' },
      { role: 'Personel 1', email: 'devrimmehmet@msn.com' },
      { role: 'Personel 2', email: 'personel2@test1.com' },
    ],
  },
  {
    code: 'izometri',
    label: 'İzometri Tenant',
    color: '#6366f1',
    accounts: [
      { role: 'Admin', email: 'admin@izometri.com' },
      { role: 'HR', email: 'hr@izometri.com' },
      { role: 'Personel 1', email: 'personel@izometri.com' },
      { role: 'Personel 2', email: 'personel2@izometri.com' },
    ],
  },
  {
    code: 'test2',
    label: 'TEST2 Tenant',
    color: '#06b6d4',
    accounts: [
      { role: 'Admin', email: 'admin@test2.com' },
      { role: 'HR', email: 'hr@test2.com' },
      { role: 'Personel 1', email: 'personel@test2.com' },
      { role: 'Personel 2', email: 'personel2@test2.com' },
    ],
  },
];

// ── Demo adımları ──────────────────────────────────────────────────
const demoSteps = [
  {
    title: 'Personel ile Giriş Yap',
    subtitle: 'test1 / devrimmehmet@msn.com / Pass123!',
    icon: 'person',
    color: 'teal',
    detail: 'Harcamalar sayfasından yeni bir talep oluşturun ve "Onaya Gönder" butonuna tıklayın.',
  },
  {
    title: 'HR ile Giriş Yap',
    subtitle: 'test1 / devrimmehmet@gmail.com / Pass123!',
    icon: 'badge',
    color: 'blue',
    detail: 'Bekleyen harcamayı görüp onaylayın. 5.000 TL altıysa işlem tamamdır.',
  },
  {
    title: 'Admin ile Giriş Yap',
    subtitle: 'test1 / pattabanoglu@devrimmehmet.com / Pass123!',
    icon: 'admin_panel_settings',
    color: 'purple',
    detail: '5.000 TL üstü harcamalarda Admin onay adımı aktif olur.',
  },
  {
    title: 'Bildirimleri Kontrol Et',
    subtitle: 'Her kullanıcı yalnızca kendi bildirimini görür',
    icon: 'notifications',
    color: 'primary',
    detail: 'Bildirimler sayfasında event kayıtlarını inceleyin.',
  },
  {
    title: 'E-postaları Doğrula',
    subtitle: 'http://localhost:8025 — Mailpit',
    icon: 'mark_email_read',
    color: 'cyan',
    detail: 'Gönderilen tüm e-postalar Mailpit üzerinde yakalanır.',
  },
];

const notificationTriggers = [
  {
    icon: 'add_circle',
    color: 'positive',
    event: 'Harcama Talebi Oluşturuldu',
    recipient: '→ Tüm HR ve Admin kullanıcılarına',
  },
  {
    icon: 'check_circle',
    color: 'positive',
    event: 'Harcama Onaylandı',
    recipient: '→ Talep sahibi personele',
  },
  {
    icon: 'cancel',
    color: 'negative',
    event: 'Harcama Reddedildi',
    recipient: '→ Talep sahibi personele',
  },
  {
    icon: 'shield',
    color: 'warning',
    event: 'Admin Onayı Gerekli (5.000 TL)',
    recipient: '→ Tüm Admin kullanıcılarına',
  },
];

// ── Mimari ─────────────────────────────────────────────────────────
const expenseServiceFeatures = [
  'JWT kimlik doğrulama (Keycloak OIDC + yerel)',
  'Çok kiracılı (multi-tenant) veri izolasyonu',
  'Outbox pattern ile güvenilir event yayınlama',
  'RabbitMQ üzerinden integration event gönderme',
  'Entity Framework Core 10 + PostgreSQL 16',
  'FluentValidation ile istek doğrulama',
  'BCrypt ile şifre hashleme',
  'Serilog structured logging',
  'OpenTelemetry tracing desteği',
  'TCMB döviz kuru entegrasyonu',
];

const notificationServiceFeatures = [
  'RabbitMQ consumer (event-driven mimari)',
  'MailKit ile SMTP e-posta gönderimi',
  'Per-alıcı bildirim kaydı (her alıcı ayrı satır)',
  'Dead-letter kuyruk yönetimi',
  'ProcessedMessage tablosu ile idempotency',
  'OpenTelemetry tracing desteği',
  "Admin dead-letter inceleme endpoint'i",
  'Serilog structured logging',
];

const communicationPatterns = [
  {
    icon: 'sync_alt',
    color: 'indigo-4',
    title: 'REST / HTTP',
    desc: 'Frontend ↔ API; servisler arası sorgular',
  },
  { icon: 'hub', color: 'orange', title: 'RabbitMQ', desc: 'Async event — Expense → Notification' },
  {
    icon: 'lock',
    color: 'orange-8',
    title: 'Keycloak OIDC',
    desc: 'Merkezi kimlik yönetimi & JWT',
  },
  { icon: 'timeline', color: 'blue-4', title: 'OpenTelemetry', desc: 'Dağıtık izleme — Jaeger' },
];

// ── Teknoloji yığını ───────────────────────────────────────────────
const techStacks = [
  {
    category: 'Backend',
    icon: 'dns',
    accent: '#818cf8',
    items: [
      '.NET 10',
      'ASP.NET Core',
      'EF Core 10',
      'FluentValidation',
      'BCrypt.Net',
      'Serilog',
      'Swashbuckle',
      'JWT Bearer',
    ],
  },
  {
    category: 'Frontend',
    icon: 'web',
    accent: '#2dd4bf',
    items: ['Vue 3', 'Quasar 2', 'Pinia', 'TypeScript', 'Vite', 'Axios'],
  },
  {
    category: 'Altyapı',
    icon: 'storage',
    accent: '#fb923c',
    items: ['PostgreSQL 16', 'RabbitMQ 3', 'Keycloak 25', 'MailKit', 'Mailpit', 'Docker Compose'],
  },
  {
    category: 'Gözlemlenebilirlik',
    icon: 'monitoring',
    accent: '#60a5fa',
    items: ['OpenTelemetry', 'Jaeger', 'Serilog', 'Health Checks', 'OTLP Exporter'],
  },
];

// ── ServiceCard bileşeni ───────────────────────────────────────────
const ServiceCard = {
  props: ['svc'],
  emits: ['open', 'copy'],
  setup(props: { svc: ServiceDef }, { emit }: { emit: (e: string, ...args: unknown[]) => void }) {
    return () => {
      const { svc } = props;
      const hasLink = svc.url.startsWith('http');

      return h(
        'div',
        {
          class: ['svc-card', hasLink ? 'svc-card--clickable' : ''],
          style: { '--accent': svc.accent },
          onClick: () => emit('open', svc),
        },
        [
          // Üst şerit
          h('div', { class: 'svc-card__header q-px-md q-py-sm row items-center' }, [
            h('div', { class: 'svc-icon-wrap q-mr-sm' }, [
              h('q-icon', { name: svc.icon, size: '18px', style: { color: svc.accent } }),
            ]),
            h('div', { class: 'col' }, [
              h('div', { class: 'text-caption text-weight-bold text-white' }, svc.name),
              h('div', { class: 'text-caption text-grey-6' }, svc.desc),
            ]),
            svc.port
              ? h(
                  'q-badge',
                  {
                    outline: true,
                    class: 'text-caption q-ml-sm',
                    style: { color: svc.accent, borderColor: `${svc.accent}66` },
                  },
                  () => `:${svc.port}`,
                )
              : null,
          ]),

          // URL satırı
          h('div', { class: 'q-px-md q-pt-sm q-pb-xs' }, [
            h('div', { class: 'url-row row items-center justify-between' }, [
              h('span', { class: 'text-caption text-mono url-text' }, svc.url),
              hasLink ? h('q-icon', { name: 'open_in_new', size: '14px', color: 'grey-6' }) : null,
            ]),
          ]),

          // Badge (giriş gerekmez)
          svc.badge
            ? h('div', { class: 'q-px-md q-pb-sm' }, [
                h(
                  'q-badge',
                  { color: svc.badgeColor ?? 'positive', class: 'text-caption' },
                  () => svc.badge,
                ),
              ])
            : null,

          // Kimlik bilgileri
          ...(svc.credentials?.length
            ? [
                h('q-separator', { dark: true, class: 'q-mx-md' }),
                h(
                  'div',
                  { class: 'q-pa-md' },
                  svc.credentials.map((cred) =>
                    h(
                      'div',
                      {
                        class: 'cred-row row items-center justify-between q-mb-xs',
                        key: cred.label,
                      },
                      [
                        h('div', { class: 'row items-center' }, [
                          h(
                            'span',
                            { class: 'text-caption text-grey-6 q-mr-xs' },
                            `${cred.label}:`,
                          ),
                          h('span', { class: 'text-caption text-mono text-grey-3' }, cred.value),
                        ]),
                        h(
                          'q-btn',
                          {
                            flat: true,
                            dense: true,
                            round: true,
                            icon: 'content_copy',
                            size: 'xs',
                            color: 'grey-6',
                            onClick: (e: MouseEvent) => {
                              e.stopPropagation();
                              emit('copy', cred.value);
                            },
                          },
                          () => h('q-tooltip', {}, () => 'Kopyala'),
                        ),
                      ],
                    ),
                  ),
                ),
              ]
            : [h('div', { class: 'q-pb-sm' })]),
        ],
      );
    };
  },
};

// ── SectionHeader bileşeni ─────────────────────────────────────────
const SectionHeader = {
  props: ['icon', 'title', 'subtitle'],
  setup(props: { icon: string; title: string; subtitle: string }) {
    return () =>
      h('div', { class: 'q-mb-lg' }, [
        h('div', { class: 'row items-center q-mb-xs' }, [
          h('q-icon', { name: props.icon, color: 'primary', size: '20px', class: 'q-mr-sm' }),
          h('span', { class: 'text-h6 text-weight-bold text-white' }, props.title),
        ]),
        h('div', { class: 'text-grey-5 text-caption q-ml-lg q-pl-xs' }, props.subtitle),
        h('q-separator', { dark: true, class: 'q-mt-sm' }),
      ]);
  },
};
</script>

<style scoped>
/* ── Genel ── */
.glass-card {
  background: rgba(19, 17, 28, 0.7);
  border: 1px solid rgba(99, 102, 241, 0.15);
  border-radius: 12px;
}
.h-full {
  height: 100%;
}

/* ── Marka ikonu ── */
.brand-icon-wrap {
  width: 52px;
  height: 52px;
  border-radius: 14px;
  background: linear-gradient(135deg, #6366f1, #8b5cf6);
  display: flex;
  align-items: center;
  justify-content: center;
  flex-shrink: 0;
  box-shadow: 0 4px 16px rgba(99, 102, 241, 0.4);
}

/* ── Servis kartı ── */
.svc-card {
  background: rgba(19, 17, 28, 0.75);
  border: 1px solid rgba(255, 255, 255, 0.07);
  border-radius: 14px;
  overflow: hidden;
  height: 100%;
  display: flex;
  flex-direction: column;
  transition:
    border-color 0.2s,
    box-shadow 0.2s,
    transform 0.15s;
  border-top: 2px solid var(--accent, #6366f1);
}
.svc-card--clickable {
  cursor: pointer;
}
.svc-card--clickable:hover {
  border-color: var(--accent, #6366f1);
  box-shadow:
    0 8px 28px rgba(0, 0, 0, 0.35),
    0 0 0 1px var(--accent, #6366f1);
  transform: translateY(-3px);
}

.svc-card__header {
  background: rgba(255, 255, 255, 0.03);
  border-bottom: 1px solid rgba(255, 255, 255, 0.05);
}

.svc-icon-wrap {
  width: 32px;
  height: 32px;
  border-radius: 8px;
  background: rgba(255, 255, 255, 0.06);
  display: flex;
  align-items: center;
  justify-content: center;
  flex-shrink: 0;
}

.url-row {
  background: rgba(255, 255, 255, 0.04);
  border: 1px solid rgba(255, 255, 255, 0.07);
  border-radius: 6px;
  padding: 4px 8px;
}
.url-text {
  font-family: 'Courier New', monospace;
  font-size: 0.7rem;
  color: var(--accent, #6366f1);
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
}

.cred-row {
  background: rgba(255, 255, 255, 0.03);
  border: 1px solid rgba(255, 255, 255, 0.05);
  border-radius: 5px;
  padding: 3px 8px;
}

/* ── Tenant kartı ── */
.tenant-card {
  background: rgba(19, 17, 28, 0.7);
  border: 1px solid rgba(255, 255, 255, 0.08);
  border-radius: 14px;
  overflow: hidden;
  height: 100%;
}
.tenant-header {
  min-height: 44px;
}

.account-row:not(:last-child) {
  padding-bottom: 10px;
  border-bottom: 1px solid rgba(255, 255, 255, 0.05);
  margin-bottom: 10px;
}

.cred-box {
  background: rgba(255, 255, 255, 0.04);
  border: 1px solid rgba(255, 255, 255, 0.06);
  border-radius: 6px;
}

/* ── Demo / kural blokları ── */
.rule-block {
  background: rgba(255, 255, 255, 0.04);
  border: 1px solid rgba(255, 255, 255, 0.07);
  border-radius: 8px;
}

.comm-block {
  background: rgba(255, 255, 255, 0.04);
  border: 1px solid rgba(255, 255, 255, 0.07);
  border-radius: 10px;
}

/* ── Teknoloji yığını ── */
.stack-card {
  background: rgba(19, 17, 28, 0.7);
  border: 1px solid rgba(255, 255, 255, 0.08);
  border-radius: 14px;
  height: 100%;
}
.stack-icon-wrap {
  width: 28px;
  height: 28px;
  border-radius: 7px;
  display: flex;
  align-items: center;
  justify-content: center;
  flex-shrink: 0;
}
.tech-chip {
  font-size: 0.68rem !important;
  height: 22px !important;
  background: transparent !important;
}

/* ── Mono font ── */
.text-mono {
  font-family: 'Courier New', monospace;
  font-size: 0.72rem;
}

.feature-item {
  line-height: 1.5;
}
</style>
