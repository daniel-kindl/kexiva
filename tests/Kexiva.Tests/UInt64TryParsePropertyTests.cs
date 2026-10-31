using System;
using System.Globalization;
using System.Text;
using CsCheck;
using Xunit;

namespace Kexiva.Tests;

/// <summary>
/// Samples text and compares <see cref="UInt64.TryParse(string?, out ulong)"/> with <see cref="ulong.TryParse(string?, out ulong)"/>.
/// </summary>
public sealed class UInt64TryParsePropertyTests
{
    /// <summary>
    /// Verifies a fixed sample of strings for the main number styles.
    /// </summary>
    [Fact]
    public void SampledText_MatchesReferenceTryParse()
    {
        NumberStyles[] styles =
        [
            NumberStyles.Integer,
            NumberStyles.Number,
            NumberStyles.HexNumber,
            NumberStyles.Any,
        ];
        CultureInfo[] cultures =
        [
            CultureInfo.InvariantCulture,
            CultureInfo.GetCultureInfo("en-US"),
            CultureInfo.GetCultureInfo("fr-FR"),
            CultureInfo.GetCultureInfo("sv-SE"),
        ];

        Gen.String.Sample(
            text =>
            {
                if (text is null)
                {
                    return true;
                }

                foreach (NumberStyles style in styles)
                {
                    foreach (CultureInfo culture in cultures)
                    {
                        if (!Same(text, style, culture))
                        {
                            return false;
                        }
                    }
                }

                return true;
            },
            seed: "000000000025",
            iter: 40);
    }

    /// <summary>
    /// Verifies structured samples of valid numbers, boundaries, signs, whitespace, exponents, and formatted text.
    /// </summary>
    [Fact]
    public void StructuredText_MatchesReferenceTryParse()
    {
        CultureInfo invariant = CultureInfo.InvariantCulture;
        CultureInfo enUs = CultureInfo.GetCultureInfo("en-US");
        CultureInfo frFr = CultureInfo.GetCultureInfo("fr-FR");
        CultureInfo svSe = CultureInfo.GetCultureInfo("sv-SE");
        CultureInfo[] cultures = [invariant, enUs, frFr, svSe];
        NumberStyles[] integerStyles = [NumberStyles.Integer, NumberStyles.Number, NumberStyles.Any];
        NumberStyles[] floatStyles = [NumberStyles.Float, NumberStyles.Any, NumberStyles.Number];
        NumberStyles[] formatStyles =
        [
            NumberStyles.Number,
            NumberStyles.Currency,
            NumberStyles.HexNumber,
            NumberStyles.BinaryNumber,
            NumberStyles.Any,
        ];

        Gen<string> valid = Gen.Select(Gen.ULong, value => value.ToString(invariant));
        Gen<string> leadingZeros = Gen.Select(Gen.ULong, value => "000" + value.ToString(invariant));
        Gen<string> boundaries = Gen.OneOf(
        [
            "0",
            "1",
            "-0",
            "+0",
            "-1",
            "18446744073709551614",
            "18446744073709551615",
            "18446744073709551616",
            "000000000018446744073709551615",
            "000000000018446744073709551616",
            "9999999999999999999",
            "10000000000000000000",
        ]);
        Gen<string> nearMax = Gen.Select(Gen.ULong, value => (ulong.MaxValue - (value % 32UL)).ToString(invariant));
        Gen<string> nearMin = Gen.Select(Gen.ULong, value => (value % 32UL).ToString(invariant));
        Gen<string> overflowDigit = Gen.Select(Gen.ULong, value => value.ToString(invariant) + "0");
        Gen<string> signs = Gen.Select(
            Gen.OneOf(["+", "-", "\u2212", "\u2012", "\u207B", "\u208B", "\u2796", "\uFE63", "\uFF0D"]),
            Gen.ULong,
            (sign, value) => sign + value.ToString(invariant));
        Gen<string> whitespace = Gen.Select(
            Gen.OneOf([" ", "\t", "\n", "\r", "\v", "\f"]),
            Gen.ULong,
            (white, value) => white + white + value.ToString(invariant) + white);
        Gen<string> exponents = Gen.Select(
            Gen.ULong,
            Gen.ULong,
            (value, power) =>
            {
                string marker = (value & 1UL) == 0 ? "e" : "E";
                string exponentSign = (power & 2UL) == 0 ? string.Empty : "-";
                ulong exponent = power % 13UL;
                return (value % 100000UL).ToString(invariant) + marker + exponentSign + exponent.ToString(invariant);
            });
        Gen<string> fractions = Gen.Select(
            Gen.ULong,
            value => (value % 10000UL).ToString(invariant) + "." + (value % 1000UL).ToString(invariant));
        Gen<string> hex = Gen.Select(Gen.ULong, value => value.ToString("X", invariant));
        Gen<string> hexSpace = Gen.Select(Gen.ULong, value => " " + value.ToString("X16", invariant));
        Gen<string> binary = Gen.Select(Gen.ULong, ToBinary);
        Gen<string> binaryOnes = Gen.Select(Gen.ULong, value => new string('1', (int)(value % 65UL) + 1));
        Gen<string> groupedEnUs = Gen.Select(Gen.ULong, value => value.ToString("N0", enUs));
        Gen<string> groupedFrFr = Gen.Select(Gen.ULong, value => value.ToString("N0", frFr));
        Gen<string> currency = Gen.Select(Gen.ULong, value => (value % 100000UL).ToString("C0", enUs));
        Gen<string> parentheses = Gen.Select(Gen.ULong, value => "(" + (value % 1000UL).ToString(invariant) + ")");

        Sample(valid, "000000000082", integerStyles, cultures);
        Sample(leadingZeros, "000000000083", integerStyles, cultures);
        Sample(boundaries, "000000000084", integerStyles, cultures);
        Sample(nearMax, "000000000085", integerStyles, cultures);
        Sample(nearMin, "000000000086", integerStyles, cultures);
        Sample(overflowDigit, "000000000087", integerStyles, cultures);
        Sample(signs, "000000000088", integerStyles, cultures);
        Sample(whitespace, "000000000089", integerStyles, cultures);
        Sample(exponents, "000000000090", floatStyles, cultures);
        Sample(fractions, "000000000091", floatStyles, cultures);
        Sample(hex, "000000000092", formatStyles, cultures);
        Sample(hexSpace, "000000000093", formatStyles, cultures);
        Sample(binary, "000000000094", formatStyles, cultures);
        Sample(binaryOnes, "000000000095", formatStyles, cultures);
        Sample(groupedEnUs, "000000000096", formatStyles, cultures);
        Sample(groupedFrFr, "000000000097", formatStyles, cultures);
        Sample(currency, "000000000098", formatStyles, cultures);
        Sample(parentheses, "000000000099", formatStyles, cultures);
        Sample(signs, "000000000100", floatStyles, [svSe, invariant]);
    }

    private static string ToBinary(ulong value)
    {
        if (value == 0)
        {
            return "0";
        }

        Span<char> buffer = stackalloc char[64];
        int index = buffer.Length;
        while (value != 0)
        {
            index--;
            buffer[index] = (value & 1UL) == 0 ? '0' : '1';
            value >>= 1;
        }

        return new string(buffer[index..]);
    }

    private static void Sample(Gen<string> texts, string seed, NumberStyles[] styles, CultureInfo[] cultures)
    {
        texts.Sample(
            text => Matches(text, styles, cultures),
            seed: seed,
            iter: 100);
    }

    private static bool Matches(string text, NumberStyles[] styles, CultureInfo[] cultures)
    {
        foreach (NumberStyles style in styles)
        {
            foreach (CultureInfo culture in cultures)
            {
                if (!Same(text, style, culture) || !SameSpan(text, style, culture))
                {
                    return false;
                }
            }
        }

        return true;
    }

    private static bool SameSpan(string text, NumberStyles style, CultureInfo culture)
    {
        bool expectedOk = ulong.TryParse(text.AsSpan(), style, culture, out ulong expected);
        bool actualOk = UInt64.TryParse(text.AsSpan(), style, culture, out ulong actual);
        return expectedOk == actualOk && expected == actual;
    }

    private static bool Same(string text, NumberStyles style, CultureInfo culture)
    {
        if (!SameString(text, style, culture))
        {
            return false;
        }

        byte[] utf8 = Encoding.UTF8.GetBytes(text);
        return SameUtf8(utf8, style, culture);
    }

    private static bool SameString(string text, NumberStyles style, CultureInfo culture)
    {
        ArgumentException? expectedException = null;
        bool expectedOk = false;
        ulong expected = 0;
        try
        {
            expectedOk = ulong.TryParse(text, style, culture, out expected);
        }
        catch (ArgumentException exception)
        {
            expectedException = exception;
        }

        ArgumentException? actualException = null;
        bool actualOk = false;
        ulong actual = 0;
        try
        {
            actualOk = UInt64.TryParse(text, style, culture, out actual);
        }
        catch (ArgumentException exception)
        {
            actualException = exception;
        }

        return expectedException?.GetType() == actualException?.GetType()
            && (expectedException as ArgumentException)?.ParamName == (actualException as ArgumentException)?.ParamName
            && expectedOk == actualOk
            && expected == actual;
    }

    private static bool SameUtf8(byte[] text, NumberStyles style, CultureInfo culture)
    {
        bool expectedOk = ulong.TryParse(text, style, culture, out ulong expected);
        bool actualOk = UInt64.TryParse(text, style, culture, out ulong actual);
        return expectedOk == actualOk && expected == actual;
    }
}
