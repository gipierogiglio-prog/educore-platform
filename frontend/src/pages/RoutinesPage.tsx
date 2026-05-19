import { useState, useEffect } from 'react';
import { Plus, Pencil, Trash2, Clock, Tag } from 'lucide-react';
import { Button } from '@/components/ui/button';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Input } from '@/components/ui/input';
import { Select } from '@/components/ui/select';
import { Badge } from '@/components/ui/badge';
import { routinesApi, type Routine, type CreateRoutineData } from '@/lib/api';

const CATEGORIES = [
  { value: 'Morning', label: 'Matinal' },
  { value: 'Activity', label: 'Atividade' },
  { value: 'Meal', label: 'Refeição' },
  { value: 'Rest', label: 'Descanso' },
  { value: 'Outdoor', label: 'Ao ar livre' },
  { value: 'Hygiene', label: 'Higiene' },
  { value: 'Transition', label: 'Transição' },
  { value: 'Closure', label: 'Encerramento' },
] as const;

const CATEGORY_LABELS: Record<string, string> = Object.fromEntries(
  CATEGORIES.map((c) => [c.value, c.label]),
);

const categoryColors: Record<string, string> = {
  Morning: 'bg-blue-100 text-blue-800',
  Activity: 'bg-purple-100 text-purple-800',
  Meal: 'bg-orange-100 text-orange-800',
  Rest: 'bg-indigo-100 text-indigo-800',
  Outdoor: 'bg-green-100 text-green-800',
  Hygiene: 'bg-cyan-100 text-cyan-800',
  Transition: 'bg-gray-100 text-gray-800',
  Closure: 'bg-rose-100 text-rose-800',
};

export default function RoutinesPage() {
  const [routines, setRoutines] = useState<Routine[]>([]);
  const [loading, setLoading] = useState(true);
  const [showForm, setShowForm] = useState(false);
  const [editingId, setEditingId] = useState<string | null>(null);
  const [form, setForm] = useState<CreateRoutineData>({
    name: '',
    category: 'Activity',
    expectedDurationMinutes: 30,
    description: '',
  });
  const [filterCategory, setFilterCategory] = useState('');

  useEffect(() => {
    loadRoutines();
  }, [filterCategory]);

  async function loadRoutines() {
    setLoading(true);
    try {
      const data = await routinesApi.list(true, filterCategory || undefined);
      setRoutines(data);
    } catch (err) {
      console.error('Failed to load routines:', err);
    } finally {
      setLoading(false);
    }
  }

  function resetForm() {
    setForm({ name: '', category: 'Activity', expectedDurationMinutes: 30, description: '' });
    setEditingId(null);
    setShowForm(false);
  }

  function editRoutine(r: Routine) {
    setForm({
      name: r.name,
      category: r.category,
      expectedDurationMinutes: r.expectedDurationMinutes,
      description: r.description || '',
    });
    setEditingId(r.id);
    setShowForm(true);
  }

  async function handleSubmit(e: React.FormEvent) {
    e.preventDefault();
    try {
      if (editingId) {
        await routinesApi.update({ id: editingId, ...form });
      } else {
        await routinesApi.create(form);
      }
      resetForm();
      await loadRoutines();
    } catch (err) {
      console.error('Failed to save routine:', err);
    }
  }

  async function handleDelete(id: string) {
    if (!confirm('Tem certeza que deseja desativar esta rotina?')) return;
    try {
      await routinesApi.delete(id);
      await loadRoutines();
    } catch (err) {
      console.error('Failed to delete routine:', err);
    }
  }

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <h2 className="text-2xl font-bold">Rotinas Diárias</h2>
          <p className="text-muted-foreground">
            Gerencie os tipos de rotinas disponíveis para as turmas
          </p>
        </div>
        <Button onClick={() => { resetForm(); setShowForm(true); }}>
          <Plus className="mr-2 h-4 w-4" />
          Nova Rotina
        </Button>
      </div>

      {/* Filter */}
      <div className="flex items-center gap-4">
        <Select
          value={filterCategory}
          onChange={(e) => setFilterCategory(e.target.value)}
          options={[
            { value: '', label: 'Todas as categorias' },
            ...CATEGORIES.map((c) => ({ value: c.value, label: c.label })),
          ]}
          className="w-48"
        />
      </div>

      {/* Form Modal */}
      {showForm && (
        <Card>
          <CardHeader>
            <CardTitle>{editingId ? 'Editar Rotina' : 'Nova Rotina'}</CardTitle>
          </CardHeader>
          <CardContent>
            <form onSubmit={handleSubmit} className="space-y-4">
              <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                <div className="space-y-2">
                  <label className="text-sm font-medium">Nome</label>
                  <Input
                    value={form.name}
                    onChange={(e) => setForm({ ...form, name: e.target.value })}
                    placeholder="Ex: Roda de Conversa"
                    required
                  />
                </div>
                <div className="space-y-2">
                  <label className="text-sm font-medium">Categoria</label>
                  <Select
                    value={form.category}
                    onChange={(e) => setForm({ ...form, category: e.target.value })}
                    options={CATEGORIES.map((c) => ({ value: c.value, label: c.label }))}
                  />
                </div>
                <div className="space-y-2">
                  <label className="text-sm font-medium">Duração (minutos)</label>
                  <Input
                    type="number"
                    min={1}
                    max={480}
                    value={form.expectedDurationMinutes}
                    onChange={(e) =>
                      setForm({ ...form, expectedDurationMinutes: Number(e.target.value) })
                    }
                    required
                  />
                </div>
                <div className="space-y-2">
                  <label className="text-sm font-medium">Descrição</label>
                  <Input
                    value={form.description || ''}
                    onChange={(e) => setForm({ ...form, description: e.target.value })}
                    placeholder="Descrição opcional"
                  />
                </div>
              </div>
              <div className="flex gap-2 justify-end">
                <Button type="button" variant="outline" onClick={resetForm}>
                  Cancelar
                </Button>
                <Button type="submit">
                  {editingId ? 'Atualizar' : 'Criar'}
                </Button>
              </div>
            </form>
          </CardContent>
        </Card>
      )}

      {/* List */}
      {loading ? (
        <p className="text-muted-foreground">Carregando...</p>
      ) : routines.length === 0 ? (
        <Card>
          <CardContent className="py-12 text-center">
            <Tag className="mx-auto h-12 w-12 text-muted-foreground mb-4" />
            <p className="text-muted-foreground">
              Nenhuma rotina encontrada. Crie a primeira!
            </p>
          </CardContent>
        </Card>
      ) : (
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
          {routines.map((r) => (
            <Card key={r.id}>
              <CardHeader className="pb-3">
                <div className="flex items-start justify-between">
                  <div>
                    <CardTitle className="text-base">{r.name}</CardTitle>
                    <Badge
                      className={`mt-1 ${categoryColors[r.category] || ''}`}
                      variant="outline"
                    >
                      {CATEGORY_LABELS[r.category] || r.category}
                    </Badge>
                  </div>
                </div>
              </CardHeader>
              <CardContent>
                <div className="flex items-center gap-2 text-sm text-muted-foreground mb-2">
                  <Clock className="h-4 w-4" />
                  <span>{r.expectedDurationMinutes} min</span>
                </div>
                {r.description && (
                  <p className="text-sm text-muted-foreground mb-3">{r.description}</p>
                )}
                <div className="flex gap-2 pt-2">
                  <Button
                    variant="outline"
                    size="sm"
                    onClick={() => editRoutine(r)}
                  >
                    <Pencil className="mr-1 h-3 w-3" />
                    Editar
                  </Button>
                  <Button
                    variant="destructive"
                    size="sm"
                    onClick={() => handleDelete(r.id)}
                  >
                    <Trash2 className="mr-1 h-3 w-3" />
                    Desativar
                  </Button>
                </div>
              </CardContent>
            </Card>
          ))}
        </div>
      )}
    </div>
  );
}
