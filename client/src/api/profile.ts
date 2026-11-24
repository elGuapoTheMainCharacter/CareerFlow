import { apiFetch } from './http'
import type { Profile } from '../types'

export const profileApi = {
  get: () => apiFetch<Profile>('/api/profile'),
  setVisibility: (isDiscoverable: boolean) =>
    apiFetch<void>('/api/profile/visibility', {
      method: 'PUT',
      body: JSON.stringify({ isDiscoverable }),
    }),
}