import { Link } from 'react-router'
import { formatSalary, typeLabels } from '../jobFormat'
import type { PublicJob } from '../types'

export default function JobCard({ job }: { job: PublicJob }) {
  const salary = formatSalary(job.salaryMin, job.salaryMax)
  return (
    <article className="lp-job">
      <div>
        <h3>{job.title}</h3>
        <p className="lp-job-company">
          {job.companyName}
          {job.location ? ` · ${job.location}` : ''}
        </p>
      </div>
      <div className="lp-tags">
        <span className="lp-tag">{typeLabels[job.employmentType]}</span>
        {salary && <span className="lp-tag">{salary}</span>}
        {job.closingDate && <span className="lp-tag">Closes {job.closingDate}</span>}
      </div>
      <Link className="lp-job-link" to={`/jobs/${job.id}`}>View details →</Link>
    </article>
  )
}