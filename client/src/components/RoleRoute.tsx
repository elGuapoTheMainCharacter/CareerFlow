import { Navigate, Outlet } from 'react-router'
import { useQuery } from '@tanstack/react-query'
import { authApi } from '../api/auth'

export default function RoleRoute({ allow }: { allow: string[] }) {
  const me = useQuery({ queryKey: ['me'], queryFn: authApi.me })
  if (me.isPending) return null
  const allowed = me.data?.roles.some(role => allow.includes(role))
  return allowed ? <Outlet /> : <Navigate to="/dashboard" replace />
}