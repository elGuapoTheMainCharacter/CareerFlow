import { Link } from 'react-router'
import { useQuery } from '@tanstack/react-query'
import { authApi } from '../api/auth'

export default function SiteHeader() {
  const me = useQuery({ queryKey: ['me'], queryFn: authApi.me, retry: false })

  return (
    <header className="lp-nav">
      <Link to="/" className="lp-logo">Career<span>Flow</span></Link>
      {me.data ? (
        <Link className="lp-btn lp-btn-primary" to="/dashboard">Dashboard</Link>
      ) : (
        <div className="lp-nav-actions">
          <Link className="lp-btn lp-btn-ghost" to="/login">Sign in</Link>
          <Link className="lp-btn lp-btn-primary" to="/register">Get started</Link>
        </div>
      )}
    </header>
  )
}