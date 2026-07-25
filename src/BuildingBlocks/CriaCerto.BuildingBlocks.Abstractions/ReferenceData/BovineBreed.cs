namespace CriaCerto.BuildingBlocks.Abstractions.ReferenceData;

public class BovineBreed
{
    public Guid Id { get; private set; }
    public string Code { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public string Category { get; private set; } = string.Empty; // Zebuíno, Taurino, Sintético/Composto, Misto
    public string Aptitude { get; private set; } = string.Empty; // Corte, Leite, Dupla Aptidão
    public string Origin { get; private set; } = string.Empty; // Brasil, Índia, Escócia, França, etc.
    public bool IsOfficial { get; private set; } = true;

    private BovineBreed() { } // For EF Core

    public BovineBreed(Guid id, string code, string name, string category, string aptitude, string origin, bool isOfficial = true)
    {
        Id = id == Guid.Empty ? Guid.NewGuid() : id;
        Code = code;
        Name = name;
        Category = category;
        Aptitude = aptitude;
        Origin = origin;
        IsOfficial = isOfficial;
    }
}
