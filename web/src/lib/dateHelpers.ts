export function toUtcDate(value: string): Date {
  if (/Z|[+-]\d{2}:\d{2}$/.test(value)) {
    return new Date(value)
  }
  return new Date(`${value}Z`)
}

