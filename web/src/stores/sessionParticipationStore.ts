import { defineStore } from 'pinia'
import { ref } from 'vue'

import { apiClient } from '@/lib/apiClient'
import { getErrorDetailsFromApiResponse } from '@/lib/httpErrors'
import type { LocalizedMessage } from '@/lib/i18n'
import type { paths } from '../../../shared/typescript/api'

type JoinResponse = paths['/api/sessions/{id}/join']['post']['responses']['200']['content']['application/json']

const STORAGE_KEY = 'wtrfll.join-info'
const storage = typeof window !== 'undefined' ? window.localStorage : null

type StoredJoinInfo = {
  sessionId: string
  joinToken: string
  role: 'controller' | 'display'
  name?: string
  scheduledAt?: string | null
  shortCode?: string | null
}

function loadStoredInfos(): StoredJoinInfo[] {
  if (!storage) {
    return []
  }
  try {
    const raw = storage.getItem(STORAGE_KEY)
    if (!raw) {
      return []
    }
    return JSON.parse(raw) as StoredJoinInfo[]
  } catch {
    return []
  }
}

function persistJoinInfo(info: StoredJoinInfo) {
  if (!storage) {
    return
  }
  const existing = loadStoredInfos().filter(
    (entry) => !(entry.sessionId === info.sessionId && entry.role === info.role),
  )
  existing.push(info)
  storage.setItem(STORAGE_KEY, JSON.stringify(existing))
}

function getStoredJoinInfo(sessionId: string, role: 'controller' | 'display'): StoredJoinInfo | undefined {
  return loadStoredInfos().find((entry) => entry.sessionId === sessionId && entry.role === role)
}

export const useSessionParticipationStore = defineStore('sessionParticipation', () => {
  const activeSessionId = ref<string | null>(null)
  const activeJoinToken = ref<string | null>(null)
  const activeRole = ref<'controller' | 'display' | null>(null)
  const activeSessionName = ref<string | null>(null)
  const activeSessionScheduledAt = ref<string | null>(null)
  const activeSessionShortCode = ref<string | null>(null)
  const joinState = ref<'idle' | 'joining' | 'joined'>('idle')
  const joinErrorMessage = ref<LocalizedMessage | null>(null)

  function setJoinError(key: LocalizedMessage['key']) {
    joinErrorMessage.value = { key }
    joinState.value = 'idle'
  }

  async function joinSession(options: { sessionId: string; joinToken: string; role: 'controller' | 'display' }) {
    if (!options.sessionId) {
      setJoinError('session.joinErrors.missingToken')
      return
    }
    if (!options.joinToken) {
      setJoinError('session.joinErrors.missingToken')
      return
    }
    if (joinState.value === 'joined' && activeSessionId.value === options.sessionId) {
      return
    }

    const stored = getStoredJoinInfo(options.sessionId, options.role)
    if (stored && stored.joinToken === options.joinToken) {
      resumeFromStorage(options)
      return
    }

    joinState.value = 'joining'
    joinErrorMessage.value = null
    activeJoinToken.value = null

    try {
      const { data, error } = await apiClient.POST('/api/sessions/{id}/join', {
        params: { path: { id: options.sessionId } },
        body: { joinToken: options.joinToken, role: options.role },
      })

      if (error) {
        const { statusCode } = getErrorDetailsFromApiResponse(error)
        if (statusCode === 409 && stored && stored.joinToken === options.joinToken) {
          resumeFromStorage(options)
        } else if (statusCode === 409) {
          setJoinError('session.joinErrors.controllerLocked')
        } else if (statusCode === 404) {
          setJoinError('session.joinErrors.notFound')
        } else if (statusCode === 400 && stored && stored.joinToken === options.joinToken) {
          resumeFromStorage(options)
        } else if (statusCode === 400) {
          setJoinError('session.joinErrors.invalidToken')
        } else {
          setJoinError('session.joinErrors.invalidToken')
        }
        return
      }

      const payload = data as JoinResponse | null
      if (!payload) {
        setJoinError('session.joinErrors.invalidToken')
        return
      }

      activeSessionId.value = options.sessionId
      activeJoinToken.value = options.joinToken
      activeRole.value = options.role
      activeSessionName.value = payload.name ?? null
      activeSessionScheduledAt.value = payload.scheduledAt ?? null
      joinState.value = 'joined'
      joinErrorMessage.value = null
      activeSessionShortCode.value = payload.shortCode ?? null
      persistJoinInfo({
        sessionId: options.sessionId,
        joinToken: options.joinToken,
        role: options.role,
        name: payload.name ?? undefined,
        scheduledAt: payload.scheduledAt ?? null,
        shortCode: payload.shortCode ?? null,
      })
    } catch {
      const fallback = getStoredJoinInfo(options.sessionId, options.role)
      if (fallback && fallback.joinToken === options.joinToken) {
        resumeFromStorage(options)
      } else {
        setJoinError('session.joinErrors.invalidToken')
      }
    }
  }

  function reset() {
    activeSessionId.value = null
    activeJoinToken.value = null
    activeRole.value = null
    activeSessionName.value = null
    activeSessionScheduledAt.value = null
    activeSessionShortCode.value = null
    joinState.value = 'idle'
    joinErrorMessage.value = null
  }

  return {
    activeSessionId,
    activeJoinToken,
    activeRole,
    activeSessionName,
    activeSessionScheduledAt,
    activeSessionShortCode,
    joinState,
    joinErrorMessage,
    joinSession,
    getStoredJoinToken: (sessionId: string, role: 'controller' | 'display') =>
      getStoredJoinInfo(sessionId, role)?.joinToken ?? null,
    setJoinError,
    reset,
  }

  function resumeFromStorage(options: { sessionId: string; joinToken: string; role: 'controller' | 'display' }) {
    const stored = getStoredJoinInfo(options.sessionId, options.role)
    activeSessionId.value = options.sessionId
    activeJoinToken.value = options.joinToken
    activeRole.value = options.role
    activeSessionName.value = stored?.name ?? null
    activeSessionScheduledAt.value = stored?.scheduledAt ?? null
    activeSessionShortCode.value = stored?.shortCode ?? null
    joinState.value = 'joined'
    joinErrorMessage.value = null
  }
})
