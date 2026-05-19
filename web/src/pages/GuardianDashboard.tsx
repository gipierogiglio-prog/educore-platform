import { useState, useEffect } from 'react'
import {
  Users,
  BookOpen,
  Calendar,
  AlertTriangle,
  TrendingUp,
  UserCheck,
} from 'lucide-react'
import { Card, CardContent, CardHeader, CardTitle, CardDescription } from '@/components/ui/card'
import { Badge } from '@/components/ui/badge'
import { Progress } from '@/components/ui/progress'
import { Separator } from '@/components/ui/separator'
import { api } from '@/lib/api'

interface GradeItem {
  subjectName: string
  bimester: number
  value: number
  recoveryValue?: number | null
}

interface ChildAttendance {
  total: number
  present: number
  absent: number
  frequencyPercent: number
}

interface ChildData {
  id: string
  name: string
  email: string
  enrollment: string
  className: string
  grades: GradeItem[]
  attendance?: ChildAttendance | null
}

interface GuardianDashboardData {
  guardianId: string
  relationship: string
  children: ChildData[]
}

export function GuardianDashboard() {
  const [data, setData] = useState<GuardianDashboardData | null>(null)
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState('')
  const [expandedChild, setExpandedChild] = useState<string | null>(null)

  useEffect(() => {
    const fetchData = async () => {
      try {
        const result = await api.get<GuardianDashboardData>('/api/dashboard/guardian')
        setData(result)
        if (result.children.length > 0) {
          setExpandedChild(result.children[0].id)
        }
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
        <div className="text-muted-foreground animate-pulse">Carregando...</div>
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

  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-2xl font-bold">Visão do Responsável</h1>
        <p className="text-muted-foreground">
          {data.relationship && `Parentesco: ${data.relationship}`}
          {data.children.length > 0 && ` • ${data.children.length} filho(s)`}
        </p>
      </div>

      {/* Children overview cards */}
      <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-4">
        {data.children.length === 0 ? (
          <Card className="col-span-full">
            <CardContent className="pt-6 text-center text-muted-foreground">
              <Users size={48} className="mx-auto mb-2 opacity-50" />
              <p>Nenhum aluno vinculado a este responsável</p>
            </CardContent>
          </Card>
        ) : (
          data.children.map((child) => (
            <Card
              key={child.id}
              className={`cursor-pointer transition-colors ${
                expandedChild === child.id ? 'ring-1 ring-primary' : ''
              }`}
              onClick={() => setExpandedChild(child.id)}
            >
              <CardHeader>
                <CardTitle className="text-lg">{child.name}</CardTitle>
                <CardDescription>
                  {child.className} • {child.enrollment}
                </CardDescription>
              </CardHeader>
              <CardContent className="space-y-2">
                {child.attendance && (
                  <div className="flex items-center gap-2 text-sm">
                    <UserCheck size={16} className="text-emerald-400" />
                    <span>Frequência: {child.attendance.frequencyPercent}%</span>
                  </div>
                )}
                <div className="flex items-center gap-2 text-sm">
                  <TrendingUp size={16} className="text-primary" />
                  <span>{child.grades.length} nota(s) lançada(s)</span>
                </div>
              </CardContent>
            </Card>
          ))
        )}
      </div>

      {/* Detailed view for selected child */}
      {expandedChild && data.children.length > 0 && (() => {
        const child = data.children.find(c => c.id === expandedChild)
        if (!child) return null

        // Group grades by subject
        const bySubject = child.grades.reduce<Record<string, GradeItem[]>>((acc, grade) => {
          if (!acc[grade.subjectName]) acc[grade.subjectName] = []
          acc[grade.subjectName].push(grade)
          return acc
        }, {})

        return (
          <div className="space-y-6">
            {/* Grades by subject */}
            <Card>
              <CardHeader>
                <CardTitle className="text-lg flex items-center gap-2">
                  <BookOpen size={18} />
                  Notas — {child.name}
                </CardTitle>
              </CardHeader>
              <CardContent>
                {Object.keys(bySubject).length === 0 ? (
                  <p className="text-sm text-muted-foreground">Nenhuma nota registrada ainda</p>
                ) : (
                  <div className="overflow-x-auto">
                    <table className="w-full text-sm">
                      <thead>
                        <tr className="border-b border-border">
                          <th className="text-left p-2 font-medium">Disciplina</th>
                          <th className="text-center p-2 font-medium">1º Bim</th>
                          <th className="text-center p-2 font-medium">2º Bim</th>
                          <th className="text-center p-2 font-medium">3º Bim</th>
                          <th className="text-center p-2 font-medium">4º Bim</th>
                        </tr>
                      </thead>
                      <tbody>
                        {Object.entries(bySubject).map(([subjectName, grades]) => {
                          const bimGrades: (number | null)[] = [null, null, null, null]
                          grades.forEach(g => {
                            bimGrades[g.bimester - 1] = g.value
                          })
                          return (
                            <tr key={subjectName} className="border-b border-border/50">
                              <td className="p-2 font-medium">{subjectName}</td>
                              {bimGrades.map((val, i) => (
                                <td key={i} className="p-2 text-center font-mono">
                                  {val != null ? val.toFixed(1) : '-'}
                                </td>
                              ))}
                            </tr>
                          )
                        })}
                      </tbody>
                    </table>
                  </div>
                )}
              </CardContent>
            </Card>

            {/* Attendance */}
            <Card>
              <CardHeader>
                <CardTitle className="text-lg flex items-center gap-2">
                  <Calendar size={18} />
                  Frequência — {child.name}
                </CardTitle>
              </CardHeader>
              <CardContent>
                {child.attendance ? (
                  <div className="space-y-3">
                    <div className="flex justify-between text-sm">
                      <span>Presença</span>
                      <span className="text-muted-foreground">
                        {child.attendance.present} de {child.attendance.total} dias
                      </span>
                    </div>
                    <Progress
                      value={child.attendance.frequencyPercent}
                      className={`h-3 ${child.attendance.frequencyPercent < 75 ? 'bg-destructive/20' : ''}`}
                    />
                    <div className="flex justify-between text-sm">
                      <span className={child.attendance.frequencyPercent >= 75 ? 'text-emerald-400' : 'text-destructive'}>
                        {child.attendance.frequencyPercent}% de frequência
                      </span>
                      <span className="text-muted-foreground">
                        {child.attendance.absent} falta(s)
                      </span>
                    </div>
                    {child.attendance.frequencyPercent < 75 && (
                      <div className="flex items-center gap-2 text-sm text-amber-400 bg-amber-400/10 p-2 rounded">
                        <AlertTriangle size={16} />
                        Frequência abaixo do mínimo recomendado (75%)
                      </div>
                    )}
                  </div>
                ) : (
                  <p className="text-sm text-muted-foreground">Nenhum registro de frequência</p>
                )}
              </CardContent>
            </Card>

            {/* Class performance summary */}
            <Card>
              <CardHeader>
                <CardTitle className="text-lg flex items-center gap-2">
                  <TrendingUp size={18} />
                  Resumo de Desempenho
                </CardTitle>
              </CardHeader>
              <CardContent>
                <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
                  {Object.entries(bySubject).map(([subjectName, grades]) => {
                    const avg = grades.reduce((acc, g) => acc + g.value, 0) / grades.length
                    const isGood = avg >= 7
                    return (
                      <div key={subjectName} className="border border-border rounded-lg p-3">
                        <div className="flex items-center justify-between mb-1">
                          <span className="font-medium text-sm">{subjectName}</span>
                          <Badge variant={isGood ? 'success' : 'warning'}>
                            {isGood ? 'Bom' : 'Atenção'}
                          </Badge>
                        </div>
                        <p className="text-2xl font-bold">{avg.toFixed(1)}</p>
                        <p className="text-xs text-muted-foreground">Média {grades.length} avaliações</p>
                      </div>
                    )
                  })}
                </div>
              </CardContent>
            </Card>
          </div>
        )
      })()}
    </div>
  )
}
