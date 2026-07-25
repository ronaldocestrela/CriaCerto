using CriaCerto.BuildingBlocks.Abstractions.Results;
using CriaCerto.Modules.Calving.Application.Domain;
using CriaCerto.Modules.Calving.Application.Domain.Services;
using FluentAssertions;
using Xunit;

namespace CriaCerto.Modules.Calving.UnitTests.Domain;

public class CalfTests
{
    private readonly Guid _tenantId = Guid.NewGuid();
    private readonly Guid _motherCowId = Guid.NewGuid();

    [Fact]
    public void Create_WithValidData_ShouldReturnSuccess()
    {
        var result = Calf.Create("BZ-501", _motherCowId, DateTime.UtcNow.AddDays(-60), "M", "Nelore", 32.5m, _tenantId);

        result.IsSuccess.Should().BeTrue();
        result.Value.TagId.Should().Be("BZ-501");
        result.Value.Status.Should().Be(CalfStatus.Unweaned);
    }

    [Fact]
    public void MarkWeaned_WhenUnweaned_ShouldTransitionToWeaned()
    {
        var calf = Calf.Create("BZ-502", _motherCowId, DateTime.UtcNow.AddDays(-205), "F", "Angus", 30.0m, _tenantId).Value;

        var result = calf.MarkWeaned();

        result.IsSuccess.Should().BeTrue();
        calf.Status.Should().Be(CalfStatus.Weaned);
    }
}

public class P205CalculatorTests
{
    [Fact]
    public void CalculateP205_WithPrimeMother_ShouldApplyCorrectFactor()
    {
        var birthDate = new DateTime(2025, 1, 1);
        var weaningDate = new DateTime(2025, 7, 25); // 205 dias exatos
        decimal birthWeight = 30m;
        decimal weaningWeight = 210m;
        int motherAgeYears = 5; // Matriz adulta (fator 1.00)

        var p205 = P205Calculator.CalculateP205(birthWeight, weaningWeight, birthDate, weaningDate, motherAgeYears);

        p205.Should().Be(210m);
    }

    [Fact]
    public void CalculateP205_WithYoungMother_ShouldApplyYoungMotherBonusFactor()
    {
        var birthDate = new DateTime(2025, 1, 1);
        var weaningDate = new DateTime(2025, 7, 25);
        decimal birthWeight = 30m;
        decimal weaningWeight = 210m;
        int motherAgeYears = 2; // Primípara jovem (fator 1.15)

        var p205 = P205Calculator.CalculateP205(birthWeight, weaningWeight, birthDate, weaningDate, motherAgeYears);

        // 210 * 1.15 = 241.5
        p205.Should().Be(241.5m);
    }
}
