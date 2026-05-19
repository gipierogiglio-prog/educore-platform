import { useState } from 'react'
import { BrowserRouter, Routes, Route, Navigate } from 'react-router-dom'
import { useAuth } from '@/hooks/useAuth'
import { Layout } from '@/components/Layout'
import { Login } from '@/pages/Login'
import { ImportStudents } from '@/pages/ImportStudents'
import { ImportTeachers } from '@/pages/ImportTeachers'
import { TeacherDashboard } from '@/pages/TeacherDashboard'
import { GuardianDashboard } from '@/pages/GuardianDashboard'

function ProtectedRoute({
  user,
  children,
  roles,
}: {
  user: NonNullable<ReturnType<typeof useAuth>['user']>
  children: React.ReactNode
  roles?: string[]
}) {
  if (roles && !roles.includes(user.role)) {
    return <Navigate to="/dashboard" replace />
  }
  return <>{children}</>
}

function DashboardRedirect({ user }: { user: NonNullable<ReturnType<typeof useAuth>['user']> }) {
  if (user.role === 'teacher') return <Navigate to="/teacher/dashboard" replace />
  if (user.role === 'guardian') return <Navigate to="/guardian/dashboard" replace />
  return (
    <div className="p-6">
      <h1 className="text-2xl font-bold mb-4">Dashboard da Escola</h1>
      <p className="text-muted-foreground">
        Bem-vindo, {user.name}! Use o menu lateral para navegar.
      </p>
    </div>
  )
}

export default function App() {
  const { user, loading, login, logout } = useAuth()

  if (loading) {
    return (
      <div className="min-h-screen flex items-center justify-center bg-background">
        <div className="text-muted-foreground animate-pulse">Carregando...</div>
      </div>
    )
  }

  if (!user) {
    return <Login onLogin={login} />
  }

  return (
    <BrowserRouter>
      <Layout user={user} onLogout={logout}>
        <Routes>
          <Route path="/dashboard" element={<DashboardRedirect user={user} />} />
          <Route
            path="/teacher/dashboard"
            element={
              <ProtectedRoute user={user} roles={['teacher']}>
                <TeacherDashboard />
              </ProtectedRoute>
            }
          />
          <Route
            path="/guardian/dashboard"
            element={
              <ProtectedRoute user={user} roles={['guardian']}>
                <GuardianDashboard />
              </ProtectedRoute>
            }
          />
          <Route path="/students/import" element={<ImportStudents />} />
          <Route path="/teachers/import" element={<ImportTeachers />} />
          <Route path="*" element={<Navigate to="/dashboard" replace />} />
        </Routes>
      </Layout>
    </BrowserRouter>
  )
}
