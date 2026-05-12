using FluentAssertions;
using Karakol.Domain.Enums;
using Karakol.Domain.Events;
using Karakol.Domain.Sources;
using Karakol.ML.Features;
using Karakol.ML.Onnx;

namespace Karakol.ML.Tests;

public sealed class MlPreparationTests
{
    [Fact]
    public void Feature_extractor_builds_normalized_text_and_numeric_features()
    {
        var securityEvent = new SecurityEvent(SecurityEventId.New(), LogSourceId.New(), LogFormat.NginxAccess, "GET /login?id=1", 1)
        {
            HttpMethod = "GET",
            Path = "/login",
            QueryString = "id=1",
            StatusCode = 401,
            ResponseSize = 128,
            UserAgent = "Mozilla/5.0"
        };

        var input = new DefaultEventFeatureExtractor().Extract(securityEvent);

        input.NormalizedText.Should().Contain("/login");
        input.NumericFeatures["statusCode"].Should().Be(401);
        input.NumericFeatures["hasQuery"].Should().Be(1);
    }

    [Fact]
    public void Disabled_onnx_factory_does_not_create_classifier_for_missing_model()
    {
        var factory = new DisabledOnnxThreatClassifierFactory();

        factory.CanCreate("missing.onnx", null).Should().BeFalse();
    }
}
