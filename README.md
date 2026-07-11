# Oficina Mecânica API

API RESTful para gerenciamento de uma oficina mecânica, desenvolvida com .NET 10, PostgreSQL e Docker.

## Fase 2 — Objetivos desta evolução

Na Fase 1 o sistema cobriu o CRUD básico de clientes, veículos, serviços, peças e ordens de
serviço (OS). Nesta Fase 2, a aplicação evoluiu para suportar maior demanda e disponibilidade:

- **Refatoração** para Clean Architecture (camadas `Domain` / `Application` / `Infrastructure` /
  `Presentation`), com testes automatizados cobrindo os fluxos críticos.
- **Novas regras de negócio na OS**: recusa de orçamento pelo cliente, consulta dedicada de
  status, listagem priorizada por status (Em Execução > Aguardando Aprovação > Diagnóstico >
  Recebida, mais antigas primeiro) e exclusão lógica das OS Finalizadas/Entregues dessa listagem.
- **Notificação por e-mail** a cada mudança de status da OS.
- **Conteinerização** via Docker/Docker Compose para desenvolvimento local.
- **Orquestração via Kubernetes** (Deployment, Service, ConfigMap, Secret, HPA) — veja [Deploy em Kubernetes](#deploy-em-kubernetes).
- **Infraestrutura como código** via Terraform, provisionando o cluster EKS e o banco RDS na AWS
  — veja [Provisionamento com Terraform](#provisionamento-com-terraform).
- **Pipeline de CI/CD** via GitHub Actions — veja [CI/CD](#cicd).

---

## Índice

- [Pré-requisitos](#pré-requisitos)
- [Arquitetura](#arquitetura)
- [Modelo de dados](#modelo-de-dados)
- [Fluxo de autenticação](#fluxo-de-autenticação)
- [Fluxo principal — Ordem de Serviço](#fluxo-principal--ordem-de-serviço)
- [Configuração do ambiente](#configuração-do-ambiente)
- [Subindo a aplicação (execução local)](#subindo-a-aplicação-execução-local)
- [Banco de dados e migrations](#banco-de-dados-e-migrations)
- [Deploy em Kubernetes](#deploy-em-kubernetes)
- [Provisionamento com Terraform](#provisionamento-com-terraform)
- [CI/CD](#cicd)
- [Autenticação JWT](#autenticação-jwt)
- [Documentação Swagger](#documentação-swagger)
- [Endpoints disponíveis](#endpoints-disponíveis)
- [Exemplos de requisição](#exemplos-de-requisição)
- [Testes](#testes)
- [Análise de segurança do código](#análise-de-segurança-do-código)
- [Vídeo demonstrativo](#vídeo-demonstrativo)

---

## Pré-requisitos

- [Docker](https://www.docker.com/) e Docker Compose instalados
- Git

---

## Arquitetura

### Arquitetura local (Docker Compose)

```mermaid
graph TD
    Cliente(["👤 Cliente / Professor"])
    Swagger["Swagger UI\n:8080/swagger"]
    API["OficinaApi\n.NET 10\n:8080"]
    DB[("PostgreSQL 16\n:5432")]
    JWT["jwt.io\nGeração do token"]

    Cliente -->|"Acessa"| Swagger
    Cliente -->|"Gera token"| JWT
    JWT -->|"Bearer token"| Swagger
    Swagger -->|"HTTP requests"| API
    API -->|"Leitura / Escrita"| DB

    subgraph Docker Compose
        API
        DB
    end
```

### Arquitetura em nuvem (Kubernetes / AWS)

```mermaid
graph TD
    Dev(["👤 Desenvolvedor"]) -->|"git push"| GH["GitHub"]
    GH -->|"dispara"| CI["GitHub Actions\nbuild → test → docker build/push"]
    CI -->|"docker push"| Hub[("Docker Hub")]
    CI -->|"kubectl apply"| EKS

    subgraph AWS["AWS (provisionado via Terraform — /infra)"]
        subgraph EKS["Cluster EKS"]
            Pod1["Pod\noficina-mecanica-api"]
            Pod2["Pod\noficina-mecanica-api"]
            HPA["HorizontalPodAutoscaler\nmin 1 / max 3\ncpu 80% · mem 75%"]
            Svc["Service\nLoadBalancer"]
            HPA -.->|"escala"| Pod1
            HPA -.->|"escala"| Pod2
            Svc --> Pod1
            Svc --> Pod2
        end
        RDS[("RDS PostgreSQL 16")]
        Pod1 --> RDS
        Pod2 --> RDS
    end

    Usuario(["👤 Usuário / Professor"]) -->|"HTTP"| Svc
```

Fluxo de deploy: push no `main` → CI builda e testa a aplicação → imagem Docker é publicada →
pipeline aplica os manifestos em `/k8s` no cluster EKS (provisionado previamente via Terraform em
`/infra`) → o `Service` do tipo `LoadBalancer` expõe a API publicamente, com o `HorizontalPodAutoscaler`
escalando os pods conforme o consumo de CPU/memória.

---

## Modelo de dados

```mermaid
erDiagram
    Customer {
        uuid Id
        string Name
        string Email
        string Phone
    }
    Vehicle {
        uuid Id
        string Plate
        string Brand
        string Model
        int Year
        uuid CustomerId
    }
    ServiceOrder {
        uuid Id
        datetime CreatedAt
        string Status
        uuid VehicleId
    }
    Service {
        uuid Id
        string Name
        string Description
        decimal Price
    }
    Part {
        uuid Id
        string Name
        int StockQuantity
        decimal Price
    }

    Customer ||--o{ Vehicle : "possui"
    Vehicle ||--o{ ServiceOrder : "gera"
    ServiceOrder }o--o{ Service : "contém"
    ServiceOrder }o--o{ Part : "utiliza"
```

---

## Fluxo de autenticação

```mermaid
sequenceDiagram
    actor Prof as Professor
    participant JwtIo as jwt.io
    participant Swagger as Swagger UI
    participant API as OficinaApi

    Prof->>JwtIo: Informa payload + secret (@Postech$2026)
    JwtIo-->>Prof: Retorna Bearer token

    Prof->>Swagger: Clica em Authorize
    Prof->>Swagger: Cola Bearer {token}
    Swagger-->>Prof: Token salvo na sessão

    Prof->>Swagger: Executa endpoint protegido
    Swagger->>API: GET /api/Customer + Authorization header
    API-->>Swagger: 200 OK + dados
    Swagger-->>Prof: Exibe resposta
```

---

## Fluxo principal — Ordem de Serviço

```mermaid
flowchart TD
    A([Início]) --> B[Criar Cliente\nPOST /api/Customer]
    B --> C[Criar Veículo\nPOST /api/Vehicle]
    C --> D[Criar Serviço\nPOST /api/Service]
    D --> E[Criar Peça e adicionar estoque\nPOST /api/Parts\nPOST /api/Parts/id/add-stock]
    E --> F[Criar Ordem de Serviço\nPOST /api/ServiceOrders]
    F --> G[Estoque debitado automaticamente]
    G --> H[Consultar OS completa\nGET /api/ServiceOrders/id]
    H --> I[Consultar apenas o status\nGET /api/ServiceOrders/id/status]
    I --> J{Cliente aprova\no orçamento?}
    J -->|Sim| K[POST /api/ServiceOrders/id/approve]
    J -->|Não| L[POST /api/ServiceOrders/id/refuse]
    K --> Z([Fim])
    L --> Z
```

---

## Configuração do ambiente

Crie o arquivo `.env` na raiz do projeto com o conteúdo abaixo (credenciais do ambiente de testes):

```env
# Banco de dados
DB_USER=postgres
DB_PASSWORD=@Postech$2026
DB_NAME=oficina_db
DB_CONNECTION_STRING=Host=db;Port=5432;Database=oficina_db;Username=postgres;Password=@Postech$2026

# JWT
JWT_SECRET=@Postech$2026
JWT_ISSUER=oficina-api
JWT_AUDIENCE=oficina-clientes
```

> Esses valores já estão configurados para o ambiente de testes. Não é necessário alterar nada para subir e testar a aplicação.

---

## Subindo a aplicação (execução local)

```bash
# Primeira vez ou após alterações no código
docker compose up --build -d

# Nas próximas vezes (sem alterações de código)
docker compose up -d

# Parar a aplicação
docker compose down
```

Verifique se os containers subiram:

```bash
docker compose ps
```

---

## Banco de dados e migrations

A migration é aplicada automaticamente na primeira vez que a aplicação sobe. Caso precise aplicar manualmente:

```bash
docker compose exec api dotnet ef database update
```

Se precisar recriar o banco do zero:

```bash
docker compose down -v   # remove os volumes
docker compose up -d
```

---

## Deploy em Kubernetes

Os manifestos estão em [`/k8s`](k8s): `configmap.yaml`, `deployment.yaml`, `service.yaml` e
`hpa.yaml` (HorizontalPodAutoscaler, escalando de 1 a 3 réplicas por CPU/memória).

Pré-requisito: o cluster (EKS) precisa já existir — veja [Provisionamento com Terraform](#provisionamento-com-terraform).

```bash
# 1. Aponta o kubectl para o cluster provisionado pelo Terraform
$(terraform -chdir=infra output -raw kubeconfig_command)

# 2. Cria o Secret com as variáveis sensíveis (não versionado no git)
cp k8s/secrets.yaml.example k8s/secrets.yaml
# edite k8s/secrets.yaml com os valores reais (a connection string pode ser obtida via
# `terraform -chdir=infra output -raw db_connection_string`)
kubectl apply -f k8s/secrets.yaml

# 3. Aplica o restante dos manifestos
kubectl apply -f k8s/configmap.yaml
kubectl apply -f k8s/deployment.yaml
kubectl apply -f k8s/service.yaml
kubectl apply -f k8s/hpa.yaml

# 4. Obtém o endereço público exposto pelo Service (LoadBalancer)
kubectl get service oficina-mecanica-api-svc
```

Esses mesmos passos (exceto a criação do Secret, que não é automatizada por segurança) são
executados automaticamente pelo job `kubernetes-deploy` do CI/CD a cada push em `main` — veja [CI/CD](#cicd).

Para acompanhar o autoscaling em ação:

```bash
kubectl get hpa oficina-mecanica-api-hpa --watch
```

---

## Provisionamento com Terraform

A infraestrutura de nuvem (VPC, cluster EKS com node group gerenciado, e RDS PostgreSQL) é
provisionada via Terraform em [`/infra`](infra). O guia completo — pré-requisitos, variáveis,
passo a passo de `apply`/`destroy` e estimativa de custo — está em [`infra/README.md`](infra/README.md).

Resumo rápido:

```bash
cd infra
cp terraform.tfvars.example terraform.tfvars
terraform init
terraform apply
```

> A infraestrutura cobra por hora enquanto estiver de pé. Recomendado: suba antes de gravar o
> vídeo demonstrativo e rode `terraform destroy` logo depois.

---

## CI/CD

O pipeline (`.github/workflows/ci-cd.yml`, GitHub Actions) roda a cada push/PR em `main`/`develop`:

1. **build-and-test** — restaura, builda e executa a suíte de testes automatizados.
2. **docker-build-push** — builda a imagem Docker e publica no Docker Hub.
3. **kubernetes-deploy** *(apenas em push)* — autentica na AWS, aponta o `kubectl` para o cluster
   EKS e aplica os manifestos de `/k8s` com a imagem recém-publicada.

Secrets necessários no GitHub (Settings → Secrets and variables → Actions):
`AWS_ACCESS_KEY_ID`, `AWS_SECRET_ACCESS_KEY`, `DOCKERHUB_USERNAME`, `DOCKERHUB_TOKEN`.

> O provisionamento da infraestrutura (Terraform) é feito manualmente antes do primeiro deploy —
> veja [Provisionamento com Terraform](#provisionamento-com-terraform). O pipeline assume que o
> cluster já existe e apenas aplica os manifestos da aplicação.

---

## Autenticação JWT

A maioria dos endpoints é protegida por JWT. Para testá-los você precisa gerar um token.

### Gerando o token via [jwt.io](https://jwt.io)

1. Acesse [https://jwt.io](https://jwt.io)
2. No painel **Decoded**, preencha:

**Header:**
```json
{
  "alg": "HS256",
  "typ": "JWT"
}
```

**Payload:**
```json
{
  "sub": "qualquer-id",
  "email": "teste@email.com",
  "jti": "qualquer-uuid",
  "iss": "oficina-api",
  "aud": "oficina-clientes",
  "exp": 9999999999
}
```

3. No campo **Verify Signature**, cole a secret abaixo:
```
@Postech$2026
```
4. Copie o token gerado no painel esquerdo

### Usando o token no Swagger

1. Acesse o Swagger: `http://<seu-host>:8080/swagger`
2. Clique no botão **Authorize** (cadeado verde no topo da página)
3. No campo **Value**, digite:
```
Bearer <token_gerado>
```
4. Clique em **Authorize** e depois em **Close**
5. A partir deste momento todos os endpoints protegidos já aceitarão o token

---

## Documentação Swagger

Acesse a documentação interativa da API:

```
http://localhost:8080/swagger
```

### Coleção completa da API

O Swagger expõe a especificação OpenAPI completa em `http://localhost:8080/swagger/v1/swagger.json`
(troque `localhost` pelo endereço do `Service` do Kubernetes quando aplicável). Esse arquivo pode
ser importado diretamente no Postman (**Import → Link**) ou em qualquer outra ferramenta compatível
com OpenAPI/Swagger, servindo como a coleção completa das rotas documentadas na seção
[Endpoints disponíveis](#endpoints-disponíveis).

---

## Endpoints disponíveis

### Customer — Clientes
| Método | Rota | Descrição | Auth |
|--------|------|-----------|------|
| GET | `/api/Customer` | Lista todos os clientes | ✅ |
| GET | `/api/Customer/{id}` | Busca cliente por ID | ✅ |
| POST | `/api/Customer` | Cria novo cliente | ✅ |
| PUT | `/api/Customer/{id}` | Atualiza cliente | ✅ |
| DELETE | `/api/Customer/{id}` | Remove cliente | ✅ |

### Vehicle — Veículos
| Método | Rota | Descrição | Auth |
|--------|------|-----------|------|
| GET | `/api/Vehicle` | Lista todos os veículos | ✅ |
| GET | `/api/Vehicle/{id}` | Busca veículo por ID | ✅ |
| POST | `/api/Vehicle` | Cria novo veículo | ✅ |
| PUT | `/api/Vehicle/{id}` | Atualiza veículo | ✅ |
| DELETE | `/api/Vehicle/{id}` | Remove veículo | ✅ |

### Service — Serviços
| Método | Rota | Descrição | Auth |
|--------|------|-----------|------|
| GET | `/api/Service` | Lista todos os serviços | ✅ |
| GET | `/api/Service/{id}` | Busca serviço por ID | ✅ |
| POST | `/api/Service` | Cria novo serviço | ✅ |
| PUT | `/api/Service/{id}` | Atualiza serviço | ✅ |
| DELETE | `/api/Service/{id}` | Remove serviço | ✅ |

### Parts — Peças / Estoque
| Método | Rota | Descrição | Auth |
|--------|------|-----------|------|
| GET | `/api/Parts` | Lista todas as peças | ✅ |
| GET | `/api/Parts/{id}` | Busca peça por ID | ✅ |
| POST | `/api/Parts` | Cria nova peça | ✅ |
| POST | `/api/Parts/{id}/add-stock` | Adiciona estoque | ✅ |
| POST | `/api/Parts/{id}/remove-stock` | Remove estoque | ✅ |
| DELETE | `/api/Parts/{id}` | Remove peça | ✅ |

### ServiceOrders — Ordens de Serviço
| Método | Rota | Descrição | Auth |
|--------|------|-----------|------|
| GET | `/api/ServiceOrders` | Lista as OS não finalizadas/entregues, ordenadas por status (Em Execução > Aguardando Aprovação > Diagnóstico > Recebida) e mais antigas primeiro | ✅ |
| GET | `/api/ServiceOrders/{id}` | Busca OS completa por ID | ✅ |
| GET | `/api/ServiceOrders/{id}/status` | Consulta apenas o status atual da OS | ✅ |
| POST | `/api/ServiceOrders` | Abre uma nova OS | ✅ |
| POST | `/api/ServiceOrders/{id}/start-analysis` | Move a OS para diagnóstico técnico | ✅ |
| POST | `/api/ServiceOrders/{id}/finish-analysis` | Finaliza o diagnóstico e calcula o orçamento (envia e-mail ao cliente) | ✅ |
| POST | `/api/ServiceOrders/{id}/parts` | Adiciona uma peça à OS | ✅ |
| POST | `/api/ServiceOrders/{id}/services` | Adiciona um serviço à OS | ✅ |
| POST | `/api/ServiceOrders/{id}/approve` | Aprova o orçamento e inicia a execução | ✅ |
| POST | `/api/ServiceOrders/{id}/refuse` | Recusa o orçamento (só a partir de "Aguardando Aprovação") | ✅ |
| POST | `/api/ServiceOrders/{id}/finish-execution` | Finaliza a execução | ✅ |
| POST | `/api/ServiceOrders/{id}/deliver` | Marca a OS como entregue ao cliente | ✅ |
| GET | `/api/ServiceOrders/{id}/pending-stocks` | Lista peças com estoque pendente de confirmação | ✅ |
| GET | `/api/ServiceOrders/average-duration` | Duração média (em dias) das OS finalizadas | ✅ |

> ✅ Requer token JWT

A cada transição de status (Recebida → Diagnóstico → Aguardando Aprovação → Execução → Finalizada
→ Entregue, ou Recusada), o cliente recebe um e-mail automático com a atualização.

---

## Exemplos de requisição

### Criar cliente

```bash
curl -X POST http://localhost:8080/api/Customer \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer <seu_token>" \
  -d '{
    "name": "João Silva",
    "email": "joao@email.com",
    "phone": "11999999999"
  }'
```

### Criar veículo

```bash
curl -X POST http://localhost:8080/api/Vehicle \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer <seu_token>" \
  -d '{
    "plate": "ABC1D23",
    "brand": "Toyota",
    "model": "Corolla",
    "year": 2022,
    "customerId": "<id_do_cliente>"
  }'
```

### Criar ordem de serviço

```bash
curl -X POST http://localhost:8080/api/ServiceOrders \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer <seu_token>" \
  -d '{
    "vehicleId": "<id_do_veiculo>",
    "services": [{ "id": "<id_do_servico>" }],
    "parts": [{ "id": "<id_da_peca>", "quantity": 2 }]
  }'
```

### Consultar apenas o status de uma OS

```bash
curl http://localhost:8080/api/ServiceOrders/<id_da_os>/status \
  -H "Authorization: Bearer <seu_token>"
```

---

## Testes

O projeto conta com uma suíte de mais de 230 testes automatizados (unitários e de integração),
cobrindo os principais fluxos de domínio. A suíte roda automaticamente no job `build-and-test`
do CI/CD a cada push/PR.

### Executar os testes

```bash
dotnet test tests/OficinaApi.Tests/OficinaApi.Tests.csproj
```

### Distribuição

| Categoria | Cobertura |
|-----------|-----------|
| Controllers (Auth, Users, Customers, Vehicles, ServiceOrders) | ✅ Unitários |
| Services (UserService, TokenService, EmailService, PartService, ServiceOrderService) | ✅ Unitários |
| Domain Entities (ServiceOrder, Part, Service, Vehicle, Customer) | ✅ Unitários |
| Value Objects (CPF/CNPJ, Placa) | ✅ Unitários |
| Repositórios | ✅ Unitários |
| Fluxos de integração (Customers, Parts, Services, ServiceOrders, Vehicles) | ✅ Integração |

---

## Análise de segurança do código

O relatório completo de análise estática do código está disponível em:

📄 [`docs/relatorio-scan-codigo.md`](docs/relatorio-scan-codigo.md)

### Resumo dos achados

| Categoria | Risco | Status |
|-----------|-------|--------|
| Injeção de SQL | Nenhum | ✅ OK |
| Autenticação JWT | Nenhum | ✅ OK |
| Autorização de endpoints | Baixo | ✅ OK |
| Segurança do contêiner Docker | Nenhum | ✅ OK |
| Credenciais expostas | Baixo (intencional — contexto acadêmico) | Aceito |
| Validação de entrada nos DTOs | Médio | ⚠️ Recomendação documentada |

---

## Vídeo demonstrativo

📺 `<link do vídeo — YouTube ou Vimeo, público ou não listado, até 15 minutos>`

O vídeo demonstra: deploy da aplicação (Terraform + Kubernetes), execução do pipeline de CI/CD,
consumo das APIs pelo Swagger e a escalabilidade automática via HPA sob carga simulada.
