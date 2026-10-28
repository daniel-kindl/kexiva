# Integer parsing baseline, 2026-10-28

One default BenchmarkDotNet run of `*ParsingBaseline*`. The workload is [../integer-parsing.md](../integer-parsing.md).

```text
BenchmarkDotNet v0.15.8, Linux Fedora Linux 44 (Container Image)
AMD Ryzen 5 3600 3.94GHz, 1 CPU, 12 logical and 6 physical cores
.NET SDK 10.0.402
  [Host]     : .NET 10.0.13 (10.0.13, 10.0.1326.50504), X64 RyuJIT x86-64-v3
  DefaultJob : .NET 10.0.13 (10.0.13, 10.0.1326.50504), X64 RyuJIT x86-64-v3
```

The same host block is in each generated report. A `?` in the `text` column means that the benchmark has no argument. Those methods use the medium value `123456`.

`Format` allocated 40 B for each integer type. `Parse`, `TryParse`, `TryParseUtf8`, and `TryFormat` show no allocated memory in this run.

The Int64 `TryParseUtf8` row has a standard deviation of 5.7629 ns and a median of 12.802 ns. The spread is wide in this run.

These numbers are not a regression gate. Regression decisions need stable benchmarking hardware.

## Int32

| Method       | text        | Mean      | Error     | StdDev    | Gen0   | Allocated |
|------------- |------------ |----------:|----------:|----------:|-------:|----------:|
| Parse        | ?           | 11.573 ns | 0.2228 ns | 0.2084 ns |      - |         - |
| TryParseUtf8 | ?           | 12.441 ns | 0.2182 ns | 0.2041 ns |      - |         - |
| Format       | ?           | 10.216 ns | 0.2600 ns | 0.2554 ns | 0.0048 |      40 B |
| TryFormat    | ?           |  4.546 ns | 0.0467 ns | 0.0437 ns |      - |         - |
| TryParse     | -2147483648 | 17.377 ns | 0.1794 ns | 0.1678 ns |      - |         - |
| TryParse     | 1           |  7.097 ns | 0.1012 ns | 0.0946 ns |      - |         - |
| TryParse     | 123456      | 10.910 ns | 0.1077 ns | 0.0954 ns |      - |         - |
| TryParse     | 12a         | 12.091 ns | 0.0516 ns | 0.0431 ns |      - |         - |
| TryParse     | 2147483647  | 15.800 ns | 0.1199 ns | 0.1122 ns |      - |         - |
| TryParse     | 2147483648  | 15.883 ns | 0.1603 ns | 0.1499 ns |      - |         - |

## UInt32

| Method       | text       | Mean      | Error     | StdDev    | Gen0   | Allocated |
|------------- |----------- |----------:|----------:|----------:|-------:|----------:|
| Parse        | ?          | 13.058 ns | 0.2916 ns | 0.4088 ns |      - |         - |
| TryParseUtf8 | ?          | 14.669 ns | 0.0671 ns | 0.0627 ns |      - |         - |
| Format       | ?          |  9.872 ns | 0.1659 ns | 0.1552 ns | 0.0048 |      40 B |
| TryFormat    | ?          |  3.781 ns | 0.0257 ns | 0.0240 ns |      - |         - |
| TryParse     | 0          |  6.901 ns | 0.0301 ns | 0.0235 ns |      - |         - |
| TryParse     | 1          |  7.378 ns | 0.0201 ns | 0.0178 ns |      - |         - |
| TryParse     | 123456     | 10.847 ns | 0.0493 ns | 0.0437 ns |      - |         - |
| TryParse     | 12a        | 12.783 ns | 0.0649 ns | 0.0607 ns |      - |         - |
| TryParse     | 4294967295 | 14.392 ns | 0.1062 ns | 0.0994 ns |      - |         - |
| TryParse     | 4294967296 | 14.348 ns | 0.0469 ns | 0.0391 ns |      - |         - |

## Int64

| Method       | text                 | Mean      | Error     | StdDev    | Median    | Gen0   | Allocated |
|------------- |--------------------- |----------:|----------:|----------:|----------:|-------:|----------:|
| Parse        | ?                    | 11.504 ns | 0.1123 ns | 0.0996 ns | 11.461 ns |      - |         - |
| TryParseUtf8 | ?                    | 15.893 ns | 2.0314 ns | 5.7629 ns | 12.802 ns |      - |         - |
| Format       | ?                    | 11.262 ns | 0.2760 ns | 0.3179 ns | 11.169 ns | 0.0048 |      40 B |
| TryFormat    | ?                    |  5.613 ns | 0.0368 ns | 0.0327 ns |  5.610 ns |      - |         - |
| TryParse     | -9223372036854775808 | 25.086 ns | 0.1935 ns | 0.1810 ns | 25.026 ns |      - |         - |
| TryParse     | 1                    |  7.751 ns | 0.2197 ns | 0.6304 ns |  7.514 ns |      - |         - |
| TryParse     | 123456               | 11.163 ns | 0.1145 ns | 0.1015 ns | 11.144 ns |      - |         - |
| TryParse     | 12a                  | 12.905 ns | 0.0987 ns | 0.0824 ns | 12.884 ns |      - |         - |
| TryParse     | 9223372036854775807  | 24.370 ns | 0.4873 ns | 0.4558 ns | 24.230 ns |      - |         - |
| TryParse     | 9223372036854775808  | 24.205 ns | 0.4377 ns | 0.4299 ns | 24.142 ns |      - |         - |

## UInt64

| Method       | text                 | Mean      | Error     | StdDev    | Gen0   | Allocated |
|------------- |--------------------- |----------:|----------:|----------:|-------:|----------:|
| Parse        | ?                    | 11.193 ns | 0.0574 ns | 0.0537 ns |      - |         - |
| TryParseUtf8 | ?                    | 14.036 ns | 0.0673 ns | 0.0597 ns |      - |         - |
| Format       | ?                    | 10.848 ns | 0.2750 ns | 0.6642 ns | 0.0048 |      40 B |
| TryFormat    | ?                    |  5.013 ns | 0.1254 ns | 0.1173 ns |      - |         - |
| TryParse     | 0                    |  7.096 ns | 0.0277 ns | 0.0246 ns |      - |         - |
| TryParse     | 1                    |  7.414 ns | 0.0617 ns | 0.0577 ns |      - |         - |
| TryParse     | 123456               | 11.530 ns | 0.2461 ns | 0.2181 ns |      - |         - |
| TryParse     | 12a                  | 13.094 ns | 0.2040 ns | 0.1809 ns |      - |         - |
| TryParse     | 18446744073709551615 | 23.702 ns | 0.2939 ns | 0.2606 ns |      - |         - |
| TryParse     | 18446744073709551616 | 23.716 ns | 0.5045 ns | 0.4954 ns |      - |         - |
