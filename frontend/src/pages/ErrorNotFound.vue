<template>
  <div class="gate-bg flex flex-center fullscreen">
    <div class="text-center">
      <div class="text-h1 text-weight-bold text-primary q-mb-md" style="font-size: 80px; opacity: 0.7;">
        404
      </div>
      <div class="text-h6 text-grey-4 q-mb-lg">Sayfa bulunamadı</div>
      <q-btn
        color="primary"
        label="Ana Sayfaya Dön"
        rounded
        unelevated
        @click="$router.push('/')"
      />
    </div>
  </div>
</template>
