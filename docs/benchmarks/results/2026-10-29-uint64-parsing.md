# UInt64 parsing comparison, 2026-10-29

One default BenchmarkDotNet run of `UInt64ParsingComparison` and `UInt64Utf8ParsingComparison`. The workload is [../integer-parsing.md](../integer-parsing.md). Issue: [#50](https://github.com/daniel-kindl/kexiva/issues/50).

```bash
dotnet run -c Release --project benchmarks/Kexiva.Benchmarks/Kexiva.Benchmarks.csproj -- --filter '*.UInt64ParsingComparison.*' '*.UInt64Utf8ParsingComparison.*'
```

```text
BenchmarkDotNet v0.15.8, Linux Fedora Linux 44 (Container Image)
AMD Ryzen 5 3600 1.89GHz, 1 CPU, 12 logical and 6 physical cores
.NET SDK 10.0.402
  [Host]     : .NET 10.0.13 (10.0.13, 10.0.1326.50504), X64 RyuJIT x86-64-v3
  DefaultJob : .NET 10.0.13 (10.0.13, 10.0.1326.50504), X64 RyuJIT x86-64-v3
```

The provider is `CultureInfo.InvariantCulture`. The style is the default `NumberStyles.Integer`. String parsing and UTF-8 parsing use the same six inputs. Each benchmark class has its own .NET baseline. The ratio compares Kexiva with that baseline for the same text.

The decimal digit loop for `ulong` stays separate from the 32-bit loops and from the `long` loop.

The summary prints allocated memory as `-` for every row. The alloc ratio is `NA`.

The run did not set high priority. The log says `Failed to set up high priority (Permission denied)`.

Global total time: 00:10:33. The run executed 24 benchmarks.

String `KexivaTryParse` for `18446744073709551615` has a standard deviation of 1.4765 ns and a median of 23.568 ns. BenchmarkDotNet removed 13 outliers from 31.16 ns to 36.67 ns. MValue = 2. String `KexivaTryParse` for `18446744073709551616` has a standard deviation of 0.8447 ns and a median of 23.975 ns. BenchmarkDotNet removed 12 outliers from 29.48 ns to 38.12 ns. MValue = 2. Do not treat those two rows as a stable difference.

These numbers are not a regression gate. Regression decisions need stable benchmarking hardware.

On this host, string `KexivaTryParse` has a lower mean than `NetTryParse` for `0`, `1`, `123456`, and `12a`. The ratio for `18446744073709551615` is 1.05. The ratio for `18446744073709551616` is 1.06. UTF-8 `KexivaTryParseUtf8` has a lower mean than `NetTryParseUtf8` for `0`, `1`, and `12a`. The ratio for `123456` is 1.09. The ratio for `18446744073709551615` is 1.21. The ratio for `18446744073709551616` is 1.02.

## String

| Method         | text                 | Mean      | Error     | StdDev    | Median    | Ratio | RatioSD | Allocated |
|--------------- |--------------------- |----------:|----------:|----------:|----------:|------:|--------:|----------:|
| NetTryParse    | 0                    |  7.101 ns | 0.0156 ns | 0.0138 ns |  7.100 ns |  1.00 |    0.00 |         - |
| KexivaTryParse | 0                    |  4.054 ns | 0.0236 ns | 0.0221 ns |  4.056 ns |  0.57 |    0.00 |         - |
| NetTryParse    | 1                    |  7.366 ns | 0.0286 ns | 0.0267 ns |  7.363 ns |  1.00 |    0.00 |         - |
| KexivaTryParse | 1                    |  4.093 ns | 0.0138 ns | 0.0122 ns |  4.092 ns |  0.56 |    0.00 |         - |
| NetTryParse    | 123456               | 11.212 ns | 0.0305 ns | 0.0270 ns | 11.203 ns |  1.00 |    0.00 |         - |
| KexivaTryParse | 123456               |  8.766 ns | 0.0767 ns | 0.0641 ns |  8.752 ns |  0.78 |    0.01 |         - |
| NetTryParse    | 12a                  | 13.296 ns | 0.0776 ns | 0.0688 ns | 13.306 ns |  1.00 |    0.01 |         - |
| KexivaTryParse | 12a                  |  6.353 ns | 0.0306 ns | 0.0286 ns |  6.352 ns |  0.48 |    0.00 |         - |
| NetTryParse    | 18446744073709551615 | 23.063 ns | 0.0933 ns | 0.0873 ns | 23.066 ns |  1.00 |    0.01 |         - |
| KexivaTryParse | 18446744073709551615 | 24.281 ns | 0.5394 ns | 1.4765 ns | 23.568 ns |  1.05 |    0.06 |         - |
| NetTryParse    | 18446744073709551616 | 22.925 ns | 0.1472 ns | 0.1305 ns | 22.875 ns |  1.00 |    0.01 |         - |
| KexivaTryParse | 18446744073709551616 | 24.355 ns | 0.4047 ns | 0.8447 ns | 23.975 ns |  1.06 |    0.04 |         - |

## UTF-8

| Method             | text                 | Mean      | Error     | StdDev    | Ratio | RatioSD | Allocated |
|------------------- |--------------------- |----------:|----------:|----------:|------:|--------:|----------:|
| NetTryParseUtf8    | 0                    | 10.154 ns | 0.1124 ns | 0.1051 ns |  1.00 |    0.01 |         - |
| KexivaTryParseUtf8 | 0                    |  6.956 ns | 0.0942 ns | 0.0835 ns |  0.69 |    0.01 |         - |
| NetTryParseUtf8    | 1                    |  9.421 ns | 0.0463 ns | 0.0410 ns |  1.00 |    0.01 |         - |
| KexivaTryParseUtf8 | 1                    |  6.130 ns | 0.1246 ns | 0.1224 ns |  0.65 |    0.01 |         - |
| NetTryParseUtf8    | 123456               | 14.043 ns | 0.0714 ns | 0.0668 ns |  1.00 |    0.01 |         - |
| KexivaTryParseUtf8 | 123456               | 15.238 ns | 0.3210 ns | 0.3153 ns |  1.09 |    0.02 |         - |
| NetTryParseUtf8    | 12a                  | 17.587 ns | 0.2193 ns | 0.2051 ns |  1.00 |    0.02 |         - |
| KexivaTryParseUtf8 | 12a                  | 11.524 ns | 0.1221 ns | 0.1082 ns |  0.66 |    0.01 |         - |
| NetTryParseUtf8    | 18446744073709551615 | 25.976 ns | 0.1809 ns | 0.1603 ns |  1.00 |    0.01 |         - |
| KexivaTryParseUtf8 | 18446744073709551615 | 31.535 ns | 0.6611 ns | 0.5162 ns |  1.21 |    0.02 |         - |
| NetTryParseUtf8    | 18446744073709551616 | 28.074 ns | 0.2570 ns | 0.2404 ns |  1.00 |    0.01 |         - |
| KexivaTryParseUtf8 | 18446744073709551616 | 28.634 ns | 0.5919 ns | 0.5247 ns |  1.02 |    0.02 |         - |
