import { useState, type FormEvent } from 'react'
import { Link, useNavigate, useParams } from 'react-router'
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { authApi } from '../api/auth'
import { publicJobsApi } from '../api/publicJobs'
import { resumesApi } from '../api/resumes'
import { applicationsApi } from '../api/applications'
import SiteHeader from '../components/SiteHeader'
import { formatSalary, typeLabels } from '../jobFormat'

export default function JobDetailPage() {
  const { id } = useParams()
  const me = useQuery({ queryKey: ['me'], queryFn: authApi.me, retry: false })
  const job = useQuery({
    queryKey: ['public-job', id],
    queryFn: () => publicJobsApi.get(id!),
    enabled: !!id,
    retry: false,
  })

  const roles = me.data?.roles ?? []
  const isSeeker = roles.includes('JobSeeker')
  const isEmployer = roles.includes('Employer')

  return (
    <>
      <SiteHeader />
      <main className="container">
        <Link to={me.data ? '/jobs' : '/'}>← All jobs</Link>

        {job.isPending && <p>Loading...</p>}
        {job.isError && <p className="error">This job is no longer available.</p>}

        {job.data && (
          <div className="two-column" style={{ marginTop: '1rem' }}>
            <section className="panel">
              <h1 style={{ marginTop: 0 }}>{job.data.title}</h1>
              <p className="lp-job-company">
                {job.data.companyName}
                {job.data.location ? ` · ${job.data.location}` : ''}
              </p>
              <div className="lp-tags">
                <span className="lp-tag">{typeLabels[job.data.employmentType]}</span>
                {formatSalary(job.data.salaryMin, job.data.salaryMax) && (
                  <span className="lp-tag">{formatSalary(job.data.salaryMin, job.data.salaryMax)}</span>
                )}
                {job.data.closingDate && <span className="lp-tag">Closes {job.data.closingDate}</span>}
              </div>
              <h2>About the role</h2>
              <p className="pre">{job.data.description || 'The employer has not added a description yet.'}</p>
              {job.data.companyWebsite && (
                <p>
                  <a href={job.data.companyWebsite} target="_blank" rel="noreferrer">
                    Visit {job.data.companyName}
                  </a>
                </p>
              )}
            </section>

            <aside className="panel" style={{ alignSelf: 'start' }}>
              {isSeeker && <ApplyPanel jobId={job.data.id} />}
              {isEmployer && <p>You are signed in as an employer. Only job seekers can apply.</p>}
              {!me.data && (
                <>
                  <h2 style={{ marginTop: 0 }}>Interested?</h2>
                  <p>Create a free job seeker account to apply with your CV.</p>
                  <Link className="lp-btn lp-btn-primary" to="/register?type=jobseeker">Sign up to apply</Link>
                  <p>Already registered? <Link to="/login">Sign in</Link></p>
                </>
              )}
            </aside>
          </div>
        )}
      </main>
    </>
  )
}

function ApplyPanel({ jobId }: { jobId: string }) {
  const navigate = useNavigate()
  const qc = useQueryClient()
  const resumes = useQuery({ queryKey: ['resumes'], queryFn: resumesApi.all })
  const [resumeId, setResumeId] = useState('')
  const [coverLetter, setCoverLetter] = useState('')

  // Until the user picks one, use their newest CV.
  const selected = resumeId || resumes.data?.[0]?.id || ''

  const apply = useMutation({
    mutationFn: () =>
      applicationsApi.apply({
        jobOpportunityId: jobId,
        resumeDocumentId: selected,
        coverLetter: coverLetter || undefined,
      }),
    onSuccess: application => {
      qc.invalidateQueries({ queryKey: ['applications'] })
      qc.invalidateQueries({ queryKey: ['dashboard'] })
      navigate(`/applications/${application.id}`)
    },
  })

  function submit(e: FormEvent) {
    e.preventDefault()
    apply.mutate()
  }

  if (resumes.isPending) return <p>Loading...</p>

  if (!resumes.data || resumes.data.length === 0) {
    return (
      <>
        <h2 style={{ marginTop: 0 }}>Apply for this job</h2>
        <p>You need a CV before you can apply.</p>
        <Link className="lp-btn lp-btn-primary" to="/cvs">Upload a CV</Link>
      </>
    )
  }

  return (
    <form className="form-stack" onSubmit={submit}>
      <h2 style={{ margin: 0 }}>Apply for this job</h2>
      <label>
        CV to send
        <select value={selected} onChange={e => setResumeId(e.target.value)}>
          {resumes.data.map(r => <option key={r.id} value={r.id}>{r.originalFileName}</option>)}
        </select>
      </label>
      <label>
        Cover letter (optional)
        <textarea
          value={coverLetter}
          maxLength={3000}
          onChange={e => setCoverLetter(e.target.value)}
          placeholder="Why are you a good fit?"
        />
      </label>
      {apply.isError && <p className="error">{apply.error.message}</p>}
      <button disabled={apply.isPending}>{apply.isPending ? 'Sending...' : 'Submit application'}</button>
    </form>
  )
}