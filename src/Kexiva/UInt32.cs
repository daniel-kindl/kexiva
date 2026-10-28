using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;

namespace Kexiva;

/// <summary>
/// Parses text as a <see cref="uint"/> with the same results as <see cref="uint.TryParse(string?, out uint)"/>.
/// </summary>
[SuppressMessage("Naming", "CA1720:Identifier contains type name", Justification = "The type name matches System.UInt32.")]
public static class UInt32
{
    /// <summary>
    /// Parses <paramref name="s"/> with <see cref="NumberStyles.Integer"/> and the current culture.
    /// </summary>
    /// <param name="s">The text to parse. Null returns <see langword="false"/>.</param>
    /// <param name="result">The parsed value, or 0 when parsing fails.</param>
    /// <returns><see langword="true"/> when parsing succeeds.</returns>
    public static bool TryParse(string? s, out uint result)
    {
        return TryParse(s, NumberStyles.Integer, provider: null, out result);
    }

    /// <summary>
    /// Parses <paramref name="s"/> with <see cref="NumberStyles.Integer"/> and the current culture.
    /// </summary>
    /// <param name="s">The text to parse.</param>
    /// <param name="result">The parsed value, or 0 when parsing fails.</param>
    /// <returns><see langword="true"/> when parsing succeeds.</returns>
    public static bool TryParse(ReadOnlySpan<char> s, out uint result)
    {
        return TryParse(s, NumberStyles.Integer, provider: null, out result);
    }

    /// <summary>
    /// Parses <paramref name="utf8Text"/> with <see cref="NumberStyles.Integer"/> and the current culture.
    /// </summary>
    /// <param name="utf8Text">The UTF-8 text to parse.</param>
    /// <param name="result">The parsed value, or 0 when parsing fails.</param>
    /// <returns><see langword="true"/> when parsing succeeds.</returns>
    public static bool TryParse(ReadOnlySpan<byte> utf8Text, out uint result)
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
    public static bool TryParse(string? s, IFormatProvider? provider, out uint result)
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
    public static bool TryParse(ReadOnlySpan<char> s, IFormatProvider? provider, out uint result)
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
    public static bool TryParse(ReadOnlySpan<byte> utf8Text, IFormatProvider? provider, out uint result)
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
    public static bool TryParse(string? s, NumberStyles style, IFormatProvider? provider, out uint result)
    {
        UInt32Parser.ValidateStyle(style);
        if (s is null)
        {
            result = 0;
            return false;
        }

        return UInt32Parser.TryParse(s.AsSpan(), style, NumberFormatInfo.GetInstance(provider), out result);
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
    public static bool TryParse(ReadOnlySpan<char> s, NumberStyles style, IFormatProvider? provider, out uint result)
    {
        UInt32Parser.ValidateStyle(style);
        return UInt32Parser.TryParse(s, style, NumberFormatInfo.GetInstance(provider), out result);
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
    public static bool TryParse(ReadOnlySpan<byte> utf8Text, NumberStyles style, IFormatProvider? provider, out uint result)
    {
        UInt32Parser.ValidateStyle(style);
        return UInt32Parser.TryParse(utf8Text, style, NumberFormatInfo.GetInstance(provider), out result);
    }
}
