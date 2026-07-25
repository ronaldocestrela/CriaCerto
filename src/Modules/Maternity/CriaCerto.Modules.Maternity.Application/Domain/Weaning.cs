using CriaCerto.BuildingBlocks.Abstractions.Events;
using CriaCerto.BuildingBlocks.Abstractions.Results;
using CriaCerto.Modules.Maternity.Application.Domain.Events;

namespace CriaCerto.Modules.Maternity.Application.Domain;

public sealed class Weaning
{
    private readonly List<IDomainEvent> _domainEvents = new();

    private Weaning()
    {
    }

    private Weaning(
        Guid id,
        Guid tenantId,
        Guid farrowingId,
        Guid sowId,
        DateTime weaningDate,
        int weanedCount,
        decimal totalWeanedWeightKg,
        string destinationPenOrBatch,
        string? notes)
    {
        Id = id;
        TenantId = tenantId;
        FarrowingId = farrowingId;
        SowId = sowId;
        WeaningDate = weaningDate;
        WeanedCount = weanedCount;
        TotalWeanedWeightKg = totalWeanedWeightKg;
        DestinationPenOrBatch = destinationPenOrBatch.Trim();
        Notes = notes?.Trim();

        _domainEvents.Add(new WeaningCompletedEvent(
            Id,
            TenantId,
            FarrowingId,
            SowId,
            WeanedCount,
            TotalWeanedWeightKg,
            WeaningDate,
            DestinationPenOrBatch));
    }

    public Guid Id { get; private set; }
    public Guid TenantId { get; private set; }
    public Guid FarrowingId { get; private set; }
    public Guid SowId { get; private set; }
    public DateTime WeaningDate { get; private set; }
    public int WeanedCount { get; private set; }
    public decimal TotalWeanedWeightKg { get; private set; }
    public decimal AverageWeanedWeightKg => WeanedCount > 0 ? Math.Round(TotalWeanedWeightKg / WeanedCount, 2) : 0m;
    public string DestinationPenOrBatch { get; private set; } = string.Empty;
    public string? Notes { get; private set; }

    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    public void ClearDomainEvents() => _domainEvents.Clear();

    public static Result<Weaning> Create(
        Guid tenantId,
        Guid farrowingId,
        Guid sowId,
        DateTime weaningDate,
        int weanedCount,
        decimal totalWeanedWeightKg,
        string destinationPenOrBatch,
        string? notes = null)
    {
        if (tenantId == Guid.Empty)
        {
            return Result.Failure<Weaning>(Error.Validation("Weaning.EmptyTenantId", "O ID do inquilino não pode ser vazio."));
        }

        if (farrowingId == Guid.Empty || sowId == Guid.Empty)
        {
            return Result.Failure<Weaning>(Error.Validation("Weaning.EmptyFarrowingOrSowId", "Os IDs de parto e matriz são obrigatórios."));
        }

        if (weanedCount <= 0)
        {
            return Result.Failure<Weaning>(FarrowingErrors.InvalidWeaningCount);
        }

        if (totalWeanedWeightKg <= 0m)
        {
            return Result.Failure<Weaning>(FarrowingErrors.InvalidWeanedWeight);
        }

        if (string.IsNullOrWhiteSpace(destinationPenOrBatch))
        {
            return Result.Failure<Weaning>(Error.Validation("Weaning.EmptyDestination", "A baia ou lote de destino é obrigatório."));
        }

        decimal avgWeight = totalWeanedWeightKg / weanedCount;
        if (avgWeight < 4.0m || avgWeight > 12.0m)
        {
            return Result.Failure<Weaning>(FarrowingErrors.UnrealisticWeanedWeight);
        }

        var weaning = new Weaning(
            Guid.NewGuid(),
            tenantId,
            farrowingId,
            sowId,
            weaningDate,
            weanedCount,
            totalWeanedWeightKg,
            destinationPenOrBatch,
            notes);

        return Result.Success(weaning);
    }
}
