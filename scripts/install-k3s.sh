#!/usr/bin/env bash
# Instala o k3s (single-node) na instância e valida que o cluster subiu.
# Uso: sudo ./install-k3s.sh
set -euo pipefail

if [ "$(id -u)" -ne 0 ]; then
  echo "Execute como root (sudo ./install-k3s.sh)" >&2
  exit 1
fi

if command -v k3s >/dev/null 2>&1; then
  echo "k3s já está instalado ($(k3s --version | head -n1)). Pulando instalação."
else
  echo "==> Instalando k3s..."
  curl -sfL https://get.k3s.io | sh -
fi

echo "==> Garantindo que o serviço k3s esteja ativo..."
systemctl enable --now k3s

echo "==> Ajustando permissão do kubeconfig..."
chmod 644 /etc/rancher/k3s/k3s.yaml

echo "==> Aguardando o node ficar Ready..."
for i in $(seq 1 30); do
  if kubectl get nodes 2>/dev/null | grep -q " Ready"; then
    break
  fi
  sleep 2
done

echo "==> Nodes:"
kubectl get nodes

echo "==> Pods (todos os namespaces):"
kubectl get pods -A

echo "==> kubeconfig disponível em /etc/rancher/k3s/k3s.yaml"
echo "    Para usar sem sudo: export KUBECONFIG=/etc/rancher/k3s/k3s.yaml"
