# Provisiona o banco de dados: sobe o Postgres do docker-compose (ja exigido como
# entregavel de desenvolvimento local) e o expoe dentro do cluster k3s atraves de um
# Service sem selector + Endpoints manuais apontando para o IP do host, ja que o
# container do Postgres roda no dockerd (docker-compose) e nao no containerd do k3s.
data "external" "host_ip" {
  program = ["bash", "-c", "echo '{\"ip\": \"'\"$(hostname -I | awk '{print $1}')\"'\"}'"]
}

resource "null_resource" "ensure_database_running" {
  triggers = {
    always_run = timestamp()
  }

  provisioner "local-exec" {
    working_dir = var.repo_root
    command     = "docker compose up -d db"
  }
}

resource "kubernetes_service" "postgres_external" {
  metadata {
    name      = "postgres-external"
    namespace = var.k8s_namespace
  }

  spec {
    type = "ClusterIP"

    port {
      port        = var.db_port
      target_port = var.db_port
    }
  }

  depends_on = [null_resource.ensure_k3s_running]
}

resource "kubernetes_endpoints" "postgres_external" {
  metadata {
    name      = kubernetes_service.postgres_external.metadata[0].name
    namespace = var.k8s_namespace
  }

  subset {
    address {
      ip = data.external.host_ip.result.ip
    }

    port {
      port = var.db_port
    }
  }

  depends_on = [null_resource.ensure_database_running]
}
