using CriaCerto.BuildingBlocks.Abstractions.Results;
using CriaCerto.Modules.Nutrition.Application.Contracts;
using CriaCerto.Modules.Nutrition.Application.Domain;
using CriaCerto.Modules.Nutrition.Application.Features.FeedingFeatures;
using CriaCerto.Modules.Nutrition.Application.Features.SiloStockFeatures;
using CriaCerto.Modules.Nutrition.Infrastructure.Persistence;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace CriaCerto.Modules.Nutrition.UnitTests.Domain;

public class SiloStockDeductionTests
{
    private static NutritionDbContext CreateInMemoryDbContext()
    {
        var options = new DbContextOptionsBuilder<NutritionDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        return new NutritionDbContext(options);
    }

    [Fact]
    public async Task RecordFeedlotTmrCommandHandler_WithValidStock_ShouldDeductSiloStockSuccessfully()
    {
        // Arrange
        using var db = CreateInMemoryDbContext();
        var tenantId = Guid.NewGuid();
        var lotId = Guid.NewGuid();

        var siloMilho = SiloStock.Create(tenantId, "Milho Moído", FeedCategory.BulkGrain, 1000m, 1.20m, 88m, 200m).Value;
        var siloFarelo = SiloStock.Create(tenantId, "Farelo de Soja", FeedCategory.BulkGrain, 500m, 2.00m, 90m, 100m).Value;
        db.SiloStocks.AddRange(siloMilho, siloFarelo);

        var rationItems = new List<FeedRationItemInput>
        {
            new(siloMilho.Id, siloMilho.Name, 70m, siloMilho.UnitCostPerKg),
            new(siloFarelo.Id, siloFarelo.Name, 30m, siloFarelo.UnitCostPerKg)
        };
        var ration = FeedRation.Create(tenantId, "Ração Engorda 70/30", RationType.FeedlotTmr, 88m, rationItems).Value;
        db.FeedRations.Add(ration);
        await db.SaveChangesAsync();

        var handler = new RecordFeedlotTmrCommandHandler(db);
        var command = new RecordFeedlotTmrCommand(
            tenantId,
            lotId,
            ration.Id,
            DateTime.UtcNow,
            OfferedAsFedKg: 100m, // 70kg milho, 30kg farelo
            TroughScore.Score0_Clean,
            HeadCountAtFeeding: 50);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        
        var updatedMilho = await db.SiloStocks.FirstAsync(s => s.Id == siloMilho.Id);
        var updatedFarelo = await db.SiloStocks.FirstAsync(s => s.Id == siloFarelo.Id);

        updatedMilho.CurrentStockKg.Should().Be(930m); // 1000 - 70
        updatedFarelo.CurrentStockKg.Should().Be(470m); // 500 - 30
    }

    [Fact]
    public async Task RecordFeedlotTmrCommandHandler_WithInsufficientStock_ShouldReturnFailureAndNotDeduct()
    {
        // Arrange
        using var db = CreateInMemoryDbContext();
        var tenantId = Guid.NewGuid();
        var lotId = Guid.NewGuid();

        var siloMilho = SiloStock.Create(tenantId, "Milho Moído", FeedCategory.BulkGrain, 50m, 1.20m, 88m, 200m).Value; // Apenas 50kg!
        db.SiloStocks.Add(siloMilho);

        var rationItems = new List<FeedRationItemInput>
        {
            new(siloMilho.Id, siloMilho.Name, 100m, siloMilho.UnitCostPerKg)
        };
        var ration = FeedRation.Create(tenantId, "Ração 100% Milho", RationType.FeedlotTmr, 88m, rationItems).Value;
        db.FeedRations.Add(ration);
        await db.SaveChangesAsync();

        var handler = new RecordFeedlotTmrCommandHandler(db);
        var command = new RecordFeedlotTmrCommand(
            tenantId,
            lotId,
            ration.Id,
            DateTime.UtcNow,
            OfferedAsFedKg: 100m, // Precisa de 100kg, mas só tem 50kg!
            TroughScore.Score0_Clean,
            HeadCountAtFeeding: 50);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Type.Should().Be(ErrorType.Validation);
        result.Error.Code.Should().Be("SiloStock.InsufficientStock");

        var updatedMilho = await db.SiloStocks.FirstAsync(s => s.Id == siloMilho.Id);
        updatedMilho.CurrentStockKg.Should().Be(50m); // Saldo intocado
    }

    [Fact]
    public async Task RecordSupplementationCommandHandler_WithValidStock_ShouldDeductSiloStockSuccessfully()
    {
        // Arrange
        using var db = CreateInMemoryDbContext();
        var tenantId = Guid.NewGuid();
        var paddockId = Guid.NewGuid();
        var lotId = Guid.NewGuid();

        var siloSal = SiloStock.Create(tenantId, "Sal Mineral 80", FeedCategory.MineralSalt, 300m, 3.50m, 98m, 50m).Value;
        db.SiloStocks.Add(siloSal);

        var rationItems = new List<FeedRationItemInput>
        {
            new(siloSal.Id, siloSal.Name, 100m, siloSal.UnitCostPerKg)
        };
        var ration = FeedRation.Create(tenantId, "Suplemento Mineral Campo", RationType.PastureSupplement, 98m, rationItems).Value;
        db.FeedRations.Add(ration);
        await db.SaveChangesAsync();

        var handler = new RecordSupplementationCommandHandler(db);
        var command = new RecordSupplementationCommand(
            tenantId,
            paddockId,
            lotId,
            ration.Id,
            DateTime.UtcNow,
            QuantityKg: 50m, // 50kg
            HeadCount: 100);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();

        var updatedSal = await db.SiloStocks.FirstAsync(s => s.Id == siloSal.Id);
        updatedSal.CurrentStockKg.Should().Be(250m); // 300 - 50
    }
}
