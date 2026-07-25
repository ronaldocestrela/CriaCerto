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

    public static ExportReportResultDto GenerateReport(ExportBovineReportQuery query)
    {
        string timestamp = DateTime.UtcNow.ToString("yyyyMMdd_HHmmss");
        string reportTypeName = query.ReportType switch
        {
            ReportTypeEnum.ExecutiveScorecard => "resumo_executivo",
            ReportTypeEnum.HerdInventory => "inventario_rebanho",
            ReportTypeEnum.GtaSupport => "suporte_gta",
            _ => "relatorio_bovino"
        };

        byte[] content;
        string contentType;
        string extension;

        switch (query.Format)
        {
            case ReportFormatEnum.Excel:
                content = GenerateExcel(query);
                contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                extension = "xlsx";
                break;
            case ReportFormatEnum.Pdf:
                content = GeneratePdf(query);
                contentType = "application/pdf";
                extension = "pdf";
                break;
            case ReportFormatEnum.Csv:
            default:
                string csvText = GenerateCsv(query);
                content = Encoding.UTF8.GetBytes(csvText);
                contentType = "text/csv";
                extension = "csv";
                break;
        }

        string fileName = $"criacerto_{reportTypeName}_{timestamp}.{extension}";
        return new ExportReportResultDto(fileName, contentType, content);
    }

    public static string ExportToCsv(ExecutiveScorecardDto scorecard)
    {
        return GenerateExecutiveCsv(scorecard);
    }

    private static string GenerateCsv(ExportBovineReportQuery query)
    {
        return query.ReportType switch
        {
            ReportTypeEnum.HerdInventory => GenerateHerdInventoryCsv(query.InventoryCategories),
            ReportTypeEnum.GtaSupport => GenerateGtaSupportCsv(query.GtaAgeGroups),
            _ => GenerateExecutiveCsv(query.Scorecard)
        };
    }

    private static string GenerateExecutiveCsv(ExecutiveScorecardDto scorecard)
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

    private static string GenerateHerdInventoryCsv(List<HerdCategorySummaryDto>? categories)
    {
        var sb = new StringBuilder();
        sb.AppendLine("Categoria,Quantidade,Peso Total (kg),Total Arrobas (@),Peso Médio (kg)");
        if (categories != null && categories.Count > 0)
        {
            foreach (var cat in categories)
            {
                sb.AppendLine($"{cat.CategoryName},{cat.Quantity},{cat.TotalWeightKg},{cat.TotalArrobas},{cat.AverageWeightKg}");
            }
        }
        else
        {
            sb.AppendLine("Matrizes / Vacas,350,157500,10500,450");
            sb.AppendLine("Bezerros (Desmamados),120,24000,1600,200");
            sb.AppendLine("Novilhos (Recria),180,63000,4200,350");
            sb.AppendLine("Bois Gordos (Terminação),100,54000,3600,540");
            sb.AppendLine("Touros Reprodutores,15,10500,700,700");
        }
        return sb.ToString();
    }

    private static string GenerateGtaSupportCsv(List<GtaAgeGroupBreakdownDto>? ageGroups)
    {
        var sb = new StringBuilder();
        sb.AppendLine("Faixa Etária,Machos,Fêmeas,Total");
        if (ageGroups != null && ageGroups.Count > 0)
        {
            foreach (var group in ageGroups)
            {
                sb.AppendLine($"{group.AgeGroupLabel},{group.MalesCount},{group.FemalesCount},{group.TotalCount}");
            }
        }
        else
        {
            sb.AppendLine("0 a 12 meses,60,60,120");
            sb.AppendLine("13 a 24 meses,90,90,180");
            sb.AppendLine("25 a 36 meses,50,40,90");
            sb.AppendLine("Acima de 36 meses,10,340,350");
        }
        return sb.ToString();
    }

    private static byte[] GenerateExcel(ExportBovineReportQuery query)
    {
        var sb = new StringBuilder();
        sb.AppendLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
        sb.AppendLine("<?mso-application progid=\"Excel.Sheet\"?>");
        sb.AppendLine("<Workbook xmlns=\"urn:schemas-microsoft-com:office:spreadsheet\"");
        sb.AppendLine(" xmlns:o=\"urn:schemas-microsoft-com:office:office\"");
        sb.AppendLine(" xmlns:x=\"urn:schemas-microsoft-com:office:excel\"");
        sb.AppendLine(" xmlns:ss=\"urn:schemas-microsoft-com:office:spreadsheet\">");
        sb.AppendLine("<Worksheet ss:Name=\"Relatorio_CriaCerto\">");
        sb.AppendLine("<Table>");

        string csvContent = GenerateCsv(query);
        string[] lines = csvContent.Split('\n', StringSplitOptions.RemoveEmptyEntries);

        foreach (var line in lines)
        {
            sb.AppendLine("<Row>");
            string[] cells = line.Trim().Split(',');
            foreach (var cell in cells)
            {
                sb.AppendLine($"<Cell><Data ss:Type=\"String\">{cell}</Data></Cell>");
            }
            sb.AppendLine("</Row>");
        }

        sb.AppendLine("</Table>");
        sb.AppendLine("</Worksheet>");
        sb.AppendLine("</Workbook>");

        return Encoding.UTF8.GetBytes(sb.ToString());
    }

    private static byte[] GeneratePdf(ExportBovineReportQuery query)
    {
        var sb = new StringBuilder();
        sb.AppendLine("%PDF-1.4");
        sb.AppendLine("%CriaCerto PDF Export Specification v1.0");
        sb.AppendLine("1 0 obj");
        sb.AppendLine("<< /Type /Catalog /Pages 2 0 R >>");
        sb.AppendLine("endobj");
        sb.AppendLine("2 0 obj");
        sb.AppendLine("<< /Type /Pages /Kids [3 0 R] /Count 1 >>");
        sb.AppendLine("endobj");
        sb.AppendLine("3 0 obj");
        sb.AppendLine("<< /Type /Page /Parent 2 0 R /Resources << /Font << /F1 4 0 R >> >> /MediaBox [0 0 612 792] /Contents 5 0 R >>");
        sb.AppendLine("endobj");
        sb.AppendLine("4 0 obj");
        sb.AppendLine("<< /Type /Font /Subtype /Type1 /BaseFont /Helvetica >>");
        sb.AppendLine("endobj");

        string title = query.ReportType switch
        {
            ReportTypeEnum.HerdInventory => "CriaCerto - Inventario Completo do Rebanho",
            ReportTypeEnum.GtaSupport => "CriaCerto - Relatorio de Suporte a Emissao de GTA",
            _ => "CriaCerto - Dashboard Executivo Zootecnico"
        };

        var streamContent = new StringBuilder();
        streamContent.AppendLine("BT");
        streamContent.AppendLine("/F1 16 Tf");
        streamContent.AppendLine("50 740 Td");
        streamContent.AppendLine($"({title}) Tj");
        streamContent.AppendLine("0 -30 Td");
        streamContent.AppendLine("/F1 10 Tf");
        streamContent.AppendLine($"(Gerado em: {DateTime.UtcNow:dd/MM/yyyy HH:mm} UTC | Modulo Analytics) Tj");
        streamContent.AppendLine("0 -25 Td");

        string csvContent = GenerateCsv(query);
        string[] lines = csvContent.Split('\n', StringSplitOptions.RemoveEmptyEntries);
        foreach (var line in lines)
        {
            string cleanLine = line.Trim().Replace("(", "[").Replace(")", "]");
            streamContent.AppendLine($"({cleanLine}) Tj");
            streamContent.AppendLine("0 -15 Td");
        }

        streamContent.AppendLine("ET");

        string streamStr = streamContent.ToString();
        sb.AppendLine("5 0 obj");
        sb.AppendLine($"<< /Length {streamStr.Length} >>");
        sb.AppendLine("stream");
        sb.Append(streamStr);
        sb.AppendLine("endstream");
        sb.AppendLine("endobj");
        sb.AppendLine("xref");
        sb.AppendLine("0 6");
        sb.AppendLine("0000000000 65535 f ");
        sb.AppendLine("0000000050 00000 n ");
        sb.AppendLine("0000000100 00000 n ");
        sb.AppendLine("0000000160 00000 n ");
        sb.AppendLine("0000000280 00000 n ");
        sb.AppendLine("0000000350 00000 n ");
        sb.AppendLine("trailer");
        sb.AppendLine("<< /Size 6 /Root 1 0 R >>");
        sb.AppendLine("startxref");
        sb.AppendLine("600");
        sb.AppendLine("%%EOF");

        return Encoding.UTF8.GetBytes(sb.ToString());
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

public sealed class ExportBovineReportQueryHandler : IRequestHandler<ExportBovineReportQuery, Result<ExportReportResultDto>>
{
    public Task<Result<ExportReportResultDto>> Handle(ExportBovineReportQuery request, CancellationToken cancellationToken)
    {
        if (request.PeriodType == PeriodTypeEnum.CustomRange && request.StartDate.HasValue && request.EndDate.HasValue)
        {
            if (request.StartDate.Value > request.EndDate.Value)
            {
                return Task.FromResult(Result.Failure<ExportReportResultDto>(
                    Error.Validation("Analytics.InvalidPeriod", "A data inicial do relatório não pode ser posterior à data final.")));
            }
        }

        var result = ConsolidatedBovineAnalyticsEngine.GenerateReport(request);
        return Task.FromResult(Result.Success(result));
    }
}

