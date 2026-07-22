# ADR 0001: Foundation Architecture for Modular Monolith

## Status
Accepted

## Date
2026-07-22

## Context
Sub-phase 0.1 requires a production-oriented foundation for a swine management SaaS with strict module boundaries, Result Pattern usage, Blazor PWA readiness, and TDD-first development.

## Decision
1. Use a monorepo with a single root solution for backend, frontend, and tests.
2. Use `src/BuildingBlocks` as shared kernel for Result Pattern, application behaviors, and infrastructure primitives.
3. Keep domain modules under `src/Modules/*` with explicit references only to BuildingBlocks and no direct module-to-module persistence coupling.
4. Use MediatR pipeline behaviors with FluentValidation to enforce command/query validation.
5. Use EF Core 10 + Npgsql base infrastructure with connection pooling support.
6. Use Blazor Web App with interactive WebAssembly and PWA service worker foundation.

## Consequences
1. New modules can be added incrementally without changing the base architecture.
2. Business failures must return `Result`/`Result<T>` instead of control-flow exceptions.
3. Cross-module joins and hidden coupling are prevented by project dependency rules.
4. Offline shell behavior is available from phase 0.1 onward, enabling rural-first evolution.