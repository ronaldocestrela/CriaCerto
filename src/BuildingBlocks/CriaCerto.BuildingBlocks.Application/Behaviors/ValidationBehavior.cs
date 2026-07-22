using CriaCerto.BuildingBlocks.Abstractions.Results;
using FluentValidation;
using MediatR;

namespace CriaCerto.BuildingBlocks.Application.Behaviors;

public sealed class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
    where TResponse : Result
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;

    public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
    {
        _validators = validators;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (!_validators.Any())
        {
            return await next();
        }

        var context = new ValidationContext<TRequest>(request);
        var failures = (await Task.WhenAll(_validators.Select(v => v.ValidateAsync(context, cancellationToken))))
            .SelectMany(r => r.Errors)
            .Where(f => f is not null)
            .ToArray();

        if (failures.Length == 0)
        {
            return await next();
        }

        var errorMessage = string.Join("; ", failures.Select(f => $"{f.PropertyName}: {f.ErrorMessage}"));
        var error = Error.Validation("Validation.Failure", errorMessage);

        return CreateFailureResponse(error);
    }

    private static TResponse CreateFailureResponse(Error error)
    {
        if (typeof(TResponse) == typeof(Result))
        {
            return (TResponse)(object)Result.Failure(error);
        }

        if (typeof(TResponse).IsGenericType &&
            typeof(TResponse).GetGenericTypeDefinition() == typeof(Result<>))
        {
            var valueType = typeof(TResponse).GenericTypeArguments[0];
            var genericMethod = typeof(Result)
                .GetMethods()
                .Single(method => method.Name == nameof(Result.Failure) && method.IsGenericMethod)
                .MakeGenericMethod(valueType);

            return (TResponse)genericMethod.Invoke(null, new object[] { error })!;
        }

        throw new InvalidOperationException($"ValidationBehavior does not support response type {typeof(TResponse).Name}.");
    }
}