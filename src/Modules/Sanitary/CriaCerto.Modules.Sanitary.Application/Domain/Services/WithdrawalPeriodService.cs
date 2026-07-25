namespace CriaCerto.Modules.Sanitary.Application.Domain.Services;

public sealed record SlaughterEligibilityResult(
    Guid AnimalId,
    bool IsEligibleForSlaughter,
    int RemainingWithdrawalDays,
    string? BlockingTreatmentName,
    DateTime? ActiveWithdrawalEndsAtUtc);

public static class WithdrawalPeriodService
{
    public static SlaughterEligibilityResult EvaluateSlaughterEligibility(
        Guid animalId,
        IEnumerable<TreatmentRecord> treatments,
        DateTime checkDateUtc)
    {
        var activeTreatments = treatments
            .Where(t => t.IsWithdrawalPeriodActive(checkDateUtc))
            .OrderByDescending(t => t.WithdrawalEndDateUtc)
            .ToList();

        if (!activeTreatments.Any())
        {
            return new SlaughterEligibilityResult(
                animalId,
                IsEligibleForSlaughter: true,
                RemainingWithdrawalDays: 0,
                BlockingTreatmentName: null,
                ActiveWithdrawalEndsAtUtc: null);
        }

        var primaryBlockingTreatment = activeTreatments.First();
        var remainingTimeSpan = primaryBlockingTreatment.WithdrawalEndDateUtc - checkDateUtc;
        int remainingDays = (int)Math.Ceiling(remainingTimeSpan.TotalDays);

        return new SlaughterEligibilityResult(
            animalId,
            IsEligibleForSlaughter: false,
            RemainingWithdrawalDays: Math.Max(1, remainingDays),
            BlockingTreatmentName: primaryBlockingTreatment.ProductCommercialName,
            ActiveWithdrawalEndsAtUtc: primaryBlockingTreatment.WithdrawalEndDateUtc);
    }
}
