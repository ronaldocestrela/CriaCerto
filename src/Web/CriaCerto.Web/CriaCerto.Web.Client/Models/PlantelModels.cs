namespace CriaCerto.Web.Client.Models;

public enum LifecycleStatus
{
    Active = 1,
    Quarantine = 2,
    Culled = 3
}

public enum ReproductiveStatus
{
    Empty = 1,
    Bred = 2,
    Pregnant = 3,
    Lactating = 4
}

public enum BodyConditionScore
{
    VeryThin = 1,
    Thin = 2,
    Ideal = 3,
    Fat = 4,
    VeryFat = 5
}

public sealed record PlantelListResponse<TAnimal>(List<TAnimal> Items, int TotalCount, int Page, int PageSize);

public sealed record SowSummaryDto(
    Guid Id,
    string TagId,
    string? Nickname,
    string Breed,
    ReproductiveStatus ReproductiveStatus,
    LifecycleStatus LifecycleStatus,
    int Parity,
    int DnpDays,
    string? LastEventName,
    DateOnly? LastEventDate,
    string? Location,
    bool RequiresAttention);

public sealed record BoarSummaryDto(
    Guid Id,
    string TagId,
    string? Nickname,
    string Breed,
    LifecycleStatus LifecycleStatus,
    decimal CurrentWeightKg,
    string? LastEventName,
    DateOnly? LastEventDate,
    string? Location,
    bool RequiresAttention);

public sealed record PlantelEventDto(string EventType, string Title, DateOnly EventDate, string? Notes);

public sealed record SowDetailDto(
    Guid Id,
    string TagId,
    string? Nickname,
    string? PbbRegistration,
    string Breed,
    string? Origin,
    DateOnly? BirthDate,
    DateOnly EntryDate,
    decimal EntryWeightKg,
    decimal CurrentWeightKg,
    decimal? AverageDailyGain,
    string? FatherTagId,
    string? MotherTagId,
    BodyConditionScore BodyConditionScore,
    int Parity,
    int DnpDays,
    ReproductiveStatus ReproductiveStatus,
    LifecycleStatus LifecycleStatus,
    string? Location,
    bool RequiresAttention,
    List<PlantelEventDto> Events);

public sealed record BoarDetailDto(
    Guid Id,
    string TagId,
    string? Nickname,
    string? PbbRegistration,
    string Breed,
    string? Origin,
    DateOnly? BirthDate,
    DateOnly EntryDate,
    decimal EntryWeightKg,
    decimal CurrentWeightKg,
    decimal? AverageDailyGain,
    string? FatherTagId,
    string? MotherTagId,
    BodyConditionScore BodyConditionScore,
    LifecycleStatus LifecycleStatus,
    string? Location,
    bool RequiresAttention,
    List<PlantelEventDto> Events);
