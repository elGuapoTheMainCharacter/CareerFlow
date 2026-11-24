import { apiFetch } from './http'
import type { PublicJob, PublicJobDetail } from '../types'

export const publicJobsApi = {
  latest: (take = 6) => apiFetch<PublicJob[]>(`/api/public/jobs?take=${take}`),
  get: (id: string) => apiFetch<PublicJobDetail>(`/api/public/jobs/${id}`),
}