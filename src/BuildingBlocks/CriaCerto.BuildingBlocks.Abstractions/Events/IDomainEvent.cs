namespace CriaCerto.BuildingBlocks.Abstractions.Events;

public interface IDomainEvent
{
    Guid Id { get; }
    DateTime OccurredOnUtc { get; }
}
