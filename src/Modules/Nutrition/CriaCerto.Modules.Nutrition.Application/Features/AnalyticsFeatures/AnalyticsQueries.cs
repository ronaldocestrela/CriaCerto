using CriaCerto.BuildingBlocks.Abstractions.Results;
using CriaCerto.Modules.Nutrition.Application.Contracts;
using CriaCerto.Modules.Nutrition.Application.Domain.Services;
using CriaCerto.Modules.Nutrition.Application.Features.SiloStockFeatures;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CriaCerto.Modules.Nutrition.Application.Features.AnalyticsFeatures;

public record GetFeedlotPerformanceQuery(
    Guid TenantId,
    Guid LotId,
    decimal TotalWeightGainKg) : IRequest<Result<FeedlotPerformanceDto>>;

public record GetCostPerArrobaQuery(
    Guid TenantId,
    Guid LotId,
    decimal TotalWeightGainKg,
    decimal? CarcassYieldPercentage) : IRequest<Result<CostPerArrobaDto>>;

public class GetFeedlotPerformanceQueryHandler : IRequestHandler<GetFeedlotPerformanceQuery, Result<FeedlotPerformanceDto>>
{
    private readonly INutritionDbContext _dbContext;

    public GetFeedlotPerformanceQueryHandler(INutritionDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<FeedlotPerformanceDto>> Handle(GetFeedlotPerformanceQuery request, CancellationToken cancellationToken)
    {
        var batches = await _dbContext.DailyFeedBatches
            .AsNoTracking()
            .Where(b => b.TenantId == request.TenantId && b.LotId == request.LotId)
            .ToListAsync(cancellationToken);

        decimal totalDryMatterKg = batches.Sum(b => b.OfferedDryMatterKg);

        var (ca, ea) = NutritionAnalyticsCalculator.CalculateFeedConversion(totalDryMatterKg, request.TotalWeightGainKg);

        return Result.Success(new FeedlotPerformanceDto(
            request.LotId,
            Math.Round(totalDryMatterKg, 2),
            Math.Round(request.TotalWeightGainKg, 2),
            ca,
            ea));
    }
}

public class GetCostPerArrobaQueryHandler : IRequestHandler<GetCostPerArrobaQuery, Result<CostPerArrobaDto>>
{
    private readonly INutritionDbContext _dbContext;

    public GetCostPerArrobaQueryHandler(INutritionDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<CostPerArrobaDto>> Handle(GetCostPerArrobaQuery request, CancellationToken cancellationToken)
    {
        // 1. Sum cost of TMR batches
        var tmrBatches = await _dbContext.DailyFeedBatches
            .AsNoTracking()
            .Where(b => b.TenantId == request.TenantId && b.LotId == request.LotId)
            .ToListAsync(cancellationToken);

        var rationIds = tmrBatches.Select(b => b.FeedRationId).Distinct().ToList();
        var rations = await _dbContext.FeedRations
            .AsNoTracking()
            .Where(r => rationIds.Contains(r.Id))
            .ToDictionaryAsync(r => r.Id, cancellationToken);

        decimal totalTmrCost = 0m;
        foreach (var batch in tmrBatches)
        {
            if (rations.TryGetValue(batch.FeedRationId, out var ration))
            {
                totalTmrCost += batch.OfferedAsFedKg * ration.CalculatedCostPerKg;
            }
        }

        // 2. Sum cost of Pasture Supplementations
        var pastureSupps = await _dbContext.PastureSupplementations
            .AsNoTracking()
            .Where(s => s.TenantId == request.TenantId && s.LotId == request.LotId)
            .ToListAsync(cancellationToken);

        var suppRationIds = pastureSupps.Select(s => s.FeedRationId).Distinct().ToList();
        var suppRations = await _dbContext.FeedRations
            .AsNoTracking()
            .Where(r => suppRationIds.Contains(r.Id))
            .ToDictionaryAsync(r => r.Id, cancellationToken);

        decimal totalSuppCost = 0m;
        foreach (var supp in pastureSupps)
        {
            if (suppRations.TryGetValue(supp.FeedRationId, out var ration))
            {
                totalSuppCost += supp.QuantityKg * ration.CalculatedCostPerKg;
            }
        }

        decimal totalNutritionCost = totalTmrCost + totalSuppCost;

        var calculation = NutritionAnalyticsCalculator.CalculateCostPerArroba(
            totalNutritionCost,
            request.TotalWeightGainKg,
            request.CarcassYieldPercentage);

        return Result.Success(new CostPerArrobaDto(
            request.LotId,
            calculation.TotalNutritionCost,
            calculation.TotalWeightGainKg,
            request.CarcassYieldPercentage ?? 50m,
            calculation.ArrobasProduced,
            calculation.CostPerArroba));
    }
}
