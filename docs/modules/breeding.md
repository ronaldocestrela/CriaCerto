# Breeding Module - Phase 2.1 Plantel

## Objective

Sub-phase 2.1 introduces the first functional slice of `Modules.Breeding`: the Sow and Boar Registry (`Plantel`). It provides tenant-scoped registry CRUD, lifecycle status transitions, Blazor PWA screens based on Stitch handoff assets, and an IndexedDB-backed read cache for offline search/filter.

## Module Boundaries

- Module: `Modules.Breeding`
- Persistence schema: `breeding`
- Database: tenant database resolved by `ITenantConnectionProvider`
- Access control: all commands and queries are decorated with `[RequiresModule("Breeding")]`
- Response contract: all handlers return `Result<T>` or `Result`

Direct joins to other modules are not allowed. Future maternity, insemination, diagnosis, and sanitary data should be integrated through module contracts or domain events, not cross-module EF navigation.

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
- `DnpDays`
- `ReproductiveStatus`
- `LifecycleStatus`
- `Location`
- `Events`

### Boar

Represents a breeding male (`Cachaço`) in the tenant's plantel.

Key fields mirror `Sow`, except reproductive cycle fields are not present in 2.1.

## Status Model

Phase 2.1 separates lifecycle state from reproductive state.

### LifecycleStatus

- `Active`: animal is available in the registry.
- `Quarantine`: animal requires attention and is highlighted in the UI.
- `Culled`: terminal state; domain methods reject later profile/status changes.

Allowed transitions:

- `Active -> Quarantine`
- `Quarantine -> Active`
- `Active -> Culled`
- `Quarantine -> Culled`

Rejected transition:

- Any transition from `Culled` to another state.

### ReproductiveStatus

Applied only to `Sow` in 2.1:

- `Empty` (`Vazia`)
- `Bred` (`Coberta`)
- `Pregnant` (`Gestante/Prenha`)
- `Lactating` (`Lactante/Maternidade`)

Automatic reproductive transitions are intentionally deferred to Phase 2.2, where insemination and pregnancy diagnosis commands will own those changes.

## Attention Rule

The UI shows the Stitch alert state when:

- `Sow.DnpDays >= 40`, or
- `LifecycleStatus == Quarantine`

This is a derived rule. No separate `Attention` status is persisted.

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

Expected failures return `Result.Failure(Error)` and are mapped to HTTP status codes:

- `Validation` -> `400`
- `NotFound` -> `404`
- `Conflict` -> `409`
- `Unauthorized` -> `403`

## Frontend

Stitch handoff assets are versioned under `docs/design/stitch/phase-2.1/`:

- `herd-mobile.html` / `herd-mobile.png`
- `registry-desktop.html` / `registry-desktop.png`
- `sow-profile.html` / `sow-profile.png`

Implemented routes:

- `/breeding/registry`
- `/breeding/sows/{id}`
- `/breeding/boars/{id}`

Reusable Blazor components:

- `StatusChip`
- `PlantelSearchBar`
- `OfflineBanner`
- `KpiStatCard`
- `LifecycleTimeline`

All pages are wrapped in `<ModuleGuard Module="Breeding">` and preserve the current Cria Certo design tokens in `app.css`.

## Offline Cache

The registry page stores online sow list snapshots in IndexedDB through `wwwroot/js/plantel-cache.js`.

Behavior:

1. Online list succeeds: update in-memory view and replace IndexedDB `sows` snapshot.
2. Online list fails: load filtered rows from IndexedDB.
3. IndexedDB is empty: show bundled fallback sample rows and the offline banner.

Offline write queueing is not enabled in 2.1. The cache is read-only and supports rural search/filter resilience for the registry list.

## Tests

`tests/Unit/CriaCerto.Modules.Breeding.UnitTests` covers lifecycle business rules:

- `Active -> Quarantine` succeeds.
- `Quarantine -> Active` succeeds for sows.
- `Culled` is terminal for sows and boars.
- DNP attention is derived for sows with `DnpDays >= 40`.

## Phase 2.1 Checklist

- [x] Backend endpoints/services created and returning `Result<T>`.
- [x] Domain logic covered by unit tests.
- [x] Blazor components implemented from Stitch handoff guidance.
- [x] Offline-capable search/filter via IndexedDB read cache.
- [x] Plan/module access controls applied through `[RequiresModule("Breeding")]` and `<ModuleGuard Module="Breeding">`.
- [x] Living documentation updated.
