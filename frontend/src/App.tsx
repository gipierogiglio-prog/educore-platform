import { Routes, Route, Navigate, Link } from 'react-router-dom';
import { CalendarCheck, ListTodo, LayoutDashboard } from 'lucide-react';
import { Button } from '@/components/ui/button';
import RoutinesPage from '@/pages/RoutinesPage';
import RoutineRegistrationPage from '@/pages/RoutineRegistrationPage';
import RoutineStatusDashboard from '@/pages/RoutineStatusDashboard';

export default function App() {
  return (
    <div className="min-h-screen bg-background">
      <header className="border-b">
        <div className="container mx-auto flex items-center justify-between px-4 py-3">
          <div className="flex items-center gap-2">
            <CalendarCheck className="h-6 w-6 text-primary" />
            <h1 className="text-xl font-bold">EduCore - Rotinas Diárias</h1>
          </div>
          <nav className="flex items-center gap-2">
            <Button variant="ghost" size="sm" asChild>
              <Link to="/routines">
                <ListTodo className="mr-1 h-4 w-4" />
                Rotinas
              </Link>
            </Button>
            <Button variant="ghost" size="sm" asChild>
              <Link to="/register">
                <CalendarCheck className="mr-1 h-4 w-4" />
                Registro
              </Link>
            </Button>
            <Button variant="ghost" size="sm" asChild>
              <Link to="/dashboard">
                <LayoutDashboard className="mr-1 h-4 w-4" />
                Dashboard
              </Link>
            </Button>
          </nav>
        </div>
      </header>

      <main className="container mx-auto px-4 py-6">
        <Routes>
          <Route path="/" element={<Navigate to="/routines" replace />} />
          <Route path="/routines" element={<RoutinesPage />} />
          <Route path="/register" element={<RoutineRegistrationPage />} />
          <Route path="/dashboard" element={<RoutineStatusDashboard />} />
        </Routes>
      </main>
    </div>
  );
}
