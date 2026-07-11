variable "kubeconfig_path" {
  description = "Caminho do kubeconfig do k3s local"
  type        = string
  default     = "/etc/rancher/k3s/k3s.yaml"
}

variable "k8s_namespace" {
  description = "Namespace onde a aplicacao e o Service do banco sao criados"
  type        = string
  default     = "default"
}

variable "repo_root" {
  description = "Caminho da raiz do repositorio (para rodar docker compose e o script do k3s)"
  type        = string
  default     = ".."
}

variable "db_port" {
  description = "Porta do PostgreSQL publicada pelo docker-compose no host"
  type        = number
  default     = 5432
}
