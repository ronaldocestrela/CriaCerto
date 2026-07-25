namespace CriaCerto.Web.Client.Models;

public enum ReproductiveStatus
{
    Open = 1,
    InIatfProtocol = 2,
    Inseminated = 3,
    Pregnant = 4,
    Culled = 5,
    Sold = 6
}

public enum DiagnosisMethod
{
    Ultrasound = 1,
    RectalPalpation = 2
}

public enum CalvingType
{
    Normal = 1,
    Dystocic = 2,
    Cesarean = 3
}

public enum BirthCondition
{
    Live = 1,
    Stillborn = 2
}

public sealed record CattleListResponse<TAnimal>(List<TAnimal> Items, int TotalCount, int Page, int PageSize);

public sealed record CowSummaryDto(
    Guid Id,
    string EarTag,
    string? SisbovId,
    string? RfidTag,
    string Breed,
    ReproductiveStatus Status,
    int ParityCount,
    DateTime? LastCalvingDate,
    double? IepMonths);

public sealed record CowDetailDto(
    Guid Id,
    string EarTag,
    string? SisbovId,
    string? RfidTag,
    string? Tattoo,
    string Breed,
    DateTime BirthDate,
    ReproductiveStatus Status,
    int ParityCount,
    DateTime? LastCalvingDate,
    double? IepMonths,
    int? OpenDays);

public sealed record BullSummaryDto(
    Guid Id,
    string EarTag,
    string Name,
    string Breed,
    string? RegistryNumber,
    bool IsActive);

public sealed record IatfProtocolDto(
    Guid Id,
    string Name,
    DateTime StartDate,
    DateTime InseminationDate,
    Guid SemenBatchId,
    int CowCount);

public sealed record CalvingDto(
    Guid Id,
    Guid MotherCowId,
    DateTime CalvingDate,
    CalvingType Type,
    Guid CalfId,
    string CalfTagId,
    BirthCondition Condition);

public sealed record WeaningDto(
    Guid Id,
    Guid CalfId,
    string CalfTagId,
    Guid MotherCowId,
    DateTime WeaningDate,
    decimal WeaningWeightKg,
    decimal Adjusted205DayWeightKg,
    Guid? DestinationLotId);
