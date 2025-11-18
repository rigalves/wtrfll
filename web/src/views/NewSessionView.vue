<template>
  <section class="mx-auto max-w-4xl px-6 py-16 space-y-6">
    <header class="space-y-2">
      <p class="text-xs uppercase tracking-[0.4em] text-slate-500">{{ t('newSession.tagline') }}</p>
      <h1 class="text-3xl font-semibold">{{ t('newSession.title') }}</h1>
      <p class="text-slate-400 text-base">{{ t('newSession.description') }}</p>
    </header>

    <div class="rounded-2xl border border-white/10 bg-white/5 p-6 space-y-4">
      <button
        type="button"
        class="rounded-lg bg-sky-500 px-5 py-3 font-semibold text-white disabled:opacity-40"
        :disabled="isCreating"
        @click="createSession"
      >
        {{ isCreating ? t('newSession.creating') : t('newSession.createButton') }}
      </button>
      <p v-if="errorMessageKey" class="text-sm text-rose-300">{{ t(errorMessageKey) }}</p>

      <div v-if="session" class="space-y-4">
        <div>
          <p class="text-sm text-slate-400">{{ t('newSession.shortCode') }}</p>
          <p class="text-2xl font-semibold">{{ session.shortCode }}</p>
          <p class="text-xs text-slate-500">
            {{ t('newSession.createdAt', { date: createdAtLabel }) }}
          </p>
        </div>

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
    </div>
  </section>
</template>

<script setup lang="ts">
import { computed, ref } from 'vue'
import { useI18n } from 'vue-i18n'

import { apiClient } from '@/lib/apiClient'
import type { paths } from '../../../shared/typescript/api'

type CreateSessionResponse = paths['/api/sessions']['post']['responses']['201']['content']['application/json']

const { t } = useI18n()
const isCreating = ref(false)
const errorMessageKey = ref<string | null>(null)
const session = ref<CreateSessionResponse | null>(null)
const copyStatus = ref({ controller: false, display: false })

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

async function createSession() {
  isCreating.value = true
  errorMessageKey.value = null
  try {
    const response = await apiClient.POST('/api/sessions', {})
    if (response.error) {
      errorMessageKey.value = 'newSession.errors.createFailed'
      session.value = null
      return
    }
    session.value = response.data ?? null
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
