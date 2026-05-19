import { useState, useRef, type ChangeEvent } from 'react'
import { Upload, FileSpreadsheet, AlertCircle, CheckCircle2, X, Download } from 'lucide-react'
import { Button } from '@/components/ui/button'
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card'
import { Badge } from '@/components/ui/badge'
import { Progress } from '@/components/ui/progress'
import { Separator } from '@/components/ui/separator'
import { api } from '@/lib/api'

interface PreviewRow {
  name: string
  email: string
  password?: string
  phone?: string
}

interface PreviewResponse {
  totalRows: number
  rows: PreviewRow[]
  existingEmails: string[]
}

interface ImportResult {
  imported: number
  skipped: number
  errors: string[]
  totalInFile: number
}

export function ImportStudents() {
  const fileInputRef = useRef<HTMLInputElement>(null)
  const [file, setFile] = useState<File | null>(null)
  const [preview, setPreview] = useState<PreviewResponse | null>(null)
  const [importing, setImporting] = useState(false)
  const [result, setResult] = useState<ImportResult | null>(null)
  const [error, setError] = useState('')
  const [dragOver, setDragOver] = useState(false)

  const handleFile = async (f: File) => {
    const ext = f.name.split('.').pop()?.toLowerCase()
    if (!['csv', 'xlsx'].includes(ext || '')) {
      setError('Formato não suportado. Use .csv ou .xlsx')
      return
    }
    setFile(f)
    setError('')
    setResult(null)
    setPreview(null)

    const formData = new FormData()
    formData.append('file', f)

    try {
      const data = await api.post<PreviewResponse>('/api/students/import/preview', formData)
      setPreview(data)
    } catch (err: any) {
      setError(err.message)
    }
  }

  const handleDrop = (e: React.DragEvent) => {
    e.preventDefault()
    setDragOver(false)
    const f = e.dataTransfer.files[0]
    if (f) handleFile(f)
  }

  const handleSelect = (e: ChangeEvent<HTMLInputElement>) => {
    const f = e.target.files?.[0]
    if (f) handleFile(f)
  }

  const handleImport = async () => {
    if (!file) return
    setImporting(true)
    setError('')
    const formData = new FormData()
    formData.append('file', file)

    try {
      const data = await api.post<ImportResult>('/api/students/import/execute', formData)
      setResult(data)
    } catch (err: any) {
      setError(err.message)
    } finally {
      setImporting(false)
    }
  }

  const reset = () => {
    setFile(null)
    setPreview(null)
    setResult(null)
    setError('')
    if (fileInputRef.current) fileInputRef.current.value = ''
  }

  return (
    <div className="max-w-4xl mx-auto space-y-6">
      <div>
        <h1 className="text-2xl font-bold">Importar Alunos</h1>
        <p className="text-muted-foreground">
          Faça upload de um arquivo CSV ou Excel com os dados dos alunos
        </p>
      </div>

      <Card>
        <CardHeader>
          <CardTitle className="text-lg flex items-center gap-2">
            <Upload size={20} />
            Upload de Arquivo
          </CardTitle>
          <CardDescription>
            Arraste o arquivo ou clique para selecionar. Formatos aceitos: .csv, .xlsx
          </CardDescription>
        </CardHeader>
        <CardContent>
          {!file ? (
            <div
              onDragOver={(e) => { e.preventDefault(); setDragOver(true) }}
              onDragLeave={() => setDragOver(false)}
              onDrop={handleDrop}
              onClick={() => fileInputRef.current?.click()}
              className={`border-2 border-dashed rounded-lg p-12 text-center cursor-pointer transition-colors ${
                dragOver ? 'border-primary bg-primary/5' : 'border-border hover:border-primary/50'
              }`}
            >
              <FileSpreadsheet className="mx-auto mb-4 text-muted-foreground" size={48} />
              <p className="text-muted-foreground mb-2">
                Arraste o arquivo aqui ou <span className="text-primary">clique para selecionar</span>
              </p>
              <p className="text-xs text-muted-foreground">CSV ou Excel (até 10MB)</p>
              <input
                ref={fileInputRef}
                type="file"
                accept=".csv,.xlsx"
                className="hidden"
                onChange={handleSelect}
              />
            </div>
          ) : (
            <div className="space-y-4">
              <div className="flex items-center justify-between p-3 bg-secondary rounded-lg">
                <div className="flex items-center gap-3">
                  <FileSpreadsheet className="text-primary" size={24} />
                  <div>
                    <p className="font-medium">{file.name}</p>
                    <p className="text-xs text-muted-foreground">
                      {(file.size / 1024).toFixed(1)} KB
                    </p>
                  </div>
                </div>
                <Button variant="ghost" size="icon" onClick={reset}>
                  <X size={16} />
                </Button>
              </div>
            </div>
          )}
        </CardContent>
      </Card>

      {error && (
        <Card className="border-destructive">
          <CardContent className="pt-6 flex items-center gap-2 text-destructive">
            <AlertCircle size={20} />
            {error}
          </CardContent>
        </Card>
      )}

      {preview && !result && (
        <Card>
          <CardHeader>
            <CardTitle className="text-lg flex items-center gap-2">
              <CheckCircle2 size={20} className="text-primary" />
              Preview — {preview.totalRows} registro(s) encontrado(s)
            </CardTitle>
            {preview.existingEmails.length > 0 && (
              <CardDescription className="text-amber-400">
                ⚠️ {preview.existingEmails.length} email(s) já existem e serão ignorados
              </CardDescription>
            )}
          </CardHeader>
          <CardContent>
            <div className="overflow-x-auto">
              <table className="w-full text-sm">
                <thead>
                  <tr className="border-b border-border">
                    <th className="text-left p-2 font-medium">#</th>
                    <th className="text-left p-2 font-medium">Nome</th>
                    <th className="text-left p-2 font-medium">Email</th>
                    <th className="text-left p-2 font-medium">Telefone</th>
                    <th className="text-left p-2 font-medium">Status</th>
                  </tr>
                </thead>
                <tbody>
                  {preview.rows.slice(0, 50).map((row, i) => (
                    <tr key={i} className="border-b border-border/50">
                      <td className="p-2 text-muted-foreground">{i + 1}</td>
                      <td className="p-2">{row.name}</td>
                      <td className="p-2">{row.email}</td>
                      <td className="p-2">{row.phone || '-'}</td>
                      <td className="p-2">
                        {preview.existingEmails.includes(row.email) ? (
                          <Badge variant="warning">Já existe</Badge>
                        ) : (
                          <Badge variant="success">Novo</Badge>
                        )}
                      </td>
                    </tr>
                  ))}
                  {preview.rows.length > 50 && (
                    <tr>
                      <td colSpan={5} className="p-2 text-center text-muted-foreground">
                        ...e mais {preview.rows.length - 50} registro(s)
                      </td>
                    </tr>
                  )}
                </tbody>
              </table>
            </div>

            <Separator className="my-4" />

            <div className="flex items-center gap-3">
              <Button onClick={handleImport} disabled={importing}>
                {importing ? 'Importando...' : `Importar ${preview.rows.length} registros`}
              </Button>
              <Button variant="outline" onClick={reset}>
                Cancelar
              </Button>
            </div>
          </CardContent>
        </Card>
      )}

      {result && (
        <Card>
          <CardHeader>
            <CardTitle className="text-lg flex items-center gap-2">
              {result.imported > 0 ? (
                <CheckCircle2 size={20} className="text-emerald-400" />
              ) : (
                <AlertCircle size={20} className="text-amber-400" />
              )}
              Resultado da Importação
            </CardTitle>
          </CardHeader>
          <CardContent className="space-y-4">
            <div className="flex gap-6">
              <div className="text-center">
                <p className="text-3xl font-bold text-emerald-400">{result.imported}</p>
                <p className="text-xs text-muted-foreground">Importados</p>
              </div>
              <div className="text-center">
                <p className="text-3xl font-bold text-amber-400">{result.skipped}</p>
                <p className="text-xs text-muted-foreground">Ignorados</p>
              </div>
              <div className="text-center">
                <p className="text-3xl font-bold">{result.totalInFile}</p>
                <p className="text-xs text-muted-foreground">Total no arquivo</p>
              </div>
            </div>

            {result.errors.length > 0 && (
              <div className="text-sm text-destructive">
                <p className="font-medium mb-1">Erros:</p>
                <ul className="list-disc pl-5 space-y-0.5">
                  {result.errors.map((err, i) => (
                    <li key={i}>{err}</li>
                  ))}
                </ul>
              </div>
            )}

            <Button onClick={reset}>Importar outro arquivo</Button>
          </CardContent>
        </Card>
      )}
    </div>
  )
}
