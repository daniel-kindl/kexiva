using System;
using System.Globalization;
using System.Text;
using CsCheck;
using Xunit;

namespace Kexiva.Tests;

/// <summary>
/// Samples text and compares <see cref="UInt32.TryParse(string?, out uint)"/> with <see cref="uint.TryParse(string?, out uint)"/>.
/// </summary>
public sealed class UInt32TryParsePropertyTests
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

        Gen<string> valid = Gen.Select(Gen.UInt, value => value.ToString(invariant));
        Gen<string> leadingZeros = Gen.Select(Gen.UInt, value => "000" + value.ToString(invariant));
        Gen<string> boundaries = Gen.OneOf(
        [
            "0",
            "1",
            "4294967294",
            "4294967295",
            "4294967296",
            "42949672950",
            "00000000004294967295",
            "00000000004294967296",
            "9999999999",
            "10000000000",
        ]);
        Gen<string> nearMax = Gen.Select(Gen.UInt, value => (uint.MaxValue - (value % 32)).ToString(invariant));
        Gen<string> overflowDigit = Gen.Select(Gen.UInt, value => value.ToString(invariant) + "0");
        Gen<string> signs = Gen.Select(
            Gen.OneOf(["+", "-", "\u2212", "\u2012", "\u207B", "\u208B", "\u2796", "\uFE63", "\uFF0D"]),
            Gen.UInt,
            (sign, value) => sign + value.ToString(invariant));
        Gen<string> whitespace = Gen.Select(
            Gen.OneOf([" ", "\t", "\n", "\r", "\v", "\f"]),
            Gen.UInt,
            (white, value) => white + white + value.ToString(invariant) + white);
        Gen<string> exponents = Gen.Select(
            Gen.UInt,
            Gen.Int,
            (value, power) =>
            {
                string marker = (value & 1) == 0 ? "e" : "E";
                int exponent = power % 13;
                return (value % 100000).ToString(invariant) + marker + exponent.ToString(invariant);
            });
        Gen<string> fractions = Gen.Select(
            Gen.UInt,
            value => (value % 10000).ToString(invariant) + "." + (value % 1000).ToString(invariant));
        Gen<string> hex = Gen.Select(Gen.UInt, value => value.ToString("X", invariant));
        Gen<string> hexSpace = Gen.Select(Gen.UInt, value => " " + value.ToString("X4", invariant));
        Gen<string> binary = Gen.Select(Gen.UInt, value => Convert.ToString(value, 2));
        Gen<string> binaryOnes = Gen.Select(Gen.UInt, value => new string('1', (int)(value % 33) + 1));
        Gen<string> groupedEnUs = Gen.Select(Gen.UInt, value => value.ToString("N0", enUs));
        Gen<string> groupedFrFr = Gen.Select(Gen.UInt, value => value.ToString("N0", frFr));
        Gen<string> currency = Gen.Select(Gen.UInt, value => (value % 100000).ToString("C0", enUs));
        Gen<string> parentheses = Gen.Select(Gen.UInt, value => "(" + (value % 1000).ToString(invariant) + ")");

        Sample(valid, "000000000026", integerStyles, cultures);
        Sample(leadingZeros, "000000000027", integerStyles, cultures);
        Sample(boundaries, "000000000028", integerStyles, cultures);
        Sample(nearMax, "000000000029", integerStyles, cultures);
        Sample(overflowDigit, "000000000030", integerStyles, cultures);
        Sample(signs, "000000000031", integerStyles, cultures);
        Sample(whitespace, "000000000032", integerStyles, cultures);
        Sample(exponents, "000000000033", floatStyles, cultures);
        Sample(fractions, "000000000034", floatStyles, cultures);
        Sample(hex, "000000000035", formatStyles, cultures);
        Sample(hexSpace, "000000000036", formatStyles, cultures);
        Sample(binary, "000000000037", formatStyles, cultures);
        Sample(binaryOnes, "000000000038", formatStyles, cultures);
        Sample(groupedEnUs, "000000000039", formatStyles, cultures);
        Sample(groupedFrFr, "000000000040", formatStyles, cultures);
        Sample(currency, "000000000041", formatStyles, cultures);
        Sample(parentheses, "000000000042", formatStyles, cultures);
        Sample(signs, "000000000043", floatStyles, [svSe, invariant]);
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
        bool expectedOk = uint.TryParse(text.AsSpan(), style, culture, out uint expected);
        bool actualOk = UInt32.TryParse(text.AsSpan(), style, culture, out uint actual);
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
        uint expected = 0;
        try
        {
            expectedOk = uint.TryParse(text, style, culture, out expected);
        }
        catch (ArgumentException exception)
        {
            expectedException = exception;
        }

        ArgumentException? actualException = null;
        bool actualOk = false;
        uint actual = 0;
        try
        {
            actualOk = UInt32.TryParse(text, style, culture, out actual);
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
        bool expectedOk = uint.TryParse(text, style, culture, out uint expected);
        bool actualOk = UInt32.TryParse(text, style, culture, out uint actual);
        return expectedOk == actualOk && expected == actual;
    }
}
