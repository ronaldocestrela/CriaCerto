using CriaCerto.Modules.Breeding.Application.Domain;
using CriaCerto.Modules.Breeding.Application.Domain.Services;
using FluentAssertions;

namespace CriaCerto.Modules.Breeding.UnitTests.Domain;

public sealed class DnpCalculatorTests
{
    private readonly DnpCalculator _calculator = new();

    [Fact]
    public void Calculate_ShouldCountDaysInEmptyStatusFromEntryDate()
    {
        var sow = new Sow(Guid.NewGuid(), "BR-001", "F1", new DateOnly(2026, 7, 1), 120, reproductiveStatus: ReproductiveStatus.Empty);
        var asOf = new DateOnly(2026, 7, 22);

        var dnp = _calculator.Calculate(sow, [], [], asOf);

        dnp.Should().Be(21);
    }

    [Fact]
    public void Calculate_ShouldCountDaysInBredStatus()
    {
        var sow = new Sow(Guid.NewGuid(), "BR-002", "F1", new DateOnly(2026, 6, 1), 120, reproductiveStatus: ReproductiveStatus.Empty);
        sow.RegisterBreeding(new DateOnly(2026, 7, 1), BreedingMethod.ArtificialInsemination, null, null, null);
        var asOf = new DateOnly(2026, 7, 22);

        var dnp = _calculator.Calculate(sow, [], [], asOf);

        dnp.Should().Be(51);
    }

    [Fact]
    public void Calculate_ShouldFreezeDnpAtPregnancyConfirmation()
    {
        var sowId = Guid.NewGuid();
        var sow = new Sow(sowId, "BR-003", "F1", new DateOnly(2026, 6, 1), 120, reproductiveStatus: ReproductiveStatus.Empty);
        sow.RegisterBreeding(new DateOnly(2026, 7, 1), BreedingMethod.ArtificialInsemination, null, null, null);
        var breedingEvent = new BreedingEvent(Guid.NewGuid(), sowId, new DateOnly(2026, 7, 1), BreedingMethod.ArtificialInsemination, null, null, null, null, null);
        var diagnosis = new PregnancyDiagnosis(
            Guid.NewGuid(),
            sowId,
            breedingEvent.Id,
            new DateOnly(2026, 7, 22),
            PregnancyDiagnosisMethod.Ultrasound,
            PregnancyDiagnosisResult.Pregnant,
            null);
        sow.ApplyPregnancyDiagnosis(new DateOnly(2026, 7, 22), PregnancyDiagnosisResult.Pregnant);

        var dnpAtPregnancy = _calculator.Calculate(sow, [breedingEvent], [diagnosis], new DateOnly(2026, 7, 22));
        var dnpLater = _calculator.Calculate(sow, [breedingEvent], [diagnosis], new DateOnly(2026, 8, 22));

        dnpAtPregnancy.Should().Be(51);
        dnpLater.Should().Be(51);
    }

    [Fact]
    public void Calculate_ShouldResetAfterFailedDiagnosis()
    {
        var sowId = Guid.NewGuid();
        var sow = new Sow(sowId, "BR-004", "F1", new DateOnly(2026, 5, 1), 120, reproductiveStatus: ReproductiveStatus.Empty);
        sow.RegisterBreeding(new DateOnly(2026, 6, 1), BreedingMethod.ArtificialInsemination, null, null, null);
        var breedingEvent = new BreedingEvent(Guid.NewGuid(), sowId, new DateOnly(2026, 6, 1), BreedingMethod.ArtificialInsemination, null, null, null, null, null);
        var failedDiagnosis = new PregnancyDiagnosis(
            Guid.NewGuid(),
            sowId,
            breedingEvent.Id,
            new DateOnly(2026, 6, 22),
            PregnancyDiagnosisMethod.Ultrasound,
            PregnancyDiagnosisResult.Empty,
            null);
        sow.ApplyPregnancyDiagnosis(new DateOnly(2026, 6, 22), PregnancyDiagnosisResult.Empty);

        var dnp = _calculator.Calculate(sow, [breedingEvent], [failedDiagnosis], new DateOnly(2026, 7, 22));

        dnp.Should().Be(30);
    }
}
