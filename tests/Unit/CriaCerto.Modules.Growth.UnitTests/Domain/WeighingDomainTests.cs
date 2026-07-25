using CriaCerto.Modules.Growth.Application.Domain;
using FluentAssertions;
using Xunit;

namespace CriaCerto.Modules.Growth.UnitTests.Domain;

public class WeighingDomainTests
{
    [Fact]
    public void Create_WithValidParameters_ShouldReturnSuccess()
    {
        Guid tenantId = Guid.NewGuid();
        string animalTagId = "BR-102030";
        var date = DateTime.UtcNow;

        var result = Weighing.Create(
            tenantId,
            animalTagId,
            null,
            date,
            350.0m,
            50.0m,
            "Pesagem inicial de recria");

        result.IsSuccess.Should().BeTrue();
        result.Value.AnimalTagId.Should().Be("BR-102030");
        result.Value.WeightKg.Should().Be(350.0m);
        result.Value.CarcassYieldPercentage.Should().Be(50.0m);
        result.Value.CalculatedArrobasTotal.Should().Be(11.67m); // 350 * 0.50 / 15 = 11.6666... -> 11.67
        result.Value.CalculatedAdgKgPerDay.Should().Be(0.0m);
        result.Value.IsWeightLossWarning.Should().BeFalse();
    }

    [Fact]
    public void Create_WithZeroOrNegativeWeight_ShouldReturnFailure()
    {
        Guid tenantId = Guid.NewGuid();

        var result = Weighing.Create(
            tenantId,
            "BR-101",
            null,
            DateTime.UtcNow,
            0.0m,
            50.0m);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Weighing.InvalidWeight");
    }

    [Fact]
    public void Create_WithInvalidCarcassYield_ShouldReturnFailure()
    {
        Guid tenantId = Guid.NewGuid();

        var result = Weighing.Create(
            tenantId,
            "BR-101",
            null,
            DateTime.UtcNow,
            300.0m,
            75.0m); // Yield > 65%

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Weighing.InvalidCarcassYield");
    }

    [Fact]
    public void ApplyPreviousWeighing_WithWeightGain_ShouldCalculatePositiveGpdAndNoWarning()
    {
        Guid tenantId = Guid.NewGuid();
        var prevDate = DateTime.UtcNow.AddDays(-30);
        var currDate = DateTime.UtcNow;

        var prevResult = Weighing.Create(tenantId, "BR-101", null, prevDate, 300.0m, 50.0m);
        var currResult = Weighing.Create(tenantId, "BR-101", null, currDate, 330.0m, 50.0m);

        currResult.Value.ApplyPreviousWeighing(prevResult.Value);

        currResult.Value.CalculatedAdgKgPerDay.Should().Be(1.0m); // (330 - 300) / 30 = 1.0
        currResult.Value.CalculatedMonthlyArrobaGain.Should().Be(1.0m); // 1.0 * 30 * 0.5 / 15 = 1.0
        currResult.Value.IsWeightLossWarning.Should().BeFalse();
    }

    [Fact]
    public void ApplyPreviousWeighing_WithWeightLoss_ShouldCalculateNegativeGpdAndSetWarning()
    {
        Guid tenantId = Guid.NewGuid();
        var prevDate = DateTime.UtcNow.AddDays(-20);
        var currDate = DateTime.UtcNow;

        var prevResult = Weighing.Create(tenantId, "BR-101", null, prevDate, 300.0m, 50.0m);
        var currResult = Weighing.Create(tenantId, "BR-101", null, currDate, 280.0m, 50.0m);

        currResult.Value.ApplyPreviousWeighing(prevResult.Value);

        currResult.Value.CalculatedAdgKgPerDay.Should().Be(-1.0m); // (280 - 300) / 20 = -1.0
        currResult.Value.IsWeightLossWarning.Should().BeTrue();
    }
}
