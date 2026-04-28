<template>
  <div class="mermaid-container">
    <div ref="diagram" class="mermaid">
      <slot></slot>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted, watch, nextTick } from 'vue';
import mermaid from 'mermaid';

const props = defineProps({
  code: {
    type: String,
    required: false,
    default: ''
  }
});

const diagram = ref<HTMLElement | null>(null);

const renderDiagram = async () => {
  if (!diagram.value) return;

  try {
    // Clear previous content
    diagram.value.removeAttribute('data-processed');
    
    // If code prop is provided, use it; otherwise use slot content
    const content = props.code || diagram.value.textContent || '';
    
    if (!content.trim()) return;

    // Initialize mermaid with dark theme to match our aesthetic
    mermaid.initialize({
      startOnLoad: false,
      theme: 'dark',
      securityLevel: 'loose',
      fontFamily: 'Inter, system-ui, sans-serif'
    });

    const { svg } = await mermaid.render(`mermaid-${Math.random().toString(36).substr(2, 9)}`, content);
    diagram.value.innerHTML = svg;
  } catch (error) {
    console.error('Mermaid rendering failed:', error);
  }
};

onMounted(() => {
  void renderDiagram();
});

watch(() => props.code, () => {
  void nextTick(() => renderDiagram());
});
</script>

<style scoped>
.mermaid-container {
  display: flex;
  justify-content: center;
  align-items: center;
  width: 100%;
  overflow-x: auto;
  padding: 1rem;
  background: rgba(15, 23, 42, 0.4);
  border-radius: 12px;
  border: 1px solid rgba(255, 255, 255, 0.05);
}

.mermaid :deep(svg) {
  max-width: 100%;
  height: auto;
}
</style>
