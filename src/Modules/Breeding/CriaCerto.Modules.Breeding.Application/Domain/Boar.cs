using CriaCerto.BuildingBlocks.Abstractions.Results;

namespace CriaCerto.Modules.Breeding.Application.Domain;

public sealed class Boar
{
    private readonly List<PlantelEvent> _events = new();

    private Boar()
    {
    }

    public Boar(
        Guid id,
        string tagId,
        string breed,
        DateOnly entryDate,
        decimal entryWeightKg,
        string? nickname = null,
        string? pbbRegistration = null,
        string? origin = null,
        DateOnly? birthDate = null,
        decimal? currentWeightKg = null,
        decimal? averageDailyGain = null,
        string? fatherTagId = null,
        string? motherTagId = null,
        BodyConditionScore bodyConditionScore = BodyConditionScore.Ideal,
        string? location = null)
    {
        Id = id;
        TagId = tagId.Trim().ToUpperInvariant();
        Nickname = nickname?.Trim();
        PbbRegistration = pbbRegistration?.Trim();
        Breed = breed.Trim();
        Origin = origin?.Trim();
        BirthDate = birthDate;
        EntryDate = entryDate;
        EntryWeightKg = entryWeightKg;
        CurrentWeightKg = currentWeightKg ?? entryWeightKg;
        AverageDailyGain = averageDailyGain;
        FatherTagId = fatherTagId?.Trim();
        MotherTagId = motherTagId?.Trim();
        BodyConditionScore = bodyConditionScore;
        LifecycleStatus = LifecycleStatus.Active;
        Location = location?.Trim();
        LastEventName = "Cadastro";
        LastEventDate = entryDate;

        _events.Add(PlantelEvent.Registered(Id, entryDate, "Cachaço registrado no plantel."));
    }

    public Guid Id { get; private set; }
    public string TagId { get; private set; } = string.Empty;
    public string? Nickname { get; private set; }
    public string? PbbRegistration { get; private set; }
    public string Breed { get; private set; } = string.Empty;
    public string? Origin { get; private set; }
    public DateOnly? BirthDate { get; private set; }
    public DateOnly EntryDate { get; private set; }
    public decimal EntryWeightKg { get; private set; }
    public decimal CurrentWeightKg { get; private set; }
    public decimal? AverageDailyGain { get; private set; }
    public string? FatherTagId { get; private set; }
    public string? MotherTagId { get; private set; }
    public BodyConditionScore BodyConditionScore { get; private set; }
    public LifecycleStatus LifecycleStatus { get; private set; }
    public string? Location { get; private set; }
    public string? LastEventName { get; private set; }
    public DateOnly? LastEventDate { get; private set; }
    public IReadOnlyCollection<PlantelEvent> Events => _events;

    public bool RequiresAttention => LifecycleStatus == LifecycleStatus.Quarantine;

    public Result UpdateProfile(
        string tagId,
        string breed,
        DateOnly entryDate,
        decimal entryWeightKg,
        decimal currentWeightKg,
        BodyConditionScore bodyConditionScore,
        string? nickname,
        string? pbbRegistration,
        string? origin,
        DateOnly? birthDate,
        decimal? averageDailyGain,
        string? fatherTagId,
        string? motherTagId,
        string? location)
    {
        if (LifecycleStatus == LifecycleStatus.Culled)
        {
            return Result.Failure(Error.Conflict("Boar.Culled", "Cachaços descartados não podem ser editados."));
        }

        TagId = tagId.Trim().ToUpperInvariant();
        Breed = breed.Trim();
        EntryDate = entryDate;
        EntryWeightKg = entryWeightKg;
        CurrentWeightKg = currentWeightKg;
        BodyConditionScore = bodyConditionScore;
        Nickname = nickname?.Trim();
        PbbRegistration = pbbRegistration?.Trim();
        Origin = origin?.Trim();
        BirthDate = birthDate;
        AverageDailyGain = averageDailyGain;
        FatherTagId = fatherTagId?.Trim();
        MotherTagId = motherTagId?.Trim();
        Location = location?.Trim();
        LastEventName = "Atualização cadastral";
        LastEventDate = DateOnly.FromDateTime(DateTime.UtcNow);
        return Result.Success();
    }

    public Result ChangeLifecycleStatus(LifecycleStatus status, DateOnly eventDate, string? notes = null)
    {
        if (LifecycleStatus == LifecycleStatus.Culled)
        {
            return Result.Failure(Error.Conflict("Boar.InvalidTransition", "O descarte é um status terminal para cachaços."));
        }

        if (LifecycleStatus == status)
        {
            return Result.Success();
        }

        LifecycleStatus = status;
        LastEventName = status == LifecycleStatus.Culled ? "Descarte solicitado" : $"Status alterado para {status}";
        LastEventDate = eventDate;
        _events.Add(PlantelEvent.StatusChanged(Id, status.ToString(), eventDate, notes));
        return Result.Success();
    }
}
