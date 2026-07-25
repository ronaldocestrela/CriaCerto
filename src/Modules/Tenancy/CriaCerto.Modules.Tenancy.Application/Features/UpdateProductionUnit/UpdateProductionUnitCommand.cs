using CriaCerto.BuildingBlocks.Abstractions.Results;
using CriaCerto.Modules.Tenancy.Application.Abstractions;
using CriaCerto.Modules.Tenancy.Application.Contracts;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CriaCerto.Modules.Tenancy.Application.Features.UpdateProductionUnit;

public record UpdateProductionUnitCommand(
    Guid Id,
    Guid TenantId,
    string Name,
    string Type,
    string Status,
    int Capacity,
    string? LocationDetails
) : IRequest<Result<ProductionUnitDto>>;

public class UpdateProductionUnitCommandHandler : IRequestHandler<UpdateProductionUnitCommand, Result<ProductionUnitDto>>
{
    private readonly ITenancyDbContext _dbContext;

    public UpdateProductionUnitCommandHandler(ITenancyDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<ProductionUnitDto>> Handle(UpdateProductionUnitCommand request, CancellationToken cancellationToken)
    {
        var unit = await _dbContext.ProductionUnits
            .FirstOrDefaultAsync(u => u.Id == request.Id && u.TenantId == request.TenantId, cancellationToken);

        if (unit is null)
        {
            return Result.Failure<ProductionUnitDto>(Error.NotFound("ProductionUnit.NotFound", $"Unidade de produção com ID '{request.Id}' não foi encontrada para esta organização."));
        }

        unit.Name = request.Name;
        unit.Type = request.Type;
        unit.Status = request.Status;
        unit.Capacity = request.Capacity;
        unit.LocationDetails = request.LocationDetails;

        await _dbContext.SaveChangesAsync(cancellationToken);

        var dto = new ProductionUnitDto(
            unit.Id,
            unit.TenantId,
            unit.Code,
            unit.Name,
            unit.Type,
            unit.Status,
            unit.Capacity,
            unit.CurrentHeadCount,
            unit.LocationDetails,
            unit.Capacity > 0 ? Math.Round((decimal)unit.CurrentHeadCount / unit.Capacity * 100, 1) : 0
        );

        return Result.Success(dto);
    }
}
