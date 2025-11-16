import { defineStore } from 'pinia'
import { computed, ref } from 'vue'

const DEFAULT_REFERENCE = 'Juan 3:16-18'
const MAX_HISTORY_ENTRIES = 10

interface PublishOptions {
  readonly pushToHistory?: boolean
}

export const useReferenceInputStore = defineStore('referenceInput', () => {
  const draftInput = ref(DEFAULT_REFERENCE)
  const currentInput = ref(DEFAULT_REFERENCE)
  const recentHistory = ref<string[]>([DEFAULT_REFERENCE])

  const hasDraftChanges = computed(() => draftInput.value.trim() !== currentInput.value.trim())

  function updateDraftInput(nextValue: string, options?: { autoPublish?: boolean }) {
    draftInput.value = nextValue
    if (options?.autoPublish ?? true) {
      publishDraftInput({ pushToHistory: false })
    }
  }

  function publishDraftInput(options?: PublishOptions) {
    const normalizedValue = draftInput.value.trim()
    currentInput.value = normalizedValue

    const shouldRecord = options?.pushToHistory ?? true
    if (shouldRecord && normalizedValue.length > 0) {
      addToHistory(normalizedValue)
    }
  }

  function addToHistory(value: string) {
    recentHistory.value = [value, ...recentHistory.value.filter((entry) => entry !== value)].slice(
      0,
      MAX_HISTORY_ENTRIES,
    )
  }

  function applyHistoryItem(value: string) {
    draftInput.value = value
    publishDraftInput({ pushToHistory: false })
  }

  function resetInputs() {
    draftInput.value = DEFAULT_REFERENCE
    publishDraftInput({ pushToHistory: false })
  }

  return {
    draftInput,
    currentInput,
    recentHistory,
    hasDraftChanges,
    updateDraftInput,
    publishDraftInput,
    applyHistoryItem,
    resetInputs,
  }
})
