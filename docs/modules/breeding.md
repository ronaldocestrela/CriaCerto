# Module Specification: Cattle Herd & IATF Breeding (`Modules.Breeding`)

## 1. Overview
The `Modules.Breeding` module manages the reproductive lifecycle of beef and dairy cattle herds. It handles individual cow/matriz identification (Ear Tag / Brinco Amarelo, SISBOV, RFID, Tattoo), bull/semen inventory, Fixed-Time Artificial Insemination (IATF) protocols, pregnancy diagnosis, and core zootecnic indicators (Calving Interval - IEP, Open Days).

---

## 2. Core Entities & Aggregates

### 2.1 Cow / Bovine (`Cow`)
* **Identification:** `EarTag` (Mandatory ear tag ID), `SisbovId` (Optional state traceability code), `RfidTag` (Electronic chip), `Tattoo` (Ear tattoo), `Nickname` (Alcunha/Apelido do animal), `RegistryNumber` (Registro de Associação de Raça PBB).
* **Demographics & Origin:** `Breed` (Raça), `Category` (Matriz/Fêmea, Reprodutor/Touro, Bezerro/a, Novilho/Garrote), `Origin` (Nascimento Interno, Compra/Aquisição, Transferência), `BirthDate`, `EntryDate` (Data de entrada no rebanho), `EntryWeightKg` (Peso inicial de entrada em kg).
* **Genealogy & Condition:** `SireInfo` (Pai / Código do Sêmen), `DamInfo` (Mãe / Matriz), `BodyConditionScore` (ECC - Escore de Condição Corporal de 1.0 a 5.0).
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
* **Escore de Condição Corporal (ECC):** Visual/palpation score from 1.0 (muito magra) to 5.0 (obesa).

---

## 4. API Endpoints
* `GET /api/breeding/cows` - List cows/bovines with status, search filter (EarTag, Nickname, SISBOV, RFID) and pagination.
* `POST /api/breeding/cows` - Create new matriz/bovine with complete MVP fields and tenant ear tag uniqueness check.
* `GET /api/breeding/cows/{id:guid}` - Get detailed animal profile (`CowDetailDto`) including unified timeline events (`TimelineEventDto`).
* `PUT /api/breeding/cows/{id:guid}` - Update animal zootecnic attributes.
* `POST /api/breeding/iatf-protocols` - Register IATF synchronization batch.
* `POST /api/breeding/diagnoses` - Record ultrasound or rectal palpation pregnancy diagnosis.
