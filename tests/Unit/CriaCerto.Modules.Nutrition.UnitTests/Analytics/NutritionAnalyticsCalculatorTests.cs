using CriaCerto.Modules.Nutrition.Application.Domain.Services;
using FluentAssertions;
using Xunit;

namespace CriaCerto.Modules.Nutrition.UnitTests.Analytics;

public class NutritionAnalyticsCalculatorTests
{
    [Fact]
    public void CalculateFeedConversion_WithValidInput_ShouldCalculateCAAndEA()
    {
        // Arrange
        decimal totalDryMatterIntakeKg = 900m; // 900 kg MS consumida
        decimal totalWeightGainKg = 150m;       // 150 kg ganho de peso total

        // Act
        var (ca, ea) = NutritionAnalyticsCalculator.CalculateFeedConversion(totalDryMatterIntakeKg, totalWeightGainKg);

        // Assert
        // CA = 900 / 150 = 6.00 kg MS / kg peso vivo
        ca.Should().Be(6.00m);
        // EA = 150 / 900 = 0.1667 (16.67% de eficiência)
        ea.Should().BeApproximately(0.1667m, 0.0001m);
    }

    [Fact]
    public void CalculateFeedConversion_WithZeroWeightGain_ShouldReturnZeroCA()
    {
        // Arrange & Act
        var (ca, ea) = NutritionAnalyticsCalculator.CalculateFeedConversion(500m, 0m);

        // Assert
        ca.Should().Be(0m);
        ea.Should().Be(0m);
    }

    [Fact]
    public void CalculateCostPerArroba_WithStandardCarcassYield_ShouldCalculateCostPerArroba()
    {
        // Arrange
        decimal totalNutritionCost = 3000m; // R$ 3.000 em ração/suplemento
        decimal totalWeightGainKg = 300m;    // 300 kg ganho peso vivo
        decimal? carcassYieldPercentage = 50m; // 50% de rendimento de carcaça -> 150 kg carcaça / 15 = 10 @

        // Act
        var result = NutritionAnalyticsCalculator.CalculateCostPerArroba(
            totalNutritionCost,
            totalWeightGainKg,
            carcassYieldPercentage);

        // Assert
        // 300 kg ganho * 50% / 15 = 10 Arrobas ganhas
        // Custo por @ = R$ 3000 / 10 @ = R$ 300.00 / @
        result.ArrobasProduced.Should().Be(10m);
        result.CostPerArroba.Should().Be(300.00m);
    }

    [Fact]
    public void CalculateCostPerArroba_WithHighCarcassYield_ShouldCalculateAccurateArrobas()
    {
        // Arrange
        decimal totalNutritionCost = 2700m; // R$ 2.700 em ração
        decimal totalWeightGainKg = 150m;    // 150 kg ganho peso vivo
        decimal? carcassYieldPercentage = 54m; // 54% RC (ex: boi magro em confinamento)

        // Act
        var result = NutritionAnalyticsCalculator.CalculateCostPerArroba(
            totalNutritionCost,
            totalWeightGainKg,
            carcassYieldPercentage);

        // Assert
        // 150 * 0.54 / 15 = 5.4 @
        // Custo por @ = 2700 / 5.4 = 500.00 / @
        result.ArrobasProduced.Should().Be(5.4m);
        result.CostPerArroba.Should().Be(500.00m);
    }
}
