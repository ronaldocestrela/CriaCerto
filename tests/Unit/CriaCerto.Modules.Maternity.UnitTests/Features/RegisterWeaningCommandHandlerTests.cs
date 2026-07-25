using CriaCerto.BuildingBlocks.Abstractions.Tenancy;
using CriaCerto.Modules.Maternity.Application.Abstractions;
using CriaCerto.Modules.Maternity.Application.Domain;
using CriaCerto.Modules.Maternity.Application.Features.Weaning;
using FluentAssertions;
using NSubstitute;

namespace CriaCerto.Modules.Maternity.UnitTests.Features;

public class RegisterWeaningCommandHandlerTests
{
    private readonly IFarrowingRepository _farrowingRepository = Substitute.For<IFarrowingRepository>();
    private readonly IPigletTransferRepository _transferRepository = Substitute.For<IPigletTransferRepository>();
    private readonly IWeaningRepository _weaningRepository = Substitute.For<IWeaningRepository>();
    private readonly ITenantContext _tenantContext = Substitute.For<ITenantContext>();
    private readonly RegisterWeaningCommandHandler _handler;

    private static readonly Guid TenantId = Guid.NewGuid();
    private static readonly Guid SowId = Guid.NewGuid();

    public RegisterWeaningCommandHandlerTests()
    {
        _tenantContext.TenantId.Returns(TenantId);
        _handler = new RegisterWeaningCommandHandler(_farrowingRepository, _transferRepository, _weaningRepository, _tenantContext);
    }

    [Fact]
    public async Task Handle_WithValidWeaning_ShouldCreateWeaningAndReturnSuccess()
    {
        // Arrange
        var farrowing = Farrowing.Create(SowId, TenantId, DateTime.UtcNow.AddDays(-21), liveBorn: 12, stillborn: 0, mummified: 0, litterWeightKg: 18m).Value;

        _farrowingRepository.GetByIdAsync(farrowing.Id, Arg.Any<CancellationToken>()).Returns(farrowing);
        _weaningRepository.GetByFarrowingIdAsync(farrowing.Id, Arg.Any<CancellationToken>()).Returns((Weaning?)null);
        _transferRepository.GetBySourceFarrowingIdAsync(farrowing.Id, Arg.Any<CancellationToken>()).Returns(new List<PigletTransfer>());
        _transferRepository.GetByTargetFarrowingIdAsync(farrowing.Id, Arg.Any<CancellationToken>()).Returns(new List<PigletTransfer>());

        var command = new RegisterWeaningCommand(farrowing.Id, DateTime.UtcNow, 11, 77m, "Creche Baia 01", "Desmame 21 dias");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.WeanedCount.Should().Be(11);
        result.Value.TotalWeanedWeightKg.Should().Be(77m);
        result.Value.AverageWeanedWeightKg.Should().Be(7m);

        await _weaningRepository.Received(1).AddAsync(Arg.Any<Weaning>(), Arg.Any<CancellationToken>());
        await _weaningRepository.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenAlreadyWeaned_ShouldReturnFailure()
    {
        // Arrange
        var farrowing = Farrowing.Create(SowId, TenantId, DateTime.UtcNow.AddDays(-21), liveBorn: 10, stillborn: 0, mummified: 0, litterWeightKg: 15m).Value;
        var existingWeaning = Weaning.Create(TenantId, farrowing.Id, SowId, DateTime.UtcNow, 10, 70m, "Creche A").Value;

        _farrowingRepository.GetByIdAsync(farrowing.Id, Arg.Any<CancellationToken>()).Returns(farrowing);
        _weaningRepository.GetByFarrowingIdAsync(farrowing.Id, Arg.Any<CancellationToken>()).Returns(existingWeaning);

        var command = new RegisterWeaningCommand(farrowing.Id, DateTime.UtcNow, 10, 70m, "Creche B", null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Weaning.AlreadyWeaned");
    }
}
