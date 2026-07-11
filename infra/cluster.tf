# Provisiona o cluster Kubernetes local (k3s). Reaproveita scripts/install-k3s.sh,
# que instala o k3s se necessario, ajusta a permissao do kubeconfig e aguarda o node
# ficar Ready. Roda a cada apply para garantir que o cluster esteja de pe (idempotente).
resource "null_resource" "ensure_k3s_running" {
  triggers = {
    always_run = timestamp()
  }

  provisioner "local-exec" {
    working_dir = var.repo_root
    command     = "sudo ./scripts/install-k3s.sh"
  }
}
