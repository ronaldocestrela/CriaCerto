using CriaCerto.BuildingBlocks.Abstractions.Events;

namespace CriaCerto.Modules.Maternity.Application.Domain.Events;

public sealed record PigletTransferredEvent(
    Guid TransferId,
    Guid TenantId,
    Guid SourceFarrowingId,
    Guid SourceSowId,
    Guid TargetFarrowingId,
    Guid TargetSowId,
    int Quantity,
    DateTime TransferDate) : IDomainEvent
{
    public Guid Id { get; } = Guid.NewGuid();
    public DateTime OccurredOnUtc { get; } = DateTime.UtcNow;
}
