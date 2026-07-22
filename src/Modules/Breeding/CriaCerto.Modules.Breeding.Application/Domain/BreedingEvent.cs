namespace CriaCerto.Modules.Breeding.Application.Domain;

public sealed class BreedingEvent
{
    private BreedingEvent()
    {
    }

    public BreedingEvent(
        Guid id,
        Guid sowId,
        DateOnly eventDate,
        BreedingMethod method,
        string? boarOrSemenRef,
        string? technician,
        BodyConditionScore? bodyConditionScoreAtBreeding,
        string? location,
        string? notes)
    {
        Id = id;
        SowId = sowId;
        EventDate = eventDate;
        Method = method;
        BoarOrSemenRef = boarOrSemenRef?.Trim();
        Technician = technician?.Trim();
        BodyConditionScoreAtBreeding = bodyConditionScoreAtBreeding;
        Location = location?.Trim();
        Notes = notes?.Trim();
    }

    public Guid Id { get; private set; }
    public Guid SowId { get; private set; }
    public DateOnly EventDate { get; private set; }
    public BreedingMethod Method { get; private set; }
    public string? BoarOrSemenRef { get; private set; }
    public string? Technician { get; private set; }
    public BodyConditionScore? BodyConditionScoreAtBreeding { get; private set; }
    public string? Location { get; private set; }
    public string? Notes { get; private set; }
}
