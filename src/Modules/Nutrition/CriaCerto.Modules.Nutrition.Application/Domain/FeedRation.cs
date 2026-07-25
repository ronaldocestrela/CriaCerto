using CriaCerto.BuildingBlocks.Abstractions.Results;

namespace CriaCerto.Modules.Nutrition.Application.Domain;

public record FeedRationItemInput(Guid FeedItemId, string FeedItemName, decimal Percentage, decimal UnitCostPerKg);

public class FeedRationItem
{
    public Guid Id { get; private set; }
    public Guid FeedRationId { get; private set; }
    public Guid FeedItemId { get; private set; }
    public string FeedItemName { get; private set; } = default!;
    public decimal Percentage { get; private set; }
    public decimal UnitCostPerKg { get; private set; }

    private FeedRationItem() { }

    internal static FeedRationItem Create(Guid feedRationId, FeedRationItemInput input)
    {
        return new FeedRationItem
        {
            Id = Guid.NewGuid(),
            FeedRationId = feedRationId,
            FeedItemId = input.FeedItemId,
            FeedItemName = input.FeedItemName,
            Percentage = input.Percentage,
            UnitCostPerKg = input.UnitCostPerKg
        };
    }
}

public class FeedRation
{
    public Guid Id { get; private set; }
    public Guid TenantId { get; private set; }
    public string Name { get; private set; } = default!;
    public RationType RationType { get; private set; }
    public decimal DryMatterPercentage { get; private set; }
    public decimal CalculatedCostPerKg { get; private set; }
    public DateTime CreatedAt { get; private set; }

    private readonly List<FeedRationItem> _items = new();
    public IReadOnlyCollection<FeedRationItem> Items => _items.AsReadOnly();

    private FeedRation() { }

    public static Result<FeedRation> Create(
        Guid tenantId,
        string name,
        RationType rationType,
        decimal dryMatterPercentage,
        IEnumerable<FeedRationItemInput> items)
    {
        if (tenantId == Guid.Empty)
            return Result.Failure<FeedRation>(Error.Validation("FeedRation.InvalidTenant", "O Id do Tenant é obrigatório."));

        if (string.IsNullOrWhiteSpace(name))
            return Result.Failure<FeedRation>(Error.Validation("FeedRation.EmptyName", "O nome da receita de ração é obrigatório."));

        if (dryMatterPercentage <= 0 || dryMatterPercentage > 100)
            return Result.Failure<FeedRation>(Error.Validation("FeedRation.InvalidDryMatter", "O teor de matéria seca (%) deve estar entre 1 e 100%."));

        var itemList = items?.ToList() ?? new List<FeedRationItemInput>();
        if (!itemList.Any())
            return Result.Failure<FeedRation>(Error.Validation("FeedRation.EmptyItems", "A receita deve conter pelo menos um ingrediente."));

        decimal totalPercentage = itemList.Sum(i => i.Percentage);
        if (Math.Abs(totalPercentage - 100m) > 0.01m)
            return Result.Failure<FeedRation>(Error.Validation("FeedRation.InvalidProportions", $"A soma das proporções dos ingredientes deve ser 100%. Soma atual: {totalPercentage:N2}%."));

        var ration = new FeedRation
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            Name = name.Trim(),
            RationType = rationType,
            DryMatterPercentage = dryMatterPercentage,
            CreatedAt = DateTime.UtcNow
        };

        decimal weightedCost = 0m;
        foreach (var itemInput in itemList)
        {
            var item = FeedRationItem.Create(ration.Id, itemInput);
            ration._items.Add(item);
            weightedCost += (itemInput.Percentage / 100m) * itemInput.UnitCostPerKg;
        }

        ration.CalculatedCostPerKg = Math.Round(weightedCost, 4);

        return Result.Success(ration);
    }
}
