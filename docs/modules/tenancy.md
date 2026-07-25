# Módulo de Tenancy & Autenticação (`Modules.Tenancy`)

## Visão Geral
O módulo `Modules.Tenancy` gerencia as identidades dos usuários, organizações/fazendas (Tenants), mapeamento multi-tenant (`UserTenants`), fluxo de autenticação e licenças de acesso.

---

## 1. Entidades de Domínio

- **User**: Representa a identidade global do usuário no sistema.
  - `Id`: `Guid`
  - `FullName`: `string` (obrigatório, max 150)
  - `Email`: `string` (obrigatório, único, max 150)
  - `PasswordHash`: `string` (hash PBKDF2 com SHA256)
  - `PhoneNumber`: `string?` (opcional, max 30)
  - `PasswordResetToken`: `string?` (token para recuperação de senha)
  - `PasswordResetTokenExpiresAt`: `DateTime?` (data/hora de expiração do token)
  - `UserTenants`: `List<UserTenant>` (relacionamento N:N com Tenants)

- **Tenant**: Representa a unidade produtiva/fazenda.
  - `Id`: `Guid`
  - `Name`: `string`
  - `CNPJ`: `string`
  - `State`: `string`
  - `Type`: `string` (ex: Matriz, Recria, Engorda)
  - `Status`: `string`
  - `SubscribedPlan`: `string` (Starter, Pro, Enterprise)

- **UserTenant**: Tabela associativa entre `User` e `Tenant`.

---

## 2. Endpoints da API (`/api/auth`)

| Método | Endpoint | Descrição | Requer Auth | Status de Sucesso |
|---|---|---|---|---|
| `POST` | `/api/auth/login` | Autenticação por e-mail e senha | Não | `200 OK` (`AuthResponse`) |
| `POST` | `/api/auth/select-tenant` | Seleção de tenant para contas com múltiplas fazendas | Não | `200 OK` (`AuthResponse`) |
| `POST` | `/api/auth/register` | Auto-cadastro de novo usuário (Sign-Up) | Não | `201 Created` (`UserDto`) |
| `POST` | `/api/auth/forgot-password` | Solicitação de código/token para redefinição de senha | Não | `200 OK` (token) |
| `POST` | `/api/auth/reset-password` | Redefinição de senha com token de verificação | Não | `200 OK` |
| `GET` | `/api/v1/tenancy/plans` | Consulta de planos de assinatura comercial | Não | `200 OK` (`List<SubscriptionPlanDto>`) |

---

## 3. Casos de Uso (CQRS / MediatR)

### 3.1 `RegisterUserCommand`
- **Contrato:** `RegisterUserCommand(string FullName, string Email, string Password, string? PhoneNumber)`
- **Validações (`RegisterUserCommandValidator`):**
  - Nome completo: Mínimo 3 caracteres, máximo 150.
  - E-mail: Formato de e-mail válido.
  - Senha: Mínimo 8 caracteres, maiúscula, minúscula e número.
- **Regra de Negócio:** Se o e-mail já estiver cadastrado, retorna `Result.Failure(Error.Conflict("User.EmailAlreadyExists", ...))`.

### 3.2 `ForgotPasswordCommand`
- **Contrato:** `ForgotPasswordCommand(string Email)`
- **Validações (`ForgotPasswordCommandValidator`):** Formato de e-mail válido.
- **Regra de Negócio:** Gera token de 6 dígitos numéricos alfanuméricos com validade de 1 hora (`PasswordResetTokenExpiresAt = DateTime.UtcNow.AddHours(1)`). Retorna `Result.Success`.

### 3.3 `ResetPasswordCommand`
- **Contrato:** `ResetPasswordCommand(string Email, string Token, string NewPassword)`
- **Validações (`ResetPasswordCommandValidator`):** Valida e-mail, obrigatoriedade do token e senha forte.
- **Regra de Negócio:** Verifica se o token corresponde ao usuário e se `PasswordResetTokenExpiresAt > DateTime.UtcNow`. Se válido, atualiza o hash da nova senha e limpa o token.

---

## 4. Componentes Frontend (Blazor WASM)

- **`Login.razor` (`/login`):** Tela de login em 2 passos (Credenciais -> Seleção de Fazenda para usuários multi-tenant). Possui links diretos para `/register` e `/forgot-password`.
- **`Register.razor` (`/register`):** Formulário reativo de auto-cadastro com feedback visual, tratamento de erro do Result Pattern e card de confirmação.
- **`ForgotPassword.razor` (`/forgot-password`):** Assistente de recuperação em 2 passos (Solicitar código -> Redefinir senha).

---

## 5. Testes Unitários (`CriaCerto.Modules.Tenancy.UnitTests`)

- `RegisterUserCommandHandlerTests`: Testes de criação bem-sucedida e rejeição de e-mail duplicado (`Error.Conflict`).
- `RegisterUserCommandValidatorTests`: Testes de regras de validação cliente/servidor.
- `ForgotPasswordCommandHandlerTests`: Testes de geração de token e expiração.
- `ResetPasswordCommandHandlerTests`: Testes de alteração de senha e rejeição de tokens expirados/inválidos.
