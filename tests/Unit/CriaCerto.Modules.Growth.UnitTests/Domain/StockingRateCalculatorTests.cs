using CriaCerto.Modules.Growth.Application.Domain.Services;
using FluentAssertions;
using Xunit;

namespace CriaCerto.Modules.Growth.UnitTests.Domain;

public class StockingRateCalculatorTests
{
    [Fact]
    public void CalculateTotalUA_With4500Kg_ShouldReturn10UA()
    {
        // 4500 kg / 450 = 10 UA
        var ua = StockingRateCalculator.CalculateTotalUA(4500.0m);
        ua.Should().Be(10.0m);
    }

    [Fact]
    public void CalculateStockingRate_With20UAAnd10Hectares_ShouldReturn2UAPerHa()
    {
        // 20 UA / 10 ha = 2.00 UA/ha
        var rate = StockingRateCalculator.CalculateStockingRate(20.0m, 10.0m);
        rate.Should().Be(2.00m);
    }

    [Fact]
    public void IsOvergrazed_WhenCurrentExceedsMaxCapacity_ShouldReturnTrue()
    {
        // Max capacity = 15 UA, Current = 18 UA -> Overgrazed
        bool overgrazed = StockingRateCalculator.IsOvergrazed(18.0m, 15.0m);
        overgrazed.Should().BeTrue();
    }

    [Fact]
    public void IsNearCapacity_WhenCurrentIs85PercentOrMore_ShouldReturnTrue()
    {
        // Max capacity = 20 UA, Current = 17 UA (85%) -> Near capacity
        bool nearCapacity = StockingRateCalculator.IsNearCapacity(17.0m, 20.0m);
        nearCapacity.Should().BeTrue();
    }
}
