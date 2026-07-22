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
