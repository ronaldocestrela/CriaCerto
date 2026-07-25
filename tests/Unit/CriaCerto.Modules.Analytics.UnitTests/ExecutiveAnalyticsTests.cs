using CriaCerto.Modules.Analytics.Application.Contracts;
using CriaCerto.Modules.Analytics.Application.Services;
using FluentAssertions;
using Xunit;

namespace CriaCerto.Modules.Analytics.UnitTests;

public class ExecutiveAnalyticsTests
{
    [Fact]
    public void CalculateExecutiveScorecard_WithValidInputs_ShouldReturnCorrectMetrics()
    {
        // Arrange
        var input = new ExecutiveAnalyticsInput(
            TotalCows: 500,
            PregnantCows: 400,
            CalvesWeaned: 380,
            TotalPastureHectares: 250,
            TotalAnimalUnits: 375,
            AverageGpdKg: 0.850m,
            AverageCostPerArroba: 185.50m,
            AnimalsUnderWithdrawal: 12);

        // Act
        var scorecard = ConsolidatedBovineAnalyticsEngine.CalculateExecutiveScorecard(input);

        // Assert
        scorecard.PregnancyRatePercentage.Should().Be(80.0m); // 400 / 500 = 80%
        scorecard.WeaningRatePercentage.Should().Be(76.0m);   // 380 / 500 = 76%
        scorecard.StockingRateUAPerHa.Should().Be(1.5m);       // 375 / 250 = 1.5 UA/ha
        scorecard.AverageGpdKg.Should().Be(0.850m);
        scorecard.CostPerArrobaProduced.Should().Be(185.50m);
        scorecard.AnimalsUnderSlaughterWithdrawal.Should().Be(12);
        scorecard.OverallHealthStatus.Should().Be("Atenção Sanitária");
    }

    [Fact]
    public void ExportBovineReport_ShouldGenerateValidCsvContent()
    {
        // Arrange
        var scorecard = new ExecutiveScorecardDto(
            PregnancyRatePercentage: 82.5m,
            WeaningRatePercentage: 78.0m,
            StockingRateUAPerHa: 1.6m,
            AverageGpdKg: 0.910m,
            CostPerArrobaProduced: 178.00m,
            AnimalsUnderSlaughterWithdrawal: 5,
            OverallHealthStatus: "Ótimo");

        // Act
        var csvContent = ConsolidatedBovineAnalyticsEngine.ExportToCsv(scorecard);

        // Assert
        csvContent.Should().Contain("Métrica,Valor");
        csvContent.Should().Contain("Taxa de Prenhez (%),82.5");
        csvContent.Should().Contain("Taxa de Lotação (UA/ha),1.6");
        csvContent.Should().Contain("Animais em Carência Sanitária,5");
    }

    [Fact]
    public async Task Handle_ExportQuery_WithExcelFormat_ShouldReturnExcelFileContent()
    {
        // Arrange
        var scorecard = new ExecutiveScorecardDto(80m, 75m, 1.5m, 0.85m, 180m, 0, "Excelente");
        var query = new ExportBovineReportQuery(
            Scorecard: scorecard,
            ReportType: ReportTypeEnum.HerdInventory,
            Format: ReportFormatEnum.Excel);

        var handler = new ExportBovineReportQueryHandler();

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.ContentType.Should().Be("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
        result.Value.FileName.Should().Contain("criacerto_inventario_rebanho_");
        result.Value.FileName.Should().EndWith(".xlsx");

        string excelString = System.Text.Encoding.UTF8.GetString(result.Value.FileContents);
        excelString.Should().Contain("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
        excelString.Should().Contain("<Data ss:Type=\"String\">Categoria</Data>");
        excelString.Should().Contain("<Data ss:Type=\"String\">Quantidade</Data>");
    }

    [Fact]
    public async Task Handle_ExportQuery_WithPdfFormat_ShouldReturnPdfFileContent()
    {
        // Arrange
        var scorecard = new ExecutiveScorecardDto(80m, 75m, 1.5m, 0.85m, 180m, 0, "Excelente");
        var query = new ExportBovineReportQuery(
            Scorecard: scorecard,
            ReportType: ReportTypeEnum.GtaSupport,
            Format: ReportFormatEnum.Pdf);

        var handler = new ExportBovineReportQueryHandler();

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.ContentType.Should().Be("application/pdf");
        result.Value.FileName.Should().Contain("criacerto_suporte_gta_");
        result.Value.FileName.Should().EndWith(".pdf");

        string pdfHeader = System.Text.Encoding.UTF8.GetString(result.Value.FileContents, 0, 8);
        pdfHeader.Should().Be("%PDF-1.4");
    }

    [Fact]
    public async Task Handle_ExportQuery_WithInvalidCustomDateRange_ShouldReturnValidationError()
    {
        // Arrange
        var scorecard = new ExecutiveScorecardDto(80m, 75m, 1.5m, 0.85m, 180m, 0, "Excelente");
        var query = new ExportBovineReportQuery(
            Scorecard: scorecard,
            PeriodType: PeriodTypeEnum.CustomRange,
            StartDate: DateTime.UtcNow.AddDays(5),
            EndDate: DateTime.UtcNow);

        var handler = new ExportBovineReportQueryHandler();

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Analytics.InvalidPeriod");
        result.Error.Message.Should().Contain("A data inicial do relatório não pode ser posterior à data final");
    }
}

