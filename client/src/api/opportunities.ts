import { apiFetch } from './http'
import type { EmploymentType, Opportunity } from '../types'

export type OpportunityInput = {
  title: string
  description?: string
  employmentType: EmploymentType
  salaryMin?: number | null
  salaryMax?: number | null
  closingDate?: string | null
  companyId: string
}

export const opportunitiesApi = {
  all: () => apiFetch<Opportunity[]>('/api/opportunities'),
  create: (data: OpportunityInput) =>
    apiFetch<Opportunity>('/api/opportunities', { method: 'POST', body: JSON.stringify(data) }),
  update: (id: string, data: OpportunityInput) =>
    apiFetch<void>(`/api/opportunities/${id}`, { method: 'PUT', body: JSON.stringify(data) }),
  close: (id: string) => apiFetch<void>(`/api/opportunities/${id}/close`, { method: 'POST' }),
  reopen: (id: string) => apiFetch<void>(`/api/opportunities/${id}/reopen`, { method: 'POST' }),
  remove: (id: string) => apiFetch<void>(`/api/opportunities/${id}`, { method: 'DELETE' }),
}