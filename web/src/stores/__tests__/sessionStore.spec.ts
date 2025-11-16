import { beforeEach, describe, expect, it, vi } from 'vitest'
import { createPinia, setActivePinia } from 'pinia'
import { nextTick } from 'vue'

import { useReferenceInputStore } from '@/stores/referenceInputStore'
import { useSessionStore } from '@/stores/sessionStore'

const mockGet = vi.hoisted(() =>
  vi.fn().mockResolvedValue({
    data: {
      reference: 'Juan 3:16-17',
      translation: 'RVR1960',
      cachePolicy: 'public',
      verses: [
        { book: 'Juan', chapter: 3, verse: 16, text: 'Texto 16' },
        { book: 'Juan', chapter: 3, verse: 17, text: 'Texto 17' },
      ],
    },
  }),
)

vi.mock('@/lib/apiClient', () => ({
  apiClient: {
    GET: mockGet,
  },
}))

describe('sessionStore', () => {
beforeEach(() => {
  setActivePinia(createPinia())
  mockGet.mockClear()
})

  it('derives normalized passages from the current input', async () => {
    const referenceStore = useReferenceInputStore()
    const sessionStore = useSessionStore()

    referenceStore.updateDraftInput('Juan 3:16-17', { autoPublish: false })
    sessionStore.publishDraftToSession()
    await nextTick()
    await nextTick()

    expect(sessionStore.currentPassage?.book.id).toBe('JHN')
    expect(sessionStore.displayViewModel.verses).toHaveLength(2)
  })

  it('captures parse errors without crashing', async () => {
    const referenceStore = useReferenceInputStore()
    const sessionStore = useSessionStore()

    referenceStore.updateDraftInput('Unknown', { autoPublish: false })
    sessionStore.publishDraftToSession()
    await nextTick()

    expect(sessionStore.currentPassage).toBeNull()
    expect(sessionStore.lastParseError?.type).toBe('InvalidFormat')
  })
})
