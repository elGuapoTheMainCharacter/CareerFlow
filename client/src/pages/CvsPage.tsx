import { useState, type ChangeEvent } from 'react'
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { resumesApi } from '../api/resumes'
import { profileApi } from '../api/profile'

const MAX_BYTES = 2 * 1024 * 1024

export default function CvsPage() {
  const qc = useQueryClient()
  const [localError, setLocalError] = useState<string | null>(null)
  const resumes = useQuery({ queryKey: ['resumes'], queryFn: resumesApi.all })
  const profile = useQuery({ queryKey: ['profile'], queryFn: profileApi.get })

  const upload = useMutation({
    mutationFn: (file: File) => resumesApi.upload(file),
    onSuccess: () => qc.invalidateQueries({ queryKey: ['resumes'] }),
  })

  const remove = useMutation({
    mutationFn: resumesApi.remove,
    onSuccess: () => qc.invalidateQueries({ queryKey: ['resumes'] }),
  })

  const visibility = useMutation({
    mutationFn: profileApi.setVisibility,
    onSuccess: () => qc.invalidateQueries({ queryKey: ['profile'] }),
  })

  function onPick(e: ChangeEvent<HTMLInputElement>) {
    const file = e.target.files?.[0]
    e.target.value = ''
    if (!file) return

    const name = file.name.toLowerCase()
    if (!name.endsWith('.pdf') && !name.endsWith('.docx')) {
      setLocalError('Only PDF and DOCX files are allowed.')
      return
    }
    if (file.size > MAX_BYTES) {
      setLocalError('File must be 2 MB or smaller.')
      return
    }
    setLocalError(null)
    upload.mutate(file)
  }

  const error =
    localError ??
    (upload.isError ? upload.error.message : null) ??
    (remove.isError ? remove.error.message : null) ??
    (visibility.isError ? visibility.error.message : null)

  return (
    <div className="two-column">
      <section>
        <h1>My CVs</h1>
        {resumes.isPending && <p>Loading...</p>}
        {resumes.data?.length === 0 && <p>You have not uploaded a CV yet.</p>}
        {resumes.data?.map(r => (
          <article key={r.id} className="row-card">
            <div>
              <b>{r.originalFileName}</b>
              <small>
                {(r.sizeBytes / 1024).toFixed(0)} KB · uploaded {new Date(r.uploadedAtUtc).toLocaleDateString()}
              </small>
            </div>
            <div style={{ display: 'flex', gap: '0.6rem', alignItems: 'center' }}>
              <a href={resumesApi.downloadUrl(r.id)}>Download</a>
              <button
                className="danger-ghost"
                disabled={remove.isPending}
                onClick={() => confirm(`Delete ${r.originalFileName}?`) && remove.mutate(r.id)}
              >
                Delete
              </button>
            </div>
          </article>
        ))}
      </section>

      <div>
        <aside className="panel">
          <h2>Upload a CV</h2>
          <p>PDF or DOCX, up to 2 MB. You can keep up to 3 CVs.</p>
          <input type="file" accept=".pdf,.docx" onChange={onPick} disabled={upload.isPending} />
          {upload.isPending && <p>Uploading...</p>}
          {error && <p className="error">{error}</p>}
        </aside>

        <section className="panel">
          <h2>Let employers find me</h2>
          <label className="inline">
            <input
              type="checkbox"
              checked={profile.data?.isDiscoverable ?? false}
              disabled={profile.isPending || visibility.isPending}
              onChange={e => visibility.mutate(e.target.checked)}
            />
            Show me in the employer talent directory
          </label>
          <p>
            When this is on, employers can see your name and view your <b>latest CV</b>, and can invite you
            to apply for their jobs. Your email stays private. Turn it off any time and your CV can no
            longer be opened by employers.
          </p>
        </section>
      </div>
    </div>
  )
}
