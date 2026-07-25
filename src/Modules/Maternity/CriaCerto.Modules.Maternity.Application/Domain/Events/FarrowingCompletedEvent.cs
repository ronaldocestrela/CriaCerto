using CriaCerto.BuildingBlocks.Abstractions.Events;

namespace CriaCerto.Modules.Maternity.Application.Domain.Events;

public sealed record FarrowingCompletedEvent(
    Guid FarrowingId,
    Guid SowId,
    Guid TenantId,
    int LiveBorn,
    int Stillborn,
    int Mummified,
    DateTime FarrowingDate) : IDomainEvent
{
    public Guid Id { get; } = Guid.NewGuid();
    public DateTime OccurredOnUtc { get; } = DateTime.UtcNow;
}
