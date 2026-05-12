using FluentValidation;
using Karakol.Application.Abstractions.Files;
using Karakol.Application.Behaviors;
using Karakol.Application.Commands.ScanLogFile;
using Karakol.Application.Configuration;
using Karakol.Application.Policies;
using Karakol.Contracts.Formats;
using Karakol.Cli.Logging;
using Karakol.Correlation.Engine;
using Karakol.Detection.Abstractions;
using Karakol.Detection.Engine;
using Karakol.Detection.Registry;
using Karakol.Domain.Enums;
using Karakol.Infrastructure.Files;
using Karakol.ML.Abstractions;
using Karakol.ML.Features;
using Karakol.ML.Onnx;
using Karakol.Parsing.Abstractions;
using Karakol.Parsing.Detection;
using Karakol.Parsing.Input;
using Karakol.Parsing.Registry;
using Karakol.Persistence.Abstractions;
using Karakol.Persistence.Disabled;
using Karakol.Persistence.DuckDb.Repository;
using Karakol.Plugins.Abstractions.Registry;
using Karakol.Plugins.BuiltIn.Catalog;
using Karakol.Reporting.Abstractions;
using Karakol.Reporting.Registry;
using Karakol.Reporting.Sanitization;
using Karakol.Risk.Scoring;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;

namespace Karakol.Cli.Composition;

public sealed class KarakolRuntime(IServiceProvider services)
{
    public static KarakolRuntime CreateDefault()
    {
        var serviceCollection = new ServiceCollection();
        ConfigureServices(serviceCollection, new KarakolLogLevelSwitch());
        return new KarakolRuntime(serviceCollection.BuildServiceProvider());
    }

    public static void ConfigureServices(IServiceCollection services, KarakolLogLevelSwitch logLevelSwitch)
    {
        ArgumentNullException.ThrowIfNull(logLevelSwitch);

        var loggerConfiguration = new LoggerConfiguration()
            .MinimumLevel.ControlledBy(logLevelSwitch.Switch)
            .Filter.ByExcluding(logEvent => logEvent.RenderMessage().Contains("Lucky Penny software MediatR", StringComparison.Ordinal))
            .Enrich.FromLogContext()
            .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}");

        var logFilePath = Environment.GetEnvironmentVariable("KARAKOL_LOG_FILE");
        if (!string.IsNullOrWhiteSpace(logFilePath))
        {
            loggerConfiguration.WriteTo.File(
                logFilePath,
                rollingInterval: RollingInterval.Day,
                outputTemplate: "{Timestamp:O} [{Level:u3}] {Message:lj}{NewLine}{Exception}");
        }

        Log.Logger = loggerConfiguration.CreateLogger();

        services.AddSingleton(logLevelSwitch);
        services.AddLogging(builder =>
        {
            builder.ClearProviders();
            builder.AddFilter("LuckyPennySoftware.MediatR.License", LogLevel.None);
            builder.AddSerilog(Log.Logger, dispose: false);
        });
        services.AddMediatR(configuration => configuration.RegisterServicesFromAssembly(typeof(ScanLogFileCommand).Assembly));
        services.AddSingleton<IValidator<ScanLogFileCommand>, ScanLogFileCommandValidator>();
        services.AddTransient<ScanLogFileCommandHandler>();
        services.AddSingleton<IScanConfigurationProvider, JsonScanConfigurationProvider>();
        services.AddSingleton<IPolicyProvider, JsonPolicyProvider>();

        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ExceptionToResultBehavior<,>));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(CancellationBehavior<,>));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(PerformanceBehavior<,>));

        var builtInPlugin = new KarakolBuiltInPlugin();
        var pluginRegistry = new PluginRegistry();
        pluginRegistry.Register((Karakol.Plugins.Abstractions.Parsers.IParserPlugin)builtInPlugin);
        pluginRegistry.Register((Karakol.Plugins.Abstractions.Detection.IDetectionPlugin)builtInPlugin);
        pluginRegistry.Register((Karakol.Plugins.Abstractions.Reporting.IReporterPlugin)builtInPlugin);
        pluginRegistry.Register((Karakol.Plugins.Abstractions.ML.IMlPlugin)builtInPlugin);
        services.AddSingleton(pluginRegistry);

        services.AddSingleton<ILogInputReader, PhysicalLogInputReader>();
        foreach (var parser in pluginRegistry.GetParsers())
        {
            services.AddSingleton(typeof(ILogParser), parser);
        }

        services.AddSingleton<IParserRegistry, ParserRegistry>();
        services.AddSingleton<ILogFormatDetector, DefaultLogFormatDetector>();
        services.AddSingleton<ILogSourceFactory, FileMetadataProvider>();

        foreach (var rule in pluginRegistry.GetRules())
        {
            services.AddSingleton(typeof(IDetectionRule), rule);
        }

        services.AddSingleton<IRuleRegistry, RuleRegistry>();
        services.AddSingleton<SingleEventRuleEngine>();
        services.AddSingleton<CorrelationEngine>();
        services.AddSingleton<DefaultRiskScoringService>();
        services.AddSingleton<Karakol.Risk.Abstractions.IRiskScoringService>(provider => provider.GetRequiredService<DefaultRiskScoringService>());

        services.AddSingleton<ISensitiveDataMasker, SensitiveDataMasker>();
        foreach (var writer in pluginRegistry.GetReportWriters())
        {
            services.AddSingleton(typeof(IReportWriter), writer);
        }

        services.AddSingleton<ReportWriterRegistry>();

        services.AddSingleton<IThreatClassifier>(provider => pluginRegistry.GetClassifiers().First());
        services.AddSingleton<IEventFeatureExtractor, DefaultEventFeatureExtractor>();
        services.AddSingleton<IOnnxThreatClassifierFactory, DisabledOnnxThreatClassifierFactory>();
        services.AddSingleton<IScanHistoryRepository, DisabledScanHistoryRepository>();
        services.AddSingleton<IScanHistoryRepositoryProvider, DisabledScanHistoryRepositoryProvider>();
        services.AddSingleton<IScanHistoryRepositoryProvider, DuckDbScanHistoryRepositoryProvider>();
    }

    public IServiceProvider Services { get; } = services;

    public IMediator Mediator => Services.GetRequiredService<IMediator>();

    public ScanLogFileCommandHandler ScanHandler => Services.GetRequiredService<ScanLogFileCommandHandler>();

    public IReadOnlyCollection<IDetectionRule> Rules => Services.GetRequiredService<IRuleRegistry>().GetRules();

    public IReadOnlyCollection<SupportedFormatDto> Formats => Services.GetRequiredService<IParserRegistry>()
        .GetParsers()
        .Select(parser => new SupportedFormatDto(parser.Format.ToString(), parser.FormatName, parser.Format != LogFormat.Generic))
        .ToArray();
}
