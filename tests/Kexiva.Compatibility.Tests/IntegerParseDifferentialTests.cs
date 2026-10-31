using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using Xunit;

namespace Kexiva.Compatibility.Tests;

/// <summary>
/// Compares integer Parse values, exceptions, providers, and signatures with .NET.
/// </summary>
public sealed class IntegerParseDifferentialTests
{
    private static readonly string?[] _texts =
    [
        null, "", " ", "\t\n\v\f\r", "\u00A0", "0", "1", "123456", "+0", "-0", "+1", "-1",
        "2147483647", "2147483648", "-2147483648", "-2147483649", "4294967295", "4294967296",
        "9223372036854775807", "9223372036854775808", "-9223372036854775808", "-9223372036854775809",
        "18446744073709551615", "18446744073709551616", "184467440737095516160a", "-18446744073709551616a",
        "(0)", "(1)", "-0.0", "-0,0", "-0.1", "1.0", "1.1", "1.5e1", "11e-1", "0e1000000000", "1e1000000000",
        "1,234", "1.234", "1 234", "1\u202F234", "1\u00A0234", "€1", "1€", "1,,234", "123 -", "- 1",
        "\u221212", "plus12", "minus12", "+", "-", "12a", "12\0", "12\0\0", "12\03", "12 \0", "12\0 ",
        "FFFFFFFF", "80000000", "FFFFFFFFFFFFFFFF", "10000000000000000", "0xFF", "0b1", "１２", "١٢",
        new string('0', 1024) + "1", new string('9', 1024), new string('9', 1024) + "a",
        new string('F', 100), new string('1', 64), "1" + new string('0', 64),
    ];

    private static readonly NumberStyles[] _styles =
    [
        NumberStyles.None, NumberStyles.Integer, NumberStyles.Number, NumberStyles.Float,
        NumberStyles.Currency, NumberStyles.Any, NumberStyles.HexNumber, NumberStyles.BinaryNumber,
        NumberStyles.AllowLeadingSign | NumberStyles.AllowTrailingSign,
        (NumberStyles)(-1), (NumberStyles)0x800, NumberStyles.AllowHexSpecifier | NumberStyles.AllowLeadingSign,
        NumberStyles.AllowHexSpecifier | NumberStyles.AllowBinarySpecifier,
    ];

    /// <summary>
    /// Tests all declared overloads and omitted optional arguments on each integer width.
    /// </summary>
    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    public void AllOverloads_MatchReference(int width)
    {
        CultureInfo previous = CultureInfo.CurrentCulture;
        try
        {
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("sv-SE");
            NumberFormatInfo custom = (NumberFormatInfo)CultureInfo.InvariantCulture.NumberFormat.Clone();
            custom.PositiveSign = "plus";
            custom.NegativeSign = "minus";
            custom.NumberGroupSeparator = new string('~', 80);
            custom.CurrencySymbol = "€";
            IFormatProvider?[] providers = [null, CultureInfo.InvariantCulture, CultureInfo.GetCultureInfo("fr-FR"), CultureInfo.GetCultureInfo("de-DE"), CultureInfo.GetCultureInfo("sv-SE"), custom];
            foreach (IFormatProvider? provider in providers)
            {
                foreach (string? text in _texts)
                {
                    for (int overload = 0; overload < 12; overload++)
                    {
                        AssertSame(width, text, NumberStyles.Integer, provider, overload);
                    }

                    foreach (NumberStyles style in _styles)
                    {
                        AssertSame(width, text, style, provider, 3);
                        AssertSame(width, text, style, provider, 4);
                        AssertSame(width, text, style, provider, 6);
                    }
                }
            }
        }
        finally
        {
            CultureInfo.CurrentCulture = previous;
        }
    }

    /// <summary>
    /// Checks every parameter name, parameter type, return type, and optional default against .NET.
    /// </summary>
    [Theory]
    [InlineData(typeof(int), typeof(Int32))]
    [InlineData(typeof(uint), typeof(UInt32))]
    [InlineData(typeof(long), typeof(Int64))]
    [InlineData(typeof(ulong), typeof(UInt64))]
    public void Signatures_MatchReference(Type reference, Type actual)
    {
        ArgumentNullException.ThrowIfNull(reference);
        ArgumentNullException.ThrowIfNull(actual);
        MethodInfo[] expectedMethods = [.. reference.GetMethods(BindingFlags.Public | BindingFlags.Static).Where(method => method.Name == "Parse")];
        MethodInfo[] actualMethods = [.. actual.GetMethods(BindingFlags.Public | BindingFlags.Static).Where(method => method.Name == "Parse")];
        Assert.Equal(8, expectedMethods.Length);
        Assert.Equal(8, actualMethods.Length);
        foreach (MethodInfo expected in expectedMethods)
        {
            ParameterInfo[] parameters = expected.GetParameters();
            MethodInfo? method = actual.GetMethod("Parse", [.. parameters.Select(parameter => parameter.ParameterType)]);
            Assert.NotNull(method);
            Assert.Equal(expected.ReturnType, method.ReturnType);
            ParameterInfo[] actualParameters = method.GetParameters();
            for (int i = 0; i < parameters.Length; i++)
            {
                Assert.Equal(parameters[i].Name, actualParameters[i].Name);
                Assert.Equal(parameters[i].IsOptional, actualParameters[i].IsOptional);
                Assert.Equal(parameters[i].DefaultValue, actualParameters[i].DefaultValue);
                Assert.Equal(new NullabilityInfoContext().Create(parameters[i]).ReadState, new NullabilityInfoContext().Create(actualParameters[i]).ReadState);
            }
        }
    }

    /// <summary>
    /// Preserves provider call counts on success, failures, and competing argument errors.
    /// </summary>
    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    public void ProviderCalls_MatchReference(int width)
    {
        foreach (bool throws in new[] { false, true })
        {
            foreach (string? text in new string?[] { null, "1", "bad", "9999999999999999999999999" })
            {
                foreach (NumberStyles style in new[] { NumberStyles.Integer, (NumberStyles)(-1) })
                {
                    for (int overload = 0; overload < 12; overload++)
                    {
                        CountingProvider expectedProvider = new(throws);
                        CountingProvider actualProvider = new(throws);
                        (decimal? Value, Type? Error, string? Parameter) expected = Capture(() => Parse(false, width, text, style, expectedProvider, overload));
                        Assert.Equal(expected, Capture(() => Parse(true, width, text, style, actualProvider, overload)));
                        Assert.Equal(expectedProvider.Calls, actualProvider.Calls);
                    }
                }
            }
        }
    }

    /// <summary>
    /// Rejects malformed UTF-8 with the same failure as .NET.
    /// </summary>
    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    public void InvalidUtf8_MatchesReference(int width)
    {
        foreach (byte[] bytes in new byte[][] { [0xFF], [0x31, 0xC0, 0xAF], [0xE2, 0x88], [0xED, 0xA0, 0x80] })
        {
            Assert.Equal(Capture(() => ParseUtf8(false, width, bytes)), Capture(() => ParseUtf8(true, width, bytes)));
        }
    }

    /// <summary>
    /// Checks competing signs, exponent signs, and each permitted whitespace character.
    /// </summary>
    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    public void SignAndWhitespaceRules_MatchReference(int width)
    {
        NumberFormatInfo hyphen = (NumberFormatInfo)CultureInfo.InvariantCulture.NumberFormat.Clone();
        hyphen.PositiveSign = "-";
        hyphen.NegativeSign = "\u2212";
        NumberFormatInfo prefix = (NumberFormatInfo)CultureInfo.InvariantCulture.NumberFormat.Clone();
        prefix.PositiveSign = "n";
        prefix.NegativeSign = "neg";
        NumberFormatInfo digitPrefix = (NumberFormatInfo)hyphen.Clone();
        digitPrefix.PositiveSign = "-1";
        NumberFormatInfo pattern = (NumberFormatInfo)CultureInfo.InvariantCulture.NumberFormat.Clone();
        pattern.NumberNegativePattern = 2;
        foreach (NumberFormatInfo info in new[] { hyphen, prefix, digitPrefix, pattern })
        {
            foreach (NumberStyles style in new[] { NumberStyles.Integer, NumberStyles.Number, NumberStyles.Float, NumberStyles.Any })
            {
                foreach (string text in new[] { "-1", "-11", "n1", "neg1", "- 1", "+ 1", "1e-1", "1en1", "1eneg1", "1e\u22121" })
                {
                    foreach (int overload in new[] { 3, 4, 6 })
                    {
                        AssertSame(width, text, style, info, overload);
                    }
                }
            }
        }

        foreach (char whitespace in new[] { '\t', '\n', '\v', '\f', '\r', ' ' })
        {
            foreach (NumberStyles style in new[] { NumberStyles.None, NumberStyles.AllowLeadingWhite, NumberStyles.AllowTrailingWhite, NumberStyles.Integer })
            {
                foreach (string text in new[] { whitespace + "12", "12" + whitespace, whitespace + "12" + whitespace })
                {
                    foreach (int overload in new[] { 3, 4, 6 })
                    {
                        AssertSame(width, text, style, CultureInfo.InvariantCulture, overload);
                    }
                }
            }
        }
    }

    private static void AssertSame(int width, string? text, NumberStyles style, IFormatProvider? provider, int overload)
    {
        (decimal? Value, Type? Error, string? Parameter) expected = Capture(() => Parse(false, width, text, style, provider, overload));
        Assert.Equal(expected, Capture(() => Parse(true, width, text, style, provider, overload)));
    }

    private static (decimal? Value, Type? Error, string? Parameter) Capture(Func<decimal> parse)
    {
        try
        {
            return (parse(), null, null);
        }
        catch (Exception exception) when (exception is ArgumentException or FormatException or OverflowException or InvalidOperationException)
        {
            return (null, exception.GetType(), (exception as ArgumentException)?.ParamName);
        }
    }

    private static decimal Parse(bool kexiva, int width, string? text, NumberStyles style, IFormatProvider? provider, int overload)
    {
        return (kexiva, width) switch
        {
            (false, 0) => ParseInt32(false, text, style, provider, overload),
            (true, 0) => ParseInt32(true, text, style, provider, overload),
            (false, 1) => ParseUInt32(false, text, style, provider, overload),
            (true, 1) => ParseUInt32(true, text, style, provider, overload),
            (false, 2) => ParseInt64(false, text, style, provider, overload),
            (true, 2) => ParseInt64(true, text, style, provider, overload),
            (false, 3) => ParseUInt64(false, text, style, provider, overload),
            (true, 3) => ParseUInt64(true, text, style, provider, overload),
            _ => throw new ArgumentOutOfRangeException(nameof(width)),
        };
    }

    [SuppressMessage("Globalization", "CA1305:Specify IFormatProvider", Justification = "This test must call the .NET overloads that omit a provider to verify current-culture compatibility.")]
    private static decimal ParseInt32(bool kexiva, string? text, NumberStyles style, IFormatProvider? provider, int overload)
    {
        byte[] utf8 = Encoding.UTF8.GetBytes(text ?? string.Empty);
        return (kexiva, overload) switch
        {
            (false, 0) => int.Parse(text!),
            (false, 1) => int.Parse(text!, style),
            (false, 2) => int.Parse(text!, provider),
            (false, 3) => int.Parse(text!, style, provider),
            (false, 4) => int.Parse(text.AsSpan(), style, provider),
            (false, 5) => int.Parse(text.AsSpan(), provider),
            (false, 6) => int.Parse(utf8, style, provider),
            (false, 7) => int.Parse(utf8, provider),
            (false, 8) => int.Parse(text.AsSpan()),
            (false, 9) => int.Parse(text.AsSpan(), style),
            (false, 10) => int.Parse(utf8),
            (false, 11) => int.Parse(utf8, style),
            (true, 0) => Int32.Parse(text!),
            (true, 1) => Int32.Parse(text!, style),
            (true, 2) => Int32.Parse(text!, provider),
            (true, 3) => Int32.Parse(text!, style, provider),
            (true, 4) => Int32.Parse(text.AsSpan(), style, provider),
            (true, 5) => Int32.Parse(text.AsSpan(), provider),
            (true, 6) => Int32.Parse(utf8, style, provider),
            (true, 7) => Int32.Parse(utf8, provider),
            (true, 8) => Int32.Parse(text.AsSpan()),
            (true, 9) => Int32.Parse(text.AsSpan(), style),
            (true, 10) => Int32.Parse(utf8),
            (true, 11) => Int32.Parse(utf8, style),
            _ => throw new ArgumentOutOfRangeException(nameof(overload)),
        };
    }

    [SuppressMessage("Globalization", "CA1305:Specify IFormatProvider", Justification = "This test must call the .NET overloads that omit a provider to verify current-culture compatibility.")]
    private static decimal ParseUInt32(bool kexiva, string? text, NumberStyles style, IFormatProvider? provider, int overload)
    {
        byte[] utf8 = Encoding.UTF8.GetBytes(text ?? string.Empty);
        return (kexiva, overload) switch
        {
            (false, 0) => uint.Parse(text!),
            (false, 1) => uint.Parse(text!, style),
            (false, 2) => uint.Parse(text!, provider),
            (false, 3) => uint.Parse(text!, style, provider),
            (false, 4) => uint.Parse(text.AsSpan(), style, provider),
            (false, 5) => uint.Parse(text.AsSpan(), provider),
            (false, 6) => uint.Parse(utf8, style, provider),
            (false, 7) => uint.Parse(utf8, provider),
            (false, 8) => uint.Parse(text.AsSpan()),
            (false, 9) => uint.Parse(text.AsSpan(), style),
            (false, 10) => uint.Parse(utf8),
            (false, 11) => uint.Parse(utf8, style),
            (true, 0) => UInt32.Parse(text!),
            (true, 1) => UInt32.Parse(text!, style),
            (true, 2) => UInt32.Parse(text!, provider),
            (true, 3) => UInt32.Parse(text!, style, provider),
            (true, 4) => UInt32.Parse(text.AsSpan(), style, provider),
            (true, 5) => UInt32.Parse(text.AsSpan(), provider),
            (true, 6) => UInt32.Parse(utf8, style, provider),
            (true, 7) => UInt32.Parse(utf8, provider),
            (true, 8) => UInt32.Parse(text.AsSpan()),
            (true, 9) => UInt32.Parse(text.AsSpan(), style),
            (true, 10) => UInt32.Parse(utf8),
            (true, 11) => UInt32.Parse(utf8, style),
            _ => throw new ArgumentOutOfRangeException(nameof(overload)),
        };
    }

    [SuppressMessage("Globalization", "CA1305:Specify IFormatProvider", Justification = "This test must call the .NET overloads that omit a provider to verify current-culture compatibility.")]
    private static decimal ParseInt64(bool kexiva, string? text, NumberStyles style, IFormatProvider? provider, int overload)
    {
        byte[] utf8 = Encoding.UTF8.GetBytes(text ?? string.Empty);
        return (kexiva, overload) switch
        {
            (false, 0) => long.Parse(text!),
            (false, 1) => long.Parse(text!, style),
            (false, 2) => long.Parse(text!, provider),
            (false, 3) => long.Parse(text!, style, provider),
            (false, 4) => long.Parse(text.AsSpan(), style, provider),
            (false, 5) => long.Parse(text.AsSpan(), provider),
            (false, 6) => long.Parse(utf8, style, provider),
            (false, 7) => long.Parse(utf8, provider),
            (false, 8) => long.Parse(text.AsSpan()),
            (false, 9) => long.Parse(text.AsSpan(), style),
            (false, 10) => long.Parse(utf8),
            (false, 11) => long.Parse(utf8, style),
            (true, 0) => Int64.Parse(text!),
            (true, 1) => Int64.Parse(text!, style),
            (true, 2) => Int64.Parse(text!, provider),
            (true, 3) => Int64.Parse(text!, style, provider),
            (true, 4) => Int64.Parse(text.AsSpan(), style, provider),
            (true, 5) => Int64.Parse(text.AsSpan(), provider),
            (true, 6) => Int64.Parse(utf8, style, provider),
            (true, 7) => Int64.Parse(utf8, provider),
            (true, 8) => Int64.Parse(text.AsSpan()),
            (true, 9) => Int64.Parse(text.AsSpan(), style),
            (true, 10) => Int64.Parse(utf8),
            (true, 11) => Int64.Parse(utf8, style),
            _ => throw new ArgumentOutOfRangeException(nameof(overload)),
        };
    }

    [SuppressMessage("Globalization", "CA1305:Specify IFormatProvider", Justification = "This test must call the .NET overloads that omit a provider to verify current-culture compatibility.")]
    private static decimal ParseUInt64(bool kexiva, string? text, NumberStyles style, IFormatProvider? provider, int overload)
    {
        byte[] utf8 = Encoding.UTF8.GetBytes(text ?? string.Empty);
        return (kexiva, overload) switch
        {
            (false, 0) => ulong.Parse(text!),
            (false, 1) => ulong.Parse(text!, style),
            (false, 2) => ulong.Parse(text!, provider),
            (false, 3) => ulong.Parse(text!, style, provider),
            (false, 4) => ulong.Parse(text.AsSpan(), style, provider),
            (false, 5) => ulong.Parse(text.AsSpan(), provider),
            (false, 6) => ulong.Parse(utf8, style, provider),
            (false, 7) => ulong.Parse(utf8, provider),
            (false, 8) => ulong.Parse(text.AsSpan()),
            (false, 9) => ulong.Parse(text.AsSpan(), style),
            (false, 10) => ulong.Parse(utf8),
            (false, 11) => ulong.Parse(utf8, style),
            (true, 0) => UInt64.Parse(text!),
            (true, 1) => UInt64.Parse(text!, style),
            (true, 2) => UInt64.Parse(text!, provider),
            (true, 3) => UInt64.Parse(text!, style, provider),
            (true, 4) => UInt64.Parse(text.AsSpan(), style, provider),
            (true, 5) => UInt64.Parse(text.AsSpan(), provider),
            (true, 6) => UInt64.Parse(utf8, style, provider),
            (true, 7) => UInt64.Parse(utf8, provider),
            (true, 8) => UInt64.Parse(text.AsSpan()),
            (true, 9) => UInt64.Parse(text.AsSpan(), style),
            (true, 10) => UInt64.Parse(utf8),
            (true, 11) => UInt64.Parse(utf8, style),
            _ => throw new ArgumentOutOfRangeException(nameof(overload)),
        };
    }

    [SuppressMessage("Globalization", "CA1305:Specify IFormatProvider", Justification = "This test verifies the optional provider default in the .NET signature.")]
    private static decimal ParseUtf8(bool kexiva, int width, byte[] bytes)
    {
        return (kexiva, width) switch
        {
            (false, 0) => int.Parse(bytes),
            (true, 0) => Int32.Parse(bytes),
            (false, 1) => uint.Parse(bytes),
            (true, 1) => UInt32.Parse(bytes),
            (false, 2) => long.Parse(bytes),
            (true, 2) => Int64.Parse(bytes),
            (false, 3) => ulong.Parse(bytes),
            (true, 3) => UInt64.Parse(bytes),
            _ => throw new ArgumentOutOfRangeException(nameof(width)),
        };
    }

    private sealed class CountingProvider(bool throws) : IFormatProvider
    {
        internal int Calls { get; private set; }

        public object? GetFormat(Type? formatType)
        {
            Calls++;
            if (throws)
            {
                throw new InvalidOperationException("The test provider failed.");
            }

            NumberFormatInfo info = (NumberFormatInfo)CultureInfo.InvariantCulture.NumberFormat.Clone();
            if (Calls > 1)
            {
                info.PositiveSign = "1";
            }

            return info;
        }
    }
}
