namespace CriaCerto.Modules.Growth.Application.Domain.Services;

public static class GrowthZootecnicCalculator
{
    public const decimal KgPerArroba = 15.0m;

    public static decimal CalculateArrobas(decimal weightKg, decimal carcassYieldPercentage)
    {
        if (weightKg <= 0 || carcassYieldPercentage <= 0)
            return 0.0m;

        decimal carcassWeightKg = weightKg * (carcassYieldPercentage / 100.0m);
        return Math.Round(carcassWeightKg / KgPerArroba, 2);
    }

    public static decimal CalculateAdg(decimal currentWeightKg, decimal previousWeightKg, DateTime currentDate, DateTime previousDate)
    {
        int days = (currentDate.Date - previousDate.Date).Days;
        if (days <= 0)
            return 0.0m;

        decimal weightDiff = currentWeightKg - previousWeightKg;
        return Math.Round(weightDiff / days, 2);
    }

    public static decimal CalculateMonthlyArrobaGain(decimal adgKgPerDay, decimal carcassYieldPercentage)
    {
        if (adgKgPerDay == 0.0m || carcassYieldPercentage <= 0)
            return 0.0m;

        decimal monthlyWeightGainKg = adgKgPerDay * 30.0m;
        decimal monthlyCarcassGainKg = monthlyWeightGainKg * (carcassYieldPercentage / 100.0m);
        return Math.Round(monthlyCarcassGainKg / KgPerArroba, 2);
    }
}
