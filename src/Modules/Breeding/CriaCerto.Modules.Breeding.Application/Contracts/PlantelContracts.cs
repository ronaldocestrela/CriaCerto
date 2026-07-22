using CriaCerto.Modules.Breeding.Application.Domain;

namespace CriaCerto.Modules.Breeding.Application.Contracts;

public sealed record PlantelListResponse<TAnimal>(IReadOnlyList<TAnimal> Items, int TotalCount, int Page, int PageSize);

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
    IReadOnlyList<PlantelEventDto> Events);

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
    IReadOnlyList<PlantelEventDto> Events);
