# ADR 0002: Identity and Multi-Tenancy Architecture

## Status
Accepted

## Date
2026-07-22

## Context
Sub-phase 1.1 requires a robust, secure, and performant multi-tenant database isolation strategy (database-per-tenant), claims-based tenant context resolution, and a double-step authentication flow allowing users associated with multiple farms/tenants to select the active tenant context during login.

## Decision
1. **Claims-Based Context Resolution**: Do not use subdomains or HTTP headers to identify the tenant. Instead, resolve the active tenant ID from the `TenantId` claim in the authenticated user's JWT token.
2. **Database-per-Tenant Isolation**: Configure EF Core database contexts to dynamically switch connection strings based on the resolved `TenantId` via a shared `ITenantConnectionProvider`.
3. **Master (Foundation) Database**: Maintain a global master database schema (`tenancy`) containing user accounts, organizations (tenants), and their associations (`UserTenants`) for authentication.
4. **Double-Step Authentication Flow**: 
   - Step 1: User enters email and password.
   - Step 2: If the user belongs to multiple tenants, the system prompts them to select a tenant, then issues a JWT containing the selected `TenantId` claim. If they belong to exactly one tenant, the system generates the token directly.
5. **Standardized Cryptography**: Use `PBKDF2` (SHA256) inside the standard library for secure password hashing and verification.

## Consequences
1. Complete tenant data isolation is guaranteed at the physical database layer.
2. Cross-tenant data leaks are prevented by scoping connections dynamically per request.
3. Users can seamlessly access multiple farms/organizations using a single credential set.
4. No network infrastructure configurations (like wildcards or DNS subdomains) are required to scale tenants.
