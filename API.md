# 🏫 EduCore Platform - API Documentation

**Base URL:** `https://platform.devgiglio.uk` (em breve)
**Auth:** JWT Bearer Token (via `/api/auth/login`)

---

## 🔐 Autenticação

### POST /api/auth/login
Login do usuário.
```json
{ "email": "admin@escola.com", "password": "123456" }
// Response:
{ "token": "eyJ...", "name": "Administrador", "email": "admin@escola.com", "role": "org_admin", "organizationId": "guid" }
```

### POST /api/auth/register
Criar novo usuário (associado a uma organização).
```json
{ "name": "João", "email": "joao@teste.com", "password": "123456", "role": "teacher", "organizationId": "guid(opcional)" }
```

### GET /api/auth/me
Dados do usuário logado. (Header: `Authorization: Bearer {token}`)

---

## 🏢 Organização

### POST /api/organization/sync-status
Atualizar status da org (Admin Panel → ERP).
```json
{ "organizationId": "guid", "status": "inactive" }
// Desativa todos os usuários ativos (marca como AutoDeactivated)
// Reativar reativa apenas quem foi desativado automaticamente
```

---

## 📋 Secretaria

### GET /api/students
Lista alunos da organização. (Filtrado por org automaticamente)

### POST /api/students
Criar aluno (cria User + Student).
```json
{ "name": "João", "email": "joao@teste.com", "password": "123456", "classId": "guid(opcional)" }
```

### PATCH /api/students/{id}/class
Atualizar turma do aluno.
```json
{ "classId": "guid" }
```

### GET /api/teachers
Lista professores.

### POST /api/teachers
Criar professor.
```json
{ "name": "Maria", "email": "maria@teste.com", "password": "123456", "specialization": "Matemática" }
```

---

## 📚 Acadêmico

### GET /api/academic/classes
Lista turmas.

### POST /api/academic/classes
Criar turma.
```json
{ "name": "3º Ano A", "shift": "morning", "year": 2026, "room": "Sala 101" }
```

### GET /api/academic/subjects
Lista disciplinas.

### POST /api/academic/subjects
Criar disciplina.
```json
{ "name": "Matemática", "code": "MAT101", "workload": 80 }
```

### POST /api/academic/grades/batch
Lançar notas em lote.
```json
{ "subjectId": "guid", "bimester": 1, "year": 2026, "grades": [{ "s": "studentId", "v": 8.5, "r": null }] }
```

### POST /api/academic/attendance/batch
Registrar frequência em lote.
```json
{ "classId": "guid", "subjectId": "guid", "date": "2026-05-15", "items": [{ "s": "studentId", "p": true, "j": null }] }
```

---

## 🧮 Grading Engine (Cálculo de Notas)

### GET /api/grading/rules
Lista regras de cálculo da organização.

### POST /api/grading/rules
Criar regra de cálculo personalizada.
```json
{
  "name": "Média Ponderada",
  "description": "P1 peso 4, P2 peso 6",
  "components": [
    { "name": "Prova 1", "type": "exam", "weight": 4, "maxValue": 10, "hasRecovery": true },
    { "name": "Prova 2", "type": "exam", "weight": 6, "maxValue": 10, "hasRecovery": false }
  ]
}
```
**Types disponíveis:** `exam`, `homework`, `project`, `participation`, `recovery`

### PUT /api/grading/rules/{id}
Atualizar regra (remove componentes antigos, adiciona novos).

### DELETE /api/grading/rules/{id}
Remover regra.

### POST /api/grading/entries
Lançar notas por componente.
```json
{ "componentId": "guid", "bimester": 1, "schoolYear": 2026, "entries": [{ "studentId": "guid", "value": 8.5, "recoveryValue": null }] }
```

### POST /api/grading/calculate
Calcular média final de alunos usando uma regra.
```json
{
  "ruleId": "guid",
  "studentIds": ["guid1", "guid2"],
  "bimester": 1,
  "schoolYear": 2026,
  "passingGrade": 6
}
// Response: [{ "studentId": "guid", "finalValue": 8.2, "status": "approved" }]
```

### GET /api/grading/students/{studentId}/grades
Histórico de notas de um aluno.

---

## 👥 Usuários

### GET /api/users
Lista usuários da organização (com grupos).
**Apenas Admin da escola.** Retorna:
```json
[{ "id": "guid", "name": "", "email": "", "role": "", "active": true, "groups": ["Secretário"] }]
```

### POST /api/users
Criar usuário na organização.
```json
{ "name": "João", "email": "joao@teste.com", "password": "123456", "role": "teacher", "phone": "" }
```
**Roles válidas:** `teacher`, `coordinator`, `student`, `guardian`

### PATCH /api/users/{id}/toggle-status
Ativar/desativar usuário.

---

## 🔐 Permissões

### GET /api/permissions
Lista TODAS as permissões disponíveis (33 no total).
```json
[{ "id": "guid", "resource": "students", "action": "view", "name": "Visualizar Alunos" }]
```

### GET /api/permissions/groups
Lista grupos de permissão da organização.

### POST /api/permissions/groups
Criar grupo.
```json
{ "name": "Secretários", "description": "Acesso administrativo", "permissionIds": ["guid1", "guid2"] }
```

### PUT /api/permissions/groups/{id}
Atualizar grupo (substitui permissões).

### DELETE /api/permissions/groups/{id}
Remover grupo.

### POST /api/permissions/users/{userId}/groups
Atribuir usuário a um grupo.
```json
{ "groupId": "guid" }
```

### POST /api/permissions/users/{userId}/direct
Permissão direta (sobrescreve grupo).
```json
{ "permissionId": "guid", "granted": true }
```

---

## 📊 Dashboard

### GET /api/dashboard
Indicadores da organização.
```json
{
  "totalStudents": 150, "totalTeachers": 12, "totalClasses": 8, "totalUsers": 20,
  "recentStudents": [{ "id": "guid", "name": "", "enrollment": "STU20260001", "enrollmentDate": "..." }],
  "studentsByClass": [{ "name": "3º Ano A", "count": 35 }]
}
```

---

## 📋 Regras de Frontend (para IA)

### Telas necessárias (MVP):

1. **Login** → form email+senha, salva token no localStorage
2. **Dashboard** → cards com stats, gráficos
3. **Alunos** → tabela + formulário de cadastro
4. **Professores** → tabela + formulário
5. **Turmas** → CRUD
6. **Disciplinas** → CRUD
7. **Matrículas** → vincular aluno a turma
8. **Notas** → lançamento por turma/bimestre
9. **Frequência** → chamada por turma
10. **Regras de Notas** → criar/editar regras de cálculo
11. **Usuários** → admin da escola gerencia
12. **Permissões** → grupos + permissões diretas

### Layout:
- Sidebar com menu lateral
- Header com nome do usuário e botão sair
- Modo escuro por padrão
- Responsivo (mobile-first)

### Regras de negócio:
- Usuário sem organização → não pode criar/editr nada
- Super Admin e Admin da escola → acesso total
- Demais users → controlado por [RequirePermission] nos endpoints
- Uma escola NUNCA vê dados da outra
