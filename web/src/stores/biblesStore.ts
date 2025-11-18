import { defineStore } from 'pinia'
import { computed, ref } from 'vue'

import { apiClient } from '@/lib/apiClient'
import type { paths } from '../../../shared/typescript/api'

type BibleTranslation = paths['/api/bibles']['get']['responses']['200']['content']['application/json'][number]

export const useBiblesStore = defineStore('bibles', () => {
  const translations = ref<BibleTranslation[]>([])
  const loading = ref(false)
  const errorMessage = ref<string | null>(null)
  const hasLoaded = ref(false)

  const offlineTranslations = computed(() =>
    translations.value.filter((translation) => translation.isOfflineReady),
  )

  function findByCode(code: string): BibleTranslation | null {
    return translations.value.find((translation) => translation.code === code) ?? null
  }

  async function loadTranslations() {
    if (hasLoaded.value || loading.value) {
      return
    }
    loading.value = true
    errorMessage.value = null
    try {
      const response = await apiClient.GET('/api/bibles')
      const data = response.data ?? []
      translations.value = data
      hasLoaded.value = true
    } catch (error) {
      errorMessage.value = error instanceof Error ? error.message : 'Unexpected error'
    } finally {
      loading.value = false
    }
  }

  return {
    translations,
    offlineTranslations,
    loading,
    errorMessage,
    loadTranslations,
    findByCode,
  }
})
