import { useState, useEffect } from 'react';
import { format } from 'date-fns';
import { ptBR } from 'date-fns/locale/pt-BR';
import {
  ChevronLeft,
  ChevronRight,
  Play,
  CheckCircle2,
  XCircle,
  Clock,
  RotateCcw,
} from 'lucide-react';
import { Button } from '@/components/ui/button';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Select } from '@/components/ui/select';
import { Badge } from '@/components/ui/badge';
import { recordsApi, statusApi, classRoutinesApi, type RoutineStatus } from '@/lib/api';

const DAYS_OF_WEEK = [
  'Domingo', 'Segunda', 'Terça', 'Quarta', 'Quinta', 'Sexta', 'Sábado',
];

const statusBadge: Record<string, string> = {
  Pending: 'bg-gray-100 text-gray-800',
  InProgress: 'bg-blue-100 text-blue-800',
  Completed: 'bg-green-100 text-green-800',
  Cancelled: 'bg-red-100 text-red-800',
};

const statusIcon: Record<string, React.ReactNode> = {
  Pending: <Clock className="h-5 w-5 text-gray-400" />,
  InProgress: <RotateCcw className="h-5 w-5 text-blue-500 animate-spin" />,
  Completed: <CheckCircle2 className="h-5 w-5 text-green-500" />,
  Cancelled: <XCircle className="h-5 w-5 text-red-400" />,
};

const statusLabel: Record<string, string> = {
  Pending: 'Pendente',
  InProgress: 'Em andamento',
  Completed: 'Concluída',
  Cancelled: 'Cancelada',
};

export default function RoutineRegistrationPage() {
  const [classes, setClasses] = useState<{ id: string; name: string }[]>([]);
  const [selectedClass, setSelectedClass] = useState('');
  const [currentDate, setCurrentDate] = useState(new Date());
  const [status, setStatus] = useState<RoutineStatus | null>(null);
  const [loading, setLoading] = useState(false);

  useEffect(() => {
    // Load classes from API
    fetchClasses();
  }, []);

  useEffect(() => {
    if (selectedClass) {
      loadStatus();
    }
  }, [selectedClass, currentDate]);

  async function fetchClasses() {
    try {
      const res = await fetch('/api/classes');
      const data = await res.json();
      setClasses(data || []);
    } catch {
      // If classes endpoint not available, provide fallback
      setClasses([]);
    }
  }

  async function loadStatus() {
    if (!selectedClass) return;
    setLoading(true);
    try {
      const dateStr = format(currentDate, 'yyyy-MM-dd');
      const data = await statusApi.get(selectedClass, dateStr);
      setStatus(data);
    } catch {
      setStatus(null);
    } finally {
      setLoading(false);
    }
  }

  async function handleStart(classRoutineId: string) {
    try {
      const dateStr = format(currentDate, 'yyyy-MM-dd');
      const now = new Date();
      const timeStr = `${String(now.getHours()).padStart(2, '0')}:${String(now.getMinutes()).padStart(2, '0')}:00`;
      await recordsApi.start(classRoutineId, dateStr, timeStr);
      await loadStatus();
    } catch (err) {
      console.error('Failed to start routine:', err);
    }
  }

  async function handleComplete(classRoutineId: string, recordId: string | null) {
    if (!recordId) return;
    try {
      const now = new Date();
      const timeStr = `${String(now.getHours()).padStart(2, '0')}:${String(now.getMinutes()).padStart(2, '0')}:00`;
      await recordsApi.complete(recordId, timeStr);
      await loadStatus();
    } catch (err) {
      console.error('Failed to complete routine:', err);
    }
  }

  async function handleCancel(classRoutineId: string, recordId: string | null) {
    if (!recordId) return;
    try {
      await recordsApi.cancel(recordId, 'Cancelado manualmente');
      await loadStatus();
    } catch (err) {
      console.error('Failed to cancel routine:', err);
    }
  }

  return (
    <div className="space-y-6">
      <div>
        <h2 className="text-2xl font-bold">Registro de Rotina</h2>
        <p className="text-muted-foreground">
          Inicie, conclua ou cancele as rotinas da turma
        </p>
      </div>

      {/* Filters */}
      <div className="flex flex-wrap items-center gap-4">
        <div className="flex-1 min-w-[200px]">
          <Select
            value={selectedClass}
            onChange={(e) => setSelectedClass(e.target.value)}
            options={[
              { value: '', label: 'Selecione uma turma...' },
              ...classes.map((c) => ({ value: c.id, label: c.name })),
            ]}
          />
        </div>

        <div className="flex items-center gap-2">
          <Button
            variant="outline"
            size="icon"
            onClick={() => {
              const d = new Date(currentDate);
              d.setDate(d.getDate() - 1);
              setCurrentDate(d);
            }}
          >
            <ChevronLeft className="h-4 w-4" />
          </Button>
          <span className="text-sm font-medium min-w-[140px] text-center">
            {format(currentDate, "dd 'de' MMMM", { locale: ptBR })}
            <span className="text-muted-foreground ml-1">
              ({DAYS_OF_WEEK[currentDate.getDay()]})
            </span>
          </span>
          <Button
            variant="outline"
            size="icon"
            onClick={() => {
              const d = new Date(currentDate);
              d.setDate(d.getDate() + 1);
              setCurrentDate(d);
            }}
          >
            <ChevronRight className="h-4 w-4" />
          </Button>
        </div>
      </div>

      {/* Status Summary */}
      {status && (
        <div className="grid grid-cols-2 md:grid-cols-5 gap-3">
          <Card>
            <CardContent className="py-3 text-center">
              <p className="text-2xl font-bold">{status.totalRoutines}</p>
              <p className="text-xs text-muted-foreground">Total</p>
            </CardContent>
          </Card>
          <Card className="border-green-200">
            <CardContent className="py-3 text-center">
              <p className="text-2xl font-bold text-green-600">{status.completedRoutines}</p>
              <p className="text-xs text-muted-foreground">Concluídas</p>
            </CardContent>
          </Card>
          <Card className="border-blue-200">
            <CardContent className="py-3 text-center">
              <p className="text-2xl font-bold text-blue-600">{status.inProgressRoutines}</p>
              <p className="text-xs text-muted-foreground">Em andamento</p>
            </CardContent>
          </Card>
          <Card className="border-gray-200">
            <CardContent className="py-3 text-center">
              <p className="text-2xl font-bold text-gray-600">{status.pendingRoutines}</p>
              <p className="text-xs text-muted-foreground">Pendentes</p>
            </CardContent>
          </Card>
          <Card className="border-red-200">
            <CardContent className="py-3 text-center">
              <p className="text-2xl font-bold text-red-600">{status.cancelledRoutines}</p>
              <p className="text-xs text-muted-foreground">Canceladas</p>
            </CardContent>
          </Card>
        </div>
      )}

      {/* Routine List */}
      {!selectedClass && (
        <Card>
          <CardContent className="py-12 text-center">
            <Clock className="mx-auto h-12 w-12 text-muted-foreground mb-4" />
            <p className="text-muted-foreground">
              Selecione uma turma para começar
            </p>
          </CardContent>
        </Card>
      )}

      {loading && <p className="text-muted-foreground">Carregando...</p>}

      {status && status.items.length === 0 && !loading && (
        <Card>
          <CardContent className="py-8 text-center">
            <p className="text-muted-foreground">
              Nenhuma rotina programada para esta turma neste dia da semana.
            </p>
          </CardContent>
        </Card>
      )}

      {status && status.items.length > 0 && (
        <div className="space-y-3">
          {status.items
            .sort((a, b) => a.scheduledTime.localeCompare(b.scheduledTime))
            .map((item) => (
              <Card
                key={item.classRoutineId}
                className={`${
                  item.status === 'InProgress'
                    ? 'ring-2 ring-blue-400'
                    : item.status === 'Completed'
                    ? 'opacity-80'
                    : ''
                }`}
              >
                <CardContent className="py-4">
                  <div className="flex items-center justify-between">
                    <div className="flex items-center gap-3">
                      {statusIcon[item.status] || <Clock className="h-5 w-5" />}
                      <div>
                        <p className="font-medium">{item.routineName}</p>
                        <div className="flex items-center gap-2 text-sm text-muted-foreground">
                          <span>{item.scheduledTime.slice(0, 5)}</span>
                          <span>·</span>
                          <span>{item.durationMinutes} min</span>
                          <span>·</span>
                          <Badge
                            className={statusBadge[item.status] || ''}
                            variant="outline"
                          >
                            {statusLabel[item.status] || item.status}
                          </Badge>
                        </div>
                      </div>
                    </div>

                    {/* Actions */}
                    <div className="flex gap-2">
                      {item.status === 'Pending' && (
                        <Button
                          size="sm"
                          onClick={() => handleStart(item.classRoutineId)}
                        >
                          <Play className="mr-1 h-4 w-4" />
                          Iniciar
                        </Button>
                      )}
                      {item.status === 'InProgress' && (
                        <>
                          <Button
                            size="sm"
                            variant="outline"
                            onClick={() => handleCancel(item.classRoutineId, item.recordId)}
                          >
                            <XCircle className="mr-1 h-4 w-4" />
                            Cancelar
                          </Button>
                          <Button
                            size="sm"
                            variant="default"
                            className="bg-green-600 hover:bg-green-700"
                            onClick={() => handleComplete(item.classRoutineId, item.recordId)}
                          >
                            <CheckCircle2 className="mr-1 h-4 w-4" />
                            Concluir
                          </Button>
                        </>
                      )}
                      {item.status === 'Pending' && (
                        <Button
                          size="sm"
                          variant="ghost"
                          onClick={() => handleCancel(item.classRoutineId, item.recordId)}
                        >
                          <XCircle className="h-4 w-4" />
                        </Button>
                      )}
                    </div>
                  </div>
                </CardContent>
              </Card>
            ))}
        </div>
      )}
    </div>
  );
}
