<template>
  <section class="mx-auto max-w-6xl px-6 py-16 space-y-10">
    <header class="space-y-3">
      <p class="text-sm uppercase tracking-[0.4em] text-slate-500">{{ t('landing.tagline') }}</p>
      <h1 class="text-4xl font-semibold">{{ t('landing.title') }}</h1>
      <p class="text-base text-slate-300">{{ t('landing.description') }}</p>
    </header>

    <div class="grid gap-8 md:grid-cols-2">
      <article
        v-for="card in roleCards"
        :key="card.role"
        class="rounded-3xl border border-white/10 bg-white/5 p-6 shadow-xl shadow-black/30"
      >
        <header class="space-y-2">
          <p class="text-xs uppercase tracking-[0.4em] text-slate-500">{{ card.roleLabel }}</p>
          <h2 class="text-2xl font-semibold">{{ card.title }}</h2>
          <p class="text-sm text-slate-300">{{ card.description }}</p>
        </header>

        <RouterLink
          v-if="card.primaryAction"
          class="mt-4 inline-flex items-center rounded-xl bg-sky-500 px-4 py-2 text-sm font-semibold text-white shadow-lg shadow-sky-900/30"
          :to="card.primaryAction"
        >
          {{ card.primaryActionLabel }}
        </RouterLink>

        <div class="mt-6 space-y-3">
          <p v-if="isLoading" class="text-sm text-slate-400">{{ t('landing.status.loading') }}</p>
          <p v-else-if="errorMessageKey" class="text-sm text-rose-300">
            {{ t(errorMessageKey) }}
          </p>
          <p v-else-if="!orderedSessions.length" class="text-sm text-slate-400">
            {{ card.emptyLabel }}
          </p>

          <ul v-else class="space-y-3">
            <li
              v-for="session in orderedSessions"
              :key="session.id"
              class="flex flex-col gap-3 rounded-2xl border border-white/10 bg-black/30 p-4 text-sm text-slate-200 md:flex-row md:items-center md:justify-between"
            >
              <div>
                <p class="text-base font-semibold text-white">{{ session.name }}</p>
                <p class="text-xs uppercase tracking-[0.3em] text-slate-500">
                  {{ t('landing.shortCodeLabel', { code: session.shortCode }) }}
                </p>
                <p class="text-xs text-slate-400">
                  {{ formatSessionSchedule(session) }}
                </p>
              </div>
              <div class="flex flex-col gap-2 md:flex-row">
                <button
                  type="button"
                  class="rounded-lg border border-white/20 px-4 py-2 text-sm font-semibold text-white hover:border-sky-400 hover:text-sky-100"
                  @click="openSession(session, card.role)"
                >
                  {{ card.buttonLabel }}
                </button>
              </div>
            </li>
          </ul>
        </div>
      </article>
    </div>
  </section>
</template>

<script setup lang="ts">
import { computed, onMounted } from 'vue'
import { useI18n } from 'vue-i18n'
import { RouterLink, useRouter } from 'vue-router'
import { storeToRefs } from 'pinia'

import { useUpcomingSessionsStore } from '@/stores/upcomingSessionsStore'
import { toUtcDate } from '@/lib/dateHelpers'

const { t } = useI18n()
const router = useRouter()
const upcomingSessionsStore = useUpcomingSessionsStore()
const { orderedSessions, isLoading, loadErrorMessageKey } = storeToRefs(upcomingSessionsStore)

const roleCards = computed(() => [
  {
    role: 'controller' as const,
    roleLabel: t('landing.roles.controller.roleLabel'),
    title: t('landing.roles.controller.title'),
    description: t('landing.roles.controller.description'),
    buttonLabel: t('landing.roles.controller.button'),
    emptyLabel: t('landing.roles.controller.empty'),
    primaryAction: { name: 'new-session' },
    primaryActionLabel: t('landing.roles.controller.primaryActionLabel'),
  },
  {
    role: 'display' as const,
    roleLabel: t('landing.roles.display.roleLabel'),
    title: t('landing.roles.display.title'),
    description: t('landing.roles.display.description'),
    buttonLabel: t('landing.roles.display.button'),
    emptyLabel: t('landing.roles.display.empty'),
    primaryAction: null,
    primaryActionLabel: '',
  },
])

const errorMessageKey = computed(() => loadErrorMessageKey.value)

onMounted(() => {
  upcomingSessionsStore.ensureLoaded()
})

function formatSessionSchedule(session: { scheduledAt?: string | null; createdAt: string }) {
  const timestamp = session.scheduledAt ?? session.createdAt
  return toUtcDate(timestamp).toLocaleString(undefined, {
    dateStyle: 'medium',
    timeStyle: 'short',
  })
}

function openSession(
  session: { id: string; controllerJoinToken: string; displayJoinToken: string },
  role: 'controller' | 'display',
) {
  const routeName = role === 'controller' ? 'control' : 'display'
  const token = role === 'controller' ? session.controllerJoinToken : session.displayJoinToken
  router.push({ name: routeName, params: { sessionId: session.id }, query: { token } })
}
</script>
