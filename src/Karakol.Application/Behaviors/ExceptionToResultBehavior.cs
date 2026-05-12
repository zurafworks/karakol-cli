using Karakol.Shared.Errors;
using MediatR;

namespace Karakol.Application.Behaviors;

public sealed class ExceptionToResultBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        try
        {
            return await next(cancellationToken).ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            return ResultResponseFactory.Failure<TResponse>(
                new Error("Unexpected", "Unexpected error while processing the request.", ex.Message, ErrorType.Unexpected));
        }
    }
}
