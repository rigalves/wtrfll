<template>
  <section class="mx-auto max-w-4xl px-6 py-16 space-y-8">
    <header class="space-y-2">
      <p class="text-xs uppercase tracking-[0.4em] text-slate-500">{{ t('newSession.tagline') }}</p>
      <h1 class="text-3xl font-semibold">{{ t('newSession.title') }}</h1>
      <p class="text-slate-400 text-base">{{ t('newSession.description') }}</p>
    </header>

    <form class="grid gap-4 rounded-2xl border border-white/10 bg-white/5 p-6" @submit.prevent="createSession">
      <div class="space-y-2">
        <label class="text-sm font-semibold text-slate-100" for="session-name">
          {{ t('newSession.form.nameLabel') }}
        </label>
        <input
          id="session-name"
          v-model="sessionName"
          type="text"
          class="w-full rounded-lg border border-white/15 bg-black/30 px-4 py-3 text-base placeholder:text-slate-500 focus:border-sky-400 focus:outline-none"
          :placeholder="t('newSession.form.namePlaceholder')"
          maxlength="160"
        />
      </div>

      <div class="space-y-2">
        <label class="text-sm font-semibold text-slate-100" for="session-scheduled">
          {{ t('newSession.form.scheduleLabel') }}
        </label>
        <input
          id="session-scheduled"
          v-model="scheduledAtLocal"
          type="datetime-local"
          class="w-full rounded-lg border border-white/15 bg-black/30 px-4 py-3 text-base text-slate-100 focus:border-sky-400 focus:outline-none"
        />
        <p class="text-xs text-slate-500">{{ t('newSession.form.scheduleHelp') }}</p>
      </div>

      <button
        type="submit"
        class="mt-2 inline-flex items-center justify-center rounded-lg bg-sky-500 px-5 py-3 font-semibold text-white shadow-lg shadow-sky-900/30 disabled:opacity-40"
        :disabled="isCreating"
      >
        {{ isCreating ? t('newSession.creating') : t('newSession.createButton') }}
      </button>
      <p v-if="errorMessageKey" class="text-sm text-rose-300">{{ t(errorMessageKey) }}</p>
    </form>

    <div v-if="session" class="space-y-4 rounded-2xl border border-white/10 bg-white/5 p-6">
      <div>
        <p class="text-sm text-slate-400">{{ t('newSession.sessionSummary.nameLabel') }}</p>
        <p class="text-2xl font-semibold">{{ session.name }}</p>
      </div>
      <div class="grid gap-4 md:grid-cols-2">
        <div>
          <p class="text-sm text-slate-400">{{ t('newSession.sessionSummary.shortCodeLabel') }}</p>
          <p class="text-2xl font-semibold tracking-widest">{{ session.shortCode }}</p>
        </div>
        <div>
          <p class="text-sm text-slate-400">{{ t('newSession.sessionSummary.scheduleLabel') }}</p>
          <p class="text-xl font-semibold">
            {{ scheduledAtLabel }}
          </p>
        </div>
      </div>
      <p class="text-xs text-slate-500">
        {{ t('newSession.sessionSummary.createdAt', { date: createdAtLabel }) }}
      </p>

      <div class="grid gap-4 md:grid-cols-2">
        <div class="rounded-xl border border-white/15 bg-black/20 p-4">
          <p class="text-sm uppercase tracking-[0.3em] text-slate-500">{{ t('newSession.controlLinkLabel') }}</p>
          <p class="text-sm text-slate-400">{{ t('newSession.controlLinkDescription') }}</p>
          <code class="mt-2 block break-all rounded bg-black/30 p-3 text-xs text-slate-200">
            {{ controllerLink }}
          </code>
          <button
            type="button"
            class="mt-3 rounded-lg border border-white/20 px-3 py-1 text-xs text-slate-100"
            @click="copyLink('controller')"
          >
            {{ copyStatus.controller ? t('newSession.copy.copied') : t('newSession.copy.copyButton') }}
          </button>
        </div>
        <div class="rounded-xl border border-white/15 bg-black/20 p-4">
          <p class="text-sm uppercase tracking-[0.3em] text-slate-500">{{ t('newSession.displayLinkLabel') }}</p>
          <p class="text-sm text-slate-400">{{ t('newSession.displayLinkDescription') }}</p>
          <code class="mt-2 block break-all rounded bg-black/30 p-3 text-xs text-slate-200">
            {{ displayLink }}
          </code>
          <button
            type="button"
            class="mt-3 rounded-lg border border-white/20 px-3 py-1 text-xs text-slate-100"
            @click="copyLink('display')"
          >
            {{ copyStatus.display ? t('newSession.copy.copied') : t('newSession.copy.copyButton') }}
          </button>
        </div>
      </div>
    </div>
  </section>
</template>

<script setup lang="ts">
import { computed, ref } from 'vue'
import { useI18n } from 'vue-i18n'

import { apiClient } from '@/lib/apiClient'
import { useUpcomingSessionsStore } from '@/stores/upcomingSessionsStore'
import type { paths } from '../../../shared/typescript/api'

type CreateSessionResponse = paths['/api/sessions']['post']['responses']['201']['content']['application/json']
type CreateSessionRequestBody =
  NonNullable<paths['/api/sessions']['post']['requestBody']> extends {
    content: { 'application/json': infer Body }
  }
    ? Body
    : Record<string, never>

const { t } = useI18n()
const isCreating = ref(false)
const errorMessageKey = ref<string | null>(null)
const session = ref<CreateSessionResponse | null>(null)
const copyStatus = ref({ controller: false, display: false })
const sessionName = ref('')
const scheduledAtLocal = ref('')
const upcomingSessionsStore = useUpcomingSessionsStore()

const controllerLink = computed(() => {
  if (!session.value) return ''
  return `${window.location.origin}/control/${session.value.id}?token=${session.value.controllerJoinToken}`
})

const displayLink = computed(() => {
  if (!session.value) return ''
  return `${window.location.origin}/display/${session.value.id}?token=${session.value.displayJoinToken}`
})

const createdAtLabel = computed(() =>
  session.value ? new Date(session.value.createdAt).toLocaleString() : '',
)

const scheduledAtLabel = computed(() => {
  if (!session.value) {
    return ''
  }
  const source = session.value.scheduledAt ?? session.value.createdAt
  return new Date(source).toLocaleString()
})

async function createSession() {
  isCreating.value = true
  errorMessageKey.value = null
  session.value = null
  try {
    const payload: CreateSessionRequestBody = {}
    const trimmedName = sessionName.value.trim()
    if (trimmedName) {
      payload.name = trimmedName
    }
    if (scheduledAtLocal.value) {
      const parsed = new Date(scheduledAtLocal.value)
      payload.scheduledAt = parsed.toISOString()
    }

    const response = await apiClient.POST('/api/sessions', {
      body: payload,
    })
    if (response.error || !response.data) {
      errorMessageKey.value = 'newSession.errors.createFailed'
      session.value = null
      return
    }
    session.value = response.data
    upcomingSessionsStore.registerCreatedSession(response.data)
  } catch {
    errorMessageKey.value = 'newSession.errors.unexpected'
    session.value = null
  } finally {
    isCreating.value = false
  }
}

async function copyLink(kind: 'controller' | 'display') {
  const link = kind === 'controller' ? controllerLink.value : displayLink.value
  if (!link) {
    return
  }
  try {
    await navigator.clipboard.writeText(link)
    copyStatus.value[kind] = true
    window.setTimeout(() => {
      copyStatus.value[kind] = false
    }, 2000)
  } catch {
    // ignore clipboard failures
  }
}
</script>
