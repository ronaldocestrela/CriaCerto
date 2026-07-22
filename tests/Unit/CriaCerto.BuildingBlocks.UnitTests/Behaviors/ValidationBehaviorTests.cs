using CriaCerto.BuildingBlocks.Abstractions.Results;
using CriaCerto.BuildingBlocks.Application.Behaviors;
using FluentAssertions;
using FluentValidation;
using MediatR;

namespace CriaCerto.BuildingBlocks.UnitTests.Behaviors;

public class ValidationBehaviorTests
{
    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenValidationFails()
    {
        var validators = new List<IValidator<CreateSowCommand>>
        {
            new CreateSowCommandValidator()
        };

        var behavior = new ValidationBehavior<CreateSowCommand, Result<Guid>>(validators);
        var command = new CreateSowCommand(string.Empty);

        var result = await behavior.Handle(
            command,
            () => Task.FromResult(Result.Success(Guid.NewGuid())),
            CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Type.Should().Be(ErrorType.Validation);
    }

    [Fact]
    public async Task Handle_ShouldCallNext_WhenValidationPasses()
    {
        var validators = new List<IValidator<CreateSowCommand>>
        {
            new CreateSowCommandValidator()
        };

        var behavior = new ValidationBehavior<CreateSowCommand, Result<Guid>>(validators);
        var command = new CreateSowCommand("SOW-001");
        var expected = Guid.NewGuid();

        var result = await behavior.Handle(
            command,
            () => Task.FromResult(Result.Success(expected)),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(expected);
    }

    private sealed record CreateSowCommand(string Identifier) : IRequest<Result<Guid>>;

    private sealed class CreateSowCommandValidator : AbstractValidator<CreateSowCommand>
    {
        public CreateSowCommandValidator()
        {
            RuleFor(command => command.Identifier).NotEmpty();
        }
    }
}