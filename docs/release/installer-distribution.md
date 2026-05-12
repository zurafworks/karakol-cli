# Installer and Distribution

Karakol's primary distribution model is a .NET global tool.

## Local Package Smoke

Use this flow before publishing:

```bash
dotnet pack --no-restore src/Karakol.Cli/Karakol.Cli.csproj -o artifacts/packages
dotnet tool install Karakol.Cli --add-source artifacts/packages --tool-path .tmp-karakol-tool
.tmp-karakol-tool/karakol version
```

Windows PowerShell:

```powershell
dotnet pack --no-restore src/Karakol.Cli/Karakol.Cli.csproj -o artifacts/packages
dotnet tool install Karakol.Cli --add-source artifacts/packages --tool-path .tmp-karakol-tool
.\.tmp-karakol-tool\karakol.exe version
```

Remove the temporary tool directory after the smoke test.

## Global Install From NuGet

After public package release:

```bash
dotnet tool install --global Karakol.Cli
karakol version
```

## Update

```bash
dotnet tool update --global Karakol.Cli
karakol version
```

## Uninstall

```bash
dotnet tool uninstall --global Karakol.Cli
```

## Linux/macOS Smoke

```bash
karakol scan samples/nginx/nginx-access.log --format nginx --report html --report json --verbose
ls reports
```

## Windows PowerShell Smoke

```powershell
karakol scan samples/nginx/nginx-access.log --format nginx --report html --report json --verbose
Get-ChildItem reports
```

## Package Contents

The package must include `karakol_logo.png` beside the tool binaries. HTML reports embed this logo as a data URI, so generated reports remain single-file and offline.
