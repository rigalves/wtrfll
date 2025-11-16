import { beforeEach, describe, expect, it } from 'vitest'
import { createPinia, setActivePinia } from 'pinia'

import { useReferenceInputStore } from '@/stores/referenceInputStore'

describe('referenceInputStore', () => {
  beforeEach(() => {
    setActivePinia(createPinia())
  })

  it('auto publishes draft updates by default', () => {
    const store = useReferenceInputStore()
    store.updateDraftInput('Romanos 8:1-2')

    expect(store.currentInput).toBe('Romanos 8:1-2')
  })

  it('records published references in history without duplicates', () => {
    const store = useReferenceInputStore()
    store.updateDraftInput('Sal 23', { autoPublish: false })
    store.publishDraftInput()
    store.updateDraftInput('Sal 23', { autoPublish: false })
    store.publishDraftInput()

    expect(store.recentHistory[0]).toBe('Sal 23')
    expect(store.recentHistory.filter((ref) => ref === 'Sal 23')).toHaveLength(1)
  })
})
