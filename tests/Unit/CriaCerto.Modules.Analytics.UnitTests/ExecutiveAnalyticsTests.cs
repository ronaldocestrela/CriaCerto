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
}
