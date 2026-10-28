# 0007. Property, fuzz, and mutation tooling

- Status: Accepted
- Date: 2026-10-28

## Context

ADR 0006 left property-based, fuzz, and mutation testing as planned work. Issue #32 adds that tooling before parsing tests grow. The pull request gate stays `eng/verify.sh` and `eng/verify.ps1`. The test runner is xUnit v3 on Microsoft.Testing.Platform, so a library that needs the xUnit v2 runner does not fit.

## Decision

- Property tests use CsCheck 5.0.0 (Apache-2.0). Pass a fixed `seed` so a run can be repeated. FsCheck is not used. Its xUnit integration targets xUnit v2.
- Fuzzing uses an in-process seed corpus under `tests/Kexiva.Fuzz/corpus`. `eng/fuzz.sh` runs that project. SharpFuzz is not used. It needs libFuzzer or AFL, and the nightly runner does not install that toolchain.
- Mutation testing uses `dotnet-stryker` 5.0.0 (Apache-2.0) with `"test-runner": "mtp"`. The package is a local tool in `.config/dotnet-tools.json`, not a compile reference. `eng/mutate.sh` runs it. While `src/Kexiva` has no mutable production source, the script exits 0 and reports the skip.
- The nightly workflow runs `eng/fuzz.sh` and `eng/mutate.sh`. The pull request gate does not.
- The mutation break threshold is 0 until production code exists. A later issue can raise it.

## Consequences

- Differential tests can take generated integers from CsCheck.
- A parser fuzz target replaces the current corpus delivery check. The corpus files stay.
- Licenses for direct packages and the Stryker tool are in [Dependencies](../development/dependencies.md).
