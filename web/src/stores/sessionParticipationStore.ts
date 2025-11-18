import { defineStore } from 'pinia'
import { ref } from 'vue'

import { apiClient } from '@/lib/apiClient'
import { getErrorDetailsFromApiResponse } from '@/lib/httpErrors'
import type { LocalizedMessage } from '@/lib/i18n'
import type { paths } from '../../../shared/typescript/api'

type JoinResponse = paths['/api/sessions/{id}/join']['post']['responses']['200']['content']['application/json']

export const useSessionParticipationStore = defineStore('sessionParticipation', () => {
  const activeSessionId = ref<string | null>(null)
  const activeJoinToken = ref<string | null>(null)
  const activeRole = ref<'controller' | 'display' | null>(null)
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
        if (statusCode === 409) {
          setJoinError('session.joinErrors.controllerLocked')
        } else if (statusCode === 404) {
          setJoinError('session.joinErrors.notFound')
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
      joinState.value = 'joined'
      joinErrorMessage.value = null
    } catch (error) {
      setJoinError('session.joinErrors.invalidToken')
    }
  }

  function reset() {
    activeSessionId.value = null
    activeJoinToken.value = null
    activeRole.value = null
    joinState.value = 'idle'
    joinErrorMessage.value = null
  }

  return {
    activeSessionId,
    activeJoinToken,
    activeRole,
    joinState,
    joinErrorMessage,
    joinSession,
    setJoinError,
    reset,
  }
})
