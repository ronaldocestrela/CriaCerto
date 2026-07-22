# Product Development Roadmap: Swine Management SaaS Platform

## 1. Roadmap Strategy & Rules of Engagement

To ensure production-ready quality and prevent technical debt, this roadmap is structured into sequential **Phases** and **Sub-phases**.

### Core Completion Rule
> **A Phase or Sub-phase is considered "DONE" ONLY when both the Backend (.NET 10) and Frontend (Blazor PWA + MCP Stitch UI) are fully implemented, covered by unit/integration tests (TDD), offline-capable where specified, and accompanied by updated living documentation.**

---

## 2. Phase Architecture Overview

```
               ┌─────────────────────────────────────────┐
               │  Phase 0: Architecture & Core Standards  │
               └────────────────────┬────────────────────┘
                                    │
               ┌────────────────────▼────────────────────┐
               │     Phase 1: Multi-Tenancy & Licensing   │
               └────────────────────┬────────────────────┘
                                    │
               ┌────────────────────▼────────────────────┐
               │   Phase 2: Breeding & Sow Management    │
               └────────────────────┬────────────────────┘
                                    │
               ┌────────────────────▼────────────────────┐
               │    Phase 3: Maternity & Farrowing Ops   │
               └────────────────────┬────────────────────┘
                                    │
               ┌────────────────────▼────────────────────┐
               │ Phase 4: Nursery, Growth & Finishing    │
               └────────────────────┬────────────────────┘
                                    │
               ┌────────────────────▼────────────────────┐
               │ Phase 5: Nutrition, Sanitary & Analytics│
               └─────────────────────────────────────────┘
```

---

## 3. Detailed Phase Breakdown

---

### Phase 0: Foundation, Architecture & Tooling Setup

#### Sub-phase 0.1: Solution Structure & Shared Kernels
* **Backend (.NET 10):**
  * Create Modular Monolith solution setup (`src/BuildingBlocks`, `src/Modules/*`).
  * Implement base `Result`, `Result<T>`, and `Error` response types.
  * Configure EF Core 10 base infrastructure, PostgreSQL connection pooling, and MediatR pipelines.
* **Frontend (Blazor .NET 10):**
  * Create Blazor Web App with WebAssembly render mode setup.
  * Integrate PWA Service Worker manifest and offline caching shell.
  * Setup **MCP Stitch** layout generator pipelines and component library foundation.
* **TDD & Living Doc Gate:**
  * Unit tests for `Result` pattern extensions and pipeline behaviors.
  * Initialize `/docs/adrs/` and automated API documentation flow.

---

### Phase 1: Multi-Tenancy, Authentication & Module Licensing

#### Sub-phase 1.1: Identity & Multi-Tenant Infrastructure
* **Backend:**
  * Tenant identification through authenticated identity (user-tenant membership), without subdomain/header resolution.
  * Tenant database isolation strategy (database-per-tenant, dedicated connection per tenant).
  * JWT Auth endpoints returning `Result<AuthResponse>`.
* **Frontend:**
  * Login and Register Blazor components generated via MCP Stitch, with tenant resolved from authenticated user profile.
  * Optional tenant selection only for users linked to multiple tenants, still without subdomain dependency.
  * Local token storage & AuthStateProvider setup.
* **TDD & Living Doc Gate:** Integration tests verifying tenant context separation and tenant resolution from login claims.

#### Sub-phase 1.2: Plan-Based Feature Gating (Modular Monolith Licensing)
* **Backend:**
  * Subscription Plan model (`Starter`, `Pro`, `Enterprise`).
  * Module access enforcement attributes & MediatR pipeline behaviors (`[RequiresModule("Breeding")]`).
* **Frontend:**
  * Dynamic sidebar & routing guard component (`<ModuleGuard Module="Breeding">`).
  * Plan upgrade UI callout components.
* **TDD & Living Doc Gate:** End-to-end tests ensuring restricted plan users cannot invoke locked endpoints/views.

---

### Phase 2: Breeding & Sow Management (`Modules.Breeding`)

#### Sub-phase 2.1: Sow & Boar Registry (Plantel)
* **Backend:**
  * CRUD commands/queries for Sows (Matrizes) and Boars (Cachaços) returning `Result<T>`.
  * Event handlers for status changes (Active, Culled, Quarantine).
* **Frontend:**
  * Sow and Boar list/detail views built with MCP Stitch atomic components.
  * Offline-capable search/filter via IndexedDB local cache.
* **TDD & Living Doc Gate:** TDD coverage for status transition business rules. Updated `docs/modules/breeding.md`.

#### Sub-phase 2.2: Insemination, Breeding & Pregnancy Diagnosis
* **Backend:**
  * `RegisterBreedingEventCommand` (IA, IATF, Monta Natural).
  * `RegisterPregnancyDiagnosisCommand` (Ultrassom / Retorno ao Cio).
  * DNP (Days Non-Productive) calculation service.
* **Frontend:**
  * Fast-entry breeding batch log form for barn workers.
  * Pregnancy check task list component with offline queueing support.
* **TDD & Living Doc Gate:** Unit tests verifying DNP calculations and invalid state transitions (e.g., inseminating a sow already pregnant).

---

### Phase 3: Maternity & Farrowing Operations (`Modules.Maternity`)

#### Sub-phase 3.1: Farrowing & Piglet Registration (Partos)
* **Backend:**
  * `RegisterFarrowingCommand` (Nascidos Vivos, Natimortos, Mumificados, Peso da Ninhada).
  * Automated domain events emitting `FarrowingCompletedEvent`.
* **Frontend:**
  * Touch-friendly mobile-first maternity room entry form.
  * Real-time validation for live vs dead count.
* **TDD & Living Doc Gate:** TDD tests for `Farrowing` entity invariant validations.

#### Sub-phase 3.2: Cross-Fostering & Weaning (Adoções e Desmame)
* **Backend:**
  * `TransferPigletCommand` (Adoções/Transferências entre matrizes).
  * `RegisterWeaningCommand` (Desmame por matriz, quantidade, peso total).
  * Calculation services for NVMA (Nascidos Vivos/Matriz/Ano) and DMA (Desmamados/Matriz/Ano).
* **Frontend:**
  * Visual matrix adoption management UI.
  * Weaning record wizard with automatic lot destination selection.
* **TDD & Living Doc Gate:** Integration tests for cross-fostering inventory integrity.

---

### Phase 4: Nursery, Growth & Finishing (`Modules.Growth`)

#### Sub-phase 4.1: Batch Creation & Movement (Creche, Recria e Terminação)
* **Backend:**
  * `CreateBatchCommand`, `MoveBatchCommand`, `CloseBatchCommand`.
  * Lot tracking and location/pen assignment logic.
* **Frontend:**
  * Interactive barn layout / lot visualizer component built via MCP Stitch.
  * Batch movement modal with background sync handler.
* **TDD & Living Doc Gate:** Unit tests for lot capacity and transfer constraints.

#### Sub-phase 4.2: Weighing, ADG/GPD & Mortality Tracking
* **Backend:**
  * `RecordWeighingCommand` and `RecordMortalityCommand`.
  * Zootecnic calculations: GPD (Ganho de Peso Diário) and Mortality Rate per phase.
* **Frontend:**
  * Quick-input daily mortality entry form.
  * Interactive GPD trend charts using Blazor SVG components.
* **TDD & Living Doc Gate:** Tests covering GPD edge cases (e.g., zero-day weight deltas).

---

### Phase 5: Nutrition, Sanitary & Zootecnic Analytics (`Modules.Analytics`)

#### Sub-phase 5.1: Feed Consumption & FCR/CA Calculation
* **Backend:**
  * `RecordFeedConsumptionCommand` (Ração por setor/lote).
  * Feed Conversion Ratio (CA / FCR = Total Feed Consumed / Total Weight Gained).
  * Cost per kg of meat produced estimation query.
* **Frontend:**
  * Silo stock level and daily feed log views.
  * Feed conversion dashboard widgets.
* **TDD & Living Doc Gate:** Full test suite for multi-phase FCR equations.

#### Sub-phase 5.2: Sanitary Schedules & Withdrawal Warnings
* **Backend:**
  * Vaccination schedule generator based on pig age/phase.
  * `ApplyTreatmentCommand` with active withdrawal period (Período de Carência) block.
* **Frontend:**
  * Veterinary task calendar and alert notification center.
  * Visual warning badge on animals/lots under drug withdrawal.
* **TDD & Living Doc Gate:** TDD tests ensuring animals under grace period cannot be dispatched for slaughter.

#### Sub-phase 5.3: Executive Zootecnic Analytics & Final Hardening
* **Backend:**
  * Consolidated multi-farm KPI reporting engine (DNP, NVMA, DMA, GPD, CA).
  * Data export endpoints (CSV, Excel, PDF generation).
* **Frontend:**
  * Executive Dashboard with key indicators, target vs actual comparisons, and offline cache backup.
  * Complete UI polish across all MCP Stitch components.
* **TDD & Living Doc Gate:** End-to-end integration tests across all modules. Final living documentation sync.

---

## 4. Phase Sign-Off Checklist Template

Before closing any sub-phase or phase, the following verification checklist MUST be executed:

```markdown
- [ ] Backend .NET 10 endpoints/services created and returning Result<T>.
- [ ] Domain logic covered by unit tests (Red/Green/Refactor verified).
- [ ] Blazor components generated/refactored using MCP Stitch guidelines.
- [ ] Offline capability & IndexedDB sync verified for rural forms.
- [ ] Plan/Module access controls tested and active.
- [ ] Living documentation (/docs) updated to reflect latest domain logic.
```
