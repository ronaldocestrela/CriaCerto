using CriaCerto.Modules.Tenancy.Application.Features.ForgotPassword;
using FluentAssertions;

namespace CriaCerto.Modules.Tenancy.UnitTests;

public class ForgotPasswordCommandValidatorTests
{
    private readonly ForgotPasswordCommandValidator _validator = new();

    [Fact]
    public void Validate_Should_Pass_When_Email_Is_Valid()
    {
        var command = new ForgotPasswordCommand("produtor@fazenda.com.br");
        var result = _validator.Validate(command);
        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("")]
    [InlineData("email-invalido")]
    [InlineData("   ")]
    public void Validate_Should_Fail_When_Email_Is_Invalid(string email)
    {
        var command = new ForgotPasswordCommand(email);
        var result = _validator.Validate(command);
        result.IsValid.Should().BeFalse();
    }
}
