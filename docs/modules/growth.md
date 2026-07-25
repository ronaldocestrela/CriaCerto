# Module Specification: Growth, Pasture Management & Stocking Rate (`Modules.Growth`)

## 1. Overview
The `Modules.Growth` module manages animal lots, pasture paddocks (piquetes), animal movements between paddocks, and zootecnic stocking rate indicators (Taxa de Lotação em Unidades Animais por Hectare - UA/ha).

---

## 2. Domain Models & Rules

### 2.1 Pasture Paddock (`PasturePaddock`)
* **Identification:** `Name`, `Code` (Identificador do Piquete).
* **Metrics:** `AreaHectares` (Área em Hectares), `MaxCapacityUA` (Capacidade Máxima Suportável em UA).
* **Status:** `Active` (Em pastejo), `Resting` (Pousio/Descanso), `Maintenance` (Reforma de pastagem).

### 2.2 Animal Lot (`Lot`)
* **Category:** `Bezerros`, `Recria`, `Garrotes`, `Engorda`, `Matrizes`, `Reprodutores`.
* **Zootecnic Load (UA):**
  * Standard Unit: $1\text{ UA} = 450\text{ kg}$ of live weight.
  * $$\text{Total Weight (kg)} = \text{HeadCount} \times \text{AverageWeightKg}$$
  * $$\text{Total UA} = \frac{\text{Total Weight (kg)}}{450.0}$$
* **Status:** `Active`, `Closed`.

### 2.3 Stocking Rate & Overgrazing Alert
* **Stocking Rate Equation:**
  $$\text{Taxa de Lotação (UA/ha)} = \frac{\sum \text{UA dos lotes alocados no piquete}}{\text{Área do Piquete (ha)}}$$
* **Overgrazing Warning:** Triggered when total allocated UA exceeds `MaxCapacityUA` or when lots are placed in a paddock with status `Resting` or `Maintenance`.

---

## 3. API Endpoints
* `GET /api/growth/paddocks?tenantId={id}` - List paddocks with current stocking rate metrics.
* `POST /api/growth/paddocks` - Register a new pasture paddock.
* `GET /api/growth/lots?tenantId={id}` - List animal lots.
* `POST /api/growth/lots` - Create a new animal lot.
* `POST /api/growth/lots/move` - Move a lot to a destination paddock (records history).
* `POST /api/growth/lots/{id}/close` - Close an active lot.

---

## 4. Field Offline Capability
* Pasture lot movements performed in remote paddocks are cached in client `IndexedDB` when network connection is unavailable and synchronized automatically when connectivity is re-established.
