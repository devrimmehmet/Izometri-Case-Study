<template>
  <span class="copy-value">
    <span class="copy-value__text">{{ value || '-' }}</span>
    <q-btn
      v-if="value"
      flat
      round
      dense
      size="sm"
      icon="content_copy"
      class="copy-value__button"
      @click.stop="copy"
    >
      <q-tooltip>{{ tooltip }}</q-tooltip>
    </q-btn>
  </span>
</template>

<script setup lang="ts">
import { copyToClipboard, useQuasar } from 'quasar';

const props = withDefaults(
  defineProps<{
    value?: string | null;
    tooltip?: string;
  }>(),
  {
    value: '',
    tooltip: 'Kopyala',
  },
);

const $q = useQuasar();

function copy() {
  if (!props.value) return;
  void copyToClipboard(props.value);
  $q.notify({ type: 'positive', message: 'Kopyalandı' });
}
</script>

<style scoped>
.copy-value {
  align-items: center;
  display: inline-flex;
  gap: 4px;
  max-width: 100%;
}

.copy-value__text {
  display: inline-block;
  max-width: min(220px, 52vw);
  overflow: hidden;
  text-overflow: ellipsis;
  vertical-align: middle;
  white-space: nowrap;
}

.copy-value__button {
  flex: 0 0 auto;
}
</style>
