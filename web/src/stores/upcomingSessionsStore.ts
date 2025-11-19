import { defineStore } from 'pinia'
import { computed, ref } from 'vue'

import { apiClient } from '@/lib/apiClient'
import { toUtcDate } from '@/lib/dateHelpers'
import type { paths } from '../../../shared/typescript/api'

type UpcomingSessionsResponse =
  paths['/api/sessions/upcoming']['get']['responses']['200']['content']['application/json']
type UpcomingSession = UpcomingSessionsResponse extends Array<infer T> ? T : never
type SessionCreatedResponse =
  paths['/api/sessions']['post']['responses']['201']['content']['application/json']

export const useUpcomingSessionsStore = defineStore('upcomingSessions', () => {
  const sessions = ref<UpcomingSession[]>([])
  const isLoading = ref(false)
  const loadErrorMessageKey = ref<string | null>(null)
  let hasLoadedOnce = false

  const groupedByDate = computed(() => {
    const groups = new Map<string, UpcomingSession[]>()
    for (const session of orderedSessions.value) {
      const dateKey = (session.scheduledAt ?? session.createdAt).slice(0, 10)
      const list = groups.get(dateKey)
      if (list) {
        list.push(session)
      } else {
        groups.set(dateKey, [session])
      }
    }
    return groups
  })

  const orderedSessions = computed(() =>
    [...sessions.value].sort((a, b) => {
      const first = toUtcDate(a.scheduledAt ?? a.createdAt).getTime()
      const second = toUtcDate(b.scheduledAt ?? b.createdAt).getTime()
      return second - first
    }),
  )

  async function loadUpcomingSessions() {
    if (isLoading.value) {
      return
    }
    isLoading.value = true
    loadErrorMessageKey.value = null
    try {
      const { data, error } = await apiClient.GET('/api/sessions/upcoming', {})
      if (error) {
        loadErrorMessageKey.value = 'landing.errors.loadUpcomingFailed'
        return
      }
      sessions.value = (data ?? []).map(normalizeSession)
      prunePastSessions()
      hasLoadedOnce = true
    } catch {
      loadErrorMessageKey.value = 'landing.errors.loadUpcomingFailed'
    } finally {
      isLoading.value = false
    }
  }

  function ensureLoaded() {
    if (!hasLoadedOnce) {
      void loadUpcomingSessions()
    }
  }

  function upsertSession(session: UpcomingSession) {
    const next = normalizeSession(session)
    const existingIndex = sessions.value.findIndex((entry) => entry.id === next.id)
    if (existingIndex >= 0) {
      sessions.value.splice(existingIndex, 1, next)
    } else {
      sessions.value.push(next)
    }
    prunePastSessions()
  }

  function registerCreatedSession(created: SessionCreatedResponse) {
    upsertSession({
      id: created.id,
      name: created.name,
      shortCode: created.shortCode,
      controllerJoinToken: created.controllerJoinToken,
      displayJoinToken: created.displayJoinToken,
      createdAt: created.createdAt,
      scheduledAt: created.scheduledAt ?? null,
    })
  }

  function prunePastSessions(reference: Date = new Date()) {
    const scheduleCutoffTimestamp = reference.getTime()
    const localDayStart = new Date(reference.getFullYear(), reference.getMonth(), reference.getDate()).getTime()
    sessions.value = sessions.value.filter((session) => {
      const scheduledTimestamp = session.scheduledAt ? toUtcDate(session.scheduledAt).getTime() : null
      const createdTimestamp = toUtcDate(session.createdAt).getTime()
      if (scheduledTimestamp !== null) {
        return scheduledTimestamp >= scheduleCutoffTimestamp
      }
      return createdTimestamp >= localDayStart
    })
  }

  return {
    sessions,
    groupedByDate,
    orderedSessions,
    isLoading,
    loadErrorMessageKey,
    ensureLoaded,
    loadUpcomingSessions,
    upsertSession,
    registerCreatedSession,
  }
})

function normalizeSession(session: UpcomingSession): UpcomingSession {
  return {
    ...session,
    scheduledAt: session.scheduledAt ?? null,
  }
}
