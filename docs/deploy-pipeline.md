# 🚀 Deploy Pipeline — EduCore Platform

## Fluxo

```
Push no GitHub main
       │
       ▼
Dokploy detecta o push
       │
       ▼
Homologação (build + test)
  ├── dotnet build
  ├── dotnet test (34 SQLite)
  ├── dotnet publish
  └── ✅ Se passou → API dispara deploy em produção
       │
       ▼
Produção sobe com a nova versão
```

## Ambientes

| Ambiente | URL | Dockerfile | App ID |
|----------|-----|-----------|--------|
| 🟡 **Homologação** | https://hml-platform.devgiglio.uk | `Dockerfile.hml` | `9hMNEOpTN5eh-SZ2oTXJR` |
| 🟢 **Produção** | https://platform.devgiglio.uk | `Dockerfile` | `EkPqlYG11SnA8Kw7q0wMH` |

## Dockerfile.hml — O que ele faz

1. **Builda** o projeto completo
2. **Roda os 34 testes SQLite** — se falharem, o build para
3. **Publica** o artefato
4. **Dispara deploy na produção** via API do Dokploy
5. Sobe o app em homologação

Se os testes falharem, o container nem sobe — o deploy é abortado.

## Configuração Inicial (uma vez)

### 1. No Dokploy Dashboard

**App de Homologação:**
1. Acessar https://dokploy.devgiglio.com
2. Projeto: EduCore Platform → Ambiente: homologacao
3. App: educore-platform-hml
4. Configurar:
   - **Build type**: `Dockerfile`
   - **Dockerfile path**: `Dockerfile.hml`
   - **Source**: Git → `https://github.com/gipierogiglio-prog/educore-platform.git`
   - **Branch**: `main`

**App de Produção:**
1. Projeto: EduCore Platform → Ambiente: production
2. App: educore-platform
3. Configurar:
   - **Build type**: `Dockerfile`
   - **Dockerfile path**: `Dockerfile`
   - **Source**: Git → `https://github.com/gipierogiglio-prog/educore-platform.git`
   - **Branch**: `main`

### 2. Variável de Ambiente

No app de homologação, adicionar:
```
DOKPLOY_API_KEY = NeBPnyBQkAxJqMiPJtBMONwqqvWqvPPwFhEyQcZIEhPfpZNnFNasUoGlDIhsXKfd
```

### 3. Webhook (auto-deploy ao push)

Em ambos os apps, configurar o webhook do GitHub:
1. GitHub repo → Settings → Webhooks → Add webhook
2. Payload URL: `https://dokploy.devgiglio.com/api/deploy?applicationId=APP_ID`
3. Content type: `application/json`
4. Secret: (deixar vazio)
5. Events: `Push`
6. Adicionar header: `x-api-key: NeBPnyBQkAxJqMiPJtBMONwqqvWqvPPwFhEyQcZIEhPfpZNnFNasUoGlDIhsXKfd`

## Testes Localmente

```bash
# Apenas SQLite (34 testes, ~2s)
docker compose -f tests/docker-compose.yml up tests-sqlite

# Completo (SQLite + PostgreSQL, 42 testes)
docker compose -f tests/docker-compose.yml up --abort-on-container-exit

# Limpar
docker compose -f tests/docker-compose.yml down
```

## Arquitetura dos Testes

```
tests/Financial.IntegrationTests/
├── TestBase.cs                  🟦 SQLite (padrão) — 34 testes
├── FinancialPlanTests.cs        10 testes (CRUD, descontos, validações)
├── ExpenseTests.cs              10 testes (CRUD, pay/cancel, filtros)
├── MonthlyChargeTests.cs        8 testes (CRUD, status, AddRange)
├── PaymentTests.cs              6 testes (registro, parciais, fluxo)
├── PostgresTestBase.cs          🟩 PostgreSQL (Testcontainers) — 8 testes
└── PostgresSpecificTests.cs     8 testes (precisão, case, concorrência)
```
