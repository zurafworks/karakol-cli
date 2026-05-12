using MediatR;
using Microsoft.Extensions.Logging;

namespace Karakol.Application.Behaviors;

public sealed class LoggingBehavior<TRequest, TResponse>(
    ILogger<LoggingBehavior<TRequest, TResponse>> logger) : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;
        logger.LogInformation("Karakol request started: {RequestName}", requestName);

        try
        {
            var response = await next(cancellationToken).ConfigureAwait(false);
            logger.LogInformation("Karakol request completed: {RequestName}", requestName);
            return response;
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            logger.LogError(exception, "Karakol request failed: {RequestName}", requestName);
            throw;
        }
    }
}
