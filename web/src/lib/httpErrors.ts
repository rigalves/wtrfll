type KnownErrorResponse = {
  response?: { status?: number }
  error?: string
}

export function getErrorDetailsFromApiResponse(error: unknown): { statusCode?: number; message?: string } {
  const typed = error as KnownErrorResponse
  return {
    statusCode: typed.response?.status,
    message: typed.error,
  }
}
