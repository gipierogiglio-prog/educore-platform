#!/bin/bash
set -euo pipefail

# ============================================================
# 🚀 EduCore Platform — Deploy Pipeline
# 
# Pipeline completo: testa → se passar → deploy produção
# Uso no Dokploy: configurar como comando de start
# ============================================================

DOKPLOY_URL="https://dokploy.devgiglio.com"
DOKPLOY_KEY="${DOKPLOY_API_KEY:-}"
PROD_APP_ID="EkPqlYG11SnA8Kw7q0wMH"
HML_APP_ID="9hMNEOpTN5eh-SZ2oTXJR"

RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m'

log() { echo -e "${YELLOW}[$(date '+%H:%M:%S')]${NC} $1"; }
success() { echo -e "${GREEN}✅${NC} $1"; }
fail() { echo -e "${RED}❌${NC} $1"; }

# ──────────────────────────────────────────────────
# 1. Build da aplicação
# ──────────────────────────────────────────────────
log "🔨 Buildando aplicação..."
dotnet restore
dotnet build --configuration Release --no-restore
success "Build concluído"

# ──────────────────────────────────────────────────
# 2. Testes SQLite (rápidos)
# ──────────────────────────────────────────────────
log "🏃 Rodando testes SQLite (34 testes)..."
if dotnet test tests/Financial.IntegrationTests/EduCore.Financial.IntegrationTests.csproj \
    --configuration Release \
    --no-build \
    --filter "FullyQualifiedName!~Postgres" \
    --logger "console;verbosity=minimal" 2>&1 | tail -5; then
    success "Testes SQLite passaram!"
else
    fail "Testes SQLite falharam!"
    exit 1
fi

# ──────────────────────────────────────────────────
# 3. Testes PostgreSQL (fidelidade)
# ──────────────────────────────────────────────────
if [ "${SKIP_PG_TESTS:-false}" != "true" ]; then
    log "🐘 Rodando testes PostgreSQL (8 testes)..."
    if dotnet test tests/Financial.IntegrationTests/EduCore.Financial.IntegrationTests.csproj \
        --configuration Release \
        --no-build \
        --filter "Database=PostgreSQL" \
        --logger "console;verbosity=minimal" 2>&1 | tail -5; then
        success "Testes PostgreSQL passaram!"
    else
        fail "Testes PostgreSQL falharam!"
        exit 1
    fi
else
    log "⏭️  PostgreSQL tests pulados (SKIP_PG_TESTS=true)"
fi

# ──────────────────────────────────────────────────
# 4. Publicar
# ──────────────────────────────────────────────────
log "📦 Publicando..."
dotnet publish src/api/Gateway/Educore.Api.csproj \
    --configuration Release \
    --no-build \
    -o /out
success "Publicação concluída"

# ──────────────────────────────────────────────────
# 5. Deploy produção via API Dokploy
# ──────────────────────────────────────────────────
if [ -n "$DOKPLOY_KEY" ]; then
    log "🚀 Disparando deploy em produção..."
    RESP=$(curl -s -w "\n%{http_code}" "$DOKPLOY_URL/api/application.deploy" \
        -H "x-api-key: $DOKPLOY_KEY" \
        -H "Content-Type: application/json" \
        -X POST -d "{\"applicationId\":\"$PROD_APP_ID\"}")
    HTTP_CODE=$(echo "$RESP" | tail -1)
    
    if [ "$HTTP_CODE" = "200" ]; then
        success "Deploy em produção disparado! ✅"
        log "📡 https://platform.devgiglio.uk"
    else
        fail "Falha ao disparar deploy (HTTP $HTTP_CODE)"
        exit 1
    fi
else
    log "⏭️  Deploy automático pulado (DOKPLOY_API_KEY não configurada)"
fi

echo ""
success "🎉 Pipeline concluído com sucesso!"
