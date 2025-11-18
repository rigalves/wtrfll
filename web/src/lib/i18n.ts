import { createI18n } from 'vue-i18n'

import en from '@/locales/en.json'
import es from '@/locales/es.json'

const STORAGE_KEY = 'wtrfll.locale'
type SupportedLocale = 'en' | 'es'
const browserLocale: SupportedLocale = navigator.language?.startsWith('es') ? 'es' : 'en'
const savedLocale = localStorage.getItem(STORAGE_KEY)
const initialLocale: SupportedLocale =
  savedLocale === 'es' || savedLocale === 'en' ? savedLocale : browserLocale

export const i18n = createI18n({
  legacy: false,
  locale: initialLocale,
  fallbackLocale: 'en',
  messages: {
    en,
    es,
  },
})

export type LocalizedMessage = {
  key: string
  params?: Record<string, string | number>
}

export function setLocale(locale: string) {
  const next = (locale === 'es' || locale === 'en' ? locale : 'en') as SupportedLocale
  i18n.global.locale.value = next
  localStorage.setItem(STORAGE_KEY, next)
}

export function getCurrentLocale(): string {
  return i18n.global.locale.value as string
}
