import { Navigate, Outlet, useLocation } from 'react-router'
import { useQuery } from '@tanstack/react-query'
import { authApi } from '../api/auth'
export default function ProtectedRoute() {
 const location = useLocation()
 const me = useQuery({ queryKey: ['me'], queryFn: authApi.me, retry: false })
 if (me.isPending) return <p className="center">Checking session...</p>
 if (me.isError)
 return <Navigate to="/login" replace state={{ from: location.pathname }} />
 return <Outlet />
}