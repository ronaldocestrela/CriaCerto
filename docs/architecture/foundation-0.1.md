# Foundation 0.1 Architecture Guide

## Objective
Deliver Sub-phase 0.1 as the technical baseline for future business modules.

## Folder Strategy
`src/BuildingBlocks`
- Shared abstractions (`Result`, `Error`, contracts)
- Application cross-cutting concerns (MediatR behaviors)
- Infrastructure primitives (EF Core base setup)

`src/Modules/*`
- Domain-aligned modules with isolated application and infrastructure projects
- No direct data-level coupling between modules

`src/Host`
- API composition root and environment configuration

`src/Web`
- Blazor Web App + WebAssembly interactive client
- PWA assets and service worker baseline

`tests`
- Unit tests for shared kernel invariants and behaviors
- Integration tests for architecture and host-level contracts

## Current Cross-Cutting Baseline
1. Validation pipeline based on MediatR + FluentValidation.
2. Base SQL Server registration using EF Core 10 and connection pooling.
3. Result Pattern as default command/query response contract.

## Out of Scope for 0.1
1. Tenant resolution and feature gating business rules.
2. Module-specific functional use cases beyond assembly placeholders.
3. Full offline queue synchronization logic.