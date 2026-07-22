namespace CriaCerto.Modules.Breeding.Application.Domain;

public enum LifecycleStatus
{
    Active = 1,
    Quarantine = 2,
    Culled = 3
}

public enum ReproductiveStatus
{
    Empty = 1,
    Bred = 2,
    Pregnant = 3,
    Lactating = 4
}

public enum BodyConditionScore
{
    VeryThin = 1,
    Thin = 2,
    Ideal = 3,
    Fat = 4,
    VeryFat = 5
}

public enum BreedingMethod
{
    ArtificialInsemination = 1,
    TimedArtificialInsemination = 2,
    NaturalService = 3
}

public enum PregnancyDiagnosisResult
{
    Pregnant = 1,
    Empty = 2,
    ReturnToEstrus = 3,
    AbortOrDoubt = 4
}

public enum PregnancyDiagnosisMethod
{
    Ultrasound = 1,
    ReturnToEstrus = 2
}
