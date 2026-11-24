import { useState } from 'react'
import { useSearchParams } from 'react-router'
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { employerApplicationsApi } from '../api/employerApplications'
import { opportunitiesApi } from '../api/opportunities'
import { statusLabels } from '../jobFormat'
import type { ApplicationStatus } from '../types'

const settable: ApplicationStatus[] = ['Applied', 'Assessment', 'Interview', 'Offer', 'Rejected']
const filterable: ApplicationStatus[] = [...settable, 'Withdrawn']

export default function ApplicantsPage() {
  const qc = useQueryClient()
  const [params, setParams] = useSearchParams()
  const jobId = params.get('job') ?? ''
  const [status, setStatus] = useState('')

  const jobs = useQuery({ queryKey: ['opportunities'], queryFn: opportunitiesApi.all })
  const received = useQuery({
    queryKey: ['received-applications', jobId, status],
    queryFn: () => employerApplicationsApi.list(jobId, status),
  })

  const changeStatus = useMutation({
    mutationFn: (v: { id: string; status: ApplicationStatus }) =>
      employerApplicationsApi.setStatus(v.id, v.status),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ['received-applications'] })
      qc.invalidateQueries({ queryKey: ['dashboard'] })
    },
  })

  function chooseJob(value: string) {
    const next = new URLSearchParams(params)
    if (value) next.set('job', value)
    else next.delete('job')
    setParams(next)
  }

  return (
    <>
      <div className="page-title">
        <div>
          <h1>Applicants</h1>
          <p>People who applied to your jobs. Accepting or rejecting notifies them.</p>
        </div>
      </div>

      <div className="filters">
        <select value={jobId} onChange={e => chooseJob(e.target.value)}>
          <option value="">All jobs</option>
          {jobs.data?.map(j => <option key={j.id} value={j.id}>{j.title} ({j.companyName})</option>)}
        </select>
        <select value={status} onChange={e => setStatus(e.target.value)}>
          <option value="">All statuses</option>
          {filterable.map(s => <option key={s} value={s}>{statusLabels[s]}</option>)}
        </select>
      </div>

      {changeStatus.isError && <p className="error">{changeStatus.error.message}</p>}
      {received.isPending && <p>Loading...</p>}
      {received.isError && <p className="error">{received.error.message}</p>}
      {received.data?.length === 0 && <p>No applicants match these filters yet.</p>}

      {received.data?.map(a => (
        <article key={a.id} className="panel">
          <div className="page-title" style={{ marginBottom: '0.5rem' }}>
            <div>
              <h3 style={{ margin: 0 }}>{a.applicantName}</h3>
              <p>{a.applicantEmail}</p>
            </div>
            <span className={`badge status-${a.status}`}>{statusLabels[a.status]}</span>
          </div>

          <p>
            <b>{a.jobTitle}</b> · {a.companyName} · applied {a.appliedDate ?? '—'}
          </p>
          <p>
            {a.hasResume ? (
              <a href={employerApplicationsApi.resumeUrl(a.id)}>Download CV ({a.resumeFileName})</a>
            ) : (
              'No CV attached'
            )}
          </p>
          {a.coverLetter && (
            <details>
              <summary>Cover letter</summary>
              <p className="pre">{a.coverLetter}</p>
            </details>
          )}

          {a.status === 'Withdrawn' ? (
            <p>The applicant withdrew this application.</p>
          ) : (
            <div style={{ display: 'flex', gap: '0.6rem', alignItems: 'center', flexWrap: 'wrap', marginTop: '0.75rem' }}>
              <button
                disabled={changeStatus.isPending || a.status === 'Offer'}
                onClick={() => changeStatus.mutate({ id: a.id, status: 'Offer' })}
              >
                Accept
              </button>
              <button
                className="danger-ghost"
                disabled={changeStatus.isPending || a.status === 'Rejected'}
                onClick={() =>
                  confirm(`Reject ${a.applicantName}? They will be notified.`) &&
                  changeStatus.mutate({ id: a.id, status: 'Rejected' })
                }
              >
                Reject
              </button>
              <select
                value={a.status}
                disabled={changeStatus.isPending}
                onChange={e => changeStatus.mutate({ id: a.id, status: e.target.value as ApplicationStatus })}
              >
                {settable.map(s => <option key={s} value={s}>{statusLabels[s]}</option>)}
              </select>
            </div>
          )}
        </article>
      ))}
    </>
  )
}