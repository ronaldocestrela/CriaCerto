namespace CriaCerto.Modules.Breeding.Application.Domain.Services;

public static class IepCalculator
{
    /// <summary>
    /// Calcula o Intervalo Entre Partos (IEP) em meses entre dois partos consecutivos de uma matriz bovina.
    /// </summary>
    public static double? CalculateIepMonths(DateTime? previousCalvingDate, DateTime currentCalvingDate)
    {
        if (!previousCalvingDate.HasValue || currentCalvingDate <= previousCalvingDate.Value)
            return null;

        double totalDays = (currentCalvingDate - previousCalvingDate.Value).TotalDays;
        return Math.Round(totalDays / 30.4375, 1); // média de dias no mês
    }

    /// <summary>
    /// Calcula os Dias em Aberto (Open Days): número de dias decorridos entre o último parto e a confirmação de gestação.
    /// </summary>
    public static int? CalculateOpenDays(DateTime? lastCalvingDate, DateTime diagnosisDate)
    {
        if (!lastCalvingDate.HasValue || diagnosisDate < lastCalvingDate.Value)
            return null;

        return (int)(diagnosisDate - lastCalvingDate.Value).TotalDays;
    }
}
