namespace CriaCerto.Modules.Growth.Application.Domain.Services;

public static class WeightLossAnomalyService
{
    /// <summary>
    /// Checks if an animal has 2 or more consecutive weighings with negative GPD (weight loss).
    /// </summary>
    public static bool IsConsecutiveWeightLoss(IEnumerable<Weighing> weighings)
    {
        if (weighings is null) return false;

        var sortedHistory = weighings
            .OrderByDescending(w => w.WeighingDate)
            .ToList();

        if (sortedHistory.Count < 2) return false;

        int consecutiveLosses = 0;

        foreach (var weighing in sortedHistory)
        {
            if (weighing.CalculatedAdgKgPerDay < 0.0m || weighing.IsWeightLossWarning)
            {
                consecutiveLosses++;
                if (consecutiveLosses >= 2)
                {
                    return true;
                }
            }
            else
            {
                // Reset count if a positive/zero GPD is encountered
                consecutiveLosses = 0;
            }
        }

        return false;
    }
}
