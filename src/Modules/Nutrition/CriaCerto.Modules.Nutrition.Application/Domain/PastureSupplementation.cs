using CriaCerto.BuildingBlocks.Abstractions.Results;

namespace CriaCerto.Modules.Nutrition.Application.Domain;

public class PastureSupplementation
{
    public Guid Id { get; private set; }
    public Guid TenantId { get; private set; }
    public Guid PaddockId { get; private set; }
    public Guid LotId { get; private set; }
    public Guid FeedRationId { get; private set; }
    public DateTime DistributionDate { get; private set; }
    public decimal QuantityKg { get; private set; }
    public int HeadCount { get; private set; }
    public decimal CalculatedIntakeGramsPerHead { get; private set; }

    private PastureSupplementation() { }

    public static Result<PastureSupplementation> Create(
        Guid tenantId,
        Guid paddockId,
        Guid lotId,
        Guid feedRationId,
        DateTime distributionDate,
        decimal quantityKg,
        int headCount)
    {
        if (tenantId == Guid.Empty)
            return Result.Failure<PastureSupplementation>(Error.Validation("PastureSupplementation.InvalidTenant", "O Id do Tenant é obrigatório."));

        if (paddockId == Guid.Empty)
            return Result.Failure<PastureSupplementation>(Error.Validation("PastureSupplementation.InvalidPaddock", "O Piquete de destino é obrigatório."));

        if (lotId == Guid.Empty)
            return Result.Failure<PastureSupplementation>(Error.Validation("PastureSupplementation.InvalidLot", "O Lote de animais é obrigatório."));

        if (feedRationId == Guid.Empty)
            return Result.Failure<PastureSupplementation>(Error.Validation("PastureSupplementation.InvalidRation", "O Suplemento/Ração é obrigatório."));

        if (quantityKg <= 0)
            return Result.Failure<PastureSupplementation>(Error.Validation("PastureSupplementation.InvalidQuantity", "A quantidade em kg deve ser maior que zero."));

        if (headCount <= 0)
            return Result.Failure<PastureSupplementation>(Error.Validation("PastureSupplementation.InvalidHeadCount", "O número de cabeças deve ser maior que zero."));

        decimal intakeGramsPerHead = (quantityKg * 1000m) / headCount;

        var entity = new PastureSupplementation
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            PaddockId = paddockId,
            LotId = lotId,
            FeedRationId = feedRationId,
            DistributionDate = distributionDate == default ? DateTime.UtcNow : distributionDate,
            QuantityKg = quantityKg,
            HeadCount = headCount,
            CalculatedIntakeGramsPerHead = Math.Round(intakeGramsPerHead, 2)
        };

        return Result.Success(entity);
    }
}
