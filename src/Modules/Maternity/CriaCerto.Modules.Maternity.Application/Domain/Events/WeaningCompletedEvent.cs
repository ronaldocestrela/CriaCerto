using CriaCerto.BuildingBlocks.Abstractions.Events;

namespace CriaCerto.Modules.Maternity.Application.Domain.Events;

public sealed record WeaningCompletedEvent(
    Guid WeaningId,
    Guid TenantId,
    Guid FarrowingId,
    Guid SowId,
    int WeanedCount,
    decimal TotalWeanedWeightKg,
    DateTime WeaningDate,
    string DestinationPenOrBatch) : IDomainEvent
{
    public Guid Id { get; } = Guid.NewGuid();
    public DateTime OccurredOnUtc { get; } = DateTime.UtcNow;
}
