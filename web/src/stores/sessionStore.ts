import { defineStore } from 'pinia'
import { computed, ref, watch } from 'vue'

import {
  parseScriptureReference,
  type ParsedScriptureReference,
  type ReferenceParseError,
} from '@/lib/parseReference'
import { useReferenceInputStore } from '@/stores/referenceInputStore'
import {
  SessionRealtimeClient,
  type SessionPresentationOptions,
  type SessionStatePatchPayload,
  type SessionStateUpdatePayload,
  type DisplayCommand,
} from '@/lib/realtimeClient'
import { apiClient } from '@/lib/apiClient'
import type { LocalizedMessage } from '@/lib/i18n'
import { useSessionParticipationStore } from '@/stores/sessionParticipationStore'
import { useBiblesStore } from '@/stores/biblesStore'
import type { paths } from '../../../shared/typescript/api'

export const useSessionStore = defineStore('session', () => {
  const referenceInputStore = useReferenceInputStore()
  const participationStore = useSessionParticipationStore()
  const biblesStore = useBiblesStore()
  void biblesStore.loadTranslations()

  type PassageResponse = paths['/api/passage']['get']['responses']['200']['content']['application/json']

  const currentPassage = ref<ParsedScriptureReference | null>(null)
  const lastParseError = ref<ReferenceParseError | null>(null)
  const lastNormalizedAt = ref<Date | null>(null)
  const activeTranslationCode = ref('RVR1960')
  const realtimeClient = ref<SessionRealtimeClient | null>(null)
  const enableRealtime = (import.meta.env.VITE_ENABLE_REALTIME ?? 'true') === 'true'
  const passageContent = ref<PassageResponse | null>(null)
  const passageLoadErrorMessage = ref<LocalizedMessage | null>(null)
  const isLoadingPassage = ref(false)
  const translationNotice = ref<LocalizedMessage | null>(null)
  const displayCommand = ref<DisplayCommand>('normal')
  const defaultPresentationOptions: SessionPresentationOptions = {
    showReference: true,
    showVerseNumbers: true,
  }
  const presentationOptions = ref<SessionPresentationOptions>({ ...defaultPresentationOptions })
  const currentPresentationIndex = ref(0)
  let latestLoadToken = 0

  const resolvedSessionId = computed(() => participationStore.activeSessionId ?? 'demo')
  const resolvedJoinState = computed(() => participationStore.joinState)
  const resolvedRole = computed(() => participationStore.activeRole ?? 'display')
  const resolvedJoinToken = computed(() => participationStore.activeJoinToken)
  const activeTranslationMetadata = computed(() => biblesStore.findByCode(activeTranslationCode.value))

  const controllerViewModel = computed(() => {
    const verses = passageContent.value?.verses ?? []
    const currentVerse = verses[currentPresentationIndex.value]
    return {
      normalizedReferenceLabel: currentPassage.value?.normalizedReferenceLabel ?? referenceInputStore.currentInput,
      verseSpans: verses.map((verse) => ({ startVerse: verse.verse, endVerse: verse.verse })),
      isFullChapter: verses.length ? false : currentPassage.value?.isFullChapter ?? false,
      lastParseError: lastParseError.value,
      lastNormalizedAt: lastNormalizedAt.value,
      passageVerses: verses,
      passageErrorMessage: passageLoadErrorMessage.value,
      isLoadingPassage: isLoadingPassage.value,
      options: presentationOptions.value,
      currentIndex: currentPresentationIndex.value,
      displayCommand: displayCommand.value,
      activeVerseLabel: currentVerse ? `v${currentVerse.verse}` : null,
    }
  })

  const displayViewModel = computed(() => ({
    reference: passageContent.value?.reference ?? controllerViewModel.value.normalizedReferenceLabel,
    verses: passageContent.value?.verses ?? [],
    attribution: passageContent.value?.attribution?.text ?? null,
    isLoading: isLoadingPassage.value,
    options: presentationOptions.value,
    currentIndex: currentPresentationIndex.value,
    displayCommand: displayCommand.value,
  }))

  function publishDraftToSession(options?: { recordHistory?: boolean }) {
    referenceInputStore.publishDraftInput({ pushToHistory: options?.recordHistory ?? true })
    void normalizeAndLoad()
  }

  function applyHistoryEntry(value: string) {
    referenceInputStore.applyHistoryItem(value)
    void normalizeAndLoad()
  }

  async function normalizeAndLoad(options: { broadcast?: boolean } = {}) {
    const shouldBroadcast = options.broadcast ?? true
    if (participationStore.activeRole !== 'controller') {
      return
    }

    const nextInput = referenceInputStore.currentInput
    if (!nextInput) {
      currentPassage.value = null
      lastParseError.value = null
      passageContent.value = null
      return
    }

    const parseResult = parseScriptureReference(nextInput)
    if (parseResult.ok) {
      currentPassage.value = parseResult.value
      lastParseError.value = null
      lastNormalizedAt.value = new Date()
      currentPresentationIndex.value = 0
      if (shouldBroadcast) {
        broadcastStatePatch(parseResult.value)
      }
      const loadToken = ++latestLoadToken
      await loadPassageContent(parseResult.value, loadToken)
      return
    }

    currentPassage.value = null
    lastParseError.value = parseResult.error
    passageContent.value = null
  }

  async function loadPassageContent(parsed: ParsedScriptureReference, token: number) {
    isLoadingPassage.value = true
    passageLoadErrorMessage.value = null
    try {
      const { data, error } = await apiClient.GET('/api/passage', {
        params: {
          query: {
            translation: activeTranslationCode.value,
            ref: parsed.normalizedReferenceLabel,
          },
        },
      })
      if (token !== latestLoadToken) {
        return
      }
      if (error) {
        throw new Error(error.error ?? 'No se pudo obtener el pasaje')
      }
      passageContent.value = data ?? null
      translationNotice.value = null
      passageLoadErrorMessage.value = null
    } catch (error) {
      if (token !== latestLoadToken) {
        return
      }
      passageContent.value = null
      passageLoadErrorMessage.value = { key: 'session.passageErrors.requestFailed' }
      const fallback = selectOfflineFallback()
      if (fallback && fallback.code !== activeTranslationCode.value) {
        translationNotice.value = {
          key: 'session.translationFallback',
          params: {
            failed: activeTranslationMetadata.value?.name ?? activeTranslationCode.value,
            fallback: fallback.name,
          },
        }
        activeTranslationCode.value = fallback.code
        const retryToken = ++latestLoadToken
        await loadPassageContent(parsed, retryToken)
      }
    } finally {
      if (token === latestLoadToken) {
        isLoadingPassage.value = false
      }
    }
  }

  function selectOfflineFallback() {
    if (activeTranslationMetadata.value?.isOfflineReady) {
      return null
    }
    const fallback = biblesStore.offlineTranslations.find((translation) => translation.code !== activeTranslationCode.value)
    return fallback ?? null
  }

  function handleStateUpdate(payload: SessionStateUpdatePayload) {
    if (payload.sessionId !== resolvedSessionId.value) {
      return
    }
    const nextCommand = payload.state.displayCommand ?? displayCommand.value
    displayCommand.value = nextCommand

    const shouldApply = nextCommand !== 'freeze'
    if (shouldApply) {
      passageContent.value = payload.state as PassageResponse
      presentationOptions.value = {
        ...defaultPresentationOptions,
        ...(payload.state.options ?? {}),
      }
      currentPresentationIndex.value = payload.state.currentIndex ?? 0
    }
  }

  watch(
    [resolvedSessionId, resolvedJoinToken, resolvedJoinState, resolvedRole],
    () => {
      if (!enableRealtime) {
        return
      }
      realtimeClient.value?.dispose()
      realtimeClient.value = null
      if (resolvedJoinState.value !== 'joined' || !resolvedJoinToken.value) {
        return
      }
      const client = new SessionRealtimeClient({
        sessionId: resolvedSessionId.value,
        joinToken: resolvedJoinToken.value,
        role: resolvedRole.value,
      })
      client.onStateUpdate(handleStateUpdate)
      realtimeClient.value = client
      client.connect().catch(() => {
        realtimeClient.value = null
      })
    },
    { immediate: true },
  )

  watch(
    () => [participationStore.joinState, participationStore.activeRole] as const,
    async ([state, role]) => {
      if (state === 'joined' && role === 'controller') {
        presentationOptions.value = { ...defaultPresentationOptions }
        await normalizeAndLoad()
      }
    },
    { immediate: true },
  )

  function broadcastStatePatch(passage: ParsedScriptureReference) {
    if (
      !enableRealtime ||
      !realtimeClient.value ||
      participationStore.joinState !== 'joined' ||
      participationStore.activeRole !== 'controller'
    ) {
      return
    }

    const payload: SessionStatePatchPayload = {
      contractVersion: 1,
      sessionId: resolvedSessionId.value,
      patch: {
        translation: activeTranslationCode.value,
        passageRef: passage.normalizedReferenceLabel,
        currentIndex: currentPresentationIndex.value,
        options: { ...presentationOptions.value },
        displayCommand: displayCommand.value,
      },
    }

    realtimeClient.value.sendPatch(payload).catch(() => {})
  }

  void normalizeAndLoad()

  function stepPresentationIndex(delta: number) {
    if (
      participationStore.joinState !== 'joined' ||
      participationStore.activeRole !== 'controller' ||
      !currentPassage.value
    ) {
      return
    }

    const verses = controllerViewModel.value.passageVerses
    if (!verses.length) {
      return
    }

    const nextIndex = clampIndex(currentPresentationIndex.value + delta, verses.length)
    if (nextIndex === currentPresentationIndex.value) {
      return
    }

    currentPresentationIndex.value = nextIndex
    broadcastStatePatch(currentPassage.value)
  }

  function setDisplayCommand(command: DisplayCommand) {
    if (participationStore.joinState !== 'joined' || participationStore.activeRole !== 'controller') {
      return
    }
    if (!currentPassage.value) {
      return
    }
    displayCommand.value = command
    broadcastStatePatch(currentPassage.value)
  }

  function clampIndex(index: number, total: number) {
    if (total <= 0) {
      return 0
    }
    return Math.min(Math.max(index, 0), total - 1)
  }

  async function setActiveTranslation(code: string) {
    if (!code || code === activeTranslationCode.value) {
      return
    }
    activeTranslationCode.value = code
    if (
      participationStore.joinState === 'joined' &&
      participationStore.activeRole === 'controller' &&
      currentPassage.value
    ) {
      const loadToken = ++latestLoadToken
      await loadPassageContent(currentPassage.value, loadToken)
      broadcastStatePatch(currentPassage.value)
    }
  }

  let previewDebounceId: ReturnType<typeof setTimeout> | null = null
  function schedulePreviewRefresh() {
    if (participationStore.joinState !== 'joined' || participationStore.activeRole !== 'controller') {
      return
    }
    if (previewDebounceId) {
      clearTimeout(previewDebounceId)
    }
    previewDebounceId = setTimeout(() => {
      previewDebounceId = null
      referenceInputStore.publishDraftInput({ pushToHistory: false })
      void normalizeAndLoad({ broadcast: false })
    }, 500)
  }

  return {
    currentPassage,
    lastParseError,
    lastNormalizedAt,
    controllerViewModel,
    displayViewModel,
    publishDraftToSession,
    applyHistoryEntry,
    activeTranslationCode,
    translationNotice,
    stepToPreviousVerse: () => stepPresentationIndex(-1),
    stepToNextVerse: () => stepPresentationIndex(1),
    setDisplayCommand,
    displayCommand,
    currentPresentationIndex,
    setActiveTranslation,
    schedulePreviewRefresh,
  }
})
