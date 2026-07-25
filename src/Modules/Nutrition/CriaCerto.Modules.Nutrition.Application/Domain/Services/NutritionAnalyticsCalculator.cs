namespace CriaCerto.Modules.Nutrition.Application.Domain.Services;

public record CostPerArrobaResult(
    decimal TotalNutritionCost,
    decimal TotalWeightGainKg,
    decimal ArrobasProduced,
    decimal CostPerArroba);

public static class NutritionAnalyticsCalculator
{
    public static (decimal FeedConversionRatio, decimal FeedEfficiency) CalculateFeedConversion(
        decimal totalDryMatterIntakeKg,
        decimal totalWeightGainKg)
    {
        if (totalWeightGainKg <= 0 || totalDryMatterIntakeKg <= 0)
            return (0m, 0m);

        decimal ca = totalDryMatterIntakeKg / totalWeightGainKg;
        decimal ea = totalWeightGainKg / totalDryMatterIntakeKg;

        return (Math.Round(ca, 2), Math.Round(ea, 4));
    }

    public static CostPerArrobaResult CalculateCostPerArroba(
        decimal totalNutritionCost,
        decimal totalWeightGainKg,
        decimal? carcassYieldPercentage = 50m)
    {
        if (totalWeightGainKg <= 0 || totalNutritionCost <= 0)
            return new CostPerArrobaResult(totalNutritionCost, totalWeightGainKg, 0m, 0m);

        decimal yieldFactor = (carcassYieldPercentage ?? 50m) / 100m;
        // Formula: Arrobas = (Ganho Peso Vivo * RC%) / 15kg
        decimal arrobasProduced = (totalWeightGainKg * yieldFactor) / 15m;

        if (arrobasProduced <= 0)
            return new CostPerArrobaResult(totalNutritionCost, totalWeightGainKg, 0m, 0m);

        decimal costPerArroba = totalNutritionCost / arrobasProduced;

        return new CostPerArrobaResult(
            Math.Round(totalNutritionCost, 2),
            Math.Round(totalWeightGainKg, 2),
            Math.Round(arrobasProduced, 2),
            Math.Round(costPerArroba, 2));
    }
}
