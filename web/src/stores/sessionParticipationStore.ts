import { defineStore } from 'pinia'
import { ref } from 'vue'

import { apiClient } from '@/lib/apiClient'
import { getErrorDetailsFromApiResponse } from '@/lib/httpErrors'
import type { paths } from '../../../shared/typescript/api'

type JoinResponse = paths['/api/sessions/{id}/join']['post']['responses']['200']['content']['application/json']

export const useSessionParticipationStore = defineStore('sessionParticipation', () => {
  const activeSessionId = ref<string | null>(null)
  const activeJoinToken = ref<string | null>(null)
  const activeRole = ref<'controller' | 'display' | null>(null)
  const joinState = ref<'idle' | 'joining' | 'joined'>('idle')
  const joinError = ref<string | null>(null)

  function setJoinError(message: string) {
    joinError.value = message
    joinState.value = 'idle'
  }

  async function joinSession(options: { sessionId: string; joinToken: string; role: 'controller' | 'display' }) {
    if (!options.sessionId) {
      setJoinError('Falta el id de sesion.')
      return
    }
    if (!options.joinToken) {
      setJoinError('Falta el token de union en la URL.')
      return
    }
    if (joinState.value === 'joined' && activeSessionId.value === options.sessionId) {
      return
    }

    joinState.value = 'joining'
    joinError.value = null
    activeJoinToken.value = null

    try {
      const { data, error } = await apiClient.POST('/api/sessions/{id}/join', {
        params: { path: { id: options.sessionId } },
        body: { joinToken: options.joinToken, role: options.role },
      })

      if (error) {
        const { statusCode, message } = getErrorDetailsFromApiResponse(error)
        if (statusCode === 409) {
          setJoinError('El controlador ya esta conectado a esta sesion.')
        } else if (statusCode === 404) {
          setJoinError('Sesion no encontrada.')
        } else if (statusCode === 400) {
          setJoinError(message ?? 'Token invalido.')
        } else {
          setJoinError('No se pudo unir a la sesion.')
        }
        return
      }

      const payload = data as JoinResponse | null
      if (!payload) {
        setJoinError('Respuesta inesperada del servidor.')
        return
      }

      activeSessionId.value = options.sessionId
      activeJoinToken.value = options.joinToken
      activeRole.value = options.role
      joinState.value = 'joined'
      joinError.value = null
    } catch (error) {
      setJoinError(error instanceof Error ? error.message : 'No se pudo unir a la sesion.')
    }
  }

  function reset() {
    activeSessionId.value = null
    activeJoinToken.value = null
    activeRole.value = null
    joinState.value = 'idle'
    joinError.value = null
  }

  return {
    activeSessionId,
    activeJoinToken,
    activeRole,
    joinState,
    joinError,
    joinSession,
    setJoinError,
    reset,
  }
})
