<template>
  <q-page class="q-pa-lg">
    <div class="text-h5 text-weight-bold q-mb-sm">Dokümantasyon & Servisler</div>
    <div class="text-grey-5 text-body2 q-mb-lg">
      Yerel ortamda çalışan tüm servisler ve doküman bağlantıları
    </div>

    <!-- Servisler -->
    <div class="text-h6 text-weight-bold q-mb-md">
      <q-icon name="dns" class="q-mr-sm" />Çalışan Servisler
    </div>
    <div class="row q-gutter-md q-mb-xl">
      <div
        v-for="svc in services"
        :key="svc.title"
        class="col-12 col-sm-6 col-md-4"
      >
        <div class="doc-link-card" @click="openUrl(svc.url)">
          <q-icon :name="svc.icon" :color="svc.color" class="doc-icon" />
          <div class="doc-title text-white">{{ svc.title }}</div>
          <div class="doc-desc">{{ svc.desc }}</div>
          <div class="text-primary text-caption q-mt-sm">{{ svc.url }}</div>
          
          <div v-if="svc.credentials" class="q-mt-sm text-caption text-grey-4 bg-dark q-pa-xs rounded-borders" style="border: 1px solid rgba(255,255,255,0.1)">
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

    <!-- Case Dokümanları -->
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

    <!-- Mimari -->
    <div class="text-h6 text-weight-bold q-mb-md">
      <q-icon name="architecture" class="q-mr-sm" />Mimari Genel Bakış
    </div>
    <div class="glass-card q-mb-xl">
      <q-list dark bordered separator class="rounded-borders">
        <q-expansion-item
          icon="api"
          label="Harcama Servisi (ExpenseService)"
          caption="Port 5001 - Ana API"
          header-class="text-primary text-weight-bold"
        >
          <q-card class="bg-transparent">
            <q-card-section>
              <q-list dark dense>
                <q-item v-for="ep in expenseEndpoints" :key="ep">
                  <q-item-section avatar>
                    <q-icon name="chevron_right" color="primary" size="xs" />
                  </q-item-section>
                  <q-item-section class="text-caption text-grey-4" style="font-family: monospace">{{ ep }}</q-item-section>
                </q-item>
              </q-list>
            </q-card-section>
          </q-card>
        </q-expansion-item>

        <q-expansion-item
          icon="notifications"
          label="Bildirim Servisi (NotificationService)"
          caption="Port 5002 - RabbitMQ Consumer"
          header-class="text-accent text-weight-bold"
        >
          <q-card class="bg-transparent">
            <q-card-section>
              <q-list dark dense>
                <q-item v-for="ep in notificationEndpoints" :key="ep">
                  <q-item-section avatar>
                    <q-icon name="chevron_right" color="accent" size="xs" />
                  </q-item-section>
                  <q-item-section class="text-caption text-grey-4" style="font-family: monospace">{{ ep }}</q-item-section>
                </q-item>
              </q-list>
            </q-card-section>
          </q-card>
        </q-expansion-item>
      </q-list>
    </div>
  </q-page>
</template>

<script setup lang="ts">
import { reactive, onMounted } from 'vue';
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

const services: ServiceLink[] = [
  {
    title: 'Harcama API Swagger',
    desc: 'Harcama servisi API dokümantasyonu',
    url: 'http://localhost:5001/swagger',
    icon: 'code',
    color: 'primary',
    healthKey: 'expense',
  },
  {
    title: 'Bildirim API Swagger',
    desc: 'Bildirim servisi API dokümantasyonu',
    url: 'http://localhost:5002/swagger',
    icon: 'notifications',
    color: 'accent',
    healthKey: 'notification',
  },
  {
    title: 'RabbitMQ Yönetimi',
    desc: 'Mesaj kuyruğu yönetim paneli',
    url: 'http://localhost:15673',
    icon: 'message',
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
    desc: 'Dağıtık izleme görselleştirme',
    url: 'http://localhost:16686',
    icon: 'timeline',
    color: 'secondary',
    credentials: 'Şifre gerektirmez',
  },
];

const documents = [
  { 
    title: '🏢 BR-1 & BR-2: Multi-Tenant & Rol Yapısı', 
    desc: 'Tenant izolasyonu ve statik rol yetkilendirmesi', 
    icon: 'corporate_fare',
    content: `
      <div style="padding: 10px; background: rgba(99, 102, 241, 0.1); border-left: 4px solid #6366f1; border-radius: 4px; margin-bottom: 12px;">
        <h4 style="margin: 0 0 8px 0; color: #8b5cf6; font-size: 16px;">Multi-Tenant İzolasyon</h4>
        <p style="margin: 0;">Her şirket (tenant) sadece kendi verisini görür. EF Core tarafında <strong>Global Query Filter</strong> ile <code>TenantId</code> bazlı veri yalıtımı sağlanır. JWT içinden okunan tenant bilgisine göre tüm sorgular otomatik filtrelenir.</p>
      </div>
      <h4 style="margin: 16px 0 8px 0; color: #fff; font-size: 14px;">Statik Roller</h4>
      <ul style="padding-left: 20px; margin: 0;">
        <li style="margin-bottom: 6px;"><strong>Admin:</strong> Tenant içindeki tüm verilere erişebilir, kullanıcıları yönetebilir ve tüm harcamaları görebilir.</li>
        <li style="margin-bottom: 6px;"><strong>HR (İnsan Kaynakları):</strong> Tenant'taki tüm harcamaları görebilir ve onay/red işlemi yapabilir.</li>
        <li style="margin-bottom: 0;"><strong>Personel:</strong> Sadece kendi harcamalarını görebilir ve yeni talep oluşturabilir.</li>
      </ul>
    `
  },
  { 
    title: '💸 BR-3 & BR-4: Harcama & Onay Akışı', 
    desc: 'Harcama kategorileri ve tutara bağlı kademeli onay', 
    icon: 'price_check',
    content: `
      <p style="margin-bottom: 12px;">Personel Seyahat, Malzeme, Eğitim ve Diğer kategorilerinde harcama oluşturabilir.</p>
      
      <div style="display: flex; gap: 10px; flex-wrap: wrap; margin-bottom: 16px;">
        <div style="background: rgba(34, 197, 94, 0.15); padding: 8px 12px; border-radius: 4px; border: 1px solid rgba(34, 197, 94, 0.3);">
          <div style="color: #4ade80; font-weight: bold; margin-bottom: 4px;">≤ 5.000 TL</div>
          <div style="font-size: 12px;">Sadece <strong>HR</strong> onayı gerekir.</div>
        </div>
        <div style="background: rgba(239, 68, 68, 0.15); padding: 8px 12px; border-radius: 4px; border: 1px solid rgba(239, 68, 68, 0.3);">
          <div style="color: #f87171; font-weight: bold; margin-bottom: 4px;">> 5.000 TL</div>
          <div style="font-size: 12px;">Önce <strong>HR</strong>, sonra <strong>Admin</strong> onayı gerekir.</div>
        </div>
      </div>
      
      <div style="padding: 10px; background: rgba(255, 255, 255, 0.05); border-radius: 4px;">
        <strong>Red İşlemi:</strong> Onay reddedildiğinde, reddeden kişinin mutlaka geçerli bir sebep (min 10 karakter) girmesi zorunludur.
      </div>
    `
  },
  { 
    title: '⚙️ TR: Mimari Teknik Gereksinimler', 
    desc: 'Mikroservisler, Onion Architecture ve Patternler', 
    icon: 'account_tree',
    content: `
      <ul style="padding-left: 20px; margin: 0; line-height: 1.6;">
        <li><strong>Mikroservis Mimarisi:</strong> <code>ExpenseService</code> (Ana API) ve <code>NotificationService</code> (Tüketici API) olarak Database-per-Service prensibiyle ikiye ayrılmıştır.</li>
        <li><strong>Onion Architecture:</strong> Proje Domain, Application, Infrastructure ve Api katmanlarına bölünmüştür.</li>
        <li><strong>CQRS & MediatR:</strong> Command (Yazma) ve Query (Okuma) işlemleri ayrıştırılmıştır.</li>
        <li><strong>Soft Delete:</strong> Hiçbir veri fiziksel silinmez. <code>IsDeleted</code> bayrağı ile işaretlenip Global Query Filter ile gizlenir.</li>
        <li><strong>Unit of Work & Generic Repository:</strong> Veritabanı işlemleri tek bir transaction üzerinden topluca yönetilir.</li>
      </ul>
    `
  },
  { 
    title: '📨 BR-5 & TB-1: Asenkron İletişim & Outbox Pattern', 
    desc: 'RabbitMQ entegrasyonu ve güvenli event fırlatma', 
    icon: 'move_to_inbox',
    content: `
      <div style="padding: 10px; background: rgba(234, 179, 8, 0.1); border-left: 4px solid #eab308; border-radius: 4px; margin-bottom: 16px;">
        <h4 style="margin: 0 0 8px 0; color: #facc15; font-size: 14px;">Event-Driven Architecture</h4>
        <p style="margin: 0; font-size: 13px;">Harcama Onaylandığında veya Reddedildiğinde doğrudan e-posta atılmaz. RabbitMQ'ya bir event (<code>ExpenseApprovedIntegrationEvent</code> vb.) gönderilir.</p>
      </div>

      <p style="margin-bottom: 8px;"><strong>Outbox Pattern (Quartz.NET):</strong></p>
      <p style="font-size: 13px; color: #cbd5e1; margin-bottom: 16px;">Veritabanı kaydı ile RabbitMQ'ya mesaj gönderme işleminin atomik olması için kullanılır. Mesaj önce veritabanındaki <code>OutboxMessages</code> tablosuna yazılır. Arka planda çalışan bir Quartz worker bu mesajları okuyup RabbitMQ'ya güvenli şekilde iletir.</p>

      <p style="margin-bottom: 8px;"><strong>Notification Service (MassTransit):</strong></p>
      <p style="font-size: 13px; color: #cbd5e1; margin: 0;">Bu ayrı servis RabbitMQ'yu dinler. Mesaj gelince SMTP (Mailpit) üzerinden e-postayı gönderir ve kendi veritabanına bildirim logunu kaydeder. Detaylı bilgiye ihtiyaç duyarsa senkron (HTTP) olarak ExpenseService'e sorgu atabilir.</p>
    `
  },
];

const expenseEndpoints = [
  'POST /api/auth/login',
  'GET  /api/admin/users',
  'POST /api/admin/users',
  'PUT  /api/admin/users/{id}/roles',
  'GET  /api/admin/outbox/dead-letters',
  'POST /api/expenses',
  'GET  /api/expenses',
  'GET  /api/expenses/{id}',
  'PUT  /api/expenses/{id}/submit',
  'PUT  /api/expenses/{id}/approve',
  'PUT  /api/expenses/{id}/reject',
  'DELETE /api/expenses/{id}',
  'GET  /health',
];

const notificationEndpoints = [
  'GET /api/notifications',
  'GET /api/notifications?tenantId={tenantId}',
  'GET /api/admin/notifications/dead-letters',
  'GET /health',
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
.doc-link-card {
  cursor: pointer;
  transition: all 0.3s ease;
}
.doc-link-card:hover {
  transform: translateY(-2px);
  border-color: rgba(99, 102, 241, 0.3);
}
</style>
