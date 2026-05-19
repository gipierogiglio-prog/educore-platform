import { useState, useEffect, useCallback } from 'react'
import { api } from '@/lib/api'

export interface User {
  id: string
  name: string
  email: string
  role: string
  organizationId: string
}

export function useAuth() {
  const [user, setUser] = useState<User | null>(null)
  const [loading, setLoading] = useState(true)

  const loadUser = useCallback(async () => {
    const token = localStorage.getItem('token')
    if (!token) {
      setLoading(false)
      return
    }
    try {
      const data = await api.get<{ id: string; name: string; email: string; role: string; organizationId?: string }>('/api/auth/me')
      setUser({
        id: data.id,
        name: data.name,
        email: data.email,
        role: data.role,
        organizationId: data.organizationId || '',
      })
    } catch {
      localStorage.removeItem('token')
    }
    setLoading(false)
  }, [])

  useEffect(() => { loadUser() }, [loadUser])

  const login = async (email: string, password: string) => {
    const data = await api.post<{ token: string; name: string; email: string; role: string; organizationId: string }>('/api/auth/login', { email, password })
    localStorage.setItem('token', data.token)
    setUser({
      id: '',
      name: data.name,
      email: data.email,
      role: data.role,
      organizationId: data.organizationId,
    })
    return data
  }

  const logout = () => {
    localStorage.removeItem('token')
    setUser(null)
  }

  return { user, loading, login, logout }
}
