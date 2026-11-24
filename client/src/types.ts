export type User = {
  id: string
  displayName: string
  email: string
  roles: string[]
}

export type AccountType = 'Employer' | 'JobSeeker'

export type Company = {
  id: string
  name: string
  website?: string | null
  location?: string | null
  opportunityCount: number
}

export type EmploymentType =
  | 'FullTime' | 'PartTime' | 'Contract' | 'Internship'
  | 'GraduateProgramme' | 'Learnership' | 'Other'

export type ApplicationStatus =
  | 'Saved' | 'Applied' | 'Assessment' | 'Interview'
  | 'Offer' | 'Rejected' | 'Withdrawn'

export type Opportunity = {
  id: string
  title: string
  employmentType: EmploymentType
  salaryMin?: number | null
  salaryMax?: number | null
  closingDate?: string | null
  companyId: string
  companyName: string
  isClosed: boolean
  applicantCount: number
}

export type ApplicationListItem = {
  id: string
  jobTitle: string
  companyName: string
  status: ApplicationStatus
  appliedDate?: string | null
  nextActionDate?: string | null
  updatedAtUtc: string
}

export type ApplicationDetail = {
  id: string
  jobOpportunityId: string
  jobTitle: string
  companyName: string
  status: ApplicationStatus
  appliedDate?: string | null
  nextActionDate?: string | null
  sourceUrl?: string | null
  coverLetter?: string | null
  resumeDocumentId?: string | null
  resumeFileName?: string | null
  statusHistory: { from: ApplicationStatus; to: ApplicationStatus; changedAtUtc: string }[]
  notes: { id: number; body: string; createdAtUtc: string }[]
}

export type PagedResult<T> = {
  items: T[]
  page: number
  pageSize: number
  totalCount: number
  totalPages: number
}

export type Dashboard = {
  companies: number
  opportunities: number
  applications: number
  upcomingActions: number
  byStatus: { status: string; count: number }[]
}

// ---------- public job board ----------

export type PublicJob = {
  id: string
  title: string
  companyName: string
  location?: string | null
  employmentType: EmploymentType
  salaryMin?: number | null
  salaryMax?: number | null
  closingDate?: string | null
  postedAtUtc: string
}

export type PublicJobDetail = {
  id: string
  title: string
  description?: string | null
  companyName: string
  companyWebsite?: string | null
  location?: string | null
  employmentType: EmploymentType
  salaryMin?: number | null
  salaryMax?: number | null
  closingDate?: string | null
  postedAtUtc: string
}

// ---------- CVs and applicants ----------

export type Resume = {
  id: string
  originalFileName: string
  sizeBytes: number
  uploadedAtUtc: string
}

export type ReceivedApplication = {
  id: string
  jobOpportunityId: string
  jobTitle: string
  companyName: string
  applicantName: string
  applicantEmail: string
  status: ApplicationStatus
  appliedDate?: string | null
  coverLetter?: string | null
  hasResume: boolean
  resumeFileName?: string | null
  updatedAtUtc: string
}

// ---------- talent, notifications, profile ----------

export type TalentItem = {
  id: string
  displayName: string
  latestCvFileName: string
  latestCvUploadedAtUtc: string
  cvCount: number
}

export type NotificationItem = {
  id: number
  message: string
  link?: string | null
  isRead: boolean
  createdAtUtc: string
}

export type NotificationsResponse = {
  items: NotificationItem[]
  unreadCount: number
}

export type Profile = { isDiscoverable: boolean }