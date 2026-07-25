using CriaCerto.Modules.Nutrition.Application.Domain;

namespace CriaCerto.Modules.Nutrition.Application.Contracts;

public record SiloStockDto(
    Guid Id,
    Guid TenantId,
    string Name,
    FeedCategory Category,
    decimal CurrentStockKg,
    decimal UnitCostPerKg,
    decimal DryMatterPercentage,
    decimal MinimumThresholdKg,
    DateTime LastRestockedAt,
    bool IsStockLow);

public record FeedRationItemDto(
    Guid FeedItemId,
    string FeedItemName,
    decimal Percentage,
    decimal UnitCostPerKg);

public record FeedRationDto(
    Guid Id,
    Guid TenantId,
    string Name,
    RationType RationType,
    decimal DryMatterPercentage,
    decimal CalculatedCostPerKg,
    DateTime CreatedAt,
    List<FeedRationItemDto> Items);

public record PastureSupplementationDto(
    Guid Id,
    Guid TenantId,
    Guid PaddockId,
    Guid LotId,
    Guid FeedRationId,
    DateTime DistributionDate,
    decimal QuantityKg,
    int HeadCount,
    decimal CalculatedIntakeGramsPerHead);

public record DailyFeedBatchDto(
    Guid Id,
    Guid TenantId,
    Guid LotId,
    Guid FeedRationId,
    DateTime FeedingTime,
    decimal OfferedAsFedKg,
    decimal OfferedDryMatterKg,
    TroughScore TroughScore,
    int HeadCountAtFeeding);

public record FeedlotPerformanceDto(
    Guid LotId,
    decimal TotalDryMatterIntakeKg,
    decimal TotalWeightGainKg,
    decimal FeedConversionRatio,
    decimal FeedEfficiency);

public record CostPerArrobaDto(
    Guid LotId,
    decimal TotalNutritionCost,
    decimal TotalWeightGainKg,
    decimal CarcassYieldPercentage,
    decimal ArrobasProduced,
    decimal CostPerArroba);
