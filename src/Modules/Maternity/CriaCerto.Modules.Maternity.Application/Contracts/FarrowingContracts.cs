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

public sealed record PigletTransferDto(
    Guid Id,
    Guid TenantId,
    Guid SourceFarrowingId,
    Guid SourceSowId,
    Guid TargetFarrowingId,
    Guid TargetSowId,
    int Quantity,
    DateTime TransferDate,
    string? Notes);

public sealed record WeaningDto(
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

public sealed record MaternityMetricsDto(
    decimal Nvma, // Nascidos Vivos / Matriz / Ano
    decimal Dma,  // Desmamados / Matriz / Ano
    decimal PreWeaningMortalityRate, // % Mortalidade Pré-Desmame
    int TotalActiveSows,
    int TotalLiveBornInPeriod,
    int TotalWeanedInPeriod,
    int TotalTransferredInPeriod);

