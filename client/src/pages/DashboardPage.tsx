import { useQuery } from '@tanstack/react-query'
import { Link } from 'react-router'
import { authApi } from '../api/auth'
import { dashboardApi } from '../api/dashboard'

export default function DashboardPage() {
  const me = useQuery({ queryKey: ['me'], queryFn: authApi.me })
  const query = useQuery({ queryKey: ['dashboard'], queryFn: dashboardApi.get })

  if (query.isPending || me.isPending) return <p>Loading dashboard...</p>
  if (query.isError) return <p className="error">{query.error.message}</p>

  const d = query.data
  const isEmployer = me.data?.roles.includes('Employer') ?? false
  const maxCount = Math.max(1, ...d.byStatus.map(v => v.count))

  return (
    <>
      <div className="page-title">
        <div>
          <h1>Welcome, {me.data?.displayName}</h1>
          <p>{isEmployer ? 'Your hiring at a glance.' : 'Your job search at a glance.'}</p>
        </div>
      </div>

      <section className="stats-grid">
        {isEmployer ? (
          <>
            <Stat label="Companies" value={d.companies} />
            <Stat label="Jobs posted" value={d.opportunities} />
            <Stat label="Applications received" value={d.applications} />
          </>
        ) : (
          <>
            <Stat label="Applications sent" value={d.applications} />
            <Stat label="Actions in next 7 days" value={d.upcomingActions} />
          </>
        )}
      </section>

      <section className="panel">
        <h2>{isEmployer ? 'Applications received by status' : 'My applications by status'}</h2>
        {d.byStatus.length === 0 ? (
          <p>
            {isEmployer
              ? 'No applications yet. They will appear here once job seekers apply to your jobs.'
              : 'No applications yet. Browse the latest openings to get started.'}
          </p>
        ) : (
          <div className="status-bars">
            {d.byStatus.map(x => (
              <div key={x.status}>
                <span>{x.status}</span>
                <progress value={x.count} max={maxCount} />
                <b>{x.count}</b>
              </div>
            ))}
          </div>
        )}
        {!isEmployer && <p><Link to="/#jobs">Browse the latest jobs →</Link></p>}
      </section>
    </>
  )
}

function Stat({ label, value }: { label: string; value: number }) {
  return (
    <div className="stat-card">
      <span>{label}</span>
      <strong>{value}</strong>
    </div>
  )
}