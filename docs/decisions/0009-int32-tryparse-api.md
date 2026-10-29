# 0009. Int32 TryParse API

- Status: Accepted
- Date: 2026-10-28

## Context

Issue #26 adds signed `Int32` `TryParse` after `UInt32` `TryParse`. `Parse`, formatting, `Int64`, and `UInt64` stay in later issues.

The public methods must take the same arguments as `System.Int32.TryParse` and return the same result. A negative value is valid through `int.MinValue`. Hex and binary results use the two's-complement bit pattern.

## Decision

- The declaring type is `Kexiva.Int32`. The name suppresses CA1720 because it matches `System.Int32`.
- Ship the nine `TryParse` overloads that `System.Int32` provides.
- Record the signatures in `PublicAPI.Unshipped.txt`.
- A valid style that fails returns `false` and sets the result to 0.
- An invalid style throws `ArgumentException`. The parameter name is `style`.

## Consequences

- Callers write `Kexiva.Int32.TryParse`.
- Later integer issues can follow the same type name for `Int64` and `UInt64`.
- Formatting needs its own issue and its own public methods.
