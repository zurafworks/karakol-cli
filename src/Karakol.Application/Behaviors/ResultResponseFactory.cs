using Karakol.Shared.Errors;
using Karakol.Shared.Results;

namespace Karakol.Application.Behaviors;

internal static class ResultResponseFactory
{
    public static TResponse Failure<TResponse>(Error error)
    {
        var responseType = typeof(TResponse);
        if (!responseType.IsGenericType || responseType.GetGenericTypeDefinition() != typeof(Result<>))
        {
            throw new InvalidOperationException($"Response type '{responseType.Name}' is not supported by Karakol result behaviors.");
        }

        var valueType = responseType.GetGenericArguments()[0];
        var resultType = typeof(Result<>).MakeGenericType(valueType);
        var method = resultType.GetMethod(nameof(Result<object>.Failure), [typeof(Error)])
            ?? throw new InvalidOperationException("Result failure factory could not be found.");

        return (TResponse)method.Invoke(null, [error])!;
    }
}
