using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;

namespace Kexiva;

/// <summary>
/// Parses text as a <see cref="long"/> with .NET-compatible values and exceptions.
/// </summary>
[SuppressMessage("Naming", "CA1720:Identifier contains type name", Justification = "The type name matches System.Int64.")]
public static class Int64
{
    /// <summary>
    /// Parses the text as a <see cref="long"/>.
    /// </summary>
    /// <param name="s">The text to parse.</param>
    /// <returns>The parsed value.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="s"/> is null.</exception>
    /// <exception cref="FormatException">The text does not have the required format.</exception>
    /// <exception cref="OverflowException">The value is outside the range of <see cref="long"/>.</exception>
    public static long Parse(string s)
    {
        return Parse(s, NumberStyles.Integer, provider: null);
    }

    /// <summary>
    /// Parses the text as a <see cref="long"/>.
    /// </summary>
    /// <param name="s">The text to parse.</param>
    /// <param name="style">The permitted number style.</param>
    /// <returns>The parsed value.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="s"/> is null.</exception>
    /// <exception cref="ArgumentException"><paramref name="style"/> is not a valid integer style.</exception>
    /// <exception cref="FormatException">The text does not have the required format.</exception>
    /// <exception cref="OverflowException">The value is outside the range of <see cref="long"/>.</exception>
    public static long Parse(string s, NumberStyles style)
    {
        return Parse(s, style, provider: null);
    }

    /// <summary>
    /// Parses the text as a <see cref="long"/>.
    /// </summary>
    /// <param name="s">The text to parse.</param>
    /// <param name="provider">The format provider. Null uses the current culture.</param>
    /// <returns>The parsed value.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="s"/> is null.</exception>
    /// <exception cref="FormatException">The text does not have the required format.</exception>
    /// <exception cref="OverflowException">The value is outside the range of <see cref="long"/>.</exception>
    public static long Parse(string s, IFormatProvider? provider)
    {
        return Parse(s, NumberStyles.Integer, provider);
    }

    /// <summary>
    /// Parses the text as a <see cref="long"/>.
    /// </summary>
    /// <param name="s">The text to parse.</param>
    /// <param name="style">The permitted number style.</param>
    /// <param name="provider">The format provider. Null uses the current culture.</param>
    /// <returns>The parsed value.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="s"/> is null.</exception>
    /// <exception cref="ArgumentException"><paramref name="style"/> is not a valid integer style.</exception>
    /// <exception cref="FormatException">The text does not have the required format.</exception>
    /// <exception cref="OverflowException">The value is outside the range of <see cref="long"/>.</exception>
    public static long Parse(string s, NumberStyles style, IFormatProvider? provider)
    {
        ArgumentNullException.ThrowIfNull(s);
        Int64Parser.ValidateStyle(style);
        NumberFormatInfo info = NumberFormatInfo.GetInstance(provider);
        if (Int64Parser.TryParse(s.AsSpan(), style, info, out long result))
        {
            return result;
        }

        return long.Parse(s, style, info);
    }

    /// <summary>
    /// Parses the text as a <see cref="long"/>.
    /// </summary>
    /// <param name="s">The text to parse.</param>
    /// <param name="style">The permitted number style.</param>
    /// <param name="provider">The format provider. Null uses the current culture.</param>
    /// <returns>The parsed value.</returns>
    /// <exception cref="ArgumentException"><paramref name="style"/> is not a valid integer style.</exception>
    /// <exception cref="FormatException">The text does not have the required format.</exception>
    /// <exception cref="OverflowException">The value is outside the range of <see cref="long"/>.</exception>
    [SuppressMessage("ApiDesign", "RS0027:Public API with optional parameter(s) should have the most parameters amongst its public overloads", Justification = "The span overloads must match the optional arguments and overload set of the corresponding System integer type.")]
    public static long Parse(ReadOnlySpan<char> s, NumberStyles style = NumberStyles.Integer, IFormatProvider? provider = null)
    {
        Int64Parser.ValidateStyle(style);
        NumberFormatInfo info = NumberFormatInfo.GetInstance(provider);
        if (Int64Parser.TryParse(s, style, info, out long result))
        {
            return result;
        }

        return long.Parse(s, style, info);
    }

    /// <summary>
    /// Parses the text as a <see cref="long"/>.
    /// </summary>
    /// <param name="s">The text to parse.</param>
    /// <param name="provider">The format provider. Null uses the current culture.</param>
    /// <returns>The parsed value.</returns>
    /// <exception cref="FormatException">The text does not have the required format.</exception>
    /// <exception cref="OverflowException">The value is outside the range of <see cref="long"/>.</exception>
    public static long Parse(ReadOnlySpan<char> s, IFormatProvider? provider)
    {
        return Parse(s, NumberStyles.Integer, provider);
    }

    /// <summary>
    /// Parses the text as a <see cref="long"/>.
    /// </summary>
    /// <param name="utf8Text">The UTF-8 text to parse.</param>
    /// <param name="style">The permitted number style.</param>
    /// <param name="provider">The format provider. Null uses the current culture.</param>
    /// <returns>The parsed value.</returns>
    /// <exception cref="ArgumentException"><paramref name="style"/> is not a valid integer style.</exception>
    /// <exception cref="FormatException">The text does not have the required format.</exception>
    /// <exception cref="OverflowException">The value is outside the range of <see cref="long"/>.</exception>
    [SuppressMessage("ApiDesign", "RS0027:Public API with optional parameter(s) should have the most parameters amongst its public overloads", Justification = "The span overloads must match the optional arguments and overload set of the corresponding System integer type.")]
    public static long Parse(ReadOnlySpan<byte> utf8Text, NumberStyles style = NumberStyles.Integer, IFormatProvider? provider = null)
    {
        Int64Parser.ValidateStyle(style);
        NumberFormatInfo info = NumberFormatInfo.GetInstance(provider);
        if (Int64Parser.TryParse(utf8Text, style, info, out long result))
        {
            return result;
        }

        return long.Parse(utf8Text, style, info);
    }

    /// <summary>
    /// Parses the text as a <see cref="long"/>.
    /// </summary>
    /// <param name="utf8Text">The UTF-8 text to parse.</param>
    /// <param name="provider">The format provider. Null uses the current culture.</param>
    /// <returns>The parsed value.</returns>
    /// <exception cref="FormatException">The text does not have the required format.</exception>
    /// <exception cref="OverflowException">The value is outside the range of <see cref="long"/>.</exception>
    public static long Parse(ReadOnlySpan<byte> utf8Text, IFormatProvider? provider)
    {
        return Parse(utf8Text, NumberStyles.Integer, provider);
    }

    /// <summary>
    /// Parses <paramref name="s"/> with <see cref="NumberStyles.Integer"/> and the current culture.
    /// </summary>
    /// <param name="s">The text to parse. Null returns <see langword="false"/>.</param>
    /// <param name="result">The parsed value, or 0 when parsing fails.</param>
    /// <returns><see langword="true"/> when parsing succeeds.</returns>
    public static bool TryParse(string? s, out long result)
    {
        return TryParse(s, NumberStyles.Integer, provider: null, out result);
    }

    /// <summary>
    /// Parses <paramref name="s"/> with <see cref="NumberStyles.Integer"/> and the current culture.
    /// </summary>
    /// <param name="s">The text to parse.</param>
    /// <param name="result">The parsed value, or 0 when parsing fails.</param>
    /// <returns><see langword="true"/> when parsing succeeds.</returns>
    public static bool TryParse(ReadOnlySpan<char> s, out long result)
    {
        return TryParse(s, NumberStyles.Integer, provider: null, out result);
    }

    /// <summary>
    /// Parses <paramref name="utf8Text"/> with <see cref="NumberStyles.Integer"/> and the current culture.
    /// </summary>
    /// <param name="utf8Text">The UTF-8 text to parse.</param>
    /// <param name="result">The parsed value, or 0 when parsing fails.</param>
    /// <returns><see langword="true"/> when parsing succeeds.</returns>
    public static bool TryParse(ReadOnlySpan<byte> utf8Text, out long result)
    {
        return TryParse(utf8Text, NumberStyles.Integer, provider: null, out result);
    }

    /// <summary>
    /// Parses <paramref name="s"/> with <see cref="NumberStyles.Integer"/> and <paramref name="provider"/>.
    /// </summary>
    /// <param name="s">The text to parse. Null returns <see langword="false"/>.</param>
    /// <param name="provider">The format provider. Null uses the current culture.</param>
    /// <param name="result">The parsed value, or 0 when parsing fails.</param>
    /// <returns><see langword="true"/> when parsing succeeds.</returns>
    public static bool TryParse(string? s, IFormatProvider? provider, out long result)
    {
        return TryParse(s, NumberStyles.Integer, provider, out result);
    }

    /// <summary>
    /// Parses <paramref name="s"/> with <see cref="NumberStyles.Integer"/> and <paramref name="provider"/>.
    /// </summary>
    /// <param name="s">The text to parse.</param>
    /// <param name="provider">The format provider. Null uses the current culture.</param>
    /// <param name="result">The parsed value, or 0 when parsing fails.</param>
    /// <returns><see langword="true"/> when parsing succeeds.</returns>
    public static bool TryParse(ReadOnlySpan<char> s, IFormatProvider? provider, out long result)
    {
        return TryParse(s, NumberStyles.Integer, provider, out result);
    }

    /// <summary>
    /// Parses <paramref name="utf8Text"/> with <see cref="NumberStyles.Integer"/> and <paramref name="provider"/>.
    /// </summary>
    /// <param name="utf8Text">The UTF-8 text to parse.</param>
    /// <param name="provider">The format provider. Null uses the current culture.</param>
    /// <param name="result">The parsed value, or 0 when parsing fails.</param>
    /// <returns><see langword="true"/> when parsing succeeds.</returns>
    public static bool TryParse(ReadOnlySpan<byte> utf8Text, IFormatProvider? provider, out long result)
    {
        return TryParse(utf8Text, NumberStyles.Integer, provider, out result);
    }

    /// <summary>
    /// Parses <paramref name="s"/> with <paramref name="style"/> and <paramref name="provider"/>.
    /// </summary>
    /// <param name="s">The text to parse. Null returns <see langword="false"/> when <paramref name="style"/> is valid.</param>
    /// <param name="style">The permitted number style.</param>
    /// <param name="provider">The format provider. Null uses the current culture.</param>
    /// <param name="result">The parsed value, or 0 when parsing fails.</param>
    /// <returns><see langword="true"/> when parsing succeeds.</returns>
    /// <exception cref="ArgumentException"><paramref name="style"/> is not a valid integer style. The parameter name is <c>style</c>.</exception>
    public static bool TryParse(string? s, NumberStyles style, IFormatProvider? provider, out long result)
    {
        Int64Parser.ValidateStyle(style);
        if (s is null)
        {
            result = 0;
            return false;
        }

        return Int64Parser.TryParse(s.AsSpan(), style, NumberFormatInfo.GetInstance(provider), out result);
    }

    /// <summary>
    /// Parses <paramref name="s"/> with <paramref name="style"/> and <paramref name="provider"/>.
    /// </summary>
    /// <param name="s">The text to parse.</param>
    /// <param name="style">The permitted number style.</param>
    /// <param name="provider">The format provider. Null uses the current culture.</param>
    /// <param name="result">The parsed value, or 0 when parsing fails.</param>
    /// <returns><see langword="true"/> when parsing succeeds.</returns>
    /// <exception cref="ArgumentException"><paramref name="style"/> is not a valid integer style. The parameter name is <c>style</c>.</exception>
    public static bool TryParse(ReadOnlySpan<char> s, NumberStyles style, IFormatProvider? provider, out long result)
    {
        Int64Parser.ValidateStyle(style);
        return Int64Parser.TryParse(s, style, NumberFormatInfo.GetInstance(provider), out result);
    }

    /// <summary>
    /// Parses <paramref name="utf8Text"/> with <paramref name="style"/> and <paramref name="provider"/>.
    /// </summary>
    /// <param name="utf8Text">The UTF-8 text to parse.</param>
    /// <param name="style">The permitted number style.</param>
    /// <param name="provider">The format provider. Null uses the current culture.</param>
    /// <param name="result">The parsed value, or 0 when parsing fails.</param>
    /// <returns><see langword="true"/> when parsing succeeds.</returns>
    /// <exception cref="ArgumentException"><paramref name="style"/> is not a valid integer style. The parameter name is <c>style</c>.</exception>
    public static bool TryParse(ReadOnlySpan<byte> utf8Text, NumberStyles style, IFormatProvider? provider, out long result)
    {
        Int64Parser.ValidateStyle(style);
        return Int64Parser.TryParse(utf8Text, style, NumberFormatInfo.GetInstance(provider), out result);
    }
}
