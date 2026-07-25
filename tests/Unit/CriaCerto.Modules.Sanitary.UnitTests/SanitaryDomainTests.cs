using CriaCerto.BuildingBlocks.Abstractions.Results;
using CriaCerto.Modules.Sanitary.Application.Domain;
using CriaCerto.Modules.Sanitary.Application.Domain.Services;
using FluentAssertions;
using Xunit;

namespace CriaCerto.Modules.Sanitary.UnitTests;

public class SanitaryDomainTests
{
    [Fact]
    public void CreateVaccinationCampaign_WithValidDates_ShouldReturnSuccess()
    {
        // Arrange
        var startDate = DateTime.UtcNow.AddDays(-1);
        var endDate = DateTime.UtcNow.AddDays(30);

        // Act
        var result = VaccinationCampaign.Create(
            "Campanha Febre Aftosa 2026",
            CampaignType.Aftosa,
            startDate,
            endDate,
            "Vacinação obrigatória para todo o rebanho bovino.");

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Name.Should().Be("Campanha Febre Aftosa 2026");
        result.Value.Type.Should().Be(CampaignType.Aftosa);
        result.Value.IsActive.Should().BeTrue();
    }

    [Fact]
    public void CreateVaccinationCampaign_WithEndDateBeforeStartDate_ShouldReturnFailure()
    {
        // Arrange
        var startDate = DateTime.UtcNow.AddDays(10);
        var endDate = DateTime.UtcNow.AddDays(5);

        // Act
        var result = VaccinationCampaign.Create(
            "Campanha Inválida",
            CampaignType.Brucelose,
            startDate,
            endDate);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Sanitary.InvalidCampaignDates");
    }

    [Fact]
    public void ApplyTreatment_WithPositiveWithdrawalDays_ShouldCalculateWithdrawalEndDateCorrectly()
    {
        // Arrange
        var animalId = Guid.NewGuid();
        var applicationDate = new DateTime(2026, 7, 1, 10, 0, 0, DateTimeKind.Utc);
        int withdrawalDays = 28; // Carência de 28 dias para abate

        // Act
        var result = TreatmentRecord.Create(
            animalId,
            "Ivermectina 3.5%",
            TreatmentType.Deworming,
            "Lote A-102",
            "10ml SC",
            withdrawalDays,
            applicationDate,
            "Dr. Carlos Veterinário");

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.WithdrawalEndDateUtc.Should().Be(applicationDate.AddDays(28));
        result.Value.IsWithdrawalPeriodActive(new DateTime(2026, 7, 15, 0, 0, 0, DateTimeKind.Utc)).Should().BeTrue();
        result.Value.IsWithdrawalPeriodActive(new DateTime(2026, 8, 5, 0, 0, 0, DateTimeKind.Utc)).Should().BeFalse();
    }

    [Fact]
    public void ValidateSlaughterEligibility_WhenAnimalHasActiveWithdrawal_ShouldReturnSlaughterBlocked()
    {
        // Arrange
        var animalId = Guid.NewGuid();
        var applicationDate = DateTime.UtcNow.AddDays(-10);
        var treatment = TreatmentRecord.Create(
            animalId,
            "Antibiótico Ceftiofur",
            TreatmentType.Medication,
            "Lote C-55",
            "20ml IM",
            20, // Carência de 20 dias (ainda faltam 10 dias)
            applicationDate).Value;

        var treatments = new List<TreatmentRecord> { treatment };
        var checkDate = DateTime.UtcNow;

        // Act
        var eligibility = WithdrawalPeriodService.EvaluateSlaughterEligibility(animalId, treatments, checkDate);

        // Assert
        eligibility.IsEligibleForSlaughter.Should().BeFalse();
        eligibility.RemainingWithdrawalDays.Should().BeGreaterThan(0);
        eligibility.BlockingTreatmentName.Should().Be("Antibiótico Ceftiofur");
    }

    [Fact]
    public void ValidateSlaughterEligibility_WhenWithdrawalExpired_ShouldReturnEligible()
    {
        // Arrange
        var animalId = Guid.NewGuid();
        var applicationDate = DateTime.UtcNow.AddDays(-35);
        var treatment = TreatmentRecord.Create(
            animalId,
            "Vermífugo Abamectina",
            TreatmentType.Deworming,
            "Lote V-12",
            "5ml SC",
            30, // Carência de 30 dias (expirou há 5 dias)
            applicationDate).Value;

        var treatments = new List<TreatmentRecord> { treatment };
        var checkDate = DateTime.UtcNow;

        // Act
        var eligibility = WithdrawalPeriodService.EvaluateSlaughterEligibility(animalId, treatments, checkDate);

        // Assert
        eligibility.IsEligibleForSlaughter.Should().BeTrue();
        eligibility.RemainingWithdrawalDays.Should().Be(0);
        eligibility.BlockingTreatmentName.Should().BeNull();
    }
}
