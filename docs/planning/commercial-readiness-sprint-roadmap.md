# Karakol Commercial Readiness Sprint Roadmap

Bu dokuman `commercial-readiness-10-10-plan.md` icindeki hedefleri uygulanabilir fazlara ve sprintlere boler. Amac, Karakol'u calisan MVP seviyesinden open-source olarak yayinlanabilir, kullanilabilir ve zamanla ticari kaliteye yaklasan bir urune tasimaktir.

Bu roadmap uygulama dokumanidir. Her sprint sonunda build, test ve sample scan calismali; tamamlanan sprintler dokumanda isaretlenmelidir.

## Hedef Durum

Karakol icin hedef durum:

- Open source olarak yayinlanabilir repo.
- Yeni bir gelistiricinin README ve docs ile projeyi calistirabilmesi.
- CLI'nin global dotnet tool olarak kurulabilmesi.
- Raporlarin profesyonel, offline, encoded ve logolu olmasi.
- Parser ve rule kalitesinin gercek dunya loglariyla sertlestirilmesi.
- Buyuk loglarda streaming ve bounded memory davranisinin kanitlanmasi.
- CI/CD ile build, test, pack ve sample scan kapilarinin otomatik calismasi.
- Serilog ile kontrollu, redaction-aware logging.
- DuckDB ile opsiyonel local OLAP analytics storage.
- Config/policy schema ve ornek dosyalarin netlesmesi.

## Uygulama Prensipleri

1. Core scan flow her sprint sonunda calismali.
2. Local-first garanti bozulmamali.
3. Yeni dependency eklenirse gerekcesi dokumante edilmeli.
4. Domain katmani framework bagimsiz kalmali.
5. CLI business logic icermemeli.
6. DuckDB, Serilog ve ileri persistence ozellikleri opt-in olmali.
7. Raw loglar default olarak kalici storage'a yazilmamali.
8. Her yeni parser/rule/reporter/provider icin test ve dokuman eklenmeli.
9. Open-source repo icin secret, token, gercek musteri logu veya hassas veri commit edilmemeli.

## Faz 0 - Baseline ve Repo Kilitleme

### Sprint 0.1 - Baseline Verification

Taskler:

- [x] `dotnet restore` calistirilacak.
- [x] `dotnet build --no-restore` calistirilacak.
- [x] `dotnet test --no-restore` calistirilacak.
- [x] Sample scan calistirilacak.
- [x] JSON ve HTML rapor olustugu dogrulanacak.
- [x] HTML raporda `karakol_logo.png` data URI olarak gomulu mu kontrol edilecek.
- [x] `dotnet pack --no-restore src/Karakol.Cli/Karakol.Cli.csproj -o artifacts/packages` calistirilacak.
- [x] Mevcut test sayisi ve kritik komut ciktilari release notuna yazilacak.

Kabul kriterleri:

- Build 0 warning / 0 error.
- Test suite tamamen yesil.
- Sample scan exit code `0`.
- Raporlar `reports/` altinda olusuyor.
- Pack komutu `.nupkg` uretiyor.

### Sprint 0.2 - Roadmap Tracking

Taskler:

- [x] Bu roadmap ana takip dokumani olarak kabul edilecek.
- [x] `docs/planning/commercial-readiness-10-10-plan.md` stratejik plan olarak korunacak.
- [x] Her sprint sonunda bu dosyadaki checklist guncellenecek.
- [x] Sprint tamamlandikca ilgili docs linkleri README'de korunacak.

Kabul kriterleri:

- Stratejik plan ve sprint roadmap birbirini tekrar etmeyecek.
- Sprint bazli durum tek dokumandan takip edilebilecek.

## Faz 1 - Open Source Release Foundation

### Sprint 1.1 - Repo Metadata ve Community Dosyalari

Taskler:

- [x] `LICENSE` dosyasi kontrol edilecek.
- [x] `CONTRIBUTING.md` gelistirme akisini tam anlatacak sekilde guncellenecek.
- [x] `CODE_OF_CONDUCT.md` kontrol edilecek.
- [x] `SECURITY.md` private vulnerability reporting stratejisiyle guncellenecek.
- [x] `README.md` open-source public repo icin okunacak hale getirilecek.
- [x] Package metadata final GitHub owner/repo ile guncellenecek.
- [x] `CHANGELOG.md` release formatina gore guncellenecek.

Kabul kriterleri:

- Yeni contributor build/test/run adimlarini README'den yapabiliyor.
- Security report yolu net.
- Package metadata placeholder icermiyor.

### Sprint 1.2 - GitHub Templates

Taskler:

- [x] Bug report issue template guncellenecek.
- [x] Feature request issue template guncellenecek.
- [x] Security issue public template olmamali; SECURITY dosyasina yonlendirme yapilmali.
- [x] Pull request template eklenecek.
- [x] PR checklist icine build/test/sample scan/docs maddeleri eklenecek.

Kabul kriterleri:

- Yeni issue/PR acan kisi gerekli bilgileri verir.
- Hassas loglarin public issue'ya konmamasi acikca belirtilir.

### Sprint 1.3 - Documentation Index

Taskler:

- [x] `docs/README.md` konu bazli linkleri dogrulayacak.
- [x] `docs/architecture/` altindaki mimari dokumanlar guncel path'lerle uyumlu olacak.
- [x] `docs/user-guide/` kullanici odakli kalacak.
- [x] `docs/configuration/` config/policy odakli kalacak.
- [x] `docs/data/` DuckDB ve analytics odakli kalacak.
- [x] Eski dokuman yollarina kirik link kalmayacak.

Kabul kriterleri:

- `rg "docs/.*\\.md" README.md docs` ile eski/kirik linkler yakalanmayacak.
- Dokuman yapisi yeni gelistirici icin okunabilir.

## Faz 2 - CI/CD ve Release Gate

### Sprint 2.1 - Minimum CI

Taskler:

- [x] `.github/workflows/ci.yml` restore calistiracak.
- [x] CI build calistiracak.
- [x] CI test calistiracak.
- [x] CI pack calistiracak.
- [x] CI sample scan calistiracak.
- [x] CI artifacts olarak paket ve sample reports saklanacak.
- [x] CI Windows ve Ubuntu matrix icin degerlendirilecek.

Kabul kriterleri:

- Pull request CI yesil olmadan merge edilmemeli.
- CI sample scan rapor uretimini kanitlamali.

### Sprint 2.2 - Release Workflow

Taskler:

- [x] Manual release workflow tasarlanacak.
- [x] Tag format kurali belirlenecek: `vMAJOR.MINOR.PATCH`.
- [x] `CHANGELOG.md` release gate'e baglanacak.
- [x] NuGet publish secret gereksinimleri dokumante edilecek.
- [x] NuGet publish manual approval ile calisacak sekilde planlanacak.
- [x] Release checklist dokumani guncellenecek.

Kabul kriterleri:

- Release candidate ayni komut setiyle uretilebilir.
- NuGet publish otomatik ama manuel tetiklemeli olur.

### Sprint 2.3 - Installer ve Global Tool Dağıtımı

Taskler:

- [x] Local tool install komutu dokumante edilecek.
- [x] Global tool install komutu dokumante edilecek.
- [x] Update/uninstall komutlari dokumante edilecek.
- [x] Windows PowerShell smoke komutlari eklenecek.
- [x] Linux/macOS shell smoke komutlari eklenecek.
- [x] Paket icinde logo dosyasi var mi pack smoke ile dogrulanacak.

Kabul kriterleri:

- Kullanici `dotnet tool install` ile CLI kurabilir.
- `karakol version` package metadata'dan versiyon okur.

## Faz 3 - Serilog Gercek Entegrasyonu

### Sprint 3.1 - Paket ve Composition

Taskler:

- [x] `Serilog` merkezi package management'a eklenecek.
- [x] `Serilog.Extensions.Logging` eklenecek.
- [x] `Serilog.Sinks.Console` eklenecek.
- [x] Opsiyonel file sink gerekecekse `Serilog.Sinks.File` degerlendirilecek.
- [x] `KarakolRuntime.ConfigureServices` logging provider'i Serilog'a baglayacak.
- [x] CLI startup Serilog lifecycle'i kapatirken flush edecek.

Kabul kriterleri:

- Debug yerine `ILogger` kullanilir.
- Logging provider gercekten Serilog olur.
- Build/test yesil kalir.

### Sprint 3.2 - Application Logging Behavior

Taskler:

- [x] `LoggingBehavior<TRequest,TResponse>` constructor ile `ILogger` alacak.
- [x] Request start/completed eventleri structured log olarak yazilacak.
- [x] Duration bilgisi `PerformanceBehavior` ile tutarli olacak.
- [x] Hata durumlari stack trace sizmadan controlled loglanacak.
- [x] Sensitive command alanlari loglanmayacak.

Kabul kriterleri:

- `ScanLogFileCommand.FilePath` gerekirse sadece path olarak, secret alanlar loglanmadan yazilir.
- Config/policy icerigi loglanmaz.
- Testlerle sensitive value leakage kontrol edilir.

### Sprint 3.3 - CLI Verbose/Quiet Semantics

Taskler:

- [x] `--verbose` diagnostic log seviyesini artiracak.
- [x] `--quiet` terminal summary'yi bastirirken error output'u koruyacak.
- [x] Default mod sade kalacak.
- [x] Log seviyeleri dokumante edilecek.
- [x] Integration testlerde quiet/verbose behavior korunacak.

Kabul kriterleri:

- Quiet mode scan sonucunda gereksiz stdout uretmez.
- Verbose mode debugging icin yeterli context verir.

## Faz 4 - DuckDB Local OLAP Persistence

### Sprint 4.1 - Proje ve Abstraction

Taskler:

- [x] `src/Karakol.Persistence.DuckDb/` projesi eklenecek.
- [x] `tests/Karakol.Persistence.DuckDb.Tests/` projesi eklenecek.
- [x] `DuckDB.NET.Data.Full` package merkezi package management'a eklenecek.
- [x] DuckDB provider mevcut `IScanHistoryRepository` abstraction'ina uyacak.
- [x] Raw log storage default olarak yasaklanacak.
- [x] Provider sadece `Persistence.Enabled=true` ve `Provider=DuckDB` ise secilecek.

Kabul kriterleri:

- Core scan DuckDB olmadan calisir.
- DuckDB provider opt-in olur.
- Domain katmani DuckDB referansi almaz.

### Sprint 4.2 - Schema ve Migration

Taskler:

- [x] `scan_sessions` tablosu tasarlanacak.
- [x] `findings` tablosu tasarlanacak.
- [x] `finding_evidence` tablosu tasarlanacak.
- [x] `timeline_buckets` tablosu tasarlanacak.
- [x] Basit migration runner yazilacak.
- [x] Schema version tablosu eklenecek.

Kabul kriterleri:

- Yeni DuckDB dosyasinda schema otomatik olusur.
- Migration idempotent calisir.
- Testler temp DuckDB dosyasi kullanir.

### Sprint 4.3 - Report Projection ve Save

Taskler:

- [x] `ScanReport` DuckDB fact modellerine project edilecek.
- [x] `SaveAsync(ScanReport report)` DuckDB'ye yazacak.
- [x] Target resource ve evidence value maskelenmis kaydedilecek.
- [x] Duplicate scan id davranisi tanimlanacak.
- [x] Persistence hatasi scan'i fail etmeli mi yoksa warning mi olmali karari dokumante edilecek.

Kabul kriterleri:

- Bir scan raporu DuckDB'ye yazilabilir.
- Raw log satirlari kaydedilmez.
- Sensitive output masking korunur.

### Sprint 4.4 - Analytics Queries

Taskler:

- [x] Top source IPs across scans query eklenecek.
- [x] Category trend query eklenecek.
- [x] Severity trend query eklenecek.
- [x] Parser failure ratio query eklenecek.
- [x] Repeated sensitive path access query eklenecek.
- [x] Query DTO'lari Contracts/Application yuzeyinde netlesecek.

Kabul kriterleri:

- DuckDB analytics query testleri geciyor.
- CLI/dashboard future integration icin contract hazir.

### Sprint 4.5 - CLI Persistence Flag

Taskler:

- [x] `--persist-history` scan command'a baglanacak.
- [x] Config `Persistence.Enabled` gercek provider secimini etkiler.
- [x] `karakol doctor` DuckDB dosya path yazilabilirligini kontrol eder.
- [x] Persistence disabled iken disk state yazilmadigi test edilir.

Kabul kriterleri:

- Persistence default disabled.
- DuckDB enabled scan local `.duckdb` dosyasi olusturur.

## Faz 5 - Parser Hardening

### Sprint 5.1 - Fixture Strategy

Taskler:

- [x] `samples/fixtures/` klasor yapisi tasarlanacak.
- [x] Nginx edge-case fixture dosyasi eklenecek.
- [x] Apache edge-case fixture dosyasi eklenecek.
- [x] SSH/Auth edge-case fixture dosyasi eklenecek.
- [x] JSON/CSV edge-case fixture dosyalari eklenecek.
- [x] Fixture dosyalarinda gercek IP/token/customer data olmayacak.

Kabul kriterleri:

- Fixture'lar open source olarak guvenle yayinlanabilir.
- Her fixture icin beklenen parser sonucu test edilir.

### Sprint 5.2 - Access Log Edge Cases

Taskler:

- [x] IPv6 source IP parse testi.
- [x] Missing referrer parse testi.
- [x] Missing user-agent parse testi.
- [x] Response size `-` parse testi.
- [x] Escaped quote URL parse testi.
- [x] Query string with spaces/encoded chars parse testi.
- [x] Malformed timestamp parse error testi.

Kabul kriterleri:

- Parser crash etmez.
- Invalid line anlamli parse error uretir.

### Sprint 5.3 - Structured Logs

Taskler:

- [x] JSON array multi-event testleri.
- [x] JSON line missing optional fields testleri.
- [x] CSV quoted comma fields testleri.
- [x] CSV alias mapping testleri.
- [x] Unknown columns metadata'ya alinacak mi karari verilecek.

Kabul kriterleri:

- JSON/CSV parser real-world varyasyonlara toleransli olur.

## Faz 6 - Detection Quality ve False Positive Azaltma

### Sprint 6.1 - Rule Corpus

Taskler:

- [x] `tests/TestData/rules/` klasoru eklenecek.
- [x] Her rule icin malicious corpus eklenecek.
- [x] Her rule icin benign corpus eklenecek.
- [x] Encoded payload corpus eklenecek.
- [x] Double-encoded payload corpus eklenecek.

Kabul kriterleri:

- Corpus test runner her rule icin calisir.
- False positive case'leri regression altina alinir.

### Sprint 6.2 - Recommended Actions ve Evidence Standard

Taskler:

- [x] Her category icin recommended action metni gozden gecirilecek.
- [x] Evidence field naming standardize edilecek.
- [x] Empty evidence yasaklanacak veya acikca gerekcelendirilecek.
- [x] DetectorId naming convention dokumante edilecek.

Kabul kriterleri:

- Finding okunabilir ve aksiyon alinabilir olur.

### Sprint 6.3 - Rule Options

Taskler:

- [x] Per-rule enabled/disabled options modeli eklenecek.
- [x] Severity override modeli tasarlanacak.
- [x] Pattern group override degerlendirilecek.
- [x] Policy ve config interaction dokumante edilecek.

Kabul kriterleri:

- Kurumlar rule davranisini kod degistirmeden ayarlayabilir.

## Faz 7 - Performance ve Memory Rigor

### Sprint 7.1 - Large Log Smoke

Taskler:

- [x] 1M line generated fixture test utility eklenecek.
- [x] Test kategori olarak optional tutulacak.
- [x] `MaxLines` 1M fixture uzerinde dogrulanacak.
- [x] Elapsed time budget dokumante edilecek.

Kabul kriterleri:

- Large smoke manuel calistirilabilir.
- CI default suite asiri yavaslamaz.

### Sprint 7.2 - Memory Measurement

Taskler:

- [x] Peak memory olcumu icin test helper eklenecek.
- [x] Streaming reader memory ceiling test edilecek.
- [x] Correlation bounded state memory behavior test edilecek.
- [x] Report generation memory impact dokumante edilecek.

Kabul kriterleri:

- Buyuk dosya isleme icin memory davranisi savunulabilir olur.

### Sprint 7.3 - BenchmarkDotNet Hazirligi

Taskler:

- [x] `tests/Karakol.Benchmarks/` veya `benchmarks/` projesi tasarlanacak.
- [x] Parser benchmarklari eklenecek.
- [x] Detection rule benchmarklari eklenecek.
- [x] End-to-end scan benchmark eklenecek.

Kabul kriterleri:

- Performance degisiklikleri olculebilir hale gelir.

## Faz 8 - Reporting UX 10/10

### Sprint 8.1 - HTML Report Refinement

Taskler:

- [x] Current HTML report UI review edilecek.
- [x] Print stylesheet eklenecek.
- [x] Severity colors accessibility acisindan kontrol edilecek.
- [x] Mobile layout kontrol edilecek.
- [x] Empty state'ler test edilecek.
- [x] Large findings table icin okunabilirlik iyilestirilecek.

Kabul kriterleri:

- HTML report executive ve engineer profiline birlikte hitap eder.
- Offline single-file garanti korunur.

### Sprint 8.2 - Report Snapshot Tests

Taskler:

- [x] HTML section assertions genisletilecek.
- [x] JSON schema assertions genisletilecek.
- [x] Markdown report assertions genisletilecek.
- [x] Logo fallback behavior test edilecek.

Kabul kriterleri:

- Rapor UI accidental regression'a karsi korunur.

## Faz 9 - Config ve Policy Hardening

### Sprint 9.1 - Schema Validation

Taskler:

- [x] Unknown severity controlled validation error.
- [x] Unknown report format controlled validation error.
- [x] Invalid output directory controlled validation error.
- [x] Invalid policy JSON controlled validation error.
- [x] Config examples integration test ile dogrulanacak.

Kabul kriterleri:

- Kullanici hatalari stack trace'e donusmez.

### Sprint 9.2 - JSON Schema Files

Taskler:

- [x] `schemas/karakolsettings.schema.json` eklenecek.
- [x] `schemas/karakol.policy.schema.json` eklenecek.
- [x] Docs bu schema dosyalarina link verecek.
- [x] Example config schema ile uyumlu olacak.

Kabul kriterleri:

- Editor/IDE schema support mumkun olur.

## Faz 10 - Final Open Source Release Candidate

### Sprint 10.1 - Cross Platform Verification

Taskler:

- [x] Windows build/test/sample scan.
- [x] Ubuntu build/test/sample scan CI matrix ile kapsanacak.
- [x] macOS build/test/sample scan CI matrix ile kapsanacak.
- [x] Path separator davranisi kontrol edilecek.
- [x] HTML report output cross-platform dogrulanacak.

Kabul kriterleri:

- CLI cross-platform temel kullanima hazir olur.

### Sprint 10.2 - Release Candidate Checklist

Taskler:

- [x] `CHANGELOG.md` guncellenecek.
- [x] Version bump yapilacak.
- [x] NuGet package uretilecek.
- [x] Local/global tool smoke yapilacak.
- [x] Docs linkleri kontrol edilecek.
- [x] Known limitations listesi guncellenecek.

Kabul kriterleri:

- `v0.1.0` veya belirlenen release tag'i yayinlanabilir.

## Siradaki Uygulama Sirasi

Onerilen uygulama sirasi:

1. Faz 0
2. Faz 1
3. Faz 2
4. Faz 3
5. Faz 4
6. Faz 5
7. Faz 6
8. Faz 7
9. Faz 8
10. Faz 9
11. Faz 10

Kritik not: DuckDB ve Serilog gibi yeni dependency iceren fazlarda restore/network ihtiyaci dogabilir. Bu fazlar uygulanirken package ekleme ve restore adimlari ayrica dogrulanmalidir.
