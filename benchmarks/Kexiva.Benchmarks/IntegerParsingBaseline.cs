using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using BenchmarkDotNet.Attributes;

namespace Kexiva.Benchmarks;

/// <summary>
/// Baseline for <see cref="int"/> parsing and formatting in the reference runtime.
/// </summary>
/// <remarks>
/// The provider is <see cref="CultureInfo.InvariantCulture"/> and the style is the default <see cref="NumberStyles.Integer"/>.
/// Valid inputs are <c>1</c> and <c>123456</c>. Boundary inputs are <see cref="int.MaxValue"/> and <see cref="int.MinValue"/>.
/// The invalid input is <c>12a</c>. The overflow input is <c>2147483648</c>.
/// </remarks>
[MemoryDiagnoser]
[SuppressMessage("Maintainability", "CA1515:Consider making public types internal", Justification = "BenchmarkDotNet requires benchmark classes to be public.")]
public class Int32ParsingBaseline
{
    private const string _medium = "123456";

    private CultureInfo _provider = null!;
    private int _value;
    private string _text = null!;
    private byte[] _utf8 = null!;
    private char[] _destination = null!;

    /// <summary>
    /// Stores the invariant provider and the medium input.
    /// </summary>
    [GlobalSetup]
    public void Setup()
    {
        _provider = CultureInfo.InvariantCulture;
        _value = 123456;
        _text = _medium;
        _utf8 = System.Text.Encoding.UTF8.GetBytes(_medium);
        _destination = new char[32];
    }

    /// <summary>
    /// Parses a decimal integer with <see cref="int.TryParse(string?, IFormatProvider?, out int)"/>.
    /// </summary>
    /// <param name="text">The input text.</param>
    /// <returns>The parsed value, or <c>0</c> when parsing fails.</returns>
    [Benchmark]
    [Arguments("1")]
    [Arguments(_medium)]
    [Arguments("2147483647")]
    [Arguments("-2147483648")]
    [Arguments("12a")]
    [Arguments("2147483648")]
    public int TryParse(string text)
    {
        int.TryParse(text, _provider, out int value);
        return value;
    }

    /// <summary>
    /// Parses the medium value with <see cref="int.Parse(string, IFormatProvider?)"/>.
    /// </summary>
    /// <returns>The parsed value.</returns>
    [Benchmark]
    public int Parse()
    {
        return int.Parse(_text, _provider);
    }

    /// <summary>
    /// Parses the UTF-8 encoding of the medium value.
    /// </summary>
    /// <returns>The parsed value.</returns>
    [Benchmark]
    public int TryParseUtf8()
    {
        int.TryParse(_utf8, _provider, out int value);
        return value;
    }

    /// <summary>
    /// Formats the medium value with <see cref="int.ToString(IFormatProvider?)"/>.
    /// </summary>
    /// <returns>The formatted text.</returns>
    [Benchmark]
    public string Format()
    {
        return _value.ToString(_provider);
    }

    /// <summary>
    /// Formats the medium value with <see cref="int.TryFormat(Span{char}, out int, ReadOnlySpan{char}, IFormatProvider?)"/>.
    /// </summary>
    /// <returns>The number of characters written.</returns>
    [Benchmark]
    public int TryFormat()
    {
        _value.TryFormat(_destination, out int written, default, _provider);
        return written;
    }
}

/// <summary>
/// Baseline for <see cref="uint"/> parsing and formatting in the reference runtime.
/// </summary>
/// <remarks>
/// The provider is <see cref="CultureInfo.InvariantCulture"/> and the style is the default <see cref="NumberStyles.Integer"/>.
/// Valid inputs are <c>1</c> and <c>123456</c>. Boundary inputs are <c>0</c> and <see cref="uint.MaxValue"/>.
/// The invalid input is <c>12a</c>. The overflow input is <c>4294967296</c>.
/// </remarks>
[MemoryDiagnoser]
[SuppressMessage("Maintainability", "CA1515:Consider making public types internal", Justification = "BenchmarkDotNet requires benchmark classes to be public.")]
public class UInt32ParsingBaseline
{
    private const string _medium = "123456";

    private CultureInfo _provider = null!;
    private uint _value;
    private string _text = null!;
    private byte[] _utf8 = null!;
    private char[] _destination = null!;

    /// <summary>
    /// Stores the invariant provider and the medium input.
    /// </summary>
    [GlobalSetup]
    public void Setup()
    {
        _provider = CultureInfo.InvariantCulture;
        _value = 123456;
        _text = _medium;
        _utf8 = System.Text.Encoding.UTF8.GetBytes(_medium);
        _destination = new char[32];
    }

    /// <summary>
    /// Parses a decimal integer with <see cref="uint.TryParse(string?, IFormatProvider?, out uint)"/>.
    /// </summary>
    /// <param name="text">The input text.</param>
    /// <returns>The parsed value, or <c>0</c> when parsing fails.</returns>
    [Benchmark]
    [Arguments("1")]
    [Arguments(_medium)]
    [Arguments("0")]
    [Arguments("4294967295")]
    [Arguments("12a")]
    [Arguments("4294967296")]
    public uint TryParse(string text)
    {
        uint.TryParse(text, _provider, out uint value);
        return value;
    }

    /// <summary>
    /// Parses the medium value with <see cref="uint.Parse(string, IFormatProvider?)"/>.
    /// </summary>
    /// <returns>The parsed value.</returns>
    [Benchmark]
    public uint Parse()
    {
        return uint.Parse(_text, _provider);
    }

    /// <summary>
    /// Parses the UTF-8 encoding of the medium value.
    /// </summary>
    /// <returns>The parsed value.</returns>
    [Benchmark]
    public uint TryParseUtf8()
    {
        uint.TryParse(_utf8, _provider, out uint value);
        return value;
    }

    /// <summary>
    /// Formats the medium value with <see cref="uint.ToString(IFormatProvider?)"/>.
    /// </summary>
    /// <returns>The formatted text.</returns>
    [Benchmark]
    public string Format()
    {
        return _value.ToString(_provider);
    }

    /// <summary>
    /// Formats the medium value with <see cref="uint.TryFormat(Span{char}, out int, ReadOnlySpan{char}, IFormatProvider?)"/>.
    /// </summary>
    /// <returns>The number of characters written.</returns>
    [Benchmark]
    public int TryFormat()
    {
        _value.TryFormat(_destination, out int written, default, _provider);
        return written;
    }
}

/// <summary>
/// Baseline for <see cref="long"/> parsing and formatting in the reference runtime.
/// </summary>
/// <remarks>
/// The provider is <see cref="CultureInfo.InvariantCulture"/> and the style is the default <see cref="NumberStyles.Integer"/>.
/// Valid inputs are <c>1</c> and <c>123456</c>. Boundary inputs are <see cref="long.MaxValue"/> and <see cref="long.MinValue"/>.
/// The invalid input is <c>12a</c>. The overflow input is <c>9223372036854775808</c>.
/// </remarks>
[MemoryDiagnoser]
[SuppressMessage("Maintainability", "CA1515:Consider making public types internal", Justification = "BenchmarkDotNet requires benchmark classes to be public.")]
public class Int64ParsingBaseline
{
    private const string _medium = "123456";

    private CultureInfo _provider = null!;
    private long _value;
    private string _text = null!;
    private byte[] _utf8 = null!;
    private char[] _destination = null!;

    /// <summary>
    /// Stores the invariant provider and the medium input.
    /// </summary>
    [GlobalSetup]
    public void Setup()
    {
        _provider = CultureInfo.InvariantCulture;
        _value = 123456;
        _text = _medium;
        _utf8 = System.Text.Encoding.UTF8.GetBytes(_medium);
        _destination = new char[32];
    }

    /// <summary>
    /// Parses a decimal integer with <see cref="long.TryParse(string?, IFormatProvider?, out long)"/>.
    /// </summary>
    /// <param name="text">The input text.</param>
    /// <returns>The parsed value, or <c>0</c> when parsing fails.</returns>
    [Benchmark]
    [Arguments("1")]
    [Arguments(_medium)]
    [Arguments("9223372036854775807")]
    [Arguments("-9223372036854775808")]
    [Arguments("12a")]
    [Arguments("9223372036854775808")]
    public long TryParse(string text)
    {
        long.TryParse(text, _provider, out long value);
        return value;
    }

    /// <summary>
    /// Parses the medium value with <see cref="long.Parse(string, IFormatProvider?)"/>.
    /// </summary>
    /// <returns>The parsed value.</returns>
    [Benchmark]
    public long Parse()
    {
        return long.Parse(_text, _provider);
    }

    /// <summary>
    /// Parses the UTF-8 encoding of the medium value.
    /// </summary>
    /// <returns>The parsed value.</returns>
    [Benchmark]
    public long TryParseUtf8()
    {
        long.TryParse(_utf8, _provider, out long value);
        return value;
    }

    /// <summary>
    /// Formats the medium value with <see cref="long.ToString(IFormatProvider?)"/>.
    /// </summary>
    /// <returns>The formatted text.</returns>
    [Benchmark]
    public string Format()
    {
        return _value.ToString(_provider);
    }

    /// <summary>
    /// Formats the medium value with <see cref="long.TryFormat(Span{char}, out int, ReadOnlySpan{char}, IFormatProvider?)"/>.
    /// </summary>
    /// <returns>The number of characters written.</returns>
    [Benchmark]
    public int TryFormat()
    {
        _value.TryFormat(_destination, out int written, default, _provider);
        return written;
    }
}

/// <summary>
/// Baseline for <see cref="ulong"/> parsing and formatting in the reference runtime.
/// </summary>
/// <remarks>
/// The provider is <see cref="CultureInfo.InvariantCulture"/> and the style is the default <see cref="NumberStyles.Integer"/>.
/// Valid inputs are <c>1</c> and <c>123456</c>. Boundary inputs are <c>0</c> and <see cref="ulong.MaxValue"/>.
/// The invalid input is <c>12a</c>. The overflow input is <c>18446744073709551616</c>.
/// </remarks>
[MemoryDiagnoser]
[SuppressMessage("Maintainability", "CA1515:Consider making public types internal", Justification = "BenchmarkDotNet requires benchmark classes to be public.")]
public class UInt64ParsingBaseline
{
    private const string _medium = "123456";

    private CultureInfo _provider = null!;
    private ulong _value;
    private string _text = null!;
    private byte[] _utf8 = null!;
    private char[] _destination = null!;

    /// <summary>
    /// Stores the invariant provider and the medium input.
    /// </summary>
    [GlobalSetup]
    public void Setup()
    {
        _provider = CultureInfo.InvariantCulture;
        _value = 123456;
        _text = _medium;
        _utf8 = System.Text.Encoding.UTF8.GetBytes(_medium);
        _destination = new char[32];
    }

    /// <summary>
    /// Parses a decimal integer with <see cref="ulong.TryParse(string?, IFormatProvider?, out ulong)"/>.
    /// </summary>
    /// <param name="text">The input text.</param>
    /// <returns>The parsed value, or <c>0</c> when parsing fails.</returns>
    [Benchmark]
    [Arguments("1")]
    [Arguments(_medium)]
    [Arguments("0")]
    [Arguments("18446744073709551615")]
    [Arguments("12a")]
    [Arguments("18446744073709551616")]
    public ulong TryParse(string text)
    {
        ulong.TryParse(text, _provider, out ulong value);
        return value;
    }

    /// <summary>
    /// Parses the medium value with <see cref="ulong.Parse(string, IFormatProvider?)"/>.
    /// </summary>
    /// <returns>The parsed value.</returns>
    [Benchmark]
    public ulong Parse()
    {
        return ulong.Parse(_text, _provider);
    }

    /// <summary>
    /// Parses the UTF-8 encoding of the medium value.
    /// </summary>
    /// <returns>The parsed value.</returns>
    [Benchmark]
    public ulong TryParseUtf8()
    {
        ulong.TryParse(_utf8, _provider, out ulong value);
        return value;
    }

    /// <summary>
    /// Formats the medium value with <see cref="ulong.ToString(IFormatProvider?)"/>.
    /// </summary>
    /// <returns>The formatted text.</returns>
    [Benchmark]
    public string Format()
    {
        return _value.ToString(_provider);
    }

    /// <summary>
    /// Formats the medium value with <see cref="ulong.TryFormat(Span{char}, out int, ReadOnlySpan{char}, IFormatProvider?)"/>.
    /// </summary>
    /// <returns>The number of characters written.</returns>
    [Benchmark]
    public int TryFormat()
    {
        _value.TryFormat(_destination, out int written, default, _provider);
        return written;
    }
}
