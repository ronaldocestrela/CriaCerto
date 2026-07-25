using CriaCerto.BuildingBlocks.Abstractions.Results;
using CriaCerto.BuildingBlocks.Application.Abstractions.Messaging;
using MediatR;

namespace CriaCerto.Modules.Analytics.Application.Contracts;

public sealed record ExecutiveAnalyticsInput(
    int TotalCows,
    int PregnantCows,
    int CalvesWeaned,
    decimal TotalPastureHectares,
    decimal TotalAnimalUnits,
    decimal AverageGpdKg,
    decimal AverageCostPerArroba,
    int AnimalsUnderWithdrawal);

public sealed record ExecutiveScorecardDto(
    decimal PregnancyRatePercentage,
    decimal WeaningRatePercentage,
    decimal StockingRateUAPerHa,
    decimal AverageGpdKg,
    decimal CostPerArrobaProduced,
    int AnimalsUnderSlaughterWithdrawal,
    string OverallHealthStatus);

public sealed record GetExecutiveAnalyticsQuery(
    int TotalCows,
    int PregnantCows,
    int CalvesWeaned,
    decimal TotalPastureHectares,
    decimal TotalAnimalUnits,
    decimal AverageGpdKg,
    decimal AverageCostPerArroba,
    int AnimalsUnderWithdrawal) : IQuery<ExecutiveScorecardDto>;

public sealed record ExportBovineReportQuery(
    ExecutiveScorecardDto Scorecard) : IQuery<string>;
