using CriaCerto.Modules.Breeding.Application.Domain;
using FluentAssertions;

namespace CriaCerto.Modules.Breeding.UnitTests.Domain;

public sealed class SowBreedingTests
{
    [Fact]
    public void RegisterBreeding_ShouldMoveEmptyToBred()
    {
        var sow = CreateSow(ReproductiveStatus.Empty);

        var result = sow.RegisterBreeding(
            new DateOnly(2026, 7, 1),
            BreedingMethod.ArtificialInsemination,
            "SEM-001",
            "Ricardo S.",
            BodyConditionScore.Ideal);

        result.IsSuccess.Should().BeTrue();
        sow.ReproductiveStatus.Should().Be(ReproductiveStatus.Bred);
        sow.LastEventName.Should().Be("Cobrição");
    }

    [Fact]
    public void RegisterBreeding_ShouldAllowReServiceWhenAlreadyBred()
    {
        var sow = CreateSow(ReproductiveStatus.Bred);

        var result = sow.RegisterBreeding(
            new DateOnly(2026, 7, 5),
            BreedingMethod.TimedArtificialInsemination,
            "SEM-002",
            "Marcos V.",
            null);

        result.IsSuccess.Should().BeTrue();
        sow.ReproductiveStatus.Should().Be(ReproductiveStatus.Bred);
    }

    [Fact]
    public void RegisterBreeding_ShouldRejectWhenPregnant()
    {
        var sow = CreateSow(ReproductiveStatus.Pregnant);

        var result = sow.RegisterBreeding(
            new DateOnly(2026, 7, 1),
            BreedingMethod.ArtificialInsemination,
            null,
            null,
            null);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Sow.AlreadyPregnant");
    }

    [Fact]
    public void RegisterBreeding_ShouldRejectWhenLactating()
    {
        var sow = CreateSow(ReproductiveStatus.Lactating);

        var result = sow.RegisterBreeding(
            new DateOnly(2026, 7, 1),
            BreedingMethod.NaturalService,
            "BOAR-01",
            null,
            null);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Sow.InvalidBreedingState");
    }

    [Fact]
    public void RegisterBreeding_ShouldRejectWhenCulled()
    {
        var sow = CreateSow(ReproductiveStatus.Empty);
        sow.ChangeLifecycleStatus(LifecycleStatus.Culled, new DateOnly(2026, 6, 1));

        var result = sow.RegisterBreeding(
            new DateOnly(2026, 7, 1),
            BreedingMethod.ArtificialInsemination,
            null,
            null,
            null);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Sow.Culled");
    }

    private static Sow CreateSow(ReproductiveStatus status) =>
        new(Guid.NewGuid(), "BR-45092", "Landrace", new DateOnly(2025, 1, 1), 120, reproductiveStatus: status);
}
