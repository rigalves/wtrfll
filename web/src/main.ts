import { createPinia, setActivePinia } from 'pinia'
import { createApp } from 'vue'

import { i18n } from '@/lib/i18n'
import { useBibleBooksStore } from '@/stores/bibleBooksStore'
import App from './App.vue'
import './assets/main.css'
import { router } from './router'

const app = createApp(App)
const pinia = createPinia()

app.use(pinia)
setActivePinia(pinia)

app.use(i18n)
app.use(router).mount('#app')

const bibleBooksStore = useBibleBooksStore()
void bibleBooksStore.ensureLoaded()
