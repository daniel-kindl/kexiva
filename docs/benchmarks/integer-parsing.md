# Integer parsing baseline

Baseline measurements for the .NET integer parse and format methods. Issue: [#24](https://github.com/daniel-kindl/kexiva/issues/24). The benchmarks call the reference runtime. They do not call Kexiva.

The benchmark definitions are in `benchmarks/Kexiva.Benchmarks/IntegerParsingBaseline.cs`.

## Workload

The provider is `CultureInfo.InvariantCulture`. The style is the default `NumberStyles.Integer`. The format is the general format `G`.

Each integer type has the same operation set:

- `TryParse` on a `string`
- `Parse` on the medium string
- `TryParse` on the UTF-8 bytes of the medium string
- `ToString`
- `TryFormat` into a 32-character buffer

`TryParse` uses these inputs:

| Input | Int32 | UInt32 | Int64 | UInt64 |
| --- | --- | --- | --- | --- |
| Short valid | `1` | `1` | `1` | `1` |
| Medium valid | `123456` | `123456` | `123456` | `123456` |
| Low boundary | `Int32.MinValue` | `0` | `Int64.MinValue` | `0` |
| High boundary | `Int32.MaxValue` | `UInt32.MaxValue` | `Int64.MaxValue` | `UInt64.MaxValue` |
| Invalid | `12a` | `12a` | `12a` | `12a` |
| Overflow | `2147483648` | `4294967296` | `9223372036854775808` | `18446744073709551616` |

`Parse`, UTF-8 `TryParse`, `ToString`, and `TryFormat` use the medium value `123456`.

Hex, binary, currency, and the general number path are not in this baseline.

## Run

```bash
dotnet run -c Release --project benchmarks/Kexiva.Benchmarks/Kexiva.Benchmarks.csproj -- --filter "*ParsingBaseline*"
```

The command uses the default BenchmarkDotNet job and `MemoryDiagnoser`. It records latency and allocated memory. The returned value is consumed so the call is not removed.

Do not use this run as a performance claim. Regression decisions need stable benchmarking hardware. See [methodology.md](methodology.md).

The recorded run is [results/2026-10-28-integer-parsing.md](results/2026-10-28-integer-parsing.md).
