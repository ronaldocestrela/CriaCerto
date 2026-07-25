# Module Specification: Cattle Herd & IATF Breeding (`Modules.Breeding`)

## 1. Overview
The `Modules.Breeding` module manages the reproductive lifecycle of beef and dairy cattle herds. It handles individual cow/matriz identification (Ear Tag / Brinco Amarelo, SISBOV, RFID, Tattoo), bull/semen inventory, Fixed-Time Artificial Insemination (IATF) protocols, pregnancy diagnosis, and core zootecnic indicators (Calving Interval - IEP, Open Days).

---

## 2. Core Entities & Aggregates

### 2.1 Cow (`Cow`)
* **Identification:** `EarTag` (Mandatory), `SisbovId` (Optional state traceability code), `RfidTag` (Electronic chip), `Tattoo`.
* **Reproductive Statuses:** `Open` (Vazia), `InIatfProtocol` (Em Protocolo IATF), `Inseminated` (Inseminada), `Pregnant` (Prenhe), `Culled` (Descartada), `Sold` (Vendida).
* **Parity & History:** `ParityCount` (Partos realizados), `LastCalvingDate`.

### 2.2 Bull & Semen Batch (`Bull`, `SemenBatch`)
* **Reprodutor/Touro:** Active breeding bull tracking with registration numbers.
* **Semen Batch:** Straw inventory control (`StrawQuantity`) by breed and type (`Conventional`, `SexedFemale`, `SexedMale`).

### 2.3 IATF Protocol (`IatfProtocol`)
* Synchronization batches tracking protocol name, start date, hormone insertion/withdrawal dates, insemination date, semen batch code, and assigned cow batch.

### 2.4 Pregnancy Diagnosis (`PregnancyDiagnosis`)
* Ultrasound or rectal palpation record, gestating status, gestational age in days, and diagnosis notes.

---

## 3. Zootecnic Calculations
* **Intervalo Entre Partos (IEP):** Measured in months between two consecutive calvings for a matriz ($IEP = \text{Dias} / 30.4375$).
* **Dias em Aberto (Open Days):** Days elapsed between the last calving and pregnancy confirmation.

---

## 4. API Endpoints
* `GET /api/breeding/cows` - List cows with status/search filters.
* `POST /api/breeding/cows` - Create new matriz.
* `POST /api/breeding/iatf-protocols` - Register IATF synchronization batch.
* `POST /api/breeding/diagnoses` - Record ultrasound or rectal palpation pregnancy diagnosis.
