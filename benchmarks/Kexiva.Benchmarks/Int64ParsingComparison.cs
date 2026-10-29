using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using BenchmarkDotNet.Attributes;
using Kexiva;

namespace Kexiva.Benchmarks;

/// <summary>
/// Compares <see cref="Int64.TryParse(string?, IFormatProvider?, out long)"/> with <see cref="long.TryParse(string?, IFormatProvider?, out long)"/>.
/// </summary>
/// <remarks>
/// The provider is <see cref="CultureInfo.InvariantCulture"/> and the style is the default <see cref="NumberStyles.Integer"/>.
/// The inputs match the Int64 baseline: <c>1</c>, <c>123456</c>, <c>9223372036854775807</c>, <c>-9223372036854775808</c>, <c>12a</c>, and <c>9223372036854775808</c>.
/// <see cref="long.TryParse(string?, IFormatProvider?, out long)"/> is the baseline for this class.
/// </remarks>
[MemoryDiagnoser]
[SuppressMessage("Maintainability", "CA1515:Consider making public types internal", Justification = "BenchmarkDotNet requires benchmark classes to be public.")]
public class Int64ParsingComparison
{
    private const string _medium = "123456";

    private CultureInfo _provider = null!;

    /// <summary>
    /// Stores the invariant provider.
    /// </summary>
    [GlobalSetup]
    public void Setup()
    {
        _provider = CultureInfo.InvariantCulture;
    }

    /// <summary>
    /// Parses with <see cref="long.TryParse(string?, IFormatProvider?, out long)"/>.
    /// </summary>
    /// <param name="text">The input text.</param>
    /// <returns>The parsed value, or 0 when parsing fails.</returns>
    [Benchmark(Baseline = true)]
    [Arguments("1")]
    [Arguments(_medium)]
    [Arguments("9223372036854775807")]
    [Arguments("-9223372036854775808")]
    [Arguments("12a")]
    [Arguments("9223372036854775808")]
    public long NetTryParse(string text)
    {
        long.TryParse(text, _provider, out long value);
        return value;
    }

    /// <summary>
    /// Parses with <see cref="Int64.TryParse(string?, IFormatProvider?, out long)"/>.
    /// </summary>
    /// <param name="text">The input text.</param>
    /// <returns>The parsed value, or 0 when parsing fails.</returns>
    [Benchmark]
    [Arguments("1")]
    [Arguments(_medium)]
    [Arguments("9223372036854775807")]
    [Arguments("-9223372036854775808")]
    [Arguments("12a")]
    [Arguments("9223372036854775808")]
    public long KexivaTryParse(string text)
    {
        Int64.TryParse(text, _provider, out long value);
        return value;
    }
}

/// <summary>
/// Compares <see cref="Int64.TryParse(ReadOnlySpan{byte}, IFormatProvider?, out long)"/> with <see cref="long.TryParse(ReadOnlySpan{byte}, IFormatProvider?, out long)"/>.
/// </summary>
/// <remarks>
/// The provider is <see cref="CultureInfo.InvariantCulture"/> and the style is the default <see cref="NumberStyles.Integer"/>.
/// The inputs match <see cref="Int64ParsingComparison"/>.
/// <see cref="long.TryParse(ReadOnlySpan{byte}, IFormatProvider?, out long)"/> is the baseline for this class.
/// </remarks>
[MemoryDiagnoser]
[SuppressMessage("Maintainability", "CA1515:Consider making public types internal", Justification = "BenchmarkDotNet requires benchmark classes to be public.")]
public class Int64Utf8ParsingComparison
{
    private const string _medium = "123456";

    private CultureInfo _provider = null!;

    /// <summary>
    /// Stores the invariant provider.
    /// </summary>
    [GlobalSetup]
    public void Setup()
    {
        _provider = CultureInfo.InvariantCulture;
    }

    /// <summary>
    /// Parses UTF-8 text with <see cref="long.TryParse(ReadOnlySpan{byte}, IFormatProvider?, out long)"/>.
    /// </summary>
    /// <param name="text">The input text. The method selects a compile-time UTF-8 span for that text.</param>
    /// <returns>The parsed value, or 0 when parsing fails.</returns>
    [Benchmark(Baseline = true)]
    [Arguments("1")]
    [Arguments(_medium)]
    [Arguments("9223372036854775807")]
    [Arguments("-9223372036854775808")]
    [Arguments("12a")]
    [Arguments("9223372036854775808")]
    public long NetTryParseUtf8(string text)
    {
        long.TryParse(Utf8(text), _provider, out long value);
        return value;
    }

    /// <summary>
    /// Parses UTF-8 text with <see cref="Int64.TryParse(ReadOnlySpan{byte}, IFormatProvider?, out long)"/>.
    /// </summary>
    /// <param name="text">The input text. The method selects a compile-time UTF-8 span for that text.</param>
    /// <returns>The parsed value, or 0 when parsing fails.</returns>
    [Benchmark]
    [Arguments("1")]
    [Arguments(_medium)]
    [Arguments("9223372036854775807")]
    [Arguments("-9223372036854775808")]
    [Arguments("12a")]
    [Arguments("9223372036854775808")]
    public long KexivaTryParseUtf8(string text)
    {
        Int64.TryParse(Utf8(text), _provider, out long value);
        return value;
    }

    private static ReadOnlySpan<byte> Utf8(string text)
    {
        return text switch
        {
            "1" => "1"u8,
            _medium => "123456"u8,
            "9223372036854775807" => "9223372036854775807"u8,
            "-9223372036854775808" => "-9223372036854775808"u8,
            "12a" => "12a"u8,
            _ => "9223372036854775808"u8,
        };
    }
}
