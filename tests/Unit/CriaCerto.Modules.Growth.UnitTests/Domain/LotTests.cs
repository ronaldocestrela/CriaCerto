using CriaCerto.Modules.Growth.Application.Domain;
using FluentAssertions;
using Xunit;

namespace CriaCerto.Modules.Growth.UnitTests.Domain;

public class LotTests
{
    [Fact]
    public void Create_WithValidParameters_ShouldCalculateTotalUAAndReturnSuccess()
    {
        // Arrange
        var tenantId = Guid.NewGuid();

        // 30 cabeças com 450kg cada = 13.500kg total => 13.500 / 450 = 30 UA
        var result = Lot.Create("Lote Engorda Machos", "L-ENG-01", LotCategory.Engorda, 30, 450.0m, tenantId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.TotalWeightKg.Should().Be(13500.0m);
        result.Value.TotalUA.Should().Be(30.0m);
        result.Value.Status.Should().Be(LotStatus.Active);
    }

    [Fact]
    public void AssignToPaddock_WhenActive_ShouldUpdatePaddockId()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var lot = Lot.Create("Lote Recria", "L-REC-01", LotCategory.Recria, 20, 300.0m, tenantId).Value;
        var paddockId = Guid.NewGuid();

        // Act
        var result = lot.AssignToPaddock(paddockId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        lot.CurrentPaddockId.Should().Be(paddockId);
    }

    [Fact]
    public void CloseLot_ShouldClearPaddockAndSetStatusClosed()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var paddockId = Guid.NewGuid();
        var lot = Lot.Create("Lote Recria", "L-REC-01", LotCategory.Recria, 20, 300.0m, tenantId, paddockId).Value;

        // Act
        var result = lot.CloseLot();

        // Assert
        result.IsSuccess.Should().BeTrue();
        lot.Status.Should().Be(LotStatus.Closed);
        lot.CurrentPaddockId.Should().BeNull();
    }
}
