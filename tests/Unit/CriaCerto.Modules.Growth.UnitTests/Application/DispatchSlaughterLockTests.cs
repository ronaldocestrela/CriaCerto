using CriaCerto.BuildingBlocks.Abstractions.Results;
using CriaCerto.Modules.Growth.Application.Features.DispatchFeatures;
using CriaCerto.Modules.Sanitary.Application.Contracts;
using CriaCerto.Modules.Sanitary.Application.Domain;
using FluentAssertions;
using MediatR;
using NSubstitute;
using Xunit;

namespace CriaCerto.Modules.Growth.UnitTests.Application;

public class DispatchSlaughterLockTests
{
    private readonly ISender _sender = Substitute.For<ISender>();

    [Fact]
    public async Task Handle_WhenSlaughterDispatchAndAnimalUnderWithdrawal_ShouldReturnSanitaryFailure()
    {
        // Arrange
        var animalId = Guid.NewGuid();
        var tenantId = Guid.NewGuid();

        _sender.Send(Arg.Is<ValidateSlaughterEligibilityQuery>(q => q.AnimalId == animalId), Arg.Any<CancellationToken>())
            .Returns(Result.Failure<SlaughterEligibilityDto>(SanitaryErrors.ActiveSlaughterWithdrawalPeriod));

        var handler = new DispatchAnimalCommandHandler(_sender);
        var command = new DispatchAnimalCommand(
            tenantId,
            animalId,
            "BOI-999",
            "Frigorífico Minerva",
            DateTime.UtcNow,
            IsSlaughter: true);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(SanitaryErrors.ActiveSlaughterWithdrawalPeriod);
        result.Error.Code.Should().Be("Sanitary.ActiveSlaughterWithdrawalPeriod");
    }

    [Fact]
    public async Task Handle_WhenSlaughterDispatchAndAnimalEligible_ShouldReturnSuccess()
    {
        // Arrange
        var animalId = Guid.NewGuid();
        var tenantId = Guid.NewGuid();

        var eligibilityDto = new SlaughterEligibilityDto(animalId, IsEligibleForSlaughter: true, 0, null, null);
        _sender.Send(Arg.Is<ValidateSlaughterEligibilityQuery>(q => q.AnimalId == animalId), Arg.Any<CancellationToken>())
            .Returns(Result.Success(eligibilityDto));

        var handler = new DispatchAnimalCommandHandler(_sender);
        var command = new DispatchAnimalCommand(
            tenantId,
            animalId,
            "BOI-888",
            "Frigorífico JBS",
            DateTime.UtcNow,
            IsSlaughter: true);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.AnimalId.Should().Be(animalId);
        result.Value.Destination.Should().Be("Frigorífico JBS");
        result.Value.Status.Should().Be("Despachado");
    }

    [Fact]
    public async Task Handle_WhenNotSlaughterDispatch_ShouldSkipSanitaryCheckAndReturnSuccess()
    {
        // Arrange
        var animalId = Guid.NewGuid();
        var tenantId = Guid.NewGuid();

        var handler = new DispatchAnimalCommandHandler(_sender);
        var command = new DispatchAnimalCommand(
            tenantId,
            animalId,
            "BOI-777",
            "Fazenda Esperança (Recria)",
            DateTime.UtcNow,
            IsSlaughter: false); // Não é para abate!

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        await _sender.DidNotReceive().Send(Arg.Any<ValidateSlaughterEligibilityQuery>(), Arg.Any<CancellationToken>());
    }
}
