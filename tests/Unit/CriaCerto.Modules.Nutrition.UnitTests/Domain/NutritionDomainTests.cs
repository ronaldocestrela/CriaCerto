using CriaCerto.BuildingBlocks.Abstractions.Results;
using CriaCerto.Modules.Nutrition.Application.Domain;
using FluentAssertions;
using Xunit;

namespace CriaCerto.Modules.Nutrition.UnitTests.Domain;

public class NutritionDomainTests
{
    [Fact]
    public void CreateSiloStock_WithValidData_ShouldReturnSuccess()
    {
        // Arrange & Act
        var result = SiloStock.Create(
            Guid.NewGuid(),
            "Milho Moído Gado Engorda",
            FeedCategory.BulkGrain,
            initialStockKg: 10000m,
            unitCostPerKg: 1.20m,
            dryMatterPercentage: 88m,
            minimumThresholdKg: 2000m);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Name.Should().Be("Milho Moído Gado Engorda");
        result.Value.CurrentStockKg.Should().Be(10000m);
        result.Value.UnitCostPerKg.Should().Be(1.20m);
        result.Value.DryMatterPercentage.Should().Be(88m);
    }

    [Fact]
    public void RestockSilo_WithValidAmount_ShouldUpdateStockAndCost()
    {
        // Arrange
        var silo = SiloStock.Create(
            Guid.NewGuid(),
            "Farelo de Soja 46%",
            FeedCategory.BulkGrain,
            initialStockKg: 1000m,
            unitCostPerKg: 2.00m,
            dryMatterPercentage: 90m,
            minimumThresholdKg: 500m).Value;

        // Act
        var result = silo.Restock(addedKg: 1000m, newUnitCostPerKg: 2.50m);

        // Assert
        result.IsSuccess.Should().BeTrue();
        silo.CurrentStockKg.Should().Be(2000m);
        // Weighted average cost: (1000 * 2.00 + 1000 * 2.50) / 2000 = 2.25
        silo.UnitCostPerKg.Should().Be(2.25m);
    }

    [Fact]
    public void ConsumeSiloStock_ExceedingAvailable_ShouldReturnValidationFailure()
    {
        // Arrange
        var silo = SiloStock.Create(
            Guid.NewGuid(),
            "Sal Mineral 80",
            FeedCategory.MineralSalt,
            initialStockKg: 100m,
            unitCostPerKg: 3.50m,
            dryMatterPercentage: 98m,
            minimumThresholdKg: 20m).Value;

        // Act
        var result = silo.ConsumeStock(150m);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Type.Should().Be(ErrorType.Validation);
    }

    [Fact]
    public void CreateFeedRation_WithItemsNotEqualing100Percent_ShouldReturnFailure()
    {
        // Arrange
        var items = new List<FeedRationItemInput>
        {
            new(Guid.NewGuid(), "Milho", 50m, 1.20m),
            new(Guid.NewGuid(), "Farelo", 30m, 2.20m) // Sum = 80%, invalid!
        };

        // Act
        var result = FeedRation.Create(
            Guid.NewGuid(),
            "Dieta Adaptativa 1",
            RationType.FeedlotTmr,
            dryMatterPercentage: 75m,
            items);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Type.Should().Be(ErrorType.Validation);
    }

    [Fact]
    public void CreateFeedRation_WithValidItems_ShouldCalculateWeightedCostPerKg()
    {
        // Arrange
        var items = new List<FeedRationItemInput>
        {
            new(Guid.NewGuid(), "Milho Moído", 70m, 1.00m), // 70% * $1.00 = 0.70
            new(Guid.NewGuid(), "Farelo Soja", 30m, 2.00m)  // 30% * $2.00 = 0.60
        };

        // Act
        var result = FeedRation.Create(
            Guid.NewGuid(),
            "Dieta Engorda TMR 1",
            RationType.FeedlotTmr,
            dryMatterPercentage: 85m,
            items);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.CalculatedCostPerKg.Should().Be(1.30m);
    }

    [Fact]
    public void RecordSupplementation_ShouldCalculateIntakeGramsPerHead()
    {
        // Arrange & Act
        var result = PastureSupplementation.Create(
            Guid.NewGuid(),
            Guid.NewGuid(), // PaddockId
            Guid.NewGuid(), // LotId
            Guid.NewGuid(), // RationId
            DateTime.UtcNow,
            quantityKg: 30m, // 30 kg = 30,000 grams
            headCount: 100); // 100 cabeças -> 300g/cab/dia

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.CalculatedIntakeGramsPerHead.Should().Be(300m);
    }
}
