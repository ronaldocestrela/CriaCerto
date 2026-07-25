namespace CriaCerto.Modules.Maternity.Application.Contracts;

public sealed record FarrowingDto(
    Guid Id,
    Guid SowId,
    Guid TenantId,
    DateTime FarrowingDate,
    int LiveBorn,
    int Stillborn,
    int Mummified,
    int TotalBorn,
    decimal LitterWeightKg,
    decimal AveragePigletWeightKg,
    string? MaternityRoomId,
    bool Assisted,
    string? Notes);

public sealed record FarrowingSummaryDto(
    Guid Id,
    Guid SowId,
    DateTime FarrowingDate,
    int LiveBorn,
    int Stillborn,
    int Mummified,
    int TotalBorn,
    decimal LitterWeightKg,
    string? MaternityRoomId);
