using FluentAssertions;
using Karakol.Application.Behaviors;
using Karakol.Shared.Results;
using MediatR;
using Microsoft.Extensions.Logging;
using Xunit;

namespace Karakol.Application.Tests;

public sealed class LoggingBehaviorTests
{
    [Fact]
    public async Task Handle_WhenRequestContainsSensitiveValues_DoesNotLogRequestPayload()
    {
        var logger = new CapturingLogger<LoggingBehavior<SensitiveRequest, Result<string>>>();
        var behavior = new LoggingBehavior<SensitiveRequest, Result<string>>(logger);

        await behavior.Handle(
            new SensitiveRequest("samples/nginx/nginx-access.log", "super-secret-token"),
            _ => Task.FromResult(Result<string>.Success("ok")),
            CancellationToken.None);

        logger.Messages.Should().Contain(message => message.Contains(nameof(SensitiveRequest), StringComparison.Ordinal));
        logger.Messages.Should().NotContain(message => message.Contains("super-secret-token", StringComparison.Ordinal));
        logger.Messages.Should().NotContain(message => message.Contains("samples/nginx/nginx-access.log", StringComparison.Ordinal));
    }

    [Fact]
    public async Task Handle_WhenNextThrows_LogsRequestNameWithoutSensitivePayload()
    {
        var logger = new CapturingLogger<LoggingBehavior<SensitiveRequest, Result<string>>>();
        var behavior = new LoggingBehavior<SensitiveRequest, Result<string>>(logger);

        var act = async () => await behavior.Handle(
            new SensitiveRequest("samples/nginx/nginx-access.log", "super-secret-token"),
            _ => throw new InvalidOperationException("boom"),
            CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>();
        logger.Messages.Should().Contain(message => message.Contains("failed", StringComparison.OrdinalIgnoreCase));
        logger.Messages.Should().NotContain(message => message.Contains("super-secret-token", StringComparison.Ordinal));
    }

    private sealed record SensitiveRequest(string FilePath, string ApiToken) : IRequest<Result<string>>;

    private sealed class CapturingLogger<T> : ILogger<T>
    {
        private readonly List<string> messages = [];

        public IReadOnlyCollection<string> Messages => messages;

        public IDisposable BeginScope<TState>(TState state)
            where TState : notnull
        {
            return NullScope.Instance;
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return true;
        }

        public void Log<TState>(
            LogLevel logLevel,
            EventId eventId,
            TState state,
            Exception? exception,
            Func<TState, Exception?, string> formatter)
        {
            messages.Add(formatter(state, exception));
        }
    }

    private sealed class NullScope : IDisposable
    {
        public static readonly NullScope Instance = new();

        public void Dispose()
        {
        }
    }
}
