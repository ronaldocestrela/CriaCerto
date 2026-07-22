namespace CriaCerto.Modules.Breeding.Application.Domain.Services;

public sealed class DnpCalculator : IDnpCalculator
{
    public int Calculate(
        Sow sow,
        IReadOnlyList<BreedingEvent> breedingEvents,
        IReadOnlyList<PregnancyDiagnosis> diagnoses,
        DateOnly asOf)
    {
        if (sow.ReproductiveStatus is ReproductiveStatus.Pregnant or ReproductiveStatus.Lactating)
        {
            var lastPregnantDiagnosis = diagnoses
                .Where(d => d.Result == PregnancyDiagnosisResult.Pregnant)
                .OrderByDescending(d => d.DiagnosisDate)
                .FirstOrDefault();

            if (lastPregnantDiagnosis is null)
            {
                return sow.DnpDays;
            }

            var periodStart = ResolveNonProductivePeriodStart(sow, diagnoses, lastPregnantDiagnosis.DiagnosisDate);
            return Math.Max(0, lastPregnantDiagnosis.DiagnosisDate.DayNumber - periodStart.DayNumber);
        }

        var start = ResolveNonProductivePeriodStart(sow, diagnoses, asOf);
        return Math.Max(0, asOf.DayNumber - start.DayNumber);
    }

    private static DateOnly ResolveNonProductivePeriodStart(
        Sow sow,
        IReadOnlyList<PregnancyDiagnosis> diagnoses,
        DateOnly asOf)
    {
        var lastNonPregnantDiagnosis = diagnoses
            .Where(d => d.Result != PregnancyDiagnosisResult.Pregnant && d.DiagnosisDate <= asOf)
            .OrderByDescending(d => d.DiagnosisDate)
            .FirstOrDefault();

        return lastNonPregnantDiagnosis?.DiagnosisDate ?? sow.EntryDate;
    }
}
