import createClient from 'openapi-fetch'

import type { paths } from '../../../shared/typescript/api'

const defaultBaseUrl = import.meta.env.VITE_API_BASE_URL ?? ''

export const apiClient = createClient<paths>({
  baseUrl: defaultBaseUrl,
})

export type ApiClient = typeof apiClient
