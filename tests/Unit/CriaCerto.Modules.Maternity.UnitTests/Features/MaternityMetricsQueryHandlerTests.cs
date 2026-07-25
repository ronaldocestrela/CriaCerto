using CriaCerto.Modules.Maternity.Application.Abstractions;
using CriaCerto.Modules.Maternity.Application.Domain;
using CriaCerto.Modules.Maternity.Application.Features.Metrics;
using FluentAssertions;
using NSubstitute;

namespace CriaCerto.Modules.Maternity.UnitTests.Features;

public class MaternityMetricsQueryHandlerTests
{
    private readonly IFarrowingRepository _farrowingRepository = Substitute.For<IFarrowingRepository>();
    private readonly IPigletTransferRepository _transferRepository = Substitute.For<IPigletTransferRepository>();
    private readonly IWeaningRepository _weaningRepository = Substitute.For<IWeaningRepository>();
    private readonly GetMaternityMetricsQueryHandler _handler;

    private static readonly Guid TenantId = Guid.NewGuid();
    private static readonly Guid Sow1Id = Guid.NewGuid();
    private static readonly Guid Sow2Id = Guid.NewGuid();

    public MaternityMetricsQueryHandlerTests()
    {
        _handler = new GetMaternityMetricsQueryHandler(_farrowingRepository, _transferRepository, _weaningRepository);
    }

    [Fact]
    public async Task Handle_WithFarrowingsAndWeanings_ShouldCalculateMetricsCorrectly()
    {
        // Arrange
        var f1 = Farrowing.Create(Sow1Id, TenantId, DateTime.UtcNow, liveBorn: 12, stillborn: 1, mummified: 0, litterWeightKg: 18m).Value;
        var f2 = Farrowing.Create(Sow2Id, TenantId, DateTime.UtcNow, liveBorn: 14, stillborn: 0, mummified: 0, litterWeightKg: 21m).Value;

        var w1 = Weaning.Create(TenantId, f1.Id, Sow1Id, DateTime.UtcNow, weanedCount: 11, totalWeanedWeightKg: 77m, "Creche 1").Value;
        var w2 = Weaning.Create(TenantId, f2.Id, Sow2Id, DateTime.UtcNow, weanedCount: 13, totalWeanedWeightKg: 91m, "Creche 2").Value;

        _farrowingRepository.GetAllAsync(Arg.Any<CancellationToken>()).Returns(new List<Farrowing> { f1, f2 });
        _weaningRepository.GetAllAsync(Arg.Any<CancellationToken>()).Returns(new List<Weaning> { w1, w2 });
        _transferRepository.GetAllAsync(Arg.Any<CancellationToken>()).Returns(new List<PigletTransfer>());

        var query = new GetMaternityMetricsQuery();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        var metrics = result.Value;
        metrics.TotalLiveBornInPeriod.Should().Be(26);
        metrics.TotalWeanedInPeriod.Should().Be(24);
        metrics.TotalActiveSows.Should().Be(2);

        metrics.Nvma.Should().Be(13m); // 26 live born / 2 sows
        metrics.Dma.Should().Be(12m);  // 24 weaned / 2 sows
        metrics.PreWeaningMortalityRate.Should().Be(7.69m); // (26 - 24) / 26 * 100 = 7.69%
    }
}
