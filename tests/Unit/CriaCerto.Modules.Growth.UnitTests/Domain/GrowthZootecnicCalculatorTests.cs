using CriaCerto.Modules.Growth.Application.Domain.Services;
using FluentAssertions;
using Xunit;

namespace CriaCerto.Modules.Growth.UnitTests.Domain;

public class GrowthZootecnicCalculatorTests
{
    [Fact]
    public void CalculateArrobas_With300KgAnd50PercentYield_ShouldReturn10Arrobas()
    {
        // 300 kg * 0.50 / 15 kg/@ = 10 @
        decimal arrobas = GrowthZootecnicCalculator.CalculateArrobas(300.0m, 50.0m);
        arrobas.Should().Be(10.0m);
    }

    [Fact]
    public void CalculateArrobas_With450KgAnd52PercentYield_ShouldReturn15_6Arrobas()
    {
        // 450 kg * 0.52 / 15 kg/@ = 15.6 @
        decimal arrobas = GrowthZootecnicCalculator.CalculateArrobas(450.0m, 52.0m);
        arrobas.Should().Be(15.6m);
    }

    [Fact]
    public void CalculateAdg_With30KgGainOver30Days_ShouldReturn1KgPerDay()
    {
        // 330 kg - 300 kg = 30 kg / 30 days = 1.0 kg/day
        var prevDate = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var currDate = new DateTime(2026, 1, 31, 0, 0, 0, DateTimeKind.Utc);

        decimal gpd = GrowthZootecnicCalculator.CalculateAdg(330.0m, 300.0m, currDate, prevDate);
        gpd.Should().Be(1.0m);
    }

    [Fact]
    public void CalculateAdg_WithWeightLoss_ShouldReturnNegativeGpd()
    {
        // 290 kg - 300 kg = -10 kg / 10 days = -1.0 kg/day
        var prevDate = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var currDate = new DateTime(2026, 1, 11, 0, 0, 0, DateTimeKind.Utc);

        decimal gpd = GrowthZootecnicCalculator.CalculateAdg(290.0m, 300.0m, currDate, prevDate);
        gpd.Should().Be(-1.0m);
    }

    [Fact]
    public void CalculateAdg_WithSameDay_ShouldReturnZero()
    {
        var date = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        decimal gpd = GrowthZootecnicCalculator.CalculateAdg(300.0m, 300.0m, date, date);
        gpd.Should().Be(0.0m);
    }

    [Fact]
    public void CalculateMonthlyArrobaGain_With1KgPerDayAnd50PercentYield_ShouldReturn1point0ArrobaPerMonth()
    {
        // 1.0 kg/day * 30 days = 30 kg * 0.50 / 15 = 1.0 @/mês
        decimal monthlyArrobaGain = GrowthZootecnicCalculator.CalculateMonthlyArrobaGain(1.0m, 50.0m);
        monthlyArrobaGain.Should().Be(1.0m);
    }
}
