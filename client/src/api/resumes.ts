import { apiFetch } from './http'
import type { Resume } from '../types'

export const resumesApi = {
  all: () => apiFetch<Resume[]>('/api/resumes'),

  upload: (file: File) => {
    const data = new FormData()
    data.append('file', file)
    return apiFetch<Resume>('/api/resumes', { method: 'POST', body: data })
  },

  remove: (id: string) => apiFetch<void>(`/api/resumes/${id}`, { method: 'DELETE' }),

  // A plain link: the browser sends the login cookie itself.
  downloadUrl: (id: string) => `/api/resumes/${id}/download`,
}