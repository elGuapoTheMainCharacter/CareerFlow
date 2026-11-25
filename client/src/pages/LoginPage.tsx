import { useState, type FormEvent } from 'react'
import { Link, useLocation, useNavigate, useSearchParams } from 'react-router'
import { useMutation, useQueryClient } from '@tanstack/react-query'
import { authApi } from '../api/auth'
import { demoAccounts } from '../demoAccounts'

export default function LoginPage() {
  const [params] = useSearchParams()
  const demoKey = params.get('demo')
  const demo =
    demoKey === 'employer' ? demoAccounts.employer
    : demoKey === 'seeker' ? demoAccounts.seeker
    : null

  const [email, setEmail] = useState(demo?.email ?? '')
  const [password, setPassword] = useState(demo?.password ?? '')
  const [rememberMe, setRememberMe] = useState(false)
  const navigate = useNavigate()
  const location = useLocation()
  const qc = useQueryClient()

  const login = useMutation({
    mutationFn: () => authApi.login(email, password, rememberMe),
    onSuccess: user => {
      qc.setQueryData(['me'], user)
      navigate((location.state as { from?: string } | null)?.from ?? '/dashboard')
    },
  })

  function submit(e: FormEvent) {
    e.preventDefault()
    login.mutate()
  }

  return (
    <div className="auth-card">
      <h1>Sign in</h1>
      {demo && <p className="demo-hint">Demo account filled in for you. Just press Sign in.</p>}
      <form onSubmit={submit} className="form-stack">
        <label>
          Email
          <input type="email" required value={email} onChange={e => setEmail(e.target.value)} />
        </label>
        <label>
          Password
          <input type="password" required value={password} onChange={e => setPassword(e.target.value)} />
        </label>
        <label className="inline">
          <input type="checkbox" checked={rememberMe} onChange={e => setRememberMe(e.target.checked)} />
          Remember me
        </label>
        {login.isError && <p className="error">{login.error.message}</p>}
        <button disabled={login.isPending}>{login.isPending ? 'Signing in...' : 'Sign in'}</button>
      </form>
      <p>New here? <Link to="/register">Create an account</Link></p>
    </div>
  )
}
