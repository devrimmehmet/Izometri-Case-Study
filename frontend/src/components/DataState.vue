<template>
  <div :class="['data-state', dense ? 'data-state--dense' : '']">
    <q-icon :name="icon" :color="color" :size="dense ? '22px' : '32px'" />
    <div>
      <div class="data-state__title">{{ title }}</div>
      <div v-if="message" class="data-state__message">{{ message }}</div>
    </div>
    <q-btn
      v-if="retryLabel"
      flat
      dense
      color="primary"
      icon="refresh"
      :label="retryLabel"
      @click="$emit('retry')"
    />
  </div>
</template>

<script setup lang="ts">
withDefaults(
  defineProps<{
    title: string;
    message?: string;
    icon?: string;
    color?: string;
    retryLabel?: string;
    dense?: boolean;
  }>(),
  {
    message: '',
    icon: 'info',
    color: 'grey-5',
    retryLabel: '',
    dense: false,
  },
);

defineEmits<{
  retry: [];
}>();
</script>

<style scoped>
.data-state {
  align-items: center;
  color: #d1d5db;
  display: flex;
  gap: 12px;
  justify-content: center;
  min-height: 120px;
  padding: 20px;
  text-align: left;
}

.data-state--dense {
  min-height: 72px;
  padding: 12px;
}

.data-state__title {
  font-weight: 600;
}

.data-state__message {
  color: #9ca3af;
  font-size: 12px;
  margin-top: 2px;
}
</style>
