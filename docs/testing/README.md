# Testing

The testing rules are in [docs/standards/testing.md](../standards/testing.md). The runner decision is [ADR 0006](../decisions/0006-testing-xunit-v3-on-microsoft-testing-platform.md). Property, fuzz, and mutation tooling is [ADR 0007](../decisions/0007-property-fuzz-and-mutation-tooling.md).

## Run the tests

```bash
./eng/verify.sh          # Linux and macOS
pwsh ./eng/verify.ps1    # Windows
```

The scripts run locked restore (with NuGet audit), Release build with warnings as errors, format checks, unit and architecture tests with coverage, compatibility tests, and the `Kexiva.Verify` policy and coverage checks. Results go to `TestResults/verification-*`.

For a quick loop:

```bash
dotnet test tests/Kexiva.Tests/Kexiva.Tests.csproj
```

## Test projects

| Project | Contains |
| --- | --- |
| `tests/Kexiva.Tests` | Unit, architecture (`ArchitectureTests`), verification policy tests |
| `tests/Kexiva.Compatibility.Tests` | Public API and behavior compatibility |

Integer parsing and formatting differential tests follow [Integer parsing and formatting](../compatibility/integer-parsing-and-formatting.md). Compare each Kexiva method with the reference method on the same arguments.

## Platforms

CI runs the verify scripts on Linux x64, Windows x64, macOS ARM64, and macOS x64. Mark any platform that you did not run as NOT RUN in the pull request.
