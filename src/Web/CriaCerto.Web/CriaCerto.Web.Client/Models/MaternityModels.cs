namespace CriaCerto.Web.Client.Models;

public sealed record RegisterFarrowingRequest(
    Guid SowId,
    DateTime FarrowingDate,
    int LiveBorn,
    int Stillborn,
    int Mummified,
    decimal LitterWeightKg,
    string? MaternityRoomId,
    bool Assisted,
    string? Notes);

public sealed record FarrowingClientDto(
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

public sealed record FarrowingSummaryClientDto(
    Guid Id,
    Guid SowId,
    DateTime FarrowingDate,
    int LiveBorn,
    int Stillborn,
    int Mummified,
    int TotalBorn,
    decimal LitterWeightKg,
    string? MaternityRoomId);
