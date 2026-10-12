# Testing Standards

## Unit Tests
- xUnit, deterministic, isolated.
- Every public operation requires comprehensive tests.
- Every bug fix requires a regression test.

## Differential Tests
Future Kexiva operations must be tested against standard .NET implementations.

## Property-based / Fuzz / Mutation
Planned: FsCheck or equivalent, seed-corpus for fuzzing, Stryker.NET for mutation.

## Architecture Tests
Enforce dependency boundaries and prevent accidental public API expansion.

## Compatibility Tests
Protect public API signatures and behavior.

## Coverage
Track branch coverage for critical algorithms. No arbitrary 100% requirement.
No flaky tests. Do not weaken tests to pass CI.
