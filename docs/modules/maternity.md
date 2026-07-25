# Maternity Module (`Modules.Maternity`)

## Phase 3.1 — Farrowing & Piglet Registration (DONE)

Sub-phase 3.1 introduces the core capability of `Modules.Maternity`: the **Farrowing & Piglet Registration (`Partos`)**. It provides tenant-scoped farrowing records, live vs dead piglet count validation, average piglet weight calculation, touch-friendly mobile Blazor interface for field operators, IndexedDB rural offline queuing, and domain event emissions (`FarrowingCompletedEvent`).

---

## Phase 3.2 — Cross-Fostering & Weaning (DONE)

Sub-phase 3.2 expands `Modules.Maternity` with **Cross-Fostering (Adoções e Transferências entre Matrizes)**, **Weaning (Desmame por Matriz)**, e **Métricas Zootécnicas Chave (NVMA e DMA)**:

- **Cross-Fostering:** Transferência auditável de leitões entre ninhadas/matrizes com validação estrita de inventário disponível.
- **Desmame:** Encerramento do ciclo de lactação com registro de contagem desmamada, peso total, peso médio calculado e destinação de lote/baia para a Creche (Fase 4).
- **Métricas Zootécnicas:** Cálculos automatizados de **NVMA** (*Nascidos Vivos/Matriz/Ano*), **DMA** (*Desmamados/Matriz/Ano*) e % de **Mortalidade Pré-Desmame**.

---

## Module Boundaries

- **Module Name:** `Modules.Maternity`
- **Persistence Schema:** `Maternity`
- **Database:** Tenant database resolved dynamically by `ITenantConnectionProvider`
- **Access Control:** All commands and queries decorated with `[RequiresModule("Maternity")]`
- **Response Contract:** All application services and command/query handlers return `Result<T>` or `Result`
- **Cross-Module Isolation:** Direct database joins to `Modules.Breeding` or other modules are prohibited. Inter-module state changes occur via domain events (`FarrowingCompletedEvent`, `PigletTransferredEvent`, `WeaningCompletedEvent`).

---

## Domain Aggregates & Invariants

### 1. `Farrowing` (Parto)
Represents a birth event in the maternity barn.

#### Business Invariants:
1. **Positive Total Count:** `TotalBorn` (`LiveBorn + Stillborn + Mummified`) must be strictly > 0.
2. **Non-Negative Counts:** `LiveBorn`, `Stillborn`, and `Mummified` cannot be negative.
3. **Litter Weight Requirement:** If `LiveBorn > 0`, `LitterWeightKg` must be > 0.
4. **Biological Plausibility:** Average weight per live piglet must be between **0.3 kg and 3.5 kg**.

### 2. `PigletTransfer` (Adoção / Transferência)
Represents a transfer of live piglets between two active farrowing litters.

#### Business Invariants:
1. **Source & Target Distinction:** Source farrowing cannot be equal to target farrowing (`FarrowingErrors.SameSourceAndTarget`).
2. **Positive Transfer Quantity:** Quantity transferred must be strictly > 0 (`FarrowingErrors.InvalidTransferQuantity`).
3. **Inventory Integrity:** Source litter must have sufficient live piglets available (`LiveBorn` + net transfers - previous weanings) (`FarrowingErrors.InsufficientPigletsInLitter`).

### 3. `Weaning` (Desmame)
Represents the weaning event of a litter/sow.

#### Business Invariants:
1. **Positive Weaned Count & Weight:** `WeanedCount` > 0 and `TotalWeanedWeightKg` > 0 (`FarrowingErrors.InvalidWeaningCount`, `FarrowingErrors.InvalidWeanedWeight`).
2. **Biological Weight Bounds:** Average weaned weight per piglet must be between **4.0 kg and 12.0 kg** (`FarrowingErrors.UnrealisticWeanedWeight`).
3. **Single Weaning Enforcement:** A farrowing can only be weaned once (`FarrowingErrors.FarrowingAlreadyWeaned`).

---

## Domain Events

### `FarrowingCompletedEvent`
Emitted when a farrowing is registered:
- Triggers asynchronous updates in `Modules.Breeding` (transitioning sow status to `Lactating`).

### `PigletTransferredEvent`
Emitted when piglets are transferred between sows/litters:
- `TransferId`, `TenantId`, `SourceFarrowingId`, `TargetFarrowingId`, `Quantity`, `TransferDate`.

### `WeaningCompletedEvent`
Emitted when a litter is weaned:
- `WeaningId`, `TenantId`, `FarrowingId`, `SowId`, `WeanedCount`, `TotalWeanedWeightKg`, `DestinationPenOrBatch`.

---

## API Endpoints

All endpoints require JWT authorization and active `Maternity` plan subscription.

| Method | Endpoint | Description | Response |
| :--- | :--- | :--- | :--- |
| `POST` | `/api/maternity/farrowings` | Registers a new farrowing event | `201 Created (FarrowingDto)` |
| `GET` | `/api/maternity/farrowings/{id}` | Gets details of a specific farrowing | `200 OK (FarrowingDto)` |
| `GET` | `/api/maternity/farrowings` | Lists farrowings (optional filters: `sowId`, `maternityRoomId`) | `200 OK (List<FarrowingSummaryDto>)` |
| `POST` | `/api/maternity/transfers` | Registers a cross-fostering transfer between litters | `201 Created (PigletTransferDto)` |
| `GET` | `/api/maternity/transfers` | Lists piglet transfers (optional filter: `farrowingId`) | `200 OK (List<PigletTransferDto>)` |
| `POST` | `/api/maternity/weanings` | Registers a weaning event for a litter | `201 Created (WeaningDto)` |
| `GET` | `/api/maternity/weanings` | Lists weaning records (optional filter: `sowId`) | `200 OK (List<WeaningDto>)` |
| `GET` | `/api/maternity/metrics` | Calculates NVMA, DMA, and pre-weaning mortality rate | `200 OK (MaternityMetricsDto)` |

---

## Rural PWA & Offline Support

- **Touch Controls:** Touch-friendly (+ / -) counter controls for quick field data entry.
- **Offline Storage:** Farrowings and adoption entries are queued in `localStorage`/`IndexedDB` (`maternity_pending_farrowings`).
- **Auto-Sync:** Background sync worker & status banner detect connection recovery and flush pending entries to `/api/maternity/farrowings`.

---

## Phase Sign-Off Checklist (Sub-phase 3.2)

- [x] Backend .NET 10 endpoints & services returning `Result<T>`.
- [x] Domain logic & invariants covered by unit tests (xUnit / FluentAssertions).
- [x] Blazor components generated with MCP Stitch mobile-first layout.
- [x] Rural offline capability & LocalStorage/IndexedDB queueing verified.
- [x] Plan access controls `[RequiresModule("Maternity")]` and `<ModuleGuard Module="Maternity">` active.
- [x] Living documentation (`/docs/modules/maternity.md`) updated.
