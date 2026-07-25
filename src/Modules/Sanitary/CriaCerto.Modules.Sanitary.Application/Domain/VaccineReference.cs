namespace CriaCerto.Modules.Sanitary.Application.Domain;

public class VaccineReference
{
    public Guid Id { get; private set; }
    public string Code { get; private set; } = string.Empty;
    public string DiseaseName { get; private set; } = string.Empty;
    public string CommercialCategory { get; private set; } = string.Empty;
    public bool IsMandatoryMAPA { get; private set; }
    public string TargetAudience { get; private set; } = string.Empty; // Bezerros, Fêmeas 3 a 8 meses, Rebanho Geral, etc.
    public int? RecommendedAgeMonths { get; private set; }
    public int? BoosterIntervalDays { get; private set; }
    public int DefaultWithdrawalDays { get; private set; } // Dias de carência medicamentosa padrão para abate
    public string Notes { get; private set; } = string.Empty;

    private VaccineReference() { } // For EF Core

    public VaccineReference(
        Guid id,
        string code,
        string diseaseName,
        string commercialCategory,
        bool isMandatoryMAPA,
        string targetAudience,
        int? recommendedAgeMonths,
        int? boosterIntervalDays,
        int defaultWithdrawalDays,
        string notes)
    {
        Id = id == Guid.Empty ? Guid.NewGuid() : id;
        Code = code;
        DiseaseName = diseaseName;
        CommercialCategory = commercialCategory;
        IsMandatoryMAPA = isMandatoryMAPA;
        TargetAudience = targetAudience;
        RecommendedAgeMonths = recommendedAgeMonths;
        BoosterIntervalDays = boosterIntervalDays;
        DefaultWithdrawalDays = defaultWithdrawalDays;
        Notes = notes;
    }
}
