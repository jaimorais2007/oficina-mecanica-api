output "cluster_name" {
  value = module.eks.cluster_name
}

output "cluster_endpoint" {
  value = module.eks.cluster_endpoint
}

output "kubeconfig_command" {
  description = "Rode este comando para configurar o kubectl apos o apply"
  value       = "aws eks update-kubeconfig --region ${var.aws_region} --name ${module.eks.cluster_name}"
}

output "rds_endpoint" {
  value = aws_db_instance.this.address
}

output "rds_port" {
  value = aws_db_instance.this.port
}

output "db_name" {
  value = var.db_name
}

output "db_username" {
  value = var.db_username
}

output "db_password" {
  value     = random_password.db.result
  sensitive = true
}

output "db_connection_string" {
  description = "Connection string no formato usado pelo Npgsql (appsettings)"
  value       = "Host=${aws_db_instance.this.address};Port=${aws_db_instance.this.port};Database=${var.db_name};Username=${var.db_username};Password=${random_password.db.result}"
  sensitive   = true
}

output "create_k8s_secret_command" {
  description = "Cria o Secret com as variaveis sensiveis no cluster (nao versionado no git)"
  value       = "kubectl create secret generic oficina-mecanica-api-secret --from-literal=ConnectionStrings__DefaultConnection=\"Host=${aws_db_instance.this.address};Port=${aws_db_instance.this.port};Database=${var.db_name};Username=${var.db_username};Password=${random_password.db.result}\" --from-literal=Jwt__Secret=\"<definir>\" --from-literal=EmailSettings__Password=\"<definir>\""
  sensitive   = true
}
