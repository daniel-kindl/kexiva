# Architecture

Kexiva provides optimized alternatives to selected .NET data-processing APIs. Core rule: same API, same behavior, better performance.

## Projects

| Project | Purpose | Shipped |
| --- | --- | --- |
| `src/Kexiva` | The library. No project references. Only private build-time packages (Nerdbank.GitVersioning, Public API Analyzer). | NuGet package |
| `tests/Kexiva.Tests` | Unit, architecture, and verification policy tests. | No |
| `tests/Kexiva.Compatibility.Tests` | Public API and behavior compatibility with .NET. | No |
| `benchmarks/Kexiva.Benchmarks` | BenchmarkDotNet comparisons against .NET. | No |
| `eng/Kexiva.Verify` | Repository policy and coverage checks run by `eng/verify.*`. | No |

## Dependency direction

```
Kexiva.Tests ──────────────┐
Kexiva.Compatibility.Tests ┼──> Kexiva (no references)
Kexiva.Benchmarks ─────────┘
Kexiva.Tests ──> Kexiva.Verify
```

`ArchitectureTests` enforce these rules:

- The production assembly does not reference test or benchmark assemblies.
- The production project has no project references.
- The production project uses only private build infrastructure packages.

## Compatibility

Behavioral compatibility for M1 integer parsing and formatting is specified in [Integer parsing and formatting](../compatibility/integer-parsing-and-formatting.md). `UInt32` parsing is [`Kexiva.UInt32.TryParse`](../decisions/0008-uint32-tryparse-api.md). `Int32` parsing is [`Kexiva.Int32.TryParse`](../decisions/0009-int32-tryparse-api.md). `Int64` parsing is [`Kexiva.Int64.TryParse`](../decisions/0010-int64-tryparse-api.md).

## Public API

The Public API Analyzer fails the build on an unrecorded public API change. Add new API to `src/Kexiva/PublicAPI.Unshipped.txt`. Move it to `PublicAPI.Shipped.txt` at release.

## Decisions

See [docs/decisions](../decisions/README.md).
