#!/usr/bin/env bash
set -euo pipefail
export KUBECONFIG=/etc/rancher/k3s/k3s.yaml
APP_DIR="$HOME/oficina-mecanica-api"

checkall() {
  code=$(curl -s -o /dev/null -w "%{http_code}" http://localhost:8080/swagger/index.html || true)
  echo "$code"
  kubectl get pods -l app=oficina-mecanica-api
  kubectl get hpa
}

parte1() {
  cd "$APP_DIR/infra"
  terraform apply -auto-approve
  cd "$APP_DIR"
  kubectl apply -f k8s/configmap.yaml -f k8s/deployment.yaml -f k8s/service.yaml -f k8s/hpa.yaml

  for i in $(seq 1 30); do
    ready=$(kubectl get pods -l app=oficina-mecanica-api --no-headers 2>/dev/null | grep -c "Running" || true)
    total=$(kubectl get pods -l app=oficina-mecanica-api --no-headers 2>/dev/null | wc -l)
    echo "$ready/$total"
    if [ "$ready" -gt 0 ] && [ "$ready" -eq "$total" ]; then
      kubectl get pods -l app=oficina-mecanica-api
      break
    fi
    sleep 2
  done
}

token() {
  python3 - <<'EOF'
import json, base64, hmac, hashlib, time, uuid, os
def b64url(d): return base64.urlsafe_b64encode(d).rstrip(b'=').decode()
secret = None
env_path = os.path.expanduser("~/oficina-mecanica-api/.env")
for line in open(env_path):
    if line.startswith("JWT_SECRET="):
        secret = line.strip().split("=", 1)[1]
if not secret:
    raise SystemExit("JWT_SECRET não encontrado no .env")
h = b64url(json.dumps({"alg": "HS256", "typ": "JWT"}).encode())
p = b64url(json.dumps({
    "sub": str(uuid.uuid4()), "email": "demo@video.com", "jti": str(uuid.uuid4()),
    "iss": "oficina-api", "aud": "oficina-clientes", "exp": int(time.time()) + 3600
}).encode())
sig = hmac.new(secret.encode(), f"{h}.{p}".encode(), hashlib.sha256).digest()
token = f"{h}.{p}.{b64url(sig)}"
print(token)
with open("/tmp/demo_token.txt", "w") as f:
    f.write(token)
EOF
}

carga() {
  if [ ! -f /tmp/demo_token.txt ]; then
    exit 1
  fi
  TOKEN=$(cat /tmp/demo_token.txt)
  n=0
  while true; do
    for i in $(seq 1 50); do
      curl -s -o /dev/null http://localhost:30697/api/ServiceOrders -H "Authorization: Bearer $TOKEN" &
    done
    n=$((n+50))
    echo "Requisicoes enviadas: $n"
    sleep 0.2
  done
}

case "${1:-}" in
  checkall) checkall ;;
  parte1) parte1 ;;
  token) token ;;
  carga) carga ;;
  *)
    echo "Uso: $0 [checkall|parte1|token|carga]"
    ;;
esac
