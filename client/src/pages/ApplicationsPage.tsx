import { useQuery } from '@tanstack/react-query'
import { Link, useSearchParams } from 'react-router'
import { applicationsApi } from '../api/applications'
import Pagination from '../components/Pagination'
import type { ApplicationStatus } from '../types'
const statuses: ApplicationStatus[] = ['Saved','Applied','Assessment','Interview','Offer','Rejected','Withdrawn']
export default function ApplicationsPage() {
 const [params, setParams] = useSearchParams()
 const search = params.get('search') ?? ''; const status = params.get('status') ?? ''; const page =
Number(params.get('page') ?? '1')
 const qs = new URLSearchParams({ page: String(page), pageSize: '10' })
 if (search) qs.set('search', search); if (status) qs.set('status', status)
 const query = useQuery({ queryKey: ['applications', qs.toString()], queryFn: () =>
applicationsApi.search(qs.toString()) })
 function update(name:string,value:string){ const next=new URLSearchParams(params); value?
next.set(name,value):next.delete(name); if(name!=='page')next.set('page','1'); setParams(next) }
 return <><h1>Applications</h1>
 <div className="filters"><input placeholder="Search job or company" value={search}
onChange={e=>update('search',e.target.value)} />
 <select value={status} onChange={e=>update('status',e.target.value)}><option value="">All
statuses</option>{statuses.map(s=><option key={s}>{s}</option>)}</select></div>
 {query.isPending ? <p>Loading...</p> : query.isError ? <p className="error">{query.error.message}</p> : <>
 <div className="table-wrap"><table><thead><tr><th>Job</th><th>Company</th><th>Status</th><th>Next
action</th></tr></thead><tbody>
 {query.data.items.map(a=><tr key={a.id}><td><Link
to={`/applications/${a.id}`}>{a.jobTitle}</Link></td><td>{a.companyName}</td><td><span
className="badge">{a.status}</span></td><td>{a.nextActionDate ?? '—'}</td></tr>)}
 </tbody></table></div>
 <Pagination page={query.data.page} totalPages={query.data.totalPages} onChange={p=>update('page',String(p))} />
 </>}
 </>
}
