import { useState, type FormEvent } from 'react'
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { Link, useParams } from 'react-router'
import { applicationsApi } from '../api/applications'
import { resumesApi } from '../api/resumes'
import { statusLabels } from '../jobFormat'

export default function ApplicationDetailsPage() {
  const { id } = useParams()
  const qc = useQueryClient()
  const [note, setNote] = useState('')
  const [reminder, setReminder] = useState<string | null>(null)

  const query = useQuery({
    queryKey: ['application', id],
    queryFn: () => applicationsApi.get(id!),
    enabled: !!id,
  })

  const refresh = () => {
    qc.invalidateQueries({ queryKey: ['application', id] })
    qc.invalidateQueries({ queryKey: ['applications'] })
    qc.invalidateQueries({ queryKey: ['dashboard'] })
  }

  const withdraw = useMutation({
    mutationFn: () => applicationsApi.withdraw(id!),
    onSuccess: refresh,
  })

  const saveReminder = useMutation({
    mutationFn: () =>
      applicationsApi.update(id!, {
        nextActionDate: reminder || null,
        sourceUrl: query.data?.sourceUrl ?? null,
      }),
    onSuccess: () => {
      setReminder(null)
      refresh()
    },
  })

  const addNote = useMutation({
    mutationFn: () => applicationsApi.addNote(id!, note),
    onSuccess: () => {
      setNote('')
      qc.invalidateQueries({ queryKey: ['application', id] })
    },
  })

  if (query.isPending) return <p>Loading...</p>
  if (query.isError) return <p className="error">{query.error.message}</p>

  const a = query.data
  const canWithdraw = !['Withdrawn', 'Offer', 'Rejected'].includes(a.status)

  function submitNote(e: FormEvent) {
    e.preventDefault()
    if (note.trim()) addNote.mutate()
  }

  return (
    <>
      <Link to="/applications">← My applications</Link>

      <div className="page-title" style={{ marginTop: '1rem' }}>
        <div>
          <h1>{a.jobTitle}</h1>
          <p>{a.companyName}</p>
        </div>
        <span className={`badge status-${a.status}`}>{statusLabels[a.status]}</span>
      </div>

      <div className="two-column">
        <div>
          <section className="panel">
            <h2>What you sent</h2>
            <p>
              <b>CV: </b>
              {a.resumeDocumentId ? (
                <a href={resumesApi.downloadUrl(a.resumeDocumentId)}>{a.resumeFileName}</a>
              ) : (
                'No CV attached'
              )}
            </p>
            <p><b>Cover letter</b></p>
            <p className="pre">{a.coverLetter || 'You did not add a cover letter.'}</p>
            <p>Applied on {a.appliedDate ?? '—'}</p>
          </section>

          <section className="panel">
            <h2>My private notes</h2>
            <p>Only you can see these notes. The employer cannot.</p>
            <form onSubmit={submitNote}>
              <textarea
                value={note}
                onChange={e => setNote(e.target.value)}
                placeholder="Recruiter name, interview prep, questions to ask..."
              />
              <button disabled={addNote.isPending}>Add note</button>
            </form>
            {a.notes.map(n => (
              <article key={n.id} className="note">
                <p className="pre">{n.body}</p>
                <small>{new Date(n.createdAtUtc).toLocaleString()}</small>
              </article>
            ))}
          </section>
        </div>

        <aside>
          <section className="panel">
            <h2>Status history</h2>
            {a.statusHistory.length === 0 ? (
              <p>No changes yet.</p>
            ) : (
              a.statusHistory.map((h, i) => (
                <div key={i} className="timeline">
                  <b>{statusLabels[h.from]} → {statusLabels[h.to]}</b>
                  <small>{new Date(h.changedAtUtc).toLocaleString()}</small>
                </div>
              ))
            )}
          </section>

          <section className="panel">
            <h2>Reminder</h2>
            <div className="form-stack">
              <label>
                Next action date
                <input
                  type="date"
                  value={reminder ?? a.nextActionDate ?? ''}
                  onChange={e => setReminder(e.target.value)}
                />
              </label>
              <button disabled={reminder === null || saveReminder.isPending} onClick={() => saveReminder.mutate()}>
                Save reminder
              </button>
            </div>
          </section>

          {canWithdraw && (
            <section className="panel">
              <h2>Withdraw</h2>
              <p>Tell the employer you are no longer interested.</p>
              {withdraw.isError && <p className="error">{withdraw.error.message}</p>}
              <button
                className="danger-ghost"
                disabled={withdraw.isPending}
                onClick={() => confirm('Withdraw this application?') && withdraw.mutate()}
              >
                Withdraw application
              </button>
            </section>
          )}
        </aside>
      </div>
    </>
  )
}