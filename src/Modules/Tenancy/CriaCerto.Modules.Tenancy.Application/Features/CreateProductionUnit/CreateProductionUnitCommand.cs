using CriaCerto.BuildingBlocks.Abstractions.Results;
using CriaCerto.Modules.Tenancy.Application.Abstractions;
using CriaCerto.Modules.Tenancy.Application.Contracts;
using CriaCerto.Modules.Tenancy.Application.Domain;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CriaCerto.Modules.Tenancy.Application.Features.CreateProductionUnit;

public record CreateProductionUnitCommand(
    Guid TenantId,
    string Name,
    string Type,
    int Capacity,
    string? LocationDetails
) : IRequest<Result<ProductionUnitDto>>;

public class CreateProductionUnitCommandHandler : IRequestHandler<CreateProductionUnitCommand, Result<ProductionUnitDto>>
{
    private readonly ITenancyDbContext _dbContext;

    public CreateProductionUnitCommandHandler(ITenancyDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<ProductionUnitDto>> Handle(CreateProductionUnitCommand request, CancellationToken cancellationToken)
    {
        var tenantExists = await _dbContext.Tenants
            .AnyAsync(t => t.Id == request.TenantId, cancellationToken);

        if (!tenantExists)
        {
            return Result.Failure<ProductionUnitDto>(Error.NotFound("Tenant.NotFound", $"Organização/Fazenda com ID '{request.TenantId}' não foi encontrada."));
        }

        var count = await _dbContext.ProductionUnits
            .CountAsync(u => u.TenantId == request.TenantId, cancellationToken);

        var nextCode = $"UN-{(count + 1):D3}-SFE";

        var unit = new ProductionUnit
        {
            Id = Guid.NewGuid(),
            TenantId = request.TenantId,
            Code = nextCode,
            Name = request.Name,
            Type = request.Type,
            Status = "Active",
            Capacity = request.Capacity,
            CurrentHeadCount = 0,
            LocationDetails = request.LocationDetails,
            CreatedAt = DateTime.UtcNow
        };

        _dbContext.ProductionUnits.Add(unit);
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
            0m
        );

        return Result.Success(dto);
    }
}
