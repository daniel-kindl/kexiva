# UInt32 parsing comparison, 2026-10-28

One default BenchmarkDotNet run of `UInt32ParsingComparison` and `UInt32Utf8ParsingComparison`. The workload is [../integer-parsing.md](../integer-parsing.md). Issue: [#25](https://github.com/daniel-kindl/kexiva/issues/25).

```bash
dotnet run -c Release --project benchmarks/Kexiva.Benchmarks/Kexiva.Benchmarks.csproj -- --filter "*UInt32*ParsingComparison*"
```

```text
BenchmarkDotNet v0.15.8, Linux Fedora Linux 44 (Container Image)
AMD Ryzen 5 3600 3.95GHz, 1 CPU, 12 logical and 6 physical cores
.NET SDK 10.0.402
  [Host]     : .NET 10.0.13 (10.0.13, 10.0.1326.50504), X64 RyuJIT x86-64-v3
  DefaultJob : .NET 10.0.13 (10.0.13, 10.0.1326.50504), X64 RyuJIT x86-64-v3
```

The provider is `CultureInfo.InvariantCulture`. The style is the default `NumberStyles.Integer`. String parsing and UTF-8 parsing use the same six inputs. Each benchmark class has its own .NET baseline. The ratio compares Kexiva with that baseline for the same text.

The CSV records 0 B allocated for every row. The tables print that allocation as `-`.

The run did not set high priority. The log says `Failed to set up high priority (Permission denied).`

Two string rows have a wider spread than the other rows:

- `KexivaTryParse` for `4294967295` has mean 15.106 ns, standard deviation 0.5581 ns, and median 14.890 ns.
- `KexivaTryParse` for `4294967296` has mean 14.965 ns, standard deviation 0.7766 ns, and median 14.490 ns.

These numbers are not a regression gate. Regression decisions need stable benchmarking hardware.

On this host, string `KexivaTryParse` has a lower mean than `NetTryParse` for `0`, `1`, `123456`, and `12a`. The texts `4294967295` and `4294967296` have a higher mean. UTF-8 `KexivaTryParseUtf8` has a lower mean than `NetTryParseUtf8` for `0`, `1`, `12a`, and `4294967296`. The texts `123456` and `4294967295` have a higher mean.

## String

| Method         | text       | Mean      | Error     | StdDev    | Median    | Ratio | RatioSD | Allocated |
|--------------- |----------- |----------:|----------:|----------:|----------:|------:|--------:|----------:|
| NetTryParse    | 0          |  6.896 ns | 0.0331 ns | 0.0276 ns |  6.892 ns |  1.00 |    0.01 |         - |
| KexivaTryParse | 0          |  4.115 ns | 0.0254 ns | 0.0238 ns |  4.104 ns |  0.60 |    0.00 |         - |
| NetTryParse    | 1          |  7.450 ns | 0.0687 ns | 0.0609 ns |  7.428 ns |  1.00 |    0.01 |         - |
| KexivaTryParse | 1          |  4.344 ns | 0.0483 ns | 0.0452 ns |  4.340 ns |  0.58 |    0.01 |         - |
| NetTryParse    | 123456     | 11.041 ns | 0.1269 ns | 0.1187 ns | 11.059 ns |  1.00 |    0.01 |         - |
| KexivaTryParse | 123456     |  9.096 ns | 0.2093 ns | 0.1856 ns |  9.022 ns |  0.82 |    0.02 |         - |
| NetTryParse    | 12a        | 13.072 ns | 0.1293 ns | 0.1146 ns | 13.065 ns |  1.00 |    0.01 |         - |
| KexivaTryParse | 12a        |  6.628 ns | 0.1357 ns | 0.1452 ns |  6.640 ns |  0.51 |    0.01 |         - |
| NetTryParse    | 4294967295 | 14.452 ns | 0.1291 ns | 0.1144 ns | 14.428 ns |  1.00 |    0.01 |         - |
| KexivaTryParse | 4294967295 | 15.106 ns | 0.3340 ns | 0.5581 ns | 14.890 ns |  1.05 |    0.04 |         - |
| NetTryParse    | 4294967296 | 14.120 ns | 0.1490 ns | 0.1394 ns | 14.075 ns |  1.00 |    0.01 |         - |
| KexivaTryParse | 4294967296 | 14.965 ns | 0.3295 ns | 0.7766 ns | 14.490 ns |  1.06 |    0.06 |         - |

## UTF-8

| Method             | text       | Mean      | Error     | StdDev    | Ratio | RatioSD | Allocated |
|------------------- |----------- |----------:|----------:|----------:|------:|--------:|----------:|
| NetTryParseUtf8    | 0          |  9.724 ns | 0.0691 ns | 0.0646 ns |  1.00 |    0.01 |         - |
| KexivaTryParseUtf8 | 0          |  6.083 ns | 0.0358 ns | 0.0317 ns |  0.63 |    0.01 |         - |
| NetTryParseUtf8    | 1          |  8.406 ns | 0.0313 ns | 0.0277 ns |  1.00 |    0.00 |         - |
| KexivaTryParseUtf8 | 1          |  5.794 ns | 0.1467 ns | 0.1801 ns |  0.69 |    0.02 |         - |
| NetTryParseUtf8    | 123456     | 12.895 ns | 0.1156 ns | 0.1025 ns |  1.00 |    0.01 |         - |
| KexivaTryParseUtf8 | 123456     | 15.506 ns | 0.2538 ns | 0.2374 ns |  1.20 |    0.02 |         - |
| NetTryParseUtf8    | 12a        | 17.041 ns | 0.1277 ns | 0.1194 ns |  1.00 |    0.01 |         - |
| KexivaTryParseUtf8 | 12a        |  9.817 ns | 0.1177 ns | 0.1101 ns |  0.58 |    0.01 |         - |
| NetTryParseUtf8    | 4294967295 | 17.762 ns | 0.2937 ns | 0.2747 ns |  1.00 |    0.02 |         - |
| KexivaTryParseUtf8 | 4294967295 | 19.527 ns | 0.3793 ns | 0.3548 ns |  1.10 |    0.03 |         - |
| NetTryParseUtf8    | 4294967296 | 19.020 ns | 0.1593 ns | 0.1412 ns |  1.00 |    0.01 |         - |
| KexivaTryParseUtf8 | 4294967296 | 16.402 ns | 0.0714 ns | 0.0596 ns |  0.86 |    0.01 |         - |
