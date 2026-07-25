using CriaCerto.BuildingBlocks.Abstractions.Tenancy;
using CriaCerto.Modules.Maternity.Application.Abstractions;
using CriaCerto.Modules.Maternity.Application.Domain;
using CriaCerto.Modules.Maternity.Application.Features.CrossFostering;
using FluentAssertions;
using NSubstitute;

namespace CriaCerto.Modules.Maternity.UnitTests.Features;

public class CrossFosteringCommandHandlerTests
{
    private readonly IFarrowingRepository _farrowingRepository = Substitute.For<IFarrowingRepository>();
    private readonly IPigletTransferRepository _transferRepository = Substitute.For<IPigletTransferRepository>();
    private readonly ITenantContext _tenantContext = Substitute.For<ITenantContext>();
    private readonly TransferPigletCommandHandler _handler;

    private static readonly Guid TenantId = Guid.NewGuid();
    private static readonly Guid SourceSowId = Guid.NewGuid();
    private static readonly Guid TargetSowId = Guid.NewGuid();

    public CrossFosteringCommandHandlerTests()
    {
        _tenantContext.TenantId.Returns(TenantId);
        _handler = new TransferPigletCommandHandler(_farrowingRepository, _transferRepository, _tenantContext);
    }

    [Fact]
    public async Task Handle_WithValidTransfer_ShouldCreateTransferAndReturnSuccess()
    {
        // Arrange
        var sourceFarrowing = Farrowing.Create(SourceSowId, TenantId, DateTime.UtcNow, liveBorn: 14, stillborn: 0, mummified: 0, litterWeightKg: 20m).Value;
        var targetFarrowing = Farrowing.Create(TargetSowId, TenantId, DateTime.UtcNow, liveBorn: 8, stillborn: 0, mummified: 0, litterWeightKg: 11m).Value;

        _farrowingRepository.GetByIdAsync(sourceFarrowing.Id, Arg.Any<CancellationToken>()).Returns(sourceFarrowing);
        _farrowingRepository.GetByIdAsync(targetFarrowing.Id, Arg.Any<CancellationToken>()).Returns(targetFarrowing);
        _transferRepository.GetBySourceFarrowingIdAsync(sourceFarrowing.Id, Arg.Any<CancellationToken>()).Returns(new List<PigletTransfer>());
        _transferRepository.GetByTargetFarrowingIdAsync(sourceFarrowing.Id, Arg.Any<CancellationToken>()).Returns(new List<PigletTransfer>());

        var command = new TransferPigletCommand(sourceFarrowing.Id, targetFarrowing.Id, 3, DateTime.UtcNow, "Adoção balanceadora");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Quantity.Should().Be(3);
        result.Value.SourceFarrowingId.Should().Be(sourceFarrowing.Id);
        result.Value.TargetFarrowingId.Should().Be(targetFarrowing.Id);

        await _transferRepository.Received(1).AddAsync(Arg.Any<PigletTransfer>(), Arg.Any<CancellationToken>());
        await _transferRepository.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenQuantityExceedsAvailablePiglets_ShouldReturnFailure()
    {
        // Arrange
        var sourceFarrowing = Farrowing.Create(SourceSowId, TenantId, DateTime.UtcNow, liveBorn: 4, stillborn: 0, mummified: 0, litterWeightKg: 6m).Value;
        var targetFarrowing = Farrowing.Create(TargetSowId, TenantId, DateTime.UtcNow, liveBorn: 10, stillborn: 0, mummified: 0, litterWeightKg: 14m).Value;

        _farrowingRepository.GetByIdAsync(sourceFarrowing.Id, Arg.Any<CancellationToken>()).Returns(sourceFarrowing);
        _farrowingRepository.GetByIdAsync(targetFarrowing.Id, Arg.Any<CancellationToken>()).Returns(targetFarrowing);
        _transferRepository.GetBySourceFarrowingIdAsync(sourceFarrowing.Id, Arg.Any<CancellationToken>()).Returns(new List<PigletTransfer>());
        _transferRepository.GetByTargetFarrowingIdAsync(sourceFarrowing.Id, Arg.Any<CancellationToken>()).Returns(new List<PigletTransfer>());

        var command = new TransferPigletCommand(sourceFarrowing.Id, targetFarrowing.Id, 5, DateTime.UtcNow, "Tentativa excessiva");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("PigletTransfer.InsufficientPiglets");
    }
}
