# Karakol Kalan İşler, Fazlar ve Sprint Planı

Bu dokuman Karakol'un mevcut MVP durumundan ticari urun kalitesine ilerlemesi icin kalan isleri fazlara, sprintlere, task listelerine ve kabul kriterlerine boler.

Bu plan mevcut repo durumuna gore hazirlanmistir:

- Solution build aliyor.
- `karakol scan` akisi calisiyor.
- Nginx sample log uzerinden detection, risk, JSON ve HTML rapor uretimi calisiyor.
- Kod folder structure buyuk olcude ayrildi.
- Test projeleri var ve `dotnet test` gercek suite calistiriyor.
- MediatR, FluentValidation ve Spectre.Console.Cli gercek akisa baglandi.
- Serilog henuz gercek entegrasyon olarak yok.
- Config ve policy parametreleri temel scan davranisina baglandi.
- Persistence, plugin ve ML kisimlari MVP icin abstraction/placeholder seviyesinde; built-in plugin registry runtime composition'a baglandi.

## Mevcut Durum

### Calisanlar

- `.NET 9` solution mevcut.
- `dotnet build` 0 warning / 0 error ile geciyor.
- CLI bootstrap, command, renderer ve exit-code mapping klasorlerine ayrildi.
- Domain modelleri `Enums`, `Events`, `Findings`, `Risk`, `Scans`, `Sources`, `ValueObjects` altinda ayrik dosyalarda.
- Parsing katmani abstraction, input, detection, registry, parser ve result klasorlerine ayrildi.
- Detection katmani rule abstraction, registry, engine ve rule ailelerine ayrildi.
- Correlation, risk, reporting, persistence, ML ve plugin projeleri temel folder structure'a sahip.
- Nginx sample scan su komutla calisiyor:

```bash
dotnet run --project src/Karakol.Cli -- scan samples/nginx/nginx-access.log --format nginx --report html --report json
```

### Eksikler

- Serilog entegrasyonu yok.
- `karakolsettings.json` ve `karakol.policy.json` temel davranisa bagli; gelismis schema ve dokumantasyon Faz 8'de iyilestirilecek.
- Basic policy engine var.
- Parser confidence ve parser failure ratio report metadata'ya yansiyor.
- Correlation bounded window yaklasimina tasindi; ileri optimizasyonlar post-MVP performans calismasi olarak kalacak.
- Report JSON cikisi explicit report DTO projection uzerinden stabil contract olarak uretiliyor.
- Plugin registry runtime composition icin built-in kaynak olarak kullaniliyor.
- ML feature extractor ve ONNX provider arayuzu hazir; gercek ONNX inference post-MVP.
- Paketleme metadata'si ve release gate dokumani hazir.

## Önceliklendirme İlkeleri

1. Calisan scan komutu her sprint sonunda korunacak.
2. Once test omurgasi kurulacak; sonra buyuk entegrasyonlara gecilecek.
3. Domain katmani framework bagimsiz kalacak.
4. CLI katmani is kurali icermeyecek.
5. Local-first garanti bozulmayacak.
6. Her sprint sonunda `dotnet build`, `dotnet test` ve ana scan komutu calistirilacak.
7. External API, cloud sync, telemetry ve online model download kapsam disi kalacak.
8. Yeni ozellikler once abstraction ve testle korunacak, sonra implementation eklenecek.

## Faz 0 - Stabilizasyon ve Mimari Borç Temizliği

### Sprint 0.1 - Repo Hijyeni ve Mimari Netlik

Taskler:

- [x] `bin/`, `obj/`, `reports/` ciktilarinin git disinda kaldigi dogrulanacak.
- [x] `.gitignore` build ve report artifact'lerini kapsayacak sekilde gozden gecirilecek.
- [x] Solution reference graph dokumante edilecek.
- [x] `Application` katmaninin `Infrastructure` concrete class'larina dogrudan bagimliligi azaltilacak.
- [x] `FileMetadataProvider` icin Application abstraction eklenecek.
- [x] `Infrastructure` sadece Application abstraction implement edecek.
- [x] `KarakolRuntime` gecici composition root olarak isaretlenecek.
- [x] Gelecekte DI container'a gecis icin `Runtime` klasorune teknik not eklenecek.
- [x] `docs/architecture/architecture.md` guncel folder structure ile senkron tutulacak.

Done Criteria:

- [x] `dotnet build` 0 warning / 0 error.
- [x] `rg --files src -g '!**/bin/**' -g '!**/obj/**'` icinde `Core.cs`, `Models.cs`, `Contracts.cs` benzeri catch-all dosya kalmayacak.
- [x] Architecture doc, guncel klasor yapisini dogru anlatacak.
- [x] Ana scan komutu ayni sekilde calisacak.

### Sprint 0.2 - Kod Standartlari ve Naming

Taskler:

- [x] Namespace'ler folder structure ile tutarli hale getirilecek.
- [x] Public class ve interface isimleri domain diline gore gozden gecirilecek.
- [x] Internal helper class'lar public API yuzeyinden ayrilacak.
- [x] Magic string'ler constants veya options altina tasinacak.
- [x] Rule ID ve report format stringleri merkezi hale getirilecek.
- [x] `Directory.Build.props` kalite ayarlari gozden gecirilecek.

Done Criteria:

- [x] Yeni gelistirici proje klasorlerinden sorumluluklari anlayabilecek.
- [x] Build temiz kalacak.

## Faz 1 - Test Omurgası ve Kalite Kapıları

### Sprint 1.1 - Test Projeleri

Taskler:

- [x] `tests/` klasoru olusturulacak.
- [x] `Karakol.Domain.Tests` projesi olusturulacak.
- [x] `Karakol.Parsing.Tests` projesi olusturulacak.
- [x] `Karakol.Detection.Tests` projesi olusturulacak.
- [x] `Karakol.Risk.Tests` projesi olusturulacak.
- [x] `Karakol.Reporting.Tests` projesi olusturulacak.
- [x] `Karakol.Integration.Tests` projesi olusturulacak.
- [x] Test projeleri solution'a eklenecek.
- [x] xUnit, FluentAssertions ve Microsoft.NET.Test.Sdk central package management ile eklenecek.
- [x] Test projeleri dogru production project reference'larini alacak.

Done Criteria:

- [x] `dotnet test` gercek test assembly'leri bulacak.
- [x] En az bir smoke test calisacak.

### Sprint 1.2 - Kritik Unit Testler

Taskler:

- [x] `RiskScore` value range testleri yazilacak.
- [x] `RiskScore` severity mapping testleri yazilacak.
- [x] `SecurityEvent` required field invariant testleri yazilacak.
- [x] `DetectionFinding` confidence clamp ve required field testleri yazilacak.
- [x] Nginx valid line parse testi yazilacak.
- [x] Nginx invalid line parse error testi yazilacak.
- [x] Apache common/combined parse testleri yazilacak.
- [x] SSH failed password parse testi yazilacak.
- [x] SSH accepted password parse testi yazilacak.
- [x] JSON Lines parse testi yazilacak.
- [x] CSV parse testi yazilacak.
- [x] SQLi malicious ve benign testleri yazilacak.
- [x] XSS malicious ve benign testleri yazilacak.
- [x] Path traversal encoded ve decoded testleri yazilacak.
- [x] Scanner bot path ve user-agent testleri yazilacak.
- [x] Sensitive file access testleri yazilacak.
- [x] Command injection testleri yazilacak.
- [x] Sensitive masker testleri yazilacak.
- [x] HTML encoding assertion testleri yazilacak.

Done Criteria:

- [x] En az 30 unit test geciyor olacak.
- [x] False positive icin en az 5 benign test bulunacak.

### Sprint 1.3 - Integration Tests

Taskler:

- [x] Nginx sample log ile full scan integration testi yazilacak.
- [x] JSON report file olusumu dogrulanacak.
- [x] HTML report file olusumu dogrulanacak.
- [x] SQLi, XSS, path traversal, scanner bot ve sensitive file category count dogrulanacak.
- [x] CLI process execution integration testi eklenecek.
- [x] Exit code success testi eklenecek.
- [x] Missing file exit code testi eklenecek.

Done Criteria:

- [x] `dotnet test` en az 40 anlamli test calistiracak.
- [x] Ana scan regression test ile korunacak.

## Faz 2 - Gerçek CQRS, DI ve CLI Framework Entegrasyonu

### Sprint 2.1 - MediatR ve FluentValidation

Taskler:

- [x] `MediatR` paketi eklenecek.
- [x] `FluentValidation` paketi eklenecek.
- [x] `ScanLogFileCommand` MediatR request modeline donusturulecek.
- [x] `ScanLogFileCommandHandler` MediatR handler olacak.
- [x] `ScanLogFileValidator` FluentValidation validator olacak.
- [x] Validation failure'lar `Result<T>` error modeline map edilecek.
- [x] Handler integration testleri MediatR baglaminda guncellenecek.

Done Criteria:

- [x] CLI scan akisi MediatR uzerinden calisacak.
- [x] Validation hatalari controlled Result error donecek.

### Sprint 2.2 - Pipeline Behaviors

Taskler:

- [x] `ValidationBehavior<TRequest,TResponse>` eklenecek.
- [x] `LoggingBehavior<TRequest,TResponse>` eklenecek.
- [x] `PerformanceBehavior<TRequest,TResponse>` eklenecek.
- [x] `ExceptionToResultBehavior<TRequest,TResponse>` eklenecek.
- [x] `CancellationBehavior<TRequest,TResponse>` eklenecek.
- [x] Behavior ordering runtime registration ile belirlenecek.
- [x] Exception mapping icin ayrik unit test eklenecek.

Done Criteria:

- [x] Beklenmeyen exception CLI'a stack trace olarak sizmayacak.
- [x] Validation pipeline handler'dan once calisacak.

### Sprint 2.3 - Spectre.Console.Cli

Taskler:

- [x] `Spectre.Console` paketi eklenecek.
- [x] `Spectre.Console.Cli` paketi eklenecek.
- [x] Manual arg parse azaltip Spectre command settings modellerine gecilecek.
- [x] `scan` command Spectre command olacak.
- [x] `rules` command Spectre command olacak.
- [x] `formats` command Spectre command olacak.
- [x] `doctor` command Spectre command olacak.
- [x] `version` command Spectre command olacak.
- [x] Terminal summary table/panel formatina tasinacak.

Done Criteria:

- [x] CLI ayni komut uyumlulugunu koruyacak.
- [x] CLI katmani is kurali icermeyecek.
- [x] Output daha profesyonel ve okunabilir olacak.

## Faz 3 - Config, Policy ve Security Hardening

### Sprint 3.1 - Config Loading

Taskler:

- [x] `karakolsettings.json` modeli tanimlanacak.
- [x] `ScanningOptions` eklenecek.
- [x] `RuleOptions` config modeli ile ayrilacak.
- [x] `MlOptions` eklenecek.
- [x] `ReportingOptions` eklenecek.
- [x] `PersistenceOptions` eklenecek.
- [x] Defaults tek noktada tanimlanacak.
- [x] CLI `--config` gercek provider'a baglanacak.
- [x] Invalid config controlled error donecek.

Done Criteria:

- [x] Config yokken defaults calisacak.
- [x] Config varken output directory, max lines ve default report formats davranisi degisecek.

### Sprint 3.2 - Policy Engine

Taskler:

- [x] `karakol.policy.json` modeli tanimlanacak.
- [x] `IPolicyProvider` eklenecek.
- [x] `PolicyContext` domain/application yuzeyinde tanimlanacak.
- [x] `enabledRules` ve `disabledRules` rule execution'a baglanacak.
- [x] `minimumSeverity` severity filter'a baglanacak.
- [x] `trustedIps` risk modifier'a baglanacak.
- [x] `ignoredPaths` preprocessing/detection filtresine baglanacak.
- [x] `sensitivePaths` risk scoring'e baglanacak.

Done Criteria:

- [x] `--policy` opsiyonu gercek davranisi degistiriyor olacak.
- [x] Disabled rule testi gececek.
- [x] Trusted IP risk reduction testi gececek.

### Sprint 3.3 - Security Hardening

Taskler:

- [x] Output path normalize edilecek.
- [x] Output directory file collision riskine karsi kontrol eklenecek.
- [x] HTML encode coverage testlerle genisletilecek.
- [x] Sensitive masker pattern listesi genisletilecek.
- [x] Raw samples default kapali tutulacak.
- [x] Report icinde raw values mask/sanitize pipeline'ina sokulacak.
- [x] Authorization header ve long token masking testleri eklenecek.

Done Criteria:

- [x] HTML report XSS payload'i execute edilebilir bicimde tasimayacak.
- [x] Sensitive query parameters maskelenmis olacak.

## Faz 4 - Parser ve Detection Ürünleştirme

### Sprint 4.1 - Parser Hardening

Taskler:

- [x] Access log regex edge case'leri genisletilecek.
- [x] Nginx ve Apache parser common mapper daha net ayrilacak.
- [x] JSON Lines yaninda single-line JSON array support eklenecek.
- [x] CSV quoted field ve yaygin kolon alias support eklenecek.
- [x] CSV mapping config eklenecek.
- [x] Auth parser ve SSH parser ayrimi netlestirilecek.
- [x] Format detection confidence report metadata'ya eklenecek.
- [x] Parser statistics summary ve report metadata'ya eklenecek.

Done Criteria:

- [x] Parser failure ratio report metadata'da gorunecek.
- [x] Format detection confidence JSON report'ta yer alacak.

### Sprint 4.2 - Detection Rule Kalitesi

Taskler:

- [x] Rule pattern listeleri constants/options dosyalarina tasinacak.
- [x] Rule metadata standardize edilecek.
- [x] Evidence generation ortak base rule uzerinde standardize edilecek.
- [x] Recommended action metinleri kategoriye gore iyilestirilecek.
- [x] False positive azaltma testleri eklenecek.
- [x] Normal URL'lerin SQLi/XSS olarak isaretlenmemesi garanti altina alinacak.
- [x] Encoded ve double-encoded payload testleri genisletilecek.

Done Criteria:

- [x] Her rule icin malicious ve temel benign test olacak.
- [x] Evidence field/value/matchedPattern her finding'de tutarli olacak.

## Faz 5 - Correlation, Risk ve Performance

### Sprint 5.1 - Bounded Correlation

Taskler:

- [x] Full event list kullanimi azaltacak bounded event window tasarlanacak.
- [x] `EventWindow` modeli eklenecek.
- [x] Source IP aggregate state eklenecek.
- [x] Brute force rule time window bazli calisacak.
- [x] DoS rule time window bazli calisacak.
- [x] Related event reference limitleri uygulanacak.
- [x] Old bucket eviction test edilecek.

Done Criteria:

- [x] Correlation memory buyumesi sinirli olacak.
- [x] Threshold ve window testleri gececek.

### Sprint 5.2 - Risk Model İyileştirme

Taskler:

- [x] Risk component breakdown JSON rapora yansitilacak.
- [x] Risk component breakdown HTML rapora okunabilir sekilde yansitilacak.
- [x] Sensitive path modifier policy'den beslenecek.
- [x] Trusted IP modifier policy'den beslenecek.
- [x] Repetition boost testleri yazilacak.
- [x] Category diversity boost testleri yazilacak.
- [x] Scan-level risk formulu testlerle sabitlenecek.

Done Criteria:

- [x] Risk score 100'u gecmeyecek.
- [x] Risk explanation her finding icin anlamli olacak.

### Sprint 5.3 - Performance Smoke

Taskler:

- [x] 100k satir generated log fixture eklenecek.
- [x] Streaming reader tum dosyayi memory'ye almadan okuyacak sekilde test edilecek.
- [x] `MaxLines` behavior testi eklenecek.
- [x] Regex allocation ve compiled regex kullanimi gozden gecirilecek.
- [x] Performance smoke test CI'da optional kategoriye alinacak.

Done Criteria:

- [x] 100k satir smoke testi makul surede tamamlanacak.
- [x] MaxLines parse edilen event sayisini sinirlayacak.

## Faz 6 - Reporting ve Kullanıcı Deneyimi

### Sprint 6.1 - Report DTO ve Schema

Taskler:

- [x] JSON report icin stable contract dokumante edilecek.
- [x] Domain entity serialization yerine explicit report DTO projection eklenecek.
- [x] Report metadata genisletilecek.
- [x] Tool version ve schema version bilgileri eklenecek.
- [x] Report schema backward compatibility notlari icin `schemaVersion` eklenecek.

Done Criteria:

- [x] JSON report dashboard/CI tarafindan guvenle tuketilebilir olacak.
- [x] Report shape testlerle korunacak.

### Sprint 6.2 - HTML Report Kalitesi

Taskler:

- [x] Executive summary bolumu iyilestirilecek.
- [x] Severity distribution bolumu iyilestirilecek.
- [x] Threat category distribution bolumu iyilestirilecek.
- [x] Timeline bolumu eklenecek.
- [x] Top IP, top URL ve top user-agent bolumleri eklenecek.
- [x] Critical findings bolumu eklenecek.
- [x] Evidence details bolumu daha okunabilir hale getirilecek.
- [x] HTML snapshot veya string assertion testleri eklenecek.

Done Criteria:

- [x] HTML report offline tek dosya olarak acilacak.
- [x] HTML report profesyonel dashboard hissi verecek.

### Sprint 6.3 - CLI UX

Taskler:

- [x] Progress indicator eklenecek.
- [x] Verbose mode gercek davranisa baglanacak.
- [x] Quiet mode gercek davranisa baglanacak.
- [x] Error output controlled stderr davranisina baglanacak.
- [x] `rules` ve `formats` outputlari Spectre table olarak gosterilecek.
- [x] Exit code davranislari testlerle korunacak.

Done Criteria:

- [x] CLI output okunabilir, tutarli ve ticari urun hissinde olacak.
- [x] Error mesajlari teknik ama anlasilir olacak.

## Faz 7 - Persistence, Plugin ve ML Hazırlığı

### Sprint 7.1 - Persistence Abstraction Tamamlama

Taskler:

- [x] Disabled repository mevcut default davranis olarak korunacak.
- [x] SQLite implementation post-MVP flag ile hazirlanacak.
- [x] Scan history modelleri gozden gecirilecek.
- [x] Scan history command/query taslaklari eklenecek.
- [x] Persistence enabled false iken hicbir disk state yazilmayacak.

Done Criteria:

- [x] Persistence disabled default olacak.
- [x] Interface testleri ve disabled repo testleri gececek.

### Sprint 7.2 - Plugin Registry

Taskler:

- [x] Built-in plugin runtime composition'a gercek kaynak yapilacak.
- [x] Parser registration tek merkezden yapilacak.
- [x] Rule registration tek merkezden yapilacak.
- [x] Reporter registration tek merkezden yapilacak.
- [x] Runtime duplicate parser/rule listeleri kaldirilacak.
- [x] External dynamic loading MVP disi olarak dokumante edilecek.

Done Criteria:

- [x] Yeni built-in rule eklemek tek registration noktasindan mumkun olacak.
- [x] Runtime composition duplicate liste barindirmayacak.

### Sprint 7.3 - ML Abstraction

Taskler:

- [x] `IEventFeatureExtractor` eklenecek.
- [x] `ThreatModelInput` modeli eklenecek.
- [x] Dummy classifier no-op davranisi netlestirilecek.
- [x] ONNX provider interface'i tasarlanacak.
- [x] Model metadata reader eklenecek.
- [x] Model dosyasi yoksa scan fail etmeyecek.
- [x] ML disabled/default testleri yazilacak.

Done Criteria:

- [x] ML kapali iken scan davranisi degismeyecek.
- [x] ML acik ama model yokken no-op/dummy davranisi testli olacak.

## Faz 8 - Paketleme, Dokümantasyon ve Release Readiness

### Sprint 8.1 - Documentation

Taskler:

- [x] README build/test/run bilgileri guncellenecek.
- [x] `docs/user-guide/cli-usage.md` eklenecek.
- [x] `docs/user-guide/detection-rules.md` eklenecek.
- [x] `docs/security/security-considerations.md` eklenecek.
- [x] `docs/user-guide/supported-log-formats.md` eklenecek.
- [x] `docs/user-guide/risk-scoring.md` eklenecek.
- [x] Bu sprint plani checklist olarak guncel tutulacak.

Done Criteria:

- [x] Yeni gelistirici README ile projeyi calistirabilecek.
- [x] Kullanici CLI komutlarini dokumandan anlayabilecek.

### Sprint 8.2 - Packaging

Taskler:

- [x] `dotnet pack` hazirligi yapilacak.
- [x] Tool manifest/global tool stratejisi belirlenecek.
- [x] Package metadata eklenecek.
- [x] Versioning stratejisi belirlenecek.
- [x] Changelog formati eklenecek.
- [x] Local install smoke testi yazilacak.

Done Criteria:

- [x] CLI paketlenebilir hale gelecek.
- [x] Version bilgisi tek kaynaktan okunacak.

### Sprint 8.3 - Release Gate

Taskler:

- [x] Release checklist eklenecek.
- [x] Build gate tanimlanacak.
- [x] Test gate tanimlanacak.
- [x] Sample scan gate tanimlanacak.
- [x] Report generation gate tanimlanacak.
- [x] Exit code testleri release gate'e alinacak.
- [x] Local-first guarantee release notlarina eklenecek.

Done Criteria:

- [x] Release candidate icin manuel dogrulama listesi hazir olacak.
- [x] Her release oncesi ayni komut setiyle dogrulama yapilabilecek.

## Public API / Interface Değişiklikleri

- [x] `ScanLogFileCommand` MediatR request haline getirilecek.
- [x] File access icin Application abstraction eklenecek.
- [x] Concrete `FileMetadataProvider` Application'dan ayrilacak.
- [x] Config modelleri public Application yuzeyinde netlesecek.
- [x] Policy modelleri public Application yuzeyinde netlesecek.
- [x] Report output icin explicit DTO projection eklenecek veya mevcut Domain serialization bilincli karar olarak dokumante edilecek.
- [x] Plugin registry, built-in parser/rule/reporter registration icin tek kaynak olacak.

## Test Planı

Unit test alanlari:

- [x] Domain invariant tests.
- [x] Parser tests.
- [x] Detection rule tests.
- [x] Risk scoring tests.
- [x] Reporting tests.
- [x] Sensitive masking tests.

Integration test alanlari:

- [x] CLI scan.
- [x] Report creation.
- [x] Config behavior.
- [x] Policy behavior.
- [x] Exit codes.

Security test alanlari:

- [x] HTML encoding.
- [x] Sensitive data masking.
- [x] Output path normalization.
- [x] Raw sample default behavior.

Performance test alanlari:

- [x] 100k satir streaming smoke.
- [x] MaxLines behavior.
- [x] Correlation bounded memory behavior.

Regression komutlari:

```bash
dotnet build
dotnet test
dotnet run --project src/Karakol.Cli -- scan samples/nginx/nginx-access.log --format nginx --report html --report json
```

## Genel Kabul Kriterleri

- [x] `dotnet build` 0 warning / 0 error.
- [x] `dotnet test` gercek test suite calistiriyor.
- [x] Ana scan komutu her sprint sonunda calisiyor.
- [x] JSON ve HTML rapor uretiliyor.
- [x] HTML output encode ediliyor.
- [x] Sensitive data maskeleniyor.
- [x] CLI business logic icermiyor.
- [x] Domain framework bagimsiz kaliyor.
- [x] Parser/rule/reporter ekleme yollari net.
- [x] Config ve policy gercek davranisa bagli.
- [x] Local-first garanti korunuyor.
- [x] External API, telemetry veya cloud bagimliligi yok.

## Riskler ve Notlar

- MediatR, FluentValidation, Spectre.Console.Cli ve Serilog paketleri eklendiginde network/package restore ihtiyaci olabilir.
- Test projeleri eklendiginde `dotnet test` artik gercek kalite kapisi olacak; mevcut basarili sonuc test yoklugundan kaynaklaniyor.
- Config ve policy entegrasyonu rule/risk davranisini degistirecegi icin regression testlerle korunmali.
- Correlation engine su an full event list yaklasimina yakin; buyuk dosya hedefi icin bounded window onceliklidir.
- JSON report contract'i stabil hale getirilmeden dashboard/CI entegrasyonu baslatilmamalidir.
- External dynamic plugin loading MVP disi kalmalidir.
- ML entegrasyonu local model disina cikmamalidir.
