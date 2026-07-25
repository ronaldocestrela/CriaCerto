using CriaCerto.Modules.Growth.Application.Domain;
using FluentAssertions;
using Xunit;

namespace CriaCerto.Modules.Growth.UnitTests.Domain;

public class PasturePaddockTests
{
    [Fact]
    public void Create_WithValidParameters_ShouldReturnSuccess()
    {
        // Arrange
        var tenantId = Guid.NewGuid();

        // Act
        var result = PasturePaddock.Create("Piquete da Baixada", "PIQ-01", 15.5m, 20.0m, tenantId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Name.Should().Be("Piquete da Baixada");
        result.Value.Code.Should().Be("PIQ-01");
        result.Value.AreaHectares.Should().Be(15.5m);
        result.Value.MaxCapacityUA.Should().Be(20.0m);
        result.Value.Status.Should().Be(PaddockStatus.Active);
    }

    [Theory]
    [InlineData("", "PIQ-01", 10, 10)]
    [InlineData("Pasto 1", "", 10, 10)]
    [InlineData("Pasto 1", "PIQ-01", 0, 10)]
    [InlineData("Pasto 1", "PIQ-01", -5, 10)]
    [InlineData("Pasto 1", "PIQ-01", 10, 0)]
    [InlineData("Pasto 1", "PIQ-01", 10, -2)]
    public void Create_WithInvalidParameters_ShouldReturnFailure(string name, string code, decimal area, decimal capacity)
    {
        // Arrange
        var tenantId = Guid.NewGuid();

        // Act
        var result = PasturePaddock.Create(name, code, area, capacity, tenantId);

        // Assert
        result.IsFailure.Should().BeTrue();
    }
}
