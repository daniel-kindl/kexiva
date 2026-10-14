# 0002. Build standards and single verification script

- Status: Accepted
- Date: 2026-10-14

## Context

Kexiva promises the same API and the same behavior as .NET. The build must be deterministic and verifiable on Linux, Windows, and macOS by humans, agents, and CI.

## Decision

- Pin the SDK in `global.json` (`10.0.402`, `rollForward: latestPatch`, no prerelease).
- Central Package Management with exact versions, transitive pinning, and committed `packages.lock.json` files. CI restores in locked mode.
- NuGet audit on every restore (`NuGetAuditMode=all`, `NuGetAuditLevel=low`). Audit warnings are errors.
- `AnalysisMode=All`, `TreatWarningsAsErrors`, `MSBuildTreatWarningsAsErrors`, `EnforceCodeStyleInBuild`. Style rules are in `.editorconfig`. Each suppression has a reason in `.globalconfig`.
- The Public API Analyzer guards `PublicAPI.Shipped.txt` and `PublicAPI.Unshipped.txt`.
- One verification entry point: `eng/verify.sh` with an equivalent `eng/verify.ps1`. CI calls the same scripts.
- `eng/Kexiva.Verify` enforces these rules on every run and fails if any is missing:
  - both scripts run the same dotnet command sequence and contain no placeholder markers;
  - `ci.yml` uses the runners `ubuntu-24.04`, `windows-2025`, `macos-15`, and `macos-15-intel` and calls both scripts;
  - every workflow `dotnet restore` uses `--locked-mode` and every `dotnet build` uses `--warnaserror`;
  - `Directory.Build.props` sets `TreatWarningsAsErrors`, `MSBuildTreatWarningsAsErrors`, `EnableNETAnalyzers`, `EnforceCodeStyleInBuild`, `AnalysisMode=All`, `RestorePackagesWithLockFile`, `NuGetAudit`, `NuGetAuditMode=all`, `NuGetAuditLevel=low`, and, in CI, `RestoreLockedMode` and `ContinuousIntegrationBuild`;
  - package versions are central and fixed, and every project has a committed lock file;
  - production line and branch coverage thresholds.
- The SDK pin and the `.editorconfig`/`.globalconfig` rules are enforced by the SDK and the build, not by `Kexiva.Verify`.

## Consequences

- Every analyzer warning blocks the build. An SDK update can break the build; Dependabot SDK pull requests show this in CI.
- Two scripts must stay equivalent. `Kexiva.Verify` enforces the dotnet command sequence; non-dotnet lines (prerequisite checks) are reviewed by hand.
