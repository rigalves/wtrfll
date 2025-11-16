<template>
  <section class="mx-auto max-w-5xl px-6 py-16">
    <div class="space-y-6">
      <p class="text-sm uppercase tracking-[0.25em] text-slate-500">Bienvenido</p>
      <h1 class="text-4xl font-semibold">wtrfll: controla tus versiculos en dos vistas sincronizadas</h1>
      <p class="text-base text-slate-300">
        Primero crea una sesion y comparte los enlaces generados. El operador usa Control para preparar y publicar un
        pasaje, mientras que Display refleja el contenido en vivo.
      </p>

      <RouterLink
        class="inline-flex items-center rounded-xl bg-sky-500 px-6 py-3 font-semibold text-white shadow-lg shadow-sky-900/30"
        :to="{ name: 'new-session' }"
      >
        Crear nueva sesion
      </RouterLink>

      <div class="rounded-2xl border border-white/10 bg-white/5 p-6 text-sm text-slate-200">
        <p class="font-semibold">Como funciona?</p>
        <ol class="space-y-2 text-slate-300">
          <li>1. Creas una sesion y compartes los enlaces de Control y Display.</li>
          <li>2. Desde Control escribes una referencia (Juan 3:16-18, Sal 23, etc.).</li>
          <li>3. Display recibe ese estado (SignalR mantendra multiples pantallas en sincronIa).</li>
        </ol>
      </div>
    </div>
  </section>
</template>

<script setup lang="ts">
</script>

