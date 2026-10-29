using System;
using System.Globalization;
using System.Runtime.CompilerServices;

namespace Kexiva;

/// <summary>
/// Shared scan for <see cref="int"/> and <see cref="uint"/>. Each policy owns overflow.
/// </summary>
internal static class IntegerParser
{
    private const int _decimalDigitLimit = 10;
    private const int _hexDigitLimit = 8;
    private const int _binaryDigitLimit = 32;

    private const int _stateSign = 0x0001;
    private const int _stateParens = 0x0002;
    private const int _stateDigits = 0x0004;
    private const int _stateNonZero = 0x0008;
    private const int _stateDecimal = 0x0010;
    private const int _stateCurrency = 0x0020;

    private const NumberStyles _invalidStyles = ~(NumberStyles.AllowLeadingWhite | NumberStyles.AllowTrailingWhite
        | NumberStyles.AllowLeadingSign | NumberStyles.AllowTrailingSign
        | NumberStyles.AllowParentheses | NumberStyles.AllowDecimalPoint
        | NumberStyles.AllowThousands | NumberStyles.AllowExponent
        | NumberStyles.AllowCurrencySymbol | NumberStyles.AllowHexSpecifier
        | NumberStyles.AllowBinarySpecifier);

    internal static void ValidateStyle(NumberStyles style)
    {
        if ((style & (_invalidStyles | NumberStyles.AllowHexSpecifier | NumberStyles.AllowBinarySpecifier)) != 0
            && (style & ~NumberStyles.HexNumber) != 0
            && (style & ~NumberStyles.BinaryNumber) != 0)
        {
            throw new ArgumentException("The number style is not valid.", nameof(style));
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static bool TryParse(ReadOnlySpan<char> text, NumberStyles styles, NumberFormatInfo info, out uint result)
    {
        if ((styles & ~NumberStyles.Integer) == 0)
        {
            return TryParseUnsignedIntegerStyle<char, CharUnits>(text, styles, info, out result);
        }

        return TryParseCore<char, CharUnits, uint, UInt32Policy>(text, styles, info, out result);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static bool TryParse(ReadOnlySpan<byte> text, NumberStyles styles, NumberFormatInfo info, out uint result)
    {
        if ((styles & ~NumberStyles.Integer) == 0)
        {
            return TryParseUnsignedIntegerStyle<byte, ByteUnits>(text, styles, info, out result);
        }

        return TryParseCore<byte, ByteUnits, uint, UInt32Policy>(text, styles, info, out result);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static bool TryParse(ReadOnlySpan<char> text, NumberStyles styles, NumberFormatInfo info, out int result)
    {
        if ((styles & ~NumberStyles.Integer) == 0)
        {
            return TryParseSignedIntegerStyle<char, CharUnits>(text, styles, info, out result);
        }

        return TryParseCore<char, CharUnits, int, Int32Policy>(text, styles, info, out result);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static bool TryParse(ReadOnlySpan<byte> text, NumberStyles styles, NumberFormatInfo info, out int result)
    {
        if ((styles & ~NumberStyles.Integer) == 0)
        {
            return TryParseSignedIntegerStyle<byte, ByteUnits>(text, styles, info, out result);
        }

        return TryParseCore<byte, ByteUnits, int, Int32Policy>(text, styles, info, out result);
    }

    private static bool TryParseCore<TUnit, TUnits, TNumber, TPolicy>(ReadOnlySpan<TUnit> text, NumberStyles styles, NumberFormatInfo info, out TNumber result)
        where TUnit : struct
        where TUnits : IUnits<TUnit>
        where TNumber : struct
        where TPolicy : IPolicy<TNumber>
    {
        if ((styles & ~NumberStyles.Integer) == 0)
        {
            if (TPolicy.IsSigned)
            {
                bool parsed = TryParseSignedIntegerStyle<TUnit, TUnits>(text, styles, info, out int signed);
                result = Unsafe.BitCast<int, TNumber>(signed);
                return parsed;
            }

            bool parsedUnsigned = TryParseUnsignedIntegerStyle<TUnit, TUnits>(text, styles, info, out uint unsigned);
            result = Unsafe.BitCast<uint, TNumber>(unsigned);
            return parsedUnsigned;
        }

        if ((styles & NumberStyles.AllowHexSpecifier) != 0)
        {
            return TryParseRadix<TUnit, TUnits, TNumber, TPolicy>(text, styles, shift: 4, _hexDigitLimit, hex: true, out result);
        }

        if ((styles & NumberStyles.AllowBinarySpecifier) != 0)
        {
            return TryParseRadix<TUnit, TUnits, TNumber, TPolicy>(text, styles, shift: 1, _binaryDigitLimit, hex: false, out result);
        }

        return TryParseGeneral<TUnit, TUnits, TNumber, TPolicy>(text, styles, info, out result);
    }

    // Keep this loop type-specific. On 2026-10-28 a shared generic loop moved the UInt32 6-digit ratio from 0.79 to 1.06.
    private static bool TryParseUnsignedIntegerStyle<TUnit, TUnits>(ReadOnlySpan<TUnit> text, NumberStyles styles, NumberFormatInfo info, out uint result)
        where TUnit : struct
        where TUnits : IUnits<TUnit>
    {
        result = 0;
        if (text.IsEmpty)
        {
            return false;
        }

        int index = 0;
        uint num = TUnits.At(text, 0);
        if ((styles & NumberStyles.AllowLeadingWhite) != 0 && IsWhite(num))
        {
            do
            {
                index++;
                if ((uint)index >= (uint)text.Length)
                {
                    return false;
                }

                num = TUnits.At(text, index);
            }
            while (IsWhite(num));
        }

        bool isNegative = false;
        if ((styles & NumberStyles.AllowLeadingSign) != 0 && !TryReadIntegerSign<TUnit, TUnits>(text, ref index, ref num, info, ref isNegative))
        {
            return false;
        }

        // A negative value overflows. Negative zero still succeeds.
        bool overflow = isNegative;
        if (!IsDigit(num))
        {
            return false;
        }

        if (num == '0')
        {
            do
            {
                index++;
                if ((uint)index >= (uint)text.Length)
                {
                    result = 0;
                    return true;
                }

                num = TUnits.At(text, index);
            }
            while (num == '0');

            if (!IsDigit(num))
            {
                return FinishTrailing<TUnit, TUnits, uint, UInt32Policy>(text, index, num, styles, overflow: false, answer: 0, isNegative: isNegative, preserveBits: false, out result);
            }
        }

        ulong answer = num - '0';
        index++;
        for (int i = 0; i < _decimalDigitLimit - 2; i++)
        {
            if ((uint)index >= (uint)text.Length)
            {
                return CompleteUnsigned(overflow, answer, out result);
            }

            num = TUnits.At(text, index);
            if (!IsDigit(num))
            {
                return FinishTrailing<TUnit, TUnits, uint, UInt32Policy>(text, index, num, styles, overflow, answer, isNegative, preserveBits: false, out result);
            }

            index++;
            answer = (answer * 10) + (num - '0');
        }

        if ((uint)index >= (uint)text.Length)
        {
            return CompleteUnsigned(overflow, answer, out result);
        }

        num = TUnits.At(text, index);
        if (!IsDigit(num))
        {
            return FinishTrailing<TUnit, TUnits, uint, UInt32Policy>(text, index, num, styles, overflow, answer, isNegative, preserveBits: false, out result);
        }

        index++;
        const uint maxValueDividedBy10 = 429496729;
        overflow |= answer > maxValueDividedBy10 || (answer == maxValueDividedBy10 && num > '5');
        answer = (answer * 10) + (num - '0');
        if ((uint)index >= (uint)text.Length)
        {
            return CompleteUnsigned(overflow, answer, out result);
        }

        num = TUnits.At(text, index);
        while (IsDigit(num))
        {
            overflow = true;
            index++;
            if ((uint)index >= (uint)text.Length)
            {
                result = 0;
                return false;
            }

            num = TUnits.At(text, index);
        }

        return FinishTrailing<TUnit, TUnits, uint, UInt32Policy>(text, index, num, styles, overflow, answer, isNegative, preserveBits: false, out result);
    }

    private static bool TryParseSignedIntegerStyle<TUnit, TUnits>(ReadOnlySpan<TUnit> text, NumberStyles styles, NumberFormatInfo info, out int result)
        where TUnit : struct
        where TUnits : IUnits<TUnit>
    {
        result = 0;
        if (text.IsEmpty)
        {
            return false;
        }

        int index = 0;
        uint num = TUnits.At(text, 0);
        if ((styles & NumberStyles.AllowLeadingWhite) != 0 && IsWhite(num))
        {
            do
            {
                index++;
                if ((uint)index >= (uint)text.Length)
                {
                    return false;
                }

                num = TUnits.At(text, index);
            }
            while (IsWhite(num));
        }

        bool isNegative = false;
        if ((styles & NumberStyles.AllowLeadingSign) != 0)
        {
            // Char keeps the invariant sign here. A separate call slowed its negative 10-digit parse.
            if (typeof(TUnit) == typeof(char) && HasInvariantSigns(info))
            {
                if (num is '-' or '+')
                {
                    isNegative = num == '-';
                    if (!Advance<TUnit, TUnits>(text, ref index, ref num))
                    {
                        return false;
                    }
                }
            }
            else if (!TryReadIntegerSign<TUnit, TUnits>(text, ref index, ref num, info, ref isNegative))
            {
                return false;
            }
        }

        // Negative zero succeeds. A non-zero magnitude outside the signed range overflows.
        bool overflow = false;
        if (!IsDigit(num))
        {
            return false;
        }

        if (num == '0')
        {
            do
            {
                index++;
                if ((uint)index >= (uint)text.Length)
                {
                    result = 0;
                    return true;
                }

                num = TUnits.At(text, index);
            }
            while (num == '0');

            if (!IsDigit(num))
            {
                return FinishTrailing<TUnit, TUnits, int, Int32Policy>(text, index, num, styles, overflow: false, answer: 0, isNegative: isNegative, preserveBits: false, out result);
            }
        }

        ulong answer = num - '0';
        index++;
        for (int i = 0; i < _decimalDigitLimit - 2; i++)
        {
            if ((uint)index >= (uint)text.Length)
            {
                if (typeof(TUnit) == typeof(char))
                {
                    return CompleteSignedInRange(answer, isNegative, out result);
                }

                return CompleteSignedHot(overflow, answer, isNegative, preserveBits: false, out result);
            }

            num = TUnits.At(text, index);
            if (!IsDigit(num))
            {
                return FinishTrailing<TUnit, TUnits, int, Int32Policy>(text, index, num, styles, overflow, answer, isNegative, preserveBits: false, out result);
            }

            index++;
            answer = (answer * 10) + (num - '0');
        }

        if ((uint)index >= (uint)text.Length)
        {
            if (typeof(TUnit) == typeof(char))
            {
                return CompleteSignedInRange(answer, isNegative, out result);
            }

            return CompleteSignedHot(overflow, answer, isNegative, preserveBits: false, out result);
        }

        num = TUnits.At(text, index);
        if (!IsDigit(num))
        {
            return FinishTrailing<TUnit, TUnits, int, Int32Policy>(text, index, num, styles, overflow, answer, isNegative, preserveBits: false, out result);
        }

        index++;
        const uint maxValueDividedBy10 = 214748364;
        uint lastDigitLimit = isNegative ? (uint)'8' : (uint)'7';
        overflow |= answer > maxValueDividedBy10 || (answer == maxValueDividedBy10 && num > lastDigitLimit);
        answer = (answer * 10) + (num - '0');
        if ((uint)index >= (uint)text.Length)
        {
            if (typeof(TUnit) == typeof(char))
            {
                return CompleteSigned(overflow, answer, isNegative, preserveBits: false, out result);
            }

            return CompleteSignedHot(overflow, answer, isNegative, preserveBits: false, out result);
        }

        num = TUnits.At(text, index);
        while (IsDigit(num))
        {
            overflow = true;
            index++;
            if ((uint)index >= (uint)text.Length)
            {
                result = 0;
                return false;
            }

            num = TUnits.At(text, index);
        }

        return FinishTrailing<TUnit, TUnits, int, Int32Policy>(text, index, num, styles, overflow, answer, isNegative, preserveBits: false, out result);
    }

    private static bool TryReadIntegerSign<TUnit, TUnits>(
        ReadOnlySpan<TUnit> text,
        ref int index,
        ref uint num,
        NumberFormatInfo info,
        ref bool isNegative)
        where TUnit : struct
        where TUnits : IUnits<TUnit>
    {
        if (HasInvariantSigns(info))
        {
            if (num is '-' or '+')
            {
                isNegative = num == '-';
                return Advance<TUnit, TUnits>(text, ref index, ref num);
            }

            return true;
        }

        // On the integer path, U+002D is the negative sign before PositiveSign when the culture uses an alternate minus.
        if (AllowsHyphen(info) && num == '-')
        {
            isNegative = true;
            return Advance<TUnit, TUnits>(text, ref index, ref num);
        }

        if (TUnits.TryMatch(text, index, info.PositiveSign, out int consumed))
        {
            index += consumed;
            return Load<TUnit, TUnits>(text, ref index, ref num);
        }

        if (TUnits.TryMatch(text, index, info.NegativeSign, out consumed))
        {
            isNegative = true;
            index += consumed;
            return Load<TUnit, TUnits>(text, ref index, ref num);
        }

        return true;
    }

    private static bool Advance<TUnit, TUnits>(ReadOnlySpan<TUnit> text, ref int index, ref uint num)
        where TUnit : struct
        where TUnits : IUnits<TUnit>
    {
        index++;
        return Load<TUnit, TUnits>(text, ref index, ref num);
    }

    private static bool Load<TUnit, TUnits>(ReadOnlySpan<TUnit> text, ref int index, ref uint num)
        where TUnit : struct
        where TUnits : IUnits<TUnit>
    {
        if ((uint)index >= (uint)text.Length)
        {
            return false;
        }

        num = TUnits.At(text, index);
        return true;
    }

    private static bool TryParseRadix<TUnit, TUnits, TNumber, TPolicy>(
        ReadOnlySpan<TUnit> text,
        NumberStyles styles,
        int shift,
        int digitLimit,
        bool hex,
        out TNumber result)
        where TUnit : struct
        where TUnits : IUnits<TUnit>
        where TNumber : struct
        where TPolicy : IPolicy<TNumber>
    {
        result = default;
        if (text.IsEmpty)
        {
            return false;
        }

        int index = 0;
        uint num = TUnits.At(text, 0);
        if ((styles & NumberStyles.AllowLeadingWhite) != 0 && IsWhite(num))
        {
            do
            {
                index++;
                if ((uint)index >= (uint)text.Length)
                {
                    return false;
                }

                num = TUnits.At(text, index);
            }
            while (IsWhite(num));
        }

        if (DigitValue(num, hex) < 0)
        {
            return false;
        }

        if (num == '0')
        {
            do
            {
                index++;
                if ((uint)index >= (uint)text.Length)
                {
                    result = default;
                    return true;
                }

                num = TUnits.At(text, index);
            }
            while (num == '0');

            if (DigitValue(num, hex) < 0)
            {
                return FinishTrailing<TUnit, TUnits, TNumber, TPolicy>(text, index, num, styles, overflow: false, answer: 0, isNegative: false, preserveBits: true, out result);
            }
        }

        uint answer = (uint)DigitValue(num, hex);
        index++;
        for (int i = 0; i < digitLimit - 1; i++)
        {
            if ((uint)index >= (uint)text.Length)
            {
                return TPolicy.TryFinish(overflow: false, answer, isNegative: false, preserveBits: true, out result);
            }

            num = TUnits.At(text, index);
            int digit = DigitValue(num, hex);
            if (digit < 0)
            {
                return FinishTrailing<TUnit, TUnits, TNumber, TPolicy>(text, index, num, styles, overflow: false, answer, isNegative: false, preserveBits: true, out result);
            }

            index++;
            answer = (answer << shift) + (uint)digit;
        }

        if ((uint)index >= (uint)text.Length)
        {
            return TPolicy.TryFinish(overflow: false, answer, isNegative: false, preserveBits: true, out result);
        }

        num = TUnits.At(text, index);
        if (DigitValue(num, hex) < 0)
        {
            return FinishTrailing<TUnit, TUnits, TNumber, TPolicy>(text, index, num, styles, overflow: false, answer, isNegative: false, preserveBits: true, out result);
        }

        do
        {
            index++;
            if ((uint)index >= (uint)text.Length)
            {
                result = default;
                return false;
            }

            num = TUnits.At(text, index);
        }
        while (DigitValue(num, hex) >= 0);

        return FinishTrailing<TUnit, TUnits, TNumber, TPolicy>(text, index, num, styles, overflow: true, answer, isNegative: false, preserveBits: true, out result);
    }

    private static bool TryParseGeneral<TUnit, TUnits, TNumber, TPolicy>(ReadOnlySpan<TUnit> text, NumberStyles styles, NumberFormatInfo info, out TNumber result)
        where TUnit : struct
        where TUnits : IUnits<TUnit>
        where TNumber : struct
        where TPolicy : IPolicy<TNumber>
    {
        Span<byte> digits = stackalloc byte[_decimalDigitLimit + 1];
        digits.Clear();
        NumberState number = new() { Digits = digits };
        if (!TryReadGeneral<TUnit, TUnits, TNumber, TPolicy>(text, styles, info, ref number, out int end)
            || !TrailingZeros<TUnit, TUnits>(text, end))
        {
            result = default;
            return false;
        }

        return TryFromBuffer<TNumber, TPolicy>(ref number, out result);
    }

    private static bool TryReadGeneral<TUnit, TUnits, TNumber, TPolicy>(
        ReadOnlySpan<TUnit> text,
        NumberStyles styles,
        NumberFormatInfo info,
        ref NumberState number,
        out int end)
        where TUnit : struct
        where TUnits : IUnits<TUnit>
        where TNumber : struct
        where TPolicy : IPolicy<TNumber>
    {
        bool parsingCurrency = (styles & NumberStyles.AllowCurrencySymbol) != 0;
        ReadOnlySpan<char> decimalSeparator = parsingCurrency ? info.CurrencyDecimalSeparator : info.NumberDecimalSeparator;
        ReadOnlySpan<char> groupSeparator = parsingCurrency ? info.CurrencyGroupSeparator : info.NumberGroupSeparator;
        ReadOnlySpan<char> currencySymbol = parsingCurrency ? info.CurrencySymbol : [];

        int index = 0;
        int state = 0;
        uint ch = UnitAt<TUnit, TUnits>(text, index);
        while (true)
        {
            bool stopForSignGap = IsWhite(ch)
                && (styles & NumberStyles.AllowLeadingWhite) != 0
                && (state & _stateSign) != 0
                && (state & _stateCurrency) == 0
                && info.NumberNegativePattern != 2;
            if (!IsWhite(ch) || (styles & NumberStyles.AllowLeadingWhite) == 0 || stopForSignGap)
            {
                if ((styles & NumberStyles.AllowLeadingSign) != 0
                    && (state & _stateSign) == 0
                    && TryReadGeneralSign<TUnit, TUnits>(text, index, info, ref number, out int next))
                {
                    state |= _stateSign;
                    index = next - 1;
                }
                else if (ch == '(' && (styles & NumberStyles.AllowParentheses) != 0 && (state & _stateSign) == 0)
                {
                    state |= _stateSign | _stateParens;
                    number.IsNegative = true;
                }
                else if (!currencySymbol.IsEmpty && TUnits.TryMatch(text, index, currencySymbol, out int consumed))
                {
                    state |= _stateCurrency;
                    currencySymbol = [];
                    index += consumed - 1;
                }
                else
                {
                    break;
                }
            }

            index++;
            ch = UnitAt<TUnit, TUnits>(text, index);
        }

        int digitCount = 0;
        int digitEnd = 0;
        int maxDigits = number.Digits.Length - 1;
        while (true)
        {
            if (IsDigit(ch))
            {
                state |= _stateDigits;
                if (ch != '0' || (state & _stateNonZero) != 0)
                {
                    if (digitCount < maxDigits)
                    {
                        number.Digits[digitCount] = (byte)ch;
                        if (ch != '0')
                        {
                            digitEnd = digitCount + 1;
                        }
                    }
                    else if (ch != '0')
                    {
                        number.HasNonZeroTail = true;
                    }

                    if ((state & _stateDecimal) == 0)
                    {
                        number.Scale++;
                    }

                    digitCount++;
                    state |= _stateNonZero;
                }
                else if ((state & _stateDecimal) != 0)
                {
                    number.Scale--;
                }
            }
            else if ((styles & NumberStyles.AllowDecimalPoint) != 0
                && (state & _stateDecimal) == 0
                && TryMatchDecimal<TUnit, TUnits>(text, index, info, state, parsingCurrency, decimalSeparator, out int decimalLength))
            {
                state |= _stateDecimal;
                index += decimalLength - 1;
            }
            else if ((styles & NumberStyles.AllowThousands) != 0
                && (state & _stateDigits) != 0
                && (state & _stateDecimal) == 0
                && TryMatchGroup<TUnit, TUnits>(text, index, info, state, parsingCurrency, groupSeparator, out int groupLength))
            {
                index += groupLength - 1;
            }
            else
            {
                break;
            }

            index++;
            ch = UnitAt<TUnit, TUnits>(text, index);
        }

        number.DigitsCount = digitEnd;
        if ((state & _stateDigits) != 0)
        {
            if ((ch is 'E' or 'e') && (styles & NumberStyles.AllowExponent) != 0)
            {
                TryReadExponent<TUnit, TUnits>(text, ref index, ref ch, info, ref number);
            }

            while (true)
            {
                if (!IsWhite(ch) || (styles & NumberStyles.AllowTrailingWhite) == 0)
                {
                    if ((styles & NumberStyles.AllowTrailingSign) != 0
                        && (state & _stateSign) == 0
                        && TryReadGeneralSign<TUnit, TUnits>(text, index, info, ref number, out int next))
                    {
                        state |= _stateSign;
                        index = next - 1;
                    }
                    else if (ch == ')' && (state & _stateParens) != 0)
                    {
                        state &= ~_stateParens;
                    }
                    else if (!currencySymbol.IsEmpty && TUnits.TryMatch(text, index, currencySymbol, out int consumed))
                    {
                        currencySymbol = [];
                        index += consumed - 1;
                    }
                    else
                    {
                        break;
                    }
                }

                index++;
                ch = UnitAt<TUnit, TUnits>(text, index);
            }

            if ((state & _stateParens) == 0)
            {
                if ((state & _stateNonZero) == 0)
                {
                    number.Scale = 0;
                    if (!TPolicy.KeepNegativeZeroWithDecimal || (state & _stateDecimal) == 0)
                    {
                        number.IsNegative = false;
                    }
                }

                end = index;
                return true;
            }
        }

        end = index;
        return false;
    }

    private static bool TryReadGeneralSign<TUnit, TUnits>(
        ReadOnlySpan<TUnit> text,
        int index,
        NumberFormatInfo info,
        ref NumberState number,
        out int next)
        where TUnit : struct
        where TUnits : IUnits<TUnit>
    {
        // The general path matches PositiveSign before the negative sign and before U+002D.
        if (TUnits.TryMatch(text, index, info.PositiveSign, out int consumed))
        {
            next = index + consumed;
            return true;
        }

        if (TUnits.TryMatch(text, index, info.NegativeSign, out int negativeLength))
        {
            number.IsNegative = true;
            next = index + negativeLength;
            return true;
        }

        if (AllowsHyphen(info) && index < text.Length && TUnits.At(text, index) == '-')
        {
            number.IsNegative = true;
            next = index + 1;
            return true;
        }

        next = index;
        return false;
    }

    private static void TryReadExponent<TUnit, TUnits>(
        ReadOnlySpan<TUnit> text,
        ref int index,
        ref uint ch,
        NumberFormatInfo info,
        ref NumberState number)
        where TUnit : struct
        where TUnits : IUnits<TUnit>
    {
        int marker = index;
        index++;
        ch = UnitAt<TUnit, TUnits>(text, index);
        bool negativeExponent = false;
        if (TUnits.TryMatch(text, index, info.PositiveSign, out int consumed))
        {
            index += consumed;
            ch = UnitAt<TUnit, TUnits>(text, index);
        }
        else if (TUnits.TryMatch(text, index, info.NegativeSign, out int negativeLength))
        {
            negativeExponent = true;
            index += negativeLength;
            ch = UnitAt<TUnit, TUnits>(text, index);
        }
        else if (AllowsHyphen(info) && index < text.Length && TUnits.At(text, index) == '-')
        {
            negativeExponent = true;
            index++;
            ch = UnitAt<TUnit, TUnits>(text, index);
        }

        if (!IsDigit(ch))
        {
            index = marker;
            ch = UnitAt<TUnit, TUnits>(text, index);
            return;
        }

        int exponent = 0;
        do
        {
            if (exponent >= 100_000_000)
            {
                exponent = int.MaxValue;
                number.Scale = 0;
                while (IsDigit(ch))
                {
                    index++;
                    ch = UnitAt<TUnit, TUnits>(text, index);
                }

                break;
            }

            exponent = (exponent * 10) + (int)(ch - '0');
            index++;
            ch = UnitAt<TUnit, TUnits>(text, index);
        }
        while (IsDigit(ch));

        if (negativeExponent)
        {
            exponent = unchecked(-exponent);
        }

        number.Scale = unchecked(number.Scale + exponent);
    }

    private static bool TryMatchDecimal<TUnit, TUnits>(
        ReadOnlySpan<TUnit> text,
        int index,
        NumberFormatInfo info,
        int state,
        bool parsingCurrency,
        ReadOnlySpan<char> decimalSeparator,
        out int consumed)
        where TUnit : struct
        where TUnits : IUnits<TUnit>
    {
        if (TUnits.TryMatch(text, index, decimalSeparator, out consumed))
        {
            return true;
        }

        if (parsingCurrency && (state & _stateCurrency) == 0 && TUnits.TryMatch(text, index, info.NumberDecimalSeparator, out consumed))
        {
            return true;
        }

        consumed = 0;
        return false;
    }

    private static bool TryMatchGroup<TUnit, TUnits>(
        ReadOnlySpan<TUnit> text,
        int index,
        NumberFormatInfo info,
        int state,
        bool parsingCurrency,
        ReadOnlySpan<char> groupSeparator,
        out int consumed)
        where TUnit : struct
        where TUnits : IUnits<TUnit>
    {
        if (TUnits.TryMatch(text, index, groupSeparator, out consumed))
        {
            return true;
        }

        if (parsingCurrency && (state & _stateCurrency) == 0 && TUnits.TryMatch(text, index, info.NumberGroupSeparator, out consumed))
        {
            return true;
        }

        consumed = 0;
        return false;
    }

    private static bool TryFromBuffer<TNumber, TPolicy>(ref NumberState number, out TNumber result)
        where TNumber : struct
        where TPolicy : IPolicy<TNumber>
    {
        if (TPolicy.IsSigned)
        {
            bool parsed = Int32Policy.TryFromBuffer(ref number, out int signed);
            result = Unsafe.BitCast<int, TNumber>(signed);
            return parsed;
        }

        bool parsedUnsigned = UInt32Policy.TryFromBuffer(ref number, out uint unsigned);
        result = Unsafe.BitCast<uint, TNumber>(unsigned);
        return parsedUnsigned;
    }

    private static bool FinishTrailing<TUnit, TUnits, TNumber, TPolicy>(
        ReadOnlySpan<TUnit> text,
        int index,
        uint num,
        NumberStyles styles,
        bool overflow,
        ulong answer,
        bool isNegative,
        bool preserveBits,
        out TNumber result)
        where TUnit : struct
        where TUnits : IUnits<TUnit>
        where TNumber : struct
        where TPolicy : IPolicy<TNumber>
    {
        if (IsWhite(num))
        {
            if ((styles & NumberStyles.AllowTrailingWhite) == 0)
            {
                result = default;
                return false;
            }

            for (index++; index < text.Length; index++)
            {
                if (!IsWhite(TUnits.At(text, index)))
                {
                    break;
                }
            }

            if ((uint)index >= (uint)text.Length)
            {
                return TPolicy.TryFinish(overflow, answer, isNegative, preserveBits, out result);
            }
        }

        if (!TrailingZeros<TUnit, TUnits>(text, index))
        {
            result = default;
            return false;
        }

        return TPolicy.TryFinish(overflow, answer, isNegative, preserveBits, out result);
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool CompleteUnsigned(bool overflow, ulong answer, out uint result)
    {
        if (overflow || answer > uint.MaxValue)
        {
            result = 0;
            return false;
        }

        result = (uint)answer;
        return true;
    }

    // The caller accepted at most 9 digits. The magnitude fits in a positive int.
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool CompleteSignedInRange(ulong answer, bool isNegative, out int result)
    {
        if (isNegative)
        {
            result = -(int)answer;
            return true;
        }

        result = (int)answer;
        return true;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static bool CompleteSigned(bool overflow, ulong answer, bool isNegative, bool preserveBits, out int result)
    {
        return CompleteSignedHot(overflow, answer, isNegative, preserveBits, out result);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool CompleteSignedHot(bool overflow, ulong answer, bool isNegative, bool preserveBits, out int result)
    {
        if (overflow)
        {
            result = 0;
            return false;
        }

        if (preserveBits)
        {
            result = (int)(uint)answer;
            return true;
        }

        if (isNegative)
        {
            if (answer > (ulong)int.MaxValue + 1UL)
            {
                result = 0;
                return false;
            }

            result = answer == (ulong)int.MaxValue + 1UL ? int.MinValue : -(int)answer;
            return true;
        }

        if (answer > int.MaxValue)
        {
            result = 0;
            return false;
        }

        result = (int)answer;
        return true;
    }

    private static bool TrailingZeros<TUnit, TUnits>(ReadOnlySpan<TUnit> text, int index)
        where TUnit : struct
        where TUnits : IUnits<TUnit>
    {
        for (int i = index; i < text.Length; i++)
        {
            if (TUnits.At(text, i) != 0)
            {
                return false;
            }
        }

        return true;
    }

    private static uint UnitAt<TUnit, TUnits>(ReadOnlySpan<TUnit> text, int index)
        where TUnit : struct
        where TUnits : IUnits<TUnit>
    {
        return (uint)index >= (uint)text.Length ? 0 : TUnits.At(text, index);
    }

    private static int DigitValue(uint ch, bool hex)
    {
        if (hex)
        {
            if (ch is >= '0' and <= '9')
            {
                return (int)(ch - '0');
            }

            if (ch is >= 'A' and <= 'F')
            {
                return (int)(ch - 'A' + 10);
            }

            if (ch is >= 'a' and <= 'f')
            {
                return (int)(ch - 'a' + 10);
            }

            return -1;
        }

        return ch is '0' or '1' ? (int)(ch - '0') : -1;
    }

    private static bool IsWhite(uint ch)
    {
        return ch == 0x20 || (ch - 0x09) <= (0x0D - 0x09);
    }

    private static bool IsDigit(uint ch)
    {
        return ch - '0' <= 9;
    }

    private static bool HasInvariantSigns(NumberFormatInfo info)
    {
        return string.Equals(info.PositiveSign, "+", StringComparison.Ordinal)
            && string.Equals(info.NegativeSign, "-", StringComparison.Ordinal);
    }

    private static bool AllowsHyphen(NumberFormatInfo info)
    {
        if (info.NegativeSign.Length != 1)
        {
            return false;
        }

        return info.NegativeSign[0] switch
        {
            '\u2012' or '\u207B' or '\u208B' or '\u2212' or '\u2796' or '\uFE63' or '\uFF0D' => true,
            _ => false,
        };
    }

    private interface IUnits<TUnit>
        where TUnit : struct
    {
        static abstract uint At(ReadOnlySpan<TUnit> text, int index);

        static abstract bool TryMatch(ReadOnlySpan<TUnit> text, int index, ReadOnlySpan<char> expected, out int consumed);
    }

    private readonly struct CharUnits : IUnits<char>
    {
        public static uint At(ReadOnlySpan<char> text, int index)
        {
            return text[index];
        }

        public static bool TryMatch(ReadOnlySpan<char> text, int index, ReadOnlySpan<char> expected, out int consumed)
        {
            consumed = 0;
            if (expected.IsEmpty)
            {
                return false;
            }

            if (index > text.Length || expected.Length > text.Length - index)
            {
                return false;
            }

            for (int i = 0; i < expected.Length; i++)
            {
                char actual = text[index + i];
                char wanted = expected[i];
                if (actual != wanted && !(actual == ' ' && wanted is '\u00A0' or '\u202F'))
                {
                    return false;
                }
            }

            consumed = expected.Length;
            return true;
        }
    }

    private readonly struct ByteUnits : IUnits<byte>
    {
        public static uint At(ReadOnlySpan<byte> text, int index)
        {
            return text[index];
        }

        public static bool TryMatch(ReadOnlySpan<byte> text, int index, ReadOnlySpan<char> expected, out int consumed)
        {
            consumed = 0;
            if (expected.IsEmpty || (uint)index > (uint)text.Length)
            {
                return false;
            }

            // Encode one scalar into a 4-byte stack buffer. A long sign or separator must not allocate.
            ReadOnlySpan<byte> rest = text[index..];
            Span<byte> encoded = stackalloc byte[4];
            int position = 0;
            for (int charIndex = 0; charIndex < expected.Length;)
            {
                int written = EncodeScalar(expected, charIndex, out int charsUsed, encoded);
                if (position > rest.Length - written)
                {
                    return false;
                }

                if (!rest.Slice(position, written).SequenceEqual(encoded[..written]))
                {
                    return false;
                }

                position += written;
                charIndex += charsUsed;
            }

            consumed = position;
            return true;
        }

        private static int EncodeScalar(ReadOnlySpan<char> text, int index, out int charsUsed, Span<byte> destination)
        {
            char first = text[index];
            uint codePoint;
            if (char.IsHighSurrogate(first)
                && (uint)(index + 1) < (uint)text.Length
                && char.IsLowSurrogate(text[index + 1]))
            {
                codePoint = (uint)char.ConvertToUtf32(first, text[index + 1]);
                charsUsed = 2;
            }
            else
            {
                codePoint = first;
                charsUsed = 1;
                if (codePoint is >= 0xD800 and <= 0xDFFF)
                {
                    codePoint = 0xFFFD;
                }
            }

            if (codePoint <= 0x7F)
            {
                destination[0] = (byte)codePoint;
                return 1;
            }

            if (codePoint <= 0x7FF)
            {
                destination[0] = (byte)(0xC0 | (codePoint >> 6));
                destination[1] = (byte)(0x80 | (codePoint & 0x3F));
                return 2;
            }

            if (codePoint <= 0xFFFF)
            {
                destination[0] = (byte)(0xE0 | (codePoint >> 12));
                destination[1] = (byte)(0x80 | ((codePoint >> 6) & 0x3F));
                destination[2] = (byte)(0x80 | (codePoint & 0x3F));
                return 3;
            }

            destination[0] = (byte)(0xF0 | (codePoint >> 18));
            destination[1] = (byte)(0x80 | ((codePoint >> 12) & 0x3F));
            destination[2] = (byte)(0x80 | ((codePoint >> 6) & 0x3F));
            destination[3] = (byte)(0x80 | (codePoint & 0x3F));
            return 4;
        }
    }

    private ref struct NumberState
    {
        public Span<byte> Digits;
        public int DigitsCount;
        public int Scale;
        public bool IsNegative;
        public bool HasNonZeroTail;
    }

    /// <summary>
    /// Overflow rules for one 32-bit integer type.
    /// The scan calls these members outside the per-digit loop.
    /// </summary>
    private interface IPolicy<TNumber>
        where TNumber : struct
    {
        static abstract bool IsSigned { get; }

        static abstract uint MaxValueDividedBy10 { get; }

        static abstract bool KeepNegativeZeroWithDecimal { get; }

        static abstract bool InitialOverflow(bool isNegative);

        static abstract uint LastDigitLimit(bool isNegative);

        static abstract bool TryFinish(bool overflow, ulong answer, bool isNegative, bool preserveBits, out TNumber result);
    }

    private readonly struct UInt32Policy : IPolicy<uint>
    {
        public static bool IsSigned
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => false;
        }

        public static uint MaxValueDividedBy10
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => 429496729;
        }

        public static bool KeepNegativeZeroWithDecimal
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool InitialOverflow(bool isNegative)
        {
            return isNegative;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint LastDigitLimit(bool isNegative)
        {
            _ = isNegative;
            return (uint)'5';
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryFinish(bool overflow, ulong answer, bool isNegative, bool preserveBits, out uint result)
        {
            // Unsigned overflow already includes a negative sign. Radix calls do not use these arguments.
            _ = isNegative;
            _ = preserveBits;
            return CompleteUnsigned(overflow, answer, out result);
        }

        public static bool TryFromBuffer(ref NumberState number, out uint value)
        {
            value = 0;
            int scale = number.Scale;
            if (scale > _decimalDigitLimit || scale < number.DigitsCount || number.IsNegative || number.HasNonZeroTail)
            {
                return false;
            }

            int digitIndex = 0;
            uint parsed = 0;
            while (--scale >= 0)
            {
                if (parsed > MaxValueDividedBy10)
                {
                    return false;
                }

                parsed *= 10;
                if (digitIndex < number.DigitsCount)
                {
                    uint added = parsed + (uint)(number.Digits[digitIndex] - '0');
                    digitIndex++;
                    if (added < parsed)
                    {
                        return false;
                    }

                    parsed = added;
                }
            }

            value = parsed;
            return true;
        }
    }

    private readonly struct Int32Policy : IPolicy<int>
    {
        public static bool IsSigned
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => true;
        }

        public static uint MaxValueDividedBy10
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => 214748364;
        }

        public static bool KeepNegativeZeroWithDecimal
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool InitialOverflow(bool isNegative)
        {
            _ = isNegative;
            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint LastDigitLimit(bool isNegative)
        {
            return isNegative ? (uint)'8' : (uint)'7';
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryFinish(bool overflow, ulong answer, bool isNegative, bool preserveBits, out int result)
        {
            return CompleteSigned(overflow, answer, isNegative, preserveBits, out result);
        }

        public static bool TryFromBuffer(ref NumberState number, out int value)
        {
            value = 0;
            int scale = number.Scale;
            if (scale > _decimalDigitLimit || scale < number.DigitsCount || number.HasNonZeroTail)
            {
                return false;
            }

            int digitIndex = 0;
            uint parsed = 0;
            while (--scale >= 0)
            {
                if (parsed > MaxValueDividedBy10)
                {
                    return false;
                }

                parsed *= 10;
                if (digitIndex < number.DigitsCount)
                {
                    parsed += (uint)(number.Digits[digitIndex] - '0');
                    digitIndex++;
                }
            }

            if (number.IsNegative)
            {
                if (parsed > (uint)int.MaxValue + 1U)
                {
                    return false;
                }

                value = parsed == (uint)int.MaxValue + 1U ? int.MinValue : -(int)parsed;
                return true;
            }

            if (parsed > int.MaxValue)
            {
                return false;
            }

            value = (int)parsed;
            return true;
        }
    }
}
