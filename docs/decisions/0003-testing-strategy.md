# 0003. Testing: xUnit, differential tests, coverage gate

- Status: Accepted
- Date: 2026-10-14

## Context

Correctness means identical behavior to the .NET API that Kexiva replaces. Coverage alone does not prove this.

## Decision

- Use xUnit v2 with `Microsoft.NET.Test.Sdk` and `coverlet.collector` (VSTest). A move to xUnit v3 on Microsoft.Testing.Platform needs its own issue and ADR.
- `tests/Kexiva.Tests`: unit, architecture, and verification policy tests.
- `tests/Kexiva.Compatibility.Tests`: public API and behavior compatibility.
- Every public operation gets differential tests that compare Kexiva output with the .NET API for the same input, including failure cases.
- `eng/Kexiva.Verify` requires at least 80% line and 70% branch coverage of production code when coverable code exists.

## Consequences

- Behavior differences from .NET are bugs, unless an ADR documents them.
- Property-based, fuzz, and mutation testing are planned. They are not part of M0.
