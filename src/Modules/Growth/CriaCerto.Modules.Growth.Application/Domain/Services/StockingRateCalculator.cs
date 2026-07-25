namespace CriaCerto.Modules.Growth.Application.Domain.Services;

public static class StockingRateCalculator
{
    public const decimal KgPerAnimalUnit = 450.0m;

    public static decimal CalculateTotalUA(decimal totalWeightKg)
    {
        if (totalWeightKg <= 0) return 0m;
        return Math.Round(totalWeightKg / KgPerAnimalUnit, 2);
    }

    public static decimal CalculateStockingRate(decimal totalUA, decimal areaHectares)
    {
        if (areaHectares <= 0) return 0m;
        return Math.Round(totalUA / areaHectares, 2);
    }

    public static bool IsOvergrazed(decimal currentTotalUA, decimal maxCapacityUA)
    {
        return currentTotalUA > maxCapacityUA;
    }

    public static bool IsNearCapacity(decimal currentTotalUA, decimal maxCapacityUA, decimal warningThresholdRatio = 0.85m)
    {
        if (maxCapacityUA <= 0) return false;
        return (currentTotalUA / maxCapacityUA) >= warningThresholdRatio && !IsOvergrazed(currentTotalUA, maxCapacityUA);
    }
}
