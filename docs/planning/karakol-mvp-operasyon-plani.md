# Karakol MVP Operasyon ve Mimari Planı

Bu dokuman Karakol projesinin amacini, urun isterlerini, beklenen ciktilarini, mimari kararlarini, klasor yapisini, proje sorumluluklarini, uygulama sirasini, test stratejisini ve kabul kriterlerini tanimlar.

Bu dosya README yerine gecmez. README kullaniciya ve katilimciya yonelik kisa giris dokumani olacak; bu dokuman ise uygulama ekibine verilebilecek teknik operasyon planidir.

## Ürün Amacı

Karakol, sunucu, uygulama ve guvenlik loglarini local ortamda analiz eden; saldiri izlerini, supheli aktiviteleri, brute force denemelerini, SQL injection, XSS, path traversal, scanner bot aktivitelerini, hassas dosya erisimlerini, supheli user-agent'lari, credential stuffing ve DoS pattern'lerini tespit eden; risk skoru ureten; terminal, JSON, Markdown ve HTML raporlari olusturan local-first bir siber guvenlik analiz aracidir.

Ilk urun formu CLI olacaktir. CLI, is kurallarini barindirmayan ince bir adapter olarak calisacak ve butun analiz akisi Application katmani uzerinden orkestre edilecektir.

Karakol'un uzun vadeli hedefi, ayni cekirdek analiz motorunu farkli arayuzlerle kullanabilmektir:

- CLI tool
- Background agent
- Watch mode
- Desktop app
- Local web dashboard
- GitHub Action veya CI tool
- VS Code extension
- Self-hosted enterprise runner
- SIEM entegrasyon adaptorleri

MVP'nin ana hedefi, local bir log dosyasini dis servise gondermeden analiz etmek ve anlasilir rapor uretmektir.

Ilk calistirilabilir hedef:

```bash
dotnet run --project src/Karakol.Cli -- scan samples/nginx/nginx-access.log --format nginx --report html --report json
```

Bu komut su davranislari gostermelidir:

- `samples/nginx/nginx-access.log` dosyasini streaming olarak okumali.
- Nginx access log formatini parse etmeli.
- SQLi, XSS, path traversal, scanner bot ve sensitive file denemelerini yakalamali.
- Finding bazli ve scan bazli risk skoru uretmeli.
- Terminalde okunabilir bir summary gostermeli.
- `reports/` altinda JSON rapor uretmeli.
- `reports/` altinda offline acilabilen HTML rapor uretmeli.
- Log icerigini hicbir external API'ye, cloud servisine veya telemetry endpoint'ine gondermemeli.

## Temel Vaat

Karakol'un temel vaadi sudur:

> Karakol, log dosyalarini disari gondermeden local olarak analiz eder, saldiri izlerini bulur ve anlasilir risk raporu uretir.

Bu vaat mimaride asagidaki prensiplerle korunacaktir:

- Local-first calisma modeli.
- API key gerektirmeyen MVP.
- Dis network bagimliligi olmayan scan akisi.
- Log verisinin disk ve proses disina cikarilmamasi.
- Offline rapor uretimi.
- Varsayilan olarak hassas veri maskeleme.
- Framework bagimsiz Domain katmani.
- CLI disinda yeniden kullanilabilir Application cekirdegi.
- Streaming log isleme.
- Plugin ve interface tabanli genisletilebilirlik.

## MVP Beklenen Çıktı

MVP sonunda asagidaki ciktilar hazir olacaktir:

- Calisan `.NET 9` solution.
- Clean Architecture proje yapisi.
- Modular Monolith modulleri.
- CQRS ve MediatR altyapisi.
- FluentValidation validator'lari.
- MediatR pipeline behavior'lari.
- Result Pattern.
- CLI `scan` komutu.
- Nginx parser.
- Apache parser.
- SSH/Auth parser.
- JSON parser.
- CSV parser.
- Generic fallback parser.
- Format auto-detection.
- Rule engine.
- SQL injection rule.
- XSS rule.
- Path traversal rule.
- Scanner bot rule.
- Sensitive file access rule.
- Suspicious user-agent rule.
- Command injection rule.
- Brute force correlation rule.
- Basic DoS correlation rule.
- Risk scoring service.
- Terminal summary.
- JSON report writer.
- HTML report writer.
- Markdown report writer.
- Config loading.
- Basic policy loading.
- Sensitive data masking.
- Sample logs.
- Unit tests.
- Integration tests.
- README.
- `docs/architecture/architecture.md`.
- `docs/planning/karakol-mvp-operasyon-plani.md`.

MVP sonunda asagidaki komut basarili calismalidir:

```bash
dotnet run --project src/Karakol.Cli -- scan samples/nginx/nginx-access.log --format nginx --report html --report json
```

Beklenen terminal davranisi:

- Kaynak dosya gosterilir.
- Secilen veya tespit edilen format gosterilir.
- Analiz edilen satir sayisi gosterilir.
- Parse edilen event sayisi gosterilir.
- Failed line sayisi gosterilir.
- Finding sayilari severity bazinda gosterilir.
- Top threat category bilgisi gosterilir.
- Overall risk score gosterilir.
- Uretilen JSON ve HTML rapor path'leri gosterilir.

## Ana İsterler

### Fonksiyonel İsterler

- Kullanici tek bir log dosyasini CLI uzerinden analiz edebilmelidir.
- Kullanici output formatlarini birden fazla kez secebilmelidir.
- Kullanici parser formatini explicit verebilmeli veya auto-detect kullanabilmelidir.
- Sistem Nginx, Apache, SSH/Auth, JSON, CSV ve Generic log formatlarini desteklemelidir.
- Sistem single-event rule detection calistirmalidir.
- Sistem bounded correlation calistirmalidir.
- Sistem risk skoru uretmelidir.
- Sistem JSON, Markdown ve HTML rapor uretmelidir.
- Sistem config dosyasi okuyabilmelidir.
- Sistem policy dosyasi okuyabilmelidir.
- Sistem sensitive data masking uygulamalidir.
- Sistem test sample loglariyla dogrulanabilmelidir.

### Non-Functional İsterler

- Buyuk log dosyalari memory'ye tamamen alinmamalidir.
- Streaming processing temel davranis olmalidir.
- CancellationToken desteklenmelidir.
- Parser, rule, reporter ve ML provider eklemek kolay olmalidir.
- Domain katmani framework bagimsiz olmalidir.
- CLI katmani is kurali icermemelidir.
- Rapor uretimi offline calismalidir.
- HTML rapor XSS'e karsi encode edilmelidir.
- Varsayilan olarak sensitive data masking aktif olmalidir.
- Hatalar kontrollu `Result<T>` uzerinden donmelidir.
- Log verisi external servislere gonderilmemelidir.

### Güvenlik İsterleri

- Raw log degerleri HTML output icinde encode edilmelidir.
- Query string icindeki `password`, `token`, `session`, `apikey`, `authorization` benzeri alanlar maskelenmelidir.
- Bearer token, JWT, API key benzeri uzun secret degerleri maskelenmelidir.
- File path normalize edilmelidir.
- Output path misuse riskleri minimize edilmelidir.
- Config ve policy path'leri kontrollu okunmalidir.
- Log scanning sirasinda external network cagrisi yapilmamalidir.

### Genişletilebilirlik İsterleri

- Yeni parser eklemek icin `ILogParser` implementasyonu ve registry registration yeterli olmalidir.
- Yeni detection rule eklemek icin `IDetectionRule` implementasyonu yeterli olmalidir.
- Yeni correlation rule eklemek icin correlation abstraction kullanilmalidir.
- Yeni reporter eklemek icin `IReportWriter` implementasyonu yeterli olmalidir.
- Yeni ML provider eklemek icin `IThreatClassifier` implementasyonu yeterli olmalidir.
- Built-in plugin catalog, MVP'de external plugin loading olmadan genisletilebilirlik zemini saglamalidir.

## Non-Goals

MVP'de asagidakiler hedef disidir:

- Cloud sync.
- External API entegrasyonu.
- Telemetry.
- Kullanici loglarini remote servise gonderme.
- Gercek ONNX model zorunlulugu.
- SQLite scan history implementation zorunlulugu.
- Watch mode runtime.
- Desktop app.
- Local web dashboard.
- GitHub Action packaging.
- VS Code extension.
- SIEM adapter implementation.
- External dynamic plugin loading.
- Enterprise runner.
- Rule marketplace.
- Online model download.

Bu non-goal maddeleri, mimaride extension point olarak dusunulebilir fakat MVP teslimat bloklayicisi olmayacaktir.

## Teknoloji Stack

### Ana Platform

- .NET 9
- C#
- Nullable reference types enabled
- TreatWarningsAsErrors enabled
- Central package management
- `global.json` ile SDK pinning

### Application ve Mimari

- Clean Architecture
- Modular Monolith
- CQRS
- MediatR
- FluentValidation
- Result Pattern
- Pipeline Pattern
- Options Pattern
- Dependency Injection
- Domain-Driven Design tactical patterns

### CLI

- Spectre.Console
- Spectre.Console.Cli
- Rich terminal tables
- Panels
- Progress indicators
- Verbose ve quiet output modes

### Logging ve Config

- Serilog
- Microsoft.Extensions.Configuration
- Microsoft.Extensions.Options
- JSON configuration

### Data ve Report

- System.Text.Json
- CsvHelper
- Custom Markdown writer
- Inline CSS HTML writer
- Optional template abstraction

### ML Hazırlığı

- Dummy classifier MVP.
- ONNX Runtime icin abstraction.
- ML.NET alternatif olarak dokumante edilebilir.
- Python training klasor yapisi hazir tutulur.

### Testing

- xUnit
- FluentAssertions
- NSubstitute veya Moq
- Snapshot test opsiyonel.
- BenchmarkDotNet performans smoke testleri icin opsiyonel.

## Mimari Yaklaşım

Karakol, Clean Architecture ve Modular Monolith yaklasimlarini birlikte kullanacaktir.

Clean Architecture, ic katmanlarin dis teknoloji bagimliliklarindan korunmasini saglayacaktir. Domain katmani saf is kavramlarini barindiracak ve framework bagimsiz kalacaktir. Application katmani use-case orkestrasyonu yapacak, dis dunya ile iletisim Infrastructure ve CLI gibi adapter katmanlari uzerinden gerceklesecektir.

Modular Monolith, tek solution ve tek deployable CLI icinde parser, detection, correlation, risk, reporting, ML ve persistence gibi modulleri net sinirlarla ayiracaktir. Bu sinirlar ileride background agent, dashboard veya CI runner gibi arayuzlere gecisi kolaylastiracaktir.

### Mimari Prensipler

- Domain first.
- Local-first.
- Streaming-first.
- Interface-driven extensibility.
- Testability first.
- CLI as adapter.
- Report output as sanitized projection.
- Configuration and policy as optional overlays.
- Plugin architecture as internal extensibility in MVP.

### Yüksek Seviyeli Veri Akışı

```text
Log File
  -> Input Reader
  -> Format Detector
  -> Parser Resolver
  -> Streaming Parser
  -> SecurityEvent
  -> Preprocessing
  -> Rule Detection
  -> Optional ML Classification
  -> Correlation
  -> Risk Scoring
  -> Aggregation
  -> Report Generation
  -> Terminal / JSON / Markdown / HTML
```

### Runtime Control Flow

```text
CLI Command
  -> ScanLogFileCommand
  -> MediatR
  -> ExceptionToResultBehavior
  -> ValidationBehavior
  -> LoggingBehavior
  -> PerformanceBehavior
  -> ScanLogFileCommandHandler
  -> Config Load
  -> Policy Load
  -> LogSource Create
  -> Format Detect
  -> Parser Resolve
  -> Streaming Read
  -> Parse LogLine to SecurityEvent
  -> Preprocess Event
  -> Run Single Event Rules
  -> Run Dummy ML Stage if enabled
  -> Update Correlation Windows
  -> Produce Correlation Findings
  -> Score Findings
  -> Aggregate Summary
  -> Write Reports
  -> Return ScanReportDto
  -> Render Terminal Summary
  -> Map Exit Code
```

## Solution ve Folder Structure

### Root Structure

```text
Karakol.sln
Directory.Build.props
Directory.Packages.props
global.json
README.md
.gitignore

src/
  Karakol.Cli/
  Karakol.Domain/
  Karakol.Application/
  Karakol.Contracts/
  Karakol.Shared/
  Karakol.Infrastructure/
  Karakol.Parsing/
  Karakol.Detection/
  Karakol.Correlation/
  Karakol.Risk/
  Karakol.Reporting/
  Karakol.ML/
  Karakol.Persistence/
  Karakol.Plugins.Abstractions/
  Karakol.Plugins.BuiltIn/

tests/
  Karakol.Domain.Tests/
  Karakol.Application.Tests/
  Karakol.Parsing.Tests/
  Karakol.Detection.Tests/
  Karakol.Correlation.Tests/
  Karakol.Risk.Tests/
  Karakol.Reporting.Tests/
  Karakol.Integration.Tests/
  Karakol.Performance.Tests/

samples/
docs/
reports/
models/
training/
```

### Advanced Source Folder Structure

```text
src/
  Karakol.Cli/
    Commands/
      Scan/
        ScanCommand.cs
        ScanCommandSettings.cs
      Rules/
        RulesCommand.cs
      Formats/
        FormatsCommand.cs
      Doctor/
        DoctorCommand.cs
      Version/
        VersionCommand.cs
    Rendering/
      ScanSummaryRenderer.cs
      RuleTableRenderer.cs
      FormatTableRenderer.cs
      ErrorRenderer.cs
    ExitCodes/
      CliExitCode.cs
      CliExitCodeMapper.cs
    DependencyInjection/
      CliServiceCollectionExtensions.cs
    Program.cs

  Karakol.Domain/
    Events/
      SecurityEvent.cs
      SecurityEventId.cs
    Findings/
      DetectionFinding.cs
      DetectionFindingId.cs
      Evidence.cs
    Risk/
      RiskScore.cs
      RiskComponent.cs
    Scans/
      ScanId.cs
      ScanSession.cs
      ScanSummary.cs
      ScanReport.cs
      ScanStatus.cs
    Sources/
      LogSource.cs
      LogSourceId.cs
      LogSourceType.cs
      LogFormat.cs
    Policies/
      PolicyContext.cs
    ValueObjects/
      TimelineBucket.cs
      TopSourceIpSummary.cs
      TopTargetUrlSummary.cs
      TopUserAgentSummary.cs
    Enums/
      ThreatCategory.cs
      Severity.cs
      DetectionType.cs
    Primitives/
      Entity.cs
      ValueObject.cs

  Karakol.Application/
    Abstractions/
      Files/
      Parsing/
      Detection/
      Reporting/
      Risk/
      Policies/
      Persistence/
      ML/
    Behaviors/
      ValidationBehavior.cs
      LoggingBehavior.cs
      PerformanceBehavior.cs
      ExceptionToResultBehavior.cs
      CancellationBehavior.cs
    Commands/
      ScanLogFile/
      AnalyzeDirectory/
      GenerateReport/
      ValidateConfig/
      WatchLogFile/
    Queries/
      GetSupportedFormats/
      GetAvailableRules/
      GetRuleDetails/
      ExplainFinding/
    Scanning/
      ScanOrchestrator.cs
      ScanOptionsFactory.cs
      ScanSummaryBuilder.cs
    Pipelines/
      DetectionPipelineOptions.cs
      DetectionPipelineResult.cs
    Validation/
      ValidationErrorMapper.cs
    DependencyInjection/
      ApplicationServiceCollectionExtensions.cs

  Karakol.Contracts/
    Scan/
      ScanReportDto.cs
      ScanSummaryDto.cs
      ScanResultDto.cs
    Reports/
      ReportFileDto.cs
      ReportFormatDto.cs
    Rules/
      RuleDescriptorDto.cs
      RuleDetailsDto.cs
    Formats/
      SupportedFormatDto.cs
    Findings/
      DetectionFindingDto.cs
      EvidenceDto.cs
    Plugins/
      PluginDescriptorDto.cs

  Karakol.Shared/
    Results/
      Result.cs
      ResultT.cs
    Errors/
      Error.cs
      ErrorType.cs
      Warning.cs
    Guards/
      Guard.cs
    Collections/
      ReadOnlyCollectionExtensions.cs
    Text/
      StringNormalizationExtensions.cs
      UrlDecodingHelper.cs
    Time/
      DateTimeOffsetExtensions.cs

  Karakol.Infrastructure/
    Files/
      PhysicalFileMetadataProvider.cs
      PhysicalLogInputReader.cs
      SafePathNormalizer.cs
    Configuration/
      KarakolSettingsProvider.cs
      OptionsSetup.cs
    Policies/
      JsonPolicyProvider.cs
    Security/
      SensitiveDataMasker.cs
      SensitiveDataPatterns.cs
    System/
      SystemClock.cs
      EnvironmentInfoProvider.cs
    DependencyInjection/
      InfrastructureServiceCollectionExtensions.cs

  Karakol.Parsing/
    Abstractions/
      ILogInputReader.cs
      ILogParser.cs
      ILogFormatDetector.cs
      IParserRegistry.cs
    Input/
      LogLine.cs
      ReadOptions.cs
    Detection/
      FormatDetectionResult.cs
      LogSample.cs
      DefaultLogFormatDetector.cs
    Parsers/
      AccessLogs/
        NginxAccessLogParser.cs
        ApacheAccessLogParser.cs
        AccessLogRegexes.cs
        AccessLogMapper.cs
      AuthLogs/
        AuthLogParser.cs
        SshAuthLogParser.cs
      Structured/
        JsonSecurityEventParser.cs
        CsvSecurityEventParser.cs
      Generic/
        GenericRegexLogParser.cs
    Registry/
      ParserRegistry.cs
    Statistics/
      ParserStatistics.cs
    DependencyInjection/
      ParsingServiceCollectionExtensions.cs

  Karakol.Detection/
    Abstractions/
      IDetectionRule.cs
      IAggregateDetectionRule.cs
      IRuleRegistry.cs
    Registry/
      RuleRegistry.cs
    Rules/
      Injection/
        SqlInjectionRule.cs
        CommandInjectionRule.cs
      Web/
        XssRule.cs
        PathTraversalRule.cs
      Recon/
        ScannerBotRule.cs
        DirectoryEnumerationRule.cs
      SensitiveData/
        SensitiveFileAccessRule.cs
        SuspiciousUserAgentRule.cs
    Context/
      RuleExecutionContext.cs
      PolicyContextAdapter.cs
    Options/
      RuleOptions.cs
    DependencyInjection/
      DetectionServiceCollectionExtensions.cs

  Karakol.Correlation/
    Abstractions/
      ICorrelationRule.cs
      ICorrelationEngine.cs
    Windows/
      EventWindow.cs
      SlidingTimeWindow.cs
    Aggregates/
      ScanAggregates.cs
      SourceIpAggregate.cs
    Rules/
      BruteForceCorrelationRule.cs
      DosAttemptCorrelationRule.cs
    DependencyInjection/
      CorrelationServiceCollectionExtensions.cs

  Karakol.Risk/
    Abstractions/
      IRiskScoringService.cs
    Scoring/
      DefaultRiskScoringService.cs
      RiskContext.cs
      RiskScoreMapper.cs
    Explanations/
      RiskExplanationBuilder.cs
    DependencyInjection/
      RiskServiceCollectionExtensions.cs

  Karakol.Reporting/
    Abstractions/
      IReportWriter.cs
      IReportWriterRegistry.cs
    Writers/
      Json/
        JsonReportWriter.cs
      Markdown/
        MarkdownReportWriter.cs
      Html/
        HtmlReportWriter.cs
        HtmlReportTemplate.cs
    Templates/
      TemplateRenderer.cs
    Sanitization/
      ReportSanitizer.cs
      HtmlEncodingHelper.cs
    DependencyInjection/
      ReportingServiceCollectionExtensions.cs

  Karakol.ML/
    Abstractions/
      IThreatClassifier.cs
      IEventFeatureExtractor.cs
    Features/
      DefaultEventFeatureExtractor.cs
      ThreatModelInput.cs
    Dummy/
      DummyThreatClassifier.cs
    Onnx/
      OnnxThreatClassifier.cs
    Metadata/
      ModelMetadata.cs
      ModelMetadataReader.cs
    DependencyInjection/
      MlServiceCollectionExtensions.cs

  Karakol.Persistence/
    Abstractions/
      IScanHistoryRepository.cs
      ScanHistoryFilter.cs
      ScanHistoryItem.cs
    Disabled/
      DisabledScanHistoryRepository.cs
    Sqlite/
      SqliteScanHistoryRepository.cs
    DependencyInjection/
      PersistenceServiceCollectionExtensions.cs

  Karakol.Plugins.Abstractions/
    Core/
      IPlugin.cs
      PluginMetadata.cs
    Parsers/
      IParserPlugin.cs
    Detection/
      IDetectionPlugin.cs
    Reporting/
      IReporterPlugin.cs
    ML/
      IMlPlugin.cs

  Karakol.Plugins.BuiltIn/
    Catalog/
      KarakolBuiltInPlugin.cs
      BuiltInPluginCatalog.cs
    Registration/
      BuiltInPluginRegistration.cs
```

### Tests Folder Structure

```text
tests/
  Karakol.Domain.Tests/
    Risk/
    Events/
    Findings/
    Scans/

  Karakol.Application.Tests/
    Commands/
    Behaviors/
    Validation/
    Scanning/

  Karakol.Parsing.Tests/
    Nginx/
    Apache/
    Auth/
    Json/
    Csv/
    Generic/
    FormatDetection/

  Karakol.Detection.Tests/
    Injection/
    Web/
    Recon/
    SensitiveData/

  Karakol.Correlation.Tests/
    BruteForce/
    Dos/
    Windows/

  Karakol.Risk.Tests/
    FindingRisk/
    ScanRisk/

  Karakol.Reporting.Tests/
    Json/
    Markdown/
    Html/
    Sanitization/

  Karakol.Integration.Tests/
    Cli/
    EndToEnd/
    Reports/

  Karakol.Performance.Tests/
    Streaming/
    LargeFiles/
```

### Samples Folder Structure

```text
samples/
  nginx/
    nginx-access.log
    nginx-malicious.log
  apache/
    apache-access.log
  auth/
    auth.log
    ssh-auth.log
  json/
    security-events.json
  csv/
    security-events.csv
  mixed/
    mixed-security.log
```

### Training Folder Structure

```text
training/
  datasets/
  notebooks/
  scripts/
    download_datasets.py
    normalize_security_dataset.py
    train_baseline_classifier.py
    evaluate_model.py
    export_onnx.py
```

MVP'de training script'leri calismak zorunda degildir. Klasor yapisi ve placeholder dokumantasyon, gelecek ML entegrasyonuna zemin saglamak icin tutulacaktir.

## Proje Bazlı Sorumluluklar

### Karakol.Cli

Sorumluluklar:

- CLI komutlarini tanimlar.
- Kullanici argumanlarini alir.
- Command settings modellerini dogrulanabilir sekilde Application request'lerine map eder.
- MediatR uzerinden command/query gonderir.
- Terminal output uretir.
- Exit code mapping yapar.
- Verbose ve quiet output davranislarini uygular.

Iceremeyecegi logic:

- Parser regex'leri.
- Detection pattern listeleri.
- Risk scoring algoritmasi.
- Report generation business logic.
- File streaming implementasyonu.
- Policy interpretation.

Public CLI komutlari:

- `karakol scan`
- `karakol rules`
- `karakol formats`
- `karakol doctor`
- `karakol version`

MVP sonrasi hazir tutulacak komutlar:

- `karakol scan-dir`
- `karakol watch`
- `karakol explain`
- `karakol report`

Test kapsami:

- Command settings mapping.
- Exit code mapping.
- Terminal renderer smoke tests.
- End-to-end integration scan.

### Karakol.Domain

Sorumluluklar:

- Entity ve value object modelleri.
- Domain enum'lari.
- Domain invariant'lari.
- Framework bagimsiz is kavramlari.
- Scan, event, finding ve risk modelleri.

Yasakli bagimliliklar:

- MediatR.
- FluentValidation.
- EF Core.
- Serilog.
- Spectre.Console.
- ONNX Runtime.
- CsvHelper.
- Microsoft.Extensions.*.

Ana modeller:

- `SecurityEvent`
- `LogSource`
- `DetectionFinding`
- `Evidence`
- `RiskScore`
- `RiskComponent`
- `ScanSession`
- `ScanSummary`
- `ScanReport`

Test kapsami:

- Required field invariant'lari.
- Severity mapping.
- Risk score range.
- Immutable collection davranisi.
- Finding creation validation.

### Karakol.Application

Sorumluluklar:

- CQRS command/query modelleri.
- Handler orchestration.
- FluentValidation validator'lari.
- MediatR pipeline behavior'lari.
- Scan pipeline orchestration.
- Parser, detection, risk, reporting abstraction'larini kullanma.
- Config ve policy overlay uygulama.
- Result-based error handling.

Ana command'lar:

- `ScanLogFileCommand`
- `AnalyzeDirectoryCommand`
- `GenerateReportCommand`
- `ValidateConfigCommand`
- `WatchLogFileCommand`

Ana query'ler:

- `GetSupportedFormatsQuery`
- `GetAvailableRulesQuery`
- `GetRuleDetailsQuery`
- `ExplainFindingQuery`

Test kapsami:

- Missing file result error.
- Validation failure mapping.
- Rules disabled scenario.
- Minimum severity filter.
- Successful scan orchestration.
- Report writer invocation.

### Karakol.Contracts

Sorumluluklar:

- CLI response DTO'lari.
- Report DTO'lari.
- Rule DTO'lari.
- Format DTO'lari.
- Plugin descriptor DTO'lari.
- Gelecekte dashboard, API veya extension tarafinda yeniden kullanilabilecek contract modelleri.

Kurallar:

- Domain entity'lerini dogrudan dis arayuze sizdirmamalidir.
- DTO'lar serialization-friendly olmalidir.
- Backward compatibility dusunulmelidir.

### Karakol.Shared

Sorumluluklar:

- `Result<T>`.
- `Error`.
- `Warning`.
- Guard helpers.
- Text normalization helpers.
- Common extension method'lar.

Kurallar:

- Proje bagimliligi minimumda tutulmalidir.
- Domain veya Application kavramlari buraya tasinmamalidir.
- Shared kernel sisirilmemelidir.

### Karakol.Infrastructure

Sorumluluklar:

- Physical filesystem access.
- Config loading.
- Policy loading.
- Clock implementation.
- Environment abstraction.
- Sensitive data masking.
- Path normalization.

Kurallar:

- Infrastructure, Application abstraction'larini implement eder.
- Domain'e teknoloji bagimliligi sizdirmaz.
- File IO hatalarini kontrollu error'a map edilebilir sekilde yurutur.

Test kapsami:

- Config default behavior.
- Policy file read.
- Sensitive masking.
- Safe path normalization.

### Karakol.Parsing

Sorumluluklar:

- Log input modelleri.
- Format detection.
- Parser registry.
- Nginx parser.
- Apache parser.
- Auth parser.
- SSH parser.
- JSON parser.
- CSV parser.
- Generic parser.
- Parser statistics.

Public interfaces:

- `ILogInputReader`
- `ILogParser`
- `ILogFormatDetector`
- `IParserRegistry`

Test kapsami:

- Valid parse scenarios.
- Invalid parse scenarios.
- Query extraction.
- Timestamp parsing.
- User-agent parsing.
- Auto-detect confidence.

### Karakol.Detection

Sorumluluklar:

- Single-event rule engine.
- Detection rule abstraction.
- Rule registry.
- Rule execution context.
- Built-in rules.

Built-in rule listesi:

- SQL injection.
- XSS.
- Path traversal.
- Scanner bot.
- Sensitive file access.
- Suspicious user-agent.
- Command injection.

Test kapsami:

- Known malicious payload detection.
- Encoded payload detection.
- Normal request false positive azaltimi.
- Evidence generation.
- Rule disabled behavior.

### Karakol.Correlation

Sorumluluklar:

- Event window yonetimi.
- IP bazli aggregate state.
- Brute force tespiti.
- Basic DoS tespiti.
- Bounded memory correlation.

Kurallar:

- Tum dosyayi memory'de tutmaz.
- Window ve aggregate state kontrollu buyur.
- Related event references limitlenir.

Test kapsami:

- Failed login threshold.
- Critical threshold.
- High request burst.
- Window eviction.
- Trusted IP behavior.

### Karakol.Risk

Sorumluluklar:

- Finding-level risk score.
- Scan-level overall risk score.
- Severity normalization.
- Confidence normalization.
- Risk component breakdown.
- Risk explanation uretimi.

Test kapsami:

- Severity base scores.
- Confidence modifiers.
- Correlation boost.
- Sensitive endpoint boost.
- Trusted IP reduction.
- Scan score cap at 100.

### Karakol.Reporting

Sorumluluklar:

- JSON report writer.
- Markdown report writer.
- HTML report writer.
- Report sanitizer.
- HTML encoding.
- Offline single-file report generation.

Kurallar:

- HTML external CDN kullanmaz.
- Raw log values encode edilir.
- Sensitive values maskelenir.
- JSON report machine-readable kalir.
- Markdown report CI/PR comment dostu olur.

Test kapsami:

- JSON output shape.
- HTML encoding.
- Sensitive masking.
- Markdown table rendering.
- Report file creation.

### Karakol.ML

Sorumluluklar:

- ML provider abstraction.
- Dummy classifier.
- Feature extractor.
- ONNX-ready classifier shape.
- Model metadata modelleri.

MVP davranisi:

- ML default disabled.
- Dummy classifier unavailable veya normal sonuc doner.
- Model yoksa scan fail etmez.
- ML stage warning uretir veya no-op calisir.

### Karakol.Persistence

Sorumluluklar:

- Scan history repository abstraction.
- Disabled repository.
- SQLite icin gelecek implementation zemini.

MVP davranisi:

- Persistence default disabled.
- Scan history kaydi zorunlu degil.
- Interface hazir olur.

### Karakol.Plugins.Abstractions

Sorumluluklar:

- Parser plugin contract.
- Detection plugin contract.
- Reporter plugin contract.
- ML plugin contract.
- Plugin metadata.

MVP davranisi:

- External loading yok.
- Contracts hazir.

### Karakol.Plugins.BuiltIn

Sorumluluklar:

- Built-in parser registration.
- Built-in rule registration.
- Built-in reporter registration.
- Dummy classifier registration.
- Built-in plugin catalog.

## Dependency Rules

### Allowed Dependency Direction

```text
Karakol.Cli
  -> Karakol.Application
  -> Karakol.Contracts
  -> Karakol.Infrastructure
  -> Karakol.Plugins.BuiltIn

Karakol.Application
  -> Karakol.Domain
  -> Karakol.Contracts
  -> Karakol.Shared
  -> Application abstractions

Karakol.Infrastructure
  -> Karakol.Application
  -> Karakol.Domain
  -> Karakol.Shared

Karakol.Parsing
  -> Karakol.Domain
  -> Karakol.Shared

Karakol.Detection
  -> Karakol.Domain
  -> Karakol.Shared

Karakol.Correlation
  -> Karakol.Domain
  -> Karakol.Shared
  -> Karakol.Detection abstractions when necessary

Karakol.Risk
  -> Karakol.Domain
  -> Karakol.Shared

Karakol.Reporting
  -> Karakol.Domain
  -> Karakol.Contracts
  -> Karakol.Shared

Karakol.ML
  -> Karakol.Domain
  -> Karakol.Shared

Karakol.Persistence
  -> Karakol.Domain
  -> Karakol.Shared

Karakol.Domain
  -> Karakol.Shared

Karakol.Shared
  -> no project dependency
```

### Forbidden Dependencies

- Domain, CLI'ya referans vermez.
- Domain, Infrastructure'a referans vermez.
- Domain, Reporting'e referans vermez.
- Domain, Parsing'e referans vermez.
- Application, CLI'ya referans vermez.
- Parsing, CLI'ya referans vermez.
- Detection, CLI'ya referans vermez.
- Risk, CLI'ya referans vermez.
- Reporting, CLI'ya referans vermez.

### Architectural Enforcement

Ilk MVP'de dependency kurallari manuel review ve testlerle korunacaktir. Sonraki asamada architecture tests eklenebilir:

- Domain forbidden assembly references.
- CLI business logic absence.
- Reporting no external CDN string checks.
- Infrastructure-only file IO checks.

## Runtime Scan Flow

### Command Input

Ornek:

```bash
karakol scan ./access.log --format nginx --report html --report json --output ./reports
```

### Step-by-Step Flow

1. CLI `ScanCommandSettings` parse edilir.
2. CLI settings, `ScanLogFileCommand` modeline map edilir.
3. Command MediatR'a gonderilir.
4. `ExceptionToResultBehavior` beklenmeyen exception'lari yakalamak icin outer layer olarak calisir.
5. `ValidationBehavior` FluentValidation validator'larini calistirir.
6. `LoggingBehavior` request baslangic ve bitisini loglar.
7. `PerformanceBehavior` elapsed time olcer.
8. Handler config dosyasini yukler veya defaults kullanir.
9. Handler policy dosyasini yukler veya default policy kullanir.
10. Dosya metadata'sindan `LogSource` olusturulur.
11. Format explicit verilmediyse `ILogFormatDetector` calisir.
12. `IParserRegistry` uygun parser'i secer.
13. `ILogInputReader` dosyayi satir satir stream eder.
14. Parser her satirdan `ParseResult` uretir.
15. Successful parse sonucunda `SecurityEvent` pipeline'a aktarilir.
16. Failed parse sonucunda parser statistics guncellenir.
17. Preprocessing stage normalized alanlari hazirlar.
18. Single-event detection rules calisir.
19. ML enabled ise dummy classifier stage calisir.
20. Correlation engine bounded aggregate state'i gunceller.
21. Correlation finding'leri uretilir.
22. Risk scoring finding skorlarini hesaplar.
23. Aggregation summary uretir.
24. Report writer registry istenen formatlari cozer.
25. JSON, HTML ve Markdown writer'lari calisir.
26. `ScanReportDto` Application sonucuna map edilir.
27. CLI terminal summary render eder.
28. CLI exit code belirlenir.

### Error Flow

- Validation hatasi: exit code 2.
- File read hatasi: exit code 3.
- Parser bulunamadi veya parser fatal hatasi: exit code 4.
- Unexpected error: exit code 5.
- Scan basarili ama fail policy tetiklenirse: exit code 1.
- Scan basarili ve blocking issue yoksa: exit code 0.

## Domain Model Planı

### SecurityEvent

Her normalize edilmis log satirini temsil eder.

Alanlar:

- `SecurityEventId Id`
- `LogSourceId SourceId`
- `DateTimeOffset? Timestamp`
- `string? SourceIp`
- `string? DestinationIp`
- `int? SourcePort`
- `int? DestinationPort`
- `string? Protocol`
- `string? HttpMethod`
- `string? Url`
- `string? Path`
- `string? QueryString`
- `int? StatusCode`
- `long? ResponseSize`
- `string? UserAgent`
- `string? Referrer`
- `string? Username`
- `string? Hostname`
- `string? ProcessName`
- `string? EventType`
- `LogFormat LogFormat`
- `string RawMessage`
- `string? NormalizedMessage`
- `int LineNumber`
- `IReadOnlyCollection<string> Tags`
- `IReadOnlyDictionary<string, string> Metadata`

Invariant'lar:

- `RawMessage` null veya empty olmamalidir.
- `LineNumber` 1'den kucuk olmamalidir.
- `Tags` null olmamalidir.
- `Metadata` null olmamalidir.
- Metadata immutable projection olarak expose edilmelidir.

### LogSource

Kaynak log dosyasini veya stream'i temsil eder.

Alanlar:

- `LogSourceId Id`
- `string FilePath`
- `LogFormat Format`
- `long SizeInBytes`
- `DateTimeOffset? CreatedAt`
- `DateTimeOffset? LastModifiedAt`
- `LogSourceType SourceType`

### DetectionFinding

Bir detection sonucunu temsil eder.

Alanlar:

- `DetectionFindingId Id`
- `SecurityEventId? EventId`
- `ThreatCategory Category`
- `Severity Severity`
- `RiskScore RiskScore`
- `double Confidence`
- `string DetectorId`
- `string DetectorName`
- `DetectionType DetectionType`
- `string Title`
- `string Description`
- `string Reason`
- `IReadOnlyCollection<Evidence> Evidence`
- `string RecommendedAction`
- `DateTimeOffset CreatedAt`
- `IReadOnlyCollection<SecurityEventId> RelatedEvents`
- `string? SourceIp`
- `string? TargetResource`
- `IReadOnlyCollection<string> Tags`

Invariant'lar:

- `Confidence` 0 ile 1 arasinda olmalidir.
- `DetectorId`, `DetectorName`, `Title`, `Description` bos olmamalidir.
- `Evidence` null olmamalidir.
- `RelatedEvents` null olmamalidir.

### RiskScore

Risk skorunu value object olarak tutar.

Alanlar:

- `int Value`
- `Severity Severity`
- `IReadOnlyCollection<RiskComponent> Components`
- `string Explanation`

Severity mapping:

- `0-9`: Info
- `10-39`: Low
- `40-69`: Medium
- `70-89`: High
- `90-100`: Critical

### ScanReport

Scan sonucunun domain temsilidir.

Alanlar:

- `ScanId ScanId`
- `string ToolName`
- `string ToolVersion`
- `DateTimeOffset GeneratedAt`
- `LogSource Source`
- `ScanSummary Summary`
- `IReadOnlyCollection<DetectionFinding> Findings`
- `IReadOnlyCollection<TimelineBucket> Timeline`
- `IReadOnlyCollection<string> Recommendations`
- `ScanConfigurationSnapshot Configuration`
- `IReadOnlyDictionary<string, string> Metadata`

## CQRS ve Application Planı

### Commands

#### ScanLogFileCommand

Alanlar:

- `string FilePath`
- `string? Format`
- `string OutputDirectory`
- `IReadOnlyCollection<string> ReportFormats`
- `bool EnableRules`
- `bool EnableMl`
- `bool EnableCorrelation`
- `bool PersistHistory`
- `string? MinimumSeverity`
- `int? MaxLines`
- `bool MaskSensitiveData`
- `bool IncludeRawSamples`
- `string? ConfigurationFile`
- `string? PolicyFile`

Handler sorumluluklari:

- File access kontrolu.
- Scan options uretimi.
- Config ve policy cozumleme.
- Parser secimi.
- Detection pipeline calistirma.
- Summary uretme.
- Report writer'lari calistirma.
- DTO response donme.

#### AnalyzeDirectoryCommand

MVP'de contract olarak hazir tutulur.

Alanlar:

- `string DirectoryPath`
- `bool Recursive`
- `IReadOnlyCollection<string> IncludePatterns`
- `IReadOnlyCollection<string> ExcludePatterns`
- `string OutputDirectory`
- `IReadOnlyCollection<string> ReportFormats`
- `bool EnableCorrelationAcrossFiles`

#### GenerateReportCommand

MVP'de contract olarak hazir tutulur.

Alanlar:

- `string InputJsonReportPath`
- `string OutputDirectory`
- `IReadOnlyCollection<string> ReportFormats`

#### ValidateConfigCommand

Config dogrulama icin hazir tutulur.

Alanlar:

- `string ConfigurationFile`

#### WatchLogFileCommand

MVP sonrasi icin hazir tutulur.

Alanlar:

- `string FilePath`
- `string? Format`
- `string? PolicyFile`
- `bool EnableNotifications`

### Queries

MVP ve post-MVP query'leri:

- `GetSupportedFormatsQuery`
- `GetAvailableRulesQuery`
- `GetRuleDetailsQuery`
- `ExplainFindingQuery`
- `GetScanHistoryQuery`
- `GetScanReportQuery`

### Validation

`ScanLogFileCommandValidator` kurallari:

- FilePath bos olamaz.
- FilePath var olan dosya olmali.
- ReportFormats bos ise default `json` kullanilmali.
- ReportFormats sadece `json`, `html`, `markdown` olabilir.
- MaxLines null veya pozitif olmali.
- MinimumSeverity gecerli enum degeri olmali.
- OutputDirectory yoksa olusturulabilir olmali.
- PolicyFile verilmisse mevcut olmali.
- ConfigurationFile verilmisse mevcut olmali.

### MediatR Behaviors

Behavior sirasi:

1. `ExceptionToResultBehavior`
2. `ValidationBehavior`
3. `LoggingBehavior`
4. `PerformanceBehavior`
5. `CancellationBehavior`

Behavior hedefleri:

- Hatayi exception olarak CLI'ya tasimamali.
- Validation failure'lari structured result error'a donusturmeli.
- Hassas argumanlari loglamamali.
- Uzun suren islemler icin warning log uretmeli.
- Cancellation'i kontrollu yonetmeli.

## Parser Sistemi

### Parser Abstractions

`ILogInputReader`:

```csharp
IAsyncEnumerable<LogLine> ReadLinesAsync(
    LogSource source,
    ReadOptions options,
    CancellationToken cancellationToken);
```

`ILogFormatDetector`:

```csharp
Task<FormatDetectionResult> DetectAsync(
    LogSource source,
    CancellationToken cancellationToken);
```

`ILogParser`:

```csharp
string FormatName { get; }
LogFormat Format { get; }
int Priority { get; }

bool CanParse(LogSample sample);

IAsyncEnumerable<ParseResult> ParseAsync(
    IAsyncEnumerable<LogLine> lines,
    ParserOptions options,
    CancellationToken cancellationToken);
```

### Nginx Parser

Desteklenecek ornek:

```text
127.0.0.1 - - [10/Oct/2026:13:55:36 +0300] "GET /login?id=1 HTTP/1.1" 200 2326 "-" "Mozilla/5.0"
```

Cikarilacak alanlar:

- SourceIp
- Timestamp
- HttpMethod
- Url
- Path
- QueryString
- StatusCode
- ResponseSize
- Referrer
- UserAgent
- RawMessage
- LineNumber

### Apache Parser

Destek:

- Common log format.
- Combined log format.

Nginx parser ile ortak access log mapper kullanilabilir.

### Auth ve SSH Parser

Desteklenecek ornek:

```text
Jan 12 10:15:32 server sshd[12345]: Failed password for invalid user admin from 192.168.1.20 port 54321 ssh2
```

Cikarilacak alanlar:

- Timestamp
- Hostname
- ProcessName
- EventType
- Username
- SourceIp
- SourcePort
- RawMessage

### JSON Parser

MVP davranisi:

- JSON Lines desteklenir.
- Common field names best-effort map edilir.
- JSON array support optional veya post-MVP olarak dokumante edilir.

### CSV Parser

MVP davranisi:

- Header row beklenir.
- Common kolon isimleri map edilir.
- Mapping config extension point olarak hazir tutulur.

### Generic Parser

Fallback davranisi:

- Raw message korunur.
- Line number set edilir.
- Best-effort IP extraction yapilabilir.
- Best-effort timestamp extraction post-MVP olabilir.

### Format Detection

Detection mantigi:

- Ilk N satir sample olarak okunur.
- Parser `CanParse` skorlamasi yapar.
- Her parser icin confidence hesaplanir.
- En yuksek confidence secilir.
- Confidence dusukse `Generic` fallback kullanilir.

## Detection Rule Engine

### Rule Contract

```csharp
public interface IDetectionRule
{
    string RuleId { get; }
    string Name { get; }
    string Description { get; }
    ThreatCategory Category { get; }
    Severity DefaultSeverity { get; }
    RuleScope Scope { get; }
    bool IsEnabled(RuleOptions options);
    bool CanAnalyze(SecurityEvent securityEvent);
    DetectionFinding? Analyze(SecurityEvent securityEvent, RuleExecutionContext context);
}
```

### Rule Execution Context

Context alanlari:

- Rule options.
- Policy context.
- Event window.
- Scan aggregates.
- System clock.

### Built-in Rules

#### SQL Injection Rule

Rule ID:

```text
sqli.basic
```

Pattern aileleri:

- `' OR '1'='1`
- `OR 1=1`
- `UNION SELECT`
- `INFORMATION_SCHEMA`
- `SLEEP(`
- `BENCHMARK(`
- `DROP TABLE`
- `xp_cmdshell`
- `concat(`
- `load_file(`
- `waitfor delay`
- `select * from`
- `insert into`
- `delete from`

Analiz alanlari:

- Url
- Path
- QueryString
- RawMessage
- NormalizedMessage

#### XSS Rule

Rule ID:

```text
xss.basic
```

Pattern aileleri:

- `<script`
- `</script>`
- `javascript:`
- `onerror=`
- `onload=`
- `alert(`
- `document.cookie`
- `%3cscript`
- `svg/onload`
- `<iframe`
- `eval(`

#### Path Traversal Rule

Rule ID:

```text
path-traversal.basic
```

Pattern aileleri:

- `../`
- `..\`
- `%2e%2e%2f`
- `%252e%252e%252f`
- `/etc/passwd`
- `boot.ini`
- `win.ini`
- `/proc/self/environ`
- `windows/system32`

#### Scanner Bot Rule

Rule ID:

```text
scanner-bot.basic
```

Path pattern'leri:

- `/wp-admin`
- `/wp-login.php`
- `/phpmyadmin`
- `/.env`
- `/server-status`
- `/actuator`
- `/admin`
- `/config.php`
- `/.git/config`
- `/vendor/phpunit`
- `/cgi-bin`
- `/boaform`

User-agent pattern'leri:

- `sqlmap`
- `nikto`
- `masscan`
- `nmap`
- `zgrab`
- `dirbuster`
- `gobuster`
- `nuclei`
- `python-requests`
- `curl`
- `wget`

#### Sensitive File Access Rule

Rule ID:

```text
sensitive-file.basic
```

Pattern aileleri:

- `.env`
- `id_rsa`
- `config.yml`
- `appsettings.json`
- `web.config`
- `.git/config`
- `backup.zip`
- `backup.sql`
- `database.sql`
- `dump.sql`
- `.npmrc`
- `.aws/credentials`

#### Suspicious User-Agent Rule

Rule ID:

```text
suspicious-user-agent.basic
```

Tespitler:

- Empty user-agent.
- Cok kisa user-agent.
- Known scanner signature.
- Cok yuksek entropy, post-MVP.
- Siklikla degisen user-agent, correlation tarafinda post-MVP.

#### Command Injection Rule

Rule ID:

```text
command-injection.basic
```

Pattern aileleri:

- `;cat `
- `|cat `
- `&&`
- `||`
- Backtick command substitution.
- `$(`
- `bash -c`
- `cmd.exe`
- `powershell`
- `wget http`
- `curl http`

### Evidence Model

Her finding evidence uretmelidir:

- Field.
- Value.
- MatchedPattern.
- Explanation.

Ornek:

```text
Field = Url
Value = /login?id=1 UNION SELECT password
MatchedPattern = UNION SELECT
Explanation = URL query contains a common SQL injection keyword.
```

## Correlation Engine

Correlation engine, tekil log satirlarindan anlasilamayan davranis pattern'lerini yakalar.

### Brute Force Correlation Rule

Rule ID:

```text
bruteforce.auth
```

Tespit:

- Ayni IP'den kisa surede cok fazla failed login.
- Ayni IP'den cok fazla 401.
- Ayni IP'den cok fazla 403.
- Login endpoint'lerine yogun deneme.

Default config:

- TimeWindowMinutes = 5
- FailedThreshold = 10
- CriticalThreshold = 50

Severity:

- Threshold ustu Medium.
- Critical threshold ustu Critical.

### Basic DoS Correlation Rule

Rule ID:

```text
dos.basic
```

Tespit:

- Ayni IP'den cok kisa surede cok fazla request.
- Cok sayida 404 veya 500.
- Tek endpoint'e yogun istek.

Default config:

- TimeWindowSeconds = 30
- RequestThreshold = 500

### Window Management

Bounded state kurallari:

- IP bazli aggregate tutulur.
- Window suresi dolan event referanslari atilir.
- RelatedEvents sayisi limitlenir.
- Full raw event listesi memory'de tutulmaz.
- Max tracked source IP config ile limitlenebilir.

## Risk Scoring

### Finding-Level Risk

Base severity score:

- Info = 5
- Low = 20
- Medium = 50
- High = 75
- Critical = 95

Modifier'lar:

- Dusuk confidence skor dusurur.
- Correlation finding skor artirir.
- Sensitive endpoint skor artirir.
- Trusted IP skor dusurur.
- Ayni IP'den tekrar eden finding skor artirir.
- Birden fazla threat category ayni IP'de birlesirse skor artirir.

### Scan-Level Risk

Hesaplamada dikkate alinacaklar:

- Critical finding sayisi.
- High finding sayisi.
- Medium finding sayisi.
- Unique threat category sayisi.
- Top attacking IP concentration.
- Timeline density.
- Sensitive endpoints touched.
- Repeated attacks.
- Failed parse ratio.

Basit MVP formulu:

```text
baseScore =
  criticalCount * 25 +
  highCount * 10 +
  mediumCount * 4 +
  lowCount * 1

repetitionBoost =
  repeatedSourceIpCount * 5

categoryDiversityBoost =
  uniqueThreatCategoryCount * 3

sensitiveEndpointBoost =
  sensitiveEndpointFindingCount * 5

finalScore =
  min(100, baseScore + repetitionBoost + categoryDiversityBoost + sensitiveEndpointBoost)
```

Bu algoritma `IRiskScoringService` arkasinda strategy olarak tutulacaktir.

## Reporting

### Report Writers

MVP writer'lari:

- `JsonReportWriter`
- `MarkdownReportWriter`
- `HtmlReportWriter`

Report writer contract:

```csharp
public interface IReportWriter
{
    string Format { get; }
    string FileExtension { get; }

    Task<ReportWriteResult> WriteAsync(
        ScanReport report,
        ReportOptions options,
        CancellationToken cancellationToken);
}
```

### JSON Report

Amac:

- Machine-readable output.
- Dashboard ve CI entegrasyonlari icin temel veri.
- Report regeneration icin source.

Ozellikler:

- Indented JSON.
- Deterministic property naming.
- Scan metadata.
- Summary.
- Findings.
- Evidence.
- Configuration snapshot.

### Markdown Report

Amac:

- Terminal veya CI artifact olarak okunabilir output.
- PR comment formatina uygun sade ozet.

Bolumler:

- Summary.
- Risk score.
- Severity table.
- Top categories.
- Critical findings.
- Recommended actions.

### HTML Report

Amac:

- Offline acilabilen profesyonel dashboard raporu.

Kurallar:

- Tek dosya.
- External CDN yok.
- Inline CSS.
- JavaScript zorunlu degil.
- Raw log degerleri encode edilir.
- Sensitive data maskelenir.

Bolumler:

1. Executive Summary
2. Overall Risk Score
3. Severity Distribution
4. Threat Category Distribution
5. Timeline
6. Top Source IPs
7. Top Target URLs
8. Top User Agents
9. Critical Findings
10. All Findings Table
11. Evidence Details
12. Recommended Actions
13. Parser Statistics
14. Scan Configuration
15. Metadata

## CLI Planı

### Main Executable

```text
karakol
```

### scan

Kullanim:

```bash
karakol scan ./access.log
```

Options:

- `--format nginx|apache|auth|ssh|json|csv|generic|auto`
- `--report json`
- `--report html`
- `--report markdown`
- `--output ./reports`
- `--enable-ml`
- `--disable-rules`
- `--disable-correlation`
- `--min-severity low|medium|high|critical`
- `--max-lines 100000`
- `--mask-sensitive-data`
- `--include-raw-samples`
- `--policy ./karakol.policy.json`
- `--config ./karakolsettings.json`
- `--verbose`
- `--quiet`

### rules

Built-in rule listesini gosterir:

```text
RuleId              Category              Severity       Enabled
sqli.basic          SqlInjection          High           true
xss.basic           Xss                   High           true
bruteforce.auth     BruteForce            Critical       true
```

### formats

Desteklenen parser formatlarini listeler.

### doctor

Environment ve config kontrolu yapar:

- Config gecerli mi?
- Model dosyasi varsa okunabiliyor mu?
- Reports klasoru yazilabilir mi?
- Sample logs erisilebilir mi?

### version

Tool versiyonunu gosterir.

### Exit Codes

- 0: scan basarili, blocking issue yok.
- 1: scan basarili ama fail-on-critical veya fail-on-high condition tetiklendi.
- 2: validation/config error.
- 3: file read error.
- 4: parser error.
- 5: unexpected error.

## Security Considerations

Karakol local-first bir urundur. Guvenlik kararlarinin temeli, log verisinin local ortamdan cikmamasidir.

### Local-First Guarantees

- Log dosyalari external servislere gonderilmez.
- External API cagrisi yapilmaz.
- Model download otomatik yapilmaz.
- Telemetry yoktur.
- HTML report offline acilir.

### HTML Safety

- Raw log content HTML encode edilir.
- Evidence value encode edilir.
- User-agent encode edilir.
- URL ve query encode edilir.
- Inline CSS kullanilir.
- External script ve CDN kullanilmaz.

### Sensitive Data Masking

`ISensitiveDataMasker` su verileri maskeler:

- Email.
- JWT token.
- Bearer token.
- API key benzeri uzun token'lar.
- Password query parameter.
- Token query parameter.
- Session id.
- Authorization header.

Ornek:

```text
/login?username=ali&password=123456
```

Rapor output:

```text
/login?username=ali&password=******
```

### File Safety

- File path normalize edilir.
- Output directory kontrollu olusturulur.
- Config ve policy path'leri varlik kontrolunden gecer.
- Directory traversal benzeri path misuse davranislarina karsi safe path helper kullanilir.

## Config ve Policy Planı

### karakolsettings.json

Ornek:

```json
{
  "Scanning": {
    "MaxLines": null,
    "StreamingBatchSize": 1000,
    "AutoDetectFormat": true,
    "MaskSensitiveData": true,
    "IncludeRawSamples": false
  },
  "Rules": {
    "Enabled": true,
    "BruteForce": {
      "Enabled": true,
      "TimeWindowMinutes": 5,
      "FailedThreshold": 10,
      "CriticalThreshold": 50
    },
    "DosAttempt": {
      "Enabled": true,
      "TimeWindowSeconds": 30,
      "RequestThreshold": 500
    }
  },
  "ML": {
    "Enabled": false,
    "ModelPath": "./models/karakol-threat-classifier.onnx",
    "MinimumConfidence": 0.70
  },
  "Reporting": {
    "DefaultOutputDirectory": "./reports",
    "DefaultFormats": ["json", "html"],
    "IncludeEvidence": true,
    "IncludeRawSamples": false
  },
  "Persistence": {
    "Enabled": false,
    "Provider": "SQLite",
    "ConnectionString": "Data Source=karakol.db"
  }
}
```

Options classes:

- `ScanningOptions`
- `RuleOptions`
- `BruteForceRuleOptions`
- `DosAttemptRuleOptions`
- `MlOptions`
- `ReportingOptions`
- `PersistenceOptions`

### karakol.policy.json

Ornek:

```json
{
  "minimumSeverity": "Medium",
  "failOnCritical": true,
  "enabledRules": [
    "sqli.basic",
    "xss.basic",
    "bruteforce.auth"
  ],
  "disabledRules": [],
  "sensitivePaths": [
    "/admin",
    "/api/payment",
    "/api/users/export"
  ],
  "trustedIps": [
    "127.0.0.1",
    "10.0.0.0/8"
  ],
  "ignoredPaths": [
    "/health",
    "/metrics"
  ]
}
```

Policy context alanlari:

- MinimumSeverity.
- EnabledRules.
- DisabledRules.
- SensitivePaths.
- TrustedIps.
- IgnoredPaths.

## ML Hazırlık Planı

MVP'de gercek ML modeli zorunlu degildir. Ancak mimari hazir olmalidir.

### Interfaces

`IThreatClassifier`:

```csharp
public interface IThreatClassifier
{
    bool IsAvailable { get; }
    string ModelName { get; }
    string ModelVersion { get; }

    ThreatClassificationResult Classify(SecurityEvent securityEvent);
}
```

`IEventFeatureExtractor`:

```csharp
ThreatModelInput Extract(SecurityEvent securityEvent);
```

### MVP Implementation

- `DummyThreatClassifier`.
- Default disabled.
- ML enabled ama model yoksa scan fail etmez.
- No-op veya Normal sonuc doner.

### Future ONNX Implementation

- `OnnxThreatClassifier`.
- `ModelMetadataReader`.
- Confidence threshold.
- Model version validation.

### Training Structure

Python script hedefleri:

- Dataset download.
- Dataset normalize.
- Baseline classifier train.
- Evaluation metrics.
- ONNX export.

## Plugin Architecture

MVP'de external dynamic loading yoktur. Ancak plugin abstraction hazir tutulur.

### Plugin Types

- Parser plugin.
- Detection rule plugin.
- Correlation rule plugin.
- Reporter plugin.
- ML provider plugin.

### Interfaces

`IPlugin`:

- Id.
- Name.
- Version.
- Description.

`IParserPlugin`:

- `GetParsers()`

`IDetectionPlugin`:

- `GetRules()`

`IReporterPlugin`:

- `GetReportWriters()`

`IMlPlugin`:

- `CreateClassifier()`

### Built-in Plugin

`KarakolBuiltInPlugin` su bileşenleri register eder:

- Nginx parser.
- Apache parser.
- SSH/Auth parser.
- JSON parser.
- CSV parser.
- Generic parser.
- Built-in rules.
- Built-in correlation rules.
- JSON/HTML/Markdown reporters.
- Dummy classifier.

## Test Stratejisi

### Domain Tests

- RiskScore value 0-100 arasi olmali.
- Severity mapping dogru calismali.
- DetectionFinding creation valid olmali.
- SecurityEvent required fields valid olmali.
- Metadata immutable expose edilmeli.

### Parsing Tests

- Nginx valid line parse edilir.
- Nginx invalid line parse error doner.
- Apache valid line parse edilir.
- SSH failed login parse edilir.
- SSH invalid user parse edilir.
- SSH accepted password parse edilir.
- JSON event parse edilir.
- CSV event parse edilir.
- Format detection confidence dogru hesaplanir.
- QueryString dogru ayrilir.
- UserAgent dogru alinir.

### Rule Tests

- SQLi payload yakalanir.
- Normal URL SQLi olarak isaretlenmez.
- XSS payload yakalanir.
- Encoded XSS yakalanir.
- Path traversal yakalanir.
- Encoded traversal yakalanir.
- Scanner bot path yakalanir.
- sqlmap user-agent yakalanir.
- Sensitive file access `.env` yakalanir.
- Command injection payload yakalanir.

### Correlation Tests

- Ayni IP'den 10 failed login brute force uretir.
- Critical threshold asildiginda Critical severity uretir.
- Trusted IP brute force disinda tutulabilir veya risk dusurulur.
- Ayni IP'den yuksek request DoS uretir.
- Window eviction dogru calisir.

### Risk Tests

- Critical finding yuksek skor uretir.
- Trusted IP skoru dusurur.
- Sensitive endpoint skoru artirir.
- Category diversity scan riskini artirir.
- Scan risk 100'u gecmez.

### Application Tests

- Dosya yoksa Result.Error doner.
- Parser bulunamazsa anlamli error doner.
- Scan successful report doner.
- Rules disabled ise finding cikmaz.
- Minimum severity filtresi calisir.
- JSON report uretilir.
- HTML report uretilir.
- Config loading calisir.
- Policy loading calisir.

### Integration Tests

- Sample nginx log scan edilir.
- Sample SSH log scan edilir.
- Mixed directory scan post-MVP veya optional smoke olarak ele alinir.
- Report files olusur.
- CLI scan komutu uctan uca calisir.

### Performance Tests

- 100k satir log makul surede analiz edilir.
- Bellek kullanimi kontrollu kalir.
- Streaming reader tum dosyayi memory'ye almaz.
- `MaxLines` uygulanir.

## Operasyonel Geliştirme Aşamaları

### 1. Workspace Bootstrap

- `global.json` olustur.
- `Directory.Build.props` olustur.
- `Directory.Packages.props` olustur.
- `.gitignore` olustur.
- `Karakol.sln` olustur.
- Root README skeleton olustur.

Done criteria:

- `dotnet --version` expected SDK ile uyumlu.
- Empty build infrastructure hazir.

### 2. Solution ve Project Scaffold

- Tum `src` projelerini olustur.
- Tum `tests` projelerini olustur.
- Project references ekle.
- Namespace standardini belirle.
- Central package versions ekle.

Done criteria:

- `dotnet build` bos scaffold ile basarili.

### 3. Shared Kernel ve Result Pattern

- `Result<T>` implement et.
- `Result` helper implement et.
- `Error`, `ErrorType`, `Warning` ekle.
- Guard helper ekle.
- Shared tests yaz.

Done criteria:

- Success/failure behavior testleri geciyor.

### 4. Domain Models

- Strongly typed IDs ekle.
- Enums ekle.
- `SecurityEvent` ekle.
- `LogSource` ekle.
- `DetectionFinding` ekle.
- `RiskScore` ekle.
- `ScanSummary` ve `ScanReport` ekle.

Done criteria:

- Domain invariant testleri geciyor.

### 5. Application CQRS

- Commands ekle.
- Queries ekle.
- Validators ekle.
- Handler skeleton'lari ekle.
- Application abstraction'lari ekle.

Done criteria:

- Validation tests geciyor.

### 6. MediatR Behaviors

- Validation behavior.
- Logging behavior.
- Performance behavior.
- Exception-to-result behavior.
- Cancellation behavior.

Done criteria:

- Behavior ordering ve error mapping testleri geciyor.

### 7. Infrastructure Adapters

- File metadata provider.
- Physical input reader.
- Config provider.
- Policy provider.
- Sensitive data masker.
- System clock.

Done criteria:

- File read ve masking tests geciyor.

### 8. Parser Abstractions

- `ILogInputReader`.
- `ILogParser`.
- `ILogFormatDetector`.
- `IParserRegistry`.
- Parse result models.

Done criteria:

- Parser registry unit tests geciyor.

### 9. MVP Parser Implementations

- Nginx parser.
- Apache parser.
- SSH/Auth parser.
- JSON parser.
- CSV parser.
- Generic parser.

Done criteria:

- Her parser icin valid ve invalid line tests geciyor.

### 10. Format Detection

- Sample read.
- Parser confidence scoring.
- Fallback logic.

Done criteria:

- Nginx, Apache, SSH, JSON, CSV sample'lari dogru tespit ediliyor.

### 11. Preprocessing Stages

- URL decode.
- Path normalization.
- Query extraction.
- User-agent normalization.
- Raw message normalization.
- Sensitive data masking hook.

Done criteria:

- Encoded payload detection'a hazir normalized output uretiliyor.

### 12. Detection Rule Engine

- Rule contract.
- Rule registry.
- Rule execution context.
- Single-event stage.

Done criteria:

- Enabled/disabled rule behavior calisiyor.

### 13. Built-in Rules

- SQLi.
- XSS.
- Path traversal.
- Scanner bot.
- Sensitive file.
- Suspicious user-agent.
- Command injection.

Done criteria:

- Promptta verilen malicious samples finding uretiyor.

### 14. Correlation Engine

- Event window.
- Aggregates.
- Brute force.
- Basic DoS.

Done criteria:

- Threshold-based tests geciyor.

### 15. Risk Scoring

- Finding score.
- Scan score.
- Risk components.
- Explanation builder.

Done criteria:

- Overall risk scan summary'de gorunuyor.

### 16. Reporting Writers

- JSON writer.
- Markdown writer.
- HTML writer.
- Sanitization layer.

Done criteria:

- JSON ve HTML report files olusuyor.
- HTML encoded output uretiyor.

### 17. CLI Commands

- Program setup.
- DI registration.
- `scan`.
- `rules`.
- `formats`.
- `doctor`.
- `version`.
- Exit code mapping.

Done criteria:

- Ana scan komutu calisiyor.

### 18. Sample Logs

- Nginx normal ve malicious samples.
- SSH/Auth samples.
- Apache sample.
- JSON sample.
- CSV sample.

Done criteria:

- Integration test sample dosyalarla calisiyor.

### 19. Unit Tests

- Domain.
- Parsing.
- Detection.
- Correlation.
- Risk.
- Reporting.
- Application.

Done criteria:

- `dotnet test` unit test scope basarili.

### 20. Integration Tests

- End-to-end CLI scan.
- Report generation.
- Sample nginx detection.

Done criteria:

- Required command integration test ile dogrulaniyor.

### 21. Documentation

- README.
- `docs/architecture/architecture.md`.
- `docs/planning/karakol-mvp-operasyon-plani.md`.
- Detection pipeline docs.
- Security considerations docs.

Done criteria:

- Yeni gelistirici projeyi build, test ve run edebiliyor.

### 22. Final Verification

Calistirilacak komutlar:

```bash
dotnet build
dotnet test
dotnet run --project src/Karakol.Cli -- scan samples/nginx/nginx-access.log --format nginx --report html --report json
```

Done criteria:

- Build basarili.
- Testler basarili.
- Ana scan komutu basarili.
- Report artifacts mevcut.

## Kabul Kriterleri

MVP kabul icin su kriterler saglanmalidir:

- `dotnet build` basarili.
- `dotnet test` basarili.
- Ana scan komutu calisiyor.
- SQLi ornegi yakalaniyor.
- XSS ornegi yakalaniyor.
- Path traversal ornegi yakalaniyor.
- Scanner bot ornegi yakalaniyor.
- Sensitive file ornegi yakalaniyor.
- JSON report olusuyor.
- HTML report olusuyor.
- HTML report offline acilabiliyor.
- HTML output encoded.
- Sensitive data masked.
- CLI terminal summary okunabilir.
- Domain framework bagimsiz.
- Hicbir external API cagrisi yok.
- Buyuk dosya isleme streaming tabanli.
- Yeni parser eklenebilir durumda.
- Yeni rule eklenebilir durumda.
- Yeni reporter eklenebilir durumda.
- ML abstraction hazir.
- Persistence abstraction hazir.
- Built-in plugin catalog hazir.

## Varsayımlar

- Proje bos klasorden baslatilacak.
- Target framework `net9.0` olacak.
- CLI framework `Spectre.Console.Cli` olacak.
- Terminal rendering icin `Spectre.Console` kullanilacak.
- MediatR CQRS altyapisinin merkezinde olacak.
- FluentValidation validator'lar Application katmaninda tutulacak.
- Result Pattern kontrollu error handling icin kullanilacak.
- ML gercek modeli MVP'de zorunlu degil.
- Dummy classifier MVP icin yeterli.
- Persistence implementation MVP'de zorunlu degil.
- Persistence abstraction yeterli.
- External plugin loading MVP disi.
- Built-in plugin catalog MVP icin yeterli.
- JSON parser MVP'de JSON Lines odakli olacak.
- HTML rapor external CDN kullanmayacak.
- Sensitive data masking default enabled olacak.
- Report output default directory `./reports` olacak.
- Default report format `json` olacak; prompttaki hedef komutta `html` ve `json` birlikte uretilecek.
