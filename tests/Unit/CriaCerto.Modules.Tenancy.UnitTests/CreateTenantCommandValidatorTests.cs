using CriaCerto.Modules.Tenancy.Application.Features.CreateTenant;
using FluentAssertions;

namespace CriaCerto.Modules.Tenancy.UnitTests;

public class CreateTenantCommandValidatorTests
{
    private readonly CreateTenantCommandValidator _validator = new();

    [Fact]
    public void Validate_Should_Pass_When_Command_Is_Valid()
    {
        var command = new CreateTenantCommand(
            Guid.NewGuid(),
            "Fazenda Boi Gordo",
            "12345678000199",
            "MS",
            "Campo Grande",
            "IE999",
            800,
            "Starter",
            1000
        );

        var result = _validator.Validate(command);
        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("", "MS", 1000, "Starter")]
    [InlineData("Fazenda", "M", 1000, "Starter")]
    [InlineData("Fazenda", "MS", 0, "Starter")]
    [InlineData("Fazenda", "MS", 1000, "PlanoInvalido")]
    public void Validate_Should_Fail_When_Inputs_Are_Invalid(string name, string state, int capacity, string plan)
    {
        var command = new CreateTenantCommand(
            Guid.NewGuid(),
            name,
            "123",
            state,
            "Cidade",
            "IE",
            100,
            plan,
            capacity
        );

        var result = _validator.Validate(command);
        result.IsValid.Should().BeFalse();
    }
}
