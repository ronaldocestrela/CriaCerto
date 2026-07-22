using CriaCerto.BuildingBlocks.Abstractions.Results;

namespace CriaCerto.Modules.Breeding.Application.Domain;

public sealed class Sow
{
    private readonly List<PlantelEvent> _events = new();

    private Sow()
    {
    }

    public Sow(
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
        int parity = 0,
        int dnpDays = 0,
        ReproductiveStatus reproductiveStatus = ReproductiveStatus.Empty,
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
        Parity = parity;
        DnpDays = dnpDays;
        ReproductiveStatus = reproductiveStatus;
        LifecycleStatus = LifecycleStatus.Active;
        Location = location?.Trim();
        LastEventName = "Cadastro";
        LastEventDate = entryDate;

        _events.Add(PlantelEvent.Registered(Id, entryDate, "Matriz registrada no plantel."));
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
    public int Parity { get; private set; }
    public int DnpDays { get; private set; }
    public ReproductiveStatus ReproductiveStatus { get; private set; }
    public LifecycleStatus LifecycleStatus { get; private set; }
    public string? Location { get; private set; }
    public string? LastEventName { get; private set; }
    public DateOnly? LastEventDate { get; private set; }
    public IReadOnlyCollection<PlantelEvent> Events => _events;

    public bool RequiresAttention => DnpDays >= 40 || LifecycleStatus == LifecycleStatus.Quarantine;

    public Result UpdateProfile(
        string tagId,
        string breed,
        DateOnly entryDate,
        decimal entryWeightKg,
        decimal currentWeightKg,
        ReproductiveStatus reproductiveStatus,
        BodyConditionScore bodyConditionScore,
        int parity,
        int dnpDays,
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
            return Result.Failure(Error.Conflict("Sow.Culled", "Matrizes descartadas não podem ser editadas."));
        }

        TagId = tagId.Trim().ToUpperInvariant();
        Breed = breed.Trim();
        EntryDate = entryDate;
        EntryWeightKg = entryWeightKg;
        CurrentWeightKg = currentWeightKg;
        ReproductiveStatus = reproductiveStatus;
        BodyConditionScore = bodyConditionScore;
        Parity = parity;
        DnpDays = dnpDays;
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
            return Result.Failure(Error.Conflict("Sow.InvalidTransition", "O descarte é um status terminal para matrizes."));
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

    public Result RegisterBreeding(
        DateOnly eventDate,
        BreedingMethod method,
        string? boarOrSemenRef,
        string? technician,
        BodyConditionScore? bodyConditionScoreAtBreeding)
    {
        if (LifecycleStatus == LifecycleStatus.Culled)
        {
            return Result.Failure(Error.Conflict("Sow.Culled", "Matrizes descartadas não podem receber cobrição."));
        }

        if (ReproductiveStatus == ReproductiveStatus.Pregnant)
        {
            return Result.Failure(Error.Conflict("Sow.AlreadyPregnant", "Matrizes gestantes não podem ser cobertas novamente."));
        }

        if (ReproductiveStatus == ReproductiveStatus.Lactating)
        {
            return Result.Failure(Error.Conflict("Sow.InvalidBreedingState", "Matrizes lactantes não podem receber cobrição."));
        }

        ReproductiveStatus = ReproductiveStatus.Bred;
        if (bodyConditionScoreAtBreeding.HasValue)
        {
            BodyConditionScore = bodyConditionScoreAtBreeding.Value;
        }

        LastEventName = "Cobrição";
        LastEventDate = eventDate;
        var methodLabel = method switch
        {
            BreedingMethod.ArtificialInsemination => "IA",
            BreedingMethod.TimedArtificialInsemination => "IATF",
            BreedingMethod.NaturalService => "Monta Natural",
            _ => method.ToString()
        };

        var notes = string.Join(" | ", new[]
        {
            technician is not null ? $"Técnico: {technician}" : null,
            boarOrSemenRef is not null ? $"Ref: {boarOrSemenRef}" : null
        }.Where(x => x is not null));

        _events.Add(PlantelEvent.Breeding(Id, methodLabel, eventDate, notes));
        return Result.Success();
    }

    public Result ApplyPregnancyDiagnosis(DateOnly diagnosisDate, PregnancyDiagnosisResult result)
    {
        if (ReproductiveStatus != ReproductiveStatus.Bred)
        {
            return Result.Failure(Error.Conflict("Sow.NotBred", "Diagnóstico só pode ser registrado para matrizes cobertas."));
        }

        ReproductiveStatus = result == PregnancyDiagnosisResult.Pregnant
            ? ReproductiveStatus.Pregnant
            : ReproductiveStatus.Empty;

        var resultLabel = result switch
        {
            PregnancyDiagnosisResult.Pregnant => "Prenhe",
            PregnancyDiagnosisResult.Empty => "Vazia",
            PregnancyDiagnosisResult.ReturnToEstrus => "Retorno ao Cio",
            PregnancyDiagnosisResult.AbortOrDoubt => "Aborto/Dúvida",
            _ => result.ToString()
        };

        LastEventName = "Diagnóstico";
        LastEventDate = diagnosisDate;
        _events.Add(PlantelEvent.Diagnosis(Id, resultLabel, diagnosisDate, null));
        return Result.Success();
    }

    public void SetDnpDays(int dnpDays)
    {
        DnpDays = Math.Max(0, dnpDays);
    }
}
