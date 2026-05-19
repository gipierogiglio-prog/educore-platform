import { useState, useEffect } from 'react'
import {
  GraduationCap,
  BookOpen,
  Users,
  ClipboardList,
  TrendingUp,
  Calendar,
} from 'lucide-react'
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card'
import { Badge } from '@/components/ui/badge'
import { Progress } from '@/components/ui/progress'
import { api } from '@/lib/api'

interface SubjectInfo {
  subjectId: string
  subjectName: string
}

interface ClassInfo {
  classId: string
  className: string
  subjects: SubjectInfo[]
}

interface StudentCount {
  className: string
  count: number
}

interface Grade {
  studentId: string
  studentName: string
  subjectId: string
  bimester: number
  value: number
  recoveryValue?: number | null
}

interface AttendanceStat {
  className: string
  total: number
  present: number
  absent: number
}

interface TeacherDashboardData {
  teacherId: string
  specialization?: string
  hireDate: string
  totalClasses: number
  totalSubjects: number
  classes: ClassInfo[]
  studentCountByClass: StudentCount[]
  recentGrades: Grade[]
  attendanceLast30Days: AttendanceStat[]
}

export function TeacherDashboard() {
  const [data, setData] = useState<TeacherDashboardData | null>(null)
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState('')

  useEffect(() => {
    const fetchData = async () => {
      try {
        const result = await api.get<TeacherDashboardData>('/api/dashboard/teacher')
        setData(result)
      } catch (err: any) {
        setError(err.message)
      } finally {
        setLoading(false)
      }
    }
    fetchData()
  }, [])

  if (loading) {
    return (
      <div className="flex items-center justify-center h-64">
        <div className="text-muted-foreground animate-pulse">Carregando dashboard...</div>
      </div>
    )
  }

  if (error) {
    return (
      <div className="flex items-center justify-center h-64">
        <div className="text-destructive">Erro: {error}</div>
      </div>
    )
  }

  if (!data) return null

  const totalStudents = data.studentCountByClass.reduce((acc, c) => acc + c.count, 0)

  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-2xl font-bold">Dashboard do Professor</h1>
        <p className="text-muted-foreground">
          {data.specialization && `Especialização: ${data.specialization}`}
        </p>
      </div>

      {/* Stats cards */}
      <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-4">
        <Card>
          <CardHeader className="flex flex-row items-center justify-between pb-2">
            <CardTitle className="text-sm font-medium text-muted-foreground">Turmas</CardTitle>
            <GraduationCap size={18} className="text-primary" />
          </CardHeader>
          <CardContent>
            <p className="text-3xl font-bold">{data.totalClasses}</p>
          </CardContent>
        </Card>
        <Card>
          <CardHeader className="flex flex-row items-center justify-between pb-2">
            <CardTitle className="text-sm font-medium text-muted-foreground">Disciplinas</CardTitle>
            <BookOpen size={18} className="text-primary" />
          </CardHeader>
          <CardContent>
            <p className="text-3xl font-bold">{data.totalSubjects}</p>
          </CardContent>
        </Card>
        <Card>
          <CardHeader className="flex flex-row items-center justify-between pb-2">
            <CardTitle className="text-sm font-medium text-muted-foreground">Total Alunos</CardTitle>
            <Users size={18} className="text-primary" />
          </CardHeader>
          <CardContent>
            <p className="text-3xl font-bold">{totalStudents}</p>
          </CardContent>
        </Card>
        <Card>
          <CardHeader className="flex flex-row items-center justify-between pb-2">
            <CardTitle className="text-sm font-medium text-muted-foreground">Últimas Notas</CardTitle>
            <ClipboardList size={18} className="text-primary" />
          </CardHeader>
          <CardContent>
            <p className="text-3xl font-bold">{data.recentGrades.length}</p>
          </CardContent>
        </Card>
      </div>

      {/* Classes and subjects */}
      <Card>
        <CardHeader>
          <CardTitle className="text-lg">Minhas Turmas e Disciplinas</CardTitle>
        </CardHeader>
        <CardContent>
          <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
            {data.classes.map((cls) => (
              <div key={cls.classId} className="border border-border rounded-lg p-4 space-y-2">
                <div className="flex items-center justify-between">
                  <h3 className="font-semibold">{cls.className}</h3>
                  <Badge>{cls.subjects.length} disciplina(s)</Badge>
                </div>
                <div className="flex flex-wrap gap-1">
                  {cls.subjects.map((subj) => (
                    <Badge key={subj.subjectId} variant="secondary">
                      {subj.subjectName}
                    </Badge>
                  ))}
                </div>
              </div>
            ))}
          </div>
        </CardContent>
      </Card>

      {/* Students per class */}
      <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
        <Card>
          <CardHeader>
            <CardTitle className="text-lg flex items-center gap-2">
              <Users size={18} />
              Alunos por Turma
            </CardTitle>
          </CardHeader>
          <CardContent>
            <div className="space-y-3">
              {data.studentCountByClass.map((item) => {
                const pct = totalStudents > 0 ? (item.count / totalStudents) * 100 : 0
                return (
                  <div key={item.className}>
                    <div className="flex justify-between text-sm mb-1">
                      <span>{item.className}</span>
                      <span className="text-muted-foreground">{item.count} alunos</span>
                    </div>
                    <Progress value={pct} className="h-2" />
                  </div>
                )
              })}
            </div>
          </CardContent>
        </Card>

        {/* Attendance last 30 days */}
        <Card>
          <CardHeader>
            <CardTitle className="text-lg flex items-center gap-2">
              <Calendar size={18} />
              Frequência (últimos 30 dias)
            </CardTitle>
          </CardHeader>
          <CardContent>
            <div className="space-y-4">
              {data.attendanceLast30Days.map((att) => {
                const freqPct = att.total > 0 ? (att.present / att.total) * 100 : 0
                return (
                  <div key={att.className}>
                    <div className="flex justify-between text-sm mb-1">
                      <span>{att.className}</span>
                      <span className="text-muted-foreground">
                        {att.present}/{att.total} — {freqPct.toFixed(0)}%
                      </span>
                    </div>
                    <Progress value={freqPct} className={`h-2 ${freqPct < 75 ? 'bg-destructive/20' : ''}`} />
                    <div className="flex gap-2 mt-1 text-xs text-muted-foreground">
                      <span className="text-emerald-400">✅ {att.present} presentes</span>
                      <span className="text-destructive">❌ {att.absent} ausentes</span>
                    </div>
                  </div>
                )
              })}
              {data.attendanceLast30Days.length === 0 && (
                <p className="text-sm text-muted-foreground">Nenhum registro de frequência nos últimos 30 dias</p>
              )}
            </div>
          </CardContent>
        </Card>
      </div>

      {/* Recent grades */}
      <Card>
        <CardHeader>
          <CardTitle className="text-lg flex items-center gap-2">
            <TrendingUp size={18} />
            Últimas Notas Lançadas
          </CardTitle>
        </CardHeader>
        <CardContent>
          {data.recentGrades.length > 0 ? (
            <div className="overflow-x-auto">
              <table className="w-full text-sm">
                <thead>
                  <tr className="border-b border-border">
                    <th className="text-left p-2 font-medium">Aluno</th>
                    <th className="text-left p-2 font-medium">Bimestre</th>
                    <th className="text-right p-2 font-medium">Nota</th>
                    <th className="text-right p-2 font-medium">Recuperação</th>
                  </tr>
                </thead>
                <tbody>
                  {data.recentGrades.map((g, i) => (
                    <tr key={i} className="border-b border-border/50">
                      <td className="p-2">{g.studentName}</td>
                      <td className="p-2">{g.bimester}º</td>
                      <td className="p-2 text-right font-mono">{g.value?.toFixed(1)}</td>
                      <td className="p-2 text-right font-mono">
                        {g.recoveryValue != null ? g.recoveryValue.toFixed(1) : '-'}
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          ) : (
            <p className="text-sm text-muted-foreground">Nenhuma nota lançada ainda</p>
          )}
        </CardContent>
      </Card>
    </div>
  )
}
