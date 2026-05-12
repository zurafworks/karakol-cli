using FluentAssertions;
using Karakol.Application.Commands.ScanLogFile;
using Karakol.Application.Configuration;
using Karakol.Application.Policies;
using Xunit;

namespace Karakol.Application.Tests;

public sealed class ConfigurationAndPolicyValidationTests
{
    [Fact]
    public async Task JsonScanConfigurationProvider_returns_controlled_error_for_unknown_report_format()
    {
        var configPath = WriteTempJson("""{"reporting":{"defaultOutputDirectory":"./reports","defaultFormats":["json","pdf"]}}""");

        var result = await new JsonScanConfigurationProvider().LoadAsync(configPath, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error!.Code.Should().Be("Config.InvalidReportFormat");
        result.Error.Details.Should().Contain("pdf");
    }

    [Fact]
    public async Task JsonScanConfigurationProvider_returns_controlled_error_for_invalid_output_directory()
    {
        var outputFile = Path.GetTempFileName();
        var configPath = WriteTempJson("{\"reporting\":{\"defaultOutputDirectory\":\"" + EscapeJson(outputFile) + "\",\"defaultFormats\":[\"json\"]}}");

        var result = await new JsonScanConfigurationProvider().LoadAsync(configPath, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error!.Code.Should().Be("Config.InvalidOutputDirectory");
    }

    [Fact]
    public async Task JsonPolicyProvider_returns_controlled_error_for_unknown_minimum_severity()
    {
        var policyPath = WriteTempJson("""{"minimumSeverity":"Urgent"}""");

        var result = await new JsonPolicyProvider().LoadAsync(policyPath, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error!.Code.Should().Be("Policy.InvalidSeverity");
        result.Error.Details.Should().Be("Urgent");
    }

    [Fact]
    public async Task JsonPolicyProvider_returns_controlled_error_for_invalid_json()
    {
        var policyPath = WriteTempJson("""{"minimumSeverity":""");

        var result = await new JsonPolicyProvider().LoadAsync(policyPath, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error!.Code.Should().Be("Policy.InvalidJson");
    }

    [Fact]
    public void ScanLogFileCommandValidator_rejects_unknown_severity_and_report_format()
    {
        var logFile = Path.GetTempFileName();
        var command = new ScanLogFileCommand(
            logFile,
            "nginx",
            "./reports",
            ["xml"],
            true,
            false,
            true,
            false,
            "Urgent",
            null,
            true,
            false,
            null,
            null);

        var result = new ScanLogFileCommandValidator().Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Select(error => error.ErrorCode).Should().Contain("Scan.InvalidMinimumSeverity");
        result.Errors.Select(error => error.ErrorCode).Should().Contain("Scan.InvalidReportFormat");
    }

    [Fact]
    public void ScanLogFileCommandValidator_rejects_output_directory_when_it_is_file()
    {
        var logFile = Path.GetTempFileName();
        var outputFile = Path.GetTempFileName();
        var command = new ScanLogFileCommand(
            logFile,
            "nginx",
            outputFile,
            ["json"],
            true,
            false,
            true,
            false,
            null,
            null,
            true,
            false,
            null,
            null);

        var result = new ScanLogFileCommandValidator().Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Select(error => error.ErrorCode).Should().Contain("Scan.InvalidOutputDirectory");
    }

    [Fact]
    public async Task Sample_config_and_policy_files_load_successfully()
    {
        var root = FindRepositoryRoot();
        var settings = await new JsonScanConfigurationProvider().LoadAsync(
            Path.Combine(root, "samples", "config", "karakolsettings.example.json"),
            CancellationToken.None);
        var duckDbSettings = await new JsonScanConfigurationProvider().LoadAsync(
            Path.Combine(root, "samples", "config", "karakolsettings.duckdb.json"),
            CancellationToken.None);
        var policy = await new JsonPolicyProvider().LoadAsync(
            Path.Combine(root, "samples", "config", "karakol.policy.example.json"),
            CancellationToken.None);

        settings.IsSuccess.Should().BeTrue();
        duckDbSettings.IsSuccess.Should().BeTrue();
        policy.IsSuccess.Should().BeTrue();
    }

    private static string WriteTempJson(string content)
    {
        var directory = Path.Combine(Path.GetTempPath(), "karakol-application-tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        var path = Path.Combine(directory, "input.json");
        File.WriteAllText(path, content);
        return path;
    }

    private static string EscapeJson(string value)
    {
        return value.Replace(@"\", @"\\", StringComparison.Ordinal).Replace("\"", "\\\"", StringComparison.Ordinal);
    }

    private static string FindRepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null)
        {
            if (File.Exists(Path.Combine(directory.FullName, "Karakol.sln")))
            {
                return directory.FullName;
            }

            directory = directory.Parent;
        }

        throw new DirectoryNotFoundException("Could not find repository root.");
    }
}
