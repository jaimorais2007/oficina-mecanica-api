# Infraestrutura (Terraform)

Provisiona a infraestrutura em nuvem exigida pelo Tech Challenge Fase 2:

- **VPC** com subnets publicas e privadas em 2 AZs (modulo `terraform-aws-modules/vpc`).
- **Cluster EKS** com um node group gerenciado (modulo `terraform-aws-modules/eks`), com o mesmo
  nome (`oficina-mecanica-cluster`) esperado pelo workflow `.github/workflows/ci-cd.yml`.
- **RDS PostgreSQL** (motor `postgres`, versao 16, igual ao `postgres:16-alpine` usado no
  `docker-compose.yml`), acessivel somente a partir dos nodes do EKS.

## Pre-requisitos

- Terraform >= 1.5
- AWS CLI configurado (`aws configure`) com uma conta com permissao para criar VPC, EKS, RDS, IAM.
- `kubectl` instalado localmente para aplicar os manifestos de `../k8s` depois do cluster subir.

## Como aplicar

```bash
cd infra
cp terraform.tfvars.example terraform.tfvars   # ajuste se necessario
terraform init
terraform plan
terraform apply
```

Ao final do `apply`, configure o `kubectl`:

```bash
$(terraform output -raw kubeconfig_command)
```

E crie o Secret com as variaveis sensiveis (nao versionadas no git, conforme comentado em
`.github/workflows/ci-cd.yml`):

```bash
terraform output -raw create_k8s_secret_command
# edite os campos "<definir>" (Jwt__Secret e EmailSettings__Password) e rode o comando
```

Depois disso, aplique os manifestos (o pipeline de CI/CD faz isso automaticamente a cada push,
mas tambem pode ser feito manualmente):

```bash
kubectl apply -f ../k8s/configmap.yaml
kubectl apply -f ../k8s/deployment.yaml
kubectl apply -f ../k8s/service.yaml
kubectl apply -f ../k8s/hpa.yaml
```

## Custo / quando desligar

Este ambiente **cobra por hora enquanto estiver de pe** (cluster EKS ~US$0,10/h, NAT Gateway
~US$0,045/h + trafego, RDS `db.t3.micro`). Para a entrega academica, o fluxo recomendado e:

1. `terraform apply` antes de gravar o video demonstrativo.
2. Gravar o deploy, o CI/CD e a escalabilidade (HPA).
3. `terraform destroy` logo depois para nao deixar custo residual rodando.

## Variaveis principais

| Variavel | Default | Descricao |
|---|---|---|
| `aws_region` | `us-east-1` | Regiao AWS |
| `cluster_name` | `oficina-mecanica-cluster` | Precisa bater com `EKS_CLUSTER_NAME` no CI/CD |
| `node_instance_type` | `t3.medium` | Tipo de instancia dos nodes do EKS |
| `node_desired_size` / `min` / `max` | `2` / `1` / `3` | Tamanho do node group (HPA escala os pods; o node group so precisa caber ate `hpa.yaml: maxReplicas: 3`) |
| `db_instance_class` | `db.t3.micro` | Classe do RDS |

Veja todas em [variables.tf](variables.tf).

## Secrets do GitHub Actions necessarios

Para o job `kubernetes-deploy` do workflow funcionar, configure em
Settings > Secrets and variables > Actions:

- `AWS_ACCESS_KEY_ID` / `AWS_SECRET_ACCESS_KEY` (usuario com permissao de `eks:DescribeCluster` e
  acesso ao cluster via `aws-auth`/access entries).
- `DOCKERHUB_USERNAME` / `DOCKERHUB_TOKEN`.
