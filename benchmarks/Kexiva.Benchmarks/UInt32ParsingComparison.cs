using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using BenchmarkDotNet.Attributes;
using Kexiva;

namespace Kexiva.Benchmarks;

/// <summary>
/// Compares <see cref="UInt32.TryParse(string?, IFormatProvider?, out uint)"/> with <see cref="uint.TryParse(string?, IFormatProvider?, out uint)"/>.
/// </summary>
/// <remarks>
/// The provider is <see cref="CultureInfo.InvariantCulture"/> and the style is the default <see cref="NumberStyles.Integer"/>.
/// The inputs match the UInt32 baseline: <c>1</c>, <c>123456</c>, <c>0</c>, <c>4294967295</c>, <c>12a</c>, and <c>4294967296</c>.
/// String and UTF-8 parsing both use that set.
/// </remarks>
[MemoryDiagnoser]
[SuppressMessage("Maintainability", "CA1515:Consider making public types internal", Justification = "BenchmarkDotNet requires benchmark classes to be public.")]
public class UInt32ParsingComparison
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
    /// Parses with <see cref="uint.TryParse(string?, IFormatProvider?, out uint)"/>.
    /// </summary>
    /// <param name="text">The input text.</param>
    /// <returns>The parsed value, or 0 when parsing fails.</returns>
    [Benchmark(Baseline = true)]
    [Arguments("1")]
    [Arguments(_medium)]
    [Arguments("0")]
    [Arguments("4294967295")]
    [Arguments("12a")]
    [Arguments("4294967296")]
    public uint NetTryParse(string text)
    {
        uint.TryParse(text, _provider, out uint value);
        return value;
    }

    /// <summary>
    /// Parses with <see cref="UInt32.TryParse(string?, IFormatProvider?, out uint)"/>.
    /// </summary>
    /// <param name="text">The input text.</param>
    /// <returns>The parsed value, or 0 when parsing fails.</returns>
    [Benchmark]
    [Arguments("1")]
    [Arguments(_medium)]
    [Arguments("0")]
    [Arguments("4294967295")]
    [Arguments("12a")]
    [Arguments("4294967296")]
    public uint KexivaTryParse(string text)
    {
        UInt32.TryParse(text, _provider, out uint value);
        return value;
    }

    /// <summary>
    /// Parses UTF-8 text with <see cref="uint.TryParse(ReadOnlySpan{byte}, IFormatProvider?, out uint)"/>.
    /// </summary>
    /// <param name="text">The input text. The method selects a compile-time UTF-8 span for that text.</param>
    /// <returns>The parsed value, or 0 when parsing fails.</returns>
    [Benchmark]
    [Arguments("1")]
    [Arguments(_medium)]
    [Arguments("0")]
    [Arguments("4294967295")]
    [Arguments("12a")]
    [Arguments("4294967296")]
    public uint NetTryParseUtf8(string text)
    {
        uint.TryParse(Utf8(text), _provider, out uint value);
        return value;
    }

    /// <summary>
    /// Parses UTF-8 text with <see cref="UInt32.TryParse(ReadOnlySpan{byte}, IFormatProvider?, out uint)"/>.
    /// </summary>
    /// <param name="text">The input text. The method selects a compile-time UTF-8 span for that text.</param>
    /// <returns>The parsed value, or 0 when parsing fails.</returns>
    [Benchmark]
    [Arguments("1")]
    [Arguments(_medium)]
    [Arguments("0")]
    [Arguments("4294967295")]
    [Arguments("12a")]
    [Arguments("4294967296")]
    public uint KexivaTryParseUtf8(string text)
    {
        UInt32.TryParse(Utf8(text), _provider, out uint value);
        return value;
    }

    private static ReadOnlySpan<byte> Utf8(string text)
    {
        return text switch
        {
            "1" => "1"u8,
            _medium => "123456"u8,
            "0" => "0"u8,
            "4294967295" => "4294967295"u8,
            "12a" => "12a"u8,
            _ => "4294967296"u8,
        };
    }
}
