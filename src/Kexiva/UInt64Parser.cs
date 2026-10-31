using System;
using System.Globalization;
using System.Runtime.CompilerServices;

namespace Kexiva;

/// <summary>
/// Parses <see cref="ulong"/> text for <see cref="UInt64"/>.
/// </summary>
internal static class UInt64Parser
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static void ValidateStyle(NumberStyles style)
    {
        IntegerParser.ValidateStyle(style);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static bool TryParse(ReadOnlySpan<char> text, NumberStyles styles, NumberFormatInfo info, out ulong result)
    {
        return IntegerParser.TryParse(text, styles, info, out result);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static bool TryParse(ReadOnlySpan<byte> text, NumberStyles styles, NumberFormatInfo info, out ulong result)
    {
        return IntegerParser.TryParse(text, styles, info, out result);
    }
}
