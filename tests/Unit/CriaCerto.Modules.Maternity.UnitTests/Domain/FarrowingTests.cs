using CriaCerto.BuildingBlocks.Abstractions.Results;
using CriaCerto.Modules.Maternity.Application.Domain;
using CriaCerto.Modules.Maternity.Application.Domain.Events;
using FluentAssertions;

namespace CriaCerto.Modules.Maternity.UnitTests.Domain;

public class FarrowingTests
{
    private static readonly Guid ValidSowId = Guid.NewGuid();
    private static readonly Guid ValidTenantId = Guid.NewGuid();
    private static readonly DateTime ValidFarrowingDate = DateTime.UtcNow;

    [Fact]
    public void Create_WithValidParameters_ShouldReturnSuccessAndEmitDomainEvent()
    {
        // Arrange
        int liveBorn = 12;
        int stillborn = 1;
        int mummified = 0;
        decimal litterWeight = 16.8m; // 16.8 / 12 = 1.4 kg per piglet (valid)
        string? maternityRoomId = "Sala-01";
        bool assisted = true;
        string notes = "Parto tranquilo sem intercorrências.";

        // Act
        var result = Farrowing.Create(
            ValidSowId,
            ValidTenantId,
            ValidFarrowingDate,
            liveBorn,
            stillborn,
            mummified,
            litterWeight,
            maternityRoomId,
            assisted,
            notes);

        // Assert
        result.IsSuccess.Should().BeTrue();
        var farrowing = result.Value;
        farrowing.Id.Should().NotBeEmpty();
        farrowing.SowId.Should().Be(ValidSowId);
        farrowing.TenantId.Should().Be(ValidTenantId);
        farrowing.FarrowingDate.Should().Be(ValidFarrowingDate);
        farrowing.LiveBorn.Should().Be(12);
        farrowing.Stillborn.Should().Be(1);
        farrowing.Mummified.Should().Be(0);
        farrowing.TotalBorn.Should().Be(13);
        farrowing.LitterWeightKg.Should().Be(16.8m);
        farrowing.AveragePigletWeightKg.Should().Be(1.4m);
        farrowing.MaternityRoomId.Should().Be("Sala-01");
        farrowing.Assisted.Should().BeTrue();
        farrowing.Notes.Should().Be(notes);

        farrowing.DomainEvents.Should().ContainSingle(e => e is FarrowingCompletedEvent);
        var domainEvent = farrowing.DomainEvents.OfType<FarrowingCompletedEvent>().Single();
        domainEvent.FarrowingId.Should().Be(farrowing.Id);
        domainEvent.SowId.Should().Be(ValidSowId);
        domainEvent.TenantId.Should().Be(ValidTenantId);
        domainEvent.LiveBorn.Should().Be(12);
        domainEvent.Stillborn.Should().Be(1);
        domainEvent.Mummified.Should().Be(0);
        domainEvent.FarrowingDate.Should().Be(ValidFarrowingDate);
    }

    [Fact]
    public void Create_WithZeroTotalBorn_ShouldReturnFailure()
    {
        // Act
        var result = Farrowing.Create(
            ValidSowId,
            ValidTenantId,
            ValidFarrowingDate,
            liveBorn: 0,
            stillborn: 0,
            mummified: 0,
            litterWeightKg: 0m);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Farrowing.ZeroTotalBorn");
    }

    [Fact]
    public void Create_WithNegativeLiveBorn_ShouldReturnFailure()
    {
        // Act
        var result = Farrowing.Create(
            ValidSowId,
            ValidTenantId,
            ValidFarrowingDate,
            liveBorn: -1,
            stillborn: 1,
            mummified: 0,
            litterWeightKg: 1.5m);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Farrowing.NegativeCounts");
    }

    [Fact]
    public void Create_WithLiveBornAndZeroWeight_ShouldReturnFailure()
    {
        // Act
        var result = Farrowing.Create(
            ValidSowId,
            ValidTenantId,
            ValidFarrowingDate,
            liveBorn: 10,
            stillborn: 0,
            mummified: 0,
            litterWeightKg: 0m);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Farrowing.InvalidLitterWeight");
    }

    [Fact]
    public void Create_WithUnrealisticPigletWeightTooLow_ShouldReturnFailure()
    {
        // Act (10 live piglets, total weight 1.0 kg = 0.1 kg average, which is unrealistic)
        var result = Farrowing.Create(
            ValidSowId,
            ValidTenantId,
            ValidFarrowingDate,
            liveBorn: 10,
            stillborn: 0,
            mummified: 0,
            litterWeightKg: 1.0m);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Farrowing.UnrealisticWeight");
    }
}
