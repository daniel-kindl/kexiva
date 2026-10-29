using System;
using System.Globalization;
using System.Text;
using CsCheck;
using Xunit;

namespace Kexiva.Tests;

/// <summary>
/// Samples text and compares <see cref="Int32.TryParse(string?, out int)"/> with <see cref="int.TryParse(string?, out int)"/>.
/// </summary>
public sealed class Int32TryParsePropertyTests
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

        Gen<string> valid = Gen.Select(Gen.Int, value => value.ToString(invariant));
        Gen<string> leadingZeros = Gen.Select(Gen.Int, value => "000" + (value < 0 ? -(long)value : value).ToString(invariant));
        Gen<string> boundaries = Gen.OneOf(
        [
            "0",
            "1",
            "-1",
            "2147483646",
            "2147483647",
            "2147483648",
            "-2147483647",
            "-2147483648",
            "-2147483649",
            "00000000002147483647",
            "00000000002147483648",
            "-0000000002147483648",
            "9999999999",
            "10000000000",
            "4294967295",
            "4294967296",
        ]);
        Gen<string> nearMax = Gen.Select(Gen.Int, value => (int.MaxValue - (int)((uint)value % 32)).ToString(invariant));
        Gen<string> nearMin = Gen.Select(Gen.Int, value => (int.MinValue + (int)((uint)value % 32)).ToString(invariant));
        Gen<string> overflowDigit = Gen.Select(Gen.Int, value => value.ToString(invariant) + "0");
        Gen<string> signs = Gen.Select(
            Gen.OneOf(["+", "-", "\u2212", "\u2012", "\u207B", "\u208B", "\u2796", "\uFE63", "\uFF0D"]),
            Gen.Int,
            (sign, value) => sign + (value < 0 ? -(long)value : value).ToString(invariant));
        Gen<string> whitespace = Gen.Select(
            Gen.OneOf([" ", "\t", "\n", "\r", "\v", "\f"]),
            Gen.Int,
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
        Gen<string> hex = Gen.Select(Gen.Int, value => value.ToString("X", invariant));
        Gen<string> hexSpace = Gen.Select(Gen.Int, value => " " + value.ToString("X8", invariant));
        Gen<string> binary = Gen.Select(Gen.Int, value => Convert.ToString(value, 2));
        Gen<string> binaryOnes = Gen.Select(Gen.UInt, value => new string('1', (int)(value % 33) + 1));
        Gen<string> groupedEnUs = Gen.Select(Gen.Int, value => value.ToString("N0", enUs));
        Gen<string> groupedFrFr = Gen.Select(Gen.Int, value => value.ToString("N0", frFr));
        Gen<string> currency = Gen.Select(Gen.Int, value => (value % 100000).ToString("C0", enUs));
        Gen<string> parentheses = Gen.Select(Gen.Int, value => "(" + (value < 0 ? -(long)value % 1000 : value % 1000).ToString(invariant) + ")");

        Sample(valid, "000000000044", integerStyles, cultures);
        Sample(leadingZeros, "000000000045", integerStyles, cultures);
        Sample(boundaries, "000000000046", integerStyles, cultures);
        Sample(nearMax, "000000000047", integerStyles, cultures);
        Sample(nearMin, "000000000048", integerStyles, cultures);
        Sample(overflowDigit, "000000000049", integerStyles, cultures);
        Sample(signs, "000000000050", integerStyles, cultures);
        Sample(whitespace, "000000000051", integerStyles, cultures);
        Sample(exponents, "000000000052", floatStyles, cultures);
        Sample(fractions, "000000000053", floatStyles, cultures);
        Sample(hex, "000000000054", formatStyles, cultures);
        Sample(hexSpace, "000000000055", formatStyles, cultures);
        Sample(binary, "000000000056", formatStyles, cultures);
        Sample(binaryOnes, "000000000057", formatStyles, cultures);
        Sample(groupedEnUs, "000000000058", formatStyles, cultures);
        Sample(groupedFrFr, "000000000059", formatStyles, cultures);
        Sample(currency, "000000000060", formatStyles, cultures);
        Sample(parentheses, "000000000061", formatStyles, cultures);
        Sample(signs, "000000000062", floatStyles, [svSe, invariant]);
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
        bool expectedOk = int.TryParse(text.AsSpan(), style, culture, out int expected);
        bool actualOk = Int32.TryParse(text.AsSpan(), style, culture, out int actual);
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
        int expected = 0;
        try
        {
            expectedOk = int.TryParse(text, style, culture, out expected);
        }
        catch (ArgumentException exception)
        {
            expectedException = exception;
        }

        ArgumentException? actualException = null;
        bool actualOk = false;
        int actual = 0;
        try
        {
            actualOk = Int32.TryParse(text, style, culture, out actual);
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
        bool expectedOk = int.TryParse(text, style, culture, out int expected);
        bool actualOk = Int32.TryParse(text, style, culture, out int actual);
        return expectedOk == actualOk && expected == actual;
    }
}
