using CriaCerto.Modules.Maternity.Application.Domain;
using CriaCerto.Modules.Maternity.Application.Domain.Events;
using FluentAssertions;

namespace CriaCerto.Modules.Maternity.UnitTests.Domain;

public class WeaningTests
{
    private static readonly Guid ValidTenantId = Guid.NewGuid();
    private static readonly Guid ValidFarrowingId = Guid.NewGuid();
    private static readonly Guid ValidSowId = Guid.NewGuid();
    private static readonly DateTime ValidWeaningDate = DateTime.UtcNow;

    [Fact]
    public void Create_WithValidParameters_ShouldReturnSuccessAndEmitEvent()
    {
        // Arrange
        int weanedCount = 11;
        decimal totalWeight = 77.0m; // 77 / 11 = 7.0 kg per piglet (valid)
        string destination = "Creche Lote 2026-A";
        string notes = "Desmame com excelente uniformidade.";

        // Act
        var result = Weaning.Create(
            ValidTenantId,
            ValidFarrowingId,
            ValidSowId,
            ValidWeaningDate,
            weanedCount,
            totalWeight,
            destination,
            notes);

        // Assert
        result.IsSuccess.Should().BeTrue();
        var weaning = result.Value;
        weaning.Id.Should().NotBeEmpty();
        weaning.TenantId.Should().Be(ValidTenantId);
        weaning.FarrowingId.Should().Be(ValidFarrowingId);
        weaning.SowId.Should().Be(ValidSowId);
        weaning.WeaningDate.Should().Be(ValidWeaningDate);
        weaning.WeanedCount.Should().Be(11);
        weaning.TotalWeanedWeightKg.Should().Be(77.0m);
        weaning.AverageWeanedWeightKg.Should().Be(7.0m);
        weaning.DestinationPenOrBatch.Should().Be(destination);
        weaning.Notes.Should().Be(notes);

        weaning.DomainEvents.Should().ContainSingle(e => e is WeaningCompletedEvent);
        var domainEvent = weaning.DomainEvents.OfType<WeaningCompletedEvent>().Single();
        domainEvent.WeaningId.Should().Be(weaning.Id);
        domainEvent.FarrowingId.Should().Be(ValidFarrowingId);
        domainEvent.SowId.Should().Be(ValidSowId);
        domainEvent.WeanedCount.Should().Be(11);
    }

    [Fact]
    public void Create_WithZeroWeanedCount_ShouldReturnFailure()
    {
        // Act
        var result = Weaning.Create(
            ValidTenantId,
            ValidFarrowingId,
            ValidSowId,
            ValidWeaningDate,
            weanedCount: 0,
            totalWeanedWeightKg: 0m,
            destinationPenOrBatch: "Creche A");

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Weaning.InvalidCount");
    }

    [Fact]
    public void Create_WithUnrealisticWeanedWeightTooLow_ShouldReturnFailure()
    {
        // Act (10 piglets weighing 10kg = 1.0kg each, which is below 4.0kg minimum)
        var result = Weaning.Create(
            ValidTenantId,
            ValidFarrowingId,
            ValidSowId,
            ValidWeaningDate,
            weanedCount: 10,
            totalWeanedWeightKg: 10.0m,
            destinationPenOrBatch: "Creche A");

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Weaning.UnrealisticWeanedWeight");
    }
}
