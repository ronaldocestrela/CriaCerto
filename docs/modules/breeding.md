# Breeding Module

## Phase 2.1 — Plantel (DONE)

Sub-phase 2.1 introduces the first functional slice of `Modules.Breeding`: the Sow and Boar Registry (`Plantel`). It provides tenant-scoped registry CRUD, lifecycle status transitions, Blazor PWA screens based on Stitch handoff assets, and an IndexedDB-backed read cache for offline search/filter.

## Phase 2.2 — Insemination, Breeding & Pregnancy Diagnosis (DONE)

Sub-phase 2.2 adds reproductive event tracking, pregnancy diagnosis workflow, automatic DNP recalculation, offline write queueing for field operations, and mobile-first Blazor screens from Stitch phase-2.2 assets.

## Module Boundaries

- Module: `Modules.Breeding`
- Persistence schema: `breeding`
- Database: tenant database resolved by `ITenantConnectionProvider`
- Access control: all commands and queries are decorated with `[RequiresModule("Breeding")]`
- Response contract: all handlers return `Result<T>` or `Result`

Direct joins to other modules are not allowed. Maternity, sanitary, and analytics data must integrate through module contracts or domain events, not cross-module EF navigation.

## Domain Entities

### Sow

Represents a breeding female (`Matriz`) in the tenant's plantel.

Key fields:

- `TagId`, `Nickname`, `PbbRegistration`
- `Breed`, `Origin`, `BirthDate`
- `EntryDate`, `EntryWeightKg`, `CurrentWeightKg`, `AverageDailyGain`
- `FatherTagId`, `MotherTagId`
- `BodyConditionScore`
- `Parity` (`OP`)
- `DnpDays` (recalculated by `IDnpCalculator` after breeding/diagnosis events)
- `ReproductiveStatus`
- `LifecycleStatus`
- `Location`
- `Events`

Domain methods (2.2):

- `RegisterBreeding(...)` — `Empty`/`Bred` → `Bred`; rejects `Pregnant`, `Lactating`, `Culled`
- `ApplyPregnancyDiagnosis(...)` — only from `Bred`; `Pregnant` → `Pregnant`, otherwise → `Empty`
- `SetDnpDays(int)` — updated by handlers after DNP calculation

### Boar

Represents a breeding male (`Cachaço`) in the tenant's plantel.

### BreedingEvent

Records an insemination/natural service event:

- `SowId`, `EventDate`, `Method` (`IA`, `IATF`, `Monta Natural`)
- `BoarOrSemenRef`, `Technician`, `BodyConditionScoreAtBreeding`, `Location`, `Notes`

### PregnancyDiagnosis

Records ultrasound or return-to-estrus diagnosis:

- `SowId`, `BreedingEventId?`, `DiagnosisDate`, `Method`, `Result`, `Notes`

## Status Model

### LifecycleStatus

- `Active`, `Quarantine`, `Culled` (terminal)

### ReproductiveStatus

- `Empty` (`Vazia`)
- `Bred` (`Coberta`)
- `Pregnant` (`Gestante/Prenha`)
- `Lactating` (`Lactante/Maternidade`)

Reproductive transitions are owned by breeding/diagnosis domain methods in 2.2 (no manual CRUD override for event-driven states).

## DNP Calculation

`IDnpCalculator` recalculates `Sow.DnpDays` after breeding or diagnosis:

- Counts days in `Empty` or `Bred` from `EntryDate` or last non-pregnant diagnosis date
- Freezes DNP at pregnancy confirmation (`Pregnant`)
- Resets counting window after failed diagnosis (`Empty`, `ReturnToEstrus`, `AbortOrDoubt`)

UI thresholds:

- Plantel attention: `DnpDays >= 40` or `LifecycleStatus == Quarantine`
- Diagnosis queue alert: `DnpDays >= 12`

## Pregnancy Check Queue

`ListPregnancyCheckTasksQuery` returns sows where:

- `ReproductiveStatus == Bred`
- Latest breeding event is between **21 and 28 days** post-breeding
- Lifecycle is not `Culled`

## API Endpoints

All endpoints require authentication and module access to `Breeding`.

| Method | Route | Purpose |
| --- | --- | --- |
| `GET` | `/api/breeding/sows` | List sows with `search`, `status`, `page`, `pageSize` |
| `POST` | `/api/breeding/sows` | Create a sow |
| `GET` | `/api/breeding/sows/{id}` | Get sow profile |
| `PUT` | `/api/breeding/sows/{id}` | Update a sow |
| `POST` | `/api/breeding/sows/{id}/status` | Change sow lifecycle status |
| `GET` | `/api/breeding/boars` | List boars with `search`, `page`, `pageSize` |
| `POST` | `/api/breeding/boars` | Create a boar |
| `GET` | `/api/breeding/boars/{id}` | Get boar profile |
| `PUT` | `/api/breeding/boars/{id}` | Update a boar |
| `POST` | `/api/breeding/boars/{id}/status` | Change boar lifecycle status |
| `POST` | `/api/breeding/events/batch` | Register breeding batch (atomic) |
| `POST` | `/api/breeding/diagnoses` | Register pregnancy diagnosis |
| `GET` | `/api/breeding/pregnancy-checks` | Ultrasound task queue (21–28 days) |

Expected failures return `Result.Failure(Error)` and map to HTTP:

- `Validation` -> `400`
- `NotFound` -> `404`
- `Conflict` -> `409`
- `Unauthorized` -> `403`

## Frontend

Stitch handoff assets:

- Phase 2.1: `docs/design/stitch/phase-2.1/`
- Phase 2.2: `docs/design/stitch/phase-2.2/` (`breeding-entry`, `pregnancy-diagnosis`)

Implemented routes:

- `/breeding/registry`
- `/breeding/sows/{id}`
- `/breeding/boars/{id}`
- `/breeding/cobertura` — batch breeding entry (mobile)
- `/breeding/diagnostico` — pregnancy check task list (mobile)

Reusable components:

- Plantel: `StatusChip`, `PlantelSearchBar`, `OfflineBanner`, `KpiStatCard`, `LifecycleTimeline`
- Breeding ops: `BreedingEntryCard`, `EccStepper`, `PregnancyTaskCard`, `DnpAlertBanner`, `SyncStatusChip`, `BreedingBottomNav`

All pages use `<ModuleGuard Module="Breeding">`. Sow profile links to `/breeding/cobertura?tag={TagId}` via **Registrar Evento**.

## Offline Storage

`wwwroot/js/plantel-cache.js` (IndexedDB v2):

- `sows` — read cache for registry search/filter (2.1)
- `pendingOps` — write queue for `breedingBatch` and `diagnosis` payloads (2.2)

Behavior:

1. Online submit succeeds → POST to API immediately.
2. Offline submit → enqueue in `pendingOps`, show IndexedDB/wifi_off banner.
3. On page load when online → drain queue via `BreedingOpsApiClient.SyncPendingOpsAsync`.

## Tests

`tests/Unit/CriaCerto.Modules.Breeding.UnitTests` covers:

- Lifecycle transitions (2.1)
- Breeding rejection when pregnant/lactating/culled (2.2)
- Diagnosis state mapping from `Bred` (2.2)
- DNP calculator segments and reset rules (2.2)

## Phase 2.1 Checklist

- [x] Backend endpoints/services created and returning `Result<T>`.
- [x] Domain logic covered by unit tests.
- [x] Blazor components implemented from Stitch handoff guidance.
- [x] Offline-capable search/filter via IndexedDB read cache.
- [x] Plan/module access controls applied.
- [x] Living documentation updated.

## Phase 2.2 Checklist

- [x] `RegisterBreedingBatchCommand` and `RegisterPregnancyDiagnosisCommand` returning `Result<T>`.
- [x] `IDnpCalculator` service with unit tests.
- [x] Invalid reproductive transitions rejected (`Sow.AlreadyPregnant`, `Sow.NotBred`, etc.).
- [x] Blazor batch breeding form and pregnancy check list from Stitch 2.2.
- [x] IndexedDB write queue with online sync drain.
- [x] Living documentation updated.
