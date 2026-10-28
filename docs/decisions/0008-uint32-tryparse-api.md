# 0008. UInt32 TryParse API

- Status: Accepted
- Date: 2026-10-28

## Context

The integer parsing contract does not select a declaring type. Issue #25 is the first production parser. It adds `UInt32` `TryParse` only. `Parse`, formatting, and the other integer widths stay in later issues.

The public methods must take the same arguments as `System.UInt32.TryParse` and return the same result.

## Decision

- The declaring type is `Kexiva.UInt32`. The name suppresses CA1720 because it matches `System.UInt32`.
- Ship the nine `TryParse` overloads that `System.UInt32` provides.
- Record the signatures in `PublicAPI.Unshipped.txt`.
- A valid style that fails returns `false` and sets the result to 0.
- An invalid style throws `ArgumentException`. The parameter name is `style`.
- README status is M1 while this API is public. M0 forbids production parsing.

## Consequences

- Callers write `Kexiva.UInt32.TryParse`.
- Later integer issues can follow the same type name for `Int32`, `Int64`, and `UInt64`.
- Formatting needs its own issue and its own public methods.
