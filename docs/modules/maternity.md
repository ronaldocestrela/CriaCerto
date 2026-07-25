# Maternity Module (`Modules.Maternity`)

## Phase 3.1 — Farrowing & Piglet Registration (DONE)

Sub-phase 3.1 introduces the core capability of `Modules.Maternity`: the **Farrowing & Piglet Registration (`Partos`)**. It provides tenant-scoped farrowing records, live vs dead piglet count validation, average piglet weight calculation, touch-friendly mobile Blazor interface for field operators, IndexedDB rural offline queuing, and domain event emissions (`FarrowingCompletedEvent`).

---

## Module Boundaries

- **Module Name:** `Modules.Maternity`
- **Persistence Schema:** `Maternity`
- **Database:** Tenant database resolved dynamically by `ITenantConnectionProvider`
- **Access Control:** All commands and queries decorated with `[RequiresModule("Maternity")]`
- **Response Contract:** All application services and command/query handlers return `Result<T>` or `Result`
- **Cross-Module Isolation:** Direct database joins to `Modules.Breeding` or other modules are prohibited. Inter-module state changes occur via domain events (`FarrowingCompletedEvent`).

---

## Domain Aggregate & Invariants

### `Farrowing` (Parto)

Represents a birth event in the maternity barn.

#### Core Attributes:
- `Id` (Guid): Unique identifier
- `SowId` (Guid): Associated breeding female (Matriz)
- `TenantId` (Guid): Tenant context identifier
- `FarrowingDate` (DateTime): Date and time of farrowing
- `LiveBorn` (int): Number of piglets born alive (`Nascidos Vivos` >= 0)
- `Stillborn` (int): Number of stillborn piglets (`Natimortos` >= 0)
- `Mummified` (int): Number of mummified piglets (`Mumificados` >= 0)
- `TotalBorn` (int, Calculated): `LiveBorn + Stillborn + Mummified` (> 0)
- `LitterWeightKg` (decimal): Total weight of the live litter in kg
- `AveragePigletWeightKg` (decimal, Calculated): `LiveBorn > 0 ? LitterWeightKg / LiveBorn : 0`
- `MaternityRoomId` (string?): Maternity room/pen identifier (ex: `Sala-01`)
- `Assisted` (bool): Whether the farrowing was assisted by personnel
- `Notes` (string?): Additional notes or health observations

#### Business Invariants (Enforced via Result Pattern):
1. **Positive Total Count:** `TotalBorn` (`LiveBorn + Stillborn + Mummified`) must be strictly > 0. Rejects 0-piglet registrations (`FarrowingErrors.ZeroTotalBorn`).
2. **Non-Negative Counts:** `LiveBorn`, `Stillborn`, and `Mummified` cannot be negative (`FarrowingErrors.NegativeCounts`).
3. **Litter Weight Requirement:** If `LiveBorn > 0`, `LitterWeightKg` must be > 0 (`FarrowingErrors.InvalidLitterWeight`).
4. **Biological Plausibility:** Average weight per live piglet must be between **0.3 kg and 3.5 kg** (`FarrowingErrors.UnrealisticWeight`).

---

## Domain Events

### `FarrowingCompletedEvent`
Emitted when a farrowing is registered:
- `FarrowingId`, `SowId`, `TenantId`, `LiveBorn`, `Stillborn`, `Mummified`, `FarrowingDate`.
- Triggers asynchronous updates in `Modules.Breeding` (transitioning sow status to `Lactating`).

---

## API Endpoints

All endpoints require JWT authorization and active `Maternity` plan subscription.

| Method | Endpoint | Description | Response |
| :--- | :--- | :--- | :--- |
| `POST` | `/api/maternity/farrowings` | Registers a new farrowing event | `201 Created (FarrowingDto)` |
| `GET` | `/api/maternity/farrowings/{id}` | Gets details of a specific farrowing | `200 OK (FarrowingDto)` |
| `GET` | `/api/maternity/farrowings` | Lists farrowings (optional filters: `sowId`, `maternityRoomId`) | `200 OK (List<FarrowingSummaryDto>)` |

---

## Rural PWA & Offline Support

- **Touch Controls:** Large increment/decrement (+ / -) buttons optimized for glove/barn use.
- **Offline Storage:** Registered farrowings without internet connectivity are queued in `IndexedDB` / `localStorage` (`maternity_pending_farrowings`).
- **Auto-Sync:** Background worker & UI status chip detect connection recovery and flush pending entries to `/api/maternity/farrowings`.

---

## Phase Sign-Off Checklist (Phase 3.1)

- [x] Backend .NET 10 endpoints & services returning `Result<T>`.
- [x] Domain logic & invariants covered by unit tests (xUnit / FluentAssertions).
- [x] Blazor components generated with MCP Stitch mobile-first layout.
- [x] Rural offline capability & LocalStorage/IndexedDB queueing verified.
- [x] Plan access controls `[RequiresModule("Maternity")]` and `<ModuleGuard Module="Maternity">` active.
- [x] Living documentation (`/docs/modules/maternity.md`) updated.
