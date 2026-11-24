import { useState, type FormEvent } from 'react'
import { Link } from 'react-router'
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { companiesApi } from '../api/companies'
import { opportunitiesApi } from '../api/opportunities'
import { typeLabels } from '../jobFormat'
import type { EmploymentType } from '../types'

const employmentTypes: EmploymentType[] = [
  'FullTime', 'PartTime', 'Contract', 'Internship',
  'GraduateProgramme', 'Learnership', 'Other',
]

export default function OpportunitiesPage() {
  const qc = useQueryClient()
  const companies = useQuery({ queryKey: ['companies'], queryFn: companiesApi.all })
  const jobs = useQuery({ queryKey: ['opportunities'], queryFn: opportunitiesApi.all })

  const [title, setTitle] = useState('')
  const [companyId, setCompanyId] = useState('')
  const [employmentType, setEmploymentType] = useState<EmploymentType>('FullTime')
  const [description, setDescription] = useState('')
  const [salaryMin, setSalaryMin] = useState('')
  const [salaryMax, setSalaryMax] = useState('')
  const [closingDate, setClosingDate] = useState('')

  const hasCompanies = (companies.data?.length ?? 0) > 0
  const today = new Date().toISOString().slice(0, 10)

  const refreshJobs = () => {
    qc.invalidateQueries({ queryKey: ['opportunities'] })
    qc.invalidateQueries({ queryKey: ['companies'] })
    qc.invalidateQueries({ queryKey: ['dashboard'] })
    qc.invalidateQueries({ queryKey: ['public-jobs'] })
    qc.invalidateQueries({ queryKey: ['public-job'] })
  }

  const create = useMutation({
    mutationFn: opportunitiesApi.create,
    onSuccess: () => {
      setTitle('')
      setDescription('')
      setSalaryMin('')
      setSalaryMax('')
      setClosingDate('')
      refreshJobs()
    },
  })

  const toggle = useMutation({
    mutationFn: (v: { id: string; closed: boolean }) =>
      v.closed ? opportunitiesApi.close(v.id) : opportunitiesApi.reopen(v.id),
    onSuccess: refreshJobs,
  })

  function submit(e: FormEvent) {
    e.preventDefault()
    create.mutate({
      title,
      companyId,
      employmentType,
      description: description || undefined,
      salaryMin: salaryMin ? Number(salaryMin) : null,
      salaryMax: salaryMax ? Number(salaryMax) : null,
      closingDate: closingDate || null,
    })
  }

  return (
    <div className="two-column">
      <section>
        <h1>Opportunities</h1>
        {jobs.isPending && <p>Loading...</p>}
        {jobs.data?.length === 0 && (
          <p>You have not posted any jobs yet. Use the form to post your first one.</p>
        )}
        {toggle.isError && <p className="error">{toggle.error.message}</p>}

        {jobs.data?.map(j => {
          const expired = !!j.closingDate && j.closingDate < today
          const state = j.isClosed ? 'Closed' : expired ? 'Expired' : 'Open'
          return (
            <article key={j.id} className="row-card">
              <div>
                <b>{j.title}</b>
                <small>
                  {j.companyName} · {typeLabels[j.employmentType]}
                  {j.closingDate ? ` · closes ${j.closingDate}` : ''}
                </small>
                <small>
                  <Link to={`/applicants?job=${j.id}`}>{j.applicantCount} applicant(s)</Link>
                </small>
              </div>
              <div style={{ display: 'flex', gap: '0.6rem', alignItems: 'center' }}>
                <span className={`badge status-${state}`}>{state}</span>
                <button
                  className={j.isClosed ? '' : 'danger-ghost'}
                  disabled={toggle.isPending}
                  onClick={() => toggle.mutate({ id: j.id, closed: !j.isClosed })}
                >
                  {j.isClosed ? 'Reopen' : 'Close job'}
                </button>
              </div>
            </article>
          )
        })}
      </section>

      <aside className="panel">
        <h2>Add opportunity</h2>

        {companies.isSuccess && !hasCompanies && (
          <p className="error">
            You need a company first. <Link to="/companies">Add a company</Link>, then come back here.
          </p>
        )}

        <form className="form-stack" onSubmit={submit}>
          <label>
            Job title
            <input required value={title} onChange={e => setTitle(e.target.value)} />
          </label>

          <label>
            Company
            <select required value={companyId} onChange={e => setCompanyId(e.target.value)}>
              <option value="">Select...</option>
              {companies.data?.map(c => <option key={c.id} value={c.id}>{c.name}</option>)}
            </select>
          </label>

          <label>
            Employment type
            <select value={employmentType} onChange={e => setEmploymentType(e.target.value as EmploymentType)}>
              {employmentTypes.map(t => <option key={t} value={t}>{typeLabels[t]}</option>)}
            </select>
          </label>

          <label>
            Description
            <textarea value={description} onChange={e => setDescription(e.target.value)} />
          </label>

          <label>
            Minimum salary (R per year)
            <input type="number" min="0" value={salaryMin} onChange={e => setSalaryMin(e.target.value)} />
          </label>

          <label>
            Maximum salary (R per year)
            <input type="number" min="0" value={salaryMax} onChange={e => setSalaryMax(e.target.value)} />
          </label>

          <label>
            Closing date
            <input type="date" value={closingDate} onChange={e => setClosingDate(e.target.value)} />
          </label>

          {create.isError && <p className="error">{create.error.message}</p>}
          <button disabled={create.isPending || !hasCompanies}>
            {create.isPending ? 'Posting...' : 'Add opportunity'}
          </button>
        </form>
      </aside>
    </div>
  )
}