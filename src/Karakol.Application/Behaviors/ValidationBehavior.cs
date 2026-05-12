using FluentValidation;
using Karakol.Shared.Errors;
using MediatR;

namespace Karakol.Application.Behaviors;

public sealed class ValidationBehavior<TRequest, TResponse>(IEnumerable<IValidator<TRequest>> validators)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var failures = new List<FluentValidation.Results.ValidationFailure>();
        foreach (var validator in validators)
        {
            var result = await validator.ValidateAsync(request, cancellationToken).ConfigureAwait(false);
            failures.AddRange(result.Errors.Where(error => error is not null));
        }

        if (failures.Count == 0)
        {
            return await next(cancellationToken).ConfigureAwait(false);
        }

        var first = failures[0];
        var errorType = first.ErrorCode == "Scan.FileNotFound" ? ErrorType.NotFound : ErrorType.Validation;
        var details = string.Join(Environment.NewLine, failures.Select(failure => $"{failure.PropertyName}: {failure.ErrorMessage}"));
        var error = new Error(first.ErrorCode, first.ErrorMessage, details, errorType);

        return ResultResponseFactory.Failure<TResponse>(error);
    }
}
