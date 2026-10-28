using System.Globalization;
using CsCheck;
using Xunit;

namespace Kexiva.Tests;

/// <summary>
/// Pins the CsCheck generator used by later differential tests.
/// </summary>
public sealed class IntegerGenerationTests
{
    /// <summary>
    /// Verifies that a fixed CsCheck seed round-trips integers through invariant formatting.
    /// </summary>
    [Fact]
    public void GeneratedIntegers_RoundTripThroughInvariantFormat()
    {
        Gen.Int.Sample(
            value =>
            {
                string text = value.ToString(CultureInfo.InvariantCulture);
                return int.Parse(text, CultureInfo.InvariantCulture) == value;
            },
            seed: "000000000001", // CsCheck prints seeds as 12 to 17 characters.
            iter: 100);
    }
}
