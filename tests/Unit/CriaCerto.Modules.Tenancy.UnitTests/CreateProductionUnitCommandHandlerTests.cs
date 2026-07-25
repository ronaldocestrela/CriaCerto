using CriaCerto.BuildingBlocks.Abstractions.Results;
using CriaCerto.Modules.Tenancy.Application.Domain;
using CriaCerto.Modules.Tenancy.Application.Features.CreateProductionUnit;
using CriaCerto.Modules.Tenancy.Infrastructure.Persistence;
using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace CriaCerto.Modules.Tenancy.UnitTests;

public class CreateProductionUnitCommandHandlerTests : IDisposable
{
    private readonly SqliteConnection _sqliteConnection;
    private readonly TenancyDbContext _dbContext;

    public CreateProductionUnitCommandHandlerTests()
    {
        _sqliteConnection = new SqliteConnection("Filename=:memory:");
        _sqliteConnection.Open();

        var options = new DbContextOptionsBuilder<TenancyDbContext>()
            .UseSqlite(_sqliteConnection)
            .Options;

        _dbContext = new TenancyDbContext(options);
        _dbContext.Database.EnsureCreated();
    }

    public void Dispose()
    {
        _dbContext.Dispose();
        _sqliteConnection.Close();
        _sqliteConnection.Dispose();
    }

    [Fact]
    public async Task Handle_Should_Create_Production_Unit_With_Generated_Code()
    {
        // Arrange
        var tenant = new Tenant
        {
            Id = Guid.NewGuid(),
            Name = "Fazenda Vista Alegre",
            CNPJ = "12.345.678/0001-99"
        };
        _dbContext.Tenants.Add(tenant);
        await _dbContext.SaveChangesAsync();

        var handler = new CreateProductionUnitCommandHandler(_dbContext);
        var command = new CreateProductionUnitCommand(
            tenant.Id,
            "Retiro Pantanal",
            "Retiro",
            1500,
            "Setor Norte - Piquetes 1 a 5"
        );

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Code.Should().Be("UN-001-SFE");
        result.Value.Name.Should().Be("Retiro Pantanal");
        result.Value.Type.Should().Be("Retiro");
        result.Value.Capacity.Should().Be(1500);

        var unitInDb = await _dbContext.ProductionUnits.FirstOrDefaultAsync(u => u.TenantId == tenant.Id);
        unitInDb.Should().NotBeNull();
        unitInDb!.Code.Should().Be("UN-001-SFE");
    }

    [Fact]
    public async Task Handle_Should_Fail_When_Tenant_Does_Not_Exist()
    {
        // Arrange
        var handler = new CreateProductionUnitCommandHandler(_dbContext);
        var command = new CreateProductionUnitCommand(
            Guid.NewGuid(),
            "Unidade Fantasma",
            "Creche",
            500,
            null
        );

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Type.Should().Be(ErrorType.NotFound);
        result.Error.Code.Should().Be("Tenant.NotFound");
    }
}
