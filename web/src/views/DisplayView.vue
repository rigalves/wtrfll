<template>
  <main class="flex min-h-screen flex-col bg-black text-white">
    <section class="flex flex-1 flex-col items-center justify-center gap-6 px-8 text-center">
      <p class="text-sm uppercase tracking-[0.4em] text-slate-500">{{ sessionLabel }}</p>
      <div v-if="joinErrorText" class="rounded-xl border border-rose-300/40 bg-rose-500/15 p-4 text-rose-200">
        {{ joinErrorText }}
      </div>
      <div v-else-if="sessionParticipationStore.joinState !== 'joined'" class="text-slate-400">
        {{ t('display.waiting') }}
      </div>
      <template v-else>
        <p class="text-5xl font-semibold leading-tight">{{ displayViewModel.reference }}</p>
        <div class="space-y-4 text-2xl">
          <p v-for="verse in displayViewModel.verses" :key="verse.verse" class="leading-snug">
            <span class="text-slate-400 mr-2">{{ verse.verse }}</span>
            {{ verse.text }}
          </p>
          <p v-if="displayViewModel.isLoading" class="text-slate-500">{{ t('display.loading') }}</p>
        </div>
        <p v-if="displayViewModel.attribution" class="text-sm text-slate-400">{{ displayViewModel.attribution }}</p>
      </template>
    </section>
  </main>
</template>

<script setup lang="ts">
import { computed, watch } from 'vue'
import { storeToRefs } from 'pinia'
import { useI18n } from 'vue-i18n'
import { useRoute } from 'vue-router'

import { useSessionStore } from '@/stores/sessionStore'
import { useSessionParticipationStore } from '@/stores/sessionParticipationStore'

interface Props {
  sessionId: string
}

const props = defineProps<Props>()
const route = useRoute()
const { t } = useI18n()
const sessionStore = useSessionStore()
const sessionParticipationStore = useSessionParticipationStore()
const { displayViewModel } = storeToRefs(sessionStore)

const joinToken = computed(() => (typeof route.query.token === 'string' ? route.query.token : ''))
const sessionLabel = computed(() =>
  t('display.sessionLabel', { code: sessionParticipationStore.activeSessionId ?? 'demo' }),
)

const joinErrorText = computed(() =>
  sessionParticipationStore.joinErrorMessage
    ? t(sessionParticipationStore.joinErrorMessage.key, sessionParticipationStore.joinErrorMessage.params ?? {})
    : null,
)

watch(
  () => [props.sessionId, joinToken.value],
  ([sessionId, token]) => {
    if (!sessionId || !token) {
      sessionParticipationStore.setJoinError('session.joinErrors.missingToken')
      return
    }
    void sessionParticipationStore.joinSession({ sessionId, joinToken: token, role: 'display' })
  },
  { immediate: true },
)
</script>
