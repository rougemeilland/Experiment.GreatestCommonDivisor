//#define LOG_DETAIL
//#define RECORD_STATISTICS // 統計情報を記録する
#define CAN_GET_CONFIGURATION // 定義済みシンボル情報を取得可能にする
#define USE_GCD_UINT32_OVERLOAD_WITHIN_GCD_GCD_UINT64_IF_POSSIBLE // GcdCore(ulong, ulong) のオーバーロードにおいて、可能であれば Gcd(uint, uint) を呼び出す
#define USE_GCD_UINT32_OVERLOAD_WITHIN_GCD_SPAN_IF_POSSIBLE // GcdCore(Span<uint>, ReadOnlySpan<uint>) のオーバーロードにおいて、可能であれば Gcd(uint, uint) を呼び出す
#define USE_GCD_UINT64_OVERLOAD_WITHIN_GCD_SPAN_IF_POSSIBLE // GcdCore(Span<uint>, ReadOnlySpan<uint>) のオーバーロードにおいて、可能であれば Gcd(ulong, ulong) を呼び出す
#define USE_UNSAFE_CODE_AT_SHIFT_RIGHT // ShiftRight(Span<uint> value, int bitCount) において、unsafe コードを使用する
//#define USE_UNSAFE_CODE_AT_SUBTRACT_SELF // SubtractSelf(Span<uint> left, ReadOnlySpan<uint> right) において、unsafe コードを使用する

using System;
using System.Buffers;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace Experiment.CUI
{
    internal static partial class BigIntegerCalculatorVer2_1
    {
        [Flags]
        private enum HardwareAcceleratorOption
        {
            ByDefault = UseVector128IfPossible | UseVector256IfPossible | UseVector512IfPossible,
            None = 0b0,
            UseVector128IfPossible = 0b1,
            UseVector256IfPossible = 0b10,
            UseVector512IfPossible = 0b100,
        }

        private sealed class GcdBitCountsKey
            : IEquatable<GcdBitCountsKey>
        {
            public GcdBitCountsKey(int leftBitCount, int rightBitCount)
            {
                if (leftBitCount >= rightBitCount)
                {
                    LeftBitCount = leftBitCount;
                    RightBitCount = rightBitCount;
                }
                else
                {
                    LeftBitCount = rightBitCount;
                    RightBitCount = leftBitCount;
                }
            }

            public int LeftBitCount { get; }
            public int RightBitCount { get; }

            public bool Equals(GcdBitCountsKey? other)
                => other is not null && LeftBitCount == other.LeftBitCount && RightBitCount == other.RightBitCount;

            public override bool Equals(object? other)
                => other is not null && GetType() == other.GetType() && Equals((GcdBitCountsKey)other);

            public override int GetHashCode()
                => HashCode.Combine(LeftBitCount, RightBitCount);
        }

        private const int _BIT_COUNT_PER_UINT32 = 32;
        private const int _BIT_COUNT_MASK_FOR_UINT32 = 31;
        private const int _SHIFT_BIT_COUNT_PER_UINT32 = 5;
        private const int _STACKALLOC_THRESHOLD = 64;
        private static readonly Dictionary<GcdBitCountsKey, ulong> _statistics = [];

        static BigIntegerCalculatorVer2_1()
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

        public static void ClearStatistics() => _statistics.Clear();

        public static IEnumerable<(int leftBitCount, int rightBitCount, ulong count)> EnumerateStatistics()
        {
            foreach (var item in _statistics)
                yield return (item.Key.LeftBitCount, item.Key.RightBitCount, item.Value);
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.AggressiveOptimization)]
        public static void TryToShiftRight(int type, Span<uint> buffer, int bitCount)
        {
            switch (type)
            {
                case 0:
                    ShiftRightTry0(buffer, bitCount);
                    break;
                case 1:
                    ShiftRightTry1(buffer, bitCount);
                    break;
                case 2:
                    ShiftRightTry2(buffer, bitCount, HardwareAcceleratorOption.ByDefault);
                    break;
                case 3:
                    ShiftRightTry2(buffer, bitCount, HardwareAcceleratorOption.None);
                    break;
                case 4:
                    ShiftRightTry2(buffer, bitCount, HardwareAcceleratorOption.UseVector128IfPossible);
                    break;
                case 5:
                    ShiftRightTry2(buffer, bitCount, HardwareAcceleratorOption.UseVector256IfPossible);
                    break;
                case 6:
                    ShiftRightTry2(buffer, bitCount, HardwareAcceleratorOption.UseVector512IfPossible);
                    break;
                default:
                    throw new Exception();
            }
        }

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

        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
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

        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
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

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        private static uint GcdCore(uint left, uint right)
        {
            Assert(left > 0 && right > 0);

            var trailingZeroCountOfLeft = BitOperations.TrailingZeroCount(left);
            var trailingZeroCountOfRight = BitOperations.TrailingZeroCount(right);
            left >>= trailingZeroCountOfLeft;
            right >>= trailingZeroCountOfRight;
            var k = Min(trailingZeroCountOfLeft, trailingZeroCountOfRight);

            Assert(left > 0 && right > 0 && uint.IsOddInteger(left) && uint.IsOddInteger(right));

            return GcdCoreForOddInteger(left, right) << k;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        private static ulong GcdCore(ulong left, ulong right)
        {
            Assert(left > 0 && right > 0);

            var trailingZeroCountOfLeft = BitOperations.TrailingZeroCount(left);
            var trailingZeroCountOfRight = BitOperations.TrailingZeroCount(right);
            left >>= trailingZeroCountOfLeft;
            right >>= trailingZeroCountOfRight;
            var k = Min(trailingZeroCountOfLeft, trailingZeroCountOfRight);

            Assert(left > 0 && right > 0 && ulong.IsOddInteger(left) && ulong.IsOddInteger(right));

            return GcdCoreForOddInteger(left, right) << k;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        private static void GcdCore(Span<uint> left, Span<uint> right)
        {
            // Remember that the variable 'left' must hold the greatest common divisor (GCD) when the function returns.

            Assert(left.Length > 0 && right.Length > 0);
            Assert(left.Length >= 2 || right.Length >= 2);
            Assert(left.Length >= right.Length);
            Assert(Compare(left, right) >= 0);

            var result = left;

            var trailingZeroCountOfLeft = GetTrailingZeroBitCount(left);
            var trailingZeroCountOfRight = GetTrailingZeroBitCount(right);
            var k = Math.Min(trailingZeroCountOfLeft, trailingZeroCountOfRight);

            ShiftRight(left, trailingZeroCountOfLeft);
            ShiftRight(right, trailingZeroCountOfRight);

            left = left[..GetActualWordLength(left)];
            right = right[..GetActualWordLength(right)];

            Assert(left.Length > 0 && right.Length > 0 && IsOdd(left) && IsOdd(right));

            GcdCoreForOddInteger(left, right, k, result);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        private static uint GcdCoreForOddInteger(uint u, uint v)
        {
            while (true)
            {
#if LOG_DETAIL
                System.Diagnostics.Debug.Write($"({u}, {v})=>");
#endif
                Assert(u > 0 && v > 0 && uint.IsOddInteger(u) && uint.IsOddInteger(v));

                if (u == v)
                {
#if LOG_DETAIL
                    System.Diagnostics.Debug.WriteLine($"{u}");
#endif
                    return u;
                }

#if RECORD_STATISTICS
                AddToStatistics(GetActualBitCount(left), GetActualBitCount(right));
#endif

                if (u < v)
                    (v, u) = (u, v);

                Assert(u > 0 && v > 0 && u > v && uint.IsOddInteger(u) && uint.IsOddInteger(v));

                u -= v;

                Assert(uint.IsEvenInteger(u));

                u >>= BitOperations.TrailingZeroCount(u);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        private static ulong GcdCoreForOddInteger(ulong u, ulong v)
        {
            while (true)
            {
#if LOG_DETAIL
                System.Diagnostics.Debug.Write($"({u}, {b})=>");
#endif
                Assert(u > 0 && v > 0 && ulong.IsOddInteger(u) && ulong.IsOddInteger(v));

#if USE_GCD_UINT32_OVERLOAD_WITHIN_GCD_GCD_UINT64_IF_POSSIBLE
                if (u <= uint.MaxValue && v <= uint.MaxValue)
                    return GcdCoreForOddInteger((uint)u, (uint)v);
#endif
                if (u == v)
                {
#if LOG_DETAIL
                    System.Diagnostics.Debug.WriteLine($"{u}");
#endif
                    return u;
                }

#if RECORD_STATISTICS
                AddToStatistics(GetActualBitCount(left), GetActualBitCount(right));
#endif

                if (u < v)
                    (v, u) = (u, v);

                Assert(u > 0 && v > 0 && u > v && ulong.IsOddInteger(u) && ulong.IsOddInteger(v));

                u -= v;

                Assert(ulong.IsEvenInteger(u));

                u >>= BitOperations.TrailingZeroCount(u);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        private static void GcdCoreForOddInteger(Span<uint> u, Span<uint> v, int k, Span<uint> r)
        {
            // Note that either u or v may be memory shared with r.

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
                                    GcdCoreForOddInteger(
                                        ((ulong)u[1] << _BIT_COUNT_PER_UINT32) | u[0],
                                        ((ulong)v[1] << _BIT_COUNT_PER_UINT32) | v[0]);
                                Span<uint> resultBuffer = [(uint)gcd, (uint)(gcd >> _BIT_COUNT_PER_UINT32)];
                                ShiftLeft(resultBuffer, k, r);
#if LOG_DETAIL
                                System.Diagnostics.Debug.WriteLine($"{ToFriendlyString(r)}");
#endif
                                return;
                            }
                            else
                            {
                                var gcd =
                                    GcdCoreForOddInteger(
                                        ((ulong)u[1] << _BIT_COUNT_PER_UINT32) | u[0],
                                        v[0]);
                                Assert(gcd <= uint.MaxValue);
                                Span<uint> resultBuffer = [(uint)gcd];
                                ShiftLeft(resultBuffer, k, r);
#if LOG_DETAIL
                                System.Diagnostics.Debug.WriteLine($"{ToFriendlyString(r)}");
#endif
                                return;
                            }
                        }
                        else
                        {
                            if (v.Length == 2)
                            {
                                var gcd =
                                    GcdCoreForOddInteger(
                                        u[0],
                                        ((ulong)v[1] << _BIT_COUNT_PER_UINT32) | v[0]);
                                Assert(gcd <= uint.MaxValue);
                                Span<uint> resultBuffer = [(uint)gcd];
                                ShiftLeft(resultBuffer, k, r);
#if LOG_DETAIL
                                System.Diagnostics.Debug.WriteLine($"{ToFriendlyString(r)}");
#endif
                                return;
                            }
                            else
                            {
                                var gcd = GcdCoreForOddInteger(u[0], v[0]);
                                Span<uint> resultBuffer = [gcd];
                                ShiftLeft(resultBuffer, k, r);
#if LOG_DETAIL
                                System.Diagnostics.Debug.WriteLine($"{ToFriendlyString(r)}");
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
                        var gcd = GcdCoreForOddInteger(u[0], v[0]);
                        Span<uint> resultBuffer = [gcd];
                        ShiftLeft(resultBuffer, k, r);
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

                    ShiftLeft(u, k, r);
#if LOG_DETAIL
                    System.Diagnostics.Debug.WriteLine($"{ToFriendlyString(r)}");
#endif
                    return;
                }

#if RECORD_STATISTICS
                AddToStatistics(GetActualBitCount(left), GetActualBitCount(right));
#endif

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

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
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

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
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

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        private static int GetActualBitCount(uint value)
        {
            Assert(value > 0);
            return _BIT_COUNT_MASK_FOR_UINT32 - BitOperations.LeadingZeroCount(value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        private static int GetActualBitCount(ulong value)
        {
            Assert(value > 0);
            return _BIT_COUNT_MASK_FOR_UINT32 - BitOperations.LeadingZeroCount(value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        private static int GetActualBitCount(ReadOnlySpan<uint> value)
        {
            Assert(value.Length > 0);

            var index = value.Length;
            do
            {
                --index;
                var element = value[index];
                if (element != 0)
                    return ((index + 1) << _SHIFT_BIT_COUNT_PER_UINT32) - BitOperations.LeadingZeroCount(element);

            } while (index > 0);

            return 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
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
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        private static void ShiftLeft(ReadOnlySpan<uint> source, int bitCount, Span<uint> result)
        {
            Assert(source.Length > 0);
            Assert(result.Length > 0);
            Assert(result.Length >= source.Length);
            Assert(bitCount >= 0);
            Assert(bitCount <= result.Length * _BIT_COUNT_PER_UINT32);

            if (bitCount == 0)
            {
                source.CopyTo(result);
                result[source.Length..].Clear();
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

                    Assert(source.Length + offset <= result.Length);

                    result[(source.Length + offset)..].Clear();
                    source.CopyTo(result[offset..]);
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

                    Assert(source.Length + offset <= result.Length);

                    if (source.Length + offset < result.Length)
                    {
                        result[(source.Length + offsetPlusOne)..].Clear();
                        result[source.Length + offset] = source[^1] >> rightShiftCount;
                    }
                    else
                    {
                        Assert((source[^1] >> rightShiftCount) == 0);
                    }

                    for (var index = source.Length - 2; index >= 0; --index)
                        result[index + offsetPlusOne] = (source[index + 1] << leftShiftCount) | (source[index] >> rightShiftCount);
                    result[offset] = source[0] << leftShiftCount;
                    result[..offset].Clear();
                }
            }
        }

        // equivalent to "value >>= bitCount"
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        private static void ShiftRight(Span<uint> value, int bitCount)
        {
#if DEBUG
            var v = value.ToBigInteger();
            if (v.Sign == 0)
                throw new Exception();
            var vs = value.ToFormattedString();
#endif
            if (value.Length < 4) // < 128bit
                ShiftRightTry1(value, bitCount);
            else if (value.Length < 5) // < 160bit
                ShiftRightTry2(value, bitCount, HardwareAcceleratorOption.None);
            else if (value.Length < 8) // < 256bit
                ShiftRightTry2(value, bitCount, HardwareAcceleratorOption.UseVector128IfPossible);
            else if (value.Length < 16) // < 512bit
                ShiftRightTry2(value, bitCount, HardwareAcceleratorOption.UseVector128IfPossible | HardwareAcceleratorOption.UseVector256IfPossible);
            else
                ShiftRightTry2(value, bitCount, HardwareAcceleratorOption.UseVector128IfPossible | HardwareAcceleratorOption.UseVector256IfPossible | HardwareAcceleratorOption.UseVector512IfPossible);
#if DEBUG
            var r = value.ToBigInteger();
            if (r.Sign == 0)
                throw new Exception();
            var rs = value.ToFormattedString();
            if (r != (v >> bitCount))
                throw new Exception($"ShiftRight({vs}, {bitCount}) => {rs}");
#endif
        }

        // equivalent to "left -= right << (wordCountOfShift * 32)"
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        private static void SubtractSelf(Span<uint> left, ReadOnlySpan<uint> right)
            => SubtractSelfTry0(left, right);

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        private static bool IsEven(ReadOnlySpan<uint> value)
        {
            Assert(value.Length > 0);

            return (value[0] & 1) == 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        private static bool IsOdd(ReadOnlySpan<uint> value)
        {
            Assert(value.Length > 0);

            return (value[0] & 1) != 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        private static int TrailingZeroCount(uint value)
        {
            Assert(value != 0); // Do not call TrailingZeroCount() for 0 because the return value of BitOperations.TrailingZeroCount() for 0 may be undefined on some processors.

            return BitOperations.TrailingZeroCount(value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        private static int Min(int left, int right) => left < right ? left : right;

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
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

#if RECORD_STATISTICS
        public static void AddToStatistics(int leftBitCount, int rightBitCount)
        {
            var key = new GcdBitCountsKey(leftBitCount, rightBitCount);
            if (!_statistics.TryGetValue(key, out var value))
                value = 0;
            _statistics[key] = value + 1;
        }
#endif

        // equivalent to "value >>= bitCount"
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        private static void ShiftRightTry0(Span<uint> value, int bitCount)
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

                for (var index = 0; index < value.Length - offset - 1; ++index)
                    value[index] = (value[index + offset] >> rightShiftCount) | (value[index + offset + 1] << leftShiftCount);
                value[^(offset + 1)] = value[^1] >> rightShiftCount;
                value[^offset..].Clear();
            }
        }

        // equivalent to "left -= right << (wordCountOfShift * 32)"
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        private static void SubtractSelfTry0(Span<uint> left, ReadOnlySpan<uint> right)
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
    }
}
