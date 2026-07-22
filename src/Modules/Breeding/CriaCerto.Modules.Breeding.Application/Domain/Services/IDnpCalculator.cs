namespace CriaCerto.Modules.Breeding.Application.Domain.Services;

public interface IDnpCalculator
{
    int Calculate(
        Sow sow,
        IReadOnlyList<BreedingEvent> breedingEvents,
        IReadOnlyList<PregnancyDiagnosis> diagnoses,
        DateOnly asOf);
}
