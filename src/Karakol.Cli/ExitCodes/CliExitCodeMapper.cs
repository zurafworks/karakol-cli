using Karakol.Shared.Errors;

namespace Karakol.Cli.ExitCodes;

public static class CliExitCodeMapper
{
    public static int FromError(Error? error)
    {
        return error?.Type switch
        {
            ErrorType.Validation or ErrorType.Configuration => 2,
            ErrorType.NotFound => 3,
            ErrorType.Parsing => 4,
            _ => 5
        };
    }
}
