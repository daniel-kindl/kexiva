using System;
using System.Globalization;
using System.Text;
using CsCheck;
using Xunit;

namespace Kexiva.Tests;

/// <summary>
/// Samples text and compares <see cref="Int64.TryParse(string?, out long)"/> with <see cref="long.TryParse(string?, out long)"/>.
/// </summary>
public sealed class Int64TryParsePropertyTests
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

        Gen<string> valid = Gen.Select(Gen.Long, value => value.ToString(invariant));
        Gen<string> leadingZeros = Gen.Select(Gen.Long, value => "000" + Magnitude(value).ToString(invariant));
        Gen<string> boundaries = Gen.OneOf(
        [
            "0",
            "1",
            "-1",
            "9223372036854775806",
            "9223372036854775807",
            "9223372036854775808",
            "-9223372036854775807",
            "-9223372036854775808",
            "-9223372036854775809",
            "00000000009223372036854775807",
            "00000000009223372036854775808",
            "-0000000009223372036854775808",
            "9999999999999999999",
            "10000000000000000000",
            "18446744073709551615",
            "18446744073709551616",
        ]);
        Gen<string> nearMax = Gen.Select(Gen.Long, value => (long.MaxValue - (long)((ulong)value % 32)).ToString(invariant));
        Gen<string> nearMin = Gen.Select(Gen.Long, value => (long.MinValue + (long)((ulong)value % 32)).ToString(invariant));
        Gen<string> overflowDigit = Gen.Select(Gen.Long, value => value.ToString(invariant) + "0");
        Gen<string> signs = Gen.Select(
            Gen.OneOf(["+", "-", "\u2212", "\u2012", "\u207B", "\u208B", "\u2796", "\uFE63", "\uFF0D"]),
            Gen.Long,
            (sign, value) => sign + Magnitude(value).ToString(invariant));
        Gen<string> whitespace = Gen.Select(
            Gen.OneOf([" ", "\t", "\n", "\r", "\v", "\f"]),
            Gen.Long,
            (white, value) => white + white + value.ToString(invariant) + white);
        Gen<string> exponents = Gen.Select(
            Gen.ULong,
            Gen.Long,
            (value, power) =>
            {
                string marker = (value & 1) == 0 ? "e" : "E";
                long exponent = power % 13;
                return (value % 100000).ToString(invariant) + marker + exponent.ToString(invariant);
            });
        Gen<string> fractions = Gen.Select(
            Gen.ULong,
            value => (value % 10000).ToString(invariant) + "." + (value % 1000).ToString(invariant));
        Gen<string> hex = Gen.Select(Gen.Long, value => value.ToString("X", invariant));
        Gen<string> hexSpace = Gen.Select(Gen.Long, value => " " + value.ToString("X16", invariant));
        Gen<string> binary = Gen.Select(Gen.Long, value => Convert.ToString(value, 2));
        Gen<string> binaryOnes = Gen.Select(Gen.ULong, value => new string('1', (int)(value % 65) + 1));
        Gen<string> groupedEnUs = Gen.Select(Gen.Long, value => value.ToString("N0", enUs));
        Gen<string> groupedFrFr = Gen.Select(Gen.Long, value => value.ToString("N0", frFr));
        Gen<string> currency = Gen.Select(Gen.Long, value => (value % 100000).ToString("C0", enUs));
        Gen<string> parentheses = Gen.Select(Gen.Long, value => "(" + (Magnitude(value) % 1000).ToString(invariant) + ")");

        Sample(valid, "000000000063", integerStyles, cultures);
        Sample(leadingZeros, "000000000064", integerStyles, cultures);
        Sample(boundaries, "000000000065", integerStyles, cultures);
        Sample(nearMax, "000000000066", integerStyles, cultures);
        Sample(nearMin, "000000000067", integerStyles, cultures);
        Sample(overflowDigit, "000000000068", integerStyles, cultures);
        Sample(signs, "000000000069", integerStyles, cultures);
        Sample(whitespace, "000000000070", integerStyles, cultures);
        Sample(exponents, "000000000071", floatStyles, cultures);
        Sample(fractions, "000000000072", floatStyles, cultures);
        Sample(hex, "000000000073", formatStyles, cultures);
        Sample(hexSpace, "000000000074", formatStyles, cultures);
        Sample(binary, "000000000075", formatStyles, cultures);
        Sample(binaryOnes, "000000000076", formatStyles, cultures);
        Sample(groupedEnUs, "000000000077", formatStyles, cultures);
        Sample(groupedFrFr, "000000000078", formatStyles, cultures);
        Sample(currency, "000000000079", formatStyles, cultures);
        Sample(parentheses, "000000000080", formatStyles, cultures);
        Sample(signs, "000000000081", floatStyles, [svSe, invariant]);
    }

    private static ulong Magnitude(long value)
    {
        if (value >= 0)
        {
            return (ulong)value;
        }

        return (ulong)(-(value + 1)) + 1UL;
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
        bool expectedOk = long.TryParse(text.AsSpan(), style, culture, out long expected);
        bool actualOk = Int64.TryParse(text.AsSpan(), style, culture, out long actual);
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
        long expected = 0;
        try
        {
            expectedOk = long.TryParse(text, style, culture, out expected);
        }
        catch (ArgumentException exception)
        {
            expectedException = exception;
        }

        ArgumentException? actualException = null;
        bool actualOk = false;
        long actual = 0;
        try
        {
            actualOk = Int64.TryParse(text, style, culture, out actual);
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
        bool expectedOk = long.TryParse(text, style, culture, out long expected);
        bool actualOk = Int64.TryParse(text, style, culture, out long actual);
        return expectedOk == actualOk && expected == actual;
    }
}
