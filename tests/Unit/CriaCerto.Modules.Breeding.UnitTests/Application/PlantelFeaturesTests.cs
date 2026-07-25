using CriaCerto.BuildingBlocks.Abstractions.Results;
using CriaCerto.Modules.Breeding.Application.Abstractions;
using CriaCerto.Modules.Breeding.Application.Contracts;
using CriaCerto.Modules.Breeding.Application.Domain;
using CriaCerto.Modules.Breeding.Application.Features.Plantel;
using CriaCerto.Modules.Breeding.Infrastructure.Persistence;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace CriaCerto.Modules.Breeding.UnitTests.Application;

public class PlantelFeaturesTests
{
    private readonly Guid _tenantId = Guid.NewGuid();

    private static BreedingDbContext CreateInMemoryDbContext()
    {
        var options = new DbContextOptionsBuilder<BreedingDbContext>()
            .UseSqlite("DataSource=:memory:")
            .Options;

        var context = new BreedingDbContext(options);
        context.Database.OpenConnection();
        context.Database.EnsureCreated();
        return context;
    }

    [Fact]
    public void CreateCowCommandValidator_WithValidCommand_ShouldNotHaveValidationErrors()
    {
        var validator = new CreateCowCommandValidator();
        var command = new CreateCowCommand("BR-990", "Nelore", DateTime.UtcNow.AddYears(-2), _tenantId, BodyConditionScore: 3.5m);

        var result = validator.Validate(command);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void CreateCowCommandValidator_WithInvalidBcs_ShouldHaveValidationError()
    {
        var validator = new CreateCowCommandValidator();
        var command = new CreateCowCommand("BR-990", "Nelore", DateTime.UtcNow.AddYears(-2), _tenantId, BodyConditionScore: 7.0m);

        var result = validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => e.PropertyName == "BodyConditionScore");
    }

    [Fact]
    public void UpdateCowCommandValidator_WithEmptyId_ShouldHaveValidationError()
    {
        var validator = new UpdateCowCommandValidator();
        var command = new UpdateCowCommand(Guid.Empty, "BR-990", "Nelore", DateTime.UtcNow.AddYears(-2));

        var result = validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => e.PropertyName == "Id");
    }

    [Fact]
    public async Task CreateCowCommandHandler_WhenEarTagAlreadyExistsInSameTenant_ShouldReturnConflict()
    {
        using var dbContext = CreateInMemoryDbContext();
        var handler = new CreateCowCommandHandler(dbContext);

        var command1 = new CreateCowCommand("BR-200", "Nelore", DateTime.UtcNow.AddYears(-2), _tenantId);
        await handler.Handle(command1, CancellationToken.None);

        var command2 = new CreateCowCommand("BR-200", "Angus", DateTime.UtcNow.AddYears(-3), _tenantId);
        var result = await handler.Handle(command2, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Cow.EarTagAlreadyExists");
    }

    [Fact]
    public async Task CreateCowCommandHandler_WhenValid_ShouldSaveCow()
    {
        using var dbContext = CreateInMemoryDbContext();
        var handler = new CreateCowCommandHandler(dbContext);

        var command = new CreateCowCommand("BR-201", "Nelore PO", DateTime.UtcNow.AddYears(-2), _tenantId, Nickname: "Famosa", BodyConditionScore: 4.0m);
        var result = await handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.EarTag.Should().Be("BR-201");
        result.Value.Nickname.Should().Be("Famosa");
        result.Value.BodyConditionScore.Should().Be(4.0m);
    }
}
