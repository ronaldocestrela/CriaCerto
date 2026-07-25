using CriaCerto.BuildingBlocks.Abstractions.Results;
using CriaCerto.Modules.Breeding.Application.Domain;
using FluentAssertions;
using Xunit;

namespace CriaCerto.Modules.Breeding.UnitTests.Domain;

public class CowTests
{
    private readonly Guid _tenantId = Guid.NewGuid();

    [Fact]
    public void Create_WithValidData_ShouldReturnSuccess()
    {
        var result = Cow.Create("BR-101", "Nelore", DateTime.UtcNow.AddYears(-3), _tenantId, sisbovId: "123456789");

        result.IsSuccess.Should().BeTrue();
        result.Value.EarTag.Should().Be("BR-101");
        result.Value.Status.Should().Be(ReproductiveStatus.Open);
        result.Value.ParityCount.Should().Be(0);
    }

    [Fact]
    public void Create_WithoutEarTag_ShouldReturnFailure()
    {
        var result = Cow.Create("", "Nelore", DateTime.UtcNow.AddYears(-3), _tenantId);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Cow.EarTagRequired");
    }

    [Fact]
    public void StartIatfProtocol_WhenOpen_ShouldChangeStatusToInIatfProtocol()
    {
        var cow = Cow.Create("BR-102", "Angus", DateTime.UtcNow.AddYears(-4), _tenantId).Value;

        var result = cow.StartIatfProtocol(Guid.NewGuid());

        result.IsSuccess.Should().BeTrue();
        cow.Status.Should().Be(ReproductiveStatus.InIatfProtocol);
    }

    [Fact]
    public void RecordPregnancyDiagnosis_WhenConfirmedPregnant_ShouldSetStatusToPregnant()
    {
        var cow = Cow.Create("BR-103", "Nelore", DateTime.UtcNow.AddYears(-3), _tenantId).Value;

        var result = cow.RecordPregnancyDiagnosis(true, DateTime.UtcNow);

        result.IsSuccess.Should().BeTrue();
        cow.Status.Should().Be(ReproductiveStatus.Pregnant);
    }

    [Fact]
    public void RecordCalving_ShouldIncrementParityAndSetLastCalvingDate()
    {
        var cow = Cow.Create("BR-104", "Gyr", DateTime.UtcNow.AddYears(-5), _tenantId).Value;
        var calvingDate = DateTime.UtcNow.AddDays(-2);

        var result = cow.RecordCalving(calvingDate);

        result.IsSuccess.Should().BeTrue();
        cow.ParityCount.Should().Be(1);
        cow.LastCalvingDate.Should().Be(calvingDate);
        cow.Status.Should().Be(ReproductiveStatus.Open);
    }
}
