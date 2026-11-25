import { Route, Routes } from 'react-router'
import ProtectedRoute from './components/ProtectedRoute'
import RoleRoute from './components/RoleRoute'
import AppLayout from './components/AppLayout'
import DemoBanner from './components/DemoBanner'
import LandingPage from './pages/LandingPage'
import LoginPage from './pages/LoginPage'
import RegisterPage from './pages/RegisterPage'
import JobDetailPage from './pages/JobDetailPage'
import DashboardPage from './pages/DashboardPage'
import JobsPage from './pages/JobsPage'
import NotificationsPage from './pages/NotificationsPage'
import CompaniesPage from './pages/CompaniesPage'
import OpportunitiesPage from './pages/OpportunitiesPage'
import ApplicantsPage from './pages/ApplicantsPage'
import TalentPage from './pages/TalentPage'
import ApplicationsPage from './pages/ApplicationsPage'
import ApplicationDetailsPage from './pages/ApplicationDetailsPage'
import CvsPage from './pages/CvsPage'

export default function App() {
  return (
    <>
      <DemoBanner />
      <Routes>
        {/* public */}
        <Route path="/" element={<LandingPage />} />
        <Route path="/login" element={<LoginPage />} />
        <Route path="/register" element={<RegisterPage />} />
        <Route path="/jobs/:id" element={<JobDetailPage />} />

        {/* signed in */}
        <Route element={<ProtectedRoute />}>
          <Route element={<AppLayout />}>
            <Route path="dashboard" element={<DashboardPage />} />
            <Route path="jobs" element={<JobsPage />} />
            <Route path="notifications" element={<NotificationsPage />} />

            <Route element={<RoleRoute allow={['Employer']} />}>
              <Route path="companies" element={<CompaniesPage />} />
              <Route path="opportunities" element={<OpportunitiesPage />} />
              <Route path="applicants" element={<ApplicantsPage />} />
              <Route path="talent" element={<TalentPage />} />
            </Route>

            <Route element={<RoleRoute allow={['JobSeeker']} />}>
              <Route path="applications" element={<ApplicationsPage />} />
              <Route path="applications/:id" element={<ApplicationDetailsPage />} />
              <Route path="cvs" element={<CvsPage />} />
            </Route>
          </Route>
        </Route>

        <Route path="*" element={<h1>404 - Page not found</h1>} />
      </Routes>
    </>
  )
}
