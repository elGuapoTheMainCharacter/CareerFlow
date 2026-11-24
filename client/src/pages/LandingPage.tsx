import { Link } from 'react-router'
import { useQuery } from '@tanstack/react-query'
import { authApi } from '../api/auth'
import { publicJobsApi } from '../api/publicJobs'
import JobCard from '../components/JobCard'

// Edit these three before you show the site to recruiters.
const BUILDER_NAME = 'ZS NKEFE'
const CONTACT_EMAIL = 'you@example.com'
const GITHUB_URL = 'https://github.com/your-username'

export default function LandingPage() {
  const me = useQuery({ queryKey: ['me'], queryFn: authApi.me, retry: false })
  const jobs = useQuery({ queryKey: ['public-jobs', 6], queryFn: () => publicJobsApi.latest(6) })
  const signedIn = !!me.data

  return (
    <div id="top">
      <header className="lp-nav">
        <a href="#top" className="lp-logo">Career<span>Flow</span></a>
        <nav className="lp-links">
          <a href="#how">How it works</a>
          <a href="#jobs">Jobs</a>
          <a href="#about">About</a>
          <a href="#contact">Contact</a>
        </nav>
        {signedIn ? (
          <Link className="lp-btn lp-btn-primary" to="/dashboard">Dashboard</Link>
        ) : (
          <div className="lp-nav-actions">
            <Link className="lp-btn lp-btn-ghost" to="/login">Sign in</Link>
            <Link className="lp-btn lp-btn-primary" to="/register">Get started</Link>
          </div>
        )}
      </header>

      <section className="lp-hero">
        <p className="lp-eyebrow">Hiring and job hunting, in one flow</p>
        <h1>Welcome to CareerFlow</h1>
        <p className="lp-lead">
          Companies post openings and find talent. Job seekers discover roles, apply in a few
          clicks and track every step of their search, all in one place.
        </p>
        {signedIn ? (
          <div className="lp-cta">
            <Link className="lp-btn lp-btn-primary lp-btn-lg" to="/dashboard">
              Go to your dashboard
            </Link>
          </div>
        ) : (
          <div className="lp-cta">
            <Link className="lp-btn lp-btn-primary lp-btn-lg" to="/register?type=employer">
              Looking to Hire
            </Link>
            <Link className="lp-btn lp-btn-outline lp-btn-lg" to="/register?type=jobseeker">
              Looking for a Job
            </Link>
          </div>
        )}
        <p className="lp-stack">
          Built with ASP.NET Core · Entity Framework Core · Identity · React · TypeScript
        </p>
      </section>

      <section id="how" className="lp-band">
        <div className="lp-section">
          <h2>How it works</h2>
          <p className="lp-sub">Two sides of the same hiring process, each with its own tools.</p>
          <div className="lp-grid-2">
            <div className="lp-card">
              <h3>For employers</h3>
              <ol className="lp-steps">
                <li>Create your company profile.</li>
                <li>Post an opening with salary, type and closing date.</li>
                <li>Review applicants and move them through your process.</li>
              </ol>
            </div>
            <div className="lp-card">
              <h3>For job seekers</h3>
              <ol className="lp-steps">
                <li>Create your free account.</li>
                <li>Browse open roles and apply.</li>
                <li>Track every application from saved to offer.</li>
              </ol>
            </div>
          </div>
        </div>
      </section>

      <section id="jobs">
        <div className="lp-section">
          <h2>Latest openings</h2>
          <p className="lp-sub">Fresh roles from employers on CareerFlow. No account needed to browse.</p>
          {jobs.isPending && <p>Loading jobs...</p>}
          {jobs.isError && <p className="error">Could not load jobs right now.</p>}
          {jobs.data && jobs.data.length === 0 && (
            <div className="lp-empty">
              <p>No openings yet. Be the first to post one.</p>
              <Link className="lp-btn lp-btn-primary" to="/register?type=employer">
                Looking to Hire
              </Link>
            </div>
          )}
          {jobs.data && jobs.data.length > 0 && (
            <div className="lp-jobs">
              {jobs.data.map(job => <JobCard key={job.id} job={job} />)}
            </div>
          )}
        </div>
      </section>

      <section id="about" className="lp-band">
        <div className="lp-section">
          <h2>About CareerFlow</h2>
          <p className="lp-sub">
            CareerFlow is a full-stack platform designed and built by {BUILDER_NAME} to show how
            a real hiring product fits together, from the database to the browser.
          </p>
          <div className="lp-grid-3">
            <div className="lp-card">
              <h3>Two roles, one platform</h3>
              <p>Employers and job seekers get different tools, and the server enforces the difference.</p>
            </div>
            <div className="lp-card">
              <h3>Secure by design</h3>
              <p>ASP.NET Core Identity cookies, CSRF protection and data scoped to its owner.</p>
            </div>
            <div className="lp-card">
              <h3>Modern stack</h3>
              <p>.NET 10 and EF Core on the back end, React, TypeScript and React Query on the front.</p>
            </div>
          </div>
        </div>
      </section>

      <section id="contact">
        <div className="lp-section">
          <h2>Contact</h2>
          <p className="lp-sub">Questions about CareerFlow, or want to talk about the code? Get in touch.</p>
          <div className="lp-contact-links">
            <a className="lp-btn lp-btn-primary" href={`mailto:${CONTACT_EMAIL}`}>Email me</a>
            <a className="lp-btn lp-btn-ghost" href={GITHUB_URL} target="_blank" rel="noreferrer">GitHub</a>
          </div>
        </div>
      </section>

      <footer className="lp-footer">
        <p>© {new Date().getFullYear()} CareerFlow · Built by {BUILDER_NAME}</p>
      </footer>
    </div>
  )
}