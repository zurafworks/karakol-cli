using System.Text.Json;
using Karakol.Application.Validation;
using Karakol.Shared.Errors;
using Karakol.Shared.Results;

namespace Karakol.Application.Policies;

public sealed class JsonPolicyProvider : IPolicyProvider
{
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNameCaseInsensitive = true
    };

    public async Task<Result<PolicyContext>> LoadAsync(string? policyFile, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(policyFile))
        {
            return Result<PolicyContext>.Success(PolicyContext.Default);
        }

        try
        {
            await using var stream = File.OpenRead(policyFile);
            var policy = await JsonSerializer.DeserializeAsync<PolicyContext>(stream, SerializerOptions, cancellationToken).ConfigureAwait(false);
            return Validate(policy ?? PolicyContext.Default);
        }
        catch (JsonException ex)
        {
            return Result<PolicyContext>.Failure(new Error("Policy.InvalidJson", "Policy file is not valid JSON.", ex.Message, ErrorType.Configuration));
        }
        catch (IOException ex)
        {
            return Result<PolicyContext>.Failure(new Error("Policy.ReadFailed", "Policy file could not be read.", ex.Message, ErrorType.Configuration));
        }
        catch (UnauthorizedAccessException ex)
        {
            return Result<PolicyContext>.Failure(new Error("Policy.Unauthorized", "Policy file could not be accessed.", ex.Message, ErrorType.Unauthorized));
        }
    }

    private static Result<PolicyContext> Validate(PolicyContext policy)
    {
        if (!KarakolInputValues.IsKnownSeverity(policy.MinimumSeverity))
        {
            return Result<PolicyContext>.Failure(new Error(
                "Policy.InvalidSeverity",
                "Policy minimumSeverity must be one of: Info, Low, Medium, High, Critical.",
                policy.MinimumSeverity,
                ErrorType.Configuration));
        }

        return Result<PolicyContext>.Success(policy);
    }
}
