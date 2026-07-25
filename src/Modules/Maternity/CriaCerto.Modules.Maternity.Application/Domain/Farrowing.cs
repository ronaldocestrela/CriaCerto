using CriaCerto.BuildingBlocks.Abstractions.Events;
using CriaCerto.BuildingBlocks.Abstractions.Results;
using CriaCerto.Modules.Maternity.Application.Domain.Events;

namespace CriaCerto.Modules.Maternity.Application.Domain;

public sealed class Farrowing
{
    private readonly List<IDomainEvent> _domainEvents = new();

    private Farrowing()
    {
    }

    private Farrowing(
        Guid id,
        Guid sowId,
        Guid tenantId,
        DateTime farrowingDate,
        int liveBorn,
        int stillborn,
        int mummified,
        decimal litterWeightKg,
        string? maternityRoomId,
        bool assisted,
        string? notes)
    {
        Id = id;
        SowId = sowId;
        TenantId = tenantId;
        FarrowingDate = farrowingDate;
        LiveBorn = liveBorn;
        Stillborn = stillborn;
        Mummified = mummified;
        LitterWeightKg = litterWeightKg;
        MaternityRoomId = maternityRoomId?.Trim();
        Assisted = assisted;
        Notes = notes?.Trim();

        _domainEvents.Add(new FarrowingCompletedEvent(
            Id,
            SowId,
            TenantId,
            LiveBorn,
            Stillborn,
            Mummified,
            FarrowingDate));
    }

    public Guid Id { get; private set; }
    public Guid SowId { get; private set; }
    public Guid TenantId { get; private set; }
    public DateTime FarrowingDate { get; private set; }
    public int LiveBorn { get; private set; }
    public int Stillborn { get; private set; }
    public int Mummified { get; private set; }
    public int TotalBorn => LiveBorn + Stillborn + Mummified;
    public decimal LitterWeightKg { get; private set; }
    public decimal AveragePigletWeightKg => LiveBorn > 0 ? Math.Round(LitterWeightKg / LiveBorn, 2) : 0m;
    public string? MaternityRoomId { get; private set; }
    public bool Assisted { get; private set; }
    public string? Notes { get; private set; }

    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    public void ClearDomainEvents() => _domainEvents.Clear();

    public static Result<Farrowing> Create(
        Guid sowId,
        Guid tenantId,
        DateTime farrowingDate,
        int liveBorn,
        int stillborn,
        int mummified,
        decimal litterWeightKg,
        string? maternityRoomId = null,
        bool assisted = false,
        string? notes = null)
    {
        if (sowId == Guid.Empty)
        {
            return Result.Failure<Farrowing>(Error.Validation("Farrowing.EmptySowId", "O ID da matriz não pode ser vazio."));
        }

        if (tenantId == Guid.Empty)
        {
            return Result.Failure<Farrowing>(Error.Validation("Farrowing.EmptyTenantId", "O ID do inquilino não pode ser vazio."));
        }

        if (liveBorn < 0 || stillborn < 0 || mummified < 0)
        {
            return Result.Failure<Farrowing>(FarrowingErrors.NegativeCounts);
        }

        int totalBorn = liveBorn + stillborn + mummified;
        if (totalBorn <= 0)
        {
            return Result.Failure<Farrowing>(FarrowingErrors.ZeroTotalBorn);
        }

        if (liveBorn > 0)
        {
            if (litterWeightKg <= 0m)
            {
                return Result.Failure<Farrowing>(FarrowingErrors.InvalidLitterWeight);
            }

            decimal avgWeight = litterWeightKg / liveBorn;
            if (avgWeight < 0.3m || avgWeight > 3.5m)
            {
                return Result.Failure<Farrowing>(FarrowingErrors.UnrealisticWeight);
            }
        }

        var farrowing = new Farrowing(
            Guid.NewGuid(),
            sowId,
            tenantId,
            farrowingDate,
            liveBorn,
            stillborn,
            mummified,
            litterWeightKg,
            maternityRoomId,
            assisted,
            notes);

        return Result.Success(farrowing);
    }
}
