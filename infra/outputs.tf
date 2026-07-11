output "kubeconfig_path" {
  description = "Caminho do kubeconfig para usar com kubectl"
  value       = var.kubeconfig_path
}

output "host_ip" {
  description = "IP do host usado para expor o Postgres dentro do cluster"
  value       = data.external.host_ip.result.ip
}

output "postgres_in_cluster_dns" {
  description = "Nome DNS para a aplicacao (rodando nos pods) se conectar ao Postgres do docker-compose"
  value       = "postgres-external.${var.k8s_namespace}.svc.cluster.local"
}

output "generate_secret_command" {
  description = "Gera/atualiza o Secret com as variaveis sensiveis a partir do .env local (nao versionado no git)"
  value       = "${var.repo_root}/scripts/generate-k8s-secret.sh"
}
