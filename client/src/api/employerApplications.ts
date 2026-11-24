import { apiFetch } from './http'
import type { ApplicationStatus, ReceivedApplication } from '../types'

export const employerApplicationsApi = {
  list: (jobId: string, status: string) => {
    const qs = new URLSearchParams()
    if (jobId) qs.set('jobId', jobId)
    if (status) qs.set('status', status)
    return apiFetch<ReceivedApplication[]>(`/api/employer/applications?${qs.toString()}`)
  },

  setStatus: (id: string, status: ApplicationStatus) =>
    apiFetch<void>(`/api/employer/applications/${id}/status`, {
      method: 'PUT',
      body: JSON.stringify({ status }),
    }),

  resumeUrl: (applicationId: string) => `/api/employer/applications/${applicationId}/resume`,
}