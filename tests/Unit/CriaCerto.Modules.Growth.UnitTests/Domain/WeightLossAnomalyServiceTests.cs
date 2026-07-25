using CriaCerto.Modules.Growth.Application.Domain;
using CriaCerto.Modules.Growth.Application.Domain.Services;
using FluentAssertions;
using Xunit;

namespace CriaCerto.Modules.Growth.UnitTests.Domain;

public class WeightLossAnomalyServiceTests
{
    [Fact]
    public void IsConsecutiveWeightLoss_WithTwoConsecutiveNegativeGpds_ShouldReturnTrue()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var animalTag = "BR-990";
        var d1 = DateTime.UtcNow.AddDays(-60);
        var d2 = DateTime.UtcNow.AddDays(-30);
        var d3 = DateTime.UtcNow;

        var w1 = Weighing.Create(tenantId, animalTag, null, d1, 400.0m, 50.0m).Value;
        var w2 = Weighing.Create(tenantId, animalTag, null, d2, 390.0m, 50.0m).Value; // GPD -0.33
        w2.ApplyPreviousWeighing(w1);

        var w3 = Weighing.Create(tenantId, animalTag, null, d3, 380.0m, 50.0m).Value; // GPD -0.33
        w3.ApplyPreviousWeighing(w2);

        var history = new List<Weighing> { w1, w2, w3 };

        // Act
        var isAnomaly = WeightLossAnomalyService.IsConsecutiveWeightLoss(history);

        // Assert
        isAnomaly.Should().BeTrue();
    }

    [Fact]
    public void IsConsecutiveWeightLoss_WithSingleLossThenGain_ShouldReturnFalse()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var animalTag = "BR-990";
        var d1 = DateTime.UtcNow.AddDays(-60);
        var d2 = DateTime.UtcNow.AddDays(-30);
        var d3 = DateTime.UtcNow;

        var w1 = Weighing.Create(tenantId, animalTag, null, d1, 400.0m, 50.0m).Value;
        var w2 = Weighing.Create(tenantId, animalTag, null, d2, 390.0m, 50.0m).Value; // Loss
        w2.ApplyPreviousWeighing(w1);

        var w3 = Weighing.Create(tenantId, animalTag, null, d3, 410.0m, 50.0m).Value; // Gain
        w3.ApplyPreviousWeighing(w2);

        var history = new List<Weighing> { w1, w2, w3 };

        // Act
        var isAnomaly = WeightLossAnomalyService.IsConsecutiveWeightLoss(history);

        // Assert
        isAnomaly.Should().BeFalse();
    }
}
