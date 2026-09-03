#!/usr/bin/env bash
# Gera/atualiza o Secret oficina-mecanica-api-secret no cluster a partir do .env local.
# Os valores nunca sao versionados no git nem escritos em disco como YAML.
# Uso: ./scripts/generate-k8s-secret.sh
set -euo pipefail

REPO_ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
ENV_FILE="$REPO_ROOT/.env"

if [ ! -f "$ENV_FILE" ]; then
  echo "==> Arquivo .env não encontrado em $ENV_FILE. Pulando a criação do Secret Kubernetes (assumindo que já existe no cluster)."
  exit 0
fi

# Le o .env manualmente (sem "source") para nao expandir "$" presentes nos valores
# (ex: senhas como "@Postech$2026" seriam corrompidas por uma expansao de shell).
declare -A envmap
while IFS='=' read -r key value; do
  [[ -z "$key" || "$key" == \#* ]] && continue
  envmap["$key"]="$value"
done < "$ENV_FILE"

DB_NAME="${envmap[DB_NAME]:-}"
DB_USER="${envmap[DB_USER]:-}"
DB_PASSWORD="${envmap[DB_PASSWORD]:-}"
DB_CONNECTION_STRING="${envmap[DB_CONNECTION_STRING]:-}"
JWT_SECRET="${envmap[JWT_SECRET]:-}"
EMAIL_PASSWORD="${envmap[EMAIL_PASSWORD]:-}"
OTEL_EXPORTER_OTLP_HEADERS="${envmap[OTEL_EXPORTER_OTLP_HEADERS]:-}"

if [ -n "$DB_CONNECTION_STRING" ]; then
  CONNECTION_STRING="$DB_CONNECTION_STRING"
else
  CONNECTION_STRING="Host=postgres-external;Port=5432;Database=${DB_NAME};Username=${DB_USER};Password=${DB_PASSWORD}"
fi

kubectl create secret generic oficina-mecanica-api-secret \
  --from-literal=ConnectionStrings__DefaultConnection="$CONNECTION_STRING" \
  --from-literal=Jwt__Secret="$JWT_SECRET" \
  --from-literal=EmailSettings__Password="$EMAIL_PASSWORD" \
  --from-literal=OTEL_EXPORTER_OTLP_HEADERS="$OTEL_EXPORTER_OTLP_HEADERS" \
  --dry-run=client -o yaml | kubectl apply -f -

echo "==> Secret oficina-mecanica-api-secret aplicado."

