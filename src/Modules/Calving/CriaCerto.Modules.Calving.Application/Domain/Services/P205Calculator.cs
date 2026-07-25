namespace CriaCerto.Modules.Calving.Application.Domain.Services;

public static class P205Calculator
{
    /// <summary>
    /// Calcula o Peso Ajustado aos 205 dias (P205) para desmame bovino.
    /// Formula: P205 = ((PesoDesmame - PesoNascimento) / IdadeEmDias * 205) + PesoNascimento * FatorIdadeMae
    /// </summary>
    public static decimal CalculateP205(
        decimal birthWeightKg,
        decimal weaningWeightKg,
        DateTime birthDate,
        DateTime weaningDate,
        int motherAgeYears)
    {
        int ageInDays = (int)(weaningDate - birthDate).TotalDays;
        if (ageInDays <= 0)
            return weaningWeightKg;

        decimal adg = (weaningWeightKg - birthWeightKg) / ageInDays;
        decimal baseP205 = (adg * 205m) + birthWeightKg;

        // Fator de correção de idade da matriz (Associação Brasileira de Criadores de Zebu - ABCZ)
        decimal motherAgeFactor = motherAgeYears switch
        {
            <= 3 => 1.15m, // Matriz primípara/jovem
            >= 4 and <= 10 => 1.00m, // Matriz adulta em pico de produção
            _ => 1.05m // Matriz idosa
        };

        return Math.Round(baseP205 * motherAgeFactor, 2);
    }
}
