using CriaCerto.BuildingBlocks.Abstractions.Results;
using CriaCerto.Modules.Nutrition.Application.Contracts;
using CriaCerto.Modules.Nutrition.Application.Domain;
using CriaCerto.Modules.Nutrition.Application.Features.SiloStockFeatures;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CriaCerto.Modules.Nutrition.Application.Features.FeedingFeatures;

public record RecordSupplementationCommand(
    Guid TenantId,
    Guid PaddockId,
    Guid LotId,
    Guid FeedRationId,
    DateTime DistributionDate,
    decimal QuantityKg,
    int HeadCount) : IRequest<Result<PastureSupplementationDto>>;

public record RecordFeedlotTmrCommand(
    Guid TenantId,
    Guid LotId,
    Guid FeedRationId,
    DateTime FeedingTime,
    decimal OfferedAsFedKg,
    TroughScore TroughScore,
    int HeadCountAtFeeding) : IRequest<Result<DailyFeedBatchDto>>;

public class RecordSupplementationCommandHandler : IRequestHandler<RecordSupplementationCommand, Result<PastureSupplementationDto>>
{
    private readonly INutritionDbContext _dbContext;

    public RecordSupplementationCommandHandler(INutritionDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<PastureSupplementationDto>> Handle(RecordSupplementationCommand request, CancellationToken cancellationToken)
    {
        var ration = await _dbContext.FeedRations
            .Include(r => r.Items)
            .FirstOrDefaultAsync(r => r.Id == request.FeedRationId && r.TenantId == request.TenantId, cancellationToken);

        if (ration == null)
            return Result.Failure<PastureSupplementationDto>(Error.NotFound("FeedRation.NotFound", "A ração/suplemento informada não foi encontrada."));

        var result = PastureSupplementation.Create(
            request.TenantId,
            request.PaddockId,
            request.LotId,
            request.FeedRationId,
            request.DistributionDate,
            request.QuantityKg,
            request.HeadCount);

        if (result.IsFailure)
            return Result.Failure<PastureSupplementationDto>(result.Error);

        // Deduzir dos silos componentes da receita de suplementação
        if (ration.Items.Any())
        {
            var siloIds = ration.Items.Select(i => i.FeedItemId).ToList();
            var silos = await _dbContext.SiloStocks
                .Where(s => siloIds.Contains(s.Id) && s.TenantId == request.TenantId)
                .ToListAsync(cancellationToken);

            foreach (var item in ration.Items)
            {
                var silo = silos.FirstOrDefault(s => s.Id == item.FeedItemId);
                if (silo == null)
                    return Result.Failure<PastureSupplementationDto>(Error.NotFound("SiloStock.NotFound", $"Insumo '{item.FeedItemName}' não encontrado no estoque do silo."));

                decimal requiredKg = Math.Round((item.Percentage / 100m) * request.QuantityKg, 2);
                var consumeResult = silo.ConsumeStock(requiredKg);
                if (consumeResult.IsFailure)
                    return Result.Failure<PastureSupplementationDto>(consumeResult.Error);
            }
        }

        var entity = result.Value;
        _dbContext.PastureSupplementations.Add(entity);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return Result.Success(new PastureSupplementationDto(
            entity.Id,
            entity.TenantId,
            entity.PaddockId,
            entity.LotId,
            entity.FeedRationId,
            entity.DistributionDate,
            entity.QuantityKg,
            entity.HeadCount,
            entity.CalculatedIntakeGramsPerHead));
    }
}

public class RecordFeedlotTmrCommandHandler : IRequestHandler<RecordFeedlotTmrCommand, Result<DailyFeedBatchDto>>
{
    private readonly INutritionDbContext _dbContext;

    public RecordFeedlotTmrCommandHandler(INutritionDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<DailyFeedBatchDto>> Handle(RecordFeedlotTmrCommand request, CancellationToken cancellationToken)
    {
        var ration = await _dbContext.FeedRations
            .Include(r => r.Items)
            .FirstOrDefaultAsync(r => r.Id == request.FeedRationId && r.TenantId == request.TenantId, cancellationToken);

        if (ration == null)
            return Result.Failure<DailyFeedBatchDto>(Error.NotFound("FeedRation.NotFound", "A ração de trato informada não foi encontrada."));

        var result = DailyFeedBatch.Create(
            request.TenantId,
            request.LotId,
            request.FeedRationId,
            request.FeedingTime,
            request.OfferedAsFedKg,
            ration.DryMatterPercentage,
            request.TroughScore,
            request.HeadCountAtFeeding);

        if (result.IsFailure)
            return Result.Failure<DailyFeedBatchDto>(result.Error);

        // Deduzir dos silos componentes da receita de trato
        if (ration.Items.Any())
        {
            var siloIds = ration.Items.Select(i => i.FeedItemId).ToList();
            var silos = await _dbContext.SiloStocks
                .Where(s => siloIds.Contains(s.Id) && s.TenantId == request.TenantId)
                .ToListAsync(cancellationToken);

            foreach (var item in ration.Items)
            {
                var silo = silos.FirstOrDefault(s => s.Id == item.FeedItemId);
                if (silo == null)
                    return Result.Failure<DailyFeedBatchDto>(Error.NotFound("SiloStock.NotFound", $"Insumo '{item.FeedItemName}' não encontrado no estoque do silo."));

                decimal requiredKg = Math.Round((item.Percentage / 100m) * request.OfferedAsFedKg, 2);
                var consumeResult = silo.ConsumeStock(requiredKg);
                if (consumeResult.IsFailure)
                    return Result.Failure<DailyFeedBatchDto>(consumeResult.Error);
            }
        }

        var batch = result.Value;
        _dbContext.DailyFeedBatches.Add(batch);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return Result.Success(new DailyFeedBatchDto(
            batch.Id,
            batch.TenantId,
            batch.LotId,
            batch.FeedRationId,
            batch.FeedingTime,
            batch.OfferedAsFedKg,
            batch.OfferedDryMatterKg,
            batch.TroughScore,
            batch.HeadCountAtFeeding));
    }
}
