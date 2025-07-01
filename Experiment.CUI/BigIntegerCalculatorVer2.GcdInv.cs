//#define LOG_DETAIL
#define ANALYZE_PERFORMANCE // パフォーマンスの分析をする
#define CAN_GET_CONFIGURATION // 定義済みシンボル情報を取得可能にする
#define USE_GCD_UINT32_OVERLOAD_WITHIN_GCD_GCD_UINT64_IF_POSSIBLE // Gcd(ulong, ulong) のオーバーロードにおいて、可能であれば Gcd(uint, uint) を呼び出す
#define USE_GCD_UINT32_OVERLOAD_WITHIN_GCD_SPAN_IF_POSSIBLE // Gcd(Span<uint>, ReadOnlySpan<uint>) のオーバーロードにおいて、可能であれば Gcd(uint, uint) を呼び出す
#define USE_GCD_UINT64_OVERLOAD_WITHIN_GCD_SPAN_IF_POSSIBLE // Gcd(Span<uint>, ReadOnlySpan<uint>) のオーバーロードにおいて、可能であれば Gcd(ulong, ulong) を呼び出す
using System;
using System.Buffers;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace Experiment.CUI
{
    internal static partial class BigIntegerCalculatorVer2
    {
        private const int _BIT_COUNT_PER_UINT32 = 32;
        private const int _BIT_COUNT_MASK_FOR_UINT32 = 31;
        private const int _SHIFT_BIT_COUNT_PER_UINT32 = 5;
        private const int _STACKALLOC_THRESHOLD = 64;

        static BigIntegerCalculatorVer2()
        {
#if DEBUG
            Assert(_BIT_COUNT_PER_UINT32 == sizeof(uint) * 8);
            Assert(_BIT_COUNT_MASK_FOR_UINT32 + 1 == _BIT_COUNT_PER_UINT32);
            Assert((1 << _SHIFT_BIT_COUNT_PER_UINT32) == _BIT_COUNT_PER_UINT32);
#endif
        }

#if CAN_GET_CONFIGURATION
        public static string GetGcdConfiguration()
        {
            var configurations =
                new[]
                {
#if NET8_0
                    "NET8_0",
#elif NET9_0
                    "NET9_0",
#else
#error Not supported .NET runtime.
#endif
#if USE_GCD_UINT32_OVERLOAD_WITHIN_GCD_GCD_UINT64_IF_POSSIBLE
                    "USE_GCD_UINT32_OVERLOAD_WITHIN_GCD_GCD_UINT64_IF_POSSIBLE",
#endif
#if USE_GCD_UINT32_OVERLOAD_WITHIN_GCD_SPAN_IF_POSSIBLE
                    "USE_GCD_UINT32_OVERLOAD_WITHIN_GCD_SPAN_IF_POSSIBLE",
#endif
#if USE_GCD_UINT64_OVERLOAD_WITHIN_GCD_SPAN_IF_POSSIBLE
                    "USE_GCD_UINT64_OVERLOAD_WITHIN_GCD_SPAN_IF_POSSIBLE",
#endif
                };
            var configuration = string.Join(" && ", configurations);
            return string.IsNullOrEmpty(configuration) ? "(NONE)" : configuration;
        }
#endif

        #region Gcd

        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        public static uint Gcd(uint left, uint right)
        {
#if LOG_DETAIL
            System.Diagnostics.Debug.Write($"GCD({left}, {right})=>");
#endif
            if (left == 0)
                return right != 0 ? right : throw new ArithmeticException("0 and 0 cannot be calculated.");
            if (right == 0)
                return left;

            return GcdCore(left, right);
        }

        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        public static ulong Gcd(ulong left, ulong right)
        {
#if LOG_DETAIL
            System.Diagnostics.Debug.Write($"GCD({left}, {right})=>");
#endif
            if (left == 0)
                return right != 0 ? right : throw new ArithmeticException("0 and 0 cannot be calculated.");
            if (right == 0)
                return left;

            Assert(left > 0 && right > 0);

#if USE_GCD_UINT32_OVERLOAD_WITHIN_GCD_GCD_UINT64_IF_POSSIBLE
            if (left <= uint.MaxValue && right <= uint.MaxValue)
                return GcdCore((uint)left, (uint)right);
#endif

            return GcdCore(left, right);
        }

#if ANALYZE_PERFORMANCE
        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
#else
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
#endif
        public static uint Gcd(ReadOnlySpan<uint> left, uint right)
        {
            Assert(left.Length >= 1);
            Assert(right != 0);

            if (left.Length == 1)
                return Gcd(left[0], right);

            Assert(left.Length >= 2);

            var leftBufferBytes = (uint[]?)null;
            try
            {
                Span<uint> rightBuffer = [right];
                Span<uint> leftBuffer =
                    left.Length <= _STACKALLOC_THRESHOLD
                    ? stackalloc uint[_STACKALLOC_THRESHOLD]
                    : (leftBufferBytes = ArrayPool<uint>.Shared.Rent(left.Length));
#if DEBUG
                leftBuffer.Fill(0xe5e5e5e5u);
#endif
                leftBuffer = leftBuffer[..left.Length];
                left.CopyTo(leftBuffer);
                GcdCore(leftBuffer, rightBuffer);
                Assert(GetActualWordLength(leftBuffer) == 1);
                return leftBuffer[0];
            }
            finally
            {
                if (leftBufferBytes != null)
                    ArrayPool<uint>.Shared.Return(leftBufferBytes);
            }
        }

#if ANALYZE_PERFORMANCE
        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
#else
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
#endif
        public static void Gcd(ReadOnlySpan<uint> left, ReadOnlySpan<uint> right, Span<uint> result)
        {
            Assert(left.Length >= 2 && right.Length >= 2);
            Assert(Compare(left, right) >= 0);
            Assert(result.Length == left.Length);

#if DEBUG
            result.Fill(0xe5e5e5e5u);
#endif
            var rightBufferBytes = (uint[]?)null;
            try
            {
                left.CopyTo(result);
                Span<uint> rightBuffer =
                    right.Length <= _STACKALLOC_THRESHOLD
                    ? stackalloc uint[_STACKALLOC_THRESHOLD]
                    : (rightBufferBytes = ArrayPool<uint>.Shared.Rent(right.Length));
#if DEBUG
                rightBuffer.Fill(0xe5e5e5e5u);
#endif
                rightBuffer = rightBuffer[..right.Length];
                right.CopyTo(rightBuffer);
                GcdCore(result, rightBuffer);
            }
            finally
            {
                if (rightBufferBytes != null)
                    ArrayPool<uint>.Shared.Return(rightBufferBytes);
            }
        }

#if ANALYZE_PERFORMANCE
        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
#else
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
#endif
        private static uint GcdCore(uint left, uint right)
        {
            Assert(left > 0 && right > 0);

            var k = 0;

            {
                var zeroBitCount = Min(BitOperations.TrailingZeroCount(left), BitOperations.TrailingZeroCount(right));
                left >>= zeroBitCount;
                right >>= zeroBitCount;
                k += zeroBitCount;
            }

            Assert(left > 0 && right > 0 && (uint.IsOddInteger(left) || uint.IsOddInteger(right)));

            left >>= BitOperations.TrailingZeroCount(left);
            right >>= BitOperations.TrailingZeroCount(right);

            while (true)
            {
#if LOG_DETAIL
                System.Diagnostics.Debug.Write($"({left}, {right})=>");
#endif
                Assert(left > 0 && right > 0 && uint.IsOddInteger(left) && uint.IsOddInteger(right));

                if (left == right)
                {
                    var result = left << k;
#if LOG_DETAIL
                    System.Diagnostics.Debug.WriteLine($"{result}");
#endif
                    return result;
                }

                if (left < right)
                    (right, left) = (left, right);

                Assert(left > 0 && right > 0 && left > right && uint.IsOddInteger(left) && uint.IsOddInteger(right));

                left -= right;

                Assert(uint.IsEvenInteger(left));

                left >>= BitOperations.TrailingZeroCount(left);
            }
        }

#if ANALYZE_PERFORMANCE
        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
#else
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
#endif
        private static ulong GcdCore(ulong left, ulong right)
        {
            var k = 0;

            {
                var zeroBitCount = Min(BitOperations.TrailingZeroCount(left), BitOperations.TrailingZeroCount(right));
                left >>= zeroBitCount;
                right >>= zeroBitCount;
                k += zeroBitCount;
            }

            Assert(left != 0 && ulong.Sign(right) > 0 && (ulong.IsOddInteger(left) || ulong.IsOddInteger(right)));

            left >>= BitOperations.TrailingZeroCount(left);
            right >>= BitOperations.TrailingZeroCount(right);

            while (true)
            {
#if LOG_DETAIL
                System.Diagnostics.Debug.Write($"({left}, {right})=>");
#endif
                Assert(left > 0 && right > 0 && ulong.IsOddInteger(left) && ulong.IsOddInteger(right));

#if USE_GCD_UINT32_OVERLOAD_WITHIN_GCD_GCD_UINT64_IF_POSSIBLE
                if (left <= uint.MaxValue && right <= uint.MaxValue)
                    return (ulong)GcdCore((uint)left, (uint)right) << k;
#endif
                if (left == right)
                {
                    var result = left << k;
#if LOG_DETAIL
                    System.Diagnostics.Debug.WriteLine($"{result}");
#endif
                    return result;
                }

                if (left < right)
                    (right, left) = (left, right);

                Assert(left > 0 && right > 0 && left > right && ulong.IsOddInteger(left) && ulong.IsOddInteger(right));

                left -= right;

                Assert(ulong.IsEvenInteger(left));

                left >>= BitOperations.TrailingZeroCount(left);
            }
        }

#if ANALYZE_PERFORMANCE
        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
#else
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
#endif
        private static void GcdCore(Span<uint> left, Span<uint> right)
        {
            // Remember that the variable 'left' must hold the greatest common divisor (GCD) when the function returns.

            Assert(left.Length > 0 && right.Length > 0);
            Assert(left.Length >= 2 || right.Length >= 2);
            Assert(left.Length >= right.Length);
            Assert(Compare(left, right) >= 0);

            var u = left[..GetActualWordLength(left)];
            var v = right[..GetActualWordLength(right)];

            Assert(u.Length > 0 && v.Length > 0);

            var k = 0;

            {
                var zeroBitCount = int.Min(GetTrailingZeroBitCount(u), GetTrailingZeroBitCount(v));
                ShiftRight(u, zeroBitCount);
                ShiftRight(v, zeroBitCount);
                k += zeroBitCount;
            }

            Assert(u.Length > 0 && v.Length > 0 && (IsOdd(u) || IsOdd(v)));

            ShiftRight(u, GetTrailingZeroBitCount(u));
            ShiftRight(v, GetTrailingZeroBitCount(v));

            u = u[..GetActualWordLength(u)];
            v = v[..GetActualWordLength(v)];

            Assert(u.Length > 0 && v.Length > 0 && IsOdd(u) && IsOdd(v));

            while (true)
            {
#if LOG_DETAIL
                System.Diagnostics.Debug.Write($"({ToFriendlyString(left)}, {ToFriendlyString(right)})=>");
#endif
                Assert(u.Length > 0 && v.Length > 0 && IsOdd(u) && IsOdd(v));

#if USE_GCD_UINT64_OVERLOAD_WITHIN_GCD_SPAN_IF_POSSIBLE
                if (Environment.Is64BitProcess)
                {
                    if (u.Length <= 2 && v.Length <= 2)
                    {
                        Assert(u.Length > 0);
                        Assert(v.Length > 0);

                        if (u.Length == 2)
                        {
                            if (v.Length == 2)
                            {
                                var gcd =
                                    GcdCore(
                                        ((ulong)u[1] << _BIT_COUNT_PER_UINT32) | u[0],
                                        ((ulong)v[1] << _BIT_COUNT_PER_UINT32) | v[0]);
                                Span<uint> resultBuffer = [(uint)gcd, (uint)(gcd >> _BIT_COUNT_PER_UINT32)];
                                ShiftLeft(resultBuffer, k, left);
#if LOG_DETAIL
                                System.Diagnostics.Debug.WriteLine($"{ToFriendlyString(left)}");
#endif
                                return;
                            }
                            else
                            {
                                var gcd =
                                    GcdCore(
                                        ((ulong)u[1] << _BIT_COUNT_PER_UINT32) | u[0],
                                        v[0]);
                                Span<uint> resultBuffer = [(uint)gcd, (uint)(gcd >> _BIT_COUNT_PER_UINT32)];
                                ShiftLeft(resultBuffer, k, left);
#if LOG_DETAIL
                                System.Diagnostics.Debug.WriteLine($"{ToFriendlyString(left)}");
#endif
                                return;
                            }
                        }
                        else
                        {
                            if (v.Length == 2)
                            {
                                var gcd =
                                    GcdCore(
                                        u[0],
                                        ((ulong)v[1] << _BIT_COUNT_PER_UINT32) | v[0]);
                                Span<uint> resultBuffer = [(uint)gcd, (uint)(gcd >> _BIT_COUNT_PER_UINT32)];
                                ShiftLeft(resultBuffer, k, left);
#if LOG_DETAIL
                                System.Diagnostics.Debug.WriteLine($"{ToFriendlyString(left)}");
#endif
                                return;
                            }
                            else
                            {
                                var gcd = GcdCore(u[0], v[0]);
                                Span<uint> resultBuffer = [gcd];
                                ShiftLeft(resultBuffer, k, left);
#if LOG_DETAIL
                                System.Diagnostics.Debug.WriteLine($"{ToFriendlyString(left)}");
#endif
                                return;
                            }
                        }
                    }
                }
                else
                {
                    if (u.Length == 1 && v.Length == 1)
                    {
                        var gcd = GcdCore(u[0], v[0]);
                        Span<uint> resultBuffer = [gcd];
                        ShiftLeft(resultBuffer, k, left);
#if LOG_DETAIL
                        System.Diagnostics.Debug.WriteLine($"{ToFriendlyString(left)}");
#endif
                        return;
                    }
                }
#elif USE_GCD_UINT32_OVERLOAD_WITHIN_GCD_SPAN_IF_POSSIBLE
                if (u.Length == 1 && v.Length == 1)
                {
                    var gcd = GcdCore(u[0], v[0]);
                    Span<uint> resultBuffer = [gcd];
                    ShiftLeft(resultBuffer, k, left);
#if LOG_DETAIL
                    System.Diagnostics.Debug.WriteLine($"{ToFriendlyString(left)}");
#endif
                    return;
                }
#endif

                var c = Compare(u, v);

                if (c == 0)
                {
                    // If "u == v" then "GCD(left, right) == u << k".

                    ShiftLeft(u, k, left);
#if LOG_DETAIL
                    System.Diagnostics.Debug.WriteLine($"{ToFriendlyString(left)}");
#endif
                    return;
                }

                if (c < 0)
                {
                    var t = u;
                    u = v;
                    v = t;
                }

                Assert(u.Length > 0 && v.Length > 0 && Compare(u, v) > 0 && IsOdd(u) && IsOdd(v) && !IsZeroMostSignificantWord(u) && !IsZeroMostSignificantWord(v));

                SubtractSelf(u, v);
                Assert(IsEven(u));
                u = u[..GetActualWordLength(u)];

                ShiftRight(u, GetTrailingZeroBitCount(u));
                u = u[..GetActualWordLength(u)];
            }
        }

        #endregion

        #region Utility methods

#if ANALYZE_PERFORMANCE
        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
#else
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
#endif
        private static int Compare(ReadOnlySpan<uint> left, ReadOnlySpan<uint> right)
        {
            Assert(left.Length <= right.Length || left[right.Length..].ContainsAnyExcept(0u));
            Assert(left.Length >= right.Length || right[left.Length..].ContainsAnyExcept(0u));

            if (left.Length != right.Length)
                return left.Length < right.Length ? -1 : 1;
            var index = left.Length;
            while (--index >= 0 && left[index] == right[index])
                ;
            if (index < 0)
                return 0;
            return left[index] < right[index] ? -1 : 1;
        }

#if ANALYZE_PERFORMANCE
        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
#else
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
#endif
        private static int GetActualWordLength(ReadOnlySpan<uint> value)
        {
            Assert(value.Length > 0);

            var index = value.Length;
            do
            {
                var element = value[--index];
                if (element != 0)
                    return index + 1;
            } while (index > 0);
            return 0;
        }

#if ANALYZE_PERFORMANCE
        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
#else
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
#endif
        private static int GetTrailingZeroBitCount(ReadOnlySpan<uint> value)
        {
            Assert(value.Length > 0);

            var index = 0;
            var count = value.Length;
            do
            {
                var element = value[index];
                if (element != 0)
                    return (index << _SHIFT_BIT_COUNT_PER_UINT32) + TrailingZeroCount(element);
                ++index;
            } while (--count > 0);

            return value.Length << _SHIFT_BIT_COUNT_PER_UINT32;
        }

        // equivalent to "result = source << bitCount"
#if ANALYZE_PERFORMANCE
        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
#else
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
#endif
        private static void ShiftLeft(ReadOnlySpan<uint> source, int bitCount, Span<uint> result)
        {
            var lengthOfSource = source.Length;

            Assert(lengthOfSource > 0);
            Assert(result.Length > 0);
            Assert(result.Length >= lengthOfSource);
            Assert(bitCount >= 0);
            Assert(bitCount <= result.Length * _BIT_COUNT_PER_UINT32);

            if (bitCount == 0)
            {
                source.CopyTo(result);
                result[lengthOfSource..].Clear();
            }
            else
            {
                var leftShiftCount = bitCount & _BIT_COUNT_MASK_FOR_UINT32;
                if (leftShiftCount == 0)
                {
                    var offset = bitCount >> _SHIFT_BIT_COUNT_PER_UINT32;

                    // result[result.Length - 1] = 0;
                    // result[result.Length - 2] = 0;
                    // ...
                    // result[source.Length + offset + 1] = 0;
                    // result[source.Length + offset] = 0;
                    // result[source.Length + offset - 1] = source[source.Length - 1];
                    // ...
                    // result[index] = (source[index - offset];
                    // ...
                    // result[offset + 1] = source[1];
                    // result[offset] = source[0];
                    // result[offset - 1] = 0;
                    // ...
                    // result[0] = 0;

                    Assert(lengthOfSource + offset <= result.Length);

                    result[(lengthOfSource + offset)..].Clear();
                    source.CopyTo(result.Slice(offset, lengthOfSource));
                    result[..offset].Clear();
                }
                else
                {
                    var rightShiftCount = _BIT_COUNT_PER_UINT32 - leftShiftCount;
                    var offset = bitCount >> _SHIFT_BIT_COUNT_PER_UINT32;
                    var offsetPlusOne = offset + 1;

                    Assert(leftShiftCount >= 0);
                    Assert(leftShiftCount < _BIT_COUNT_PER_UINT32);
                    Assert(rightShiftCount >= 0);
                    Assert(rightShiftCount < _BIT_COUNT_PER_UINT32);

                    // result[result.Length - 1] = 0;
                    // result[result.Length - 2] = 0;
                    // ...
                    // result[source.Length + offset + 1] = 0;
                    // result[source.Length + offset] = source[source.Length - 1] >> rightShiftCount;
                    // result[source.Length + offset - 1] = (source[source.Length - 1] << leftShiftCount) | (source[source.Length - 2] >> rightShiftCount);
                    // ...
                    // result[index] = (source[index - offset] << leftShiftCount) | (source[index - offset - 1] >> rightShiftCount);
                    // ...
                    // result[offset + 1] = (source[1] << leftShiftCount) | (source[0] >> rightShiftCount);
                    // result[offset] = source[0] << leftShiftCount;
                    // result[offset - 1] = 0;
                    // ...
                    // result[0] = 0;

                    Assert(lengthOfSource + offset <= result.Length);

                    if (lengthOfSource + offset < result.Length)
                    {
                        result[(lengthOfSource + offsetPlusOne)..].Clear();
                        result[lengthOfSource + offset] = source[^1] >> rightShiftCount;
                    }
                    else
                    {
                        Assert((source[^1] >> rightShiftCount) == 0);
                    }

                    for (var index = lengthOfSource - 2; index >= 0; --index)
                        result[index + offsetPlusOne] = (source[index + 1] << leftShiftCount) | (source[index] >> rightShiftCount);
                    result[offset] = source[0] << leftShiftCount;
                    result[..offset].Clear();
                }
            }
        }

        // equivalent to "value >>= bitCount"
#if ANALYZE_PERFORMANCE
        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
#else
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
#endif
        private static void ShiftRight(Span<uint> value, int bitCount)
        {
            Assert(value.Length > 0);
            Assert(bitCount >= 0);
            Assert(bitCount <= value.Length * _BIT_COUNT_PER_UINT32);

            if (bitCount == 0)
                return;

            var offset = bitCount >> _SHIFT_BIT_COUNT_PER_UINT32;
            var rightShiftCount = bitCount & _BIT_COUNT_MASK_FOR_UINT32;

            if (rightShiftCount == 0)
            {
                // value[0] = value[offset + 0];
                // value[1] = value[offset + 1];
                // ...
                // value[index] = value[index + offset];
                // ...
                // value[value.Length - offset - 2] = value[value.Length - 2];
                // value[value.Length - offset - 1] = value[value.Length - 1];
                // value[value.Length - offset] = 0;
                // ...
                // value[value.Length - 1] = 0;

                Assert(offset <= value.Length);

                value[offset..].CopyTo(value[..^offset]);
                value[^offset..].Clear();
            }
            else
            {
                var leftShiftCount = _BIT_COUNT_PER_UINT32 - rightShiftCount;

                Assert(offset < value.Length);
                Assert(leftShiftCount >= 0);
                Assert(leftShiftCount < _BIT_COUNT_PER_UINT32);
                Assert(rightShiftCount >= 0);
                Assert(rightShiftCount < _BIT_COUNT_PER_UINT32);

                // value[0] = (value[offset + 0] >> rightShiftCount) | (value[offset + 1] << leftShiftCount);
                // value[1] = (value[offset + 1] >> rightShiftCount) | (value[offset + 2] << leftShiftCount);
                // ...
                // value[index] = (value[index + offset] >> rightShiftCount) | (value[index + offset + 1] << leftShiftCount);
                // ...
                // value[value.Length - offset - 2] = (value[value.Length - 2] >> rightShiftCount) | (value[value.Length - 1] << leftShiftCount);
                // value[value.Length - offset - 1] = value[value.Length - 1] >> rightShiftCount;
                // value[value.Length - offset] = 0;
                // ...
                // value[value.Length - 1] = 0;

                var limit = value.Length - offset - 1;
                for (var index = 0; index < limit; ++index)
                    value[index] = (value[index + offset] >> rightShiftCount) | (value[index + offset + 1] << leftShiftCount);
                value[limit] = value[^1] >> rightShiftCount;
                value[^offset..].Clear();
            }
        }

        // equivalent to "left -= right << (wordCountOfShift * 32)"
#if ANALYZE_PERFORMANCE
        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
#else
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
#endif
        private static void SubtractSelf(Span<uint> left, ReadOnlySpan<uint> right)
        {
            Assert(left.Length > 0);
            Assert(right.Length > 0);
            Assert(left.Length >= right.Length);

            var borrow = 0L;
            var index = 0;
            var limit = right.Length;
            do
            {
                borrow += (long)left[index] - right[index];
                left[index++] = unchecked((uint)borrow);
                borrow >>= _BIT_COUNT_PER_UINT32;
                Assert(borrow is 0 or (-1));
            } while (index < limit);

            Assert(borrow is 0 or (-1));

            if (borrow == 0)
                return;

            Assert(index < left.Length);

            do
            {
                borrow += left[index];
                left[index] = unchecked((uint)borrow);
                borrow >>= _BIT_COUNT_PER_UINT32;
                Assert(borrow is 0 or (-1));
            } while (index < left.Length && borrow < 0);

            Assert(borrow == 0);
        }

#if ANALYZE_PERFORMANCE
        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
#else
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
#endif
        private static bool IsEven(ReadOnlySpan<uint> value)
        {
            Assert(value.Length > 0);

            return (value[0] & 1) == 0;
        }

#if ANALYZE_PERFORMANCE
        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
#else
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
#endif
        private static bool IsOdd(ReadOnlySpan<uint> value)
        {
            Assert(value.Length > 0);

            return (value[0] & 1) != 0;
        }

#if ANALYZE_PERFORMANCE
        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
#else
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
#endif
        private static int TrailingZeroCount(uint value)
        {
            Assert(value != 0); // Do not call TrailingZeroCount() for 0 because the return value of BitOperations.TrailingZeroCount() for 0 may be undefined on some processors.

            return BitOperations.TrailingZeroCount(value);
        }

#if ANALYZE_PERFORMANCE
        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
#else
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
#endif
        private static int Min(int left, int right) => left < right ? left : right;

#if ANALYZE_PERFORMANCE
        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
#else
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
#endif
        private static bool IsZeroMostSignificantWord(ReadOnlySpan<uint> value)
        {
            Assert(value.Length > 0);

            return value[^1] == 0u;
        }

        #endregion

#if LOG_DETAIL
        private static string ToFriendlyString(ReadOnlySpan<uint> value)
        {
            var result = BigInteger.Zero;
            for (var index = value.Length - 1; index >= 0; --index)
            {
                result <<= _BIT_COUNT_PER_UINT32;
                result |= value[index];
            }

            return result.ToString(System.Globalization.CultureInfo.InvariantCulture);
        }
#endif
    }
}
