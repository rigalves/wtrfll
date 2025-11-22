import { defineStore } from 'pinia'
import { computed, ref } from 'vue'

export type SessionSlideType = 'passage' | 'lyrics'

export interface SessionSlide {
  id: string
  type: SessionSlideType
  label: string
  payload: Record<string, unknown>
}

export const useSessionSlidesStore = defineStore('sessionSlides', () => {
  const slides = ref<SessionSlide[]>([])
  const activeSlideId = ref<string | null>(null)
  const draftSlide = ref<SessionSlide | null>(null)

  function addSlide(slide: SessionSlide) {
    slides.value.push(slide)
    activeSlideId.value = slide.id
  }

  function setActiveSlide(id: string) {
    activeSlideId.value = id
  }

  function updateSlide(id: string, payload: Partial<SessionSlide>) {
    const index = slides.value.findIndex((slide) => slide.id === id)
    if (index === -1) {
      return
    }
    slides.value[index] = { ...slides.value[index], ...payload }
  }

  function removeSlide(id: string) {
    slides.value = slides.value.filter((slide) => slide.id !== id)
    if (activeSlideId.value === id) {
      activeSlideId.value = slides.value[0]?.id ?? null
    }
  }

  const currentSlide = computed<SessionSlide | null>(() => {
    if (!activeSlideId.value) {
      return null
    }
    return slides.value.find((slide) => slide.id === activeSlideId.value) ?? null
  })

  function setDraftSlide(slide: SessionSlide | null) {
    draftSlide.value = slide
  }

  function commitDraftSlide() {
    if (!draftSlide.value) {
      return
    }
    addSlide(draftSlide.value)
    draftSlide.value = null
  }

  return {
    slides,
    activeSlideId,
    currentSlide,
    draftSlide,
    addSlide,
    setActiveSlide,
    updateSlide,
    removeSlide,
    setDraftSlide,
    commitDraftSlide,
  }
})
