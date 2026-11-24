import { Link, NavLink, Outlet, useNavigate } from 'react-router'
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { authApi } from '../api/auth'
import { notificationsApi } from '../api/notifications'

export default function AppLayout() {
  const navigate = useNavigate()
  const qc = useQueryClient()
  const me = useQuery({ queryKey: ['me'], queryFn: authApi.me })
  const roles = me.data?.roles ?? []
  const isEmployer = roles.includes('Employer')
  const isSeeker = roles.includes('JobSeeker')

  // Checks for new notifications every 30 seconds while this tab is visible.
  const unread = useQuery({
    queryKey: ['notifications-count'],
    queryFn: notificationsApi.unreadCount,
    refetchInterval: 30_000,
  })
  const count = unread.data?.count ?? 0

  const logout = useMutation({
    mutationFn: authApi.logout,
    onSuccess: () => {
      qc.clear()
      navigate('/')
    },
  })

  return (
    <>
      <header className="topbar">
        <Link to="/"><strong>CareerFlow</strong></Link>
        <nav>
          <NavLink to="/dashboard">Dashboard</NavLink>
          <NavLink to="/jobs">Browse jobs</NavLink>
          {isEmployer && (
            <>
              <NavLink to="/companies">Companies</NavLink>
              <NavLink to="/opportunities">Post jobs</NavLink>
              <NavLink to="/applicants">Applicants</NavLink>
              <NavLink to="/talent">Find talent</NavLink>
            </>
          )}
          {isSeeker && (
            <>
              <NavLink to="/applications">My applications</NavLink>
              <NavLink to="/cvs">My CVs</NavLink>
            </>
          )}
          <NavLink to="/notifications">
            Notifications{count > 0 && <span className="count-badge">{count}</span>}
          </NavLink>
          <button onClick={() => logout.mutate()}>Sign out</button>
        </nav>
      </header>
      <main className="container"><Outlet /></main>
    </>
  )
}