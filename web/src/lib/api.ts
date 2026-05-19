const API_BASE = import.meta.env.VITE_API_URL || ''

async function request<T>(url: string, options?: RequestInit): Promise<T> {
  const token = localStorage.getItem('token')
  const headers: Record<string, string> = {
    ...(options?.headers as Record<string, string>),
  }
  if (token) headers['Authorization'] = `Bearer ${token}`
  if (!(options?.body instanceof FormData)) {
    headers['Content-Type'] = 'application/json'
  }

  const res = await fetch(`${API_BASE}${url}`, {
    ...options,
    headers,
  })

  if (!res.ok) {
    const err = await res.json().catch(() => ({ message: res.statusText }))
    throw new Error(err.message || `Erro ${res.status}`)
  }

  return res.json()
}

export const api = {
  get: <T>(url: string) => request<T>(url),
  post: <T>(url: string, body?: unknown) =>
    request<T>(url, {
      method: 'POST',
      body: body instanceof FormData ? body : JSON.stringify(body),
    }),
  patch: <T>(url: string, body: unknown) =>
    request<T>(url, {
      method: 'PATCH',
      body: JSON.stringify(body),
    }),
  put: <T>(url: string, body: unknown) =>
    request<T>(url, {
      method: 'PUT',
      body: JSON.stringify(body),
    }),
  delete: <T>(url: string) =>
    request<T>(url, { method: 'DELETE' }),
}
