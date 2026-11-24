import { apiFetch } from './http'
import type { ApplicationDetail, ApplicationListItem, PagedResult } from '../types'

export type ApplyInput = {
  jobOpportunityId: string
  resumeDocumentId: string
  coverLetter?: string
}

export type ReminderUpdate = {
  nextActionDate?: string | null
  sourceUrl?: string | null
}

export const applicationsApi = {
  search: (qs: string) => apiFetch<PagedResult<ApplicationListItem>>(`/api/applications?${qs}`),
  get: (id: string) => apiFetch<ApplicationDetail>(`/api/applications/${id}`),
  apply: (data: ApplyInput) =>
    apiFetch<ApplicationDetail>('/api/applications', { method: 'POST', body: JSON.stringify(data) }),
  update: (id: string, data: ReminderUpdate) =>
    apiFetch<void>(`/api/applications/${id}`, { method: 'PUT', body: JSON.stringify(data) }),
  withdraw: (id: string) =>
    apiFetch<void>(`/api/applications/${id}/withdraw`, { method: 'POST' }),
  addNote: (id: string, body: string) =>
    apiFetch(`/api/applications/${id}/notes`, { method: 'POST', body: JSON.stringify({ body }) }),
}