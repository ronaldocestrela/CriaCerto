using CriaCerto.BuildingBlocks.Abstractions.Results;

namespace CriaCerto.Modules.Nutrition.Application.Domain;

public class DailyFeedBatch
{
    public Guid Id { get; private set; }
    public Guid TenantId { get; private set; }
    public Guid LotId { get; private set; }
    public Guid FeedRationId { get; private set; }
    public DateTime FeedingTime { get; private set; }
    public decimal OfferedAsFedKg { get; private set; }
    public decimal OfferedDryMatterKg { get; private set; }
    public TroughScore TroughScore { get; private set; }
    public int HeadCountAtFeeding { get; private set; }

    private DailyFeedBatch() { }

    public static Result<DailyFeedBatch> Create(
        Guid tenantId,
        Guid lotId,
        Guid feedRationId,
        DateTime feedingTime,
        decimal offeredAsFedKg,
        decimal dryMatterPercentage,
        TroughScore troughScore,
        int headCountAtFeeding)
    {
        if (tenantId == Guid.Empty)
            return Result.Failure<DailyFeedBatch>(Error.Validation("DailyFeedBatch.InvalidTenant", "O Id do Tenant é obrigatório."));

        if (lotId == Guid.Empty)
            return Result.Failure<DailyFeedBatch>(Error.Validation("DailyFeedBatch.InvalidLot", "O Lote/Curral de destino é obrigatório."));

        if (feedRationId == Guid.Empty)
            return Result.Failure<DailyFeedBatch>(Error.Validation("DailyFeedBatch.InvalidRation", "A Ração de trato é obrigatória."));

        if (offeredAsFedKg <= 0)
            return Result.Failure<DailyFeedBatch>(Error.Validation("DailyFeedBatch.InvalidOfferedKg", "A quantidade de trato em kg (MN) deve ser maior que zero."));

        if (dryMatterPercentage <= 0 || dryMatterPercentage > 100)
            return Result.Failure<DailyFeedBatch>(Error.Validation("DailyFeedBatch.InvalidDryMatter", "O teor de matéria seca (%) deve ser entre 1 e 100%."));

        if (headCountAtFeeding <= 0)
            return Result.Failure<DailyFeedBatch>(Error.Validation("DailyFeedBatch.InvalidHeadCount", "O número de cabeças no curral deve ser maior que zero."));

        decimal dryMatterKg = offeredAsFedKg * (dryMatterPercentage / 100m);

        var batch = new DailyFeedBatch
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            LotId = lotId,
            FeedRationId = feedRationId,
            FeedingTime = feedingTime == default ? DateTime.UtcNow : feedingTime,
            OfferedAsFedKg = offeredAsFedKg,
            OfferedDryMatterKg = Math.Round(dryMatterKg, 2),
            TroughScore = troughScore,
            HeadCountAtFeeding = headCountAtFeeding
        };

        return Result.Success(batch);
    }
}
