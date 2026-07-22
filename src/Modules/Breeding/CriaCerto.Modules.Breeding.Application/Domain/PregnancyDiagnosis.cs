namespace CriaCerto.Modules.Breeding.Application.Domain;

public sealed class PregnancyDiagnosis
{
    private PregnancyDiagnosis()
    {
    }

    public PregnancyDiagnosis(
        Guid id,
        Guid sowId,
        Guid? breedingEventId,
        DateOnly diagnosisDate,
        PregnancyDiagnosisMethod method,
        PregnancyDiagnosisResult result,
        string? notes)
    {
        Id = id;
        SowId = sowId;
        BreedingEventId = breedingEventId;
        DiagnosisDate = diagnosisDate;
        Method = method;
        Result = result;
        Notes = notes?.Trim();
    }

    public Guid Id { get; private set; }
    public Guid SowId { get; private set; }
    public Guid? BreedingEventId { get; private set; }
    public DateOnly DiagnosisDate { get; private set; }
    public PregnancyDiagnosisMethod Method { get; private set; }
    public PregnancyDiagnosisResult Result { get; private set; }
    public string? Notes { get; private set; }
}
