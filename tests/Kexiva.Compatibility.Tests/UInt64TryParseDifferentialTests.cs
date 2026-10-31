using System;
using System.Globalization;
using System.Text;
using Xunit;

namespace Kexiva.Compatibility.Tests;

/// <summary>
/// Compares <see cref="UInt64.TryParse(string?, out ulong)"/> with <see cref="ulong.TryParse(string?, out ulong)"/>.
/// </summary>
public sealed class UInt64TryParseDifferentialTests
{
    /// <summary>
    /// Verifies that the tests call <c>Kexiva.UInt64</c>.
    /// </summary>
    [Fact]
    public void DeclaringType_IsKexivaUInt64()
    {
        Assert.Equal("Kexiva.UInt64", typeof(UInt64).FullName);
    }

    private static readonly string[] _texts =
    [
        "",
        " ",
        "\t",
        "\n",
        "\v",
        "\f",
        "\r",
        "\u00A0",
        "0",
        "1",
        "12",
        "123456",
        "+0",
        "-0",
        "+1",
        "-1",
        "+",
        "-",
        "+\0",
        "-0.0",
        "-0,0",
        "-2147483648",
        "-2147483647",
        "-2147483649",
        "2147483646",
        "2147483647",
        "2147483648",
        "-21474836480",
        "80000000",
        "80000001",
        "7FFFFFFF",
        "10000000000000000000000000000000",
        "(123)",
        "-2147483648.0",
        "2147483647.0",
        "2147483648.0",
        "4294967295",
        "4294967296",
        "42949672950",
        "4294967295a",
        "00000000004294967295",
        "00000000004294967296",
        "000000000001",
        "12a",
        "12\0",
        "12\0\0",
        "12\0 ",
        "12 \0",
        "12\03",
        "  0012  ",
        "1,234",
        "1.234",
        "1,,234",
        "1234,",
        ",123",
        "1.0",
        "1.",
        ".0",
        "1.1",
        "(0)",
        "(1)",
        "(0.0)",
        "()",
        "-(1)",
        "123-",
        "123 -",
        "- 1",
        "+ 1",
        "1e2",
        "1E2",
        "1e+2",
        "1e000",
        "1.5e1",
        "10e-1",
        "1e-1",
        "1.5e0",
        "11e-1",
        "1e",
        "0e1000000000",
        "1e1000000000",
        "1e100000000g",
        "1e0000000001",
        "FFFFFFFF",
        "ffffffff",
        "FF",
        "100000000",
        "0x10",
        "10",
        "2",
        "11111111111111111111111111111111",
        "111111111111111111111111111111111",
        "$1",
        "\u22120",
        "\u22121",
        "1 234",
        "1\u00A0234",
        "1\u202F234",
        "-9223372036854775808",
        "-9223372036854775807",
        "-9223372036854775809",
        "9223372036854775806",
        "9223372036854775807",
        "9223372036854775808",
        "92233720368547758080",
        "8000000000000000",
        "7FFFFFFFFFFFFFFF",
        "FFFFFFFFFFFFFFFF",
        "10000000000000000",
        "18446744073709551614",
        "18446744073709551615",
        "18446744073709551616",
        "00018446744073709551615",
        "-18446744073709551615",
        "+18446744073709551615",
        "1000000000000000000",
    ];

    private static readonly NumberStyles[] _styles =
    [
        NumberStyles.None,
        NumberStyles.Integer,
        NumberStyles.Number,
        NumberStyles.Float,
        NumberStyles.Currency,
        NumberStyles.Any,
        NumberStyles.HexNumber,
        NumberStyles.BinaryNumber,
        NumberStyles.AllowHexSpecifier,
        NumberStyles.AllowBinarySpecifier,
    ];

    /// <summary>
    /// Verifies the spec inputs for string, char, and UTF-8 text.
    /// </summary>
    [Fact]
    public void SpecInputs_MatchReferenceTryParse()
    {
        CultureInfo[] cultures =
        [
            CultureInfo.InvariantCulture,
            CultureInfo.GetCultureInfo("en-US"),
            CultureInfo.GetCultureInfo("fr-FR"),
            CultureInfo.GetCultureInfo("de-DE"),
            CultureInfo.GetCultureInfo("sv-SE"),
        ];

        foreach (CultureInfo culture in cultures)
        {
            foreach (NumberStyles style in _styles)
            {
                foreach (string text in _texts)
                {
                    AssertSame(text, style, culture);
                    AssertSame(text.AsSpan(), style, culture);
                    AssertSame(Encoding.UTF8.GetBytes(text), style, culture);
                }
            }
        }
    }

    /// <summary>
    /// Verifies null text, an invalid style, and the overloads that omit the style.
    /// </summary>
    [Fact]
    public void NullAndInvalidStyle_MatchReferenceTryParse()
    {
        AssertSame((string?)null, NumberStyles.Integer, CultureInfo.InvariantCulture);
        AssertSame((string?)null, (NumberStyles)0x800, CultureInfo.InvariantCulture);
        AssertSame("1", NumberStyles.AllowHexSpecifier | NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture);
        AssertSame("1", NumberStyles.AllowHexSpecifier | NumberStyles.AllowBinarySpecifier, CultureInfo.InvariantCulture);
        AssertSame("1", (NumberStyles)0x800, CultureInfo.InvariantCulture);

        AssertOverload("123456", CultureInfo.InvariantCulture);
        AssertOverload("-9223372036854775808", CultureInfo.InvariantCulture);
        AssertOverload("9223372036854775807", CultureInfo.InvariantCulture);
        AssertOverload("9223372036854775808", CultureInfo.InvariantCulture);
        AssertOverload("4294967296", CultureInfo.InvariantCulture);
        AssertOverload("18446744073709551615", CultureInfo.InvariantCulture);
        AssertOverload("18446744073709551616", CultureInfo.InvariantCulture);
        AssertOverload("-0", CultureInfo.InvariantCulture);
        AssertOverload("-1", CultureInfo.InvariantCulture);
        AssertOverload(null, CultureInfo.InvariantCulture);
    }

    /// <summary>
    /// Verifies the integer-path hyphen rule and a positive sign that is a prefix of the negative sign.
    /// </summary>
    [Fact]
    public void CustomSigns_MatchReferenceTryParse()
    {
        NumberFormatInfo hyphenFirst = (NumberFormatInfo)CultureInfo.InvariantCulture.NumberFormat.Clone();
        hyphenFirst.PositiveSign = "-";
        hyphenFirst.NegativeSign = "\u2212";
        AssertSame("-123", NumberStyles.Integer, hyphenFirst);
        AssertSame("-123", NumberStyles.Number, hyphenFirst);
        AssertSame("-1", NumberStyles.Integer, hyphenFirst);

        NumberFormatInfo positivePrefix = (NumberFormatInfo)CultureInfo.InvariantCulture.NumberFormat.Clone();
        positivePrefix.PositiveSign = "-1";
        positivePrefix.NegativeSign = "\u2212";
        AssertSame("-123", NumberStyles.Integer, positivePrefix);
        AssertSame("-123", NumberStyles.Number, positivePrefix);

        NumberFormatInfo wordSigns = (NumberFormatInfo)CultureInfo.InvariantCulture.NumberFormat.Clone();
        wordSigns.PositiveSign = "n";
        wordSigns.NegativeSign = "neg";
        AssertSame("neg1", NumberStyles.Integer, wordSigns);
        AssertSame("n1", NumberStyles.Integer, wordSigns);
        AssertSame("-1", NumberStyles.Integer, wordSigns);

        NumberFormatInfo patternTwo = (NumberFormatInfo)CultureInfo.InvariantCulture.NumberFormat.Clone();
        patternTwo.NumberNegativePattern = 2;
        AssertSame("- 1", NumberStyles.Integer, patternTwo);
        AssertSame("- 1", NumberStyles.Number, patternTwo);
        AssertSame("+ 1", NumberStyles.Number, patternTwo);
    }

    /// <summary>
    /// Verifies that an omitted provider reads <see cref="CultureInfo.CurrentCulture"/> and restores it.
    /// </summary>
    [Fact]
    public void CurrentCulture_IsReadAndRestored()
    {
        CultureInfo original = CultureInfo.CurrentCulture;
        try
        {
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("sv-SE");
            AssertOverload("\u22120", provider: null);
            AssertOverload("-0", provider: null);

            CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;
            AssertOverload("\u22120", provider: null);
        }
        finally
        {
            CultureInfo.CurrentCulture = original;
        }

        Assert.Equal(original, CultureInfo.CurrentCulture);
    }

    /// <summary>
    /// Verifies overflow for long digit runs, large exponents, and repeated leading zeros.
    /// </summary>
    [Fact]
    public void LongInputs_MatchReferenceTryParse()
    {
        CultureInfo invariant = CultureInfo.InvariantCulture;
        string zeros = new('0', 4096);
        string nines = new('9', 2048);
        string ones = new('1', 1024);

        AssertAll(nines, NumberStyles.Integer, invariant);
        AssertAll(ones, NumberStyles.Integer, invariant);
        AssertAll(zeros + "123", NumberStyles.Integer, invariant);
        AssertAll(zeros + "2147483647", NumberStyles.Integer, invariant);
        AssertAll(zeros + "2147483648", NumberStyles.Integer, invariant);
        AssertAll(zeros + "-2147483648", NumberStyles.Integer, invariant);
        AssertAll("-" + zeros + "1", NumberStyles.Integer, invariant);
        AssertAll(zeros + "4294967295", NumberStyles.Integer, invariant);
        AssertAll(zeros + "4294967296", NumberStyles.Integer, invariant);
        AssertAll(zeros + "9223372036854775807", NumberStyles.Integer, invariant);
        AssertAll(zeros + "9223372036854775808", NumberStyles.Integer, invariant);
        AssertAll(zeros + "-9223372036854775808", NumberStyles.Integer, invariant);
        AssertAll(zeros + "18446744073709551615", NumberStyles.Integer, invariant);
        AssertAll(zeros + "18446744073709551616", NumberStyles.Integer, invariant);
        AssertAll("-" + zeros + "0", NumberStyles.Integer, invariant);
        AssertAll(nines + "a", NumberStyles.Integer, invariant);
        AssertAll("8000000000000000", NumberStyles.HexNumber, invariant);
        AssertAll("10000000000000000", NumberStyles.HexNumber, invariant);
        AssertAll("1" + new string('0', 63), NumberStyles.BinaryNumber, invariant);
        AssertAll("1" + new string('0', 64), NumberStyles.BinaryNumber, invariant);

        AssertAll("1e" + new string('9', 40), NumberStyles.Float, invariant);
        AssertAll("0e" + new string('9', 40), NumberStyles.Float, invariant);
        AssertAll("1e" + new string('0', 30) + "2", NumberStyles.Float, invariant);
        AssertAll("1" + new string('0', 100) + "e10", NumberStyles.Float, invariant);

        AssertAll(new('F', 64), NumberStyles.HexNumber, invariant);
        AssertAll(zeros + "FF", NumberStyles.HexNumber, invariant);
        AssertAll(new('1', 64), NumberStyles.BinaryNumber, invariant);
        AssertAll(zeros + "101", NumberStyles.BinaryNumber, invariant);
    }

    /// <summary>
    /// Verifies currency placement, empty signs, alternate hyphens, and a long currency symbol.
    /// </summary>
    [Fact]
    public void AdditionalInputs_MatchReferenceTryParse()
    {
        CultureInfo invariant = CultureInfo.InvariantCulture;
        AssertAll("1$", NumberStyles.Currency, CultureInfo.GetCultureInfo("en-US"));
        AssertAll("1$", NumberStyles.Currency, invariant);
        AssertAll("(1", NumberStyles.Number, invariant);
        AssertAll("(1) ", NumberStyles.Any, invariant);
        AssertAll("10000000001", NumberStyles.Number, invariant);
        AssertAll("9999999999", NumberStyles.Number, invariant);
        AssertAll("ff ", NumberStyles.HexNumber, invariant);
        AssertAll("11 ", NumberStyles.BinaryNumber, invariant);
        AssertAll("123456789a", NumberStyles.Integer, invariant);
        AssertAll("123456789 ", NumberStyles.Integer, invariant);
        AssertAll("5000000000", NumberStyles.Integer, invariant);
        AssertAll("FFFFFFFF ", NumberStyles.HexNumber, invariant);
        AssertAll("FFFFFFFFg", NumberStyles.HexNumber, invariant);
        AssertAll("111111111111111111111111111111112", NumberStyles.BinaryNumber, invariant);
        AssertAll("123+", NumberStyles.Number, invariant);
        AssertAll("($1,234.00)", NumberStyles.Currency, CultureInfo.GetCultureInfo("en-US"));
        AssertAll("(2147483648)", NumberStyles.Any, invariant);
        AssertAll("- 2147483648", NumberStyles.Number, invariant);
        AssertAll("(9223372036854775808)", NumberStyles.Any, invariant);
        AssertAll("- 9223372036854775808", NumberStyles.Number, invariant);
        AssertAll("(18446744073709551615)", NumberStyles.Any, invariant);
        AssertAll("(18446744073709551616)", NumberStyles.Any, invariant);
        AssertAll("18446744073709551615.0", NumberStyles.Float, invariant);

        NumberFormatInfo emptyPositive = (NumberFormatInfo)invariant.NumberFormat.Clone();
        emptyPositive.PositiveSign = string.Empty;
        emptyPositive.NegativeSign = "-";
        AssertAll("-1", NumberStyles.Integer, emptyPositive);
        AssertAll("+1", NumberStyles.Integer, emptyPositive);
        AssertAll("-1", NumberStyles.Number, emptyPositive);

        NumberFormatInfo emptyNegative = (NumberFormatInfo)invariant.NumberFormat.Clone();
        emptyNegative.PositiveSign = "+";
        emptyNegative.NegativeSign = string.Empty;
        AssertAll("-1", NumberStyles.Number, emptyNegative);
        AssertAll("+1", NumberStyles.Number, emptyNegative);

        NumberFormatInfo currency = (NumberFormatInfo)invariant.NumberFormat.Clone();
        currency.CurrencySymbol = "$";
        currency.CurrencyGroupSeparator = ".";
        currency.CurrencyDecimalSeparator = ":";
        currency.NumberGroupSeparator = ",";
        currency.NumberDecimalSeparator = ".";
        AssertAll("1,234$", NumberStyles.Currency, currency);
        AssertAll("1.5$", NumberStyles.Currency, currency);
        AssertAll("$1.234", NumberStyles.Currency, currency);

        string longSymbol = new('€', 90);
        NumberFormatInfo longCurrency = (NumberFormatInfo)invariant.NumberFormat.Clone();
        longCurrency.CurrencySymbol = longSymbol;
        AssertAll(longSymbol + "1", NumberStyles.Currency, longCurrency);
        AssertAll("1", NumberStyles.Currency, longCurrency);

        NumberFormatInfo longSign = (NumberFormatInfo)invariant.NumberFormat.Clone();
        longSign.PositiveSign = "positive";
        longSign.NegativeSign = "negative";
        AssertAll("1", NumberStyles.Number, longSign);
        AssertAll("positive1", NumberStyles.Number, longSign);
        AssertAll("negative1", NumberStyles.Integer, longSign);

        string[] hyphens = ["\u2012", "\u207B", "\u208B", "\u2212", "\u2796", "\uFE63", "\uFF0D"];
        foreach (string hyphen in hyphens)
        {
            NumberFormatInfo info = (NumberFormatInfo)invariant.NumberFormat.Clone();
            info.NegativeSign = hyphen;
            AssertAll(hyphen + "0", NumberStyles.Integer, info);
            AssertAll(hyphen + "1", NumberStyles.Integer, info);
            AssertAll(hyphen + "0", NumberStyles.Number, info);
            AssertAll("-1", NumberStyles.Integer, info);
            AssertAll("-0", NumberStyles.Number, info);
            AssertAll("1e" + hyphen + "1", NumberStyles.Float, info);
        }
    }

    /// <summary>
    /// Verifies one invalid UTF-8 sequence.
    /// </summary>
    [Fact]
    public void InvalidUtf8_MatchesReferenceTryParse()
    {
        byte[] invalid = [0xFF];
        AssertSame(invalid, NumberStyles.Integer, CultureInfo.InvariantCulture);
        AssertSame(Encoding.UTF8.GetBytes("\u22120"), NumberStyles.Integer, CultureInfo.GetCultureInfo("sv-SE"));
        AssertSame(Encoding.UTF8.GetBytes("1\u202F234"), NumberStyles.Number, CultureInfo.GetCultureInfo("fr-FR"));
        AssertSame("1 234".AsSpan(), NumberStyles.Number, CultureInfo.GetCultureInfo("fr-FR"));
        AssertSame(Encoding.UTF8.GetBytes("1 234"), NumberStyles.Number, CultureInfo.GetCultureInfo("fr-FR"));
    }

    private static void AssertAll(string text, NumberStyles style, IFormatProvider? provider)
    {
        AssertSame(text, style, provider);
        AssertSame(text.AsSpan(), style, provider);
        AssertSame(Encoding.UTF8.GetBytes(text), style, provider);
    }

    private static void AssertOverload(string? text, IFormatProvider? provider)
    {
        bool expectedOk = ulong.TryParse(text, provider, out ulong expected);
        bool actualOk = UInt64.TryParse(text, provider, out ulong actual);
        Assert.Equal(expectedOk, actualOk);
        Assert.Equal(expected, actual);

        if (text is null)
        {
            return;
        }

        bool expectedSpan = ulong.TryParse(text.AsSpan(), provider, out ulong expectedSpanValue);
        bool actualSpan = UInt64.TryParse(text.AsSpan(), provider, out ulong actualSpanValue);
        Assert.Equal(expectedSpan, actualSpan);
        Assert.Equal(expectedSpanValue, actualSpanValue);

        byte[] utf8 = Encoding.UTF8.GetBytes(text);
        bool expectedUtf8 = ulong.TryParse(utf8, provider, out ulong expectedUtf8Value);
        bool actualUtf8 = UInt64.TryParse(utf8, provider, out ulong actualUtf8Value);
        Assert.Equal(expectedUtf8, actualUtf8);
        Assert.Equal(expectedUtf8Value, actualUtf8Value);

        bool expectedDefault = ulong.TryParse(text, out ulong expectedDefaultValue);
        bool actualDefault = UInt64.TryParse(text, out ulong actualDefaultValue);
        Assert.Equal(expectedDefault, actualDefault);
        Assert.Equal(expectedDefaultValue, actualDefaultValue);

        bool expectedChar = ulong.TryParse(text.AsSpan(), out ulong expectedCharValue);
        bool actualChar = UInt64.TryParse(text.AsSpan(), out ulong actualCharValue);
        Assert.Equal(expectedChar, actualChar);
        Assert.Equal(expectedCharValue, actualCharValue);

        bool expectedUtf8Default = ulong.TryParse(utf8.AsSpan(), out ulong expectedUtf8DefaultValue);
        bool actualUtf8Default = UInt64.TryParse(utf8.AsSpan(), out ulong actualUtf8DefaultValue);
        Assert.Equal(expectedUtf8Default, actualUtf8Default);
        Assert.Equal(expectedUtf8DefaultValue, actualUtf8DefaultValue);
    }

    private static void AssertSame(string? text, NumberStyles style, IFormatProvider? provider)
    {
        ArgumentException? expectedException = TryReference(text, style, provider, out bool expectedOk, out ulong expected);
        ArgumentException? actualException = TryKexiva(text, style, provider, out bool actualOk, out ulong actual);
        AssertFailure(text, style, provider, expectedException, actualException, expectedOk, actualOk, expected, actual);
    }

    private static void AssertSame(ReadOnlySpan<char> text, NumberStyles style, IFormatProvider? provider)
    {
        ArgumentException? expectedException = null;
        bool expectedOk = false;
        ulong expected = 0;
        try
        {
            expectedOk = ulong.TryParse(text, style, provider, out expected);
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
            actualOk = UInt64.TryParse(text, style, provider, out actual);
        }
        catch (ArgumentException exception)
        {
            actualException = exception;
        }

        AssertFailure(text.ToString(), style, provider, expectedException, actualException, expectedOk, actualOk, expected, actual);
    }

    private static void AssertSame(byte[] text, NumberStyles style, IFormatProvider? provider)
    {
        ArgumentException? expectedException = null;
        bool expectedOk = false;
        ulong expected = 0;
        try
        {
            expectedOk = ulong.TryParse(text, style, provider, out expected);
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
            actualOk = UInt64.TryParse(text, style, provider, out actual);
        }
        catch (ArgumentException exception)
        {
            actualException = exception;
        }

        AssertFailure(Encoding.UTF8.GetString(text), style, provider, expectedException, actualException, expectedOk, actualOk, expected, actual);
    }

    private static ArgumentException? TryReference(string? text, NumberStyles style, IFormatProvider? provider, out bool ok, out ulong value)
    {
        try
        {
            ok = ulong.TryParse(text, style, provider, out value);
            return null;
        }
        catch (ArgumentException exception)
        {
            ok = false;
            value = 0;
            return exception;
        }
    }

    private static ArgumentException? TryKexiva(string? text, NumberStyles style, IFormatProvider? provider, out bool ok, out ulong value)
    {
        try
        {
            ok = UInt64.TryParse(text, style, provider, out value);
            return null;
        }
        catch (ArgumentException exception)
        {
            ok = false;
            value = 0;
            return exception;
        }
    }

    private static void AssertFailure(
        string? text,
        NumberStyles style,
        IFormatProvider? provider,
        ArgumentException? expectedException,
        ArgumentException? actualException,
        bool expectedOk,
        bool actualOk,
        ulong expected,
        ulong actual)
    {
        string description = $"text={text} style={style} provider={provider}";
        Assert.Equal(expectedException?.GetType(), actualException?.GetType());
        Assert.Equal((expectedException as ArgumentException)?.ParamName, (actualException as ArgumentException)?.ParamName);
        Assert.True(expectedOk == actualOk, description);
        Assert.True(expected == actual, description);
    }
}
