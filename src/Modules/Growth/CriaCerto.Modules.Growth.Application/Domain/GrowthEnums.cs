namespace CriaCerto.Modules.Growth.Application.Domain;

public enum PaddockStatus
{
    Active = 1,
    Resting = 2,    // Pousio
    Maintenance = 3 // Reforma/Manutenção
}

public enum LotCategory
{
    Bezerros = 1,
    Recria = 2,
    Garrotes = 3,
    Engorda = 4,
    Matrizes = 5,
    Reprodutores = 6
}

public enum LotStatus
{
    Active = 1,
    Closed = 2
}
