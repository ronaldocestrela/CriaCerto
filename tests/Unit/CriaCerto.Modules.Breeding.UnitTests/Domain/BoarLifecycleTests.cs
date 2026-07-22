using CriaCerto.Modules.Breeding.Application.Domain;
using FluentAssertions;

namespace CriaCerto.Modules.Breeding.UnitTests.Domain;

public sealed class BoarLifecycleTests
{
    [Fact]
    public void ChangeLifecycleStatus_ShouldAllowActiveToQuarantine()
    {
        var boar = CreateBoar();

        var result = boar.ChangeLifecycleStatus(LifecycleStatus.Quarantine, new DateOnly(2026, 7, 22));

        result.IsSuccess.Should().BeTrue();
        boar.LifecycleStatus.Should().Be(LifecycleStatus.Quarantine);
        boar.RequiresAttention.Should().BeTrue();
    }

    [Fact]
    public void ChangeLifecycleStatus_ShouldRejectChangesAfterCulled()
    {
        var boar = CreateBoar();
        boar.ChangeLifecycleStatus(LifecycleStatus.Culled, new DateOnly(2026, 7, 22));

        var result = boar.ChangeLifecycleStatus(LifecycleStatus.Quarantine, new DateOnly(2026, 7, 23));

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Boar.InvalidTransition");
        boar.LifecycleStatus.Should().Be(LifecycleStatus.Culled);
    }

    private static Boar CreateBoar() =>
        new(Guid.NewGuid(), "BO-302", "Duroc", new DateOnly(2025, 1, 1), 180);
}
