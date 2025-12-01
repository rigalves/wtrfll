import { defineStore } from 'pinia'
import { ref } from 'vue'

import { apiClient } from '@/lib/apiClient'
import type { components } from '../../../shared/typescript/api'

type LyricsEntrySummary = components['schemas']['LyricsEntrySummary']
type LyricsEntryDetail = components['schemas']['LyricsEntryDetail']
type LyricsStyle = { fontScale?: number | null }
type LyricsEntryDetailWithStyle = LyricsEntryDetail & { style?: LyricsStyle }

export const useLyricsStore = defineStore('lyrics', () => {
  const entries = ref<LyricsEntrySummary[]>([])
  const isLoading = ref(false)
  const errorMessage = ref<string | null>(null)
  const details = ref<Record<string, LyricsEntryDetail>>({})

  async function loadLyrics(search?: string) {
    if (isLoading.value) return
    isLoading.value = true
    errorMessage.value = null
    try {
      const response = await apiClient.GET('/api/lyrics', {
        params: search ? { query: { search } } : undefined,
      })
      entries.value = response.data ?? []
    } catch (error) {
      errorMessage.value = error instanceof Error ? error.message : 'Unexpected lyrics error'
    } finally {
      isLoading.value = false
    }
  }

  async function loadLyricsDetail(id: string): Promise<LyricsEntryDetailWithStyle | null> {
    if (details.value[id]) {
      return details.value[id] as LyricsEntryDetailWithStyle
    }
    const response = await apiClient.GET('/api/lyrics/{id}', {
      params: { path: { id } },
    })
    if (response.error) {
      throw new Error(response.error.error ?? 'Lyrics not found')
    }
    if (response.data) {
      details.value[id] = response.data
    }
    return (response.data as LyricsEntryDetailWithStyle) ?? null
  }

  async function saveLyrics(payload: { id?: string; title: string; author?: string | null; lyricsChordPro: string; style?: LyricsStyle }) {
    let saved: LyricsEntryDetailWithStyle | null = null
    if (payload.id) {
      const response = await apiClient.PUT('/api/lyrics/{id}', {
        params: { path: { id: payload.id } },
        body: {
          title: payload.title,
          author: payload.author,
          lyricsChordPro: payload.lyricsChordPro,
          style: payload.style,
        },
      })
      if (response.error) throw new Error(response.error.error ?? 'Lyrics save failed')
      saved = response.data ?? null
    } else {
      const response = await apiClient.POST('/api/lyrics', {
        body: {
          title: payload.title,
          author: payload.author,
          lyricsChordPro: payload.lyricsChordPro,
          style: payload.style,
        },
      })
      if (response.error) throw new Error(response.error.error ?? 'Lyrics save failed')
      saved = response.data ?? null
    }
    if (saved) {
      details.value[saved.id] = saved
      await loadLyrics()
    }
    return saved
  }

  return {
    entries,
    isLoading,
    errorMessage,
    details,
    loadLyrics,
    loadLyricsDetail,
    saveLyrics,
  }
})
