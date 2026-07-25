using CriaCerto.BuildingBlocks.Abstractions.Results;
using CriaCerto.Modules.Nutrition.Application.Contracts;
using CriaCerto.Modules.Nutrition.Application.Domain;
using CriaCerto.Modules.Nutrition.Application.Features.SiloStockFeatures;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CriaCerto.Modules.Nutrition.Application.Features.RationFeatures;

public record CreateFeedRationCommand(
    Guid TenantId,
    string Name,
    RationType RationType,
    decimal DryMatterPercentage,
    List<FeedRationItemInput> Items) : IRequest<Result<FeedRationDto>>;

public record GetFeedRationsQuery(Guid TenantId) : IRequest<Result<List<FeedRationDto>>>;

public class CreateFeedRationCommandHandler : IRequestHandler<CreateFeedRationCommand, Result<FeedRationDto>>
{
    private readonly INutritionDbContext _dbContext;

    public CreateFeedRationCommandHandler(INutritionDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<FeedRationDto>> Handle(CreateFeedRationCommand request, CancellationToken cancellationToken)
    {
        var result = FeedRation.Create(
            request.TenantId,
            request.Name,
            request.RationType,
            request.DryMatterPercentage,
            request.Items);

        if (result.IsFailure)
            return Result.Failure<FeedRationDto>(result.Error);

        var ration = result.Value;
        _dbContext.FeedRations.Add(ration);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return Result.Success(MapToDto(ration));
    }

    internal static FeedRationDto MapToDto(FeedRation ration) => new(
        ration.Id,
        ration.TenantId,
        ration.Name,
        ration.RationType,
        ration.DryMatterPercentage,
        ration.CalculatedCostPerKg,
        ration.CreatedAt,
        ration.Items.Select(i => new FeedRationItemDto(i.FeedItemId, i.FeedItemName, i.Percentage, i.UnitCostPerKg)).ToList());
}

public class GetFeedRationsQueryHandler : IRequestHandler<GetFeedRationsQuery, Result<List<FeedRationDto>>>
{
    private readonly INutritionDbContext _dbContext;

    public GetFeedRationsQueryHandler(INutritionDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<List<FeedRationDto>>> Handle(GetFeedRationsQuery request, CancellationToken cancellationToken)
    {
        var rations = await _dbContext.FeedRations
            .Include(r => r.Items)
            .AsNoTracking()
            .Where(r => r.TenantId == request.TenantId)
            .OrderBy(r => r.Name)
            .ToListAsync(cancellationToken);

        var dtos = rations.Select(CreateFeedRationCommandHandler.MapToDto).ToList();
        return Result.Success(dtos);
    }
}
