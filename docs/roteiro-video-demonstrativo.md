# Roteiro do vídeo demonstrativo — Tech Challenge Fase 2

Requisito (do PDF): vídeo de até 15 minutos, publicado no YouTube ou Vimeo (público ou não
listado), demonstrando:
- Deploy da aplicação
- Execução do CI/CD
- Consumo das APIs
- Escalabilidade automática

---

## 0. Preparação (antes de apertar "gravar")

**Onde roda o quê:** o servidor (`ip-172-31-43-237`) é headless — sem GUI, sem navegador, sem
gravador de tela. Os comandos de provisionamento (terraform, kubectl, git push) rodam **no
servidor via SSH**, mas a gravação de tela e os navegadores ficam na **sua máquina Ubuntu local**.

1. Na sua máquina local, abra um terminal e conecte via SSH neste servidor (mesma sessão que já
   usa no VSCode Remote-SSH serve). Se o VSCode não estiver com port-forward automático da 8080
   ativo, force manualmente:
   ```bash
   ssh -L 8080:localhost:8080 ubuntu@<ip-do-servidor>
   ```
2. Abra e organize as janelas **na máquina local**:
   - Aba do navegador: **GitHub → Actions** do repositório
   - Aba do navegador: **Swagger** (`http://localhost:8080/swagger` — via túnel SSH acima)
   - Terminal com a sessão SSH aberta, fonte grande (`Ctrl` `+` várias vezes — quem assistir
     precisa conseguir ler)
3. Rode este check antes de gravar (sem gravar ainda), **dentro da sessão SSH**:
   ```bash
   curl -s -o /dev/null -w "%{http_code}\n" http://localhost:8080/swagger/index.html
   export KUBECONFIG=/etc/rancher/k3s/k3s.yaml
   kubectl get pods -l app=oficina-mecanica-api
   kubectl get hpa
   ```
   Esperado: `200`, pods `Running`, HPA com `REPLICAS` em `2` (esse é o piso real de operação
   deste ambiente — ver nota no final da Parte 4).
4. Escolha a gravação de tela **na máquina local** (Ubuntu):
   - Mais simples: `Ctrl+Shift+Alt+R` (GNOME) — grava a tela toda, sem instalar nada. Aperte de
     novo para parar. Salva em `~/Vídeos`.
   - Mais controle: `sudo apt install obs-studio`.

---

## Parte 1 — Deploy da aplicação (~3 min)

```bash
cd ~/oficina-mecanica-api/infra
terraform apply -auto-approve
cd ..
export KUBECONFIG=/etc/rancher/k3s/k3s.yaml
kubectl apply -f k8s/configmap.yaml -f k8s/deployment.yaml -f k8s/service.yaml -f k8s/hpa.yaml
kubectl get pods -w
```

Narre: "cluster Kubernetes local (k3s), provisionado via Terraform — sem nenhum custo de nuvem,
já que o ambiente é um servidor de laboratório acadêmico". Mostre os pods `Running`. `Ctrl+C`
para sair do `-w`.

---

## Parte 2 — Execução do CI/CD (~3 min)

```bash
cd ~/oficina-mecanica-api
git add README.md && git commit -m "docs: video demo trigger" && git push
```

Troque para a aba **Actions** do GitHub, atualize a página e mostre os 3 jobs rodando e
ficando verdes, um por um:
1. `build-and-test` (nuvem)
2. `docker-build-push` (nuvem)
3. `kubernetes-deploy` (self-hosted runner — **neste mesmo servidor**, já que o cluster é local
   e não tem IP público)

Narre o que cada job faz enquanto espera.

---

## Parte 3 — Consumo das APIs (~4 min)

1. Gere um token válido (lê a secret direto do seu `.env`, não precisa digitar nada manualmente):
   ```bash
   python3 - <<'EOF'
   import json, base64, hmac, hashlib, time, uuid, os
   def b64url(d): return base64.urlsafe_b64encode(d).rstrip(b'=').decode()
   secret = None
   for line in open(os.path.expanduser("~/oficina-mecanica-api/.env")):
       if line.startswith("JWT_SECRET="):
           secret = line.strip().split("=", 1)[1]
   h = b64url(json.dumps({"alg": "HS256", "typ": "JWT"}).encode())
   p = b64url(json.dumps({
       "sub": str(uuid.uuid4()), "email": "demo@video.com", "jti": str(uuid.uuid4()),
       "iss": "oficina-api", "aud": "oficina-clientes", "exp": int(time.time()) + 3600
   }).encode())
   sig = hmac.new(secret.encode(), f"{h}.{p}".encode(), hashlib.sha256).digest()
   print(f"{h}.{p}.{b64url(sig)}")
   EOF
   ```
2. Copie o token impresso.
3. Na aba do **Swagger**, clique **Authorize**, cole `Bearer <token>`, confirme.
4. Demonstre em sequência, clicando em "Try it out" em cada endpoint:
   - Criar cliente (`POST /api/Customer`)
   - Criar veículo (`POST /api/Vehicle`)
   - Criar serviço (`POST /api/Service`) e/ou peça (`POST /api/Parts`)
   - Abrir uma OS (`POST /api/ServiceOrders`)
   - Listar OS (`GET /api/ServiceOrders`) — mostre a ordenação por status
   - Mover para diagnóstico / finalizar análise (`start-analysis`, `finish-analysis`)
   - Aprovar **ou** recusar o orçamento (`approve` / `refuse`)
   - Consultar status (`GET /api/ServiceOrders/{id}/status`)

---

## Parte 4 — Escalabilidade automática (~3 min)

Neste ambiente, o piso real de operação é **2 réplicas** (não 1): cada pod já usa ~53-60% do
limite de memória configurado (128Mi), então o HPA calcula que reduzir para 1 réplica projetaria
mais de 100% de uso — acima da meta de 75% — e evita essa oscilação de propósito. Isso já foi
testado e confirmado nesta sessão (tentei forçar 1 réplica manualmente e o HPA desfez sozinho).
Por isso a demonstração aqui é **2 → 3** réplicas (o teto configurado), não 1 → 2.

Divida a tela em 2 painéis de terminal:

```bash
# Painel A — observar o HPA em tempo real
export KUBECONFIG=/etc/rancher/k3s/k3s.yaml
kubectl get hpa oficina-mecanica-api-hpa --watch
```

```bash
# Painel B — gerar carga (use o token da Parte 3)
TOKEN="<cole_o_token_aqui>"
while true; do
  for i in $(seq 1 20); do
    curl -s -o /dev/null http://localhost:8080/api/ServiceOrders -H "Authorization: Bearer $TOKEN" &
  done
  wait
done
```

Em cerca de 1 minuto o `REPLICAS` sobe de `2` para `3` (CPU/memória passam da meta configurada:
80%/75%). Narre o que está acontecendo. `Ctrl+C` no painel B para parar a carga.

---

## Depois de gravar

1. Pare a gravação.
2. Suba o vídeo no **YouTube** ou **Vimeo** como **não listado**.
3. Copie o link e peça para eu atualizar o README e o PDF de entrega com ele.
