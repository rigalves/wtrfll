/// <reference types="vite/client" />

interface ImportMetaEnv {
  readonly VITE_API_BASE_URL?: string
  readonly VITE_REALTIME_URL?: string
  readonly VITE_ENABLE_REALTIME?: string
}

interface ImportMeta {
  readonly env: ImportMetaEnv
}
