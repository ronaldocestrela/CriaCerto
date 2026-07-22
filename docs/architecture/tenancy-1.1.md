# Identity & Multi-Tenancy (1.1) Architecture Guide

## Objective
Establish the core identity models, database-per-tenant connection resolution, and multi-tenant authentication UI.

## Tenancy Module (`src/Modules/Tenancy`)

### 1. Domain Entities
- **User**: Represents user identity containing `Id`, `Email`, `FullName`, and `PasswordHash` (hashed using PBKDF2).
- **Tenant**: Represents farm organization detailing `Id`, `Name`, `CNPJ`, `Status` (Active/Suspended/Maintenance), `SubscribedPlan` (Starter/Pro/Enterprise), and zootecnic capacity constraints.
- **UserTenant**: Join table mapping users to multiple tenants.

### 2. Database Isolation Strategy
- **Master Database (`criacerto_foundation`)**: Contains the global `tenancy` schema (`Users`, `Tenants`, `UserTenants` tables) to authenticate and resolve user-tenant mappings.
- **Tenant Database (`criacerto_tenant_{TenantId:N}`)**: Independent database catalog dynamically resolved per request using:
  - `ITenantContext`: Reads claims from `ClaimsPrincipal`.
  - `ITenantConnectionProvider`: Constructs dynamic connection strings pointing to the tenant's database.

---

## Authentication Flow

### Double-Step Login Sequence
```
[Client]                                              [Backend]
   |                                                      |
   |--- POST /api/auth/login (Email, Password) ---------->|
   |                                                      |-- Validate credentials
   |<-- 200 OK (RequiresTenantSelection: true) -----------|-- Retrieve mapped tenants
   |                                                      |
   |--- POST /api/auth/select-tenant (UserId, TenantId) ->|
   |                                                      |-- Generate JWT with claims
   |<-- 200 OK (Token: JWT) ------------------------------|   (TenantId, Plan, Name)
```

---

## Web Frontend Design System

All UI elements are implemented with Blazor WebAssembly components using standard Vanilla CSS tokens configured in [app.css](file:///home/rony/LPR/CriaCerto/src/Web/CriaCerto.Web/CriaCerto.Web/wwwroot/app.css).

### Design Variables
- **Headline Font**: `Work Sans`
- **Technical/Label Font**: `JetBrains Mono`
- **Primary Color**: `#00652c` (Deep Green)
- **Primary Container**: `#15803d` (Vibrant Grass Green)
- **Canvas Background**: `#f7f9fb` (Ice White/Light Blue-Gray)
- **Surface Panels**: `#ffffff`

### Main Components
- **Login.razor**: Credentials step featuring scale micro-interactions transitions into a farm select step listing units with specific type badges (Warehouse, Analytics, Agriculture).
- **OrganizationManagement.razor**: High-fidelity Bento Grid rendering organization stats, AES-256 tenant data isolation status, and active barns tables.
