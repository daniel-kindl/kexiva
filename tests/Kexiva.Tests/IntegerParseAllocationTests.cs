using System;
using System.Globalization;
using System.Text;
using Xunit;

namespace Kexiva.Tests;

/// <summary>
/// Verifies allocation-free successful Parse calls on string and span inputs.
/// </summary>
public sealed class IntegerParseAllocationTests
{
    /// <summary>
    /// Tests every width, radix, and long custom symbols after warmup.
    /// </summary>
    [Fact]
    public void SuccessfulPaths_DoNotAllocate()
    {
        NumberFormatInfo info = (NumberFormatInfo)CultureInfo.InvariantCulture.NumberFormat.Clone();
        info.PositiveSign = new string('A', 300);
        info.NegativeSign = new string('\u2212', 40);
        info.NumberGroupSeparator = new string('\u202F', 80);
        info.CurrencySymbol = new string('€', 90);
        foreach ((string text, NumberStyles style, decimal value) in new (string, NumberStyles, decimal)[]
        {
            ("123456", NumberStyles.Integer, 123456),
            (info.PositiveSign + "123456", NumberStyles.Integer, 123456),
            (info.NegativeSign + "0", NumberStyles.Integer, 0),
            ("1" + info.NumberGroupSeparator + "234", NumberStyles.Number, 1234),
            (info.CurrencySymbol + "42", NumberStyles.Currency, 42),
            ("FFFFFFFF", NumberStyles.HexNumber, 4294967295),
            ("101010", NumberStyles.BinaryNumber, 42),
        })
        {
            byte[] utf8 = Encoding.UTF8.GetBytes(text);
            for (int width = 0; width < 4; width++)
            {
                for (int form = 0; form < 3; form++)
                {
                    decimal expected = width == 0 && style == NumberStyles.HexNumber ? -1 : value;
                    for (int warmup = 0; warmup < 32; warmup++)
                    {
                        Parse(width, form, text, utf8, style, info);
                    }

                    long before = GC.GetAllocatedBytesForCurrentThread();
                    decimal actual = Parse(width, form, text, utf8, style, info);
                    long allocated = GC.GetAllocatedBytesForCurrentThread() - before;
                    Assert.Equal(expected, actual);
                    Assert.Equal(0L, allocated);
                }
            }
        }
    }

    private static decimal Parse(int width, int form, string text, byte[] utf8, NumberStyles style, IFormatProvider provider)
    {
        return (width, form) switch
        {
            (0, 0) => Int32.Parse(text, style, provider),
            (0, 1) => Int32.Parse(text.AsSpan(), style, provider),
            (0, 2) => Int32.Parse(utf8, style, provider),
            (1, 0) => UInt32.Parse(text, style, provider),
            (1, 1) => UInt32.Parse(text.AsSpan(), style, provider),
            (1, 2) => UInt32.Parse(utf8, style, provider),
            (2, 0) => Int64.Parse(text, style, provider),
            (2, 1) => Int64.Parse(text.AsSpan(), style, provider),
            (2, 2) => Int64.Parse(utf8, style, provider),
            (3, 0) => UInt64.Parse(text, style, provider),
            (3, 1) => UInt64.Parse(text.AsSpan(), style, provider),
            (3, 2) => UInt64.Parse(utf8, style, provider),
            _ => throw new ArgumentOutOfRangeException(nameof(width)),
        };
    }
}
