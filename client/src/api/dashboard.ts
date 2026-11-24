import { apiFetch } from './http'
import type { Dashboard } from '../types'
export const dashboardApi = { get: () => 
    apiFetch<Dashboard>('/api/dashboard') }

