# 0012. Integer Parse APIs

- Status: Proposed
- Date: 2026-10-31

## Context

Issue #51 adds `Parse` to the four integer utility types. The compatibility specification requires matching values, exception types, parameter names, and overload signatures.

The existing engines return `false` for both malformed input and overflow. They do not expose a failure category.

## Options considered

- Add a failure status throughout the shared scanner and width policies. This avoids a second scan but changes existing `TryParse` paths.
- Use the existing engine for success and the corresponding .NET `Parse` overload for failure. This keeps the shared parser unchanged but scans failed inputs twice.

## Decision

- Add the eight .NET 10 `Parse` overloads to each existing Kexiva integer type. Record the signatures in `PublicAPI.Unshipped.txt`.
- Preserve the optional style and provider arguments on char and UTF-8 span overloads.
- Check string null before style validation. Null throws `ArgumentNullException` with parameter `s`, even when the style is invalid.
- Validate style before resolving the provider. Invalid style throws `ArgumentException` with parameter `style`.
- Resolve `NumberFormatInfo` once. Use the existing internal parser for successful calls.
- On failure, call the matching .NET `Parse` overload with the same text, style, and captured format info. This selects the required exception without calling the original provider twice.
- Keep `TryParse` unchanged.

## Consequences

Successful span calls use the existing allocation-free engine. Exception paths scan twice and can cost more than .NET alone. Benchmarks measure both successful and failed inputs.

A future failure-status implementation can replace the fallback without changing the public contract. Existing `TryParse` differential tests remain necessary because the fallback could conceal a false-negative parser defect.

The optional overload set requires a method-level suppression of analyzer rule RS0027. The .NET signatures are the external contract. Tests compare parameter types, names, nullability, and optional defaults with the installed runtime.
