using CriaCerto.BuildingBlocks.Abstractions.Results;
using FluentAssertions;

namespace CriaCerto.BuildingBlocks.UnitTests.Results;

public class ResultTests
{
    [Fact]
    public void Success_Should_CreateSuccessfulResult()
    {
        var result = Result.Success();

        result.IsSuccess.Should().BeTrue();
        result.IsFailure.Should().BeFalse();
        result.Error.Should().Be(Error.None);
    }

    [Fact]
    public void Failure_Should_CreateFailureResult()
    {
        var error = Error.Validation("Validation.Required", "Name is required.");
        var result = Result.Failure(error);

        result.IsSuccess.Should().BeFalse();
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(error);
    }

    [Fact]
    public void GenericFailure_ValueAccess_ShouldThrow()
    {
        var result = Result.Failure<Guid>(Error.Conflict("Sow.State", "Sow is already pregnant."));

        var access = () => _ = result.Value;

        access.Should().Throw<InvalidOperationException>();
    }
}