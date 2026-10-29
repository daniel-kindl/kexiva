# 0011. UInt64 TryParse API

- Status: Accepted
- Date: 2026-10-29

## Context

Issue #50 adds unsigned `UInt64` `TryParse` after `Int64` `TryParse`. `Parse` and formatting stay in later issues.

The public methods must take the same arguments as `System.UInt64.TryParse` and return the same result. The minimum value is 0. The maximum value is `18446744073709551615`. Hex and binary results use the two's-complement bit pattern of the full width.

## Decision

- The declaring type is `Kexiva.UInt64`. The name suppresses CA1720 because it matches `System.UInt64`.
- Ship the nine `TryParse` overloads that `System.UInt64` provides.
- Record the signatures in `PublicAPI.Unshipped.txt`.
- A valid style that fails returns `false` and sets the result to 0.
- An invalid style throws `ArgumentException`. The parameter name is `style`.
- `IntegerParser` keeps the shared scan. The decimal digit loop for `ulong` stays separate from the 32-bit loops and from the `long` loop. `UInt64` overflow rules live on `UInt64Policy`.

## Consequences

- Callers write `Kexiva.UInt64.TryParse`.
- Formatting needs its own issue and its own public methods.
