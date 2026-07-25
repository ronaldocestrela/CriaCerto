namespace CriaCerto.Modules.Nutrition.Application.Domain;

public enum FeedCategory
{
    BulkGrain = 1,     // Milho, Sorgo, Farelo de Soja
    ForageSilage = 2,  // Silagem de Milho, Capineira, Feno
    MineralSalt = 3,   // Sal Mineral, Proteinado de Seca/Águas
    Additive = 4       // Premix, Núcleo, Virginiamicina/Monensina
}

public enum RationType
{
    PastureSupplement = 1, // Suplementação mineral/proteinada em pasto
    FeedlotTmr = 2,        // Ração Total Misturada de Confinamento
    Transition = 3         // Ração de Adaptação / Transição
}

public enum TroughScore
{
    Score0_Clean = 0,     // Cocho Limpo / Fome
    Score1_ThinLayer = 1, // Lâmina Fina / Ideal
    Score2_Excessive = 2, // Sobra Excessiva / Reduzir Trato
    Score3_Untouched = 3  // Trato Intacto / Rejeição
}
