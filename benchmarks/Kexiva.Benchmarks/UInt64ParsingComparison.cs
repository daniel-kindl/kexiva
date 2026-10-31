using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using BenchmarkDotNet.Attributes;
using Kexiva;

namespace Kexiva.Benchmarks;

/// <summary>
/// Compares <see cref="UInt64.TryParse(string?, IFormatProvider?, out ulong)"/> with <see cref="ulong.TryParse(string?, IFormatProvider?, out ulong)"/>.
/// </summary>
/// <remarks>
/// The provider is <see cref="CultureInfo.InvariantCulture"/> and the style is the default <see cref="NumberStyles.Integer"/>.
/// The inputs match the UInt64 baseline: <c>1</c>, <c>123456</c>, <c>0</c>, <c>18446744073709551615</c>, <c>12a</c>, and <c>18446744073709551616</c>.
/// <see cref="ulong.TryParse(string?, IFormatProvider?, out ulong)"/> is the baseline for this class.
/// </remarks>
[MemoryDiagnoser]
[SuppressMessage("Maintainability", "CA1515:Consider making public types internal", Justification = "BenchmarkDotNet requires benchmark classes to be public.")]
public class UInt64ParsingComparison
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
    /// Parses with <see cref="ulong.TryParse(string?, IFormatProvider?, out ulong)"/>.
    /// </summary>
    /// <param name="text">The input text.</param>
    /// <returns>The parsed value, or 0 when parsing fails.</returns>
    [Benchmark(Baseline = true)]
    [Arguments("1")]
    [Arguments(_medium)]
    [Arguments("0")]
    [Arguments("18446744073709551615")]
    [Arguments("12a")]
    [Arguments("18446744073709551616")]
    public ulong NetTryParse(string text)
    {
        ulong.TryParse(text, _provider, out ulong value);
        return value;
    }

    /// <summary>
    /// Parses with <see cref="UInt64.TryParse(string?, IFormatProvider?, out ulong)"/>.
    /// </summary>
    /// <param name="text">The input text.</param>
    /// <returns>The parsed value, or 0 when parsing fails.</returns>
    [Benchmark]
    [Arguments("1")]
    [Arguments(_medium)]
    [Arguments("0")]
    [Arguments("18446744073709551615")]
    [Arguments("12a")]
    [Arguments("18446744073709551616")]
    public ulong KexivaTryParse(string text)
    {
        UInt64.TryParse(text, _provider, out ulong value);
        return value;
    }
}

/// <summary>
/// Compares <see cref="UInt64.TryParse(ReadOnlySpan{byte}, IFormatProvider?, out ulong)"/> with <see cref="ulong.TryParse(ReadOnlySpan{byte}, IFormatProvider?, out ulong)"/>.
/// </summary>
/// <remarks>
/// The provider is <see cref="CultureInfo.InvariantCulture"/> and the style is the default <see cref="NumberStyles.Integer"/>.
/// The inputs match <see cref="UInt64ParsingComparison"/>.
/// <see cref="ulong.TryParse(ReadOnlySpan{byte}, IFormatProvider?, out ulong)"/> is the baseline for this class.
/// </remarks>
[MemoryDiagnoser]
[SuppressMessage("Maintainability", "CA1515:Consider making public types internal", Justification = "BenchmarkDotNet requires benchmark classes to be public.")]
public class UInt64Utf8ParsingComparison
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
    /// Parses UTF-8 text with <see cref="ulong.TryParse(ReadOnlySpan{byte}, IFormatProvider?, out ulong)"/>.
    /// </summary>
    /// <param name="text">The input text. The method selects a compile-time UTF-8 span for that text.</param>
    /// <returns>The parsed value, or 0 when parsing fails.</returns>
    [Benchmark(Baseline = true)]
    [Arguments("1")]
    [Arguments(_medium)]
    [Arguments("0")]
    [Arguments("18446744073709551615")]
    [Arguments("12a")]
    [Arguments("18446744073709551616")]
    public ulong NetTryParseUtf8(string text)
    {
        ulong.TryParse(Utf8(text), _provider, out ulong value);
        return value;
    }

    /// <summary>
    /// Parses UTF-8 text with <see cref="UInt64.TryParse(ReadOnlySpan{byte}, IFormatProvider?, out ulong)"/>.
    /// </summary>
    /// <param name="text">The input text. The method selects a compile-time UTF-8 span for that text.</param>
    /// <returns>The parsed value, or 0 when parsing fails.</returns>
    [Benchmark]
    [Arguments("1")]
    [Arguments(_medium)]
    [Arguments("0")]
    [Arguments("18446744073709551615")]
    [Arguments("12a")]
    [Arguments("18446744073709551616")]
    public ulong KexivaTryParseUtf8(string text)
    {
        UInt64.TryParse(Utf8(text), _provider, out ulong value);
        return value;
    }

    private static ReadOnlySpan<byte> Utf8(string text)
    {
        return text switch
        {
            "1" => "1"u8,
            _medium => "123456"u8,
            "0" => "0"u8,
            "18446744073709551615" => "18446744073709551615"u8,
            "12a" => "12a"u8,
            _ => "18446744073709551616"u8,
        };
    }
}
