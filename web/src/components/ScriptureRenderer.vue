<template>
  <div class="space-y-2">
    <p v-if="reference" class="text-lg font-semibold text-white">{{ reference }}</p>
    <div class="space-y-2 text-base text-slate-100">
      <p
        v-for="(verse, index) in verses"
        :key="verse.verse ?? index"
        :class="currentIndex !== null && currentIndex === index ? 'text-sky-300' : ''"
      >
        <span v-if="showVerseNumbers && verse.verse" class="mr-2 text-slate-400">{{ verse.verse }}</span>
        {{ verse.text }}
      </p>
      <p v-if="!verses.length" class="text-sm text-slate-500">
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
}

const props = withDefaults(defineProps<Props>(), {
  reference: '',
  currentIndex: null,
  showVerseNumbers: true,
  emptyMessage: 'No content to display.',
})

const reference = computed(() => props.reference ?? '')
const verses = computed(() => props.verses ?? [])
const currentIndex = computed(() => (props.currentIndex === null ? null : props.currentIndex ?? null))
const showVerseNumbers = computed(() => props.showVerseNumbers ?? true)
const emptyMessage = computed(() => props.emptyMessage ?? 'No content to display.')
</script>
