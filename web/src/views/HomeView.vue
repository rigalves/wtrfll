<template>
  <section class="mx-auto max-w-5xl px-6 py-16">
    <div class="space-y-6">
      <p class="text-sm uppercase tracking-[0.25em] text-slate-500">{{ t('home.tagline') }}</p>
      <h1 class="text-4xl font-semibold">{{ t('home.title') }}</h1>
      <p class="text-base text-slate-300">{{ t('home.description') }}</p>

      <RouterLink
        class="inline-flex items-center rounded-xl bg-sky-500 px-6 py-3 font-semibold text-white shadow-lg shadow-sky-900/30"
        :to="{ name: 'new-session' }"
      >
        {{ t('home.cta') }}
      </RouterLink>

      <div class="rounded-2xl border border-white/10 bg-white/5 p-6 text-sm text-slate-200">
        <p class="font-semibold">{{ t('home.howItWorks') }}</p>
        <ol class="space-y-2 text-slate-300">
          <li v-for="(step, index) in steps" :key="index">{{ index + 1 }}. {{ step }}</li>
        </ol>
      </div>
    </div>
  </section>
</template>

<script setup lang="ts">
import { computed } from 'vue'
import { useI18n } from 'vue-i18n'
import { RouterLink } from 'vue-router'

const { t, tm } = useI18n()
const steps = computed(() => tm('home.steps') as string[])
</script>
