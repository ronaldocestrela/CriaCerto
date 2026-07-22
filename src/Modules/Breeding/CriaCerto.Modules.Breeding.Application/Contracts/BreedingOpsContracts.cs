using CriaCerto.Modules.Breeding.Application.Domain;

namespace CriaCerto.Modules.Breeding.Application.Contracts;

public sealed record BreedingBatchLineDto(
    string TagId,
    BreedingMethod Method,
    string? BoarOrSemenRef,
    string? Technician,
    BodyConditionScore? BodyConditionScoreAtBreeding,
    string? Location,
    string? Notes);

public sealed record RegisterBreedingBatchRequest(
    DateOnly EventDate,
    IReadOnlyList<BreedingBatchLineDto> Lines);

public sealed record BreedingEventDto(
    Guid Id,
    Guid SowId,
    string SowTagId,
    DateOnly EventDate,
    BreedingMethod Method,
    string? BoarOrSemenRef,
    string? Technician,
    BodyConditionScore? BodyConditionScoreAtBreeding,
    string? Location);

public sealed record RegisterBreedingBatchResponse(
    IReadOnlyList<BreedingEventDto> Events,
    int RegisteredCount);

public sealed record RegisterPregnancyDiagnosisRequest(
    Guid SowId,
    DateOnly DiagnosisDate,
    PregnancyDiagnosisMethod Method,
    PregnancyDiagnosisResult Result,
    string? Notes);

public sealed record PregnancyDiagnosisDto(
    Guid Id,
    Guid SowId,
    string SowTagId,
    Guid? BreedingEventId,
    DateOnly DiagnosisDate,
    PregnancyDiagnosisMethod Method,
    PregnancyDiagnosisResult Result,
    ReproductiveStatus NewReproductiveStatus,
    int UpdatedDnpDays);

public sealed record PregnancyCheckTaskDto(
    Guid SowId,
    string TagId,
    string? Nickname,
    string? Location,
    Guid BreedingEventId,
    DateOnly BreedingDate,
    int DaysPostBreeding,
    string? Technician,
    BreedingMethod BreedingMethod,
    int DnpDays,
    bool RequiresAttention);

public sealed record PregnancyCheckQueueResponse(
    IReadOnlyList<PregnancyCheckTaskDto> Items,
    int TotalCount,
    int PendingCount,
    int DnpAlertCount,
    int Page,
    int PageSize);
