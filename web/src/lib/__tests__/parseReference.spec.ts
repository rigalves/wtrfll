import { readFileSync } from 'node:fs'
import { setActivePinia, createPinia } from 'pinia'
import { beforeAll, describe, expect, it, vi } from 'vitest'

import { isSuccessfulParse, parseScriptureReference } from '@/lib/parseReference'
import { ensureBibleBookMetadataLoaded } from '@/stores/bibleBooksStore'

const bibleBooksFixture = JSON.parse(
  readFileSync(new URL('../../../../server/Data/bibleBooks.metadata.json', import.meta.url), 'utf-8'),
) as unknown

beforeAll(() => {
  setActivePinia(createPinia())
  vi.stubGlobal(
    'fetch',
    async () =>
      new Response(JSON.stringify(bibleBooksFixture), {
        status: 200,
        headers: { 'Content-Type': 'application/json' },
      }),
  )
})

beforeAll(async () => {
  await ensureBibleBookMetadataLoaded()
})

describe('parseScriptureReference', () => {
  it('parses mixed-language input with ranges', () => {
    const result = parseScriptureReference('Jn 3:16-18')

    expect(result.ok).toBe(true)
    if (!isSuccessfulParse(result)) return

    expect(result.value.book.id).toBe('JHN')
    expect(result.value.chapter).toBe(3)
    expect(result.value.verseSpans).toEqual([{ startVerse: 16, endVerse: 18 }])
    expect(result.value.normalizedReferenceLabel).toBe('Juan 3:16-18')
  })

  it('supports verse lists and en dash separators', () => {
    const result = parseScriptureReference('Romanos 8:1–2,5')

    expect(result.ok).toBe(true)
    if (!isSuccessfulParse(result)) return

    expect(result.value.verseSpans).toEqual([
      { startVerse: 1, endVerse: 2 },
      { startVerse: 5, endVerse: 5 },
    ])
  })

  it('falls back to full chapter when no verses provided', () => {
    const result = parseScriptureReference('1 Co 13')

    expect(result.ok).toBe(true)
    if (!isSuccessfulParse(result)) return

    expect(result.value.book.id).toBe('1CO')
    expect(result.value.isFullChapter).toBe(true)
    expect(result.value.verseSpans).toHaveLength(0)
  })

  it('accepts Spanish abbreviations such as Sal for Psalms', () => {
    const result = parseScriptureReference('Sal 23')

    expect(result.ok).toBe(true)
    if (!isSuccessfulParse(result)) return

    expect(result.value.book.id).toBe('PSA')
    expect(result.value.isFullChapter).toBe(true)
    expect(result.value.normalizedReferenceLabel).toBe('Salmos 23')
  })

  it('returns an error for unknown books', () => {
    const result = parseScriptureReference('MadeUp 1')

    expect(result.ok).toBe(false)
    if (result.ok) return

    expect(result.error.type).toBe('UnknownBook')
  })

  it('returns an error for malformed chapter values', () => {
    const result = parseScriptureReference('Juan zero')

    expect(result.ok).toBe(false)
    if (result.ok) return

    expect(result.error.type).toBe('InvalidFormat')
  })
})
