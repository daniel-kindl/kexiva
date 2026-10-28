# Dependencies

Versions of libraries are in `Directory.Packages.props`. The Stryker CLI version is in `.config/dotnet-tools.json`. Lock files are `**/packages.lock.json`.

## Installed

| Package | Version | License | Used by |
| --- | --- | --- | --- |
| Nerdbank.GitVersioning | 3.10.94 | MIT | build |
| Microsoft.CodeAnalysis.PublicApiAnalyzers | 5.6.0 | MIT | `src/Kexiva` |
| BenchmarkDotNet | 0.15.8 | MIT | benchmarks |
| xunit.v3 | 4.0.2 | Apache-2.0 | tests |
| coverlet.MTP | 10.1.0 | MIT | unit and compatibility tests |
| CsCheck | 5.0.0 | Apache-2.0 | property tests |
| dotnet-stryker | 5.0.0 | Apache-2.0 | nightly mutation tool |

Transitive packages are in the lock files.

## Vulnerability checks

- NuGet audit on restore (`NuGetAuditMode` is `all`, level `low`). Warnings are errors.
- GitHub dependency review on pull requests.
- Dependabot updates NuGet, GitHub Actions, and the .NET SDK. It targets `dev`.
