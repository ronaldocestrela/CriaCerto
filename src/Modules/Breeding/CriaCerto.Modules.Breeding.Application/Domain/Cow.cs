using CriaCerto.BuildingBlocks.Abstractions.Results;

namespace CriaCerto.Modules.Breeding.Application.Domain;

public class Cow
{
    public Guid Id { get; private set; }
    public string EarTag { get; private set; } = string.Empty;
    public string? SisbovId { get; private set; }
    public string? RfidTag { get; private set; }
    public string? Tattoo { get; private set; }
    public string Breed { get; private set; } = string.Empty;
    public DateTime BirthDate { get; private set; }
    public ReproductiveStatus Status { get; private set; }
    public int ParityCount { get; private set; }
    public DateTime? LastCalvingDate { get; private set; }
    public Guid TenantId { get; private set; }

    private Cow() { }

    public static Result<Cow> Create(
        string earTag,
        string breed,
        DateTime birthDate,
        Guid tenantId,
        string? sisbovId = null,
        string? rfidTag = null,
        string? tattoo = null)
    {
        if (string.IsNullOrWhiteSpace(earTag))
            return Result.Failure<Cow>(Error.Validation("Cow.EarTagRequired", "O brinco de identificação da vaca é obrigatório."));

        if (string.IsNullOrWhiteSpace(breed))
            return Result.Failure<Cow>(Error.Validation("Cow.BreedRequired", "A raça da matriz é obrigatória."));

        if (birthDate > DateTime.UtcNow)
            return Result.Failure<Cow>(Error.Validation("Cow.InvalidBirthDate", "Data de nascimento não pode ser no futuro."));

        var cow = new Cow
        {
            Id = Guid.NewGuid(),
            EarTag = earTag.Trim(),
            Breed = breed.Trim(),
            BirthDate = birthDate,
            Status = ReproductiveStatus.Open,
            ParityCount = 0,
            LastCalvingDate = null,
            TenantId = tenantId,
            SisbovId = sisbovId?.Trim(),
            RfidTag = rfidTag?.Trim(),
            Tattoo = tattoo?.Trim()
        };

        return Result.Success(cow);
    }

    public Result StartIatfProtocol(Guid protocolId)
    {
        if (Status == ReproductiveStatus.Pregnant)
            return Result.Failure(Error.Conflict("Cow.AlreadyPregnant", "Matriz já está confirmada prenhe. Não é possível iniciar IATF."));

        if (Status == ReproductiveStatus.Culled || Status == ReproductiveStatus.Sold)
            return Result.Failure(Error.Conflict("Cow.Inactive", "Matriz inativa (descartada ou vendida)."));

        Status = ReproductiveStatus.InIatfProtocol;
        return Result.Success();
    }

    public Result RecordInsemination(DateTime inseminationDate, string semenBatchCode)
    {
        if (string.IsNullOrWhiteSpace(semenBatchCode))
            return Result.Failure(Error.Validation("Cow.SemenBatchRequired", "O código do lote de sêmen é obrigatório."));

        if (Status == ReproductiveStatus.Pregnant)
            return Result.Failure(Error.Conflict("Cow.AlreadyPregnant", "Matriz já está prenhe."));

        Status = ReproductiveStatus.Inseminated;
        return Result.Success();
    }

    public Result RecordPregnancyDiagnosis(bool isPregnant, DateTime diagnosisDate)
    {
        if (Status == ReproductiveStatus.Culled || Status == ReproductiveStatus.Sold)
            return Result.Failure(Error.Conflict("Cow.Inactive", "Matriz inativa."));

        Status = isPregnant ? ReproductiveStatus.Pregnant : ReproductiveStatus.Open;
        return Result.Success();
    }

    public Result RecordCalving(DateTime calvingDate)
    {
        if (calvingDate > DateTime.UtcNow)
            return Result.Failure(Error.Validation("Cow.InvalidCalvingDate", "Data de parto inválida."));

        ParityCount++;
        LastCalvingDate = calvingDate;
        Status = ReproductiveStatus.Open;
        return Result.Success();
    }
}
