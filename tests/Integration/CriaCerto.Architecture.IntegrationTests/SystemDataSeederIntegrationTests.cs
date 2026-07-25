using CriaCerto.Api.Seeders;
using CriaCerto.BuildingBlocks.Abstractions.ReferenceData;
using CriaCerto.BuildingBlocks.Application.Features.GetReferenceBreeds;
using CriaCerto.BuildingBlocks.Infrastructure.Persistence;
using CriaCerto.Modules.Sanitary.Application.Domain;
using CriaCerto.Modules.Sanitary.Application.Features.GetVaccineCalendar;
using CriaCerto.Modules.Sanitary.Infrastructure.Persistence;
using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace CriaCerto.Architecture.IntegrationTests;

public class SystemDataSeederIntegrationTests : IDisposable
{
    private readonly SqliteConnection _foundationConnection;
    private readonly SqliteConnection _sanitaryConnection;
    private readonly FoundationDbContext _foundationDb;
    private readonly SanitaryDbContext _sanitaryDb;

    public SystemDataSeederIntegrationTests()
    {
        _foundationConnection = new SqliteConnection("Filename=:memory:");
        _foundationConnection.Open();

        _sanitaryConnection = new SqliteConnection("Filename=:memory:");
        _sanitaryConnection.Open();

        var foundationOptions = new DbContextOptionsBuilder<FoundationDbContext>()
            .UseSqlite(_foundationConnection)
            .Options;

        var sanitaryOptions = new DbContextOptionsBuilder<SanitaryDbContext>()
            .UseSqlite(_sanitaryConnection)
            .Options;

        _foundationDb = new FoundationDbContext(foundationOptions);
        _sanitaryDb = new SanitaryDbContext(sanitaryOptions);

        _foundationDb.Database.EnsureCreated();
        _sanitaryDb.Database.EnsureCreated();
    }

    public void Dispose()
    {
        _foundationDb.Dispose();
        _sanitaryDb.Dispose();
        _foundationConnection.Close();
        _foundationConnection.Dispose();
        _sanitaryConnection.Close();
        _sanitaryConnection.Dispose();
    }

    [Fact]
    public async Task SeedAsync_WhenDatabaseIsEmpty_ShouldPopulateBreedsAndVaccines()
    {
        // Act
        await SystemDataSeeder.SeedDataAsync(_foundationDb, _sanitaryDb, CancellationToken.None);

        // Assert
        var breeds = await _foundationDb.BovineBreeds.ToListAsync();
        breeds.Should().NotBeEmpty();
        breeds.Should().Contain(b => b.Name == "Nelore" && b.Code == "NEL");
        breeds.Should().Contain(b => b.Name == "Angus" && b.Code == "ANG");
        breeds.Should().Contain(b => b.Name == "Brahman");
        breeds.Should().Contain(b => b.Name == "Senepol");
        breeds.Should().Contain(b => b.Name == "Gir");
        breeds.Should().Contain(b => b.Name == "Girolando");

        var vaccines = await _sanitaryDb.VaccineReferences.ToListAsync();
        vaccines.Should().NotBeEmpty();
        vaccines.Should().Contain(v => v.DiseaseName == "Febre Aftosa" && v.IsMandatoryMAPA);
        vaccines.Should().Contain(v => v.DiseaseName.Contains("Brucelose"));
        vaccines.Should().Contain(v => v.DiseaseName == "Clostridiose");
    }

    [Fact]
    public async Task SeedAsync_WhenExecutedMultipleTimes_ShouldBeIdempotentWithoutDuplicates()
    {
        // First seed
        await SystemDataSeeder.SeedDataAsync(_foundationDb, _sanitaryDb, CancellationToken.None);
        var breedsInitialCount = await _foundationDb.BovineBreeds.CountAsync();
        var vaccinesInitialCount = await _sanitaryDb.VaccineReferences.CountAsync();

        // Second seed
        await SystemDataSeeder.SeedDataAsync(_foundationDb, _sanitaryDb, CancellationToken.None);

        // Third seed
        await SystemDataSeeder.SeedDataAsync(_foundationDb, _sanitaryDb, CancellationToken.None);

        // Assert
        var breedsFinalCount = await _foundationDb.BovineBreeds.CountAsync();
        var vaccinesFinalCount = await _sanitaryDb.VaccineReferences.CountAsync();

        breedsFinalCount.Should().Be(breedsInitialCount);
        vaccinesFinalCount.Should().Be(vaccinesInitialCount);
    }

    [Fact]
    public async Task GetReferenceBreedsQuery_ShouldReturnPopulatedBreedsWithResultSuccess()
    {
        await SystemDataSeeder.SeedDataAsync(_foundationDb, _sanitaryDb, CancellationToken.None);

        var handler = new GetReferenceBreedsQueryHandler(_foundationDb);
        var result = await handler.Handle(new GetReferenceBreedsQuery(), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeEmpty();
        result.Value.Any(b => b.Name == "Nelore").Should().BeTrue();
    }

    [Fact]
    public async Task GetVaccineCalendarQuery_ShouldReturnPopulatedCalendarWithResultSuccess()
    {
        await SystemDataSeeder.SeedDataAsync(_foundationDb, _sanitaryDb, CancellationToken.None);

        var handler = new GetVaccineCalendarQueryHandler(_sanitaryDb);
        var result = await handler.Handle(new GetVaccineCalendarQuery(), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeEmpty();
        result.Value.Any(v => v.DiseaseName == "Febre Aftosa").Should().BeTrue();
    }
}
