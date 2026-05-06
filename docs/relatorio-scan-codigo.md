# Relatório de Análise de Código — Oficina Mecânica API

**Projeto:** Oficina Mecânica API  
**Tecnologia:** .NET 8 / ASP.NET Core / Entity Framework Core / PostgreSQL  
**Data da análise:** 2026-05-06  
**Responsável:** Equipe de desenvolvimento — FIAP Tech Challenge Fase 1  

---

## 1. Metodologia

A análise foi realizada de forma estática (sem execução do código em produção), cobrindo os seguintes aspectos:

- Exposição de credenciais e dados sensíveis
- Injeção de SQL
- Autenticação e autorização
- Validação de entrada
- Tratamento de erros e exceções
- Qualidade e cobertura de testes
- Configuração de segurança do contêiner

---

## 2. Resumo Executivo

| Categoria | Nível de Risco | Status |
|-----------|---------------|--------|
| Credenciais expostas | Baixo (intencional — ambiente acadêmico) | Aceito |
| Injeção de SQL | Nenhum | ✅ OK |
| Autenticação JWT | Nenhum | ✅ OK |
| Autorização de endpoints | Baixo | ✅ OK |
| Validação de entrada | Médio | ⚠️ Atenção |
| Tratamento de exceções | Baixo | ✅ OK |
| Cobertura de testes | OK | ✅ 257 testes / 0 falhas |
| Segurança do contêiner | Nenhum | ✅ OK |

---

## 3. Achados Detalhados

### 3.1 Credenciais Expostas — ACEITO (contexto acadêmico)

**Arquivos afetados:**
- `src/OficinaApi/Application/Services/EmailService.cs` (linhas 29–30)
- `src/OficinaApi/appsettings.json` (senha do banco nos exemplos)
- `.env` (credenciais do banco e JWT)

**Descrição:**  
As credenciais do servidor SMTP (Gmail App Password) e da senha do usuário Admin inicial estão presentes em texto claro no código-fonte. Adicionalmente, o arquivo `.env` contém a senha do banco de dados e o segredo JWT em texto plano.

**Decisão da equipe:**  
Mantidas intencionalmente para facilitar a avaliação pelos professores sem necessidade de configuração adicional. Em ambiente de produção real, todas essas informações seriam obrigatoriamente movidas para variáveis de ambiente gerenciadas por um cofre de segredos (ex: Azure Key Vault, AWS Secrets Manager).

**Mitigação aplicável em produção:**
```bash
# Exemplo de uso com variáveis de ambiente seguras
docker run -e EmailSettings__SmtpPassword="$(vault kv get -field=smtp_pass secret/oficina)" ...
```

---

### 3.2 Injeção de SQL — NENHUM RISCO

**Arquivos analisados:**  
Todos os repositórios em `src/OficinaApi/Infrastructure/Repositories/`

**Descrição:**  
A aplicação utiliza Entity Framework Core com LINQ para todas as operações de banco de dados. A única consulta SQL nativa encontrada (`ServiceOrderRepository.cs`, linha 87) utiliza `SqlQuery<double>` com SQL fixo sem parâmetros externos, o que não representa risco de injeção.

```csharp
// Seguro: SQL estático sem interpolação de entrada do usuário
var result = await _context.Database
    .SqlQuery<double>($"""
        SELECT COALESCE(AVG(duracao_segundos) / 86400.0, 0) AS "Value"
        FROM ( ... )
    """)
```

**Status:** ✅ Nenhum risco identificado.

---

### 3.3 Autenticação e Autorização JWT — OK

**Arquivos analisados:**  
`src/OficinaApi/Application/Services/TokenService.cs`, `src/OficinaApi/Program.cs`, `src/OficinaApi/Controllers/AuthController.cs`

**Pontos verificados:**

| Item | Resultado |
|------|-----------|
| Algoritmo de assinatura | HS256 (HMAC-SHA256) ✅ |
| Expiração do token | Configurável via `Jwt:ExpiresInMinutes` (padrão: 60 min) ✅ |
| Claims incluídos | `sub` (userId), `email`, `jti` (UUID único por token) ✅ |
| Validação de Issuer/Audience | Habilitada ✅ |
| Validação de tempo de vida | Habilitada (`ValidateLifetime = true`) ✅ |
| Senha armazenada | BCrypt com salt (`BCrypt.Net.BCrypt.HashPassword`) ✅ |
| Endpoint de login | `POST /api/auth/login` com `[AllowAnonymous]` ✅ |

**Endpoints públicos (sem autenticação):**
- `POST /api/auth/login` — necessário para obter token
- `POST /api/users` — permite criação do primeiro usuário admin

Todos os demais endpoints requerem token JWT válido via `[Authorize]`.

**Status:** ✅ Implementação segura.

---

### 3.4 Validação de Entrada — ATENÇÃO

**Arquivos analisados:**  
`src/OficinaApi/Application/DTOs/`

**Descrição:**  
Os DTOs de entrada não utilizam Data Annotations (`[Required]`, `[MaxLength]`, `[EmailAddress]` etc.) para validação automática pelo framework. A validação ocorre dentro dos serviços de domínio (ex: validação de CPF/CNPJ via Value Objects), porém campos como `Name`, `Email` e `Password` nos DTOs de usuário aceitam strings vazias sem rejeição automática pela API.

**Exemplo identificado:**
```csharp
// CreateUserDto.cs — sem anotações de validação
public class CreateUserDto
{
    public string Name     { get; set; } = string.Empty;  // aceita vazio
    public string Email    { get; set; } = string.Empty;  // aceita vazio
    public string Password { get; set; } = string.Empty;  // aceita vazio
    public string Role     { get; set; } = "User";
}
```

**Impacto:** Baixo — a ausência de anotações não gera vulnerabilidades diretas, mas pode permitir cadastros com dados inválidos.

**Recomendação para produção:**
```csharp
using System.ComponentModel.DataAnnotations;

public class CreateUserDto
{
    [Required] [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required] [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required] [MinLength(6)]
    public string Password { get; set; } = string.Empty;
}
```

---

### 3.5 Tratamento de Erros e Exceções — OK

**Arquivos analisados:**  
`src/OficinaApi/Controllers/`, `src/OficinaApi/ExceptionFilters/GlobalExceptionFilter.cs`

**Pontos verificados:**

| Item | Resultado |
|------|-----------|
| Filtro global de exceções | Implementado (`GlobalExceptionFilter.cs`) ✅ |
| Controllers retornam erros sem stacktrace | ✅ |
| Exceções de domínio convertidas em HTTP responses | ✅ |
| Uso de `catch (Exception ex)` genérico em controllers | Presente em `UsersController`, `PartsController` — aceitável neste contexto ✅ |

**Status:** ✅ Tratamento adequado ao nível do projeto.

---

### 3.6 Segurança do Contêiner Docker — OK

**Arquivos analisados:**  
`Dockerfile`, `docker-compose.yml`

| Item | Resultado |
|------|-----------|
| Imagem base | `mcr.microsoft.com/dotnet/aspnet:8.0-alpine` (mínima) ✅ |
| Usuário não-root | `appuser` (sistema sem shell) ✅ |
| Porta exposta | Apenas 8080 (HTTP interno) ✅ |
| Imagem de build separada da de runtime | Multi-stage build ✅ |
| Restart policy | `unless-stopped` ✅ |
| Health check no banco | `pg_isready` com retries ✅ |

**Status:** ✅ Configuração segura e seguindo boas práticas.

---

## 4. Cobertura de Testes

### Execução em 2026-05-06

```
Passed! - Failed: 0, Passed: 257, Skipped: 0, Total: 257
```

### Distribuição dos testes

| Categoria | Arquivos | Testes |
|-----------|----------|--------|
| Testes unitários — Controllers | 5 arquivos | AuthController, UsersController, CustomerController, VehicleController, ServiceOrderController |
| Testes unitários — Services | 4 arquivos | UserService, TokenService, EmailService, PartService, ServiceOrderService |
| Testes unitários — Domain Entities | 5 arquivos | ServiceOrder, Part, ServiceOrderPart, ServiceOrderService, Service |
| Testes unitários — Value Objects | 2 arquivos | Document (CPF/CNPJ), Plate |
| Testes unitários — DTOs | 2 arquivos | CustomerDto, VehicleDto |
| Testes unitários — Event Handlers | 1 arquivo | ServiceOrderApprovedEventHandler |
| Testes unitários — Repositories | 1 arquivo | ServiceOrderRepository |
| Testes de integração | 5 arquivos | Customer, Parts, Service, ServiceOrder, Vehicle |
| **Total** | **25 arquivos** | **257 testes** |

### Frameworks utilizados

- **xUnit** — framework de testes
- **Moq** — mocking de dependências
- **FluentAssertions** — assertions expressivas
- **Microsoft.EntityFrameworkCore.InMemory / SQLite** — banco em memória para testes de integração

---

## 5. Conclusão

O projeto apresenta um nível de segurança adequado para o contexto acadêmico proposto. Os pontos de atenção identificados (credenciais expostas, ausência de Data Annotations) são conhecidos pela equipe e aceitos como trade-off para facilitar a avaliação. Nenhuma vulnerabilidade crítica foi identificada nas camadas de autenticação, acesso a dados ou infraestrutura.

A suíte de testes cobre os principais fluxos de domínio com 257 casos de teste sem falhas, garantindo a confiabilidade dos comportamentos críticos da aplicação.

---

*Análise realizada pela equipe de desenvolvimento — FIAP SOAT Tech Challenge Fase 1*
