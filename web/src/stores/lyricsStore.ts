import { defineStore } from 'pinia'
import { ref } from 'vue'

import { apiClient } from '@/lib/apiClient'
type LyricsEntrySummary = { id: string; title: string; author?: string | null }
type LyricsEntryDetail = { id: string; title: string; author?: string | null; lyricsChordPro: string; style?: LyricsStyle }
type LyricsStyle = { fontScale?: number | null; columnCount?: number | null }
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
      entries.value = ((response.data ?? []).map((entry: any) => ({
        ...entry,
        author: entry.author ?? null,
      })) as LyricsEntrySummary[])
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
    const response: any = await apiClient.GET('/api/lyrics/{id}', {
      params: { path: { id } },
    })
    if (response.error) {
      throw new Error(response.error.error ?? 'Lyrics not found')
    }
    if (response.data) {
      const normalized = { ...response.data, author: response.data.author ?? null } as LyricsEntryDetailWithStyle
      details.value[id] = normalized as LyricsEntryDetail
      return normalized
    }
    return null
  }

  async function saveLyrics(payload: { id?: string; title: string; author?: string | null; lyricsChordPro: string; style?: LyricsStyle }) {
    let saved: LyricsEntryDetailWithStyle | null = null
    if (payload.id) {
      const response: any = await apiClient.PUT('/api/lyrics/{id}', {
        params: { path: { id: payload.id } },
        body: {
          title: payload.title,
          author: payload.author ?? undefined,
          lyricsChordPro: payload.lyricsChordPro,
          style: payload.style,
        },
      })
      if (response.error) throw new Error(response.error.error ?? 'Lyrics save failed')
      saved = response.data ?? null
    } else {
      const response: any = await apiClient.POST('/api/lyrics', {
        body: {
          title: payload.title,
          author: payload.author ?? undefined,
          lyricsChordPro: payload.lyricsChordPro,
          style: payload.style,
        },
      })
      if (response.error) throw new Error(response.error.error ?? 'Lyrics save failed')
      saved = response.data ?? null
    }
    if (saved) {
      const normalized = { ...saved, author: (saved as any).author ?? null } as LyricsEntryDetailWithStyle
      details.value[normalized.id] = normalized as LyricsEntryDetail
      await loadLyrics()
      return normalized
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
