<template>
  <q-page class="flex flex-center">
    <div class="text-grey-5">Yönlendiriliyorsunuz...</div>
  </q-page>
</template>

<script setup lang="ts">
import { onMounted } from 'vue';
import { useRouter } from 'vue-router';

const router = useRouter();
onMounted(() => {
  void router.push('/dashboard');
});
</script>
