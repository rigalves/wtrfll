<template>
  <section class="mx-auto max-w-4xl px-6 py-16 space-y-6">
    <header class="space-y-2">
      <p class="text-xs uppercase tracking-[0.4em] text-slate-500">Nueva sesion</p>
      <h1 class="text-3xl font-semibold">Genera enlaces para Control y Display</h1>
      <p class="text-slate-400 text-base">
        Crea una sesion para este servicio y comparte los enlaces para el controlador y la pantalla. Cada enlace incluye
        un token unico que debe acompanar a la URL.
      </p>
    </header>

    <div class="rounded-2xl border border-white/10 bg-white/5 p-6 space-y-4">
      <button
        type="button"
        class="rounded-lg bg-sky-500 px-5 py-3 font-semibold text-white disabled:opacity-40"
        :disabled="isCreating"
        @click="createSession"
      >
        {{ isCreating ? 'Creando...' : 'Crear sesion' }}
      </button>
      <p v-if="errorMessage" class="text-sm text-rose-300">{{ errorMessage }}</p>

      <div v-if="session" class="space-y-4">
        <div>
          <p class="text-sm text-slate-400">Codigo corto</p>
          <p class="text-2xl font-semibold">{{ session.shortCode }}</p>
          <p class="text-xs text-slate-500">Creada {{ createdAtLabel }}</p>
        </div>

        <div class="grid gap-4 md:grid-cols-2">
          <div class="rounded-xl border border-white/15 bg-black/20 p-4">
            <p class="text-sm uppercase tracking-[0.3em] text-slate-500">Control</p>
            <p class="text-sm text-slate-400">Comparte este enlace con el operador</p>
            <code class="mt-2 block break-all rounded bg-black/30 p-3 text-xs text-slate-200">
              {{ controllerLink }}
            </code>
          </div>
          <div class="rounded-xl border border-white/15 bg-black/20 p-4">
            <p class="text-sm uppercase tracking-[0.3em] text-slate-500">Display</p>
            <p class="text-sm text-slate-400">Comparte este enlace con la pantalla / Chromecast</p>
            <code class="mt-2 block break-all rounded bg-black/30 p-3 text-xs text-slate-200">
              {{ displayLink }}
            </code>
          </div>
        </div>
      </div>
    </div>
  </section>
</template>

<script setup lang="ts">
import { computed, ref } from 'vue'

import { apiClient } from '@/lib/apiClient'
import type { paths } from '../../../shared/typescript/api'

type CreateSessionResponse = paths['/api/sessions']['post']['responses']['201']['content']['application/json']

const isCreating = ref(false)
const errorMessage = ref<string | null>(null)
const session = ref<CreateSessionResponse | null>(null)

const controllerLink = computed(() => {
  if (!session.value) return ''
  return `${window.location.origin}/control/${session.value.id}?token=${session.value.controllerJoinToken}`
})

const displayLink = computed(() => {
  if (!session.value) return ''
  return `${window.location.origin}/display/${session.value.id}?token=${session.value.displayJoinToken}`
})

const createdAtLabel = computed(() =>
  session.value ? new Date(session.value.createdAt).toLocaleString() : '',
)

async function createSession() {
  isCreating.value = true
  errorMessage.value = null
  try {
    const response = await apiClient.POST('/api/sessions', {})
    if (response.error) {
      errorMessage.value = response.error.error ?? 'No se pudo crear la sesion'
      session.value = null
      return
    }
    session.value = response.data ?? null
  } catch (error) {
    errorMessage.value =
      error instanceof Error ? error.message : 'Ocurrio un error inesperado al crear la sesion'
    session.value = null
  } finally {
    isCreating.value = false
  }
}
</script>


