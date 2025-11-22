const directivePattern = /\{[^}]+\}/g
const chordPattern = /\[[^\]]+\]/g
const commentDirectivePattern = /\{comment:([^}]+)\}/i

export function extractChordProLines(text: string): string[] {
  if (!text) return []
  return text
    .split(/\r?\n/)
    .map((line) => {
      const commentMatch = line.match(commentDirectivePattern)
      const commentText = commentMatch?.[1]?.trim()
      const withoutDirectives = line.replace(directivePattern, '')
      const withoutChords = withoutDirectives.replace(chordPattern, '')
      const cleaned = withoutChords.replace(/\|/g, ' ').trim()
      if (cleaned.length > 0) {
        return cleaned
      }
      if (commentText) {
        return `__COMMENT__ ${commentText}`
      }
      return ''
    })
    .filter((line) => line.length > 0)
}
