using System.Text;
using CriaCerto.BuildingBlocks.Abstractions.Results;
using CriaCerto.Modules.Analytics.Application.Contracts;
using MediatR;

namespace CriaCerto.Modules.Analytics.Application.Services;

public static class ConsolidatedBovineAnalyticsEngine
{
    public static ExecutiveScorecardDto CalculateExecutiveScorecard(ExecutiveAnalyticsInput input)
    {
        decimal pregnancyRate = input.TotalCows > 0
            ? Math.Round((decimal)input.PregnantCows / input.TotalCows * 100, 2)
            : 0;

        decimal weaningRate = input.TotalCows > 0
            ? Math.Round((decimal)input.CalvesWeaned / input.TotalCows * 100, 2)
            : 0;

        decimal stockingRate = input.TotalPastureHectares > 0
            ? Math.Round(input.TotalAnimalUnits / input.TotalPastureHectares, 2)
            : 0;

        string healthStatus = input.AnimalsUnderWithdrawal == 0
            ? "Excelente"
            : input.AnimalsUnderWithdrawal <= 10
                ? "Ótimo"
                : "Atenção Sanitária";

        return new ExecutiveScorecardDto(
            PregnancyRatePercentage: pregnancyRate,
            WeaningRatePercentage: weaningRate,
            StockingRateUAPerHa: stockingRate,
            AverageGpdKg: input.AverageGpdKg,
            CostPerArrobaProduced: input.AverageCostPerArroba,
            AnimalsUnderSlaughterWithdrawal: input.AnimalsUnderWithdrawal,
            OverallHealthStatus: healthStatus);
    }

    public static string ExportToCsv(ExecutiveScorecardDto scorecard)
    {
        var sb = new StringBuilder();
        sb.AppendLine("Métrica,Valor");
        sb.AppendLine($"Taxa de Prenhez (%),{scorecard.PregnancyRatePercentage}");
        sb.AppendLine($"Taxa de Desmame (%),{scorecard.WeaningRatePercentage}");
        sb.AppendLine($"Taxa de Lotação (UA/ha),{scorecard.StockingRateUAPerHa}");
        sb.AppendLine($"Ganho de Peso Diário (GPD kg),{scorecard.AverageGpdKg}");
        sb.AppendLine($"Custo por Arroba (R$),{scorecard.CostPerArrobaProduced}");
        sb.AppendLine($"Animais em Carência Sanitária,{scorecard.AnimalsUnderSlaughterWithdrawal}");
        sb.AppendLine($"Status de Saúde Sanitária,{scorecard.OverallHealthStatus}");

        return sb.ToString();
    }
}

public sealed class GetExecutiveAnalyticsQueryHandler : IRequestHandler<GetExecutiveAnalyticsQuery, Result<ExecutiveScorecardDto>>
{
    public Task<Result<ExecutiveScorecardDto>> Handle(GetExecutiveAnalyticsQuery request, CancellationToken cancellationToken)
    {
        var input = new ExecutiveAnalyticsInput(
            request.TotalCows,
            request.PregnantCows,
            request.CalvesWeaned,
            request.TotalPastureHectares,
            request.TotalAnimalUnits,
            request.AverageGpdKg,
            request.AverageCostPerArroba,
            request.AnimalsUnderWithdrawal);

        var scorecard = ConsolidatedBovineAnalyticsEngine.CalculateExecutiveScorecard(input);
        return Task.FromResult(Result.Success(scorecard));
    }
}

public sealed class ExportBovineReportQueryHandler : IRequestHandler<ExportBovineReportQuery, Result<string>>
{
    public Task<Result<string>> Handle(ExportBovineReportQuery request, CancellationToken cancellationToken)
    {
        var csv = ConsolidatedBovineAnalyticsEngine.ExportToCsv(request.Scorecard);
        return Task.FromResult(Result.Success(csv));
    }
}
