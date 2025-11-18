<template>
  <section class="mx-auto max-w-6xl px-6 py-12 space-y-10">
    <header class="flex flex-col gap-2">
      <p class="text-xs uppercase tracking-[0.4em] text-slate-500">{{ sessionLabel }}</p>
      <h1 class="text-3xl font-semibold">{{ t('controller.title') }}</h1>
      <p class="text-slate-300">
        {{ t('controller.headerDescription') }}
      </p>
    </header>

    <div v-if="joinErrorText" class="rounded-xl border border-rose-300/40 bg-rose-500/15 p-4 text-rose-200">
      {{ joinErrorText }}
    </div>
    <div
      v-else-if="sessionParticipationStore.joinState !== 'joined'"
      class="rounded-xl border border-amber-300/30 bg-amber-500/10 p-4 text-amber-100"
    >
      {{ t('controller.waitingConnection') }}
    </div>

    <div
      v-if="translationNoticeText"
      class="rounded-xl border border-sky-300/30 bg-sky-500/10 p-4 text-sky-100"
    >
      {{ translationNoticeText }}
    </div>

    <div class="grid gap-6 lg:grid-cols-[2fr,1fr]">
      <div class="space-y-5 rounded-2xl border border-white/10 bg-white/5 p-6 shadow-xl shadow-black/30">
        <label class="block text-sm font-semibold">{{ t('controller.referenceLabel') }}</label>
        <input
          v-model="referenceInputFieldModel"
          type="text"
          class="w-full rounded-lg border border-white/15 bg-black/40 px-4 py-3 text-base placeholder:text-slate-500 focus:border-sky-400 focus:outline-none"
          :placeholder="t('controller.referencePlaceholder')"
          @keyup.enter="publishReference"
        />
        <p class="text-xs text-slate-500">{{ t('controller.historyInfo') }}</p>

        <div class="flex flex-wrap gap-3">
          <button type="button" class="rounded-lg bg-sky-500 px-4 py-2 text-white" @click="publishReference">
            {{ t('controller.publish') }}
          </button>
          <button
            type="button"
            class="rounded-lg border border-white/20 px-4 py-2 text-slate-200"
            @click="referenceInputStore.resetInputs"
          >
            {{ t('controller.reset') }}
          </button>
          <span v-if="hasDraftChanges" class="text-xs text-amber-300">{{ t('controller.draftDifferent') }}</span>
        </div>

        <div class="space-y-2">
          <p class="text-sm font-semibold text-slate-200">{{ t('controller.historyLabel') }}</p>
          <div class="flex flex-wrap gap-2">
            <button
              v-for="entry in visibleHistory"
              :key="entry"
              type="button"
              class="rounded-full border border-white/15 px-3 py-1 text-xs text-slate-200"
              @click="applyHistoryEntry(entry)"
            >
              {{ entry }}
            </button>
            <span v-if="!visibleHistory.length" class="text-xs text-slate-500">
              {{ t('controller.historyEmpty') }}
            </span>
          </div>
        </div>

        <div class="rounded-xl border border-emerald-500/20 bg-emerald-500/10 p-5">
          <p class="text-xs uppercase tracking-[0.3em] text-emerald-200">{{ t('controller.normalizedOutput') }}</p>
          <p v-if="controllerViewModel.lastParseError" class="text-rose-300">
            {{ controllerViewModel.lastParseError.message }}
          </p>
          <template v-else>
            <p class="text-2xl font-semibold">{{ controllerViewModel.normalizedReferenceLabel }}</p>
            <p class="text-sm text-slate-300" v-if="controllerViewModel.passageVerses.length">
              {{ t('controller.versesLabel') }} {{ verseSpanSummary }}
            </p>
            <p class="text-sm text-slate-300" v-else>{{ t('controller.fullChapter') }}</p>
            <div class="space-y-2 text-base text-slate-100">
              <p v-if="controllerViewModel.isLoadingPassage" class="text-slate-400">
                {{ t('display.loading') }}
              </p>
              <p v-else-if="passageErrorText" class="text-rose-300">{{ passageErrorText }}</p>
              <p v-else v-for="verse in controllerViewModel.passageVerses" :key="verse.verse">
                <span class="text-slate-500 mr-2">{{ verse.verse }}</span>{{ verse.text }}
              </p>
            </div>
            <p class="text-xs text-slate-500" v-if="controllerViewModel.lastNormalizedAt">
              {{ t('controller.normalizedUpdated', { time: controllerViewModel.lastNormalizedAt.toLocaleTimeString() }) }}
            </p>
          </template>
        </div>
      </div>

      <aside class="space-y-5 rounded-2xl border border-white/10 bg-white/5 p-6">
        <div>
          <p class="text-sm font-semibold text-slate-200">{{ t('controller.translations.available') }}</p>
          <p class="text-xs text-slate-500">{{ t('controller.translations.providerHint') }}</p>
        </div>
        <div class="space-y-3 text-sm">
          <div v-if="biblesStore.loading" class="text-slate-400">{{ t('display.loading') }}</div>
          <ul v-else class="space-y-2">
            <li v-for="bible in biblesStore.translations" :key="bible.code" class="rounded border border-white/10 p-3">
              <div class="flex items-center justify-between">
                <div class="flex flex-col">
                  <p class="font-semibold">{{ bible.name }}</p>
                  <p class="text-xs text-slate-400">
                    {{ t('controller.translations.provider') }}: {{ bible.provider }} · Cache: {{ bible.cachePolicy }}
                  </p>
                </div>
                <div class="flex flex-col items-end gap-1">
                  <span class="text-xs uppercase text-slate-400">{{ bible.language }}</span>
                  <span
                    v-if="bible.isOfflineReady"
                    class="rounded-full bg-emerald-500/20 px-2 py-0.5 text-[10px] font-semibold uppercase tracking-wide text-emerald-200"
                  >
                    {{ t('controller.translations.offlineBadge') }}
                  </span>
                </div>
              </div>
            </li>
          </ul>
        </div>
      </aside>
    </div>
  </section>
</template>

<script setup lang="ts">
import { computed, onMounted, watch } from 'vue'
import { storeToRefs } from 'pinia'
import { useI18n } from 'vue-i18n'
import { useRoute } from 'vue-router'

import { useBiblesStore } from '@/stores/biblesStore'
import { useReferenceInputStore } from '@/stores/referenceInputStore'
import { useSessionStore } from '@/stores/sessionStore'
import { useSessionParticipationStore } from '@/stores/sessionParticipationStore'

interface Props {
  sessionId: string
}

const props = defineProps<Props>()
const route = useRoute()
const { t } = useI18n()
const referenceInputStore = useReferenceInputStore()
const sessionStore = useSessionStore()
const sessionParticipationStore = useSessionParticipationStore()
const biblesStore = useBiblesStore()

const referenceInputFieldModel = computed({
  get: () => referenceInputStore.draftInput,
  set: (value: string) => referenceInputStore.updateDraftInput(value, { autoPublish: false }),
})

const { recentHistory, hasDraftChanges } = storeToRefs(referenceInputStore)
const { controllerViewModel } = storeToRefs(sessionStore)

const visibleHistory = computed(() => recentHistory.value.slice(0, 5))
const verseSpanSummary = computed(() =>
  controllerViewModel.value.passageVerses.map((verse) => verse.verse).join(', '),
)

const joinToken = computed(() => (typeof route.query.token === 'string' ? route.query.token : ''))
const sessionLabel = computed(() =>
  t('controller.sessionLabel', { code: sessionParticipationStore.activeSessionId ?? 'demo' }),
)

const joinErrorText = computed(() =>
  sessionParticipationStore.joinErrorMessage
    ? t(sessionParticipationStore.joinErrorMessage.key, sessionParticipationStore.joinErrorMessage.params ?? {})
    : null,
)

const translationNoticeText = computed(() =>
  sessionStore.translationNotice
    ? t(sessionStore.translationNotice.key, sessionStore.translationNotice.params ?? {})
    : null,
)

const passageErrorText = computed(() =>
  controllerViewModel.value.passageErrorMessage
    ? t(
        controllerViewModel.value.passageErrorMessage.key,
        controllerViewModel.value.passageErrorMessage.params ?? {},
      )
    : null,
)

function publishReference() {
  sessionStore.publishDraftToSession({ recordHistory: true })
}

function applyHistoryEntry(entry: string) {
  sessionStore.applyHistoryEntry(entry)
}

watch(
  () => [props.sessionId, joinToken.value],
  ([sessionId, token]) => {
    if (!sessionId || !token) {
      sessionParticipationStore.setJoinError('session.joinErrors.missingToken')
      return
    }
    void sessionParticipationStore.joinSession({ sessionId, joinToken: token, role: 'controller' })
  },
  { immediate: true },
)

onMounted(() => {
  void biblesStore.loadTranslations()
})
</script>
