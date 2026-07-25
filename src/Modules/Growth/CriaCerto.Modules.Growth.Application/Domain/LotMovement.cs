using CriaCerto.BuildingBlocks.Abstractions.Results;

namespace CriaCerto.Modules.Growth.Application.Domain;

public sealed class LotMovement
{
    public Guid Id { get; private set; }
    public Guid LotId { get; private set; }
    public Guid? SourcePaddockId { get; private set; }
    public Guid? DestinationPaddockId { get; private set; }
    public DateTime MovementDate { get; private set; }
    public int HeadCountMoved { get; private set; }
    public string Notes { get; private set; } = string.Empty;
    public Guid TenantId { get; private set; }

    private LotMovement() { }

    public static Result<LotMovement> Create(
        Guid lotId,
        Guid? sourcePaddockId,
        Guid? destinationPaddockId,
        int headCountMoved,
        Guid tenantId,
        string? notes = null,
        DateTime? movementDate = null)
    {
        if (lotId == Guid.Empty)
            return Result.Failure<LotMovement>(Error.Validation("LotMovement.InvalidLotId", "LotId é obrigatório."));

        if (headCountMoved <= 0)
            return Result.Failure<LotMovement>(Error.Validation("LotMovement.InvalidHeadCount", "Quantidade de cabeças movimentadas deve ser maior que zero."));

        if (tenantId == Guid.Empty)
            return Result.Failure<LotMovement>(Error.Validation("LotMovement.InvalidTenant", "TenantId é obrigatório."));

        var movement = new LotMovement
        {
            Id = Guid.NewGuid(),
            LotId = lotId,
            SourcePaddockId = sourcePaddockId,
            DestinationPaddockId = destinationPaddockId,
            MovementDate = movementDate ?? DateTime.UtcNow,
            HeadCountMoved = headCountMoved,
            Notes = notes?.Trim() ?? string.Empty,
            TenantId = tenantId
        };

        return Result.Success(movement);
    }
}
