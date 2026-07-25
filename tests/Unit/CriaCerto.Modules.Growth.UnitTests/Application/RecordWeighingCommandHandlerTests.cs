using CriaCerto.Modules.Growth.Application.Abstractions;
using CriaCerto.Modules.Growth.Application.Contracts;
using CriaCerto.Modules.Growth.Application.Domain;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace CriaCerto.Modules.Growth.UnitTests.Application;

public class RecordWeighingCommandHandlerTests
{
    private static IGrowthDbContext CreateInMemoryDbContext()
    {
        var options = new DbContextOptionsBuilder<MockGrowthDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new MockGrowthDbContext(options);
    }

    private class MockGrowthDbContext : DbContext, IGrowthDbContext
    {
        public MockGrowthDbContext(DbContextOptions<MockGrowthDbContext> options) : base(options) { }

        public DbSet<PasturePaddock> Paddocks => Set<PasturePaddock>();
        public DbSet<Lot> Lots => Set<Lot>();
        public DbSet<LotMovement> LotMovements => Set<LotMovement>();
        public DbSet<Weighing> Weighings => Set<Weighing>();
    }

    [Fact]
    public async Task RecordWeighingCommandHandler_FirstWeighing_ShouldSaveAndReturnSuccess()
    {
        // Arrange
        var dbContext = CreateInMemoryDbContext();
        var handler = new RecordWeighingCommandHandler(dbContext);
        var tenantId = Guid.NewGuid();
        var command = new RecordWeighingCommand(
            tenantId,
            "BR-990",
            null,
            DateTime.UtcNow,
            320.0m,
            50.0m,
            "Primeira pesagem");

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.AnimalTagId.Should().Be("BR-990");
        result.Value.WeightKg.Should().Be(320.0m);
        result.Value.CalculatedArrobasTotal.Should().Be(10.67m);
        result.Value.CalculatedAdgKgPerDay.Should().Be(0.0m);

        var savedWeighing = await dbContext.Weighings.FirstOrDefaultAsync(w => w.Id == result.Value.Id);
        savedWeighing.Should().NotBeNull();
    }

    [Fact]
    public async Task RecordWeighingCommandHandler_SecondWeighing_ShouldCalculateGpdFromPrevious()
    {
        // Arrange
        var dbContext = CreateInMemoryDbContext();
        var tenantId = Guid.NewGuid();
        var animalTag = "BR-990";
        var date1 = DateTime.UtcNow.AddDays(-20);
        var date2 = DateTime.UtcNow;

        var prevWeighing = Weighing.Create(tenantId, animalTag, null, date1, 300.0m, 50.0m).Value;
        dbContext.Weighings.Add(prevWeighing);
        await dbContext.SaveChangesAsync();

        var handler = new RecordWeighingCommandHandler(dbContext);
        var command = new RecordWeighingCommand(
            tenantId,
            animalTag,
            null,
            date2,
            330.0m,
            50.0m,
            "Segunda pesagem");

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.CalculatedAdgKgPerDay.Should().Be(1.5m); // (330 - 300) / 20 = 1.5 kg/day
        result.Value.IsWeightLossWarning.Should().BeFalse();
    }
}
