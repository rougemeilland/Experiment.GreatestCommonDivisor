using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace Experiment.CUI
{
    internal static partial class BigIntegerCalculatorVer2_1
    {
        private static class Test
        {
            public static void TestCompare()
            {
                var listOfWidth = new[] { 32, 64, 96, 128 };
                var listOfLeadingBit = new[] { "", "1", $"{new string('0', 1)}1", $"{new string('0', 2)}1", $"{new string('0', 29)}1", $"{new string('0', 30)}1", $"{new string('0', 31)}1", $"{new string('0', 32)}1", $"{new string('0', 33)}1" };
                var listOfOffset = new[] { 0, 1, 2 };
                var bitPattern = string.Concat(Enumerable.Repeat("011", 342))[..1024];
                var zero32bitValue = new string('0', 32);
                var listOfValue =
                    listOfWidth
                    .Select(width => bitPattern[..width])
                    .SelectMany(value => listOfLeadingBit.Where(leadingBit => leadingBit.Length < value.Length), (value, leadingBit) => $"{leadingBit}{value[leadingBit.Length..]}")
                    .Distinct()
                    .Where(value =>
                    {
                        Assert(value.Length >= 32);
                        Assert(value.Length % 32 == 0);
                        return value[..32] != zero32bitValue;
                    })
                    .ToArray();
                var testSet =
                    listOfValue
                    .SelectMany(left => listOfValue, (left, right) => (left, right))
                    .ToArray();

                foreach (var (left, right) in testSet)
                {
                    var leftArray = ToArrayOfUInt32(left).Span;
                    var rightArray = ToArrayOfUInt32(right).Span;
                    var expected = GetExpected(left, right);
                    var actual = Compare(leftArray, rightArray);
                    if (!AreEqualSign(actual, expected))
                        throw new Exception($"Test is failed.: Compare({FormatArray(leftArray)}, {FormatArray(rightArray)}), actual : {actual} expected : {expected}");
                }

                static int GetExpected(string left, string right)
                {
                    var leftArray = ToBigInteger(ToArrayOfUInt32(left).Span);
                    var rightArray = ToBigInteger(ToArrayOfUInt32(right).Span);
                    return leftArray.CompareTo(rightArray);
                }
            }

            public static void TestGetActualWordLength()
            {
                var listOfLeadingZeroBitCount = new[] { 0, 1, 30, 31, 32, 33, 62, 63, 64, 65 };
                var listOfMiddleZeroBitCount = new[] { 0, 1, 30, 31, 32, 33, 62, 63, 64, 65 };
                var listOfTralingZeroBitCount = new[] { 0, 1, 30, 31, 32, 33, 62, 63, 64, 65 };

                var testSet =
                    listOfLeadingZeroBitCount
                    .SelectMany(leadingZeroBitCount => listOfMiddleZeroBitCount.Select(middleZeroBitCount => (leadingZeroBitCount, middleZeroBitCount)))
                    .SelectMany(item => listOfTralingZeroBitCount.Select(tralingZeroBitCount => (value: $"{new string('0', item.leadingZeroBitCount)}1{new string('0', item.middleZeroBitCount)}1{new string('0', tralingZeroBitCount)}", bitLength: 1 + item.middleZeroBitCount + 1 + tralingZeroBitCount)))
                    .ToArray();
                foreach (var (value, bitLength) in testSet)
                {
                    var valueArray = ToArrayOfUInt32(value).Span;
                    var expected = (bitLength + _BIT_COUNT_PER_UINT32 - 1) / _BIT_COUNT_PER_UINT32;
                    var actual = GetActualWordLength(valueArray);
                    if (actual != expected)
                        throw new Exception($"Test is failed.: GetActualWordLength({FormatArray(valueArray)}), actual : {actual} expected : {expected}");
                }
            }

            public static void TestGetTrailingZeroBitCount()
            {
                var listOfLeadingZeroBitCount = new[] { 0, 1, 30, 31, 32, 33, 62, 63, 64, 65 };
                var listOfMiddleZeroBitCount = new[] { 0, 1, 30, 31, 32, 33, 62, 63, 64, 65 };
                var listOfTralingZeroBitCount = new[] { 0, 1, 30, 31, 32, 33, 62, 63, 64, 65 };

                var testSet =
                    listOfLeadingZeroBitCount
                    .SelectMany(leadingZeroBitCount => listOfMiddleZeroBitCount.Select(middleZeroBitCount => (leadingZeroBitCount, middleZeroBitCount)))
                    .SelectMany(item => listOfTralingZeroBitCount.Select(tralingZeroBitCount => (value:$"{new string('0', item.leadingZeroBitCount)}1{new string('0', item.middleZeroBitCount)}1{new string('0', tralingZeroBitCount)}", tralingZeroBitCount)))
                    .ToArray();
                foreach (var (value, tralingZeroBitCount) in testSet)
                {
                    var valueArray = ToArrayOfUInt32(value).Span;
                    var expected = tralingZeroBitCount;
                    var actual = GetTrailingZeroBitCount(valueArray);
                    if (actual != expected)
                        throw new Exception($"Test is failed.: GetTrailingZeroBitCount({FormatArray(valueArray)}), actual : {actual} expected : {expected}");
                }
            }

            public static void TestShiftLeft()
            {
                var listOfBitCount = new[] { 0, 1, 31, 32, 33, 63, 64, 54 };
                var listOfWidth = new[] { 32, 64, 96 };
                var listOfLeadingBit = new[] { "", "0", "1", "11", "111111111111111111111111111111", "1111111111111111111111111111111", "11111111111111111111111111111111", "111111111111111111111111111111111" };
                var listOfResultLength = new int[] { 1, 2, 3, 4, 5 };

                var bitPattern = string.Concat(Enumerable.Repeat("011", 342))[..1024];
                var testSet =
                    listOfBitCount
                    .SelectMany(bitCount => listOfWidth, (bitcount, width) => (value: bitPattern[..width], bitcount))
                    .SelectMany(item => listOfLeadingBit, (item, leadingBit) => (value: $"{leadingBit}{item.value}", item.bitcount))
                    .SelectMany(item => listOfResultLength, (item, lengthOfResult) => (item.value, item.bitcount, lengthOfResult))
                    .Where(item => Trim(item.value).Length + item.bitcount <= item.lengthOfResult)
                    .ToArray();

                foreach (var (value, bitcount, lengthOfResult) in testSet)
                {
                    var valueArray = ToArrayOfUInt32(value).Span;
                    var expected = GetExpected(value, bitcount, lengthOfResult).Span;
                    var actual = new uint[lengthOfResult];
                    ShiftLeft(valueArray, bitcount, actual);
                    if (!AreEqual(actual, expected))
                        throw new Exception($"Test is failed.: ShiftLeft({FormatArray(valueArray)}, {bitcount}), actual : {FormatArray(actual)} expected : {FormatArray(expected)}");
                }

                static ReadOnlyMemory<uint> GetExpected(string value, int bitCount, int lengthOfResult)
                {
                    var s = $"{value}{new string('0', bitCount)}";
                    return ToArrayOfUInt32($"{new string('0', lengthOfResult * 32 - s.Length)}{s}");
                }
            }

            public static void TestShiftRight()
            {
                var listOfBitCount = new[] { 0, 1, 31, 32, 33, 63, 64, 54 };
                var listOfWidth = new[] { 32, 64, 96 };
                var listOfLeadingBit = new[] { "", "1", $"{new string('0', 1)}1", $"{new string('0', 2)}1", $"{new string('0', 29)}1", $"{new string('0', 30)}1", $"{new string('0', 31)}1", $"{new string('0', 32)}1", $"{new string('0', 33)}1" };

                var bitPattern = string.Concat(Enumerable.Repeat("011", 342))[..1024];
                var testSet =
                    listOfBitCount
                    .SelectMany(bitCount => listOfWidth, (bitcount, width) => (value: bitPattern[..width], bitcount))
                    .SelectMany(item => listOfLeadingBit.Where(leadingBit => leadingBit.Length < item.value.Length), (item, leadingBit) => (value: $"{leadingBit}{item.value[leadingBit.Length..]}", item.bitcount))
                    .Where(item => item.bitcount <= item.value.Length)
                    .ToArray();

                foreach (var (value, bitcount) in testSet)
                {
                    var valueArray = ToArrayOfUInt32(value).Span;
                    var expected = GetExpected(value, bitcount).Span;
                    var actual = new uint[expected.Length].AsSpan();
                    actual.Clear();
                    valueArray.CopyTo(actual);
                    ShiftRight(actual, bitcount);
                    if (!AreEqual(actual, expected))
                        throw new Exception($"Test is failed.: ShiftRight({FormatArray(valueArray)}, {bitcount}), actual : {FormatArray(actual)} expected : {FormatArray(expected)}");
                }

                static ReadOnlyMemory<uint> GetExpected(string value, int bitCount)
                {
                    Assert(value.Length % 32 == 0);
                    var s = bitCount <= value.Length ? value[..^bitCount] : "";
                    return ToArrayOfUInt32($"{new string('0', value.Length - s.Length)}{s}");
                }
            }

            public static void TestIsEven()
            {
                var testSet = new[]
                {
                    (value: "0", expected: true),
                    (value: "1", expected: false),
                    (value: "110", expected: true),
                    (value: "111", expected: false),
                    (value: "0_110", expected: true),
                    (value: "0_111", expected: false),
                    (value: "1_110", expected: true),
                    (value: "1_111", expected: false),
                };
                foreach (var (value, expected) in testSet)
                {
                    var valueArray = ToArrayOfUInt32(value).Span;
                    var actual = IsEven(valueArray);
                    if (actual != expected)
                        throw new Exception($"Test is failed.: IsEven({FormatArray(valueArray)}), actual : {actual} expected : {expected}");
                }
            }

            public static void TestIsOdd()
            {
                var testSet = new[]
                {
                    (value: "0", expected: false),
                    (value: "1", expected: true),
                    (value: "110", expected: false),
                    (value: "111", expected: true),
                    (value: "0_110", expected: false),
                    (value: "0_111", expected: true),
                    (value: "1_110", expected: false),
                    (value: "1_111", expected: true),
                };
                foreach (var (value, expected) in testSet)
                {
                    var valueArray = ToArrayOfUInt32(value).Span;
                    var actual = IsOdd(valueArray);
                    if (actual != expected)
                        throw new Exception($"Test is failed.: IsOdd({FormatArray(valueArray)}), actual : {actual} expected : {expected}");
                }
            }

            private static BigInteger ToBigInteger(ReadOnlySpan<uint> value)
            {
                var result = BigInteger.Zero;
                for (var index = value.Length - 1; index >= 0; --index)
                {
                    result <<= 32;
                    result |= value[index];
                }

                return result;
            }

            private static ReadOnlyMemory<uint> ToArrayOfUInt32(BigInteger value)
            {
                if (value.Sign < 0)
                    throw new Exception();
                var result = new List<uint>();
                while (value.Sign != 0)
                {
                    result.Add((uint)(value & uint.MaxValue));
                    value >>= 32;
                }

                return result.ToArray();
            }

            private static ReadOnlyMemory<uint> ToArrayOfUInt32(string s)
            {
                if (s.Contains("__", StringComparison.Ordinal))
                    throw new Exception();
                if (s.Contains("--", StringComparison.Ordinal))
                    throw new Exception();
                if (s.Contains("-_", StringComparison.Ordinal))
                    throw new Exception();
                if (s.Contains("_-", StringComparison.Ordinal))
                    throw new Exception();
                if (s.StartsWith('_'))
                    throw new Exception();
                if (s.StartsWith('-'))
                    throw new Exception();
                if (s.EndsWith('_'))
                    throw new Exception();
                if (s.EndsWith('-'))
                    throw new Exception();

                s = s.Replace("-", "");
                if (s.Contains('_'))
                {
                    return
                        s
                        .Split('_')
                        .Select(segment =>
                        {
                            if (segment.Length > 32)
                                throw new Exception();
                            return Convert.ToUInt32(segment, 2);
                        })
                        .Reverse()
                        .ToArray();
                }
                else
                {
                    var buffer = new List<uint>();
                    while (s.Length > 0)
                    {
                        if (s.Length >= 32)
                        {
                            buffer.Add(Convert.ToUInt32(s[^32..], 2));
                            s = s[..^32];
                        }
                        else
                        {
                            buffer.Add(Convert.ToUInt32(s, 2));
                            s = "";
                        }
                    }

                    return buffer.ToArray();
                }
            }

            private static string ToRawString(string s) => s.Replace("_", "").Replace("-", "");

            private static string Trim(string s)
            {
                s = ToRawString(s);
                while (s.StartsWith('0'))
                    s = s[1..];
                return s;
            }

            private static bool AreEqual(ReadOnlySpan<uint> actual, ReadOnlySpan<uint> expected)
            {
                if (actual.Length != expected.Length)
                    return false;

                for (var index = 0; index < actual.Length; ++index)
                {
                    if (expected[index] != actual[index])
                        return false;
                }

                return true;
            }

            private static bool AreEqualSign(int actual, int expected)
            {
                if (actual > 0)
                    return expected > 0;
                else if (actual < 0)
                    return expected < 0;
                else
                    return expected == 0;
            }

            private static string FormatArray(ReadOnlySpan<uint> value)
            {
                var valueTexts = new List<string>();
                for (var index = 0; index < value.Length; ++index)
                    valueTexts.Add($"0x{value[index]:x8}");
                return $"[{string.Join(", ", valueTexts)}]";
            }
        }

        public static void TestPrivateMethods()
        {
            Test.TestCompare();
            Test.TestGetActualWordLength();
            Test.TestGetTrailingZeroBitCount();
            Test.TestShiftLeft();
            Test.TestShiftRight();
            Test.TestIsEven();
            Test.TestIsOdd();
        }
    }
}
