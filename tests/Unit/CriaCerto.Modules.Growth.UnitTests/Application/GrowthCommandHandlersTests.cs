using CriaCerto.Modules.Growth.Application.Abstractions;
using CriaCerto.Modules.Growth.Application.Contracts;
using CriaCerto.Modules.Growth.Application.Domain;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using Xunit;

namespace CriaCerto.Modules.Growth.UnitTests.Application;

public class GrowthCommandHandlersTests
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
    public async Task CreatePaddockCommandHandler_WithValidData_ShouldCreateAndSave()
    {
        // Arrange
        var dbContext = CreateInMemoryDbContext();
        var handler = new CreatePaddockCommandHandler(dbContext);
        var tenantId = Guid.NewGuid();
        var command = new CreatePaddockCommand("Pasto Varjão", "VAR-01", 25.0m, 30.0m, tenantId);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Name.Should().Be("Pasto Varjão");
        result.Value.Code.Should().Be("VAR-01");

        var savedPaddock = await dbContext.Paddocks.FirstOrDefaultAsync(p => p.Id == result.Value.Id);
        savedPaddock.Should().NotBeNull();
    }

    [Fact]
    public async Task CreateLotCommandHandler_WithInitialPaddock_ShouldCreateLotAndMovement()
    {
        // Arrange
        var dbContext = CreateInMemoryDbContext();
        var tenantId = Guid.NewGuid();
        var paddock = PasturePaddock.Create("Pasto Central", "PAD-01", 10.0m, 15.0m, tenantId).Value;
        dbContext.Paddocks.Add(paddock);
        await dbContext.SaveChangesAsync();

        var handler = new CreateLotCommandHandler(dbContext);
        var command = new CreateLotCommand("Lote Garrotes", "L-GAR-01", LotCategory.Garrotes, 15, 320.0m, tenantId, paddock.Id);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.CurrentPaddockId.Should().Be(paddock.Id);
        result.Value.PaddockName.Should().Be("Pasto Central");

        var movements = await dbContext.LotMovements.ToListAsync();
        movements.Should().HaveCount(1);
        movements[0].DestinationPaddockId.Should().Be(paddock.Id);
    }

    [Fact]
    public async Task MoveLotToPaddockCommandHandler_ShouldUpdateLotLocationAndRecordMovement()
    {
        // Arrange
        var dbContext = CreateInMemoryDbContext();
        var tenantId = Guid.NewGuid();

        var paddockA = PasturePaddock.Create("Pasto A", "PAD-A", 10.0m, 15.0m, tenantId).Value;
        var paddockB = PasturePaddock.Create("Pasto B", "PAD-B", 12.0m, 18.0m, tenantId).Value;
        var lot = Lot.Create("Lote Novilhas", "L-NOV-01", LotCategory.Recria, 10, 280.0m, tenantId, paddockA.Id).Value;

        dbContext.Paddocks.AddRange(paddockA, paddockB);
        dbContext.Lots.Add(lot);
        await dbContext.SaveChangesAsync();

        var handler = new MoveLotToPaddockCommandHandler(dbContext);
        var command = new MoveLotToPaddockCommand(lot.Id, paddockB.Id, "Rodízio de pasto", tenantId);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.SourcePaddockId.Should().Be(paddockA.Id);
        result.Value.DestinationPaddockId.Should().Be(paddockB.Id);

        var updatedLot = await dbContext.Lots.FindAsync(lot.Id);
        updatedLot!.CurrentPaddockId.Should().Be(paddockB.Id);
    }
}
