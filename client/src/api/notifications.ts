import { apiFetch } from './http'
import type { NotificationsResponse } from '../types'

export const notificationsApi = {
  list: () => apiFetch<NotificationsResponse>('/api/notifications'),
  unreadCount: () => apiFetch<{ count: number }>('/api/notifications/unread-count'),
  markRead: (id: number) => apiFetch<void>(`/api/notifications/${id}/read`, { method: 'POST' }),
  markAllRead: () => apiFetch<void>('/api/notifications/read-all', { method: 'POST' }),
}