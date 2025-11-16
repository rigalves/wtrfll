import { defineStore } from 'pinia'
import { ref, computed } from 'vue'

import { apiClient } from '@/lib/apiClient'
import type { paths } from '../../../shared/typescript/api'

export interface BibleBookMetadata {
  readonly id: string
  readonly englishDisplayName: string
  readonly spanishDisplayName: string
  readonly slug: string
  readonly ordinal?: number
  readonly aliases: readonly string[]
}

type BibleBooksResponse = paths['/api/bible-books']['get']['responses']['200']['content']['application/json']

export const useBibleBooksStore = defineStore('bibleBooks', () => {
  const books = ref<BibleBookMetadata[]>([])
  const loadState = ref<'idle' | 'loading' | 'loaded' | 'error'>('idle')
  const loadError = ref<string | null>(null)
  const aliasLookup = ref<Map<string, BibleBookMetadata>>(new Map())

  const isLoaded = computed(() => loadState.value === 'loaded')

  async function ensureLoaded(): Promise<void> {
    if (loadState.value === 'loaded') {
      return
    }
    if (loadState.value === 'loading') {
      return await waitUntilLoaded()
    }

    loadState.value = 'loading'
    loadError.value = null

    try {
      const response = await apiClient.GET('/api/bible-books')
      const payload = response.data as BibleBooksResponse | null
      if (!payload) {
        throw new Error('No se pudo cargar el catalogo de libros.')
      }

      const normalized = payload.map((entry) => ({
        ...entry,
        ordinal: entry.ordinal ?? undefined,
      }))

      books.value = normalized
      const lookup = new Map<string, BibleBookMetadata>()
      for (const book of normalized) {
        for (const alias of book.aliases ?? []) {
          const normalizedAlias = normalizeToken(alias)
          if (!normalizedAlias || lookup.has(normalizedAlias)) {
            continue
          }
          lookup.set(normalizedAlias, book)
        }
      }

      aliasLookup.value = lookup
      loadState.value = 'loaded'
    } catch (error) {
      loadState.value = 'error'
      books.value = []
      aliasLookup.value = new Map()
      loadError.value = error instanceof Error ? error.message : 'No se pudo cargar el catalogo de libros.'
      throw error
    }
  }

  async function waitUntilLoaded(): Promise<void> {
    if (loadState.value === 'loaded' || loadState.value === 'error') {
      return
    }
    await new Promise<void>((resolve) => {
      const check = () => {
        if (loadState.value === 'loaded' || loadState.value === 'error') {
          resolve()
        } else {
          requestAnimationFrame(check)
        }
      }
      check()
    })
  }

  function findBookByAlias(input: string): BibleBookMetadata | undefined {
    const normalized = normalizeToken(input)
    if (!normalized) {
      return undefined
    }
    return aliasLookup.value.get(normalized)
  }

  return {
    books,
    loadState,
    loadError,
    isLoaded,
    ensureLoaded,
    findBookByAlias,
  }
})

export async function ensureBibleBookMetadataLoaded(): Promise<void> {
  const store = useBibleBooksStore()
  await store.ensureLoaded()
}

export function findBibleBookMetadata(input: string): BibleBookMetadata | undefined {
  const store = useBibleBooksStore()
  return store.findBookByAlias(input)
}

export function normalizeToken(value: string): string {
  return value
    .normalize('NFD')
    .replace(/\p{Diacritic}/gu, '')
    .toLowerCase()
    .replace(/[^a-z0-9]/g, '')
}
