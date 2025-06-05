<script>
import { computed } from 'vue'
import { useBibleStore } from '../../stores/bible'

export default {
  setup() {
    const bible = useBibleStore()
    const verse = computed(() => bible.verseText)
    return { verse }
  }
}
</script>

<template>
  <div>{{ verse }}</div>
</template>
