# Infraestrutura (Terraform) — cluster local (k3s)

Provisiona, **100% local e sem custo de nuvem**, a infraestrutura exigida pelo Tech Challenge
Fase 2:

- **Cluster Kubernetes**: garante que o k3s (já instalado neste servidor) esteja ativo, via
  `scripts/install-k3s.sh`.
- **Banco de dados**: sobe o Postgres do `docker-compose.yml` e o expõe dentro do cluster através
  de um `Service`/`Endpoints` (`postgres-external`), já que o Postgres roda no `dockerd` (docker
  compose) e o k3s roda seu próprio `containerd` — são runtimes de container separados no mesmo
  host, então a ponte entre os dois é feita apontando o Service para o IP do host na porta 5432
  publicada pelo docker-compose.

Não é criado nenhum recurso na AWS ou em qualquer outro provedor de nuvem — tudo roda dentro
deste servidor.

## Pré-requisitos

- Terraform >= 1.5
- k3s (instalado via `../scripts/install-k3s.sh`, chamado automaticamente pelo `apply`)
- `kubectl` e `docker compose` disponíveis no host

## Como aplicar

```bash
cd infra
cp terraform.tfvars.example terraform.tfvars   # ajuste se necessario
terraform init
terraform apply
```

Isso deixa o cluster k3s ativo, o Postgres do docker-compose no ar, e o Service
`postgres-external` criado no namespace `default` apontando para ele.

Em seguida, gere o Secret com as variáveis sensíveis (lidas do `.env` local, nunca versionadas):

```bash
../scripts/generate-k8s-secret.sh
```

E aplique os manifestos da aplicação (isso também é feito automaticamente pelo job
`kubernetes-deploy` do CI/CD a cada push, rodando no self-hosted runner deste servidor):

```bash
kubectl apply -f ../k8s/configmap.yaml
kubectl apply -f ../k8s/deployment.yaml
kubectl apply -f ../k8s/service.yaml
kubectl apply -f ../k8s/hpa.yaml
```

## Por que não EKS/RDS?

Este servidor é um laboratório de pós-graduação e não deve gerar custo de AWS. O enunciado do
desafio permite explicitamente cluster Kubernetes "local ou cloud" — optamos por local por esse
motivo. Toda a infraestrutura (cluster, banco, deploy) roda neste único servidor, sem depender de
nenhuma conta ou recurso pago na nuvem.

## Variáveis

| Variável | Default | Descrição |
|---|---|---|
| `kubeconfig_path` | `/etc/rancher/k3s/k3s.yaml` | Kubeconfig do k3s local |
| `k8s_namespace` | `default` | Namespace da aplicação e do Service do banco |
| `repo_root` | `..` | Caminho da raiz do repositório |
| `db_port` | `5432` | Porta do Postgres publicada pelo docker-compose |

Veja todas em [variables.tf](variables.tf).
