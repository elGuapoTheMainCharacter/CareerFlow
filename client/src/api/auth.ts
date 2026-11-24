import { apiFetch, resetCsrfToken } from './http'
import type { AccountType, User } from '../types'

export const authApi = {
  me: () => apiFetch<User>('/api/auth/me'),

  async login(email: string, password: string, rememberMe: boolean) {
    const user = await apiFetch<User>('/api/auth/login', {
      method: 'POST',
      body: JSON.stringify({ email, password, rememberMe }),
    })
    resetCsrfToken()
    return user
  },

  async register(
    displayName: string,
    email: string,
    password: string,
    accountType: AccountType,
    isDiscoverable = false,
  ) {
    const user = await apiFetch<User>('/api/auth/register', {
      method: 'POST',
      body: JSON.stringify({ displayName, email, password, accountType, isDiscoverable }),
    })
    resetCsrfToken()
    return user
  },

  async logout() {
    await apiFetch<void>('/api/auth/logout', { method: 'POST' })
    resetCsrfToken()
  },
}