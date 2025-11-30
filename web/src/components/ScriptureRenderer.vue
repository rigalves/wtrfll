<template>
  <div class="space-y-2" :style="{ fontSize: `${fontScale}rem` }">
    <p v-if="reference" class="font-serif text-amber-100 text-3xl font-semibold">{{ reference }}</p>
    <div class="space-y-2 text-slate-100">
      <p
        v-for="(verse, index) in verses"
        :key="verse.verse ?? index"
        :class="currentIndex !== null && currentIndex === index ? 'text-amber-200' : ''"
      >
        <span v-if="showVerseNumbers && verse.verse" class="mr-2 text-slate-400">{{ verse.verse }}</span>
        {{ verse.text }}
      </p>
      <p v-if="!verses.length" class="text-slate-500">
        {{ emptyMessage }}
      </p>
    </div>
  </div>
</template>

<script setup lang="ts">
import { computed } from 'vue'

interface VerseLine {
  verse?: number | string
  text: string
}

interface Props {
  reference?: string | null
  verses: VerseLine[]
  currentIndex?: number | null
  showVerseNumbers?: boolean
  emptyMessage?: string
  fontScale?: number
}

const props = withDefaults(defineProps<Props>(), {
  reference: '',
  currentIndex: null,
  showVerseNumbers: true,
  emptyMessage: 'No content to display.',
  fontScale: 1,
})

const reference = computed(() => props.reference ?? '')
const verses = computed(() => props.verses ?? [])
const currentIndex = computed(() => (props.currentIndex === null ? null : props.currentIndex ?? null))
const showVerseNumbers = computed(() => props.showVerseNumbers ?? true)
const emptyMessage = computed(() => props.emptyMessage ?? 'No content to display.')
const fontScale = computed(() => props.fontScale ?? 1)
</script>
