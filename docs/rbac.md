# Matriz de Acesso e Controle de Permissões (RBAC) & Convites de Equipe

## 1. Visão Geral
Este documento define o modelo de segurança **Role-Based Access Control (RBAC)** e o fluxo de convites de colaboradores para o sistema multi-tenant **CriaCerto**.

---

## 2. Papéis de Acesso (UserRole)

| Papel | Enum Code | Descrição | Permissões Principais |
| :--- | :---: | :--- | :--- |
| **Admin** | `1` | Proprietário ou Administrador da Fazenda | Gestão da Organização, Unidades de Produção, Convites de Equipe, Gestão Financeira e Acesso Total. |
| **Zootecnista** | `2` | Responsável Técnico Zootécnico & Nutricional | Gestão do Plantel, Registros de IATF, Manejo Nutricional, Análise de GPD, Balanças e Dashboard Executivo. |
| **Veterinario** | `3` | Responsável Sanitário e Clínico | Campanhas de Vacinação, Tratamentos Medicamentosos, Controle de Carência para Abate e Sanidade. |
| **OperadorCurral** | `4` | Manejador de Campo e Operador de Curral | Lançamento rápido de pesagem em curral, carregamento de trato, movimentação de piquetes (PWA Off-line). Sem acesso financeiro ou administrativo. |

---

## 3. Fluxo de Convites de Equipe (TeamInvite)

```
┌────────────────────────┐         ┌────────────────────────┐         ┌────────────────────────┐
│ Admin envia convite    │ ──────> │ E-mail gerado com      │ ──────> │ Convidado aceita       │
│ por e-mail & Role      │         │ Token (Validade: 7d)   │         │ convite & cria senha   │
└────────────────────────┘         └────────────────────────┘         └────────────────────────┘
                                                                                  │
                                                                                  ▼
                                                                      ┌────────────────────────┐
                                                                      │ Vínculo UserTenant     │
                                                                      │ criado com UserRole    │
                                                                      └────────────────────────┘
```

1. **Geração do Convite:** O `Admin` informa o e-mail do colaborador e seleciona a `Role`. O sistema gera um token de 16 caracteres e define a expiração em 7 dias (`TeamInvite`).
2. **Aceite do Convite:** O convidado acessa a página de aceite ou cadastro. O handler `AcceptTeamInviteCommandHandler` valida o token e associa a conta do usuário ao `TenantId` com a `Role` correspondente.
3. **Revogação / Remoção:** O `Admin` pode revogar convites pendentes ou remover membros ativos do tenant a qualquer momento via `OrganizationManagement.razor`.

---

## 4. Injeção de Claims JWT & Políticas em ASP.NET Core

Os tokens JWT emitidos no Login ou na seleção de Fazenda passam a incluir os claims:
- `ClaimTypes.Role`: `Admin`, `Zootecnista`, `Veterinario`, ou `OperadorCurral`.
- `Role`: Nome textual da função ativa.

### Políticas de Autorização Configuradas:
- `AdminOnly`: Exige papel `Admin`.
- `ZootecniaOrAdmin`: Exige papel `Admin` ou `Zootecnista`.
- `CurralAccess`: Acessível por todos os papéis operacionais (`Admin`, `Zootecnista`, `Veterinario`, `OperadorCurral`).
