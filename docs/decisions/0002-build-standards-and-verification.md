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
- One verification entry point: `eng/verify.sh` with an equivalent `eng/verify.ps1`. CI calls the same scripts. `eng/Kexiva.Verify` fails if the scripts run different command sequences, if CI does not cover the four platforms, or if build policies are removed.

## Consequences

- Every analyzer warning blocks the build. An SDK update can break the build; Dependabot SDK pull requests show this in CI.
- Two scripts must stay equivalent. `Kexiva.Verify` enforces the dotnet command sequence.
