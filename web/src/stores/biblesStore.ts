import { defineStore } from 'pinia'
import { ref } from 'vue'

import { apiClient } from '@/lib/apiClient'
import type { paths } from '../../../shared/typescript/api'

type BibleTranslation = paths['/api/bibles']['get']['responses']['200']['content']['application/json'][number]

export const useBiblesStore = defineStore('bibles', () => {
  const translations = ref<BibleTranslation[]>([])
  const loading = ref(false)
  const errorMessage = ref<string | null>(null)
  const hasLoaded = ref(false)

  async function loadTranslations() {
    if (hasLoaded.value) {
      return
    }
    loading.value = true
    errorMessage.value = null
    try {
      const response = await apiClient.GET('/api/bibles')
      // openapi-fetch returns `error` as `never` when no non-200 responses exist; guard defensively.
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
    loading,
    errorMessage,
    loadTranslations,
  }
})
