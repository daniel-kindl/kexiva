using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using BenchmarkDotNet.Attributes;
using Kexiva;

namespace Kexiva.Benchmarks;

/// <summary>
/// Compares <see cref="Int32.TryParse(string?, IFormatProvider?, out int)"/> with <see cref="int.TryParse(string?, IFormatProvider?, out int)"/>.
/// </summary>
/// <remarks>
/// The provider is <see cref="CultureInfo.InvariantCulture"/> and the style is the default <see cref="NumberStyles.Integer"/>.
/// The inputs match the Int32 baseline: <c>1</c>, <c>123456</c>, <c>2147483647</c>, <c>-2147483648</c>, <c>12a</c>, and <c>2147483648</c>.
/// <see cref="int.TryParse(string?, IFormatProvider?, out int)"/> is the baseline for this class.
/// </remarks>
[MemoryDiagnoser]
[SuppressMessage("Maintainability", "CA1515:Consider making public types internal", Justification = "BenchmarkDotNet requires benchmark classes to be public.")]
public class Int32ParsingComparison
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
    /// Parses with <see cref="int.TryParse(string?, IFormatProvider?, out int)"/>.
    /// </summary>
    /// <param name="text">The input text.</param>
    /// <returns>The parsed value, or 0 when parsing fails.</returns>
    [Benchmark(Baseline = true)]
    [Arguments("1")]
    [Arguments(_medium)]
    [Arguments("2147483647")]
    [Arguments("-2147483648")]
    [Arguments("12a")]
    [Arguments("2147483648")]
    public int NetTryParse(string text)
    {
        int.TryParse(text, _provider, out int value);
        return value;
    }

    /// <summary>
    /// Parses with <see cref="Int32.TryParse(string?, IFormatProvider?, out int)"/>.
    /// </summary>
    /// <param name="text">The input text.</param>
    /// <returns>The parsed value, or 0 when parsing fails.</returns>
    [Benchmark]
    [Arguments("1")]
    [Arguments(_medium)]
    [Arguments("2147483647")]
    [Arguments("-2147483648")]
    [Arguments("12a")]
    [Arguments("2147483648")]
    public int KexivaTryParse(string text)
    {
        Int32.TryParse(text, _provider, out int value);
        return value;
    }
}

/// <summary>
/// Compares <see cref="Int32.TryParse(ReadOnlySpan{byte}, IFormatProvider?, out int)"/> with <see cref="int.TryParse(ReadOnlySpan{byte}, IFormatProvider?, out int)"/>.
/// </summary>
/// <remarks>
/// The provider is <see cref="CultureInfo.InvariantCulture"/> and the style is the default <see cref="NumberStyles.Integer"/>.
/// The inputs match <see cref="Int32ParsingComparison"/>.
/// <see cref="int.TryParse(ReadOnlySpan{byte}, IFormatProvider?, out int)"/> is the baseline for this class.
/// </remarks>
[MemoryDiagnoser]
[SuppressMessage("Maintainability", "CA1515:Consider making public types internal", Justification = "BenchmarkDotNet requires benchmark classes to be public.")]
public class Int32Utf8ParsingComparison
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
    /// Parses UTF-8 text with <see cref="int.TryParse(ReadOnlySpan{byte}, IFormatProvider?, out int)"/>.
    /// </summary>
    /// <param name="text">The input text. The method selects a compile-time UTF-8 span for that text.</param>
    /// <returns>The parsed value, or 0 when parsing fails.</returns>
    [Benchmark(Baseline = true)]
    [Arguments("1")]
    [Arguments(_medium)]
    [Arguments("2147483647")]
    [Arguments("-2147483648")]
    [Arguments("12a")]
    [Arguments("2147483648")]
    public int NetTryParseUtf8(string text)
    {
        int.TryParse(Utf8(text), _provider, out int value);
        return value;
    }

    /// <summary>
    /// Parses UTF-8 text with <see cref="Int32.TryParse(ReadOnlySpan{byte}, IFormatProvider?, out int)"/>.
    /// </summary>
    /// <param name="text">The input text. The method selects a compile-time UTF-8 span for that text.</param>
    /// <returns>The parsed value, or 0 when parsing fails.</returns>
    [Benchmark]
    [Arguments("1")]
    [Arguments(_medium)]
    [Arguments("2147483647")]
    [Arguments("-2147483648")]
    [Arguments("12a")]
    [Arguments("2147483648")]
    public int KexivaTryParseUtf8(string text)
    {
        Int32.TryParse(Utf8(text), _provider, out int value);
        return value;
    }

    private static ReadOnlySpan<byte> Utf8(string text)
    {
        return text switch
        {
            "1" => "1"u8,
            _medium => "123456"u8,
            "2147483647" => "2147483647"u8,
            "-2147483648" => "-2147483648"u8,
            "12a" => "12a"u8,
            _ => "2147483648"u8,
        };
    }
}
