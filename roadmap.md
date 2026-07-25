# Product Development Roadmap: Cattle / Bovine Management SaaS Platform

## 1. Roadmap Strategy & Rules of Engagement

To ensure production-ready quality and prevent technical debt, this roadmap is structured into sequential **Phases** and **Sub-phases** tailored for Bovine/Cattle Farm Management (Beef & Dairy).

### Core Completion Rule
> **A Phase or Sub-phase is considered "DONE" ONLY when both the Backend (.NET 10) and Frontend (Blazor PWA + MCP Stitch UI) are fully implemented, covered by unit/integration tests (TDD), offline-capable for field/curral environments, and accompanied by updated living documentation.**

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
               │    Phase 2: Cattle Herd & IATF Breeding │
               └────────────────────┬────────────────────┘
                                    │
               ┌────────────────────▼────────────────────┐
               │  Phase 3: Calving Ops & Calf Nursery    │
               └────────────────────┬────────────────────┘
                                    │
               ┌────────────────────▼────────────────────┐
               │ Phase 4: Pasture, Feedlot & Weighings   │
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
  * Integrate PWA Service Worker manifest and offline caching shell for remote field/pasture usage.
  * Setup **MCP Stitch** layout generator pipelines and component library foundation.
* **TDD & Living Doc Gate:**
  * Unit tests for `Result` pattern extensions and pipeline behaviors.
  * Initialize `/docs/adrs/` and automated API documentation flow.

---

### Phase 1: Multi-Tenancy, Authentication & Module Licensing

#### Sub-phase 1.1: Identity & Multi-Tenant Infrastructure
* **Backend:**
  * Tenant resolution middleware (Header/Subdomain per Farm/Fazenda).
  * Tenant database isolation strategy (Schema-per-tenant / Row-Level Security).
  * JWT Auth endpoints returning `Result<AuthResponse>`.
* **Frontend:**
  * Login, Register, and Farm/Tenant Switcher Blazor components generated via MCP Stitch.
  * Local token storage & AuthStateProvider setup.
* **TDD & Living Doc Gate:** Integration tests verifying tenant context separation across distinct farms.

#### Sub-phase 1.2: Plan-Based Feature Gating (Modular Monolith Licensing)
* **Backend:**
  * Subscription Plan model (`Starter`, `Pro`, `Enterprise`).
  * Module access enforcement attributes & MediatR pipeline behaviors (`[RequiresModule("Breeding")]`).
* **Frontend:**
  * Dynamic sidebar & routing guard component (`<ModuleGuard Module="Breeding">`).
  * Head-of-cattle capacity limit warnings and plan upgrade UI callout components.
* **TDD & Living Doc Gate:** End-to-end tests ensuring restricted plan users cannot invoke locked endpoints/views (e.g., Feedlot / Confinamento optimization).

---

### Phase 2: Cattle Herd & IATF Breeding Management (`Modules.Breeding`)

#### Sub-phase 2.1: Cattle Registry & Individual Identification (Plantel e Brincagem)
* **Backend:**
  * CRUD commands/queries for Cows (Vacas/Matrizes), Bulls (Touros/Reprodutores), and Heifers (Novilhas) returning `Result<T>`.
  * Support for SISBOV, Ear Tag ID (Brinco Amarelo), RFID Electronic Tags, and Tattoos.
  * Event handlers for status changes (Active, Pregnant, Open/Vazia, Culled, Sold).
* **Frontend:**
  * Cattle list and card view with RFID/Ear Tag search built via MCP Stitch.
  * Offline-capable search/filter via IndexedDB local cache for field use in remote pastures.
* **TDD & Living Doc Gate:** TDD coverage for status transition business rules. Updated `docs/modules/breeding.md`.

#### Sub-phase 2.2: Artificial Insemination (IATF Protocols) & Pregnancy Diagnosis
* **Backend:**
  * `RegisterIatfProtocolCommand` (Synchronization batch, hormone insertion/withdrawal dates, insemination date, semen lot).
  * `RegisterPregnancyDiagnosisCommand` (Ultrasound / Rectal Palpation - Prenhe vs Vazia).
  * Calculation service for Calving Interval (IEP - Intervalo Entre Partos), Conception Rate, and Open Days (Dias em Aberto).
* **Frontend:**
  * Curral/Mangueiro fast-entry IATF protocol entry form for field technicians.
  * Pregnancy check task queue component with offline queueing support.
* **TDD & Living Doc Gate:** Unit tests verifying IEP calculations and invalid state transitions (e.g., applying IATF protocol to a cow confirmed pregnant).

---

### Phase 3: Calving Operations & Calf Nursery (`Modules.Calving`)

#### Sub-phase 3.1: Calving & Calf Registration (Partos e Bezerreiro)
* **Backend:**
  * `RegisterCalvingCommand` (Mother Cow ID, Birth Date, Birth Weight, Sex, Breed, Calf Tag ID, Birth Condition).
  * Domain events emitting `CalvingCompletedEvent`.
* **Frontend:**
  * Mobile-first pasture calving registration form with touch-friendly controls.
  * Quick tag/RFID scanner integration for calf assignment.
* **TDD & Living Doc Gate:** TDD tests for `Calving` entity invariant validations.

#### Sub-phase 3.2: Weaning & 205-Day Adjusted Weight (Desmame)
* **Backend:**
  * `RegisterWeaningCommand` (Weaning weight, weaning date, pasture lot destination).
  * Calculation services for 205-Day Adjusted Weight (P205) and Pre-weaning Mortality Rate.
* **Frontend:**
  * Weaning management wizard with automatic lot assignment.
  * Performance badges for top-producing cows based on calf weaning weights.
* **TDD & Living Doc Gate:** Integration tests for weaning weight normalization algorithms.

---

### Phase 4: Pasture Management, Feedlot & Growth (`Modules.Growth`)

#### Sub-phase 4.1: Lot Creation, Paddock Management & Stocking Rate (Pastos e Lotação)
* **Backend:**
  * `CreateLotCommand`, `MoveLotToPaddockCommand`, `CloseLotCommand`.
  * Paddock capacity calculator: Animal Unit per Hectare (UA/ha - Taxa de Lotação).
* **Frontend:**
  * Interactive Pasture Map / Paddock Visualizer built via MCP Stitch.
  * Lot movement modal with offline background sync handler.
* **TDD & Living Doc Gate:** Unit tests for paddock overgrazing warnings and stocking rate math.

#### Sub-phase 4.2: Curral Weighings, ADG/GPD & Arroba (@) Tracking
* **Backend:**
  * `RecordWeighingCommand` (Scale integration / weight entry, carcass yield estimation %).
  * Zootecnic calculations: Average Daily Gain (ADG / GPD in kg/day and Arrobas @/month).
* **Frontend:**
  * Curral/Scale fast-input weighing screen with automated weight change calculation.
  * Interactive GPD trend charts using Blazor SVG components.
* **TDD & Living Doc Gate:** Tests covering GPD edge cases and negative weight loss warnings.

---

### Phase 5: Nutrition, Sanitary & Bovine Analytics (`Modules.Analytics`)

#### Sub-phase 5.1: Supplementation, Feedlot TMR & Cost per Arroba (@)
* **Backend:**
  * `RecordSupplementationCommand` (Sal Mineral, Proteinado em Pasto).
  * `RecordFeedlotTmrCommand` (Carregamento de Trato / Ração no Confinamento).
  * Feed Conversion Ratio (CA) and Cost per Arroba (@) produced query engine.
* **Frontend:**
  * Silo stock level, daily trough log (trato), and mineral salt consumption views.
  * Feed conversion and cost per @ dashboard widgets.
* **TDD & Living Doc Gate:** Full test suite for multi-phase feeding equations.

#### Sub-phase 5.2: Official Vaccination Campaigns & Slaughter Withdrawal Period
* **Backend:**
  * Official vaccination schedule generator (Febre Aftosa, Brucelose, Raiva, Clostridioses).
  * `ApplyTreatmentCommand` with active slaughter/milk withdrawal period (Período de Carência para Abate/Leite) block.
* **Frontend:**
  * Sanitary campaign manager and veterinary alert notification center.
  * Visual warning badge on animals/lots under active drug withdrawal period preventing slaughter dispatch.
* **TDD & Living Doc Gate:** TDD tests ensuring animals under grace period cannot be dispatched to slaughterhouse (Frigorífico).

#### Sub-phase 5.3: Executive Bovine Analytics & Final Hardening
* **Backend:**
  * Consolidated multi-farm KPI reporting engine (IEP, Conception Rate, GPD/Arrobas, UA/ha, Cost/@).
  * Data export endpoints (CSV, Excel, PDF generation for GTA/State inspections).
* **Frontend:**
  * Executive Dashboard with key cattle performance indicators, target vs actual comparisons, and offline cache backup.
  * Complete UI polish across all MCP Stitch components.
* **TDD & Living Doc Gate:** End-to-end integration tests across all modules. Final living documentation sync.

---

## 4. Phase Sign-Off Checklist Template

Before closing any sub-phase or phase, the following verification checklist MUST be executed:

```markdown
- [ ] Backend .NET 10 endpoints/services created and returning Result<T>.
- [ ] Domain logic covered by unit tests (Red/Green/Refactor verified).
- [ ] Blazor components generated/refactored using MCP Stitch guidelines.
- [ ] Offline capability & IndexedDB sync verified for curral/pasture forms.
- [ ] Plan/Module access controls tested and active.
- [ ] Living documentation (/docs) updated to reflect latest domain logic.
```
