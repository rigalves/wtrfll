<template>
  <main class="relative flex min-h-screen flex-col bg-black text-white">
    <section class="flex flex-1 flex-col items-center justify-center gap-6 px-8 text-center">
      <div v-if="joinErrorText" class="rounded-xl border border-rose-300/40 bg-rose-500/15 p-4 text-rose-200">
        {{ joinErrorText }}
      </div>
      <div v-else-if="sessionParticipationStore.joinState !== 'joined'" class="text-slate-400">
        {{ t('display.waiting') }}
      </div>
      <template v-else>
        <div v-if="displayCommand === 'black'" class="h-full w-full bg-black" />
        <div v-else-if="displayCommand === 'clear'" class="text-slate-500">{{ t('display.cleared') }}</div>
        <template v-else-if="lyricsDisplayState">
          <LyricsRenderer
            class="space-y-4"
            :title="lyricsDisplayState.title"
            :author="lyricsDisplayState.author"
            :lines="lyricsDisplayState.lines"
            :fontScale="lyricsDisplayState.fontScale ?? passageFontScale"
            :columnCount="lyricsDisplayState.columnCount ?? 1"
            :emptyMessage="t('display.loading')"
          />
        </template>
        <template v-else>
          <ScriptureRenderer
            class="space-y-4"
            :reference="displayViewModel.reference"
            :verses="displayViewModel.verses"
            :currentIndex="displayViewModel.currentIndex ?? 0"
            :fontScale="passageFontScale"
            :showVerseNumbers="true"
            :emptyMessage="t('display.loading')"
          />
          <p v-if="displayViewModel.attribution" class="text-sm text-slate-400">{{ displayViewModel.attribution }}</p>
        </template>
      </template>
    </section>
    <div
      v-if="sessionBadge"
      class="pointer-events-none absolute left-6 top-6 text-xs uppercase tracking-[0.3em] text-slate-600"
    >
      {{ sessionBadge }}
    </div>
  </main>
</template>

<script setup lang="ts">
import { computed, onBeforeUnmount, ref, watch } from 'vue'
import { storeToRefs } from 'pinia'
import { useI18n } from 'vue-i18n'
import { useRoute } from 'vue-router'

import { useSessionStore } from '@/stores/sessionStore'
import { useSessionParticipationStore } from '@/stores/sessionParticipationStore'
import { extractChordProLines } from '@/lib/chordPro'
import LyricsRenderer from '@/components/LyricsRenderer.vue'
import ScriptureRenderer from '@/components/ScriptureRenderer.vue'

interface Props {
  sessionId: string
}

const props = defineProps<Props>()
const route = useRoute()
const { t } = useI18n()
const sessionStore = useSessionStore()
const sessionParticipationStore = useSessionParticipationStore()
const { displayViewModel, lyricsViewModel, presentationOptions } = storeToRefs(sessionStore)
const wakeLock = ref<WakeLockSentinel | null>(null)
type WakeLockNavigator = Navigator & {
  wakeLock?: {
    request: (type: 'screen') => Promise<WakeLockSentinel>
  }
}
const wakeLockSupported = typeof navigator !== 'undefined' && 'wakeLock' in navigator

const joinToken = computed(() => {
  if (typeof route.query.token === 'string') {
    return route.query.token
  }
  return (
    sessionParticipationStore.getStoredJoinToken(props.sessionId, 'display') ??
    ''
  )
})
const displayCommand = computed(() => displayViewModel.value.displayCommand ?? 'normal')
const lyricsDisplayState = computed(() => {
  const state = lyricsViewModel.value
  if (!state) {
    return null
  }
  const lines = Array.isArray(state.lines) ? state.lines : extractChordProLines((state as any).lyricsChordPro ?? '')
  return {
    title: state.title ?? '',
    author: state.author ?? '',
    lines,
    fontScale: (state as any).fontScale ?? presentationOptions.value.fontScale ?? 1,
    columnCount: (state as any).columnCount ?? 1,
  }
})
const passageFontScale = computed(() => displayViewModel.value.options?.fontScale ?? presentationOptions.value.fontScale ?? 1)

const joinErrorText = computed(() =>
  sessionParticipationStore.joinErrorMessage
    ? t(sessionParticipationStore.joinErrorMessage.key, sessionParticipationStore.joinErrorMessage.params ?? {})
    : null,
)

const sessionBadge = computed(() => {
  const name = sessionParticipationStore.activeSessionName
  const code = sessionParticipationStore.activeSessionShortCode
  if (name && code) {
    return t('display.sessionBadge', { name, code })
  }
  if (code) {
    return t('display.sessionBadgeFallback', { code })
  }
  if (sessionParticipationStore.activeSessionId) {
    const fallback = sessionParticipationStore.activeSessionId.slice(-6).toUpperCase()
    return t('display.sessionBadgeFallback', { code: fallback })
  }
  return null
})

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

watch(
  () => sessionParticipationStore.joinState,
  (state) => {
    if (state === 'joined') {
      requestWakeLock()
    } else {
      releaseWakeLock()
    }
  },
)

document.addEventListener('visibilitychange', () => {
  if (document.visibilityState === 'visible' && sessionParticipationStore.joinState === 'joined') {
    requestWakeLock()
  }
})

onBeforeUnmount(() => {
  releaseWakeLock()
})

async function requestWakeLock() {
  if (!wakeLockSupported) {
    return
  }
  try {
    const nav = navigator as WakeLockNavigator
    const sentinel = await nav.wakeLock?.request('screen')
    if (!sentinel) {
      return
    }
    wakeLock.value = sentinel
    sentinel.addEventListener('release', () => {
      wakeLock.value = null
    })
  } catch {
    wakeLock.value = null
  }
}

function releaseWakeLock() {
  wakeLock.value?.release().catch(() => {})
  wakeLock.value = null
}
</script>
