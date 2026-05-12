using System.Diagnostics;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Karakol.Application.Behaviors;

public sealed class PerformanceBehavior<TRequest, TResponse>(
    ILogger<PerformanceBehavior<TRequest, TResponse>> logger) : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private static readonly TimeSpan WarningThreshold = TimeSpan.FromSeconds(5);

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var stopwatch = Stopwatch.StartNew();
        var response = await next(cancellationToken).ConfigureAwait(false);
        stopwatch.Stop();

        if (stopwatch.Elapsed > WarningThreshold)
        {
            logger.LogWarning(
                "Karakol request exceeded performance threshold: {RequestName} took {ElapsedMilliseconds} ms",
                typeof(TRequest).Name,
                stopwatch.ElapsedMilliseconds);
        }

        return response;
    }
}
