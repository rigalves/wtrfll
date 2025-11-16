import { fileURLToPath, URL } from 'node:url'

import vue from '@vitejs/plugin-vue'
import { defineConfig } from 'vite'

export default defineConfig({
  plugins: [vue()],
  resolve: {
    alias: {
      '@': fileURLToPath(new URL('./src', import.meta.url)),
    },
  },
  server: {
    proxy: {
      '/api': {
        target: process.env.WTRFLL_DEV_API_URL ?? 'http://localhost:5065',
        changeOrigin: false,
      },
      '/realtime': {
        target: process.env.WTRFLL_DEV_API_URL ?? 'http://localhost:5065',
        ws: true,
      },
    },
  },
  test: {
    globals: true,
    environment: 'node',
    setupFiles: [],
    coverage: {
      reporter: ['text', 'lcov'],
    },
  },
})
