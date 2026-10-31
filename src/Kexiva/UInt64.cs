using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;

namespace Kexiva;

/// <summary>
/// Parses text as a <see cref="ulong"/> with .NET-compatible values and exceptions.
/// </summary>
[SuppressMessage("Naming", "CA1720:Identifier contains type name", Justification = "The type name matches System.UInt64.")]
public static class UInt64
{
    /// <summary>
    /// Parses the text as a <see cref="ulong"/>.
    /// </summary>
    /// <param name="s">The text to parse.</param>
    /// <returns>The parsed value.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="s"/> is null.</exception>
    /// <exception cref="FormatException">The text does not have the required format.</exception>
    /// <exception cref="OverflowException">The value is outside the range of <see cref="ulong"/>.</exception>
    public static ulong Parse(string s)
    {
        return Parse(s, NumberStyles.Integer, provider: null);
    }

    /// <summary>
    /// Parses the text as a <see cref="ulong"/>.
    /// </summary>
    /// <param name="s">The text to parse.</param>
    /// <param name="style">The permitted number style.</param>
    /// <returns>The parsed value.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="s"/> is null.</exception>
    /// <exception cref="ArgumentException"><paramref name="style"/> is not a valid integer style.</exception>
    /// <exception cref="FormatException">The text does not have the required format.</exception>
    /// <exception cref="OverflowException">The value is outside the range of <see cref="ulong"/>.</exception>
    public static ulong Parse(string s, NumberStyles style)
    {
        return Parse(s, style, provider: null);
    }

    /// <summary>
    /// Parses the text as a <see cref="ulong"/>.
    /// </summary>
    /// <param name="s">The text to parse.</param>
    /// <param name="provider">The format provider. Null uses the current culture.</param>
    /// <returns>The parsed value.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="s"/> is null.</exception>
    /// <exception cref="FormatException">The text does not have the required format.</exception>
    /// <exception cref="OverflowException">The value is outside the range of <see cref="ulong"/>.</exception>
    public static ulong Parse(string s, IFormatProvider? provider)
    {
        return Parse(s, NumberStyles.Integer, provider);
    }

    /// <summary>
    /// Parses the text as a <see cref="ulong"/>.
    /// </summary>
    /// <param name="s">The text to parse.</param>
    /// <param name="style">The permitted number style.</param>
    /// <param name="provider">The format provider. Null uses the current culture.</param>
    /// <returns>The parsed value.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="s"/> is null.</exception>
    /// <exception cref="ArgumentException"><paramref name="style"/> is not a valid integer style.</exception>
    /// <exception cref="FormatException">The text does not have the required format.</exception>
    /// <exception cref="OverflowException">The value is outside the range of <see cref="ulong"/>.</exception>
    public static ulong Parse(string s, NumberStyles style, IFormatProvider? provider)
    {
        ArgumentNullException.ThrowIfNull(s);
        UInt64Parser.ValidateStyle(style);
        NumberFormatInfo info = NumberFormatInfo.GetInstance(provider);
        if (UInt64Parser.TryParse(s.AsSpan(), style, info, out ulong result))
        {
            return result;
        }

        return ulong.Parse(s, style, info);
    }

    /// <summary>
    /// Parses the text as a <see cref="ulong"/>.
    /// </summary>
    /// <param name="s">The text to parse.</param>
    /// <param name="style">The permitted number style.</param>
    /// <param name="provider">The format provider. Null uses the current culture.</param>
    /// <returns>The parsed value.</returns>
    /// <exception cref="ArgumentException"><paramref name="style"/> is not a valid integer style.</exception>
    /// <exception cref="FormatException">The text does not have the required format.</exception>
    /// <exception cref="OverflowException">The value is outside the range of <see cref="ulong"/>.</exception>
    [SuppressMessage("ApiDesign", "RS0027:Public API with optional parameter(s) should have the most parameters amongst its public overloads", Justification = "The span overloads must match the optional arguments and overload set of the corresponding System integer type.")]
    public static ulong Parse(ReadOnlySpan<char> s, NumberStyles style = NumberStyles.Integer, IFormatProvider? provider = null)
    {
        UInt64Parser.ValidateStyle(style);
        NumberFormatInfo info = NumberFormatInfo.GetInstance(provider);
        if (UInt64Parser.TryParse(s, style, info, out ulong result))
        {
            return result;
        }

        return ulong.Parse(s, style, info);
    }

    /// <summary>
    /// Parses the text as a <see cref="ulong"/>.
    /// </summary>
    /// <param name="s">The text to parse.</param>
    /// <param name="provider">The format provider. Null uses the current culture.</param>
    /// <returns>The parsed value.</returns>
    /// <exception cref="FormatException">The text does not have the required format.</exception>
    /// <exception cref="OverflowException">The value is outside the range of <see cref="ulong"/>.</exception>
    public static ulong Parse(ReadOnlySpan<char> s, IFormatProvider? provider)
    {
        return Parse(s, NumberStyles.Integer, provider);
    }

    /// <summary>
    /// Parses the text as a <see cref="ulong"/>.
    /// </summary>
    /// <param name="utf8Text">The UTF-8 text to parse.</param>
    /// <param name="style">The permitted number style.</param>
    /// <param name="provider">The format provider. Null uses the current culture.</param>
    /// <returns>The parsed value.</returns>
    /// <exception cref="ArgumentException"><paramref name="style"/> is not a valid integer style.</exception>
    /// <exception cref="FormatException">The text does not have the required format.</exception>
    /// <exception cref="OverflowException">The value is outside the range of <see cref="ulong"/>.</exception>
    [SuppressMessage("ApiDesign", "RS0027:Public API with optional parameter(s) should have the most parameters amongst its public overloads", Justification = "The span overloads must match the optional arguments and overload set of the corresponding System integer type.")]
    public static ulong Parse(ReadOnlySpan<byte> utf8Text, NumberStyles style = NumberStyles.Integer, IFormatProvider? provider = null)
    {
        UInt64Parser.ValidateStyle(style);
        NumberFormatInfo info = NumberFormatInfo.GetInstance(provider);
        if (UInt64Parser.TryParse(utf8Text, style, info, out ulong result))
        {
            return result;
        }

        return ulong.Parse(utf8Text, style, info);
    }

    /// <summary>
    /// Parses the text as a <see cref="ulong"/>.
    /// </summary>
    /// <param name="utf8Text">The UTF-8 text to parse.</param>
    /// <param name="provider">The format provider. Null uses the current culture.</param>
    /// <returns>The parsed value.</returns>
    /// <exception cref="FormatException">The text does not have the required format.</exception>
    /// <exception cref="OverflowException">The value is outside the range of <see cref="ulong"/>.</exception>
    public static ulong Parse(ReadOnlySpan<byte> utf8Text, IFormatProvider? provider)
    {
        return Parse(utf8Text, NumberStyles.Integer, provider);
    }

    /// <summary>
    /// Parses <paramref name="s"/> with <see cref="NumberStyles.Integer"/> and the current culture.
    /// </summary>
    /// <param name="s">The text to parse. Null returns <see langword="false"/>.</param>
    /// <param name="result">The parsed value, or 0 when parsing fails.</param>
    /// <returns><see langword="true"/> when parsing succeeds.</returns>
    public static bool TryParse(string? s, out ulong result)
    {
        return TryParse(s, NumberStyles.Integer, provider: null, out result);
    }

    /// <summary>
    /// Parses <paramref name="s"/> with <see cref="NumberStyles.Integer"/> and the current culture.
    /// </summary>
    /// <param name="s">The text to parse.</param>
    /// <param name="result">The parsed value, or 0 when parsing fails.</param>
    /// <returns><see langword="true"/> when parsing succeeds.</returns>
    public static bool TryParse(ReadOnlySpan<char> s, out ulong result)
    {
        return TryParse(s, NumberStyles.Integer, provider: null, out result);
    }

    /// <summary>
    /// Parses <paramref name="utf8Text"/> with <see cref="NumberStyles.Integer"/> and the current culture.
    /// </summary>
    /// <param name="utf8Text">The UTF-8 text to parse.</param>
    /// <param name="result">The parsed value, or 0 when parsing fails.</param>
    /// <returns><see langword="true"/> when parsing succeeds.</returns>
    public static bool TryParse(ReadOnlySpan<byte> utf8Text, out ulong result)
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
    public static bool TryParse(string? s, IFormatProvider? provider, out ulong result)
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
    public static bool TryParse(ReadOnlySpan<char> s, IFormatProvider? provider, out ulong result)
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
    public static bool TryParse(ReadOnlySpan<byte> utf8Text, IFormatProvider? provider, out ulong result)
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
    public static bool TryParse(string? s, NumberStyles style, IFormatProvider? provider, out ulong result)
    {
        UInt64Parser.ValidateStyle(style);
        if (s is null)
        {
            result = 0;
            return false;
        }

        return UInt64Parser.TryParse(s.AsSpan(), style, NumberFormatInfo.GetInstance(provider), out result);
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
    public static bool TryParse(ReadOnlySpan<char> s, NumberStyles style, IFormatProvider? provider, out ulong result)
    {
        UInt64Parser.ValidateStyle(style);
        return UInt64Parser.TryParse(s, style, NumberFormatInfo.GetInstance(provider), out result);
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
    public static bool TryParse(ReadOnlySpan<byte> utf8Text, NumberStyles style, IFormatProvider? provider, out ulong result)
    {
        UInt64Parser.ValidateStyle(style);
        return UInt64Parser.TryParse(utf8Text, style, NumberFormatInfo.GetInstance(provider), out result);
    }
}
