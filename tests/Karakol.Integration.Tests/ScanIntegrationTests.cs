using System.Diagnostics;
using FluentAssertions;
using Karakol.Cli.Composition;
using Karakol.Domain.Enums;
using MediatR;

namespace Karakol.Integration.Tests;

public sealed class ScanIntegrationTests
{
    [Fact]
    public async Task Runtime_scan_detects_expected_nginx_categories_and_writes_reports()
    {
        var output = CreateTempDirectory();
        var command = new Karakol.Application.Commands.ScanLogFile.ScanLogFileCommand(
            SamplePath("samples", "nginx", "nginx-access.log"),
            "nginx",
            output,
            ["json", "html"],
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

        var result = await KarakolRuntime.CreateDefault().Mediator.Send(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value!.Report.Summary.CategoryCounts.Should().ContainKey(ThreatCategory.SqlInjection);
        result.Value.Report.Summary.CategoryCounts.Should().ContainKey(ThreatCategory.Xss);
        result.Value.Report.Summary.CategoryCounts.Should().ContainKey(ThreatCategory.PathTraversal);
        result.Value.Report.Summary.CategoryCounts.Should().ContainKey(ThreatCategory.ScannerBot);
        result.Value.Report.Summary.CategoryCounts.Should().ContainKey(ThreatCategory.SensitiveFileAccess);
        result.Value.ReportFiles.Should().Contain(file => file.Format == "json" && File.Exists(file.FilePath));
        result.Value.ReportFiles.Should().Contain(file => file.Format == "html" && File.Exists(file.FilePath));
    }

    [Fact]
    public async Task Runtime_scan_returns_not_found_for_missing_file()
    {
        var command = new Karakol.Application.Commands.ScanLogFile.ScanLogFileCommand("missing.log", "nginx", CreateTempDirectory(), ["json"], true, false, true, false, null, null, true, false, null, null);

        var result = await KarakolRuntime.CreateDefault().Mediator.Send(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error!.Code.Should().Be("Scan.FileNotFound");
    }

    [Fact]
    public async Task Runtime_scan_honors_rules_disabled()
    {
        var command = new Karakol.Application.Commands.ScanLogFile.ScanLogFileCommand(SamplePath("samples", "nginx", "nginx-access.log"), "nginx", CreateTempDirectory(), ["json"], false, false, false, false, null, null, true, false, null, null);

        var result = await KarakolRuntime.CreateDefault().Mediator.Send(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value!.Report.Summary.FindingsCount.Should().Be(0);
    }

    [Fact]
    public async Task Runtime_scan_uses_config_defaults_when_cli_omits_values()
    {
        var output = CreateTempDirectory();
        var configPath = Path.Combine(CreateTempDirectory(), "karakolsettings.json");
        await File.WriteAllTextAsync(configPath, $$"""
{
  "Scanning": {
    "MaxLines": 1
  },
  "Reporting": {
    "DefaultOutputDirectory": "{{JsonPath(output)}}",
    "DefaultFormats": ["json"]
  }
}
""");
        var command = new Karakol.Application.Commands.ScanLogFile.ScanLogFileCommand(SamplePath("samples", "nginx", "nginx-access.log"), "nginx", "./reports", [], true, false, true, false, null, null, true, false, configPath, null);

        var result = await KarakolRuntime.CreateDefault().Mediator.Send(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue(result.Error?.Message);
        result.Value!.Report.Summary.TotalLines.Should().Be(1);
        result.Value.ReportFiles.Should().ContainSingle(file => file.Format == "json" && file.FilePath.StartsWith(output, StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task Runtime_scan_applies_policy_disabled_rules_and_ignored_paths()
    {
        var policyPath = Path.Combine(CreateTempDirectory(), "karakol.policy.json");
        await File.WriteAllTextAsync(policyPath, """
{
  "disabledRules": ["sqli.basic"],
  "ignoredPaths": ["/wp-admin"]
}
""");
        var command = new Karakol.Application.Commands.ScanLogFile.ScanLogFileCommand(SamplePath("samples", "nginx", "nginx-access.log"), "nginx", CreateTempDirectory(), ["json"], true, false, true, false, null, null, true, false, null, policyPath);

        var result = await KarakolRuntime.CreateDefault().Mediator.Send(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue(result.Error?.Message);
        result.Value!.Report.Summary.CategoryCounts.Should().NotContainKey(ThreatCategory.SqlInjection);
        result.Value.Report.Findings.Should().NotContain(finding => finding.TargetResource == "/wp-admin");
    }

    [Fact]
    public async Task Runtime_scan_applies_policy_minimum_severity()
    {
        var policyPath = Path.Combine(CreateTempDirectory(), "karakol.policy.json");
        await File.WriteAllTextAsync(policyPath, """
{
  "minimumSeverity": "High"
}
""");
        var command = new Karakol.Application.Commands.ScanLogFile.ScanLogFileCommand(SamplePath("samples", "nginx", "nginx-access.log"), "nginx", CreateTempDirectory(), ["json"], true, false, true, false, null, null, true, false, null, policyPath);

        var result = await KarakolRuntime.CreateDefault().Mediator.Send(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue(result.Error?.Message);
        result.Value!.Report.Findings.Should().OnlyContain(finding => finding.Severity == Severity.High || finding.Severity == Severity.Critical);
    }

    [Fact]
    public async Task Runtime_scan_records_auto_detection_and_parser_statistics_metadata()
    {
        var command = new Karakol.Application.Commands.ScanLogFile.ScanLogFileCommand(SamplePath("samples", "nginx", "nginx-access.log"), "auto", CreateTempDirectory(), ["json"], true, false, true, false, null, null, true, false, null, null);

        var result = await KarakolRuntime.CreateDefault().Mediator.Send(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue(result.Error?.Message);
        result.Value!.Report.Metadata.Should().ContainKey("formatDetectionConfidence");
        result.Value.Report.Metadata["formatDetectionConfidence"].Should().NotBe("manual");
        result.Value.Report.Metadata.Should().ContainKey("parserFailureRatio");
    }

    [Fact]
    public async Task Cli_process_scan_returns_success_exit_code()
    {
        var output = CreateTempDirectory();
        var result = await RunDotnetAsync(CliDllPath(), "scan", SamplePath("samples", "nginx", "nginx-access.log"), "--format", "nginx", "--report", "json", "--output", output);

        result.ExitCode.Should().Be(0, result.StdErr);
        result.StdOut.Should().Contain("Overall Risk");
        Directory.GetFiles(output, "*.json").Should().ContainSingle();
    }

    [Fact]
    public async Task Cli_process_quiet_suppresses_summary_but_creates_report()
    {
        var output = CreateTempDirectory();
        var result = await RunDotnetAsync(CliDllPath(), "scan", SamplePath("samples", "nginx", "nginx-access.log"), "--format", "nginx", "--report", "json", "--output", output, "--quiet");

        result.ExitCode.Should().Be(0, result.StdErr);
        result.StdOut.Should().BeNullOrWhiteSpace();
        Directory.GetFiles(output, "*.json").Should().ContainSingle();
    }

    [Fact]
    public async Task Cli_process_verbose_prints_metadata()
    {
        var output = CreateTempDirectory();
        var result = await RunDotnetAsync(CliDllPath(), "scan", SamplePath("samples", "nginx", "nginx-access.log"), "--format", "auto", "--report", "json", "--output", output, "--verbose");

        result.ExitCode.Should().Be(0, result.StdErr);
        result.StdOut.Should().Contain("formatDetectionConfidence");
    }

    [Fact]
    public async Task Cli_process_missing_file_returns_file_read_exit_code()
    {
        var result = await RunDotnetAsync(CliDllPath(), "scan", "missing.log", "--format", "nginx");

        result.ExitCode.Should().Be(3);
        result.StdErr.Should().Contain("Scan.FileNotFound");
    }

    private static string CreateTempDirectory()
    {
        var path = Path.Combine(Path.GetTempPath(), "karakol-integration-tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(path);
        return path;
    }

    private static string SamplePath(params string[] segments)
    {
        return Path.Combine(new[] { RepoRoot() }.Concat(segments).ToArray());
    }

    private static string JsonPath(string path)
    {
        return path.Replace("\\", "\\\\", StringComparison.Ordinal);
    }

    private static string RepoRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "Karakol.sln")))
        {
            directory = directory.Parent;
        }

        directory.Should().NotBeNull("integration tests need to locate the repository root");
        return directory!.FullName;
    }

    private static async Task<(int ExitCode, string StdOut, string StdErr)> RunDotnetAsync(params string[] arguments)
    {
        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(30));
        var startInfo = new ProcessStartInfo("dotnet")
        {
            WorkingDirectory = RepoRoot(),
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false
        };

        foreach (var argument in arguments)
        {
            startInfo.ArgumentList.Add(argument);
        }

        using var process = Process.Start(startInfo);
        process.Should().NotBeNull();
        var stdoutTask = process!.StandardOutput.ReadToEndAsync();
        var stderrTask = process.StandardError.ReadToEndAsync();
        try
        {
            await process.WaitForExitAsync(timeout.Token);
        }
        catch (OperationCanceledException)
        {
            process.Kill(entireProcessTree: true);
            throw new TimeoutException("CLI process timed out.");
        }

        var stdout = await stdoutTask;
        var stderr = await stderrTask;
        return (process.ExitCode, stdout, stderr);
    }

    private static string CliDllPath()
    {
        return Path.Combine(RepoRoot(), "src", "Karakol.Cli", "bin", "Debug", "net9.0", "Karakol.Cli.dll");
    }
}
