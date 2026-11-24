import type { ApplicationStatus, EmploymentType } from './types'

export const typeLabels: Record<EmploymentType, string> = {
  FullTime: 'Full-time',
  PartTime: 'Part-time',
  Contract: 'Contract',
  Internship: 'Internship',
  GraduateProgramme: 'Graduate programme',
  Learnership: 'Learnership',
  Other: 'Other',
}

export function formatSalary(min?: number | null, max?: number | null): string | null {
  if (min == null && max == null) return null
  const fmt = (n: number) => `R${n.toLocaleString('en-ZA')}`
  if (min != null && max != null) return `${fmt(min)} – ${fmt(max)}`
  return min != null ? `From ${fmt(min)}` : `Up to ${fmt(max!)}`
}
// "Offer" is shown to people as "Accepted".
export const statusLabels: Record<ApplicationStatus, string> = {
  Saved: 'Saved',
  Applied: 'Applied',
  Assessment: 'Assessment',
  Interview: 'Interview',
  Offer: 'Accepted',
  Rejected: 'Rejected',
  Withdrawn: 'Withdrawn',
}