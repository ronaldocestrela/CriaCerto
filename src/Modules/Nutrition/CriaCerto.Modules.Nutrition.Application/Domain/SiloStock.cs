using CriaCerto.BuildingBlocks.Abstractions.Results;

namespace CriaCerto.Modules.Nutrition.Application.Domain;

public class SiloStock
{
    public Guid Id { get; private set; }
    public Guid TenantId { get; private set; }
    public string Name { get; private set; } = default!;
    public FeedCategory Category { get; private set; }
    public decimal CurrentStockKg { get; private set; }
    public decimal UnitCostPerKg { get; private set; }
    public decimal DryMatterPercentage { get; private set; }
    public decimal MinimumThresholdKg { get; private set; }
    public DateTime LastRestockedAt { get; private set; }

    private SiloStock() { }

    public static Result<SiloStock> Create(
        Guid tenantId,
        string name,
        FeedCategory category,
        decimal initialStockKg,
        decimal unitCostPerKg,
        decimal dryMatterPercentage,
        decimal minimumThresholdKg)
    {
        if (tenantId == Guid.Empty)
            return Result.Failure<SiloStock>(Error.Validation("SiloStock.InvalidTenant", "O Id do Tenant é obrigatório."));

        if (string.IsNullOrWhiteSpace(name))
            return Result.Failure<SiloStock>(Error.Validation("SiloStock.EmptyName", "O nome do insumo é obrigatório."));

        if (initialStockKg < 0)
            return Result.Failure<SiloStock>(Error.Validation("SiloStock.NegativeStock", "O estoque inicial não pode ser negativo."));

        if (unitCostPerKg < 0)
            return Result.Failure<SiloStock>(Error.Validation("SiloStock.NegativeCost", "O custo unitário por kg não pode ser negativo."));

        if (dryMatterPercentage <= 0 || dryMatterPercentage > 100)
            return Result.Failure<SiloStock>(Error.Validation("SiloStock.InvalidDryMatter", "O teor de matéria seca (%) deve estar entre 1 e 100%."));

        var silo = new SiloStock
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            Name = name.Trim(),
            Category = category,
            CurrentStockKg = initialStockKg,
            UnitCostPerKg = unitCostPerKg,
            DryMatterPercentage = dryMatterPercentage,
            MinimumThresholdKg = Math.Max(0, minimumThresholdKg),
            LastRestockedAt = DateTime.UtcNow
        };

        return Result.Success(silo);
    }

    public Result Restock(decimal addedKg, decimal newUnitCostPerKg)
    {
        if (addedKg <= 0)
            return Result.Failure(Error.Validation("SiloStock.InvalidRestockKg", "A quantidade a adicionar deve ser maior que zero."));

        if (newUnitCostPerKg < 0)
            return Result.Failure(Error.Validation("SiloStock.InvalidCost", "O novo custo unitário por kg deve ser maior ou igual a zero."));

        decimal totalValueBefore = CurrentStockKg * UnitCostPerKg;
        decimal totalValueAdded = addedKg * newUnitCostPerKg;
        decimal newTotalStockKg = CurrentStockKg + addedKg;

        CurrentStockKg = newTotalStockKg;
        UnitCostPerKg = newTotalStockKg > 0 ? (totalValueBefore + totalValueAdded) / newTotalStockKg : newUnitCostPerKg;
        LastRestockedAt = DateTime.UtcNow;

        return Result.Success();
    }

    public Result ConsumeStock(decimal kg)
    {
        if (kg <= 0)
            return Result.Failure(Error.Validation("SiloStock.InvalidConsumeAmount", "A quantidade consumida deve ser maior que zero."));

        if (kg > CurrentStockKg)
            return Result.Failure(Error.Validation("SiloStock.InsufficientStock", $"Estoque insuficiente no silo {Name}. Saldo atual: {CurrentStockKg:N1}kg, Solicitado: {kg:N1}kg."));

        CurrentStockKg -= kg;
        return Result.Success();
    }
}
