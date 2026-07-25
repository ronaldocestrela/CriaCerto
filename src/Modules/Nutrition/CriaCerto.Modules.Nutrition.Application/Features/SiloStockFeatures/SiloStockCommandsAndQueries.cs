using CriaCerto.BuildingBlocks.Abstractions.Results;
using CriaCerto.Modules.Nutrition.Application.Contracts;
using CriaCerto.Modules.Nutrition.Application.Domain;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CriaCerto.Modules.Nutrition.Application.Features.SiloStockFeatures;

public record CreateSiloStockCommand(
    Guid TenantId,
    string Name,
    FeedCategory Category,
    decimal InitialStockKg,
    decimal UnitCostPerKg,
    decimal DryMatterPercentage,
    decimal MinimumThresholdKg) : IRequest<Result<SiloStockDto>>;

public record RestockSiloCommand(
    Guid Id,
    Guid TenantId,
    decimal AddedKg,
    decimal NewUnitCostPerKg) : IRequest<Result<SiloStockDto>>;

public record GetSiloStocksQuery(Guid TenantId) : IRequest<Result<List<SiloStockDto>>>;

public interface INutritionDbContext
{
    DbSet<SiloStock> SiloStocks { get; }
    DbSet<FeedRation> FeedRations { get; }
    DbSet<PastureSupplementation> PastureSupplementations { get; }
    DbSet<DailyFeedBatch> DailyFeedBatches { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}

public class CreateSiloStockCommandHandler : IRequestHandler<CreateSiloStockCommand, Result<SiloStockDto>>
{
    private readonly INutritionDbContext _dbContext;

    public CreateSiloStockCommandHandler(INutritionDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<SiloStockDto>> Handle(CreateSiloStockCommand request, CancellationToken cancellationToken)
    {
        var result = SiloStock.Create(
            request.TenantId,
            request.Name,
            request.Category,
            request.InitialStockKg,
            request.UnitCostPerKg,
            request.DryMatterPercentage,
            request.MinimumThresholdKg);

        if (result.IsFailure)
            return Result.Failure<SiloStockDto>(result.Error);

        var silo = result.Value;
        _dbContext.SiloStocks.Add(silo);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return Result.Success(MapToDto(silo));
    }

    internal static SiloStockDto MapToDto(SiloStock silo) => new(
        silo.Id,
        silo.TenantId,
        silo.Name,
        silo.Category,
        silo.CurrentStockKg,
        silo.UnitCostPerKg,
        silo.DryMatterPercentage,
        silo.MinimumThresholdKg,
        silo.LastRestockedAt,
        silo.CurrentStockKg <= silo.MinimumThresholdKg);
}

public class RestockSiloCommandHandler : IRequestHandler<RestockSiloCommand, Result<SiloStockDto>>
{
    private readonly INutritionDbContext _dbContext;

    public RestockSiloCommandHandler(INutritionDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<SiloStockDto>> Handle(RestockSiloCommand request, CancellationToken cancellationToken)
    {
        var silo = await _dbContext.SiloStocks
            .FirstOrDefaultAsync(s => s.Id == request.Id && s.TenantId == request.TenantId, cancellationToken);

        if (silo == null)
            return Result.Failure<SiloStockDto>(Error.NotFound("SiloStock.NotFound", "Silo ou insumo não encontrado."));

        var restockResult = silo.Restock(request.AddedKg, request.NewUnitCostPerKg);
        if (restockResult.IsFailure)
            return Result.Failure<SiloStockDto>(restockResult.Error);

        await _dbContext.SaveChangesAsync(cancellationToken);
        return Result.Success(CreateSiloStockCommandHandler.MapToDto(silo));
    }
}

public class GetSiloStocksQueryHandler : IRequestHandler<GetSiloStocksQuery, Result<List<SiloStockDto>>>
{
    private readonly INutritionDbContext _dbContext;

    public GetSiloStocksQueryHandler(INutritionDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<List<SiloStockDto>>> Handle(GetSiloStocksQuery request, CancellationToken cancellationToken)
    {
        var silos = await _dbContext.SiloStocks
            .AsNoTracking()
            .Where(s => s.TenantId == request.TenantId)
            .OrderBy(s => s.Name)
            .ToListAsync(cancellationToken);

        var dtos = silos.Select(CreateSiloStockCommandHandler.MapToDto).ToList();
        return Result.Success(dtos);
    }
}
