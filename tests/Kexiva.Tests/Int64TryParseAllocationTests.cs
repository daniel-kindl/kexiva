using System;
using System.Globalization;
using System.Text;
using Xunit;

namespace Kexiva.Tests;

/// <summary>
/// Verifies that custom signs and separators do not allocate during <see cref="Int64.TryParse(ReadOnlySpan{byte}, NumberStyles, IFormatProvider?, out long)"/>.
/// </summary>
public sealed class Int64TryParseAllocationTests
{
    /// <summary>
    /// Verifies long signs, a long currency symbol, a long group separator, and non-ASCII signs.
    /// </summary>
    [Fact]
    public void CustomFormat_DoesNotAllocate()
    {
        NumberFormatInfo invariant = CultureInfo.InvariantCulture.NumberFormat;
        AssertNoAllocation("123456", NumberStyles.Integer, invariant);

        NumberFormatInfo currency = (NumberFormatInfo)invariant.Clone();
        currency.CurrencySymbol = new string('€', 90);
        AssertNoAllocation(currency.CurrencySymbol + "42", NumberStyles.Currency, currency);

        NumberFormatInfo longSign = (NumberFormatInfo)invariant.Clone();
        longSign.PositiveSign = new string('A', 300);
        longSign.NegativeSign = new string('\u2212', 40);
        AssertNoAllocation(longSign.PositiveSign + "12", NumberStyles.Integer, longSign);
        AssertNoAllocation(longSign.PositiveSign + "9223372036854775807", NumberStyles.Integer, longSign);
        AssertNoAllocation(longSign.NegativeSign + "7", NumberStyles.Integer, longSign);
        AssertNoAllocation(longSign.NegativeSign + "9223372036854775808", NumberStyles.Integer, longSign);

        NumberFormatInfo grouped = (NumberFormatInfo)invariant.Clone();
        grouped.NumberGroupSeparator = new string('\u202F', 80);
        AssertNoAllocation("1" + grouped.NumberGroupSeparator + "234", NumberStyles.Number, grouped);

        NumberFormatInfo surrogate = (NumberFormatInfo)invariant.Clone();
        surrogate.PositiveSign = "\uD800";
        AssertNoAllocation(surrogate.PositiveSign + "1", NumberStyles.Integer, surrogate);

        NumberFormatInfo emoji = (NumberFormatInfo)invariant.Clone();
        emoji.PositiveSign = char.ConvertFromUtf32(0x1F600);
        AssertNoAllocation(emoji.PositiveSign + "5", NumberStyles.Integer, emoji);
    }

    private static void AssertNoAllocation(string text, NumberStyles style, NumberFormatInfo info)
    {
        bool expectedOk = long.TryParse(text, style, info, out long expected);
        bool expectedSpan = long.TryParse(text.AsSpan(), style, info, out long expectedSpanValue);
        byte[] utf8 = Encoding.UTF8.GetBytes(text);
        bool expectedUtf8 = long.TryParse(utf8, style, info, out long expectedUtf8Value);

        AssertMeasured(text, style, info, expectedOk, expected);
        AssertMeasured(text.AsSpan(), style, info, expectedSpan, expectedSpanValue);
        AssertMeasured(utf8, style, info, expectedUtf8, expectedUtf8Value);
    }

    private static void AssertMeasured(string text, NumberStyles style, NumberFormatInfo info, bool expectedOk, long expected)
    {
        for (int i = 0; i < 8; i++)
        {
            Int64.TryParse(text, style, info, out _);
        }

        long before = GC.GetAllocatedBytesForCurrentThread();
        bool actualOk = Int64.TryParse(text, style, info, out long actual);
        long after = GC.GetAllocatedBytesForCurrentThread();
        Assert.Equal(expectedOk, actualOk);
        Assert.Equal(expected, actual);
        Assert.Equal(0L, after - before);
    }

    private static void AssertMeasured(ReadOnlySpan<char> text, NumberStyles style, NumberFormatInfo info, bool expectedOk, long expected)
    {
        for (int i = 0; i < 8; i++)
        {
            Int64.TryParse(text, style, info, out _);
        }

        long before = GC.GetAllocatedBytesForCurrentThread();
        bool actualOk = Int64.TryParse(text, style, info, out long actual);
        long after = GC.GetAllocatedBytesForCurrentThread();
        Assert.Equal(expectedOk, actualOk);
        Assert.Equal(expected, actual);
        Assert.Equal(0L, after - before);
    }

    private static void AssertMeasured(byte[] text, NumberStyles style, NumberFormatInfo info, bool expectedOk, long expected)
    {
        for (int i = 0; i < 8; i++)
        {
            Int64.TryParse(text, style, info, out _);
        }

        long before = GC.GetAllocatedBytesForCurrentThread();
        bool actualOk = Int64.TryParse(text, style, info, out long actual);
        long after = GC.GetAllocatedBytesForCurrentThread();
        Assert.Equal(expectedOk, actualOk);
        Assert.Equal(expected, actual);
        Assert.Equal(0L, after - before);
    }
}
