using CriaCerto.Modules.Tenancy.Application.Features.ResetPassword;
using FluentAssertions;

namespace CriaCerto.Modules.Tenancy.UnitTests;

public class ResetPasswordCommandValidatorTests
{
    private readonly ResetPasswordCommandValidator _validator = new();

    [Fact]
    public void Validate_Should_Pass_When_Command_Is_Valid()
    {
        var command = new ResetPasswordCommand("produtor@fazenda.com.br", "123456", "Senha@123");
        var result = _validator.Validate(command);
        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("", "123456", "Senha@123")]
    [InlineData("email-invalido", "123456", "Senha@123")]
    [InlineData("produtor@fazenda.com.br", "", "Senha@123")]
    [InlineData("produtor@fazenda.com.br", "123456", "123")]
    [InlineData("produtor@fazenda.com.br", "123456", "somentesemmaiusculas1")]
    public void Validate_Should_Fail_When_Inputs_Are_Invalid(string email, string token, string newPassword)
    {
        var command = new ResetPasswordCommand(email, token, newPassword);
        var result = _validator.Validate(command);
        result.IsValid.Should().BeFalse();
    }
}
