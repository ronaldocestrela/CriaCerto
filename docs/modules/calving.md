# Module Specification: Calving Operations & Calf Nursery (`Modules.Calving`)

## 1. Overview
The `Modules.Calving` module tracks bovine births (partos de vacas), newborn calf identification (bezerreiro), weaning operations, and normalized weight indicators (205-Day Adjusted Weight - P205).

---

## 2. Domain Models

### 2.1 Calving (`Calving`)
* **Mother Cow ID:** Associated matriz.
* **Calving Type:** `Normal`, `Dystocic` (Distócico), `Cesarean` (Cesariana).
* **Birth Condition:** `Live` (Nascido Vivo), `Stillborn` (Nado Morto).

### 2.2 Calf (`Calf`)
* **Identification:** `TagId` (Brinco), `Sex` (M/F), `Breed`, `BirthWeightKg`.
* **Status:** `Unweaned` (Mamando), `Weaned` (Desmamado), `Deceased` (Óbito).

### 2.3 Weaning & P205 Adjustment (`Weaning`)
* **Weaning Weight:** Recorded weight at weaning (typically around 7 months / 205 days of age).
* **205-Day Adjusted Weight (P205):** Standard zootecnic index normalized across the herd:
  $$P205 = \left( \frac{\text{WeaningWeight} - \text{BirthWeight}}{\text{AgeInDays}} \times 205 \right) + \text{BirthWeight} \times \text{MotherAgeFactor}$$
* **Mother Age Correction Factor:**
  * Age $\le 3$ years (Primípara): $1.15$
  * Age $4 - 10$ years (Adulta): $1.00$
  * Age $> 10$ years (Idosa): $1.05$

---

## 3. API Endpoints
* `POST /api/calving/calvings` - Register new bovine birth & calf.
* `POST /api/calving/weanings` - Register weaning with P205 calculation.
