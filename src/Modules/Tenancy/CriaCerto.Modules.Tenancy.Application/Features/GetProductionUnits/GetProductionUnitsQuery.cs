using CriaCerto.BuildingBlocks.Abstractions.Results;
using CriaCerto.Modules.Tenancy.Application.Abstractions;
using CriaCerto.Modules.Tenancy.Application.Contracts;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CriaCerto.Modules.Tenancy.Application.Features.GetProductionUnits;

public record GetProductionUnitsQuery(Guid TenantId) : IRequest<Result<List<ProductionUnitDto>>>;

public class GetProductionUnitsQueryHandler : IRequestHandler<GetProductionUnitsQuery, Result<List<ProductionUnitDto>>>
{
    private readonly ITenancyDbContext _dbContext;

    public GetProductionUnitsQueryHandler(ITenancyDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<List<ProductionUnitDto>>> Handle(GetProductionUnitsQuery request, CancellationToken cancellationToken)
    {
        var units = await _dbContext.ProductionUnits
            .AsNoTracking()
            .Where(u => u.TenantId == request.TenantId)
            .OrderBy(u => u.Code)
            .ToListAsync(cancellationToken);

        var dtos = units.Select(u => new ProductionUnitDto(
            u.Id,
            u.TenantId,
            u.Code,
            u.Name,
            u.Type,
            u.Status,
            u.Capacity,
            u.CurrentHeadCount,
            u.LocationDetails,
            u.Capacity > 0 ? Math.Round((decimal)u.CurrentHeadCount / u.Capacity * 100, 1) : 0
        )).ToList();

        return Result.Success(dtos);
    }
}
