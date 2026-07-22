using CriaCerto.Modules.Breeding.Application.Domain;
using FluentAssertions;

namespace CriaCerto.Modules.Breeding.UnitTests.Domain;

public sealed class SowPregnancyDiagnosisTests
{
    [Fact]
    public void ApplyPregnancyDiagnosis_ShouldMoveBredToPregnant_WhenPositive()
    {
        var sow = CreateBredSow();

        var result = sow.ApplyPregnancyDiagnosis(new DateOnly(2026, 7, 22), PregnancyDiagnosisResult.Pregnant);

        result.IsSuccess.Should().BeTrue();
        sow.ReproductiveStatus.Should().Be(ReproductiveStatus.Pregnant);
    }

    [Theory]
    [InlineData(PregnancyDiagnosisResult.Empty)]
    [InlineData(PregnancyDiagnosisResult.ReturnToEstrus)]
    [InlineData(PregnancyDiagnosisResult.AbortOrDoubt)]
    public void ApplyPregnancyDiagnosis_ShouldMoveBredToEmpty_WhenNotPregnant(PregnancyDiagnosisResult diagnosisResult)
    {
        var sow = CreateBredSow();

        var result = sow.ApplyPregnancyDiagnosis(new DateOnly(2026, 7, 22), diagnosisResult);

        result.IsSuccess.Should().BeTrue();
        sow.ReproductiveStatus.Should().Be(ReproductiveStatus.Empty);
    }

    [Fact]
    public void ApplyPregnancyDiagnosis_ShouldRejectWhenEmpty()
    {
        var sow = new Sow(Guid.NewGuid(), "BR-001", "F1", new DateOnly(2025, 1, 1), 120, reproductiveStatus: ReproductiveStatus.Empty);

        var result = sow.ApplyPregnancyDiagnosis(new DateOnly(2026, 7, 22), PregnancyDiagnosisResult.Pregnant);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Sow.NotBred");
    }

    [Fact]
    public void ApplyPregnancyDiagnosis_ShouldRejectWhenAlreadyPregnant()
    {
        var sow = new Sow(Guid.NewGuid(), "BR-002", "F1", new DateOnly(2025, 1, 1), 120, reproductiveStatus: ReproductiveStatus.Pregnant);

        var result = sow.ApplyPregnancyDiagnosis(new DateOnly(2026, 7, 22), PregnancyDiagnosisResult.Empty);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Sow.NotBred");
    }

    private static Sow CreateBredSow()
    {
        var sow = new Sow(Guid.NewGuid(), "BR-45092", "Landrace", new DateOnly(2025, 1, 1), 120, reproductiveStatus: ReproductiveStatus.Empty);
        sow.RegisterBreeding(new DateOnly(2026, 6, 28), BreedingMethod.ArtificialInsemination, "SEM-001", "Ricardo S.", null);
        return sow;
    }
}
