import { useState, type FormEvent } from 'react'
import { Link } from 'react-router'
import { useMutation, useQuery } from '@tanstack/react-query'
import { talentApi } from '../api/talent'
import { opportunitiesApi } from '../api/opportunities'
import Pagination from '../components/Pagination'

export default function TalentPage() {
  const [input, setInput] = useState('')
  const [search, setSearch] = useState('')
  const [page, setPage] = useState(1)
  const [inviting, setInviting] = useState<string | null>(null)
  const [jobId, setJobId] = useState('')
  const [notice, setNotice] = useState<string | null>(null)

  const talent = useQuery({
    queryKey: ['talent', search, page],
    queryFn: () => talentApi.search(search, page),
  })
  const jobs = useQuery({ queryKey: ['opportunities'], queryFn: opportunitiesApi.all })

  const today = new Date().toISOString().slice(0, 10)
  const openJobs = (jobs.data ?? []).filter(j => !j.isClosed && (!j.closingDate || j.closingDate >= today))

  const invite = useMutation({
    mutationFn: (userId: string) => talentApi.invite(userId, jobId),
    onSuccess: () => {
      setInviting(null)
      setJobId('')
      setNotice('Invitation sent. They will see it in their notifications.')
    },
  })

  function submitSearch(e: FormEvent) {
    e.preventDefault()
    setSearch(input.trim())
    setPage(1)
  }

  return (
    <>
      <div className="page-title">
        <div>
          <h1>Find talent</h1>
          <p>People who chose to be found. Read their latest CV and invite them to apply.</p>
        </div>
      </div>

      <form className="filters" onSubmit={submitSearch}>
        <input placeholder="Search by name" value={input} onChange={e => setInput(e.target.value)} />
        <button>Search</button>
      </form>

      {notice && <p className="badge status-Offer" style={{ display: 'inline-block', marginBottom: '1rem' }}>{notice}</p>}
      {talent.isPending && <p>Loading...</p>}
      {talent.isError && <p className="error">{talent.error.message}</p>}
      {talent.data?.items.length === 0 && (
        <p>No one matches yet. People appear here once they upload a CV and choose to be found.</p>
      )}

      {talent.data?.items.map(t => (
        <article key={t.id} className="panel">
          <div className="page-title" style={{ marginBottom: '0.5rem' }}>
            <div>
              <h3 style={{ margin: 0 }}>{t.displayName}</h3>
              <p>
                Latest CV: {t.latestCvFileName} · uploaded {new Date(t.latestCvUploadedAtUtc).toLocaleDateString()}
                {t.cvCount > 1 ? ` · ${t.cvCount} CVs on file` : ''}
              </p>
            </div>
            <div style={{ display: 'flex', gap: '0.6rem', alignItems: 'center' }}>
              <a href={talentApi.cvUrl(t.id)}>View CV</a>
              <button onClick={() => { setInviting(inviting === t.id ? null : t.id); setNotice(null) }}>
                Invite to apply
              </button>
            </div>
          </div>

          {inviting === t.id && (
            openJobs.length === 0 ? (
              <p className="error">
                You have no open jobs. <Link to="/opportunities">Post or reopen a job</Link> first.
              </p>
            ) : (
              <div style={{ display: 'flex', gap: '0.6rem', flexWrap: 'wrap' }}>
                <select value={jobId} onChange={e => setJobId(e.target.value)}>
                  <option value="">Choose a job...</option>
                  {openJobs.map(j => <option key={j.id} value={j.id}>{j.title} ({j.companyName})</option>)}
                </select>
                <button disabled={!jobId || invite.isPending} onClick={() => invite.mutate(t.id)}>
                  {invite.isPending ? 'Sending...' : 'Send invitation'}
                </button>
                {invite.isError && <p className="error">{invite.error.message}</p>}
              </div>
            )
          )}
        </article>
      ))}

      {talent.data && (
        <Pagination page={talent.data.page} totalPages={talent.data.totalPages} onChange={setPage} />
      )}
    </>
  )
}