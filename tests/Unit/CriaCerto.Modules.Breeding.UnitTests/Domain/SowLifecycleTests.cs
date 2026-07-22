using CriaCerto.Modules.Breeding.Application.Domain;
using FluentAssertions;

namespace CriaCerto.Modules.Breeding.UnitTests.Domain;

public sealed class SowLifecycleTests
{
    [Fact]
    public void ChangeLifecycleStatus_ShouldAllowActiveToQuarantine()
    {
        var sow = CreateSow();

        var result = sow.ChangeLifecycleStatus(LifecycleStatus.Quarantine, new DateOnly(2026, 7, 22), "Suspeita sanitária");

        result.IsSuccess.Should().BeTrue();
        sow.LifecycleStatus.Should().Be(LifecycleStatus.Quarantine);
        sow.RequiresAttention.Should().BeTrue();
    }

    [Fact]
    public void ChangeLifecycleStatus_ShouldAllowQuarantineToActive()
    {
        var sow = CreateSow();
        sow.ChangeLifecycleStatus(LifecycleStatus.Quarantine, new DateOnly(2026, 7, 22));

        var result = sow.ChangeLifecycleStatus(LifecycleStatus.Active, new DateOnly(2026, 7, 23));

        result.IsSuccess.Should().BeTrue();
        sow.LifecycleStatus.Should().Be(LifecycleStatus.Active);
    }

    [Fact]
    public void ChangeLifecycleStatus_ShouldRejectChangesAfterCulled()
    {
        var sow = CreateSow();
        sow.ChangeLifecycleStatus(LifecycleStatus.Culled, new DateOnly(2026, 7, 22));

        var result = sow.ChangeLifecycleStatus(LifecycleStatus.Active, new DateOnly(2026, 7, 23));

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Sow.InvalidTransition");
        sow.LifecycleStatus.Should().Be(LifecycleStatus.Culled);
    }

    [Fact]
    public void RequiresAttention_ShouldBeTrue_WhenDnpIsFortyOrMore()
    {
        var sow = new Sow(Guid.NewGuid(), "BR-0092", "F1", new DateOnly(2025, 1, 1), 120, dnpDays: 45);

        sow.RequiresAttention.Should().BeTrue();
    }

    private static Sow CreateSow() =>
        new(Guid.NewGuid(), "BR-8042", "Landrace x Large White", new DateOnly(2025, 1, 1), 135);
}
