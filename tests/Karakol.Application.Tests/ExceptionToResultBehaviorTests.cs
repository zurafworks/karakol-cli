using FluentAssertions;
using Karakol.Application.Behaviors;
using Karakol.Shared.Results;
using MediatR;
using Xunit;

namespace Karakol.Application.Tests;

public sealed class ExceptionToResultBehaviorTests
{
    [Fact]
    public async Task Handle_WhenNextThrowsUnexpectedException_ReturnsFailureResult()
    {
        var behavior = new ExceptionToResultBehavior<TestRequest, Result<string>>();

        var result = await behavior.Handle(
            new TestRequest(),
            _ => throw new InvalidOperationException("boom"),
            CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();
        result.Error!.Code.Should().Be("Unexpected");
        result.Error.Details.Should().Be("boom");
    }

    [Fact]
    public async Task Handle_WhenNextThrowsCancellation_RethrowsCancellation()
    {
        var behavior = new ExceptionToResultBehavior<TestRequest, Result<string>>();

        var act = async () => await behavior.Handle(
            new TestRequest(),
            _ => throw new OperationCanceledException("cancelled"),
            CancellationToken.None);

        await act.Should().ThrowAsync<OperationCanceledException>();
    }

    private sealed record TestRequest : IRequest<Result<string>>;
}
