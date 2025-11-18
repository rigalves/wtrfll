<template>
  <div class="min-h-screen bg-surface text-textPrimary">
    <header class="border-b border-white/10 bg-black/30">
      <div class="mx-auto flex max-w-6xl items-center justify-between px-6 py-4">
        <RouterLink to="/" class="text-lg font-semibold tracking-wide">wtrfll</RouterLink>
        <div class="flex items-center gap-6 text-sm text-slate-300">
          <nav class="flex items-center gap-4">
            <RouterLink class="hover:text-white" to="/">{{ t('app.nav.home') }}</RouterLink>
            <RouterLink class="hover:text-white" :to="{ name: 'new-session' }">
              {{ t('app.nav.newSession') }}
            </RouterLink>
          </nav>
          <label class="flex items-center gap-2 text-xs uppercase tracking-[0.3em] text-slate-500">
            {{ t('app.language.label') }}
            <select
              class="rounded border border-white/10 bg-black/40 px-2 py-1 text-slate-200"
              :value="currentLocale"
              @change="onLocaleChange($event)"
            >
              <option value="en">EN</option>
              <option value="es">ES</option>
            </select>
          </label>
        </div>
      </div>
    </header>

    <section
      v-if="bibleBooksStore.loadState !== 'loaded'"
      class="flex flex-col items-center justify-center gap-4 px-6 py-24 text-center text-slate-300"
    >
      <span class="inline-flex h-16 w-16 animate-spin rounded-full border-4 border-white/20 border-t-sky-400"></span>
      <p class="text-xl font-semibold text-white">{{ t('app.catalog.loadingTitle') }}</p>
      <p class="max-w-xl text-sm text-slate-400">
        {{ t('app.catalog.loadingDescription') }}
        <span v-if="bibleBooksStore.loadState === 'error'">{{ t('app.catalog.retrying') }}</span>
      </p>
    </section>

    <RouterView v-else />
  </div>
</template>

<script setup lang="ts">
import { onMounted, watch } from 'vue'
import { useI18n } from 'vue-i18n'
import { RouterLink, RouterView } from 'vue-router'

import { setLocale } from '@/lib/i18n'
import { useBibleBooksStore } from '@/stores/bibleBooksStore'

const bibleBooksStore = useBibleBooksStore()
const RETRY_DELAY_MS = 3000
const { t, locale } = useI18n()
const currentLocale = locale

function triggerLoad() {
  bibleBooksStore.ensureLoaded().catch(() => {
    // auto retry handled below
  })
}

function onLocaleChange(event: Event) {
  const target = event.target as HTMLSelectElement | null
  if (!target) {
    return
  }
  setLocale(target.value)
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
