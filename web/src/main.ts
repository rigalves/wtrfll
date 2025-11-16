import { createPinia, setActivePinia } from 'pinia'
import { createApp } from 'vue'

import { useBibleBooksStore } from '@/stores/bibleBooksStore'
import App from './App.vue'
import './assets/main.css'
import { router } from './router'

const app = createApp(App)
const pinia = createPinia()

app.use(pinia)
setActivePinia(pinia)

app.use(router).mount('#app')

const bibleBooksStore = useBibleBooksStore()
bibleBooksStore.ensureLoaded().catch(() => {
  // The UI will surface the error and allow retry.
})
