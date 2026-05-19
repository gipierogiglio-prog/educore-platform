const API_BASE = '/api';

async function request<T>(path: string, options?: RequestInit): Promise<T> {
  const token = localStorage.getItem('token');
  const headers: Record<string, string> = {
    'Content-Type': 'application/json',
    ...(options?.headers as Record<string, string>),
  };

  if (token) {
    headers['Authorization'] = `Bearer ${token}`;
  }

  const res = await fetch(`${API_BASE}${path}`, {
    ...options,
    headers,
  });

  if (!res.ok) {
    const err = await res.json().catch(() => ({ error: res.statusText }));
    throw new Error(err.error || err.title || 'Request failed');
  }

  if (res.status === 204 || res.headers.get('content-length') === '0') {
    return undefined as T;
  }

  return res.json();
}

// --- Rotinas (Task #199) ---
export interface Routine {
  id: string;
  name: string;
  description: string | null;
  category: string;
  expectedDurationMinutes: number;
  isActive: boolean;
  createdAt: string;
  updatedAt: string | null;
}

export interface CreateRoutineData {
  name: string;
  category: string;
  expectedDurationMinutes: number;
  description?: string;
}

export interface UpdateRoutineData {
  id: string;
  name?: string;
  category?: string;
  expectedDurationMinutes?: number;
  description?: string;
}

export const routinesApi = {
  list: (activeOnly?: boolean, category?: string) => {
    const params = new URLSearchParams();
    if (activeOnly !== undefined) params.set('activeOnly', String(activeOnly));
    if (category) params.set('category', category);
    return request<Routine[]>(`/daily-routines/routines?${params}`);
  },
  getById: (id: string) => request<Routine>(`/daily-routines/routines/${id}`),
  create: (data: CreateRoutineData) =>
    request<Routine>('/daily-routines/routines', {
      method: 'POST',
      body: JSON.stringify(data),
    }),
  update: (data: UpdateRoutineData) =>
    request<Routine>(`/daily-routines/routines/${data.id}`, {
      method: 'PUT',
      body: JSON.stringify(data),
    }),
  delete: (id: string) =>
    request<void>(`/daily-routines/routines/${id}`, { method: 'DELETE' }),
};

// --- Class Routines (Task #201) ---
export interface ClassRoutine {
  id: string;
  classId: string;
  routineId: string;
  routineName: string | null;
  routineCategory: string | null;
  weekDay: number;
  startTime: string;
  durationMinutes: number;
  isActive: boolean;
  createdAt: string;
  updatedAt: string | null;
}

export interface CreateClassRoutineData {
  classId: string;
  routineId: string;
  weekDay: number;
  startTime: string;
  durationMinutes: number;
}

export const classRoutinesApi = {
  list: (classId: string, weekDay?: number) => {
    const params = new URLSearchParams({ classId });
    if (weekDay !== undefined) params.set('weekDay', String(weekDay));
    return request<ClassRoutine[]>(`/daily-routines/class-routines?${params}`);
  },
  create: (data: CreateClassRoutineData) =>
    request<ClassRoutine>('/daily-routines/class-routines', {
      method: 'POST',
      body: JSON.stringify(data),
    }),
  delete: (id: string) =>
    request<void>(`/daily-routines/class-routines/${id}`, { method: 'DELETE' }),
};

// --- Routine Records ---
export interface RoutineRecord {
  id: string;
  classRoutineId: string;
  routineName: string | null;
  routineCategory: string | null;
  recordDate: string;
  startTime: string | null;
  endTime: string | null;
  status: string;
  notes: string | null;
  teacherId: string | null;
  createdAt: string;
  updatedAt: string | null;
}

export const recordsApi = {
  getByClassAndDate: (classId: string, date: string) => {
    const params = new URLSearchParams({ classId, date });
    return request<RoutineRecord[]>(
      `/daily-routines/records/by-class-date?${params}`,
    );
  },
  start: (classRoutineId: string, recordDate: string, startTime: string, teacherId?: string) =>
    request<RoutineRecord>('/daily-routines/records/start', {
      method: 'POST',
      body: JSON.stringify({ classRoutineId, recordDate, startTime, teacherId }),
    }),
  complete: (id: string, endTime?: string, notes?: string) =>
    request<RoutineRecord>(`/daily-routines/records/${id}/complete`, {
      method: 'PUT',
      body: JSON.stringify({ id, endTime, notes }),
    }),
  cancel: (id: string, reason?: string) =>
    request<RoutineRecord>(`/daily-routines/records/${id}/cancel`, {
      method: 'PUT',
      body: JSON.stringify(reason),
      headers: { 'Content-Type': 'application/json' },
    }),
};

// --- Routine Status (Task #202/#203) ---
export interface RoutineStatusItem {
  classRoutineId: string;
  routineId: string;
  routineName: string;
  routineCategory: string;
  scheduledTime: string;
  durationMinutes: number;
  status: string;
  startTime: string | null;
  endTime: string | null;
  recordId: string | null;
}

export interface RoutineStatus {
  classId: string;
  className: string;
  date: string;
  totalRoutines: number;
  completedRoutines: number;
  inProgressRoutines: number;
  pendingRoutines: number;
  cancelledRoutines: number;
  completionPercentage: number;
  items: RoutineStatusItem[];
}

export const statusApi = {
  get: (classId: string, date?: string) => {
    const params = new URLSearchParams({ classId });
    if (date) params.set('date', date);
    return request<RoutineStatus>(`/daily-routines/status?${params}`);
  },
};
