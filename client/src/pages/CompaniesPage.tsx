import { useState, type FormEvent } from 'react'
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { companiesApi } from '../api/companies'

export default function CompaniesPage() {
  const qc = useQueryClient()
  const [name, setName] = useState('')
  const [website, setWebsite] = useState('')
  const [location, setLocation] = useState('')

  const companies = useQuery({ queryKey: ['companies'], queryFn: companiesApi.all })

  const refresh = () => {
    qc.invalidateQueries({ queryKey: ['companies'] })
    qc.invalidateQueries({ queryKey: ['dashboard'] })
  }

  const create = useMutation({
    mutationFn: companiesApi.create,
    onSuccess: () => {
      setName('')
      setWebsite('')
      setLocation('')
      refresh()
    },
  })

  const remove = useMutation({ mutationFn: companiesApi.remove, onSuccess: refresh })

  function submit(e: FormEvent) {
    e.preventDefault()
    create.mutate({
      name,
      website: website || undefined,
      location: location || undefined,
    })
  }

  return (
    <div className="two-column">
      <section>
        <h1>Companies</h1>
        {companies.isPending && <p>Loading...</p>}
        {companies.data?.length === 0 && <p>No companies yet. Add your first one.</p>}
        {remove.isError && <p className="error">{remove.error.message}</p>}

        {companies.data?.map(c => (
          <article className="row-card" key={c.id}>
            <div>
              <b>{c.name}</b>
              <small>{c.location || 'No location'} · {c.opportunityCount} opportunities</small>
              {c.website && (
                <small>
                  <a href={c.website} target="_blank" rel="noreferrer">{c.website}</a>
                </small>
              )}
            </div>
            <button
              className="danger-ghost"
              onClick={() => confirm(`Delete ${c.name}?`) && remove.mutate(c.id)}
            >
              Delete
            </button>
          </article>
        ))}
      </section>

      <aside className="panel">
        <h2>Add company</h2>
        <form className="form-stack" onSubmit={submit}>
          <label>
            Name
            <input required value={name} onChange={e => setName(e.target.value)} />
          </label>
          <label>
            Website
            <input
              type="text"
              inputMode="url"
              autoComplete="url"
              placeholder="investek.co.za"
              value={website}
              onChange={e => setWebsite(e.target.value)}
            />
          </label>
          <label>
            Location
            <input value={location} onChange={e => setLocation(e.target.value)} />
          </label>
          {create.isError && <p className="error">{create.error.message}</p>}
          <button disabled={create.isPending}>Add company</button>
        </form>
      </aside>
    </div>
  )
}