# ADR 0003: Nutrition & Feed Conversion Analytics Strategy

## Status
Accepted

## Context
In bovine herd management (beef and dairy), feed costs account for 60% to 80% of total operational costs in feedlot (confinamento) and high-performance pasture supplementation regimes. Accurate calculation of Feed Conversion Ratio (CA), Dry Matter Intake (DMI / CMS), and Cost per Arroba (@) produced is essential for farm profitability.

## Decision
1. **Modular Isolation:** `Modules.Nutrition` operates as an independent module within the Modular Monolith, referencing `LotId` and `PaddockId` without direct EF Core cross-module database joins.
2. **Domain-Calculated Metrics:** All mathematical zootecnic calculations (CA, EA, Cost per Arroba) are implemented as pure domain service algorithms in `NutritionAnalyticsCalculator`, thoroughly tested via unit tests (TDD).
3. **Carcass Yield Normalization:** Cost per @ produced factors in Carcass Yield percentage (defaulting to 50% for standard weight gain, or custom % like 54% for feedlot finishing bulls).
4. **Offline Resilience:** Blazor PWA components support IndexedDB caching for trough reading logs (escore de cocho 0-3) and supplementation records in remote pastures with low or zero connectivity.

## Consequences
- Clean separation of concerns between animal growth tracking (`Modules.Growth`) and feed supply (`Modules.Nutrition`).
- High performance for real-time dashboard widgets without cross-schema locks.
