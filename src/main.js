import { createApp } from 'vue'
import { createPinia } from 'pinia'
import App from './App.vue'
import { Quasar } from 'quasar'
import quasarUserOptions from './quasar-user-options'

const pinia = createPinia()
const app = createApp(App)

app.use(pinia)
app.use(Quasar, quasarUserOptions)

app.mount('#app')
