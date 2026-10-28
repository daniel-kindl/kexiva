# 0006. Testing: xUnit v3 on Microsoft.Testing.Platform

- Status: Accepted
- Date: 2026-10-28

## Context

ADR 0003 selected xUnit v2, `Microsoft.NET.Test.Sdk`, and `coverlet.collector` (VSTest). The .NET 10 SDK runs `dotnet test` through Microsoft.Testing.Platform for projects that use that runner. xUnit v3 is built on Microsoft.Testing.Platform. This record supersedes ADR 0003.

## Decision

- Use `xunit.v3` 4.0.2. `global.json` sets `test.runner` to `Microsoft.Testing.Platform`.
- Use `coverlet.MTP` 10.1.0 for Cobertura coverage. Do not use `Microsoft.NET.Test.Sdk`, `xunit.runner.visualstudio`, or `coverlet.collector`.
- `tests/Kexiva.Tests`: unit, architecture, and verification policy tests.
- `tests/Kexiva.Compatibility.Tests`: public API and behavior compatibility.
- Every public operation gets differential tests that compare Kexiva output with the .NET API for the same input, including failure cases.
- `eng/Kexiva.Verify` requires at least 80% line and 70% branch coverage of production code when coverable code exists. It reads Cobertura reports named `coverage.cobertura*.xml`.

## Consequences

- Behavior differences from .NET are bugs, unless an ADR documents them.
- Property-based, fuzz, and mutation testing are planned. They are not part of M0.
- `eng/coverage.runsettings` is removed. Coverage filters are arguments of `eng/verify.sh` and `eng/verify.ps1`.
