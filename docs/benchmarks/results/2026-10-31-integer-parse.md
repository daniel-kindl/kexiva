# Integer Parse comparison, 2026-10-31

Issue: [#51](https://github.com/daniel-kindl/kexiva/issues/51). The production implementation is commit `344f961`. Benchmark definitions are in `benchmarks/Kexiva.Benchmarks/IntegerParseComparison.cs`.

## Reproduce

```bash
dotnet run -c Release --project benchmarks/Kexiva.Benchmarks/Kexiva.Benchmarks.csproj -- --filter '*ParseComparison*' --job Short --warmupCount 1 --iterationCount 3 --iterationTime 100 --launchCount 1
```

The recorded command also set `--artifacts /tmp/kexiva-51-benchmarks`. The artifact path does not change the workload.

```text
BenchmarkDotNet v0.15.8, Linux Fedora Linux 44 (Container Image)
AMD Ryzen 5 3600 3.94GHz, 1 CPU, 12 logical and 6 physical cores
.NET SDK 10.0.402
  [Host]     : .NET 10.0.13 (10.0.13, 10.0.1326.50504), X64 RyuJIT x86-64-v3
  Job-QPBZFU : .NET 10.0.13 (10.0.13, 10.0.1326.50504), X64 RyuJIT x86-64-v3

IterationCount=3  IterationTime=100ms  LaunchCount=1  
WarmupCount=1
```

Global total time: 00:04:35 (275.46 sec), executed benchmarks: 144

## Workload and limits

Each concrete class measures one integer width. The six inputs are short, medium, maximum, minimum, malformed, and overflow text. For unsigned widths, the minimum is `0`. Each input has equal representation in the run. String, char-span, and UTF-8 comparisons use the same input set, default `NumberStyles.Integer`, and `CultureInfo.InvariantCulture`.

The setup encodes UTF-8 and checks expected values outside the timed methods. Timed methods return the value. They catch the expected format and overflow exceptions and return zero. Thus failure rows measure exception creation, unwinding, and the benchmark catch. They do not measure a nonthrowing parse.

BenchmarkDotNet used one launch, one warmup iteration, and three measurement iterations per case, with a target iteration time of 100 ms. This is a short exploratory run on a shared container. High priority could not be set. The confidence intervals can be wide with three samples. These results are not a regression gate or a general speed claim.

Mean, Error, standard deviation, ratio, and allocated bytes come from the BenchmarkDotNet CSV reports. Error is the reported confidence-interval half-width. Throughput is derived as `1,000,000,000 / mean_nanoseconds`, rounded to the nearest operation per second. Ratios compare Kexiva with .NET for the same width, text form, and input.

All successful rows reported zero allocated bytes. Failure rows include the expected exception allocations. Kexiva scans failed text twice because its existing parser returns only success or failure. The .NET fallback then distinguishes format failure from overflow. Null, style, and provider errors occur before that fallback and are outside this workload.

## Int32

| Method | Form | Text | Mean, ns | Error, ns | StdDev, ns | Ratio | Throughput, ops/s | Allocated |
| --- | --- | --- | ---: | ---: | ---: | ---: | ---: | ---: |
| NetChars | Chars | `-2147483648` | 15.083 | 1.5925 | 0.0873 | 1.00 | 66,299,808 | 0 B |
| KexivaChars | Chars | `-2147483648` | 15.191 | 0.6822 | 0.0374 | 1.01 | 65,828,451 | 0 B |
| NetChars | Chars | `1` | 5.314 | 0.3654 | 0.0200 | 1.00 | 188,182,160 | 0 B |
| KexivaChars | Chars | `1` | 4.428 | 0.1891 | 0.0104 | 0.83 | 225,835,592 | 0 B |
| NetChars | Chars | `123456` | 9.206 | 1.1966 | 0.0656 | 1.00 | 108,624,810 | 0 B |
| KexivaChars | Chars | `123456` | 10.323 | 1.3358 | 0.0732 | 1.12 | 96,871,065 | 0 B |
| NetChars | Chars | `12a` | 3080.751 | 951.4097 | 52.1500 | 1.00 | 324,596 | 480 B |
| KexivaChars | Chars | `12a` | 2984.851 | 1223.5001 | 67.0642 | 0.97 | 335,025 | 480 B |
| NetChars | Chars | `2147483647` | 13.601 | 0.9511 | 0.0521 | 1.00 | 73,524,006 | 0 B |
| KexivaChars | Chars | `2147483647` | 17.250 | 19.8452 | 1.0878 | 1.27 | 57,971,014 | 0 B |
| NetChars | Chars | `2147483648` | 2813.196 | 981.7417 | 53.8126 | 1.00 | 355,468 | 320 B |
| KexivaChars | Chars | `2147483648` | 2842.734 | 863.1298 | 47.3111 | 1.01 | 351,774 | 320 B |
| NetString | String | `-2147483648` | 15.225 | 0.8908 | 0.0488 | 1.00 | 65,681,445 | 0 B |
| KexivaString | String | `-2147483648` | 15.816 | 1.8585 | 0.1019 | 1.04 | 63,227,112 | 0 B |
| NetString | String | `1` | 5.510 | 0.9641 | 0.0528 | 1.00 | 181,488,203 | 0 B |
| KexivaString | String | `1` | 5.005 | 1.4557 | 0.0798 | 0.91 | 199,800,200 | 0 B |
| NetString | String | `123456` | 9.846 | 1.6822 | 0.0922 | 1.00 | 101,564,087 | 0 B |
| KexivaString | String | `123456` | 12.457 | 4.1461 | 0.2273 | 1.27 | 80,276,150 | 0 B |
| NetString | String | `12a` | 3147.453 | 4461.4874 | 244.5492 | 1.00 | 317,717 | 480 B |
| KexivaString | String | `12a` | 3065.321 | 791.6378 | 43.3923 | 0.98 | 326,230 | 480 B |
| NetString | String | `2147483647` | 14.398 | 3.8103 | 0.2089 | 1.00 | 69,454,091 | 0 B |
| KexivaString | String | `2147483647` | 15.657 | 4.5751 | 0.2508 | 1.09 | 63,869,196 | 0 B |
| NetString | String | `2147483648` | 2794.103 | 852.4579 | 46.7261 | 1.00 | 357,897 | 320 B |
| KexivaString | String | `2147483648` | 2797.573 | 745.9255 | 40.8867 | 1.00 | 357,453 | 320 B |
| NetUtf8 | Utf8 | `-2147483648` | 15.512 | 1.5589 | 0.0855 | 1.00 | 64,466,220 | 0 B |
| KexivaUtf8 | Utf8 | `-2147483648` | 15.904 | 3.4778 | 0.1906 | 1.03 | 62,877,264 | 0 B |
| NetUtf8 | Utf8 | `1` | 6.665 | 1.1576 | 0.0635 | 1.00 | 150,037,509 | 0 B |
| KexivaUtf8 | Utf8 | `1` | 4.609 | 0.5738 | 0.0315 | 0.69 | 216,966,804 | 0 B |
| NetUtf8 | Utf8 | `123456` | 9.647 | 2.9890 | 0.1638 | 1.00 | 103,659,169 | 0 B |
| KexivaUtf8 | Utf8 | `123456` | 9.971 | 4.9351 | 0.2705 | 1.03 | 100,290,843 | 0 B |
| NetUtf8 | Utf8 | `12a` | 2275.792 | 1189.5975 | 65.2059 | 1.00 | 439,407 | 376 B |
| KexivaUtf8 | Utf8 | `12a` | 2358.858 | 621.1895 | 34.0495 | 1.04 | 423,934 | 376 B |
| NetUtf8 | Utf8 | `2147483647` | 15.369 | 2.3062 | 0.1264 | 1.00 | 65,066,042 | 0 B |
| KexivaUtf8 | Utf8 | `2147483647` | 14.005 | 14.6465 | 0.8028 | 0.91 | 71,403,070 | 0 B |
| NetUtf8 | Utf8 | `2147483648` | 2810.150 | 435.6031 | 23.8769 | 1.00 | 355,853 | 320 B |
| KexivaUtf8 | Utf8 | `2147483648` | 2857.892 | 370.5861 | 20.3131 | 1.02 | 349,908 | 320 B |

## UInt32

| Method | Form | Text | Mean, ns | Error, ns | StdDev, ns | Ratio | Throughput, ops/s | Allocated |
| --- | --- | --- | ---: | ---: | ---: | ---: | ---: | ---: |
| NetChars | Chars | `0` | 5.386 | 0.6039 | 0.0331 | 1.00 | 185,666,543 | 0 B |
| KexivaChars | Chars | `0` | 4.090 | 0.0841 | 0.0046 | 0.76 | 244,498,778 | 0 B |
| NetChars | Chars | `1` | 5.800 | 0.4986 | 0.0273 | 1.00 | 172,413,793 | 0 B |
| KexivaChars | Chars | `1` | 4.609 | 0.1338 | 0.0073 | 0.79 | 216,966,804 | 0 B |
| NetChars | Chars | `123456` | 8.911 | 1.8869 | 0.1034 | 1.00 | 112,220,851 | 0 B |
| KexivaChars | Chars | `123456` | 9.045 | 1.4470 | 0.0793 | 1.02 | 110,558,320 | 0 B |
| NetChars | Chars | `12a` | 2470.426 | 410.0916 | 22.4785 | 1.00 | 404,788 | 376 B |
| KexivaChars | Chars | `12a` | 2500.695 | 296.3855 | 16.2459 | 1.01 | 399,889 | 376 B |
| NetChars | Chars | `4294967295` | 12.885 | 2.5253 | 0.1384 | 1.00 | 77,609,624 | 0 B |
| KexivaChars | Chars | `4294967295` | 14.016 | 2.5881 | 0.1419 | 1.09 | 71,347,032 | 0 B |
| NetChars | Chars | `4294967296` | 2878.088 | 722.7177 | 39.6146 | 1.00 | 347,453 | 320 B |
| KexivaChars | Chars | `4294967296` | 2858.163 | 753.8933 | 41.3234 | 0.99 | 349,875 | 320 B |
| NetString | String | `0` | 5.501 | 0.4498 | 0.0247 | 1.00 | 181,785,130 | 0 B |
| KexivaString | String | `0` | 4.393 | 1.2792 | 0.0701 | 0.80 | 227,634,874 | 0 B |
| NetString | String | `1` | 5.464 | 0.5932 | 0.0325 | 1.00 | 183,016,105 | 0 B |
| KexivaString | String | `1` | 4.519 | 0.2982 | 0.0163 | 0.83 | 221,287,896 | 0 B |
| NetString | String | `123456` | 8.849 | 0.6656 | 0.0365 | 1.00 | 113,007,119 | 0 B |
| KexivaString | String | `123456` | 9.207 | 0.8045 | 0.0441 | 1.04 | 108,613,012 | 0 B |
| NetString | String | `12a` | 2458.273 | 617.2533 | 33.8337 | 1.00 | 406,790 | 376 B |
| KexivaString | String | `12a` | 2493.596 | 348.6117 | 19.1086 | 1.01 | 401,027 | 376 B |
| NetString | String | `4294967295` | 12.532 | 0.6336 | 0.0347 | 1.00 | 79,795,723 | 0 B |
| KexivaString | String | `4294967295` | 12.959 | 0.9183 | 0.0503 | 1.03 | 77,166,448 | 0 B |
| NetString | String | `4294967296` | 2853.514 | 535.2804 | 29.3405 | 1.00 | 350,445 | 320 B |
| KexivaString | String | `4294967296` | 2828.710 | 697.3610 | 38.2247 | 0.99 | 353,518 | 320 B |
| NetUtf8 | Utf8 | `0` | 6.095 | 1.8349 | 0.1006 | 1.00 | 164,068,909 | 0 B |
| KexivaUtf8 | Utf8 | `0` | 4.353 | 1.0519 | 0.0577 | 0.71 | 229,726,625 | 0 B |
| NetUtf8 | Utf8 | `1` | 6.508 | 0.6965 | 0.0382 | 1.00 | 153,657,037 | 0 B |
| KexivaUtf8 | Utf8 | `1` | 4.635 | 0.1326 | 0.0073 | 0.71 | 215,749,730 | 0 B |
| NetUtf8 | Utf8 | `123456` | 10.014 | 0.5039 | 0.0276 | 1.00 | 99,860,196 | 0 B |
| KexivaUtf8 | Utf8 | `123456` | 9.013 | 1.6396 | 0.0899 | 0.90 | 110,950,849 | 0 B |
| NetUtf8 | Utf8 | `12a` | 2296.520 | 743.0552 | 40.7294 | 1.00 | 435,441 | 376 B |
| KexivaUtf8 | Utf8 | `12a` | 2342.580 | 383.9525 | 21.0457 | 1.02 | 426,880 | 376 B |
| NetUtf8 | Utf8 | `4294967295` | 13.586 | 0.7364 | 0.0404 | 1.00 | 73,605,182 | 0 B |
| KexivaUtf8 | Utf8 | `4294967295` | 13.929 | 26.3412 | 1.4438 | 1.03 | 71,792,663 | 0 B |
| NetUtf8 | Utf8 | `4294967296` | 2834.362 | 668.3478 | 36.6344 | 1.00 | 352,813 | 320 B |
| KexivaUtf8 | Utf8 | `4294967296` | 2824.612 | 751.7345 | 41.2051 | 1.00 | 354,031 | 320 B |

## Int64

| Method | Form | Text | Mean, ns | Error, ns | StdDev, ns | Ratio | Throughput, ops/s | Allocated |
| --- | --- | --- | ---: | ---: | ---: | ---: | ---: | ---: |
| NetChars | Chars | `-9223372036854775808` | 21.971 | 1.6482 | 0.0903 | 1.00 | 45,514,542 | 0 B |
| KexivaChars | Chars | `-9223372036854775808` | 25.703 | 22.8067 | 1.2501 | 1.17 | 38,905,964 | 0 B |
| NetChars | Chars | `1` | 6.230 | 27.1013 | 1.4855 | 1.03 | 160,513,644 | 0 B |
| KexivaChars | Chars | `1` | 5.037 | 0.4784 | 0.0262 | 0.84 | 198,530,872 | 0 B |
| NetChars | Chars | `123456` | 9.380 | 0.6824 | 0.0374 | 1.00 | 106,609,808 | 0 B |
| KexivaChars | Chars | `123456` | 11.520 | 4.0327 | 0.2210 | 1.23 | 86,805,556 | 0 B |
| NetChars | Chars | `12a` | 3029.901 | 1761.5260 | 96.5552 | 1.00 | 330,044 | 480 B |
| KexivaChars | Chars | `12a` | 3043.058 | 1737.4002 | 95.2328 | 1.01 | 328,617 | 480 B |
| NetChars | Chars | `9223372036854775807` | 21.133 | 1.2768 | 0.0700 | 1.00 | 47,319,358 | 0 B |
| KexivaChars | Chars | `9223372036854775807` | 25.802 | 2.5181 | 0.1380 | 1.22 | 38,756,686 | 0 B |
| NetChars | Chars | `9223372036854775808` | 2986.402 | 4487.2925 | 245.9636 | 1.00 | 334,851 | 320 B |
| KexivaChars | Chars | `9223372036854775808` | 2900.214 | 502.1775 | 27.5260 | 0.98 | 344,802 | 320 B |
| NetString | String | `-9223372036854775808` | 22.704 | 4.0231 | 0.2205 | 1.00 | 44,045,102 | 0 B |
| KexivaString | String | `-9223372036854775808` | 25.590 | 1.4441 | 0.0792 | 1.13 | 39,077,765 | 0 B |
| NetString | String | `1` | 5.612 | 0.0179 | 0.0010 | 1.00 | 178,189,594 | 0 B |
| KexivaString | String | `1` | 5.626 | 1.0727 | 0.0588 | 1.00 | 177,746,178 | 0 B |
| NetString | String | `123456` | 9.874 | 0.3591 | 0.0197 | 1.00 | 101,276,079 | 0 B |
| KexivaString | String | `123456` | 11.380 | 3.0274 | 0.1659 | 1.15 | 87,873,462 | 0 B |
| NetString | String | `12a` | 3011.256 | 686.3637 | 37.6219 | 1.00 | 332,087 | 480 B |
| KexivaString | String | `12a` | 2997.470 | 1214.3425 | 66.5622 | 1.00 | 333,615 | 480 B |
| NetString | String | `9223372036854775807` | 20.993 | 0.6713 | 0.0368 | 1.00 | 47,634,926 | 0 B |
| KexivaString | String | `9223372036854775807` | 25.079 | 4.0209 | 0.2204 | 1.19 | 39,873,998 | 0 B |
| NetString | String | `9223372036854775808` | 3207.580 | 9135.6363 | 500.7551 | 1.02 | 311,762 | 320 B |
| KexivaString | String | `9223372036854775808` | 2914.883 | 641.9349 | 35.1866 | 0.92 | 343,067 | 320 B |
| NetUtf8 | Utf8 | `-9223372036854775808` | 24.030 | 1.1962 | 0.0656 | 1.00 | 41,614,648 | 0 B |
| KexivaUtf8 | Utf8 | `-9223372036854775808` | 23.708 | 2.5412 | 0.1393 | 0.99 | 42,179,855 | 0 B |
| NetUtf8 | Utf8 | `1` | 6.761 | 0.3151 | 0.0173 | 1.00 | 147,907,114 | 0 B |
| KexivaUtf8 | Utf8 | `1` | 4.636 | 2.1696 | 0.1189 | 0.69 | 215,703,192 | 0 B |
| NetUtf8 | Utf8 | `123456` | 11.386 | 1.5844 | 0.0868 | 1.00 | 87,827,156 | 0 B |
| KexivaUtf8 | Utf8 | `123456` | 9.963 | 0.6030 | 0.0331 | 0.87 | 100,371,374 | 0 B |
| NetUtf8 | Utf8 | `12a` | 2291.092 | 865.4190 | 47.4365 | 1.00 | 436,473 | 376 B |
| KexivaUtf8 | Utf8 | `12a` | 2297.308 | 939.5540 | 51.5001 | 1.00 | 435,292 | 376 B |
| NetUtf8 | Utf8 | `9223372036854775807` | 26.155 | 4.5139 | 0.2474 | 1.00 | 38,233,607 | 0 B |
| KexivaUtf8 | Utf8 | `9223372036854775807` | 22.819 | 3.6041 | 0.1976 | 0.87 | 43,823,130 | 0 B |
| NetUtf8 | Utf8 | `9223372036854775808` | 2950.637 | 4248.0427 | 232.8496 | 1.00 | 338,910 | 320 B |
| KexivaUtf8 | Utf8 | `9223372036854775808` | 2944.653 | 1785.3660 | 97.8619 | 1.00 | 339,599 | 320 B |

## UInt64

| Method | Form | Text | Mean, ns | Error, ns | StdDev, ns | Ratio | Throughput, ops/s | Allocated |
| --- | --- | --- | ---: | ---: | ---: | ---: | ---: | ---: |
| NetChars | Chars | `0` | 5.554 | 0.3293 | 0.0181 | 1.00 | 180,050,414 | 0 B |
| KexivaChars | Chars | `0` | 4.330 | 0.2892 | 0.0159 | 0.78 | 230,946,882 | 0 B |
| NetChars | Chars | `1` | 6.256 | 2.9356 | 0.1609 | 1.00 | 159,846,547 | 0 B |
| KexivaChars | Chars | `1` | 4.636 | 0.3653 | 0.0200 | 0.74 | 215,703,192 | 0 B |
| NetChars | Chars | `123456` | 9.568 | 0.6164 | 0.0338 | 1.00 | 104,515,050 | 0 B |
| KexivaChars | Chars | `123456` | 9.766 | 0.4560 | 0.0250 | 1.02 | 102,396,068 | 0 B |
| NetChars | Chars | `12a` | 2450.440 | 1094.8127 | 60.0104 | 1.00 | 408,090 | 376 B |
| KexivaChars | Chars | `12a` | 2460.070 | 37.8641 | 2.0755 | 1.00 | 406,492 | 376 B |
| NetChars | Chars | `18446744073709551615` | 21.372 | 1.0109 | 0.0554 | 1.00 | 46,790,193 | 0 B |
| KexivaChars | Chars | `18446744073709551615` | 23.699 | 2.0750 | 0.1137 | 1.11 | 42,195,873 | 0 B |
| NetChars | Chars | `18446744073709551616` | 2795.182 | 1207.7522 | 66.2010 | 1.00 | 357,758 | 320 B |
| KexivaChars | Chars | `18446744073709551616` | 2862.062 | 574.5982 | 31.4957 | 1.02 | 349,398 | 320 B |
| NetString | String | `0` | 5.855 | 16.2031 | 0.8881 | 1.01 | 170,794,193 | 0 B |
| KexivaString | String | `0` | 4.375 | 0.2578 | 0.0141 | 0.76 | 228,571,429 | 0 B |
| NetString | String | `1` | 5.833 | 1.0374 | 0.0569 | 1.00 | 171,438,368 | 0 B |
| KexivaString | String | `1` | 4.667 | 1.2874 | 0.0706 | 0.80 | 214,270,409 | 0 B |
| NetString | String | `123456` | 9.046 | 0.1395 | 0.0076 | 1.00 | 110,546,098 | 0 B |
| KexivaString | String | `123456` | 9.997 | 0.4047 | 0.0222 | 1.11 | 100,030,009 | 0 B |
| NetString | String | `12a` | 2417.119 | 320.8890 | 17.5890 | 1.00 | 413,716 | 376 B |
| KexivaString | String | `12a` | 2488.451 | 599.5529 | 32.8635 | 1.03 | 401,856 | 376 B |
| NetString | String | `18446744073709551615` | 21.334 | 5.3342 | 0.2924 | 1.00 | 46,873,535 | 0 B |
| KexivaString | String | `18446744073709551615` | 26.790 | 69.3726 | 3.8025 | 1.26 | 37,327,361 | 0 B |
| NetString | String | `18446744073709551616` | 2838.517 | 499.2806 | 27.3673 | 1.00 | 352,297 | 320 B |
| KexivaString | String | `18446744073709551616` | 2875.629 | 952.0337 | 52.1842 | 1.01 | 347,750 | 320 B |
| NetUtf8 | Utf8 | `0` | 5.750 | 0.5035 | 0.0276 | 1.00 | 173,913,043 | 0 B |
| KexivaUtf8 | Utf8 | `0` | 4.324 | 0.4280 | 0.0235 | 0.75 | 231,267,345 | 0 B |
| NetUtf8 | Utf8 | `1` | 6.303 | 0.5818 | 0.0319 | 1.00 | 158,654,609 | 0 B |
| KexivaUtf8 | Utf8 | `1` | 4.517 | 0.0600 | 0.0033 | 0.72 | 221,385,876 | 0 B |
| NetUtf8 | Utf8 | `123456` | 9.501 | 4.0186 | 0.2203 | 1.00 | 105,252,079 | 0 B |
| KexivaUtf8 | Utf8 | `123456` | 9.010 | 0.9484 | 0.0520 | 0.95 | 110,987,791 | 0 B |
| NetUtf8 | Utf8 | `12a` | 2342.751 | 422.9619 | 23.1840 | 1.00 | 426,849 | 376 B |
| KexivaUtf8 | Utf8 | `12a` | 2343.804 | 636.4078 | 34.8837 | 1.00 | 426,657 | 376 B |
| NetUtf8 | Utf8 | `18446744073709551615` | 21.175 | 0.8263 | 0.0453 | 1.00 | 47,225,502 | 0 B |
| KexivaUtf8 | Utf8 | `18446744073709551615` | 24.439 | 24.8521 | 1.3622 | 1.15 | 40,918,205 | 0 B |
| NetUtf8 | Utf8 | `18446744073709551616` | 2844.151 | 604.7362 | 33.1476 | 1.00 | 351,599 | 320 B |
| KexivaUtf8 | Utf8 | `18446744073709551616` | 2917.265 | 416.2982 | 22.8187 | 1.03 | 342,787 | 320 B |

