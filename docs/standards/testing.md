# Testing Standards

## Unit Tests
- xUnit, deterministic, isolated.
- Every public operation requires comprehensive tests.
- Every bug fix requires a regression test.

## Differential Tests
Future Kexiva operations must be tested against standard .NET implementations.

## Property-based / Fuzz / Mutation
- Property tests use CsCheck with a fixed seed. See [ADR 0007](../decisions/0007-property-fuzz-and-mutation-tooling.md).
- Fuzzing uses the seed corpus in `tests/Kexiva.Fuzz/corpus`. `eng/fuzz.sh` runs it in the nightly workflow.
- Mutation testing uses Stryker.NET. `eng/mutate.sh` runs it in the nightly workflow.
- Do not add fuzz or mutation to `eng/verify.sh` or `eng/verify.ps1`.

## Architecture Tests
Enforce dependency boundaries and prevent accidental public API expansion.

## Compatibility Tests
Protect public API signatures and behavior.

## Coverage
Track branch coverage for critical algorithms. No arbitrary 100% requirement.
No flaky tests. Do not weaken tests to pass CI.
