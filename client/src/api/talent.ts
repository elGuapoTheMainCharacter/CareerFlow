import { apiFetch } from './http'
import type { PagedResult, TalentItem } from '../types'

export const talentApi = {
  search: (search: string, page: number) => {
    const qs = new URLSearchParams({ page: String(page) })
    if (search) qs.set('search', search)
    return apiFetch<PagedResult<TalentItem>>(`/api/employer/talent?${qs.toString()}`)
  },

  invite: (userId: string, jobOpportunityId: string) =>
    apiFetch<void>(`/api/employer/talent/${userId}/invite`, {
      method: 'POST',
      body: JSON.stringify({ jobOpportunityId }),
    }),

  cvUrl: (userId: string) => `/api/employer/talent/${userId}/cv`,
}