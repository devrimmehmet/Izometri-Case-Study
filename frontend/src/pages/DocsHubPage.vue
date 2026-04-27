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
          <q-badge
            v-if="healthStatus[svc.healthKey ?? '']"
            :color="healthStatus[svc.healthKey ?? ''] === 'ok' ? 'positive' : 'negative'"
            class="q-mt-sm"
          >
            {{ healthStatus[svc.healthKey ?? ''] === 'ok' ? '● Çevrimiçi' : '● Çevrimdışı' }}
          </q-badge>
        </div>
      </div>
    </div>

    <!-- Case Dokümanları -->
    <div class="text-h6 text-weight-bold q-mb-md">
      <q-icon name="description" class="q-mr-sm" />Case Dokümanları
    </div>
    <div class="row q-gutter-md q-mb-xl">
      <div
        v-for="doc in documents"
        :key="doc.title"
        class="col-12 col-sm-6 col-md-4"
      >
        <div class="doc-link-card">
          <q-icon name="article" color="secondary" class="doc-icon" />
          <div class="doc-title text-white">{{ doc.title }}</div>
          <div class="doc-desc">{{ doc.desc }}</div>
        </div>
      </div>
    </div>

    <!-- Mimari -->
    <div class="text-h6 text-weight-bold q-mb-md">
      <q-icon name="architecture" class="q-mr-sm" />Mimari Genel Bakış
    </div>
    <div class="glass-card q-pa-lg">
      <div class="row q-gutter-lg">
        <div class="col-12 col-md-6">
          <div class="text-weight-bold q-mb-sm text-primary">Harcama Servisi (ExpenseService)</div>
          <q-list dark dense>
            <q-item v-for="ep in expenseEndpoints" :key="ep">
              <q-item-section avatar>
                <q-icon name="api" color="primary" size="xs" />
              </q-item-section>
              <q-item-section class="text-caption">{{ ep }}</q-item-section>
            </q-item>
          </q-list>
        </div>
        <div class="col-12 col-md-6">
          <div class="text-weight-bold q-mb-sm text-accent">Bildirim Servisi (NotificationService)</div>
          <q-list dark dense>
            <q-item v-for="ep in notificationEndpoints" :key="ep">
              <q-item-section avatar>
                <q-icon name="api" color="accent" size="xs" />
              </q-item-section>
              <q-item-section class="text-caption">{{ ep }}</q-item-section>
            </q-item>
          </q-list>
        </div>
      </div>
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
  },
  {
    title: 'Mailpit',
    desc: 'Yerel e-posta test arayüzü',
    url: 'http://localhost:8025',
    icon: 'email',
    color: 'info',
  },
  {
    title: 'Jaeger İzleme',
    desc: 'Dağıtık izleme görselleştirme',
    url: 'http://localhost:16686',
    icon: 'timeline',
    color: 'secondary',
  },
];

const documents = [
  { title: 'Case Study', desc: 'Backend case gereksinimleri' },
  { title: 'Teslimat Özeti', desc: 'Teslim edilen dosya ve modüller' },
  { title: 'Mimari Topoloji', desc: 'Servis, katman ve iletişim akışı' },
  { title: 'API Deneme Rehberi', desc: 'Adım adım test senaryoları' },
  { title: 'Migration & Seed', desc: 'Veritabanı ve başlangıç verisi' },
  { title: 'Gereksinim Matrisi', desc: 'İş kuralı / Teknik gereksinim uyumu' },
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
