import { useQuery } from '@tanstack/react-query'
import { publicJobsApi } from '../api/publicJobs'
import JobCard from '../components/JobCard'

export default function JobsPage() {
  const jobs = useQuery({
    queryKey: ['public-jobs', 50],
    queryFn: () => publicJobsApi.latest(50),
  })

  return (
    <>
      <div className="page-title">
        <div>
          <h1>Browse jobs</h1>
          <p>Every open role on CareerFlow, from all employers.</p>
        </div>
      </div>

      {jobs.isPending && <p>Loading jobs...</p>}
      {jobs.isError && <p className="error">{jobs.error.message}</p>}
      {jobs.data && jobs.data.length === 0 && <p>No open jobs right now.</p>}
      {jobs.data && jobs.data.length > 0 && (
        <div className="lp-jobs">
          {jobs.data.map(job => <JobCard key={job.id} job={job} />)}
        </div>
      )}
    </>
  )
}