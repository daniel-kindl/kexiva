# UInt32 parsing comparison, 2026-10-28

One default BenchmarkDotNet run of `UInt32ParsingComparison`. The workload is [../integer-parsing.md](../integer-parsing.md). Issue: [#25](https://github.com/daniel-kindl/kexiva/issues/25).

```bash
dotnet run -c Release --project benchmarks/Kexiva.Benchmarks/Kexiva.Benchmarks.csproj -- --filter "*UInt32ParsingComparison*"
```

```text
BenchmarkDotNet v0.15.8, Linux Fedora Linux 44 (Container Image)
AMD Ryzen 5 3600 3.93GHz, 1 CPU, 12 logical and 6 physical cores
.NET SDK 10.0.402
  [Host]     : .NET 10.0.13 (10.0.13, 10.0.1326.50504), X64 RyuJIT x86-64-v3
  DefaultJob : .NET 10.0.13 (10.0.13, 10.0.1326.50504), X64 RyuJIT x86-64-v3
```

The provider is `CultureInfo.InvariantCulture`. The style is the default `NumberStyles.Integer`. String parsing and UTF-8 parsing use the same six inputs.

`NetTryParse` is the baseline for each text value. The ratio column compares each row with that string baseline. The ratio does not compare UTF-8 with UTF-8.

The CSV records 0 B allocated for every row. The table prints that allocation as `-`.

The run did not set high priority. The log says `Failed to set up high priority (Permission denied).`

Two rows have a wider spread than the other rows:

- `KexivaTryParse` for `4294967296` has mean 14.970 ns, standard deviation 0.8546 ns, and median 14.472 ns.
- `KexivaTryParseUtf8` for `4294967295` has mean 19.957 ns, standard deviation 0.6423 ns, and median 19.975 ns.

These numbers are not a regression gate. Regression decisions need stable benchmarking hardware.

On this host, string `KexivaTryParse` has a lower mean than `NetTryParse` for `0`, `1`, `123456`, `12a`, and `4294967295`. The overflow text `4294967296` has a higher mean. UTF-8 `KexivaTryParseUtf8` has a higher mean than `NetTryParseUtf8` for `123456` and `4294967295`. The other four UTF-8 texts have a lower mean.

| Method             | text       | Mean      | Error     | StdDev    | Median    | Ratio | RatioSD | Allocated |
|------------------- |----------- |----------:|----------:|----------:|----------:|------:|--------:|----------:|
| NetTryParse        | 0          |  6.891 ns | 0.0324 ns | 0.0287 ns |  6.886 ns |  1.00 |    0.01 |         - |
| KexivaTryParse     | 0          |  4.064 ns | 0.0124 ns | 0.0116 ns |  4.062 ns |  0.59 |    0.00 |         - |
| NetTryParseUtf8    | 0          |  9.727 ns | 0.0331 ns | 0.0294 ns |  9.720 ns |  1.41 |    0.01 |         - |
| KexivaTryParseUtf8 | 0          |  6.131 ns | 0.0230 ns | 0.0215 ns |  6.136 ns |  0.89 |    0.00 |         - |
| NetTryParse        | 1          |  7.406 ns | 0.0215 ns | 0.0190 ns |  7.403 ns |  1.00 |    0.00 |         - |
| KexivaTryParse     | 1          |  4.328 ns | 0.0199 ns | 0.0186 ns |  4.327 ns |  0.58 |    0.00 |         - |
| NetTryParseUtf8    | 1          |  8.421 ns | 0.0301 ns | 0.0267 ns |  8.423 ns |  1.14 |    0.00 |         - |
| KexivaTryParseUtf8 | 1          |  5.724 ns | 0.0574 ns | 0.0537 ns |  5.740 ns |  0.77 |    0.01 |         - |
| NetTryParse        | 123456     | 10.840 ns | 0.0713 ns | 0.0632 ns | 10.842 ns |  1.00 |    0.01 |         - |
| KexivaTryParse     | 123456     |  9.013 ns | 0.0807 ns | 0.0630 ns |  9.001 ns |  0.83 |    0.01 |         - |
| NetTryParseUtf8    | 123456     | 12.836 ns | 0.0559 ns | 0.0522 ns | 12.838 ns |  1.18 |    0.01 |         - |
| KexivaTryParseUtf8 | 123456     | 15.528 ns | 0.0939 ns | 0.0879 ns | 15.520 ns |  1.43 |    0.01 |         - |
| NetTryParse        | 12a        | 13.008 ns | 0.0507 ns | 0.0474 ns | 13.001 ns |  1.00 |    0.00 |         - |
| KexivaTryParse     | 12a        |  6.793 ns | 0.0663 ns | 0.0620 ns |  6.780 ns |  0.52 |    0.00 |         - |
| NetTryParseUtf8    | 12a        | 17.158 ns | 0.1118 ns | 0.1046 ns | 17.117 ns |  1.32 |    0.01 |         - |
| KexivaTryParseUtf8 | 12a        |  9.733 ns | 0.0199 ns | 0.0176 ns |  9.731 ns |  0.75 |    0.00 |         - |
| NetTryParse        | 4294967295 | 14.272 ns | 0.0218 ns | 0.0204 ns | 14.271 ns |  1.00 |    0.00 |         - |
| KexivaTryParse     | 4294967295 | 13.582 ns | 0.0378 ns | 0.0295 ns | 13.584 ns |  0.95 |    0.00 |         - |
| NetTryParseUtf8    | 4294967295 | 17.530 ns | 0.0491 ns | 0.0410 ns | 17.529 ns |  1.23 |    0.00 |         - |
| KexivaTryParseUtf8 | 4294967295 | 19.957 ns | 0.4206 ns | 0.6423 ns | 19.975 ns |  1.40 |    0.04 |         - |
| NetTryParse        | 4294967296 | 13.965 ns | 0.0620 ns | 0.0580 ns | 13.951 ns |  1.00 |    0.01 |         - |
| KexivaTryParse     | 4294967296 | 14.970 ns | 0.3266 ns | 0.8546 ns | 14.472 ns |  1.07 |    0.06 |         - |
| NetTryParseUtf8    | 4294967296 | 18.930 ns | 0.0468 ns | 0.0415 ns | 18.925 ns |  1.36 |    0.01 |         - |
| KexivaTryParseUtf8 | 4294967296 | 16.559 ns | 0.1045 ns | 0.0873 ns | 16.561 ns |  1.19 |    0.01 |         - |
