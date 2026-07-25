using CriaCerto.BuildingBlocks.Abstractions.Events;
using CriaCerto.BuildingBlocks.Abstractions.Results;
using CriaCerto.Modules.Maternity.Application.Domain.Events;

namespace CriaCerto.Modules.Maternity.Application.Domain;

public sealed class PigletTransfer
{
    private readonly List<IDomainEvent> _domainEvents = new();

    private PigletTransfer()
    {
    }

    private PigletTransfer(
        Guid id,
        Guid tenantId,
        Guid sourceFarrowingId,
        Guid sourceSowId,
        Guid targetFarrowingId,
        Guid targetSowId,
        int quantity,
        DateTime transferDate,
        string? notes)
    {
        Id = id;
        TenantId = tenantId;
        SourceFarrowingId = sourceFarrowingId;
        SourceSowId = sourceSowId;
        TargetFarrowingId = targetFarrowingId;
        TargetSowId = targetSowId;
        Quantity = quantity;
        TransferDate = transferDate;
        Notes = notes?.Trim();

        _domainEvents.Add(new PigletTransferredEvent(
            Id,
            TenantId,
            SourceFarrowingId,
            SourceSowId,
            TargetFarrowingId,
            TargetSowId,
            Quantity,
            TransferDate));
    }

    public Guid Id { get; private set; }
    public Guid TenantId { get; private set; }
    public Guid SourceFarrowingId { get; private set; }
    public Guid SourceSowId { get; private set; }
    public Guid TargetFarrowingId { get; private set; }
    public Guid TargetSowId { get; private set; }
    public int Quantity { get; private set; }
    public DateTime TransferDate { get; private set; }
    public string? Notes { get; private set; }

    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    public void ClearDomainEvents() => _domainEvents.Clear();

    public static Result<PigletTransfer> Create(
        Guid tenantId,
        Guid sourceFarrowingId,
        Guid sourceSowId,
        Guid targetFarrowingId,
        Guid targetSowId,
        int quantity,
        DateTime transferDate,
        string? notes = null)
    {
        if (tenantId == Guid.Empty)
        {
            return Result.Failure<PigletTransfer>(Error.Validation("PigletTransfer.EmptyTenantId", "O ID do inquilino não pode ser vazio."));
        }

        if (sourceFarrowingId == Guid.Empty || targetFarrowingId == Guid.Empty)
        {
            return Result.Failure<PigletTransfer>(Error.Validation("PigletTransfer.EmptyFarrowingId", "Os IDs dos partos de origem e destino são obrigatórios."));
        }

        if (sourceFarrowingId == targetFarrowingId || sourceSowId == targetSowId)
        {
            return Result.Failure<PigletTransfer>(FarrowingErrors.SameSourceAndTarget);
        }

        if (quantity <= 0)
        {
            return Result.Failure<PigletTransfer>(FarrowingErrors.InvalidTransferQuantity);
        }

        var transfer = new PigletTransfer(
            Guid.NewGuid(),
            tenantId,
            sourceFarrowingId,
            sourceSowId,
            targetFarrowingId,
            targetSowId,
            quantity,
            transferDate,
            notes);

        return Result.Success(transfer);
    }
}
