using CriaCerto.BuildingBlocks.Abstractions.Results;

namespace CriaCerto.Modules.Calving.Application.Domain;

public class Calf
{
    public Guid Id { get; private set; }
    public string TagId { get; private set; } = string.Empty;
    public Guid MotherCowId { get; private set; }
    public DateTime BirthDate { get; private set; }
    public string Sex { get; private set; } = "M"; // M ou F
    public string Breed { get; private set; } = string.Empty;
    public decimal BirthWeightKg { get; private set; }
    public CalfStatus Status { get; private set; }
    public Guid TenantId { get; private set; }

    private Calf() { }

    public static Result<Calf> Create(
        string tagId,
        Guid motherCowId,
        DateTime birthDate,
        string sex,
        string breed,
        decimal birthWeightKg,
        Guid tenantId)
    {
        if (string.IsNullOrWhiteSpace(tagId))
            return Result.Failure<Calf>(Error.Validation("Calf.TagIdRequired", "O brinco do bezerro é obrigatório."));

        if (birthWeightKg <= 0 || birthWeightKg > 100)
            return Result.Failure<Calf>(Error.Validation("Calf.InvalidBirthWeight", "Peso de nascimento inválido para bezerro bovino."));

        var calf = new Calf
        {
            Id = Guid.NewGuid(),
            TagId = tagId.Trim(),
            MotherCowId = motherCowId,
            BirthDate = birthDate,
            Sex = sex.ToUpperInvariant(),
            Breed = breed.Trim(),
            BirthWeightKg = birthWeightKg,
            Status = CalfStatus.Unweaned,
            TenantId = tenantId
        };

        return Result.Success(calf);
    }

    public Result MarkWeaned()
    {
        if (Status != CalfStatus.Unweaned)
            return Result.Failure(Error.Conflict("Calf.NotUnweaned", "Bezerro não está em estado de mamando/desmamando."));

        Status = CalfStatus.Weaned;
        return Result.Success();
    }
}

public class Calving
{
    public Guid Id { get; private set; }
    public Guid MotherCowId { get; private set; }
    public DateTime CalvingDate { get; private set; }
    public CalvingType Type { get; private set; }
    public Guid CalfId { get; private set; }
    public BirthCondition Condition { get; private set; }
    public Guid TenantId { get; private set; }

    private Calving() { }

    public static Result<Calving> Create(
        Guid motherCowId,
        DateTime calvingDate,
        CalvingType type,
        Guid calfId,
        BirthCondition condition,
        Guid tenantId)
    {
        if (calvingDate > DateTime.UtcNow)
            return Result.Failure<Calving>(Error.Validation("Calving.InvalidDate", "Data de parto não pode ser no futuro."));

        var calving = new Calving
        {
            Id = Guid.NewGuid(),
            MotherCowId = motherCowId,
            CalvingDate = calvingDate,
            Type = type,
            CalfId = calfId,
            Condition = condition,
            TenantId = tenantId
        };

        return Result.Success(calving);
    }
}

public class Weaning
{
    public Guid Id { get; private set; }
    public Guid CalfId { get; private set; }
    public Guid MotherCowId { get; private set; }
    public DateTime WeaningDate { get; private set; }
    public decimal WeaningWeightKg { get; private set; }
    public decimal Adjusted205DayWeightKg { get; private set; }
    public Guid? DestinationLotId { get; private set; }
    public Guid TenantId { get; private set; }

    private Weaning() { }

    public static Result<Weaning> Create(
        Guid calfId,
        Guid motherCowId,
        DateTime weaningDate,
        decimal weaningWeightKg,
        decimal adjusted205DayWeightKg,
        Guid tenantId,
        Guid? destinationLotId = null)
    {
        if (weaningWeightKg <= 0)
            return Result.Failure<Weaning>(Error.Validation("Weaning.InvalidWeight", "Peso ao desmame deve ser positivo."));

        var weaning = new Weaning
        {
            Id = Guid.NewGuid(),
            CalfId = calfId,
            MotherCowId = motherCowId,
            WeaningDate = weaningDate,
            WeaningWeightKg = weaningWeightKg,
            Adjusted205DayWeightKg = adjusted205DayWeightKg,
            DestinationLotId = destinationLotId,
            TenantId = tenantId
        };

        return Result.Success(weaning);
    }
}
