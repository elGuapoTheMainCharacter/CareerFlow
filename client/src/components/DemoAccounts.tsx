import { Link } from 'react-router'
import { demoAccounts } from '../demoAccounts'

export default function DemoAccounts() {
  return (
    <section id="demo">
      <div className="lp-section">
        <h2>Try it in one click</h2>
        <p className="lp-sub">
          Two ready-made accounts with sample data. Prefer your own? Creating an account takes about 30 seconds.
        </p>

        <div className="lp-grid-2">
          <div className="lp-card">
            <h3>Employer demo</h3>
            <p>Review applicants, read CVs, accept or reject, and browse talent.</p>
            <code className="demo-cred">{demoAccounts.employer.email}</code>
            <code className="demo-cred">{demoAccounts.employer.password}</code>
            <Link className="lp-btn lp-btn-primary" to="/login?demo=employer">Try the employer demo</Link>
          </div>

          <div className="lp-card">
            <h3>Job seeker demo</h3>
            <p>Browse jobs, apply with a CV, track applications and get notified of decisions.</p>
            <code className="demo-cred">{demoAccounts.seeker.email}</code>
            <code className="demo-cred">{demoAccounts.seeker.password}</code>
            <Link className="lp-btn lp-btn-primary" to="/login?demo=seeker">Try the job seeker demo</Link>
          </div>
        </div>

        <p style={{ marginTop: '1.25rem' }}>
          Or <Link to="/register?type=employer">create an employer account</Link> /{' '}
          <Link to="/register?type=jobseeker">create a job seeker account</Link>.
        </p>
      </div>
    </section>
  )
}
