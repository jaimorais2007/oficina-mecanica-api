variable "aws_region" {
  description = "Regiao AWS onde os recursos serao provisionados"
  type        = string
  default     = "us-east-1"
}

variable "project_name" {
  description = "Nome do projeto, usado como prefixo/tag dos recursos"
  type        = string
  default     = "oficina-mecanica"
}

variable "environment" {
  description = "Nome do ambiente"
  type        = string
  default     = "dev"
}

variable "cluster_name" {
  description = "Nome do cluster EKS (deve bater com EKS_CLUSTER_NAME em .github/workflows/ci-cd.yml)"
  type        = string
  default     = "oficina-mecanica-cluster"
}

variable "cluster_version" {
  description = "Versao do Kubernetes no EKS"
  type        = string
  default     = "1.31"
}

variable "vpc_cidr" {
  description = "CIDR block da VPC"
  type        = string
  default     = "10.0.0.0/16"
}

variable "node_instance_type" {
  description = "Tipo de instancia EC2 usada no node group gerenciado do EKS"
  type        = string
  default     = "t3.medium"
}

variable "node_desired_size" {
  description = "Quantidade inicial de nodes no EKS"
  type        = number
  default     = 2
}

variable "node_min_size" {
  description = "Quantidade minima de nodes no EKS"
  type        = number
  default     = 1
}

variable "node_max_size" {
  description = "Quantidade maxima de nodes no EKS"
  type        = number
  default     = 3
}

variable "db_name" {
  description = "Nome do banco de dados Postgres"
  type        = string
  default     = "oficina_db"
}

variable "db_username" {
  description = "Usuario master do RDS"
  type        = string
  default     = "oficina_admin"
}

variable "db_instance_class" {
  description = "Classe da instancia RDS"
  type        = string
  default     = "db.t3.micro"
}

variable "db_allocated_storage" {
  description = "Armazenamento alocado (GB) para o RDS"
  type        = number
  default     = 20
}
