import { useState, useEffect } from 'react';
import { format } from 'date-fns';
import { ptBR } from 'date-fns/locale/pt-BR';
import {
  ChevronLeft,
  ChevronRight,
  BarChart3,
  CheckCircle2,
  XCircle,
  Clock,
  RotateCcw,
  RefreshCw,
} from 'lucide-react';
import { Button } from '@/components/ui/button';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Select } from '@/components/ui/select';
import { Badge } from '@/components/ui/badge';
import { statusApi, type RoutineStatus, type RoutineStatusItem } from '@/lib/api';

const DAYS_OF_WEEK = [
  'Domingo', 'Segunda', 'Terça', 'Quarta', 'Quinta', 'Sexta', 'Sábado',
];

const statusBadge: Record<string, string> = {
  Pending: 'bg-gray-100 text-gray-800',
  InProgress: 'bg-blue-100 text-blue-800',
  Completed: 'bg-green-100 text-green-800',
  Cancelled: 'bg-red-100 text-red-800',
};

const statusLabel: Record<string, string> = {
  Pending: 'Pendente',
  InProgress: 'Em andamento',
  Completed: 'Concluída',
  Cancelled: 'Cancelada',
};

function StatusPieChart({ status }: { status: RoutineStatus }) {
  const total = status.totalRoutines || 1;
  const completedDeg = (status.completedRoutines / total) * 360;
  const inProgressDeg = (status.inProgressRoutines / total) * 360;
  const cancelledDeg = (status.cancelledRoutines / total) * 360;

  return (
    <div className="flex flex-col items-center gap-3">
      <div className="relative w-32 h-32">
        {/* Simple pie representation using conic-gradient */}
        <div
          className="w-32 h-32 rounded-full"
          style={{
            background: `conic-gradient(
              #22c55e 0deg ${completedDeg}deg,
              #3b82f6 ${completedDeg}deg ${completedDeg + inProgressDeg}deg,
              #ef4444 ${completedDeg + inProgressDeg}deg ${completedDeg + inProgressDeg + cancelledDeg}deg,
              #e5e7eb ${completedDeg + inProgressDeg + cancelledDeg}deg 360deg
            )`,
          }}
        />
        <div className="absolute inset-4 bg-white rounded-full flex items-center justify-center">
          <span className="text-2xl font-bold">{status.completionPercentage}%</span>
        </div>
      </div>

      <div className="flex flex-wrap gap-3 text-sm justify-center">
        <div className="flex items-center gap-1">
          <div className="w-3 h-3 rounded-full bg-green-500" />
          <span>{status.completedRoutines} concluídas</span>
        </div>
        <div className="flex items-center gap-1">
          <div className="w-3 h-3 rounded-full bg-blue-500" />
          <span>{status.inProgressRoutines} em andamento</span>
        </div>
        <div className="flex items-center gap-1">
          <div className="w-3 h-3 rounded-full bg-red-500" />
          <span>{status.cancelledRoutines} canceladas</span>
        </div>
        <div className="flex items-center gap-1">
          <div className="w-3 h-3 rounded-full bg-gray-200" />
          <span>{status.pendingRoutines} pendentes</span>
        </div>
      </div>
    </div>
  );
}

function StatusTimeline({ items }: { items: RoutineStatusItem[] }) {
  return (
    <div className="space-y-2">
      {items
        .sort((a, b) => a.scheduledTime.localeCompare(b.scheduledTime))
        .map((item) => (
          <div key={item.classRoutineId} className="flex items-center gap-3 py-2 border-b last:border-0">
            {/* Time column */}
            <div className="w-16 text-right">
              <span className="text-sm font-medium">{item.scheduledTime.slice(0, 5)}</span>
            </div>

            {/* Timeline dot */}
            <div className="flex flex-col items-center">
              <div
                className={`w-3 h-3 rounded-full ${
                  item.status === 'Completed'
                    ? 'bg-green-500'
                    : item.status === 'InProgress'
                    ? 'bg-blue-500 animate-pulse'
                    : item.status === 'Cancelled'
                    ? 'bg-red-400'
                    : 'bg-gray-300'
                }`}
              />
              <div className="w-0.5 h-6 bg-border" />
            </div>

            {/* Content */}
            <div className="flex-1 min-w-0">
              <div className="flex items-center justify-between">
                <p className="text-sm font-medium truncate">{item.routineName}</p>
                <Badge
                  className={`ml-2 shrink-0 ${statusBadge[item.status] || ''}`}
                  variant="outline"
                >
                  {statusLabel[item.status] || item.status}
                </Badge>
              </div>
              <p className="text-xs text-muted-foreground mt-0.5">
                <span>{item.routineCategory}</span>
                <span className="mx-1">·</span>
                <span>{item.durationMinutes} min</span>
                {item.startTime && (
                  <>
                    <span className="mx-1">·</span>
                    <span>Início: {item.startTime.slice(0, 5)}</span>
                  </>
                )}
                {item.endTime && (
                  <>
                    <span className="mx-1">·</span>
                    <span>Término: {item.endTime.slice(0, 5)}</span>
                  </>
                )}
              </p>
            </div>

            {/* Status icon */}
            <div className="shrink-0">
              {item.status === 'Completed' && (
                <CheckCircle2 className="h-5 w-5 text-green-500" />
              )}
              {item.status === 'InProgress' && (
                <RotateCcw className="h-5 w-5 text-blue-500 animate-spin" />
              )}
              {item.status === 'Cancelled' && (
                <XCircle className="h-5 w-5 text-red-400" />
              )}
              {item.status === 'Pending' && (
                <Clock className="h-5 w-5 text-gray-300" />
              )}
            </div>
          </div>
        ))}
    </div>
  );
}

export default function RoutineStatusDashboard() {
  const [classes, setClasses] = useState<{ id: string; name: string }[]>([]);
  const [selectedClass, setSelectedClass] = useState('');
  const [currentDate, setCurrentDate] = useState(new Date());
  const [status, setStatus] = useState<RoutineStatus | null>(null);
  const [loading, setLoading] = useState(false);
  const [multiDayRange, setMultiDayRange] = useState(1); // 1 = today, 7 = week

  useEffect(() => {
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

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <h2 className="text-2xl font-bold">Dashboard de Status</h2>
          <p className="text-muted-foreground">
            Acompanhe o andamento das rotinas da turma
          </p>
        </div>
        <Button variant="outline" size="sm" onClick={loadStatus}>
          <RefreshCw className="mr-2 h-4 w-4" />
          Atualizar
        </Button>
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
          <span className="text-sm font-medium min-w-[160px] text-center">
            {format(currentDate, "EEEE, dd 'de' MMMM", { locale: ptBR })}
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

        <Button
          variant="ghost"
          size="sm"
          onClick={() => setCurrentDate(new Date())}
        >
          Hoje
        </Button>
      </div>

      {/* Loading */}
      {loading && (
        <div className="flex items-center justify-center py-12">
          <RefreshCw className="h-6 w-6 animate-spin text-muted-foreground" />
        </div>
      )}

      {/* No selection */}
      {!selectedClass && !loading && (
        <Card>
          <CardContent className="py-12 text-center">
            <BarChart3 className="mx-auto h-12 w-12 text-muted-foreground mb-4" />
            <p className="text-muted-foreground">
              Selecione uma turma para visualizar o dashboard
            </p>
          </CardContent>
        </Card>
      )}

      {/* Dashboard */}
      {status && !loading && (
        <>
          {/* Summary Cards */}
          <div className="grid grid-cols-1 md:grid-cols-4 gap-4">
            <Card className="bg-gradient-to-br from-blue-50 to-blue-100 dark:from-blue-950 dark:to-blue-900">
              <CardContent className="py-4 text-center">
                <p className="text-3xl font-bold text-blue-700 dark:text-blue-300">
                  {status.totalRoutines}
                </p>
                <p className="text-sm text-blue-600 dark:text-blue-400 font-medium">
                  Total de Rotinas
                </p>
              </CardContent>
            </Card>
            <Card className="bg-gradient-to-br from-green-50 to-green-100 dark:from-green-950 dark:to-green-900">
              <CardContent className="py-4 text-center">
                <p className="text-3xl font-bold text-green-700 dark:text-green-300">
                  {status.completedRoutines}
                </p>
                <p className="text-sm text-green-600 dark:text-green-400 font-medium">
                  Concluídas
                </p>
              </CardContent>
            </Card>
            <Card className="bg-gradient-to-br from-yellow-50 to-yellow-100 dark:from-yellow-950 dark:to-yellow-900">
              <CardContent className="py-4 text-center">
                <p className="text-3xl font-bold text-yellow-700 dark:text-yellow-300">
                  {status.pendingRoutines + status.inProgressRoutines}
                </p>
                <p className="text-sm text-yellow-600 dark:text-yellow-400 font-medium">
                  Pendentes + Em Andamento
                </p>
              </CardContent>
            </Card>
            <Card className="bg-gradient-to-br from-purple-50 to-purple-100 dark:from-purple-950 dark:to-purple-900">
              <CardContent className="py-4 text-center">
                <p className="text-3xl font-bold text-purple-700 dark:text-purple-300">
                  {status.completionPercentage}%
                </p>
                <p className="text-sm text-purple-600 dark:text-purple-400 font-medium">
                  Progresso
                </p>
              </CardContent>
            </Card>
          </div>

          {/* Pie Chart + Timeline */}
          <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
            <Card className="lg:col-span-1">
              <CardHeader>
                <CardTitle className="text-base">Visão Geral</CardTitle>
              </CardHeader>
              <CardContent>
                <StatusPieChart status={status} />
              </CardContent>
            </Card>

            <Card className="lg:col-span-2">
              <CardHeader>
                <CardTitle className="text-base">
                  Linha do Tempo - {format(currentDate, "dd 'de' MMMM", { locale: ptBR })}
                </CardTitle>
              </CardHeader>
              <CardContent>
                <StatusTimeline items={status.items} />
              </CardContent>
            </Card>
          </div>

          {/* All routines table */}
          <Card>
            <CardHeader>
              <CardTitle className="text-base">Detalhamento</CardTitle>
            </CardHeader>
            <CardContent>
              <div className="overflow-x-auto">
                <table className="w-full text-sm">
                  <thead>
                    <tr className="border-b text-left">
                      <th className="pb-2 font-medium">Horário</th>
                      <th className="pb-2 font-medium">Rotina</th>
                      <th className="pb-2 font-medium">Categoria</th>
                      <th className="pb-2 font-medium">Duração</th>
                      <th className="pb-2 font-medium">Status</th>
                      <th className="pb-2 font-medium">Início</th>
                      <th className="pb-2 font-medium">Término</th>
                    </tr>
                  </thead>
                  <tbody>
                    {status.items
                      .sort((a, b) => a.scheduledTime.localeCompare(b.scheduledTime))
                      .map((item) => (
                        <tr key={item.classRoutineId} className="border-b hover:bg-muted/50">
                          <td className="py-2.5 font-medium">{item.scheduledTime.slice(0, 5)}</td>
                          <td className="py-2.5">{item.routineName}</td>
                          <td className="py-2.5 text-muted-foreground">{item.routineCategory}</td>
                          <td className="py-2.5">{item.durationMinutes} min</td>
                          <td className="py-2.5">
                            <Badge
                              className={statusBadge[item.status] || ''}
                              variant="outline"
                            >
                              {statusLabel[item.status] || item.status}
                            </Badge>
                          </td>
                          <td className="py-2.5">{item.startTime?.slice(0, 5) || '-'}</td>
                          <td className="py-2.5">{item.endTime?.slice(0, 5) || '-'}</td>
                        </tr>
                      ))}
                  </tbody>
                </table>
              </div>
            </CardContent>
          </Card>
        </>
      )}
    </div>
  );
}
