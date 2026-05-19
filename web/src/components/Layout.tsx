import { type ReactNode } from 'react'
import { useNavigate, useLocation } from 'react-router-dom'
import {
  Upload,
  Users,
  GraduationCap,
  LayoutDashboard,
  LogOut,
  ChevronLeft,
  School,
} from 'lucide-react'
import { cn } from '@/lib/utils'
import { Button } from '@/components/ui/button'
import { Separator } from '@/components/ui/separator'
import type { User } from '@/hooks/useAuth'

interface NavItem {
  label: string
  icon: ReactNode
  path: string
  roles?: string[]
}

const navItems: NavItem[] = [
  { label: 'Dashboard', icon: <LayoutDashboard size={18} />, path: '/dashboard', roles: ['org_admin', 'coordinator'] },
  { label: 'Importar Alunos', icon: <Upload size={18} />, path: '/students/import', roles: ['org_admin', 'coordinator', 'teacher'] },
  { label: 'Importar Professores', icon: <Upload size={18} />, path: '/teachers/import', roles: ['org_admin', 'coordinator'] },
  { label: 'Dashboard Professor', icon: <GraduationCap size={18} />, path: '/teacher/dashboard', roles: ['teacher'] },
  { label: 'Visão do Responsável', icon: <Users size={18} />, path: '/guardian/dashboard', roles: ['guardian'] },
]

interface LayoutProps {
  user: User
  onLogout: () => void
  children: ReactNode
}

export function Layout({ user, onLogout, children }: LayoutProps) {
  const navigate = useNavigate()
  const location = useLocation()

  const filtered = navItems.filter(
    item => !item.roles || item.roles.includes(user.role)
  )

  return (
    <div className="flex h-screen overflow-hidden bg-background">
      {/* Sidebar */}
      <aside className="w-64 border-r border-border bg-card hidden md:flex flex-col">
        <div className="p-4 flex items-center gap-2 border-b border-border">
          <School className="text-primary" size={24} />
          <span className="font-bold text-lg">EduCore</span>
        </div>

        <nav className="flex-1 p-2 space-y-1 overflow-y-auto">
          {filtered.map((item) => (
            <button
              key={item.path}
              onClick={() => navigate(item.path)}
              className={cn(
                "w-full flex items-center gap-3 px-3 py-2 rounded-md text-sm transition-colors",
                location.pathname === item.path
                  ? "bg-primary/10 text-primary font-medium"
                  : "text-muted-foreground hover:bg-secondary hover:text-foreground"
              )}
            >
              {item.icon}
              {item.label}
            </button>
          ))}
        </nav>

        <Separator />

        <div className="p-4">
          <div className="text-sm text-muted-foreground mb-2 truncate">{user.name}</div>
          <Button variant="ghost" size="sm" className="w-full justify-start gap-2" onClick={onLogout}>
            <LogOut size={16} />
            Sair
          </Button>
        </div>
      </aside>

      {/* Mobile header + content */}
      <div className="flex-1 flex flex-col overflow-hidden">
        <header className="md:hidden border-b border-border bg-card p-3 flex items-center justify-between">
          <div className="flex items-center gap-2">
            <School className="text-primary" size={20} />
            <span className="font-semibold">EduCore</span>
          </div>
          <Button variant="ghost" size="sm" onClick={onLogout}>
            <LogOut size={16} />
          </Button>
        </header>

        <main className="flex-1 overflow-y-auto p-4 md:p-6">
          {children}
        </main>
      </div>
    </div>
  )
}
