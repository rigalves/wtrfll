<template>
  <div class="min-h-screen bg-surface text-textPrimary">
    <header class="border-b border-white/10 bg-black/30">
      <div class="mx-auto flex max-w-6xl items-center justify-between px-6 py-4">
        <RouterLink to="/" class="text-lg font-semibold tracking-wide">wtrfll</RouterLink>
        <nav class="flex items-center gap-4 text-sm text-slate-300">
          <RouterLink class="hover:text-white" to="/">Inicio</RouterLink>
          <RouterLink class="hover:text-white" :to="{ name: 'new-session' }">Crear sesión</RouterLink>
        </nav>
      </div>
    </header>

    <section
      v-if="bibleBooksStore.loadState !== 'loaded'"
      class="flex flex-col items-center justify-center gap-4 px-6 py-24 text-center text-slate-300"
    >
      <span class="inline-flex h-16 w-16 animate-spin rounded-full border-4 border-white/20 border-t-sky-400"></span>
      <p class="text-xl font-semibold text-white">Cargando catálogos…</p>
      <p class="max-w-xl text-sm text-slate-400">
        Esperando al backend para obtener los libros bíblicos.
        <span v-if="bibleBooksStore.loadState === 'error'">
          Reintentando automáticamente en unos segundos…
        </span>
      </p>
    </section>

    <RouterView v-else />
  </div>
</template>

<script setup lang="ts">
import { onMounted, watch } from 'vue'
import { RouterLink, RouterView } from 'vue-router'

import { useBibleBooksStore } from '@/stores/bibleBooksStore'

const bibleBooksStore = useBibleBooksStore()
const RETRY_DELAY_MS = 3000

function triggerLoad() {
  bibleBooksStore.ensureLoaded().catch(() => {
    // let the watcher schedule retries
  })
}

onMounted(() => {
  if (bibleBooksStore.loadState === 'idle') {
    triggerLoad()
  }
})

watch(
  () => bibleBooksStore.loadState,
  (state) => {
    if (state === 'error') {
      window.setTimeout(triggerLoad, RETRY_DELAY_MS)
    }
  },
)
</script>
