using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Text;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;

namespace Kexiva.Benchmarks;

/// <summary>
/// Compares Int32 Parse latency and allocations for matching string and span inputs.
/// </summary>
[MemoryDiagnoser]
[GroupBenchmarksBy(BenchmarkLogicalGroupRule.ByCategory)]
[CategoriesColumn]
[SuppressMessage("Maintainability", "CA1515:Consider making public types internal", Justification = "BenchmarkDotNet requires benchmark classes to be public.")]
public class Int32ParseComparison
{
    private byte[] _utf8 = [];

    /// <summary>
    /// Gets or sets a short, medium, boundary, malformed, or overflow input.
    /// </summary>
    [Params("1", "123456", "2147483647", "-2147483648", "12a", "2147483648")]
    public string Text { get; set; } = "1";

    /// <summary>
    /// Encodes the input and checks the result outside the timed methods.
    /// </summary>
    [GlobalSetup]
    public void Setup()
    {
        _utf8 = Encoding.UTF8.GetBytes(Text);
        bool valid = int.TryParse(Text, CultureInfo.InvariantCulture, out int expected);
        if (NetString() != (valid ? expected : 0) || KexivaString() != (valid ? expected : 0)
            || NetChars() != (valid ? expected : 0) || KexivaChars() != (valid ? expected : 0)
            || NetUtf8() != (valid ? expected : 0) || KexivaUtf8() != (valid ? expected : 0))
        {
            throw new InvalidOperationException("The Parse benchmark input produced an unexpected result.");
        }
    }

    /// <summary>
    /// Parses string input with .NET and consumes expected failures.
    /// </summary>
    /// <returns>The parsed value, or zero after a format or overflow exception.</returns>
    [Benchmark(Baseline = true)]
    [BenchmarkCategory("String")]
    public int NetString()
    {
        try
        {
            return int.Parse(Text, CultureInfo.InvariantCulture);
        }
        catch (FormatException)
        {
            return 0;
        }
        catch (OverflowException)
        {
            return 0;
        }
    }

    /// <summary>
    /// Parses string input with Kexiva and consumes expected failures.
    /// </summary>
    /// <returns>The parsed value, or zero after a format or overflow exception.</returns>
    [Benchmark]
    [BenchmarkCategory("String")]
    public int KexivaString()
    {
        try
        {
            return Int32.Parse(Text, CultureInfo.InvariantCulture);
        }
        catch (FormatException)
        {
            return 0;
        }
        catch (OverflowException)
        {
            return 0;
        }
    }

    /// <summary>
    /// Parses chars input with .NET and consumes expected failures.
    /// </summary>
    /// <returns>The parsed value, or zero after a format or overflow exception.</returns>
    [Benchmark(Baseline = true)]
    [BenchmarkCategory("Chars")]
    public int NetChars()
    {
        try
        {
            return int.Parse(Text.AsSpan(), CultureInfo.InvariantCulture);
        }
        catch (FormatException)
        {
            return 0;
        }
        catch (OverflowException)
        {
            return 0;
        }
    }

    /// <summary>
    /// Parses chars input with Kexiva and consumes expected failures.
    /// </summary>
    /// <returns>The parsed value, or zero after a format or overflow exception.</returns>
    [Benchmark]
    [BenchmarkCategory("Chars")]
    public int KexivaChars()
    {
        try
        {
            return Int32.Parse(Text.AsSpan(), CultureInfo.InvariantCulture);
        }
        catch (FormatException)
        {
            return 0;
        }
        catch (OverflowException)
        {
            return 0;
        }
    }

    /// <summary>
    /// Parses utf8 input with .NET and consumes expected failures.
    /// </summary>
    /// <returns>The parsed value, or zero after a format or overflow exception.</returns>
    [Benchmark(Baseline = true)]
    [BenchmarkCategory("Utf8")]
    public int NetUtf8()
    {
        try
        {
            return int.Parse(_utf8, CultureInfo.InvariantCulture);
        }
        catch (FormatException)
        {
            return 0;
        }
        catch (OverflowException)
        {
            return 0;
        }
    }

    /// <summary>
    /// Parses utf8 input with Kexiva and consumes expected failures.
    /// </summary>
    /// <returns>The parsed value, or zero after a format or overflow exception.</returns>
    [Benchmark]
    [BenchmarkCategory("Utf8")]
    public int KexivaUtf8()
    {
        try
        {
            return Int32.Parse(_utf8, CultureInfo.InvariantCulture);
        }
        catch (FormatException)
        {
            return 0;
        }
        catch (OverflowException)
        {
            return 0;
        }
    }

}

/// <summary>
/// Compares UInt32 Parse latency and allocations for matching string and span inputs.
/// </summary>
[MemoryDiagnoser]
[GroupBenchmarksBy(BenchmarkLogicalGroupRule.ByCategory)]
[CategoriesColumn]
[SuppressMessage("Maintainability", "CA1515:Consider making public types internal", Justification = "BenchmarkDotNet requires benchmark classes to be public.")]
public class UInt32ParseComparison
{
    private byte[] _utf8 = [];

    /// <summary>
    /// Gets or sets a short, medium, boundary, malformed, or overflow input.
    /// </summary>
    [Params("1", "123456", "4294967295", "0", "12a", "4294967296")]
    public string Text { get; set; } = "1";

    /// <summary>
    /// Encodes the input and checks the result outside the timed methods.
    /// </summary>
    [GlobalSetup]
    public void Setup()
    {
        _utf8 = Encoding.UTF8.GetBytes(Text);
        bool valid = uint.TryParse(Text, CultureInfo.InvariantCulture, out uint expected);
        if (NetString() != (valid ? expected : 0) || KexivaString() != (valid ? expected : 0)
            || NetChars() != (valid ? expected : 0) || KexivaChars() != (valid ? expected : 0)
            || NetUtf8() != (valid ? expected : 0) || KexivaUtf8() != (valid ? expected : 0))
        {
            throw new InvalidOperationException("The Parse benchmark input produced an unexpected result.");
        }
    }

    /// <summary>
    /// Parses string input with .NET and consumes expected failures.
    /// </summary>
    /// <returns>The parsed value, or zero after a format or overflow exception.</returns>
    [Benchmark(Baseline = true)]
    [BenchmarkCategory("String")]
    public uint NetString()
    {
        try
        {
            return uint.Parse(Text, CultureInfo.InvariantCulture);
        }
        catch (FormatException)
        {
            return 0;
        }
        catch (OverflowException)
        {
            return 0;
        }
    }

    /// <summary>
    /// Parses string input with Kexiva and consumes expected failures.
    /// </summary>
    /// <returns>The parsed value, or zero after a format or overflow exception.</returns>
    [Benchmark]
    [BenchmarkCategory("String")]
    public uint KexivaString()
    {
        try
        {
            return UInt32.Parse(Text, CultureInfo.InvariantCulture);
        }
        catch (FormatException)
        {
            return 0;
        }
        catch (OverflowException)
        {
            return 0;
        }
    }

    /// <summary>
    /// Parses chars input with .NET and consumes expected failures.
    /// </summary>
    /// <returns>The parsed value, or zero after a format or overflow exception.</returns>
    [Benchmark(Baseline = true)]
    [BenchmarkCategory("Chars")]
    public uint NetChars()
    {
        try
        {
            return uint.Parse(Text.AsSpan(), CultureInfo.InvariantCulture);
        }
        catch (FormatException)
        {
            return 0;
        }
        catch (OverflowException)
        {
            return 0;
        }
    }

    /// <summary>
    /// Parses chars input with Kexiva and consumes expected failures.
    /// </summary>
    /// <returns>The parsed value, or zero after a format or overflow exception.</returns>
    [Benchmark]
    [BenchmarkCategory("Chars")]
    public uint KexivaChars()
    {
        try
        {
            return UInt32.Parse(Text.AsSpan(), CultureInfo.InvariantCulture);
        }
        catch (FormatException)
        {
            return 0;
        }
        catch (OverflowException)
        {
            return 0;
        }
    }

    /// <summary>
    /// Parses utf8 input with .NET and consumes expected failures.
    /// </summary>
    /// <returns>The parsed value, or zero after a format or overflow exception.</returns>
    [Benchmark(Baseline = true)]
    [BenchmarkCategory("Utf8")]
    public uint NetUtf8()
    {
        try
        {
            return uint.Parse(_utf8, CultureInfo.InvariantCulture);
        }
        catch (FormatException)
        {
            return 0;
        }
        catch (OverflowException)
        {
            return 0;
        }
    }

    /// <summary>
    /// Parses utf8 input with Kexiva and consumes expected failures.
    /// </summary>
    /// <returns>The parsed value, or zero after a format or overflow exception.</returns>
    [Benchmark]
    [BenchmarkCategory("Utf8")]
    public uint KexivaUtf8()
    {
        try
        {
            return UInt32.Parse(_utf8, CultureInfo.InvariantCulture);
        }
        catch (FormatException)
        {
            return 0;
        }
        catch (OverflowException)
        {
            return 0;
        }
    }

}

/// <summary>
/// Compares Int64 Parse latency and allocations for matching string and span inputs.
/// </summary>
[MemoryDiagnoser]
[GroupBenchmarksBy(BenchmarkLogicalGroupRule.ByCategory)]
[CategoriesColumn]
[SuppressMessage("Maintainability", "CA1515:Consider making public types internal", Justification = "BenchmarkDotNet requires benchmark classes to be public.")]
public class Int64ParseComparison
{
    private byte[] _utf8 = [];

    /// <summary>
    /// Gets or sets a short, medium, boundary, malformed, or overflow input.
    /// </summary>
    [Params("1", "123456", "9223372036854775807", "-9223372036854775808", "12a", "9223372036854775808")]
    public string Text { get; set; } = "1";

    /// <summary>
    /// Encodes the input and checks the result outside the timed methods.
    /// </summary>
    [GlobalSetup]
    public void Setup()
    {
        _utf8 = Encoding.UTF8.GetBytes(Text);
        bool valid = long.TryParse(Text, CultureInfo.InvariantCulture, out long expected);
        if (NetString() != (valid ? expected : 0) || KexivaString() != (valid ? expected : 0)
            || NetChars() != (valid ? expected : 0) || KexivaChars() != (valid ? expected : 0)
            || NetUtf8() != (valid ? expected : 0) || KexivaUtf8() != (valid ? expected : 0))
        {
            throw new InvalidOperationException("The Parse benchmark input produced an unexpected result.");
        }
    }

    /// <summary>
    /// Parses string input with .NET and consumes expected failures.
    /// </summary>
    /// <returns>The parsed value, or zero after a format or overflow exception.</returns>
    [Benchmark(Baseline = true)]
    [BenchmarkCategory("String")]
    public long NetString()
    {
        try
        {
            return long.Parse(Text, CultureInfo.InvariantCulture);
        }
        catch (FormatException)
        {
            return 0;
        }
        catch (OverflowException)
        {
            return 0;
        }
    }

    /// <summary>
    /// Parses string input with Kexiva and consumes expected failures.
    /// </summary>
    /// <returns>The parsed value, or zero after a format or overflow exception.</returns>
    [Benchmark]
    [BenchmarkCategory("String")]
    public long KexivaString()
    {
        try
        {
            return Int64.Parse(Text, CultureInfo.InvariantCulture);
        }
        catch (FormatException)
        {
            return 0;
        }
        catch (OverflowException)
        {
            return 0;
        }
    }

    /// <summary>
    /// Parses chars input with .NET and consumes expected failures.
    /// </summary>
    /// <returns>The parsed value, or zero after a format or overflow exception.</returns>
    [Benchmark(Baseline = true)]
    [BenchmarkCategory("Chars")]
    public long NetChars()
    {
        try
        {
            return long.Parse(Text.AsSpan(), CultureInfo.InvariantCulture);
        }
        catch (FormatException)
        {
            return 0;
        }
        catch (OverflowException)
        {
            return 0;
        }
    }

    /// <summary>
    /// Parses chars input with Kexiva and consumes expected failures.
    /// </summary>
    /// <returns>The parsed value, or zero after a format or overflow exception.</returns>
    [Benchmark]
    [BenchmarkCategory("Chars")]
    public long KexivaChars()
    {
        try
        {
            return Int64.Parse(Text.AsSpan(), CultureInfo.InvariantCulture);
        }
        catch (FormatException)
        {
            return 0;
        }
        catch (OverflowException)
        {
            return 0;
        }
    }

    /// <summary>
    /// Parses utf8 input with .NET and consumes expected failures.
    /// </summary>
    /// <returns>The parsed value, or zero after a format or overflow exception.</returns>
    [Benchmark(Baseline = true)]
    [BenchmarkCategory("Utf8")]
    public long NetUtf8()
    {
        try
        {
            return long.Parse(_utf8, CultureInfo.InvariantCulture);
        }
        catch (FormatException)
        {
            return 0;
        }
        catch (OverflowException)
        {
            return 0;
        }
    }

    /// <summary>
    /// Parses utf8 input with Kexiva and consumes expected failures.
    /// </summary>
    /// <returns>The parsed value, or zero after a format or overflow exception.</returns>
    [Benchmark]
    [BenchmarkCategory("Utf8")]
    public long KexivaUtf8()
    {
        try
        {
            return Int64.Parse(_utf8, CultureInfo.InvariantCulture);
        }
        catch (FormatException)
        {
            return 0;
        }
        catch (OverflowException)
        {
            return 0;
        }
    }

}

/// <summary>
/// Compares UInt64 Parse latency and allocations for matching string and span inputs.
/// </summary>
[MemoryDiagnoser]
[GroupBenchmarksBy(BenchmarkLogicalGroupRule.ByCategory)]
[CategoriesColumn]
[SuppressMessage("Maintainability", "CA1515:Consider making public types internal", Justification = "BenchmarkDotNet requires benchmark classes to be public.")]
public class UInt64ParseComparison
{
    private byte[] _utf8 = [];

    /// <summary>
    /// Gets or sets a short, medium, boundary, malformed, or overflow input.
    /// </summary>
    [Params("1", "123456", "18446744073709551615", "0", "12a", "18446744073709551616")]
    public string Text { get; set; } = "1";

    /// <summary>
    /// Encodes the input and checks the result outside the timed methods.
    /// </summary>
    [GlobalSetup]
    public void Setup()
    {
        _utf8 = Encoding.UTF8.GetBytes(Text);
        bool valid = ulong.TryParse(Text, CultureInfo.InvariantCulture, out ulong expected);
        if (NetString() != (valid ? expected : 0) || KexivaString() != (valid ? expected : 0)
            || NetChars() != (valid ? expected : 0) || KexivaChars() != (valid ? expected : 0)
            || NetUtf8() != (valid ? expected : 0) || KexivaUtf8() != (valid ? expected : 0))
        {
            throw new InvalidOperationException("The Parse benchmark input produced an unexpected result.");
        }
    }

    /// <summary>
    /// Parses string input with .NET and consumes expected failures.
    /// </summary>
    /// <returns>The parsed value, or zero after a format or overflow exception.</returns>
    [Benchmark(Baseline = true)]
    [BenchmarkCategory("String")]
    public ulong NetString()
    {
        try
        {
            return ulong.Parse(Text, CultureInfo.InvariantCulture);
        }
        catch (FormatException)
        {
            return 0;
        }
        catch (OverflowException)
        {
            return 0;
        }
    }

    /// <summary>
    /// Parses string input with Kexiva and consumes expected failures.
    /// </summary>
    /// <returns>The parsed value, or zero after a format or overflow exception.</returns>
    [Benchmark]
    [BenchmarkCategory("String")]
    public ulong KexivaString()
    {
        try
        {
            return UInt64.Parse(Text, CultureInfo.InvariantCulture);
        }
        catch (FormatException)
        {
            return 0;
        }
        catch (OverflowException)
        {
            return 0;
        }
    }

    /// <summary>
    /// Parses chars input with .NET and consumes expected failures.
    /// </summary>
    /// <returns>The parsed value, or zero after a format or overflow exception.</returns>
    [Benchmark(Baseline = true)]
    [BenchmarkCategory("Chars")]
    public ulong NetChars()
    {
        try
        {
            return ulong.Parse(Text.AsSpan(), CultureInfo.InvariantCulture);
        }
        catch (FormatException)
        {
            return 0;
        }
        catch (OverflowException)
        {
            return 0;
        }
    }

    /// <summary>
    /// Parses chars input with Kexiva and consumes expected failures.
    /// </summary>
    /// <returns>The parsed value, or zero after a format or overflow exception.</returns>
    [Benchmark]
    [BenchmarkCategory("Chars")]
    public ulong KexivaChars()
    {
        try
        {
            return UInt64.Parse(Text.AsSpan(), CultureInfo.InvariantCulture);
        }
        catch (FormatException)
        {
            return 0;
        }
        catch (OverflowException)
        {
            return 0;
        }
    }

    /// <summary>
    /// Parses utf8 input with .NET and consumes expected failures.
    /// </summary>
    /// <returns>The parsed value, or zero after a format or overflow exception.</returns>
    [Benchmark(Baseline = true)]
    [BenchmarkCategory("Utf8")]
    public ulong NetUtf8()
    {
        try
        {
            return ulong.Parse(_utf8, CultureInfo.InvariantCulture);
        }
        catch (FormatException)
        {
            return 0;
        }
        catch (OverflowException)
        {
            return 0;
        }
    }

    /// <summary>
    /// Parses utf8 input with Kexiva and consumes expected failures.
    /// </summary>
    /// <returns>The parsed value, or zero after a format or overflow exception.</returns>
    [Benchmark]
    [BenchmarkCategory("Utf8")]
    public ulong KexivaUtf8()
    {
        try
        {
            return UInt64.Parse(_utf8, CultureInfo.InvariantCulture);
        }
        catch (FormatException)
        {
            return 0;
        }
        catch (OverflowException)
        {
            return 0;
        }
    }

}
