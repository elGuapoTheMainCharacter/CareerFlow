import { useState, type FormEvent } from 'react'
import { Link, useNavigate, useSearchParams } from 'react-router'
import { useMutation, useQueryClient } from '@tanstack/react-query'
import { authApi } from '../api/auth'
import type { AccountType } from '../types'

// These mirror the password rules configured in Program.cs.
const passwordRules = [
  { label: 'At least 8 characters', test: (p: string) => p.length >= 8 },
  { label: 'One uppercase letter (A-Z)', test: (p: string) => /[A-Z]/.test(p) },
  { label: 'One lowercase letter (a-z)', test: (p: string) => /[a-z]/.test(p) },
  { label: 'One number (0-9)', test: (p: string) => /\d/.test(p) },
]

export default function RegisterPage() {
  const [params] = useSearchParams()
  const [accountType, setAccountType] = useState<AccountType>(
    params.get('type') === 'employer' ? 'Employer' : 'JobSeeker',
  )
  const [displayName, setDisplayName] = useState('')
  const [email, setEmail] = useState('')
  const [password, setPassword] = useState('')
  const [findable, setFindable] = useState(false)
  const navigate = useNavigate()
  const qc = useQueryClient()

  const passwordValid = passwordRules.every(rule => rule.test(password))

  const register = useMutation({
    mutationFn: () =>
      authApi.register(displayName, email, password, accountType, accountType === 'JobSeeker' && findable),
    onSuccess: user => {
      qc.setQueryData(['me'], user)
      navigate('/dashboard')
    },
  })

  function submit(e: FormEvent) {
    e.preventDefault()
    if (passwordValid) register.mutate()
  }

  return (
    <div className="auth-card">
      <h1>Create account</h1>
      <form onSubmit={submit} className="form-stack">
        <div className="type-switch">
          <button
            type="button"
            className={accountType === 'JobSeeker' ? 'active' : ''}
            onClick={() => setAccountType('JobSeeker')}
          >
            Looking for a Job
          </button>
          <button
            type="button"
            className={accountType === 'Employer' ? 'active' : ''}
            onClick={() => setAccountType('Employer')}
          >
            Looking to Hire
          </button>
        </div>

        <label>
          {accountType === 'Employer' ? 'Your name or company name' : 'Full name'}
          <input required value={displayName} onChange={e => setDisplayName(e.target.value)} />
        </label>

        <label>
          Email
          <input type="email" required value={email} onChange={e => setEmail(e.target.value)} />
        </label>

        <label>
          Password
          <input type="password" required value={password} onChange={e => setPassword(e.target.value)} />
        </label>

        <ul className="pw-rules">
          {passwordRules.map(rule => (
            <li key={rule.label} className={rule.test(password) ? 'ok' : ''}>
              {rule.test(password) ? '✓' : '○'} {rule.label}
            </li>
          ))}
        </ul>

        {accountType === 'JobSeeker' && (
          <label className="inline">
            <input type="checkbox" checked={findable} onChange={e => setFindable(e.target.checked)} />
            Let employers find me and view my CV (you can change this any time)
          </label>
        )}

        {register.isError && <p className="error">{register.error.message}</p>}
        <button disabled={register.isPending || !passwordValid}>
          {register.isPending ? 'Creating account...' : 'Create account'}
        </button>
      </form>
      <p>Already registered? <Link to="/login">Sign in</Link></p>
    </div>
  )
}