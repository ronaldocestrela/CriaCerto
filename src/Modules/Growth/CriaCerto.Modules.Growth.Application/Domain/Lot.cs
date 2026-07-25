using CriaCerto.BuildingBlocks.Abstractions.Results;

namespace CriaCerto.Modules.Growth.Application.Domain;

public sealed class Lot
{
    public const decimal KgPerAnimalUnit = 450.0m;

    public Guid Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string Code { get; private set; } = string.Empty;
    public LotCategory Category { get; private set; }
    public Guid? CurrentPaddockId { get; private set; }
    public int HeadCount { get; private set; }
    public decimal AverageWeightKg { get; private set; }
    public decimal TotalWeightKg => HeadCount * AverageWeightKg;
    public decimal TotalUA => Math.Round(TotalWeightKg / KgPerAnimalUnit, 2);
    public LotStatus Status { get; private set; }
    public Guid TenantId { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }

    private Lot() { }

    public static Result<Lot> Create(
        string name,
        string code,
        LotCategory category,
        int headCount,
        decimal averageWeightKg,
        Guid tenantId,
        Guid? initialPaddockId = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            return Result.Failure<Lot>(Error.Validation("Lot.InvalidName", "Nome do lote é obrigatório."));

        if (string.IsNullOrWhiteSpace(code))
            return Result.Failure<Lot>(Error.Validation("Lot.InvalidCode", "Código do lote é obrigatório."));

        if (headCount <= 0)
            return Result.Failure<Lot>(Error.Validation("Lot.InvalidHeadCount", "Quantidade de cabeças deve ser maior que zero."));

        if (averageWeightKg <= 0)
            return Result.Failure<Lot>(Error.Validation("Lot.InvalidAverageWeight", "Peso médio deve ser maior que zero."));

        if (tenantId == Guid.Empty)
            return Result.Failure<Lot>(Error.Validation("Lot.InvalidTenant", "TenantId é obrigatório."));

        var lot = new Lot
        {
            Id = Guid.NewGuid(),
            Name = name.Trim(),
            Code = code.Trim().ToUpperInvariant(),
            Category = category,
            HeadCount = headCount,
            AverageWeightKg = averageWeightKg,
            CurrentPaddockId = initialPaddockId,
            Status = LotStatus.Active,
            TenantId = tenantId,
            CreatedAtUtc = DateTime.UtcNow
        };

        return Result.Success(lot);
    }

    public Result AssignToPaddock(Guid paddockId)
    {
        if (Status == LotStatus.Closed)
            return Result.Failure(Error.Conflict("Lot.AlreadyClosed", "Lote encerrado não pode ser movimentado."));

        if (paddockId == Guid.Empty)
            return Result.Failure(Error.Validation("Lot.InvalidPaddockId", "ID do piquete é inválido."));

        CurrentPaddockId = paddockId;
        return Result.Success();
    }

    public Result RemoveFromPaddock()
    {
        CurrentPaddockId = null;
        return Result.Success();
    }

    public Result UpdateHeadCountAndWeight(int headCount, decimal averageWeightKg)
    {
        if (Status == LotStatus.Closed)
            return Result.Failure(Error.Conflict("Lot.AlreadyClosed", "Lote encerrado não pode ser alterado."));

        if (headCount <= 0)
            return Result.Failure(Error.Validation("Lot.InvalidHeadCount", "Quantidade de cabeças deve ser maior que zero."));

        if (averageWeightKg <= 0)
            return Result.Failure(Error.Validation("Lot.InvalidAverageWeight", "Peso médio deve ser maior que zero."));

        HeadCount = headCount;
        AverageWeightKg = averageWeightKg;
        return Result.Success();
    }

    public Result CloseLot()
    {
        if (Status == LotStatus.Closed)
            return Result.Failure(Error.Conflict("Lot.AlreadyClosed", "Lote já está encerrado."));

        Status = LotStatus.Closed;
        CurrentPaddockId = null;
        return Result.Success();
    }
}
