# Integer parsing and formatting

Contract for behavioral compatibility with the .NET 10 integer parse and format methods. Issue: [#23](https://github.com/daniel-kindl/kexiva/issues/23). Milestone: M1, Integer Parsing and Formatting.

This document is a reference. It states the required results. It does not describe an implementation.

## Reference

The reference runtime is .NET Runtime 10.0.13, the runtime in SDK 10.0.402 (`global.json`). The source of the rules is `dotnet/runtime` tag `v10.0.13`. The examples in this document were produced by calling the reference methods on that runtime.

A Kexiva method that implements one of the methods below matches the reference method for the same arguments:

- The parsed value, when parsing succeeds.
- `false` from `TryParse`, and `0` in the output value, when parsing fails.
- The exception type. `ArgumentNullException` and `ArgumentException` also match the parameter name.
- The formatted text from `ToString`.
- `TryFormat` success, the written prefix, and the written count.
- `TryFormat` failure: the method returns `false`, the written count is `0`, and the destination is unchanged.

Exception message text is outside this contract. Any other behavioral difference is a bug unless an ADR records it. [ADR 0003](../decisions/0003-testing-strategy.md) records that rule for behavior differences.

This document does not select the declaring type. It does not add a public API.

## Methods

The contract covers `System.Int32`, `System.UInt32`, `System.Int64`, and `System.UInt64`. The four types expose the same methods. In the lists below, `T` is the integer type.

Parse:

- `T Parse(string s)`
- `T Parse(string s, NumberStyles style)`
- `T Parse(string s, IFormatProvider? provider)`
- `T Parse(string s, NumberStyles style, IFormatProvider? provider)`
- `T Parse(ReadOnlySpan<char> s, NumberStyles style = NumberStyles.Integer, IFormatProvider? provider = null)`
- `T Parse(ReadOnlySpan<char> s, IFormatProvider? provider)`
- `T Parse(ReadOnlySpan<byte> utf8Text, NumberStyles style = NumberStyles.Integer, IFormatProvider? provider = null)`
- `T Parse(ReadOnlySpan<byte> utf8Text, IFormatProvider? provider)`

TryParse:

- `bool TryParse(string? s, out T result)`
- `bool TryParse(ReadOnlySpan<char> s, out T result)`
- `bool TryParse(ReadOnlySpan<byte> utf8Text, out T result)`
- `bool TryParse(string? s, NumberStyles style, IFormatProvider? provider, out T result)`
- `bool TryParse(ReadOnlySpan<char> s, NumberStyles style, IFormatProvider? provider, out T result)`
- `bool TryParse(ReadOnlySpan<byte> utf8Text, NumberStyles style, IFormatProvider? provider, out T result)`
- `bool TryParse(string? s, IFormatProvider? provider, out T result)`
- `bool TryParse(ReadOnlySpan<char> s, IFormatProvider? provider, out T result)`
- `bool TryParse(ReadOnlySpan<byte> utf8Text, IFormatProvider? provider, out T result)`

Format:

- `string ToString()`
- `string ToString(IFormatProvider? provider)`
- `string ToString(string? format)`
- `string ToString(string? format, IFormatProvider? provider)`
- `bool TryFormat(Span<char> destination, out int charsWritten, ReadOnlySpan<char> format = default, IFormatProvider? provider = null)`
- `bool TryFormat(Span<byte> utf8Destination, out int bytesWritten, ReadOnlySpan<char> format = default, IFormatProvider? provider = null)`

`ToString()` uses the general format `G` and the current culture. `ToString(string?)` uses a null provider. `TryFormat` with an empty format span uses `G`.

A UTF-8 span is the UTF-8 encoding of the char text. The UTF-8 overloads accept that encoding, including a multi-byte sign or separator. Runtime 10.0.13 accepts the UTF-8 encoding of U+2212 as a negative sign, U+00A0 and U+202F as group separators, and U+20AC as a currency symbol. An invalid UTF-8 sequence is a format failure. The byte `TryFormat` overload writes the UTF-8 encoding of the formatted char text.

The char parser accepts U+0020 where a separator or currency symbol is U+00A0 or U+202F. The UTF-8 parser does not accept that substitution. With the `fr-FR` group separator U+202F, the char text `"1 234"` parses as 1234 and the UTF-8 encoding of that same text is a format failure. The UTF-8 encoding of `"1"` plus U+202F plus `"234"` parses as 1234.

## Provider and default style

An overload that omits `NumberStyles` uses `NumberStyles.Integer`.

`NumberFormatInfo.GetInstance(provider)` selects the format info:

1. A null provider uses `CultureInfo.CurrentCulture` at the call.
2. A `NumberFormatInfo` provider uses that instance.
3. Any other provider uses the object returned by `GetFormat(typeof(NumberFormatInfo))` when that object is a `NumberFormatInfo`.
4. Otherwise the call uses the current culture.

`NumberStyles.Integer` does not read decimal separators, group separators, currency symbols, or parentheses. On that style, the culture affects the leading sign only. Tests that omit the provider must set `CultureInfo.CurrentCulture` for the duration of the call and restore it afterwards. Tests of separators, currency, and exponents pass an explicit provider.

## Number styles

`NumberStyles` is a flags enum. These are the named values from .NET 10:

| Name | Flags |
| --- | --- |
| `None` | No flags |
| `AllowLeadingWhite` | `0x0001` |
| `AllowTrailingWhite` | `0x0002` |
| `AllowLeadingSign` | `0x0004` |
| `AllowTrailingSign` | `0x0008` |
| `AllowParentheses` | `0x0010` |
| `AllowDecimalPoint` | `0x0020` |
| `AllowThousands` | `0x0040` |
| `AllowExponent` | `0x0080` |
| `AllowCurrencySymbol` | `0x0100` |
| `AllowHexSpecifier` | `0x0200` |
| `AllowBinarySpecifier` | `0x0400` |
| `Integer` | Leading white, trailing white, leading sign |
| `HexNumber` | Leading white, trailing white, hex specifier |
| `BinaryNumber` | Leading white, trailing white, binary specifier |
| `Number` | `Integer`, trailing sign, decimal point, thousands |
| `Float` | Leading white, trailing white, leading sign, decimal point, exponent |
| `Currency` | `Number`, parentheses, currency symbol |
| `Any` | `Currency` and exponent |

`AllowHexSpecifier` may be combined only with `AllowLeadingWhite` and `AllowTrailingWhite`. The same rule applies to `AllowBinarySpecifier`. Hex and binary cannot be combined. Any other bit, including bits above `0x0400`, is an invalid style.

`Parse` and `TryParse` both throw `ArgumentException` for an invalid style before they read the text. The parameter name is `style`. `TryParse` does not return `false` for an invalid style.

The parser selects a path from the style:

1. A style that is a subset of `Integer` uses the decimal integer path.
2. A style with `AllowHexSpecifier` uses the hex path.
3. A style with `AllowBinarySpecifier` uses the binary path.
4. Every remaining valid style uses the general number path.

## Digits, whitespace, and trailing NUL

Digits are the ASCII characters `0` through `9` on the decimal path. The hex path also accepts `A` through `F` and `a` through `f`. The binary path accepts only `0` and `1`. The parser does not accept Unicode decimal digits, fullwidth digits, underscores, a `0x` prefix, or a `0b` prefix. `NumberFormatInfo.NativeDigits` is not read.

Whitespace is only U+0009, U+000A, U+000B, U+000C, U+000D, and U+0020. Leading whitespace requires `AllowLeadingWhite`. Trailing whitespace requires `AllowTrailingWhite`. U+00A0 and U+202F are not whitespace. On the char overloads, U+0020 matches a group separator, decimal separator, or currency symbol whose own character is U+00A0 or U+202F. The UTF-8 overloads do not apply that match. They accept those separators only as their UTF-8 byte sequences.

After a successful number, the parser accepts trailing U+0000 characters. `"12\0"` parses as 12. A non-NUL character after a NUL is a format failure. `"12\03"` is a format failure.

## Limits

Leading zeros do not count toward the digit limit. The value overflows when it is outside the minimum or the maximum. It also overflows when the significant digit count is greater than the Decimal digits column. That overflow applies when the rest of the text is empty, allowed trailing whitespace, or NUL. When a character that the style does not accept follows those digits, the result is a format failure. `"2147483648"` overflows `Int32`. `"2147483648abc"` throws `FormatException`.

| Type | Minimum | Maximum | Decimal digits | Hex digits | Binary digits |
| --- | --- | --- | --- | --- | --- |
| `Int32` | `-2147483648` | `2147483647` | 10 | 8 | 32 |
| `UInt32` | `0` | `4294967295` | 10 | 8 | 32 |
| `Int64` | `-9223372036854775808` | `9223372036854775807` | 19 | 16 | 64 |
| `UInt64` | `0` | `18446744073709551615` | 20 | 16 | 64 |

Hex and binary results are the two's-complement bit pattern of the type. They are not a signed magnitude.

| Text | Style | Type | Result |
| --- | --- | --- | --- |
| `FFFFFFFF` | `HexNumber` | `Int32` | `-1` |
| `80000000` | `HexNumber` | `Int32` | `-2147483648` |
| `FFFFFFFF` | `HexNumber` | `UInt32` | `4294967295` |
| `FFFFFFFFFFFFFFFF` | `HexNumber` | `Int64` | `-1` |
| `FFFFFFFFFFFFFFFF` | `HexNumber` | `UInt64` | `18446744073709551615` |
| `100000000` | `HexNumber` | `Int32` | Overflow |
| `10` | `BinaryNumber` | `Int32` | `2` |
| `1` and 31 zeros | `BinaryNumber` | `Int32` | `-2147483648` |
| `1` and 32 zeros | `BinaryNumber` | `Int32` | Overflow |
| `2` | `BinaryNumber` | `Int32` | Format failure |
| `-1` | `HexNumber` | `Int32` | Format failure |
| `0x10` | `HexNumber` | `Int32` | Format failure |

## Decimal integer path

This path allows an optional leading sign and ASCII digits.

The sign match on this path depends on `NumberFormatInfo`:

- When `PositiveSign` is `"+"` and `NegativeSign` is `"-"`, the parser accepts those two ASCII characters.
- When `NegativeSign` is one character from U+2012, U+207B, U+208B, U+2212, U+2796, U+FE63, or U+FF0D, the parser checks U+002D first. If the next character is U+002D, that character is the negative sign. The parser does this before it matches `PositiveSign`. With `PositiveSign` `"-"` and `NegativeSign` U+2212, `NumberStyles.Integer` parses `"-1"` as `-1`. With `PositiveSign` `"-1"` and that negative sign, it parses `"-123"` as `-123`.
- Otherwise the parser matches `PositiveSign` first, then `NegativeSign`. The match is an ordinal prefix. An empty sign does not match. When `PositiveSign` is a prefix of `NegativeSign`, the positive sign matches and the following characters must be digits. With `PositiveSign` `"n"` and `NegativeSign` `"neg"`, the text `"neg1"` is a format failure.
- A multi-character sign such as `"neg"` does not make U+002D a negative sign. `sv-SE` uses U+2212, so it accepts both U+2212 and U+002D.

The sign must be followed by a digit. A sign alone is a format failure. `"+\0"` is a format failure. This path does not allow whitespace between the sign and the first digit. `"+ 1"` and `"- 1"` are format failures under `NumberStyles.Integer`.

`"-0"`, `"+0"`, and `"0"` parse as 0 for every type in this document, including the unsigned types. A negative non-zero value overflows an unsigned type. `UInt32` and `UInt64` reject `"-1"` as overflow.

## General number path

This path handles decimal separators, group separators, exponents, trailing signs, parentheses, and currency symbols.

The general path matches `PositiveSign` first, then `NegativeSign`. It accepts U+002D only when `NegativeSign` does not match and `NegativeSign` is one of the seven characters above. With `PositiveSign` `"-"` and `NegativeSign` U+2212, `NumberStyles.Number` parses `"-1"` as 1. With `PositiveSign` `"-1"` and that negative sign, it parses `"-123"` as 23. An exponent sign uses this order. With `PositiveSign` `"-"`, `NumberStyles.Float` parses `"1e-2"` as 100.

The currency flag selects separators. While `AllowCurrencySymbol` is set and the currency symbol has not been matched, both the currency separators and the number separators are accepted. After the currency symbol is matched, only the currency separators are accepted. The currency symbol is accepted once. A number with no currency symbol still parses. Invariant `"1"` with `NumberStyles.Currency` parses as 1. `en-US` `"$1,234.00"` parses as 1234, and `"($1,234.00)"` parses as `-1234`.

Group separators are removed. The parser does not check `NumberGroupSizes`. A separator is accepted only after a digit and before the decimal separator. A leading separator is a format failure. A trailing separator succeeds. Repeated separators succeed. With the invariant comma, `"1,234"`, `"12,34"`, `"1,,234"`, and `"1234,"` all parse as 1234. `",123"` is a format failure. `de-DE` uses `"."` as the group separator, so `"1.234"` with `NumberStyles.Number` parses as 1234.

The decimal separator is accepted once. Fractional zeros do not change a signed value. A decimal separator keeps a negative sign on an all-zero unsigned value, and that value overflows. A non-zero digit after the decimal separator overflows unless the final scale puts that digit in the integer part. With the invariant decimal point and `NumberStyles.Number`:

| Text | Result |
| --- | --- |
| `1.0`, `1.00`, `1.` | `1` |
| `.0` | `0` |
| `-0.0` | `0` for `Int32` and `Int64`. Overflow for `UInt32` and `UInt64` |
| `1.1`, `1.10`, `1.01`, `.1` | Overflow |

`UInt32` parses `"-0"` as 0. A decimal separator keeps the negative sign, so invariant `"-0.0"` and `de-DE` `"-0,0"` overflow `UInt32`. `de-DE` `"-0.0"` parses as 0 because `"."` is the group separator there. The same rules apply to `UInt64`. `"(0.0)"` overflows an unsigned type. `"(0)"` parses as 0.

An exponent requires `AllowExponent`. The marker is `E` or `e`, then an optional sign, then digits. The exponent sign uses the general-path sign order. No digits after the marker means the marker is not an exponent, and the result is a format failure. A negative exponent that leaves a non-zero fraction overflows. A positive exponent can move a fractional digit into the integer part. With `NumberStyles.Float` and the invariant culture, `"1e2"`, `"1E2"`, and `"1e+2"` parse as 100. `"1e000"` parses as 1. `"1.5e1"` parses as 15. `"10e-1"` parses as 1. `"1e-1"`, `"1.5e0"`, and `"11e-1"` overflow. `"1e"` is a format failure. `"2e10"` overflows `Int32`. `"1e0000000001"` parses as 10. The parser accumulates exponent digits until the value reaches `100000000`. One more digit then marks the exponent as too large. A non-zero number overflows. `Parse` of `"1e1000000000"` throws `OverflowException`, and `TryParse` returns `false` with result `0`. Zero stays zero for any exponent. `"0e1000000000"` parses as 0. A rejected character is still a format failure. `"1e100000000g"` throws `FormatException`.

Parentheses require `AllowParentheses`. An opening parenthesis makes the value negative, and a closing parenthesis is required. `"(123)"` parses as `-123`. `"(0)"` parses as 0 for signed and unsigned types. `"(1)"` overflows an unsigned type. A sign combined with parentheses is a format failure. `"-(1)"`, `"(+1)"`, and `"()"` are format failures.

One sign is accepted, either leading or trailing, when the matching flag is set. `"123-"` with `NumberStyles.Number` parses as `-123`. `"-123-"` is a format failure. Trailing whitespace may separate the digits and a trailing sign. `"123 -"` parses as `-123`.

Whitespace after a leading sign is a format failure on the decimal integer path. `NumberStyles.Integer` rejects that whitespace even when `NumberNegativePattern` is 2. On the general path, the whitespace succeeds when `NumberNegativePattern` is 2. `"- 1"` parses as `-1`, and `"+ 1"` parses as `1`. For patterns 0, 1, 3, and 4, the same text is a format failure. The whitespace also succeeds when `AllowLeadingWhite` is set and a currency symbol has already been matched. `en-US` has `NumberNegativePattern` 1. `"-$ 1"` and `"$- 1"` with `NumberStyles.Currency` parse as `-1`. `"- 1"` with `NumberStyles.Number` is a format failure for that culture.

## Parse failures

| Condition | `Parse` | `TryParse` |
| --- | --- | --- |
| `string` argument is null | `ArgumentNullException`, parameter `s` | `false`, result `0` |
| Empty text, or no digits | `FormatException` | `false`, result `0` |
| Text the style does not accept | `FormatException` | `false`, result `0` |
| Value outside the type | `OverflowException` | `false`, result `0` |
| Invalid `NumberStyles` | `ArgumentException`, parameter `style` | `ArgumentException`, parameter `style` |

A span has no null value. An empty span follows the empty-text row.

The null row applies when the style is valid, or when the overload has no style argument. When the string is null and the style is invalid, `Parse(string, NumberStyles)` throws `ArgumentNullException` with parameter `s`. The matching `TryParse` throws `ArgumentException` with parameter `style`.

## Standard format specifiers

`ToString` and `TryFormat` use one formatter. A null format string and an empty format mean `G`.

A standard specifier is one ASCII letter. It may be followed by ASCII digits. U+0000 ends the specifier, and characters after that NUL are ignored. The precision is the integer value of those digits. The value may be 0 through 999999999. When the value is already `100000000` or greater and another digit follows, `ToString` and `TryFormat` throw `FormatException`. Precision `100000000` does not throw. The value 1 with `"G100000000"` produces `1`. Leading zeros do not increase the value, so they do not throw by themselves. `"D1000000000"` throws. `"D0000000001"` has precision 1, and the value 5 produces `5`. `"X\0FF"` for the value 15 is the hex format with precision 0, which is `"F"`.

When the first character is an ASCII letter and a later character is not a digit or U+0000, the format is custom. `"D "` is custom. For the value 1 it produces the two characters `D` and space.

`D`, `X`, and `B` do not read decimal separators or group sizes. `X` and `B` do not read the negative sign. Every other specifier reads `NumberFormatInfo` as the tables say.

Rounding uses the next digit. A digit of `5` or greater rounds away from zero. A digit below `5` rounds toward zero. The formatter does not use round-to-even for these integer types. Rounding `999` with `"G2"` produces `"1E+03"`.

| Specifier | Precision | Rule | Invariant example |
| --- | --- | --- | --- |
| `G`, `g` | Omitted, or less than 1 | Decimal digits and the culture negative sign. No exponent. | `1234` and `"G"` produce `1234`. `0` and `"G0"` produce `0`. |
| `G`, `g` | 1 or greater | That many significant digits. The scale is the count of digits in the absolute value. Scientific form is used when the scale is greater than the precision. `G` uses `E`. `g` uses `e`. | `1234` and `"G4"` produce `1234`. `1234` and `"G3"` produce `1.23E+03`. `1234` and `"g3"` produce `1.23e+03`. `100` and `"G2"` produce `1E+02`. |
| `D`, `d` | Minimum digits. An omitted precision and precision 0 add no left padding. | Decimal digits. Zeros pad on the left when the precision is longer than the value. The negative sign is outside the count. | `-5` and `"D4"` produce `-0005`. `5` and `"D0"` produce `5`. `0` and `"D"` produce `0`. |
| `X`, `x` | Minimum digits | Hex of the bit pattern. `X` uses `A` through `F`. `x` uses `a` through `f`. | `-1` and `"X"` on `Int32` produce `FFFFFFFF`. `int.MinValue` and `"X"` produce `80000000`. `255` and `"X8"` produce `000000FF`. `0` and `"X"` produce `0`. |
| `B`, `b` | Minimum digits | Binary of the bit pattern. `B` and `b` use the same digits. | `5` and `"b"` produce `101`. `1` and `"B2"` produce `01`. `-1` and `"B"` on `Int32` produce 32 ones. `int.MinValue` and `"B"` produce `1` and 31 zeros. `0` and `"B"` produce `0`. |
| `F`, `f` | Fraction digits. Default is `NumberDecimalDigits`. | Fixed point and the number decimal separator. | `1234` and `"F"` produce `1234.00`. `1234` and `"F0"` produce `1234`. |
| `N`, `n` | Fraction digits. Default is `NumberDecimalDigits`. | Fixed point with number group separators. | `1234` and `"N"` produce `1,234.00`. `1234` and `"N0"` produce `1,234`. |
| `C`, `c` | Fraction digits. Default is `CurrencyDecimalDigits`. | Currency patterns, currency separators, and the currency symbol. | `1234` and `"C"` produce `¤1,234.00`. |
| `E`, `e` | Digits after the decimal point. Default is 6. | One digit before the decimal separator, then the exponent marker, a sign, and at least three exponent digits. | `1234` and `"E"` produce `1.234000E+003`. `1234` and `"e"` produce `1.234000e+003`. `1234` and `"E0"` produce `1E+003`. |
| `P`, `p` | Fraction digits. Default is `PercentDecimalDigits`. | The value is multiplied by 100 and formatted with the percent pattern. | `1` and `"P"` produce `100.00 %`. `1` and `"P0"` produce `100 %`. |
| `R`, `r` | Same as `G`, `g` | `R` follows `G`. `r` follows `g`. | `1234` and `"R"` produce `1234`. |

The reference formatter also uses scientific form when the scale is less than `-3`. A value of these four types does not have a negative scale.

Any other standard letter throws `FormatException`. `"Q"` throws from `ToString` and from `TryFormat`.

`long.MinValue` formatted with `"X"` is `8000000000000000`. `uint.MaxValue` formatted with `"X"` is `FFFFFFFF`. `ulong.MaxValue` formatted with `"D"` is `18446744073709551615`.

## Custom format strings

A format that is not a standard specifier uses the .NET custom numeric format. The required behavior is equality with the reference `ToString` for every custom string. Tests must include at least these invariant results:

| Value | Format | Result |
| --- | --- | --- |
| `12` | `000` | `012` |
| `1234` | `#,##0` | `1,234` |
| `12` | `0.00` | `12.00` |
| `1` | `0%` | `100%` |
| `-5` | `0;(0);zero` | `(5)` |
| `0` | `0;(0);zero` | `zero` |

The custom grammar includes digit placeholders `0` and `#`, the decimal separator, and group separators. It also includes group scaling, `%`, the per-mille character U+2030, quoted literals, and escaped characters. A custom string can contain a scientific section. It can contain up to three sections separated by `;`. Section 0 is the positive section. Section 1 is the negative section. Section 2 is the zero section. The reference method is the authority for every other custom string.

## TryFormat destination

When the destination can hold the formatted text, `TryFormat` returns `true`. The written count is the number of `char` values or UTF-8 bytes. The written prefix equals `ToString(format, provider)`, encoded as UTF-8 for the byte overload. Elements after the written count stay unchanged.

When the destination is too short, `TryFormat` returns `false` and writes a count of `0`. It does not throw. A zero-length destination returns `false` when the formatted text is not empty.

With `NegativeSign` set to `"neg"`, `-12` and format `"D"` produce the chars `neg12` and the five UTF-8 bytes of that text. With the `sv-SE` negative sign U+2212, `-12` and format `"D"` produce three chars. The UTF-8 form of that text is five bytes.

## Required differential tests

Differential tests compare each Kexiva method with the reference method on runtime 10.0.13. Each test passes the same arguments to both methods. The comparison covers the success value and a `TryParse` failure with result `0`. It covers the exception type. For `ArgumentNullException` and `ArgumentException`, it also covers the parameter name. It covers the formatted text and the `TryFormat` count. On `TryFormat` success it covers the written prefix. On `TryFormat` failure it covers a count of `0` and an unchanged destination.

The test set covers:

- Null `string`, empty text, whitespace-only text, and a null `string` combined with an invalid style.
- Each whitespace character in the allowed set, and U+00A0 where it is not a separator.
- The minimum value, the maximum value, one step outside each end, `"-0"`, and `"+0"`, for all four types.
- Leading zeros, a digit past the limit, and a digit past the limit followed by a rejected character.
- Trailing NUL, and a digit after a NUL.
- Signs, including a sign alone, whitespace after a sign, `sv-SE` U+2212 and U+002D, and a positive sign that is a prefix of the negative sign.
- U+002D taken as negative on the integer path before `PositiveSign`, and the same text on the general path.
- `NumberStyles.None`, `Integer`, `Number`, `Float`, `Currency`, `Any`, `HexNumber`, and `BinaryNumber`.
- Invalid styles, including hex combined with a sign, hex combined with binary, and a bit above `0x0400`. `Parse` and `TryParse` both throw `ArgumentException`.
- Decimal fractions from the table above, group separators, exponents, parentheses, and currency, including unsigned `"-0.0"` and `"(0)"`.
- An exponent that moves a fractional digit into the integer part, and a zero value with a large exponent.
- UTF-8 text for an ASCII number, a U+2212 sign, and the UTF-8 bytes of a U+00A0 or U+202F separator.
- The ASCII-space substitution that the char parser accepts and the UTF-8 parser rejects, plus one invalid UTF-8 sequence.
- Every standard specifier row, `"Q"`, `"D "`, a precision of `1000000000`, and the custom-format table.
- `TryFormat` into a buffer of the exact length, one element shorter, and length 0.
- A current-culture test that sets and restores `CultureInfo.CurrentCulture`.

## Out of scope

- `Byte`, `SByte`, `Int16`, `UInt16`, `Int128`, `UInt128`, `BigInteger`, and the floating-point and decimal types.
- `Convert.ToInt32` and the other `Convert` methods.
- Composite format strings and alignment. `string.Format` passes the specifier portion to the same formatter. Alignment stays in composite formatting.
- `System.Buffers.Text.Utf8Parser` and `Utf8Formatter`.
- JSON number readers.
- Exception message text.
- A parser or formatter implementation. That work belongs to the later M1 issues.
- Performance claims. Baseline measurements belong to issue `#24`.

## Sources

Rules and examples come from .NET Runtime 10.0.13 and `dotnet/runtime` tag `v10.0.13`:

- `src/libraries/System.Private.CoreLib/src/System/Number.Parsing.cs`
- `src/libraries/Common/src/System/Number.Parsing.Common.cs`
- `src/libraries/System.Private.CoreLib/src/System/Number.Formatting.cs`
- `src/libraries/Common/src/System/Number.Formatting.Common.cs`
- `src/libraries/System.Private.CoreLib/src/System/Globalization/NumberStyles.cs`
- `src/libraries/System.Private.CoreLib/src/System/Globalization/NumberFormatInfo.cs`, including `ValidateParseStyleInteger` and the sign flags
- `src/libraries/System.Private.CoreLib/src/System/Int32.cs`, `UInt32.cs`, `Int64.cs`, and `UInt64.cs`, for the public methods and the digit limits

The public method list was taken from those four type files. The examples were executed on Runtime 10.0.13.
