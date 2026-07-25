# Modules.Nutrition: Supplementation, Feedlot TMR & Cost per Arroba (@) Analytics

## Overview
The `Modules.Nutrition` domain handles cattle nutrition management across pasture and feedlot (confinamento) environments. It calculates crucial zootecnic KPIs such as **Feed Conversion Ratio (CA)**, **Feed Efficiency (EA)**, and **Cost per Arroba (@) Produced ($/@)**.

---

## Domain Entities & Enums

### Entities
- **`SiloStock`**: Represents bulk grains, silage, mineral salts, and additive inventories in farm silos. Tracks stock level in kg, weighted average cost per kg, dry matter percentage (MS %), and minimum reorder threshold.
- **`FeedRation`**: Total mixed ration (TMR) or pasture supplement formulations. Requires ingredient percentage total equal to 100%. Calculates weighted cost per kg automatically.
- **`PastureSupplementation`**: Field log for mineral salt or protein supplement distribution in pasture paddocks. Calculates intake in grams per head per day (`g/cab/dia`).
- **`DailyFeedBatch`**: Feedlot TMR delivery log per pen/lot. Includes offered weight in As-Fed (MN) and Dry Matter (MS), alongside Trough Reading Score (Escore de Cocho 0 to 3).

### Enums
- **`FeedCategory`**: `BulkGrain`, `ForageSilage`, `MineralSalt`, `Additive`.
- **`RationType`**: `PastureSupplement`, `FeedlotTmr`, `Transition`.
- **`TroughScore`**:
  - `Score0_Clean` (0 - Cocho Limpo / Fome)
  - `Score1_ThinLayer` (1 - Lâmina Fina / Ideal)
  - `Score2_Excessive` (2 - Sobra Excessiva / Reduzir)
  - `Score3_Untouched` (3 - Trato Intacto / Rejeição)

---

## Calculations & Zootecnic Equations

### 1. Feed Conversion Ratio (CA / Conversão Alimentar)
$$\text{CA} = \frac{\text{Total Dry Matter Intake (kg MS)}}{\text{Total Weight Gain (kg)}}$$

### 2. Feed Efficiency (EA / Eficiência Alimentar)
$$\text{EA} = \frac{\text{Total Weight Gain (kg)}}{\text{Total Dry Matter Intake (kg MS)}}$$

### 3. Cost per Arroba (@) Produced
$$\text{Arrobas Produced (@)} = \frac{\text{Total Weight Gain (kg)} \times \text{Carcass Yield \%}}{15\text{ kg}}$$
$$\text{Cost per Arroba (\$/@)} = \frac{\text{Total Nutrition Cost (\$)}}{\text{Arrobas Produced (@)}}$$

---

## API Endpoints

- `GET /api/nutrition/silos`: Get list of silos and stocks per tenant.
- `POST /api/nutrition/silos`: Create a new silo/stock item.
- `POST /api/nutrition/silos/restock`: Restock an existing silo with new quantity and acquisition unit cost.
- `GET /api/nutrition/rations`: List ration formulations.
- `POST /api/nutrition/rations`: Create a new ration recipe.
- `POST /api/nutrition/supplementation`: Log mineral/protein supplementation delivery in pasture.
- `POST /api/nutrition/tmr-batches`: Log TMR feed batch delivery in feedlot pen.
- `GET /api/nutrition/analytics/feed-conversion`: Query Feed Conversion Ratio (CA) and Efficiency (EA).
- `GET /api/nutrition/analytics/cost-per-arroba`: Query Cost per Arroba Produced ($/@).
