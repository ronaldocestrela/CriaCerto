using CriaCerto.BuildingBlocks.Abstractions.Results;
using CriaCerto.BuildingBlocks.Abstractions.Tenancy;
using CriaCerto.Modules.Maternity.Application.Abstractions;
using CriaCerto.Modules.Maternity.Application.Contracts;
using CriaCerto.Modules.Maternity.Application.Domain;
using CriaCerto.Modules.Maternity.Application.Features.Farrowing;
using FluentAssertions;
using NSubstitute;

namespace CriaCerto.Modules.Maternity.UnitTests.Features;

public class RegisterFarrowingCommandHandlerTests
{
    private readonly IFarrowingRepository _repository = Substitute.For<IFarrowingRepository>();
    private readonly ITenantContext _tenantContext = Substitute.For<ITenantContext>();
    private readonly RegisterFarrowingCommandHandler _handler;

    private static readonly Guid TenantId = Guid.NewGuid();
    private static readonly Guid SowId = Guid.NewGuid();

    public RegisterFarrowingCommandHandlerTests()
    {
        _tenantContext.TenantId.Returns(TenantId);
        _handler = new RegisterFarrowingCommandHandler(_repository, _tenantContext);
    }

    [Fact]
    public async Task Handle_WithValidCommand_ShouldSaveFarrowingAndReturnSuccess()
    {
        // Arrange
        var command = new RegisterFarrowingCommand(
            SowId: SowId,
            FarrowingDate: DateTime.UtcNow,
            LiveBorn: 14,
            Stillborn: 1,
            Mummified: 0,
            LitterWeightKg: 19.6m,
            MaternityRoomId: "Maternidade-A",
            Assisted: true,
            Notes: "Parto bem sucedido");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.SowId.Should().Be(SowId);
        result.Value.TenantId.Should().Be(TenantId);
        result.Value.LiveBorn.Should().Be(14);
        result.Value.Stillborn.Should().Be(1);
        result.Value.Mummified.Should().Be(0);
        result.Value.TotalBorn.Should().Be(15);
        result.Value.LitterWeightKg.Should().Be(19.6m);

        await _repository.Received(1).AddAsync(Arg.Any<CriaCerto.Modules.Maternity.Application.Domain.Farrowing>(), Arg.Any<CancellationToken>());
        await _repository.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithInvalidDomainData_ShouldReturnFailureWithoutSaving()
    {
        // Arrange (Zero total born)
        var command = new RegisterFarrowingCommand(
            SowId: SowId,
            FarrowingDate: DateTime.UtcNow,
            LiveBorn: 0,
            Stillborn: 0,
            Mummified: 0,
            LitterWeightKg: 0m,
            MaternityRoomId: null,
            Assisted: false,
            Notes: null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Farrowing.ZeroTotalBorn");

        await _repository.DidNotReceive().AddAsync(Arg.Any<CriaCerto.Modules.Maternity.Application.Domain.Farrowing>(), Arg.Any<CancellationToken>());
        await _repository.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenTenantContextMissing_ShouldReturnUnauthorizedFailure()
    {
        // Arrange
        _tenantContext.TenantId.Returns((Guid?)null);

        var command = new RegisterFarrowingCommand(
            SowId: SowId,
            FarrowingDate: DateTime.UtcNow,
            LiveBorn: 10,
            Stillborn: 0,
            Mummified: 0,
            LitterWeightKg: 14m,
            MaternityRoomId: null,
            Assisted: false,
            Notes: null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Type.Should().Be(ErrorType.Unauthorized);

        await _repository.DidNotReceive().AddAsync(Arg.Any<CriaCerto.Modules.Maternity.Application.Domain.Farrowing>(), Arg.Any<CancellationToken>());
    }
}
