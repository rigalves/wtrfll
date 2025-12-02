<template>
  <div
    class="space-y-2"
    :style="{ fontSize: `${fontScale}rem`, columnCount: columnCount ?? 1, columnGap: '2.5rem' }"
  >
    <p v-if="title" class="font-serif text-amber-200 text-3xl font-bold">{{ title }}</p>
    <p v-if="author" class="text-slate-400 font-medium">{{ author }}</p>
    <div class="space-y-2 text-slate-100 font-semibold">
      <p
        v-for="(line, index) in lines"
        :key="index"
        :class="line.startsWith(commentPrefix) ? 'text-amber-400 italic text-sm uppercase tracking-[0.15em]' : ''"
      >
        {{ line.replace(commentPrefix, '') }}
      </p>
      <p v-if="!lines.length" class="text-slate-500">
        {{ emptyMessage }}
      </p>
    </div>
  </div>
</template>

<script setup lang="ts">
import { computed } from 'vue'

interface Props {
  title?: string | null
  author?: string | null
  lines: string[]
  emptyMessage?: string
  commentPrefix?: string
  fontScale?: number
  columnCount?: number
}

const props = withDefaults(defineProps<Props>(), {
  title: '',
  author: '',
  lines: () => [],
  emptyMessage: 'Add lyrics to preview them here.',
  commentPrefix: '__COMMENT__ ',
  fontScale: 1,
})

const title = computed(() => props.title ?? '')
const author = computed(() => props.author ?? '')
const lines = computed(() => props.lines ?? [])
const emptyMessage = computed(() => props.emptyMessage ?? 'Add lyrics to preview them here.')
const commentPrefix = computed(() => props.commentPrefix ?? '__COMMENT__ ')
const fontScale = computed(() => props.fontScale ?? 1)
const columnCount = computed(() => props.columnCount ?? 1)
</script>
