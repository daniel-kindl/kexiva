# 0010. Int64 TryParse API

- Status: Accepted
- Date: 2026-10-29

## Context

Issue #49 adds signed `Int64` `TryParse` after `Int32` `TryParse`. `Parse`, formatting, and `UInt64` stay in later issues.

The public methods must take the same arguments as `System.Int64.TryParse` and return the same result. A negative value is valid through `long.MinValue`. Hex and binary results use the two's-complement bit pattern.

## Decision

- The declaring type is `Kexiva.Int64`. The name suppresses CA1720 because it matches `System.Int64`.
- Ship the nine `TryParse` overloads that `System.Int64` provides.
- Record the signatures in `PublicAPI.Unshipped.txt`.
- A valid style that fails returns `false` and sets the result to 0.
- An invalid style throws `ArgumentException`. The parameter name is `style`.
- `IntegerParser` keeps the shared scan. The decimal digit loop for `long` stays separate from the 32-bit loops. `Int64` overflow rules live on `Int64Policy`.

## Consequences

- Callers write `Kexiva.Int64.TryParse`.
- A later `UInt64` issue can follow the same type name and can add its own overflow policy.
- Formatting needs its own issue and its own public methods.
