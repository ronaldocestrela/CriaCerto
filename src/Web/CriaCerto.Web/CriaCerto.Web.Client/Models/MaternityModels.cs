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

public sealed record TransferPigletRequest(
    Guid SourceFarrowingId,
    Guid TargetFarrowingId,
    int Quantity,
    DateTime TransferDate,
    string? Notes);

public sealed record PigletTransferClientDto(
    Guid Id,
    Guid TenantId,
    Guid SourceFarrowingId,
    Guid SourceSowId,
    Guid TargetFarrowingId,
    Guid TargetSowId,
    int Quantity,
    DateTime TransferDate,
    string? Notes);

public sealed record RegisterWeaningRequest(
    Guid FarrowingId,
    DateTime WeaningDate,
    int WeanedCount,
    decimal TotalWeanedWeightKg,
    string DestinationPenOrBatch,
    string? Notes);

public sealed record WeaningClientDto(
    Guid Id,
    Guid TenantId,
    Guid FarrowingId,
    Guid SowId,
    DateTime WeaningDate,
    int WeanedCount,
    decimal TotalWeanedWeightKg,
    decimal AverageWeanedWeightKg,
    string DestinationPenOrBatch,
    string? Notes);

public sealed record MaternityMetricsClientDto(
    decimal Nvma,
    decimal Dma,
    decimal PreWeaningMortalityRate,
    int TotalActiveSows,
    int TotalLiveBornInPeriod,
    int TotalWeanedInPeriod,
    int TotalTransferredInPeriod);

