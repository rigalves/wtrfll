import type { BibleBookMetadata } from '@/stores/bibleBooksStore'
import { findBibleBookMetadata } from '@/stores/bibleBooksStore'

export type ReferenceParseErrorType =
  | 'EmptyInput'
  | 'InvalidFormat'
  | 'UnknownBook'
  | 'InvalidChapter'
  | 'InvalidVerseSegment'

export interface ReferenceParseError {
  readonly type: ReferenceParseErrorType
  readonly message: string
  readonly details?: Record<string, unknown>
}

export interface VerseSpan {
  readonly startVerse: number
  readonly endVerse: number
}

export interface ParsedScriptureReference {
  readonly rawInput: string
  readonly normalizedInput: string
  readonly book: BibleBookMetadata
  readonly chapter: number
  readonly verseSpans: VerseSpan[]
  readonly isFullChapter: boolean
  readonly normalizedReferenceLabel: string
}

export type ParseReferenceResult =
  | { ok: true; value: ParsedScriptureReference }
  | { ok: false; error: ReferenceParseError }

const rangeSeparatorPattern = /[-\u2013\u2014]/

export function parseScriptureReference(input: string): ParseReferenceResult {
  const sanitizedInput = sanitizeInput(input)
  if (!sanitizedInput) {
    return createResultError('EmptyInput', 'Please type a scripture reference (e.g., "Juan 3:16").')
  }

  const splitResult = splitBookAndReferencePortion(sanitizedInput)
  if (!splitResult) {
    return createResultError(
      'InvalidFormat',
      'Use "Book Chapter" or "Book Chapter:Verse" format (e.g., "Juan 3:16").',
    )
  }

  const { bookPart, numbersPart } = splitResult
  const bookMetadata = findBibleBookMetadata(bookPart)
  if (!bookMetadata) {
    return createResultError('UnknownBook', `Could not recognize the book name "${bookPart.trim()}".`)
  }

  const chapterResult = parseChapterAndVerses(numbersPart)
  if (!chapterResult.ok) {
    return chapterResult
  }

  const { chapter, verseSpans } = chapterResult.value

  const normalizedReferenceLabel = buildNormalizedLabel(bookMetadata, chapter, verseSpans)

  return {
    ok: true,
    value: {
      rawInput: input,
      normalizedInput: sanitizedInput,
      book: bookMetadata,
      chapter,
      verseSpans,
      isFullChapter: verseSpans.length === 0,
      normalizedReferenceLabel,
    },
  }
}

function sanitizeInput(value: string): string {
  return value
    .replace(/[\u2013\u2014]/g, '-')
    .replace(/\s+/g, ' ')
    .trim()
}

function splitBookAndReferencePortion(input: string): { bookPart: string; numbersPart: string } | undefined {
  const match = input.match(/(\d[\d\s:,;\-\u2013\u2014]*)$/u)
  if (!match || match.index === undefined) {
    return undefined
  }

  const bookPart = input.slice(0, match.index).trim()
  const numbersPart = match[0]?.trim() ?? ''

  if (!bookPart || !numbersPart) {
    return undefined
  }

  return { bookPart, numbersPart }
}

type ChapterParseResult =
  | { ok: true; value: { chapter: number; verseSpans: VerseSpan[] } }
  | { ok: false; error: ReferenceParseError }

function parseChapterAndVerses(numbersPortion: string): ChapterParseResult {
  const [chapterPartRaw, versesPartRaw] = numbersPortion.split(':')
  const chapter = Number.parseInt(chapterPartRaw.trim(), 10)

  if (!Number.isInteger(chapter) || chapter <= 0) {
    return { ok: false, error: createReferenceError('InvalidChapter', 'Chapter must be a positive integer.') }
  }

  if (!versesPartRaw || !versesPartRaw.trim()) {
    return { ok: true, value: { chapter, verseSpans: [] } }
  }

  const segments = versesPartRaw.split(/[;,]/)
  const verseSpans: VerseSpan[] = []

  for (const segment of segments) {
    const trimmedSegment = segment.trim()
    if (!trimmedSegment) {
      continue
    }

    const rangeParts = trimmedSegment.split(rangeSeparatorPattern)
    const startVerse = Number.parseInt(rangeParts[0]?.trim(), 10)

    if (!Number.isInteger(startVerse) || startVerse <= 0) {
      return {
        ok: false,
        error: createReferenceError('InvalidVerseSegment', `Invalid verse number "${segment}".`),
      }
    }

    let endVerse = startVerse
    if (rangeParts.length > 1) {
      endVerse = Number.parseInt(rangeParts[1]?.trim(), 10)
      if (!Number.isInteger(endVerse) || endVerse <= 0) {
        return {
          ok: false,
          error: createReferenceError('InvalidVerseSegment', `Invalid verse number "${segment}".`),
        }
      }
      if (endVerse < startVerse) {
        return {
          ok: false,
          error: createReferenceError('InvalidVerseSegment', `Verse range "${segment}" must ascend.`),
        }
      }
    }

    verseSpans.push({ startVerse, endVerse })
  }

  verseSpans.sort((a, b) => a.startVerse - b.startVerse)

  return { ok: true, value: { chapter, verseSpans } }
}

function buildNormalizedLabel(book: BibleBookMetadata, chapter: number, verseSpans: VerseSpan[]): string {
  const bookLabel = book.spanishDisplayName || book.englishDisplayName
  if (verseSpans.length === 0) {
    return `${bookLabel} ${chapter}`
  }

  const spanLabel = verseSpans
    .map((span) => (span.startVerse === span.endVerse ? `${span.startVerse}` : `${span.startVerse}-${span.endVerse}`))
    .join(',')

  return `${bookLabel} ${chapter}:${spanLabel}`
}

function createReferenceError(
  type: ReferenceParseErrorType,
  message: string,
  details?: Record<string, unknown>,
): ReferenceParseError {
  return { type, message, details }
}

function createResultError(
  type: ReferenceParseErrorType,
  message: string,
  details?: Record<string, unknown>,
): ParseReferenceResult {
  return {
    ok: false,
    error: createReferenceError(type, message, details),
  }
}

export function isSuccessfulParse(result: ParseReferenceResult): result is { ok: true; value: ParsedScriptureReference } {
  return result.ok
}

export function summarizeParsedReference(result: ParsedScriptureReference): string {
  const base = `${result.book.spanishDisplayName} ${result.chapter}`
  if (result.isFullChapter) {
    return base
  }

  const rangeSummary = result.verseSpans
    .map((span) => (span.startVerse === span.endVerse ? `${span.startVerse}` : `${span.startVerse}-${span.endVerse}`))
    .join(', ')
  return `${base}:${rangeSummary}`
}
