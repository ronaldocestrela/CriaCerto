using CriaCerto.BuildingBlocks.Abstractions.Results;
using CriaCerto.Modules.Tenancy.Application.Domain;
using CriaCerto.Modules.Tenancy.Application.Features.UpdateTenantProfile;
using CriaCerto.Modules.Tenancy.Infrastructure.Persistence;
using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace CriaCerto.Modules.Tenancy.UnitTests;

public class UpdateTenantProfileCommandHandlerTests : IDisposable
{
    private readonly SqliteConnection _sqliteConnection;
    private readonly TenancyDbContext _dbContext;

    public UpdateTenantProfileCommandHandlerTests()
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
    public async Task Handle_Should_Update_Tenant_Profile_Successfully()
    {
        // Arrange
        var tenant = new Tenant
        {
            Id = Guid.NewGuid(),
            Name = "Fazenda Antiga",
            CNPJ = "11.111.111/0001-11",
            Status = "Active",
            SubscribedPlan = "Pro",
            Capacity = 2000,
            State = "GO",
            City = "Jataí",
            AreaInHectares = 1500
        };
        _dbContext.Tenants.Add(tenant);
        await _dbContext.SaveChangesAsync();

        var handler = new UpdateTenantProfileCommandHandler(_dbContext);
        var command = new UpdateTenantProfileCommand(
            tenant.Id,
            "Fazenda Nova Era",
            "22.222.222/0001-22",
            "MT",
            "Sorriso",
            "IE-98765",
            3500.50m,
            4500,
            "Recria e Engorda"
        );

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Name.Should().Be("Fazenda Nova Era");
        result.Value.CNPJ.Should().Be("22.222.222/0001-22");
        result.Value.State.Should().Be("MT");
        result.Value.City.Should().Be("Sorriso");
        result.Value.AreaInHectares.Should().Be(3500.50m);
        result.Value.Capacity.Should().Be(4500);

        var tenantInDb = await _dbContext.Tenants.FindAsync(tenant.Id);
        tenantInDb.Should().NotBeNull();
        tenantInDb!.Name.Should().Be("Fazenda Nova Era");
    }

    [Fact]
    public async Task Handle_Should_Fail_When_Tenant_Does_Not_Exist()
    {
        // Arrange
        var handler = new UpdateTenantProfileCommandHandler(_dbContext);
        var command = new UpdateTenantProfileCommand(
            Guid.NewGuid(),
            "Fazenda Inexistente",
            "11.111.111/0001-11",
            "MT",
            "Cuiabá",
            "IE-123",
            100,
            500,
            "Matriz"
        );

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Type.Should().Be(ErrorType.NotFound);
        result.Error.Code.Should().Be("Tenant.NotFound");
    }
}
