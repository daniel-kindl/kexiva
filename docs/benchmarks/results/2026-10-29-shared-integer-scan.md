# Shared integer scan comparison, 2026-10-29

This file records the shared `IntegerParser` scan. The earlier separate parsers are the before runs. Issue: [#26](https://github.com/daniel-kindl/kexiva/issues/26).

The digit loops for `int` and `uint` stay separate. A shared generic digit loop moved the UInt32 string `123456` ratio from 0.79 to 1.06. These tables use the separate loops.

The parser shares culture handling, UTF-8 decoding, radix parsing, and the general scan. Signed overflow and unsigned overflow stay in separate methods.

The UInt32 before run and the UInt32 after run both report 3.94 GHz on this host. The Int32 before run reports 3.93 GHz. The Int32 after run reports 3.94 GHz. Compare the Kexiva to .NET ratio. A raw mean can move when the host speed moves.

The older published UInt32 file is a different run: [2026-10-28-uint32-parsing.md](2026-10-28-uint32-parsing.md). The Int32 duplicated-parser file is [2026-10-28-int32-parsing.md](2026-10-28-int32-parsing.md).

```bash
dotnet run -c Release --project benchmarks/Kexiva.Benchmarks/Kexiva.Benchmarks.csproj -- --filter '*UInt32*ParsingComparison*'
dotnet run -c Release --project benchmarks/Kexiva.Benchmarks/Kexiva.Benchmarks.csproj -- --filter '*.Int32ParsingComparison.*' '*.Int32Utf8ParsingComparison.*'
```

The Int32 filter uses two patterns. The pattern `*Int32*ParsingComparison*` also selects the UInt32 classes.

```text
BenchmarkDotNet v0.15.8, Linux Fedora Linux 44 (Container Image)
AMD Ryzen 5 3600 3.94GHz, 1 CPU, 12 logical and 6 physical cores
.NET SDK 10.0.402
  [Host]     : .NET 10.0.13 (10.0.13, 10.0.1326.50504), X64 RyuJIT x86-64-v3
  DefaultJob : .NET 10.0.13 (10.0.13, 10.0.1326.50504), X64 RyuJIT x86-64-v3
```

The provider is `CultureInfo.InvariantCulture`. The style is the default `NumberStyles.Integer`. Each benchmark class has its own .NET baseline. The ratio compares Kexiva with that baseline for the same text.

The summary prints allocated memory as `-` for every row. The alloc ratio is `NA`.

The run did not set high priority. The log says `Failed to set up high priority (Permission denied).`

These numbers are not a regression gate. Regression decisions need stable benchmarking hardware.

## Ratio comparison

The before column is the duplicated parser. The after column is the shared scan.

| Class | Text | Before Kexiva | After Kexiva | Before ratio | After ratio |
| --- | --- | ---: | ---: | ---: | ---: |
| UInt32 string | 0 | 4.033 ns | 4.037 ns | 0.58 | 0.59 |
| UInt32 string | 1 | 4.349 ns | 4.530 ns | 0.59 | 0.61 |
| UInt32 string | 123456 | 9.229 ns | 9.107 ns | 0.79 | 0.85 |
| UInt32 string | 12a | 7.203 ns | 6.450 ns | 0.55 | 0.49 |
| UInt32 string | 4294967295 | 14.821 ns | 15.342 ns | 1.03 | 1.05 |
| UInt32 string | 4294967296 | 14.624 ns | 13.639 ns | 1.05 | 0.89 |
| UInt32 UTF-8 | 0 | 6.174 ns | 6.218 ns | 0.61 | 0.62 |
| UInt32 UTF-8 | 1 | 5.766 ns | 5.677 ns | 0.66 | 0.65 |
| UInt32 UTF-8 | 123456 | 14.796 ns | 14.430 ns | 1.15 | 1.11 |
| UInt32 UTF-8 | 12a | 9.479 ns | 9.603 ns | 0.55 | 0.56 |
| UInt32 UTF-8 | 4294967295 | 21.754 ns | 20.999 ns | 1.25 | 1.20 |
| UInt32 UTF-8 | 4294967296 | 16.420 ns | 17.132 ns | 0.87 | 0.90 |
| Int32 string | -2147483648 | 16.009 ns | 17.144 ns | 0.93 | 1.00 |
| Int32 string | 1 | 4.385 ns | 4.313 ns | 0.62 | 0.61 |
| Int32 string | 123456 | 10.185 ns | 9.011 ns | 0.93 | 0.83 |
| Int32 string | 12a | 6.411 ns | 7.131 ns | 0.53 | 0.60 |
| Int32 string | 2147483647 | 15.741 ns | 14.854 ns | 1.00 | 0.95 |
| Int32 string | 2147483648 | 15.412 ns | 14.787 ns | 1.01 | 0.94 |
| Int32 UTF-8 | -2147483648 | 18.103 ns | 18.849 ns | 0.90 | 0.98 |
| Int32 UTF-8 | 1 | 5.094 ns | 5.097 ns | 0.58 | 0.60 |
| Int32 UTF-8 | 123456 | 11.121 ns | 11.237 ns | 0.84 | 0.86 |
| Int32 UTF-8 | 12a | 9.692 ns | 9.463 ns | 0.58 | 0.55 |
| Int32 UTF-8 | 2147483647 | 15.473 ns | 15.939 ns | 0.87 | 0.89 |
| Int32 UTF-8 | 2147483648 | 16.786 ns | 17.837 ns | 0.86 | 0.93 |

UInt32 string `123456` has a higher ratio after the change. The Kexiva mean is lower: 9.107 ns after, 9.229 ns before. The .NET mean fell from 11.724 ns to 10.764 ns.

Int32 string `12a` is the largest Kexiva increase. The mean moved from 6.411 ns to 7.131 ns. That is about 11%. The ratio moved from 0.53 to 0.60. Kexiva remains faster than .NET on this row.

Int32 string `-2147483648` moved from 16.009 ns to 17.144 ns. That is about 7%. The ratio moved from 0.93 to 1.00.

Int32 UTF-8 `2147483648` moved from 16.786 ns to 17.837 ns. The standard deviation is 1.1553 ns. The distribution has more than one mode. Do not treat that row as a stable change.

UInt32 string `4294967295` has standard deviation 1.4635 ns in the after run. The ratio is 1.05. The before ratio is 1.03.

UInt32 string `4294967296` removed 14 .NET outliers. The baseline ratio prints as 1.02. The Kexiva mean is 13.639 ns. The before Kexiva mean is 14.624 ns.

The other rows stay within about 10% of the before Kexiva mean, or the after mean is lower.

## UInt32 string after

| Method | text | Mean | Error | StdDev | Median | Ratio | RatioSD | Allocated |
| --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: |
| NetTryParse | 0 | 6.843 ns | 0.0330 ns | 0.0308 ns | 6.828 ns | 1.00 | 0.01 | - |
| KexivaTryParse | 0 | 4.037 ns | 0.0118 ns | 0.0105 ns | 4.038 ns | 0.59 | 0.00 | - |
| NetTryParse | 1 | 7.387 ns | 0.0401 ns | 0.0375 ns | 7.390 ns | 1.00 | 0.01 | - |
| KexivaTryParse | 1 | 4.530 ns | 0.1232 ns | 0.3114 ns | 4.352 ns | 0.61 | 0.04 | - |
| NetTryParse | 123456 | 10.764 ns | 0.0426 ns | 0.0398 ns | 10.760 ns | 1.00 | 0.01 | - |
| KexivaTryParse | 123456 | 9.107 ns | 0.0807 ns | 0.0674 ns | 9.113 ns | 0.85 | 0.01 | - |
| NetTryParse | 12a | 13.186 ns | 0.1023 ns | 0.0854 ns | 13.173 ns | 1.00 | 0.01 | - |
| KexivaTryParse | 12a | 6.450 ns | 0.1061 ns | 0.0993 ns | 6.441 ns | 0.49 | 0.01 | - |
| NetTryParse | 4294967295 | 14.622 ns | 0.0695 ns | 0.0616 ns | 14.616 ns | 1.00 | 0.01 | - |
| KexivaTryParse | 4294967295 | 15.342 ns | 0.4964 ns | 1.4635 ns | 15.134 ns | 1.05 | 0.10 | - |
| NetTryParse | 4294967296 | 15.733 ns | 1.0083 ns | 2.7432 ns | 14.541 ns | 1.02 | 0.22 | - |
| KexivaTryParse | 4294967296 | 13.639 ns | 0.0618 ns | 0.0548 ns | 13.626 ns | 0.89 | 0.11 | - |

## UInt32 UTF-8 after

| Method | text | Mean | Error | StdDev | Ratio | RatioSD | Allocated |
| --- | --- | ---: | ---: | ---: | ---: | ---: | ---: |
| NetTryParseUtf8 | 0 | 10.028 ns | 0.0815 ns | 0.0762 ns | 1.00 | 0.01 | - |
| KexivaTryParseUtf8 | 0 | 6.218 ns | 0.0931 ns | 0.0871 ns | 0.62 | 0.01 | - |
| NetTryParseUtf8 | 1 | 8.702 ns | 0.0319 ns | 0.0298 ns | 1.00 | 0.00 | - |
| KexivaTryParseUtf8 | 1 | 5.677 ns | 0.0433 ns | 0.0384 ns | 0.65 | 0.00 | - |
| NetTryParseUtf8 | 123456 | 12.965 ns | 0.1330 ns | 0.1244 ns | 1.00 | 0.01 | - |
| KexivaTryParseUtf8 | 123456 | 14.430 ns | 0.3139 ns | 0.3359 ns | 1.11 | 0.03 | - |
| NetTryParseUtf8 | 12a | 17.235 ns | 0.2239 ns | 0.2095 ns | 1.00 | 0.02 | - |
| KexivaTryParseUtf8 | 12a | 9.603 ns | 0.0457 ns | 0.0427 ns | 0.56 | 0.01 | - |
| NetTryParseUtf8 | 4294967295 | 17.545 ns | 0.1633 ns | 0.1447 ns | 1.00 | 0.01 | - |
| KexivaTryParseUtf8 | 4294967295 | 20.999 ns | 0.3326 ns | 0.2777 ns | 1.20 | 0.02 | - |
| NetTryParseUtf8 | 4294967296 | 18.960 ns | 0.1821 ns | 0.1614 ns | 1.00 | 0.01 | - |
| KexivaTryParseUtf8 | 4294967296 | 17.132 ns | 0.1508 ns | 0.1177 ns | 0.90 | 0.01 | - |

## Int32 string after

| Method | text | Mean | Error | StdDev | Ratio | Allocated |
| --- | --- | ---: | ---: | ---: | ---: | ---: |
| NetTryParse | -2147483648 | 17.073 ns | 0.0504 ns | 0.0421 ns | 1.00 | - |
| KexivaTryParse | -2147483648 | 17.144 ns | 0.0892 ns | 0.0745 ns | 1.00 | - |
| NetTryParse | 1 | 7.018 ns | 0.0350 ns | 0.0327 ns | 1.00 | - |
| KexivaTryParse | 1 | 4.313 ns | 0.0114 ns | 0.0101 ns | 0.61 | - |
| NetTryParse | 123456 | 10.798 ns | 0.0147 ns | 0.0123 ns | 1.00 | - |
| KexivaTryParse | 123456 | 9.011 ns | 0.0619 ns | 0.0517 ns | 0.83 | - |
| NetTryParse | 12a | 11.872 ns | 0.0835 ns | 0.0781 ns | 1.00 | - |
| KexivaTryParse | 12a | 7.131 ns | 0.0216 ns | 0.0192 ns | 0.60 | - |
| NetTryParse | 2147483647 | 15.556 ns | 0.0913 ns | 0.0762 ns | 1.00 | - |
| KexivaTryParse | 2147483647 | 14.854 ns | 0.1670 ns | 0.1562 ns | 0.95 | - |
| NetTryParse | 2147483648 | 15.648 ns | 0.0874 ns | 0.0774 ns | 1.00 | - |
| KexivaTryParse | 2147483648 | 14.787 ns | 0.1140 ns | 0.1010 ns | 0.94 | - |

## Int32 UTF-8 after

| Method | text | Mean | Error | StdDev | Ratio | RatioSD | Allocated |
| --- | --- | ---: | ---: | ---: | ---: | ---: | ---: |
| NetTryParseUtf8 | -2147483648 | 19.204 ns | 0.1396 ns | 0.1306 ns | 1.00 | 0.01 | - |
| KexivaTryParseUtf8 | -2147483648 | 18.849 ns | 0.1572 ns | 0.1313 ns | 0.98 | 0.01 | - |
| NetTryParseUtf8 | 1 | 8.512 ns | 0.0331 ns | 0.0310 ns | 1.00 | 0.00 | - |
| KexivaTryParseUtf8 | 1 | 5.097 ns | 0.0401 ns | 0.0355 ns | 0.60 | 0.00 | - |
| NetTryParseUtf8 | 123456 | 13.127 ns | 0.0236 ns | 0.0197 ns | 1.00 | 0.00 | - |
| KexivaTryParseUtf8 | 123456 | 11.237 ns | 0.1813 ns | 0.1696 ns | 0.86 | 0.01 | - |
| NetTryParseUtf8 | 12a | 17.138 ns | 0.0832 ns | 0.0737 ns | 1.00 | 0.01 | - |
| KexivaTryParseUtf8 | 12a | 9.463 ns | 0.0397 ns | 0.0352 ns | 0.55 | 0.00 | - |
| NetTryParseUtf8 | 2147483647 | 17.900 ns | 0.0514 ns | 0.0455 ns | 1.00 | 0.00 | - |
| KexivaTryParseUtf8 | 2147483647 | 15.939 ns | 0.1402 ns | 0.1171 ns | 0.89 | 0.01 | - |
| NetTryParseUtf8 | 2147483648 | 19.194 ns | 0.0393 ns | 0.0328 ns | 1.00 | 0.00 | - |
| KexivaTryParseUtf8 | 2147483648 | 17.837 ns | 0.3918 ns | 1.1553 ns | 0.93 | 0.06 | - |

The UInt32 run total is 00:17:19. The string half removed many outliers on `4294967296`. The Int32 run total is 00:09:13. Each run executed 24 benchmarks.
