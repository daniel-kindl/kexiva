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
