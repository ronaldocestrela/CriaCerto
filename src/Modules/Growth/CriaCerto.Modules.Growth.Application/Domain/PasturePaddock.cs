using CriaCerto.BuildingBlocks.Abstractions.Results;

namespace CriaCerto.Modules.Growth.Application.Domain;

public sealed class PasturePaddock
{
    public Guid Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string Code { get; private set; } = string.Empty;
    public decimal AreaHectares { get; private set; }
    public decimal MaxCapacityUA { get; private set; }
    public PaddockStatus Status { get; private set; }
    public Guid TenantId { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }

    private PasturePaddock() { }

    public static Result<PasturePaddock> Create(
        string name,
        string code,
        decimal areaHectares,
        decimal maxCapacityUA,
        Guid tenantId,
        PaddockStatus status = PaddockStatus.Active)
    {
        if (string.IsNullOrWhiteSpace(name))
            return Result.Failure<PasturePaddock>(Error.Validation("PasturePaddock.InvalidName", "Nome do pasto/piquete é obrigatório."));

        if (string.IsNullOrWhiteSpace(code))
            return Result.Failure<PasturePaddock>(Error.Validation("PasturePaddock.InvalidCode", "Código do pasto/piquete é obrigatório."));

        if (areaHectares <= 0)
            return Result.Failure<PasturePaddock>(Error.Validation("PasturePaddock.InvalidArea", "A área em hectares deve ser maior que zero."));

        if (maxCapacityUA <= 0)
            return Result.Failure<PasturePaddock>(Error.Validation("PasturePaddock.InvalidCapacity", "A capacidade máxima em UAs deve ser maior que zero."));

        if (tenantId == Guid.Empty)
            return Result.Failure<PasturePaddock>(Error.Validation("PasturePaddock.InvalidTenant", "TenantId é obrigatório."));

        var paddock = new PasturePaddock
        {
            Id = Guid.NewGuid(),
            Name = name.Trim(),
            Code = code.Trim().ToUpperInvariant(),
            AreaHectares = areaHectares,
            MaxCapacityUA = maxCapacityUA,
            Status = status,
            TenantId = tenantId,
            CreatedAtUtc = DateTime.UtcNow
        };

        return Result.Success(paddock);
    }

    public Result UpdateStatus(PaddockStatus newStatus)
    {
        Status = newStatus;
        return Result.Success();
    }

    public Result UpdateCapacity(decimal newAreaHectares, decimal newMaxCapacityUA)
    {
        if (newAreaHectares <= 0)
            return Result.Failure(Error.Validation("PasturePaddock.InvalidArea", "A área em hectares deve ser maior que zero."));

        if (newMaxCapacityUA <= 0)
            return Result.Failure(Error.Validation("PasturePaddock.InvalidCapacity", "A capacidade máxima em UAs deve ser maior que zero."));

        AreaHectares = newAreaHectares;
        MaxCapacityUA = newMaxCapacityUA;
        return Result.Success();
    }
}
