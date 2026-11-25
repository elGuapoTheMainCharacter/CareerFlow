let csrfToken: string | null = null

export class ApiError extends Error {
  status: number
  details?: Record<string, string[]>

  constructor(status: number, message: string, details?: Record<string, string[]>) {
    super(message)
    this.status = status
    this.details = details
  }
}
async function getCsrfToken() {
  if (csrfToken) return csrfToken
  const response = await fetch('/api/security/csrf', { credentials: 'include' })
  if (!response.ok) throw new ApiError(response.status, 'Could not obtain CSRF token.')
  const data = await response.json()
  csrfToken = data.token
  return csrfToken!
}

export function resetCsrfToken() {
  csrfToken = null
}

export async function apiFetch<T>(url: string, options: RequestInit = {}): Promise<T> {
  const method = (options.method ?? 'GET').toUpperCase()
  const headers = new Headers(options.headers)

  if (options.body && !(options.body instanceof FormData))
    headers.set('Content-Type', 'application/json')

  if (!['GET', 'HEAD', 'OPTIONS'].includes(method))
    headers.set('X-CSRF-TOKEN', await getCsrfToken())

  const response = await fetch(url, {
    ...options,
    headers,
    credentials: 'include',
  })

  if (response.status === 204) return undefined as T

  const contentType = response.headers.get('content-type') ?? ''
  const body = contentType.includes('json') ? await response.json() : null

  if (!response.ok) {
    const details = body?.errors as Record<string, string[]> | undefined
    const detailText = details ? Object.values(details).flat().join(' ') : ''
    const message =
      body?.message ?? (detailText || body?.title) ?? `Request failed (${response.status}).`
    throw new ApiError(response.status, message, details)
  }

  return body as T
}
