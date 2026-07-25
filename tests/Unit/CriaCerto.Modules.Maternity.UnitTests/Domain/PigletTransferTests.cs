using CriaCerto.Modules.Maternity.Application.Domain;
using CriaCerto.Modules.Maternity.Application.Domain.Events;
using FluentAssertions;

namespace CriaCerto.Modules.Maternity.UnitTests.Domain;

public class PigletTransferTests
{
    private static readonly Guid ValidTenantId = Guid.NewGuid();
    private static readonly Guid SourceFarrowingId = Guid.NewGuid();
    private static readonly Guid SourceSowId = Guid.NewGuid();
    private static readonly Guid TargetFarrowingId = Guid.NewGuid();
    private static readonly Guid TargetSowId = Guid.NewGuid();
    private static readonly DateTime TransferDate = DateTime.UtcNow;

    [Fact]
    public void Create_WithValidParameters_ShouldReturnSuccessAndEmitEvent()
    {
        // Arrange
        int quantity = 3;
        string notes = "Matriz receptora com alta capacidade lactante.";

        // Act
        var result = PigletTransfer.Create(
            ValidTenantId,
            SourceFarrowingId,
            SourceSowId,
            TargetFarrowingId,
            TargetSowId,
            quantity,
            TransferDate,
            notes);

        // Assert
        result.IsSuccess.Should().BeTrue();
        var transfer = result.Value;
        transfer.Id.Should().NotBeEmpty();
        transfer.TenantId.Should().Be(ValidTenantId);
        transfer.SourceFarrowingId.Should().Be(SourceFarrowingId);
        transfer.SourceSowId.Should().Be(SourceSowId);
        transfer.TargetFarrowingId.Should().Be(TargetFarrowingId);
        transfer.TargetSowId.Should().Be(TargetSowId);
        transfer.Quantity.Should().Be(3);
        transfer.TransferDate.Should().Be(TransferDate);
        transfer.Notes.Should().Be(notes);

        transfer.DomainEvents.Should().ContainSingle(e => e is PigletTransferredEvent);
        var domainEvent = transfer.DomainEvents.OfType<PigletTransferredEvent>().Single();
        domainEvent.TransferId.Should().Be(transfer.Id);
        domainEvent.SourceFarrowingId.Should().Be(SourceFarrowingId);
        domainEvent.TargetFarrowingId.Should().Be(TargetFarrowingId);
        domainEvent.Quantity.Should().Be(3);
    }

    [Fact]
    public void Create_WithSameSourceAndTarget_ShouldReturnFailure()
    {
        // Act
        var result = PigletTransfer.Create(
            ValidTenantId,
            SourceFarrowingId,
            SourceSowId,
            SourceFarrowingId,
            SourceSowId,
            quantity: 2,
            TransferDate);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("PigletTransfer.SameSourceAndTarget");
    }

    [Fact]
    public void Create_WithZeroOrNegativeQuantity_ShouldReturnFailure()
    {
        // Act
        var result = PigletTransfer.Create(
            ValidTenantId,
            SourceFarrowingId,
            SourceSowId,
            TargetFarrowingId,
            TargetSowId,
            quantity: 0,
            TransferDate);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("PigletTransfer.InvalidQuantity");
    }
}
