namespace CriaCerto.Modules.Breeding.Application.Domain;

public sealed class PlantelEvent
{
    private PlantelEvent()
    {
    }

    private PlantelEvent(Guid animalId, string eventType, string title, DateOnly eventDate, string? notes)
    {
        Id = Guid.NewGuid();
        AnimalId = animalId;
        EventType = eventType;
        Title = title;
        EventDate = eventDate;
        Notes = notes;
    }

    public Guid Id { get; private set; }
    public Guid AnimalId { get; private set; }
    public string EventType { get; private set; } = string.Empty;
    public string Title { get; private set; } = string.Empty;
    public DateOnly EventDate { get; private set; }
    public string? Notes { get; private set; }

    public static PlantelEvent Registered(Guid animalId, DateOnly eventDate, string notes) =>
        new(animalId, "Registered", "Cadastro", eventDate, notes);

    public static PlantelEvent StatusChanged(Guid animalId, string status, DateOnly eventDate, string? notes) =>
        new(animalId, "StatusChanged", $"Status alterado para {status}", eventDate, notes);
}
