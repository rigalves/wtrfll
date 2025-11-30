<template>
  <section class="mx-auto max-w-6xl px-6 py-8 space-y-6">
    <header class="flex flex-wrap items-center justify-between gap-4 rounded-2xl border border-white/10 bg-white/5 px-6 py-4">
      <div class="space-y-1">
        <p class="text-xs uppercase tracking-[0.4em] text-slate-500">{{ sessionLabel }}</p>
        <h1 class="text-3xl font-semibold">{{ t('controller.title') }}</h1>
        <p class="text-slate-300">
          {{ t('controller.headerDescription') }}
        </p>
      </div>
      <div class="flex flex-wrap items-center gap-3">
        <div class="flex gap-3">
          <button
            v-for="button in commandButtons"
            :key="button.command"
            type="button"
            class="flex h-14 w-14 items-center justify-center rounded-full border text-base text-slate-200 hover:border-sky-400 hover:text-white transition"
            :class="button.command === activeDisplayCommand ? 'bg-sky-500 text-white border-sky-400 shadow-lg shadow-sky-500/30' : 'bg-black/30 border-white/20'"
            :title="button.label"
            @click="setDisplayCommand(button.command)"
          >
            <span class="sr-only">{{ button.label }}</span>
            <svg class="h-6 w-6" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.5" stroke-linecap="round" stroke-linejoin="round">
              <path v-for="(path, index) in button.iconPaths" :key="index" :d="path" />
            </svg>
          </button>
          <button
            type="button"
            class="flex h-14 items-center gap-3 rounded-full border-2 border-sky-400 bg-sky-500 px-6 text-base font-semibold text-white shadow-lg shadow-sky-500/30 transition hover:bg-sky-400 focus:outline-none focus-visible:ring-2 focus-visible:ring-sky-300"
            @click="publishActiveSlide"
          >
            <svg class="h-6 w-6" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.5" stroke-linecap="round" stroke-linejoin="round">
              <path d="M4 4l16 8-16 8 4-8z" />
              <path d="M4 4l16 8-7 3" />
            </svg>
            <span>{{ t('controller.publish') }}</span>
          </button>
        </div>
      </div>
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

    <div class="grid gap-6 lg:grid-cols-[260px,1fr,320px]">
      <aside class="space-y-4 rounded-2xl border border-white/10 bg-white/5 p-5">
        <div>
          <p class="text-sm font-semibold text-slate-100">{{ t('controller.slidesTitle') }}</p>
          <div class="mt-3 grid grid-cols-2 gap-2 text-xs">
            <button
              v-for="button in slideCreationButtons"
              :key="button.key"
              type="button"
              class="flex flex-col items-center justify-center gap-1 rounded-xl border px-3 py-2 text-center transition"
              :class="[
                button.disabled
                  ? 'cursor-not-allowed border-white/5 text-slate-600'
                  : 'border-white/15 text-slate-100 hover:border-sky-400 hover:text-white',
              ]"
              :disabled="button.disabled"
              @click="button.action?.()"
            >
              <svg
                class="h-6 w-6"
                viewBox="0 0 24 24"
                fill="none"
                stroke="currentColor"
                stroke-width="1.5"
                stroke-linecap="round"
                stroke-linejoin="round"
              >
                <path v-for="(path, index) in button.iconPaths" :key="index" :d="path" />
              </svg>
              <span class="text-[0.65rem] uppercase tracking-[0.2em]">{{ button.label }}</span>
            </button>
          </div>
        </div>
        <ul class="space-y-2 text-sm text-slate-200">
          <li
            v-for="slide in slidesStore.slides"
            :key="slide.id"
            :class="[
              'flex items-center justify-between rounded-xl border px-3 py-3 transition',
              slidesStore.activeSlideId === slide.id ? 'border-sky-400 bg-sky-500/10' : 'border-white/10 bg-black/20',
            ]"
          >
            <div @click="slidesStore.setActiveSlide(slide.id)" class="cursor-pointer">
              <p class="font-semibold capitalize">{{ slide.type }}</p>
              <p class="text-xs text-slate-400">{{ slide.label }}</p>
            </div>
            <button
              type="button"
              class="rounded-full border border-white/15 p-2 text-slate-400 hover:border-rose-400 hover:text-rose-200"
              @click.stop="removeSlide(slide.id)"
              :aria-label="t('controller.removeSlide', { label: slide.label })"
            >
              <svg class="h-4 w-4" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-linecap="round" stroke-linejoin="round" stroke-width="1.5">
                <path d="M3 6h18" />
                <path d="M8 6v13a1 1 0 001 1h6a1 1 0 001-1V6" />
                <path d="M10 11v6" />
                <path d="M14 11v6" />
                <path d="M9 6V4a1 1 0 011-1h4a1 1 0 011 1v2" />
              </svg>
            </button>
          </li>
          <li v-if="!slidesStore.slides.length" class="rounded-xl border border-white/10 px-3 py-3 text-xs text-slate-400">
            {{ t('controller.slidesEmpty') }}
          </li>
        </ul>
      </aside>

      <div class="space-y-4 rounded-2xl border border-white/10 bg-white/5 p-6 shadow-xl shadow-black/30">
<template v-if="isPassageSlide">
          <div class="space-y-1">
            <label class="block text-sm font-semibold">{{ t('controller.referenceLabel') }}</label>
            <p class="text-xs text-slate-500">
              {{ t('controller.referenceHelper') }}
            </p>
          </div>
          <input
            v-model="referenceInputFieldModel"
            type="text"
            class="w-full rounded-lg border border-white/15 bg-black/40 px-4 py-3 text-base placeholder:text-slate-500 focus:border-sky-400 focus:outline-none"
            :placeholder="t('controller.referencePlaceholder')"
            @keyup.enter="publishActiveSlide"
          />

          <p
            v-if="controllerViewModel.lastParseError"
            class="rounded-lg border border-rose-400/50 bg-rose-500/10 px-4 py-2 text-sm text-rose-200"
          >
            {{ controllerViewModel.lastParseError.message }}
          </p>
          <div class="flex w-full flex-wrap items-center gap-3">
            <button
              type="button"
              class="flex flex-1 min-w-[6rem] flex-col items-center justify-center gap-1 rounded-full border border-white/20 px-4 py-3 text-center text-slate-200 disabled:border-white/10 disabled:text-slate-500"
              :disabled="!canStepBackward"
              @click="sessionStore.stepToPreviousVerse()"
              :aria-label="t('controller.stepper.previous')"
            >
              <svg class="h-6 w-6" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-linecap="round" stroke-linejoin="round" stroke-width="1.5">
                <path d="M15 6l-6 6 6 6" />
              </svg>
              <span class="text-[0.65rem] uppercase tracking-[0.15em]">{{ t('controller.stepper.previousShort') }}</span>
            </button>
            <button
              type="button"
              class="flex flex-1 min-w-[6rem] flex-col items-center justify-center gap-1 rounded-full border border-white/20 px-4 py-3 text-center text-slate-200 disabled:border-white/10 disabled:text-slate-500"
              :disabled="!canStepForward"
              @click="sessionStore.stepToNextVerse()"
              :aria-label="t('controller.stepper.next')"
            >
              <svg class="h-6 w-6" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-linecap="round" stroke-linejoin="round" stroke-width="1.5">
                <path d="M9 6l6 6-6 6" />
              </svg>
              <span class="text-[0.65rem] uppercase tracking-[0.15em]">{{ t('controller.stepper.nextShort') }}</span>
            </button>
          </div>

          <div class="pt-3">
            <div v-if="filteredTranslations.length">
              <Listbox
                as="div"
                :modelValue="activeTranslationCode"
                @update:modelValue="selectTranslation"
                class="w-full"
              >
                <div class="relative">
                  <ListboxButton
                    class="flex w-full items-center justify-between gap-3 rounded-xl border border-white/15 bg-black/40 px-4 py-3 text-left text-sm text-slate-100 hover:border-sky-400 focus:outline-none focus-visible:ring-2 focus-visible:ring-sky-400"
                  >
                    <div>
                      <p class="text-[0.65rem] uppercase tracking-[0.2em] text-slate-500">
                        {{ t('controller.translations.available') }}
                      </p>
                      <p class="font-semibold">{{ activeTranslationDisplayLabel }}</p>
                    </div>
                    <svg class="h-5 w-5 text-slate-400" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-linecap="round" stroke-linejoin="round" stroke-width="1.5">
                      <path d="M6 9l6 6 6-6" />
                    </svg>
                  </ListboxButton>
                  <ListboxOptions class="absolute right-0 z-20 mt-2 max-h-72 w-64 overflow-y-auto rounded-2xl border border-white/10 bg-slate-900/95 p-2 shadow-2xl shadow-black/40">
                    <ListboxOption
                      v-for="bible in filteredTranslations"
                      :key="bible.code"
                      :value="bible.code"
                      v-slot="{ active, selected }"
                    >
                      <li
                        :class="[
                          'flex items-center justify-between rounded-xl px-3 py-2 text-sm',
                          active ? 'bg-sky-500/15 text-white' : 'text-slate-200',
                        ]"
                      >
                        <div>
                          <p class="font-semibold">{{ bible.name }}</p>
                          <p class="text-xs text-slate-400">{{ bible.code }} - {{ bible.language.toUpperCase() }}</p>
                        </div>
                        <div class="flex items-center gap-2">
                          <span
                            v-if="bible.isOfflineReady"
                            class="rounded-full bg-emerald-500/15 px-2 py-0.5 text-[0.65rem] text-emerald-300"
                          >
                            {{ t('controller.translations.offlineBadge') }}
                          </span>
                          <svg
                            v-if="selected"
                            class="h-4 w-4 text-sky-300"
                            viewBox="0 0 24 24"
                            fill="none"
                            stroke="currentColor"
                            stroke-linecap="round"
                            stroke-linejoin="round"
                            stroke-width="1.5"
                          >
                            <path d="M5 13l4 4 10-10" />
                          </svg>
                        </div>
                      </li>
                    </ListboxOption>
                  </ListboxOptions>
                </div>
              </Listbox>
              <p class="mt-2 text-[0.65rem] text-slate-500">
                {{ t('controller.translations.providerHint') }}
              </p>
            </div>
            <div
              v-else
              class="rounded-xl border border-white/10 bg-black/30 px-4 py-3 text-xs text-slate-400"
            >
              {{ t('controller.translations.loading') }}
            </div>
          </div>
          <div class="mt-3 flex items-center gap-3">
            <p class="text-[0.65rem] uppercase tracking-[0.2em] text-slate-500">Appearance</p>
            <div class="flex items-center gap-2">
              <button
                type="button"
                class="rounded-full border border-white/15 px-3 py-1 text-xs text-slate-200 hover:border-sky-400 hover:text-white"
                @click="adjustFontScale(-0.1)"
              >
                A-
              </button>
              <span class="text-xs text-slate-400">{{ fontScale.toFixed(1) }}x</span>
              <button
                type="button"
                class="rounded-full border border-white/15 px-3 py-1 text-xs text-slate-200 hover:border-sky-400 hover:text-white"
                @click="adjustFontScale(0.1)"
              >
                A+
              </button>
            </div>
          </div>

          <div class="mt-4 rounded-xl border border-emerald-500/20 bg-emerald-500/10 p-4">
            <p class="text-xs uppercase tracking-[0.2em] text-emerald-200">{{ t('controller.drafts.scriptureTitle') }}</p>
            <ScriptureRenderer
              :reference="controllerViewModel.referenceDisplay"
              :verses="controllerViewModel.passageVerses"
              :currentIndex="controllerViewModel.currentIndex"
              :showVerseNumbers="true"
              :fontScale="fontScale"
              :emptyMessage="t('controller.drafts.emptyScripture')"
            />
          </div>
        </template>
        <template v-else-if="isLyricsSlide">
          <div class="space-y-4">
            <div class="grid gap-3 sm:grid-cols-[1.5fr,1fr]">
              <div class="space-y-2">
                <label class="block text-sm font-semibold">{{ t('controller.lyrics.songSearchLabel') }}</label>
                <input
                  v-model="lyricsSearchTerm"
                  type="text"
                  class="w-full rounded-lg border border-white/15 bg-black/40 px-4 py-2 text-base placeholder:text-slate-500 focus:border-sky-400 focus:outline-none"
                  :placeholder="t('controller.lyrics.searchPlaceholder')"
                />
                <div class="rounded-xl border border-white/10 bg-black/30 max-h-48 overflow-y-auto">
                  <ul>
                    <li
                      v-for="entry in lyricsStore.entries"
                      :key="entry.id"
                      class="border-b border-white/5 last:border-0"
                    >
                      <button
                        type="button"
                        class="flex w-full items-center justify-between px-4 py-2 text-left text-sm"
                        :class="entry.id === activeLyricsPayload.lyricsId ? 'text-sky-300' : 'text-slate-200'"
                        @click="selectLyricsEntry(entry.id)"
                      >
                        <span class="font-semibold">{{ entry.title }}</span>
                        <span class="text-xs text-slate-500" v-if="entry.author">{{ entry.author }}</span>
                      </button>
                    </li>
                    <li v-if="!lyricsStore.entries.length" class="px-4 py-3 text-xs text-slate-500">
                      {{ t('controller.lyrics.emptySongs') }}
                    </li>
                  </ul>
                </div>
              </div>
              <div class="space-y-2">
                <label class="block text-sm font-semibold">{{ t('controller.lyrics.titleLabel') }}</label>
                <input
                  type="text"
                  class="w-full rounded-lg border border-white/15 bg-black/40 px-3 py-2 text-sm text-slate-100 focus:border-sky-400 focus:outline-none"
                  :placeholder="t('controller.lyrics.titlePlaceholder')"
                  :value="activeLyricsPayload.title ?? ''"
                  @input="onLyricsTitleChange(($event.target as HTMLInputElement).value)"
                />
                <label class="block text-sm font-semibold">{{ t('controller.lyrics.authorLabel') }}</label>
                <input
                  type="text"
                  class="w-full rounded-lg border border-white/15 bg-black/40 px-3 py-2 text-sm text-slate-100 focus:border-sky-400 focus:outline-none"
                  :placeholder="t('controller.lyrics.authorPlaceholder')"
                  :value="activeLyricsPayload.author ?? ''"
                  @input="onLyricsAuthorChange(($event.target as HTMLInputElement).value)"
                />
                <div class="flex gap-2 pt-2">
                  <button
                    type="button"
                    class="rounded-full border border-white/20 px-4 py-2 text-xs font-semibold text-white hover:border-sky-400 hover:text-sky-200 disabled:border-white/10 disabled:text-slate-500"
                    :disabled="lyricsSaveBusy"
                    @click="saveLyricsFromSlide"
                  >
                    {{ lyricsSaveBusy ? t('controller.lyrics.saving') : t('controller.lyrics.saveButton') }}
                  </button>
                  <p v-if="lyricsSaveError" class="text-xs text-rose-300">{{ lyricsSaveError }}</p>
                </div>
              </div>
            </div>
            <div class="space-y-2">
              <label class="block text-sm font-semibold">{{ t('controller.lyrics.textLabel') }}</label>
              <textarea
                class="w-full min-h-[220px] rounded-xl border border-white/15 bg-black/40 px-4 py-3 text-sm font-mono text-slate-100 focus:border-sky-400 focus:outline-none"
                :placeholder="t('controller.lyrics.textPlaceholder')"
                :value="activeLyricsPayload.lyricsChordPro"
                @input="onLyricsTextChange(($event.target as HTMLTextAreaElement).value)"
              ></textarea>
              <p class="text-[0.65rem] text-slate-500">
                {{ t('controller.lyrics.chordProHint') }}
              </p>
              <div class="flex items-center gap-3 pt-1">
                <p class="text-[0.65rem] uppercase tracking-[0.2em] text-slate-500">Appearance</p>
                <div class="flex items-center gap-2">
                  <button
                    type="button"
                    class="rounded-full border border-white/15 px-3 py-1 text-xs text-slate-200 hover:border-sky-400 hover:text-white"
                    @click="adjustFontScale(-0.1)"
                  >
                    A-
                  </button>
                  <span class="text-xs text-slate-400">{{ fontScale.toFixed(1) }}x</span>
                  <button
                    type="button"
                    class="rounded-full border border-white/15 px-3 py-1 text-xs text-slate-200 hover:border-sky-400 hover:text-white"
                    @click="adjustFontScale(0.1)"
                  >
                    A+
                  </button>
                </div>
              </div>
            </div>
            <div class="rounded-xl border border-emerald-500/20 bg-emerald-500/10 p-4">
            <p class="text-xs uppercase tracking-[0.2em] text-emerald-200">{{ t('controller.drafts.lyricsTitle') }}</p>
              <LyricsRenderer
                :title="activeLyricsPayload.title ?? ''"
                :author="activeLyricsPayload.author ?? ''"
                :lines="lyricsPreviewLines"
                :fontScale="fontScale"
                :emptyMessage="t('controller.lyrics.previewEmpty')"
              />
            </div>
          </div>
        </template>
      </div>

      <aside class="space-y-4 rounded-2xl border border-white/10 bg-white/5 p-6">
        <div class="flex items-center justify-between">
          <p class="text-sm font-semibold text-slate-200">{{ t('controller.livePreviewTitle') }}</p>
          <div class="flex items-center gap-3 text-xs text-slate-500">
            <p v-if="displayViewModel.reference">
              {{ t('controller.livePreviewUpdatedLabel') }} {{ displayTimestamp }}
            </p>
          </div>
        </div>
        <div class="rounded-xl border border-white/10 bg-black/50 p-4 text-left">
          <template v-if="activeDisplayCommand === 'black'">
            <p class="text-sm text-slate-400">{{ t('controller.commands.black') }}</p>
          </template>
          <template v-else-if="activeDisplayCommand === 'clear'">
            <p class="text-sm text-slate-400">{{ t('display.cleared') }}</p>
          </template>
          <template v-else-if="liveLyricsPreview">
            <LyricsRenderer
              :title="liveLyricsPreview?.title ?? ''"
              :author="liveLyricsPreview?.author ?? ''"
              :lines="liveLyricsPreview?.lines ?? []"
              :fontScale="liveLyricsPreview?.fontScale ?? fontScale"
              :emptyMessage="t('controller.lyrics.previewEmpty')"
            />
          </template>
            <template v-else>
              <ScriptureRenderer
                :reference="livePassagePreview.reference"
                :verses="livePassagePreview.verses"
                :currentIndex="livePassagePreview.currentIndex"
                :fontScale="livePassagePreview.fontScale ?? fontScale"
                :showVerseNumbers="true"
                :emptyMessage="t('controller.drafts.emptyScripture')"
              />
          </template>
        </div>
      </aside>
    </div>
  </section>
</template>

<script setup lang="ts">
import { Listbox, ListboxButton, ListboxOption, ListboxOptions } from '@headlessui/vue'
import { computed, onMounted, ref, watch } from 'vue'
import { storeToRefs } from 'pinia'
import { useI18n } from 'vue-i18n'
import { useRoute } from 'vue-router'

import type { DisplayCommand } from '@/lib/realtimeClient'
import LyricsRenderer from '@/components/LyricsRenderer.vue'
import ScriptureRenderer from '@/components/ScriptureRenderer.vue'
import { useBiblesStore } from '@/stores/biblesStore'
import { useReferenceInputStore } from '@/stores/referenceInputStore'
import { useLyricsStore } from '@/stores/lyricsStore'
import { useSessionSlidesStore } from '@/stores/sessionSlidesStore'
import { useSessionStore } from '@/stores/sessionStore'
import { useSessionParticipationStore } from '@/stores/sessionParticipationStore'
import { extractChordProLines } from '@/lib/chordPro'

interface Props {
  sessionId: string
}

interface SlideCreationButton {
  key: string
  label: string
  iconPaths: string[]
  disabled: boolean
  action?: () => void
}

interface LyricsSlidePayload {
  lyricsId?: string | null
  title?: string | null
  author?: string | null
  lyricsChordPro: string
}

const commandIconPaths: Record<DisplayCommand, string[]> = {
  normal: ['M9 5v14l10-7z'],
  black: ['M6 6h12v12H6z'],
  clear: ['M5 8h14', 'M5 12h14', 'M5 16h14'],
  freeze: ['M9 5v14', 'M15 5v14', 'M5 12h14'],
}

const slideIconPaths = {
  passage: ['M5.5 4.5h5.75v15H5.5A2.25 2.25 0 013.25 17.25V6.75A2.25 2.25 0 015.5 4.5z', 'M12.75 4.5h5.75A2.25 2.25 0 0120.75 6.75v10.5A2.25 2.25 0 0118.5 19.5h-5.75z'],
  lyrics: ['M9 5v11.5a2.5 2.5 0 11-1-2 2.5 2.5 0 011 2V7h9V5z'],
  image: ['M4 6a2 2 0 012-2h12a2 2 0 012 2v10a2 2 0 01-2 2H6a2 2 0 01-2-2V6z', 'M7 14l3-3 4 5 4-6 3 4v4H5z'],
  text: ['M5 7h14', 'M5 11h14', 'M5 15h9'],
} as const

const props = defineProps<Props>()
const route = useRoute()
const { t, locale } = useI18n()
const referenceInputStore = useReferenceInputStore()
const sessionStore = useSessionStore()
const sessionParticipationStore = useSessionParticipationStore()
const biblesStore = useBiblesStore()
const slidesStore = useSessionSlidesStore()
const lyricsStore = useLyricsStore()
const lyricsSearchTerm = ref('')
const lyricsSaveError = ref<string | null>(null)
const lyricsSaveBusy = ref(false)
const lyricsPreviewState = computed(() => {
  if (isLyricsSlide.value) {
    const payload = activeLyricsPayload.value
    return {
      title: payload.title ?? '',
      author: payload.author ?? '',
      lines: extractChordProLines(payload.lyricsChordPro ?? ''),
    }
  }
  const current = sessionStore.lyricsViewModel?.value ?? null
  if (current && (current.lyricsChordPro || current.title)) {
    return {
      title: current.title ?? '',
      author: current.author ?? '',
      lines: extractChordProLines(current.lyricsChordPro ?? ''),
    }
  }
  return null
})

const livePassagePreview = computed(() => ({
  reference: publishedPassageViewModel.value.reference || t('controller.drafts.emptyScripture'),
  verses: publishedPassageViewModel.value.verses,
  currentIndex: publishedPassageViewModel.value.currentIndex ?? 0,
  fontScale: publishedPassageViewModel.value.options?.fontScale ?? fontScale.value,
}))

const liveLyricsPreview = computed(() => {
  const state = lyricsViewModel.value
  if (!state) {
    return null
  }
  const lines = Array.isArray((state as any).lines)
    ? (state as any).lines
    : extractChordProLines((state as any).lyricsChordPro ?? '')
  return {
    title: state.title ?? '',
    author: state.author ?? '',
    lines,
    fontScale: (state as any).fontScale ?? fontScale.value,
  }
})

const referenceInputFieldModel = computed({
  get: () => referenceInputStore.draftInput,
  set: (value: string) => referenceInputStore.updateDraftInput(value, { autoPublish: false }),
})

const { controllerViewModel, displayViewModel, activeTranslationCode, lyricsViewModel, publishedPassageViewModel, presentationOptions } =
  storeToRefs(sessionStore)
const { currentSlide } = storeToRefs(slidesStore)

const activeTranslation = computed(() => biblesStore.findByCode(activeTranslationCode.value))
const activeTranslationDisplayLabel = computed(() => {
  if (activeTranslation.value) {
    return `${activeTranslation.value.name} - ${activeTranslation.value.code}`
  }
  return activeTranslationCode.value ?? t('controller.translations.available')
})

const filteredTranslations = computed(() => {
  const available = biblesStore.translations
  if (!available.length) {
    return []
  }
  const currentLanguage = locale.value.split('-')[0]?.toLowerCase() ?? ''
  const matching = available.filter((translation) =>
    translation.language?.toLowerCase().startsWith(currentLanguage),
  )
  return matching.length ? matching : available
})
const fontScale = computed(() => presentationOptions.value.fontScale ?? 1)

const joinToken = computed(() =>
  typeof route.query.token === 'string'
    ? route.query.token
    : sessionParticipationStore.getStoredJoinToken(props.sessionId, 'controller') ?? '',
)
const sessionLabel = computed(() => {
  const name = sessionParticipationStore.activeSessionName
  const shortCode = sessionParticipationStore.activeSessionShortCode
  const fallback = sessionParticipationStore.activeSessionId?.slice(-6)
  if (name && shortCode) {
    return `${name} - ${shortCode}`
  }
  if (shortCode) {
    return shortCode
  }
  return name ?? fallback ?? t('controller.sessionLabel', { code: '---' })
})

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

const activeSlide = computed(() => currentSlide.value)
const activePassageSlide = computed(() =>
  activeSlide.value && activeSlide.value.type === 'passage' ? activeSlide.value : null,
)
const activeLyricsSlide = computed(() =>
  activeSlide.value && activeSlide.value.type === 'lyrics' ? activeSlide.value : null,
)

const isPassageSlide = computed(() => Boolean(activePassageSlide.value))
const isLyricsSlide = computed(() => Boolean(activeLyricsSlide.value))
const activeLyricsPayload = computed<LyricsSlidePayload>(() => {
  if (!activeLyricsSlide.value) {
    return { lyricsChordPro: '' }
  }
  return {
    lyricsId: (activeLyricsSlide.value.payload as LyricsSlidePayload).lyricsId ?? null,
    title: (activeLyricsSlide.value.payload as LyricsSlidePayload).title ?? '',
    author: (activeLyricsSlide.value.payload as LyricsSlidePayload).author ?? '',
    lyricsChordPro: (activeLyricsSlide.value.payload as LyricsSlidePayload).lyricsChordPro ?? '',
  }
})
const lyricsPreviewLines = computed(() => extractChordProLines(activeLyricsPayload.value.lyricsChordPro))

const activeDisplayCommand = computed(() => sessionStore.displayCommand)
const displayCommandLabel = computed(() => {
  const labels: Record<DisplayCommand, string> = {
    normal: t('controller.commands.normal'),
    black: t('controller.commands.black'),
    clear: t('controller.commands.clear'),
    freeze: t('controller.commands.freeze'),
  }
  return labels[activeDisplayCommand.value]
})
const displayTimestamp = computed(() => {
  if (!displayViewModel.value.reference) {
    return ''
  }
  return new Date().toLocaleTimeString()
})

const canStepBackward = computed(() => {
  const verseCount = controllerViewModel.value.passageVerses.length
  return verseCount > 0 && controllerViewModel.value.currentIndex > 0
})

const commandButtons = computed(() => [
  { command: 'normal' as DisplayCommand, label: t('controller.commands.normal'), iconPaths: commandIconPaths.normal },
  { command: 'black' as DisplayCommand, label: t('controller.commands.black'), iconPaths: commandIconPaths.black },
  { command: 'clear' as DisplayCommand, label: t('controller.commands.clear'), iconPaths: commandIconPaths.clear },
  { command: 'freeze' as DisplayCommand, label: t('controller.commands.freeze'), iconPaths: commandIconPaths.freeze },
])

const slideCreationButtons = computed<SlideCreationButton[]>(() => [
  {
    key: 'passage',
    label: t('controller.slideTypes.passage'),
    iconPaths: [...slideIconPaths.passage],
    disabled: false,
    action: createPassageSlide,
  },
  {
    key: 'lyrics',
    label: t('controller.slideTypes.lyrics'),
    iconPaths: [...slideIconPaths.lyrics],
    disabled: false,
    action: createLyricsSlide,
  },
  {
    key: 'image',
    label: t('controller.slideTypes.image'),
    iconPaths: [...slideIconPaths.image],
    disabled: true,
  },
  {
    key: 'text',
    label: t('controller.slideTypes.text'),
    iconPaths: [...slideIconPaths.text],
    disabled: true,
  },
])

const canStepForward = computed(() => {
  const verseCount = controllerViewModel.value.passageVerses.length
  if (verseCount === 0) {
    return false
  }
  return controllerViewModel.value.currentIndex < verseCount - 1
})

function publishActiveSlide() {
  if (isLyricsSlide.value) {
    const payload = activeLyricsPayload.value
    sessionStore.publishLyricsPatch({
      lyricsId: activeLyricsPayload.value.lyricsId,
      title: payload.title,
      author: payload.author,
      lyricsChordPro: payload.lyricsChordPro,
    })
    return
  }
  if (!isPassageSlide.value) {
    return
  }
  sessionStore.publishDraftToSession({ recordHistory: true })
}

function setDisplayCommand(command: DisplayCommand) {
  sessionStore.setDisplayCommand(command)
}

function selectTranslation(code: string) {
  sessionStore.setActiveTranslation(code)
}

function adjustFontScale(delta: number) {
  sessionStore.setFontScale(fontScale.value + delta)
}

function createPassageSlide() {
  const slideId = crypto.randomUUID()
  slidesStore.addSlide({
    id: slideId,
    type: 'passage',
    label: controllerViewModel.value.normalizedReferenceLabel || t('controller.referenceLabel'),
    payload: {
      referenceInput: referenceInputStore.draftInput,
      referenceNormalized: controllerViewModel.value.normalizedReferenceLabel,
    },
  })
}

function createLyricsSlide() {
  const slideId = crypto.randomUUID()
  slidesStore.addSlide({
    id: slideId,
    type: 'lyrics',
    label: t('controller.slideTypes.lyrics'),
    payload: {
      lyricsId: null,
      title: '',
      author: '',
      lyricsChordPro: '',
    },
  })
}

function removeSlide(id: string) {
  slidesStore.removeSlide(id)
  if (!slidesStore.slides.length) {
    createPassageSlide()
  }
}

async function selectLyricsEntry(id: string) {
  const detail = await lyricsStore.loadLyricsDetail(id)
  if (!detail || !activeLyricsSlide.value) {
    return
  }
  const payload: LyricsSlidePayload = {
    lyricsId: detail.id,
    title: detail.title,
    author: detail.author ?? '',
    lyricsChordPro: detail.lyricsChordPro ?? '',
  }
  slidesStore.updateSlide(activeLyricsSlide.value.id, {
    label: detail.title,
    payload,
  })
}

function updateLyricsPayload(changes: Partial<LyricsSlidePayload>) {
  const slide = activeLyricsSlide.value
  if (!slide) return
  const nextPayload: LyricsSlidePayload = {
    ...(slide.payload as LyricsSlidePayload),
    ...changes,
  }
  slidesStore.updateSlide(slide.id, {
    payload: nextPayload,
    label: nextPayload.title || t('controller.slideTypes.lyrics'),
  })
}

function onLyricsTitleChange(value: string) {
  updateLyricsPayload({ title: value })
}

function onLyricsAuthorChange(value: string) {
  updateLyricsPayload({ author: value })
}

function onLyricsTextChange(value: string) {
  updateLyricsPayload({ lyricsChordPro: value })
}

async function saveLyricsFromSlide() {
  const payload = activeLyricsPayload.value
  if (!payload.title || !payload.lyricsChordPro) {
    lyricsSaveError.value = t('controller.lyrics.saveValidation')
    return
  }
  lyricsSaveBusy.value = true
  lyricsSaveError.value = null
  try {
    const saved = await lyricsStore.saveLyrics({
      id: payload.lyricsId ?? undefined,
      title: payload.title ?? '',
      author: payload.author ?? null,
      lyricsChordPro: payload.lyricsChordPro,
    })
    if (saved && activeLyricsSlide.value) {
      updateLyricsPayload({
        lyricsId: saved.id,
        title: saved.title,
        author: saved.author ?? '',
        lyricsChordPro: saved.lyricsChordPro ?? payload.lyricsChordPro,
      })
      slidesStore.updateSlide(activeLyricsSlide.value.id, { label: saved.title })
    }
  } catch (error) {
    lyricsSaveError.value = error instanceof Error ? error.message : 'Lyrics save failed'
  } finally {
    lyricsSaveBusy.value = false
  }
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

watch(
  () => slidesStore.activeSlideId,
  () => {
    const slide = currentSlide.value
    if (slide?.type === 'passage') {
      const raw = (slide.payload.referenceInput as string) ?? ''
      referenceInputStore.updateDraftInput(raw, { autoPublish: false })
      return
    }
    if (slide?.type === 'lyrics') {
      const payload = slide.payload as LyricsSlidePayload
      if (payload.lyricsId) {
        void lyricsStore.loadLyricsDetail(payload.lyricsId)
      }
    }
  },
  { immediate: true },
)

watch(
  () => referenceInputStore.draftInput,
  (value) => {
    const slide = currentSlide.value
    if (!slide || slide.type !== 'passage') {
      return
    }
    slidesStore.updateSlide(slide.id, {
      payload: { ...slide.payload, referenceInput: value },
    })
    sessionStore.schedulePreviewRefresh()
  },
)

watch(
  () => controllerViewModel.value.normalizedReferenceLabel,
  (label) => {
    const slide = currentSlide.value
    if (!slide || slide.type !== 'passage') {
      return
    }
    slidesStore.updateSlide(slide.id, {
      label: label || t('controller.referenceLabel'),
      payload: { ...slide.payload, referenceNormalized: label },
    })
  },
)

watch(
  () => lyricsSearchTerm.value,
  (term) => {
    void lyricsStore.loadLyrics(term || undefined)
  },
  { immediate: true },
)

onMounted(() => {
  void biblesStore.loadTranslations()
  void lyricsStore.loadLyrics()
  if (!slidesStore.slides.length) {
    createPassageSlide()
  }
})
</script>


