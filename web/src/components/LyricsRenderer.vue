<template>
  <div class="space-y-2">
    <p v-if="title" class="text-lg font-semibold text-white">{{ title }}</p>
    <p v-if="author" class="text-sm text-slate-400">{{ author }}</p>
    <div class="space-y-2 text-base text-slate-100">
      <p
        v-for="(line, index) in lines"
        :key="index"
        :class="line.startsWith(commentPrefix) ? 'text-slate-400 italic text-sm uppercase tracking-[0.15em]' : ''"
      >
        {{ line.replace(commentPrefix, '') }}
      </p>
      <p v-if="!lines.length" class="text-sm text-slate-500">
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
}

const props = withDefaults(defineProps<Props>(), {
  title: '',
  author: '',
  lines: () => [],
  emptyMessage: 'Add lyrics to preview them here.',
  commentPrefix: '__COMMENT__ ',
})

const title = computed(() => props.title ?? '')
const author = computed(() => props.author ?? '')
const lines = computed(() => props.lines ?? [])
const emptyMessage = computed(() => props.emptyMessage ?? 'Add lyrics to preview them here.')
const commentPrefix = computed(() => props.commentPrefix ?? '__COMMENT__ ')
</script>
