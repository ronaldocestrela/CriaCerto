# System Instructions & Agent Guide: Swine Management SaaS Platform

## 1. Project Overview & Vision
This project is a high-performance, enterprise-grade, multi-tenant Swine Management SaaS platform (competing with legacy systems like *MeuSuino* / *SOS Suínos*). 

The platform optimizes swine farm operations across the entire lifecycle: Breeding/Sow management, Gestation, Farrowing/Maternity, Nursery, Growing, Finishing, Feed/Nutrition management, Sanitary/Veterinary workflows, and Advanced Zootecnic Analytics (DNP, NVMA, DMA, FCR/CA, ADG/GPD).

---

## 2. Architecture & Design Principles

### 2.1 Architectural Style: Modular Monolith
* **Structure:** Single unit composed of strictly separated, domain-aligned internal modules.
* **Communication:** In-process domain events and mediator patterns. Direct cross-module database joins are **prohibited**.
* **Tiered Feature Gating:** Modules are dynamically enabled/disabled based on tenant subscription plans (e.g., *Starter* = Breeding & Nursery; *Enterprise* = Full Zootecnic Analytics + Financials + Feed Optimization).
* **Tenant Context Resolution:** Tenant must be resolved from authenticated user membership/claims during login and request authorization, without subdomain/header-based routing.

### 2.2 Core Architectural Requirements
* **Backend:** .NET 10 (C# 14) following Clean Architecture & Domain-Driven Design (DDD).
* **Frontend:** Blazor WebAssembly / Blazor Web App targeting .NET 10 with **PWA (Progressive Web App)** capabilities for offline resilience in rural environments.
* **Design & UI Automation:** **MCP Stitch** integration for automated, standardized Blazor UI component generation.
* **Development Methodology:** **Test-Driven Development (TDD)** — Red/Green/Refactor mandatory for all business code.
* **Documentation Strategy:** **Living Documentation** — Automated generation and execution of specs via BDD/Tests. Architecture Decision Records (ADRs) and markdown docs must be kept up-to-date with code changes.
* **Error & Response Pattern:** Mandatory **Result Pattern** (`Result<T>`) across all Backend layers — **NO Exceptions for Control Flow**.

---

## 3. Technology Stack & Specifications

### 3.1 Backend (.NET 10)
* **Framework:** .NET 10 Web API / Modular Monolith Architecture.
* **Language:** C# 14.
* **Domain & CQRS:** MediatR / In-Memory Channel Bus.
* **Data Access:** Entity Framework Core 10, SQL Server with strict database-per-tenant isolation (one logical database per tenant) and per-tenant connection resolution.
* **Functional Error Handling:** Custom or CSharpFunctionalExtensions `Result<T>` / `Result<T, Error>`.
* **Validation:** FluentValidation integrated into MediatR pipeline.
* **Testing Stack:** xUnit, FluentAssertions, NSubstitute / Moq, Testcontainers (SQL Server integration tests).

### 3.2 Frontend (Blazor .NET 10)
* **Framework:** Blazor Web App / WASM .NET 10 with PWA offline caching strategy.
* **UI Components:** Reusable atomic component design system. Integration with **MCP Stitch** for component synthesis and UI standardization.
* **Offline Storage:** IndexedDB (via Blazored.LocalStorage or JS Interop) with IndexedDB-to-Server background synchronization workers.
* **State Management:** Fluxor or Custom Scoped State Stores.

---

## 4. Mandatory Backend Standard: The Result Pattern

Exceptions MUST NOT be used for domain validation or expected business rule failures. All application services, command handlers, query handlers, and domain methods must return `Result` or `Result<T>`.

### Key Interfaces & Base Types
```csharp
public record Error(string Code, string Message, ErrorType Type)
{
    public static readonly Error None = new(string.Empty, string.Empty, ErrorType.Failure);
    public static Error NotFound(string code, string description) => new(code, description, ErrorType.NotFound);
    public static Error Validation(string code, string description) => new(code, description, ErrorType.Validation);
    public static Error Conflict(string code, string description) => new(code, description, ErrorType.Conflict);
}

public enum ErrorType
{
    Failure,
    Validation,
    NotFound,
    Conflict,
    Unauthorized
}

public class Result
{
    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public Error Error { get; }

    protected Result(bool isSuccess, Error error)
    {
        if (isSuccess && error != Error.None || !isSuccess && error == Error.None)
            throw new InvalidOperationException("Invalid Result state.");

        IsSuccess = isSuccess;
        Error = error;
    }

    public static Result Success() => new(true, Error.None);
    public static Result Failure(Error error) => new(false, error);
    public static Result<TValue> Success<TValue>(TValue value) => new(value, true, Error.None);
    public static Result<TValue> Failure<TValue>(Error error) => new(default, false, error);
}

public class Result<TValue> : Result
{
    private readonly TValue? _value;

    protected internal Result(TValue? value, bool isSuccess, Error error)
        : base(isSuccess, error)
    {
        _value = value;
    }

    public TValue Value => IsSuccess 
        ? _value! 
        : throw new InvalidOperationException("The value of a failure result cannot be accessed.");
}
```

---

## 5. Domain Modules Specification

### 5.1 Breeding & Sow Management Module (`Modules.Breeding`)
* **Entities:** `Sow` (Matriz), `Boar` (Cachaço), `BreedingEvent` (Cobertura/IA), `SemenBatch`.
* **Features:**
  * Artificial Insemination / Natural Service tracking.
  * Pregnancy diagnosis (ultrasound / return-to-estrus tracking).
  * Sow lifecycle metrics: DNP (Non-Productive Days), IDC (Interval Desmame-Cobertura).

### 5.2 Maternity & Farrowing Module (`Modules.Maternity`)
* **Entities:** `Farrowing` (Parto), `Litter` (Ninhada), `PigletTransfer` (Adoção/Transferência), `Weaning` (Desmame).
* **Features:**
  * Live born, stillborn, mummified piglet count.
  * Cross-fostering (piglet transfers between sows).
  * Weaning events & weight recording.
  * Metrics: NVMA (Nascidos Vivos/Matriz/Ano), DMA (Desmamados/Matriz/Ano), Pre-weaning mortality.

### 5.3 Batch & Growth Management Module (`Modules.Growth`)
* **Entities:** `Batch` (Lote), `Movement`, `Weighing`, `MortalityRecord`.
* **Features:**
  * Stages: Nursery (Creche), Growing/Finishing (Recria e Terminação).
  * Daily Weight Gain (GPD/ADG) calculation.
  * Feed Conversion Ratio (CA/FCR) analysis per batch.
  * Cull & mortality tracking with cause taxonomy.

### 5.4 Nutrition & Feed Module (`Modules.Nutrition`)
* **Entities:** `FeedFormula`, `FeedConsumption`, `SiloStock`.
* **Features:**
  * Stage-specific feeding schedules.
  * Consumption vs. theoretical growth model comparisons.
  * Cost per kg of meat produced.

### 5.5 Sanitary & Veterinary Module (`Modules.Sanitary`)
* **Entities:** `VaccinationPlan`, `TreatmentRecord`, `Medication`.
* **Features:**
  * Age/Phase automated vaccination schedule.
  * Animal/Batch treatment logs with withdrawal period (período de carência) warnings.

### 5.6 Tenant & Licensing Module (`Modules.Tenancy`)
* **Entities:** `Tenant`, `SubscriptionPlan`, `ModuleAccess`.
* **Features:**
  * Authentication-driven tenant resolution (user linked to tenant), with tenant context loaded from identity claims.
  * Middleware/pipeline for module availability verification per tenant.
  * Feature toggling and usage limits (e.g., max sows supported per plan).

---

## 6. Software Engineering Practices & AI Agent Guidelines

### 6.1 TDD Workflow (Mandatory Enforcement)
Every feature implementation must strictly adhere to the TDD cycle:
1. **RED:** Write unit/integration tests defining expected behavior (e.g., verifying `Result.Failure` when input violates business rules).
2. **GREEN:** Write minimal production code required to pass tests.
3. **REFACTOR:** Clean up code, enforce design patterns, ensure module isolation.

### 6.2 Living Documentation & Documentation Drift Prevention
* **Living Documentation Rules:**
  * When adding or modifying a API endpoint, domain event, or business rule, you **MUST update the relevant markdown files** in `/docs/` during the same commit/iteration.
  * Maintain executable specifications using SpecFlow / Reqnroll or self-documenting xUnit tests.
  * Keep ADRs (Architecture Decision Records) updated in `/docs/adrs/`.

### 6.3 Frontend Development & MCP Stitch Protocols
* **Component Architecture:** All Blazor components must be modular, highly granular, and stateless where possible (smart parent / dumb child pattern).
* **MCP Stitch Integration:** Use MCP Stitch to generate consistent, accessible HTML/CSS component structures before binding them into Blazor `.razor` components.
* **PWA & Rural Offline First:**
  * Maternidade, Creche, and Parto registries must be fully operational offline.
  * Offline actions must be queued in `IndexedDB` and processed through a background worker once connection is restored.

---

## 7. AI Agent Guidelines for Output Generation

When writing code as an AI assistant for this repository:
1. **Never throw exceptions for validation or business domain errors.** Always return `Result.Failure(Error)`.
2. **Check module boundaries.** Do not reference cross-module DB entities directly.
3. **Always write unit tests first or alongside code.**
4. **Update `agents.md` or related module documentation** whenever architectural decisions or business domain rules are updated.
