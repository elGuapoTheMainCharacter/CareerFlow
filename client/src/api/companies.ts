import { apiFetch } from './http'
import type { Company } from '../types'
export type CompanyInput = { name: string; website?: string; location?: string }
export const companiesApi = {
 all: () => apiFetch<Company[]>('/api/companies'),
 create: (data: CompanyInput) => apiFetch<Company>('/api/companies', { method: 'POST', body: JSON.stringify(data) }),
 update: (id: string, data: CompanyInput) => apiFetch<void>(`/api/companies/${id}`, { method: 'PUT', body:
JSON.stringify(data) }),
 remove: (id: string) => apiFetch<void>(`/api/companies/${id}`, { method: 'DELETE' }),
}