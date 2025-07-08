using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Runtime.CompilerServices;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Experiment.CUI
{
    internal static class TestBigIntegerCalculator
    {
        public static void Test()
        {
            BigIntegerCalculatorVer2.TestPrivateMethods();
            BigIntegerCalculatorVer2_1.TestPrivateMethods();

            var dataSet = File.ReadAllLines(Path.Combine(AppContext.BaseDirectory, "TestData", "test-data-for-unit-test.txt")).Select(text => BigInteger.Parse(text, NumberStyles.None, CultureInfo.InvariantCulture)).OrderByDescending(value => value).Select(value => value.ToUInt32Array()).ToArray();
            for (var index = 0; index < dataSet.Length; ++index)
            {
                var data = dataSet[index];
                if (data.Length > 0 && data.Span[^1] == 0)
                    throw new Exception();
                System.Diagnostics.Debug.WriteLine($"dataSet[{index}]={data.Span.ToFormattedString()}");
            }

            Span<uint> resultBuffer1 = stackalloc uint[dataSet.Max(value => value.Length)];
            Span<uint> resultBuffer2 = stackalloc uint[dataSet.Max(value => value.Length)];
            Span<uint> resultBuffer3 = stackalloc uint[dataSet.Max(value => value.Length)];

            var totalCount = (long)dataSet.Length * (dataSet.Length - 1) / 2;
            var progressCount = 0L;
            var percentageText = $"{(double)progressCount / totalCount:p2}";
            Console.Write($"Test progress: {percentageText}        \r");

            for (var index1 = 0; index1 < dataSet.Length - 1; ++index1)
            {
                for (var index2 = index1 + 1; index2 < dataSet.Length; ++index2)
                {
                    var left = dataSet[index1];
                    var right = dataSet[index2];
                    if (left.Length > 0 && right.Length > 0)
                    {
                        if (left.Span[^1] == 0)
                            throw new Exception();
                        if (right.Span[^1] == 0)
                            throw new Exception();
                        var expected = resultBuffer1[..CalculateGcdByDotNet(left.Span, right.Span, resultBuffer1)];
                        var actual1 = resultBuffer2[..CalculateGcdByVer2(left.Span, right.Span, resultBuffer2)];
                        if (!actual1.DataEquals(expected))
                            throw new Exception($"GCD({left.Span.ToFormattedString()}, {right.Span.ToFormattedString()}), actual1 = {actual1.ToFormattedString()}, expected = {expected.ToFormattedString()}");
                        var actual2 = resultBuffer3[..CalculateGcdByVer2_1(left.Span, right.Span, resultBuffer3)];
                        if (!actual2.DataEquals(expected))
                            throw new Exception($"GCD({left.Span.ToFormattedString()}, {right.Span.ToFormattedString()}), actual2 = {actual2.ToFormattedString()}, expected = {expected.ToFormattedString()}");
                    }

                    ++progressCount;
                    var newPercentageText = $"{(double)progressCount / totalCount:p2}";
                    if (newPercentageText != percentageText)
                    {
                        Console.Write($"Test progress: {newPercentageText}        \r");
                        percentageText = newPercentageText;
                    }
                }
            }

            Console.WriteLine();
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static int CalculateGcdByDotNet(ReadOnlySpan<uint> left, ReadOnlySpan<uint> right, Span<uint> result)
        {
            result.Clear();
            if (left.Length == 0)
            {
                if (right.Length == 0)
                {
                    throw new Exception();
                }
                else
                {
                    right.CopyTo(result);
                    result[right.Length..].Clear();
                }
            }
            else if (left.Length == 1)
            {
                if (right.Length == 0)
                {

                    result[0] = left[0];
                    result[1..].Clear();
                }
                else if (right.Length == 1)
                {
                    result[0] = BigIntegerCalculator.Gcd(left[0], right[0]);
                    result[1..].Clear();
                }
                else
                {
                    result[0] = BigIntegerCalculator.Gcd(right, left[0]);
                    result[1..].Clear();
                }
            }
            else
            {
                if (right.Length == 0)
                {
                    left.CopyTo(result);
                    result[left.Length..].Clear();
                }
                else if (right.Length == 1)
                {
                    result[0] = BigIntegerCalculator.Gcd(left, right[0]);
                    result[1..].Clear();
                }
                else
                {
                    var c = left.DataCompareTo(right);
                    if (c > 0)
                    {
                        BigIntegerCalculator.Gcd(left, right, result[..left.Length]);
                        result[left.Length..].Clear();
                    }
                    else if (c < 0)
                    {
                        BigIntegerCalculator.Gcd(right, left, result[..right.Length]);
                        result[right.Length..].Clear();
                    }
                    else
                    {
                        left.CopyTo(result);
                        result[left.Length..].Clear();
                    }
                }
            }

            var lengthOfResult = result.Length;
            while (lengthOfResult > 0 && result[lengthOfResult - 1] == 0)
                --lengthOfResult;
            return lengthOfResult;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static int CalculateGcdByVer2(ReadOnlySpan<uint> left, ReadOnlySpan<uint> right, Span<uint> result)
        {
            result.Clear();
            if (left.Length == 0)
            {
                right.CopyTo(result);
                result[right.Length..].Clear();
            }
            else if (left.Length == 1)
            {
                if (right.Length == 0)
                {
                    result[0] = left[0];
                    if (result.Length > 1)
                        result[1..].Clear();
                }
                else if (right.Length == 1)
                {
                    result[0] = BigIntegerCalculatorVer2.Gcd(left[0], right[0]);
                    if (result.Length > 1)
                        result[1..].Clear();
                }
                else if (Environment.Is64BitProcess && right.Length == 2)
                {
                    var rightValue = ((ulong)right[1] << 32) | right[0];
                    var gcd = BigIntegerCalculatorVer2.Gcd(left[0], rightValue);
                    var lowPart = (uint)gcd;
                    var highPart = (uint)(gcd >> 32);
                    result[0] = lowPart;
                    if (highPart != 0)
                        throw new Exception();
                    result[1..].Clear();
                }
                else
                {
                    result[0] = BigIntegerCalculatorVer2.Gcd(right, left[0]);
                    result[1..].Clear();
                }
            }
            else if (Environment.Is64BitProcess && left.Length == 2)
            {
                if (right.Length == 0)
                {
                    result[0] = left[0];
                    result[1] = left[1];
                    result[2..].Clear();
                }
                else if (right.Length == 1)
                {
                    var leftValue = ((ulong)left[1] << 32) | left[0];
                    var gcd = BigIntegerCalculatorVer2.Gcd(leftValue, right[0]);
                    var lowPart = (uint)gcd;
                    var highPart = (uint)(gcd >> 32);
                    result[0] = lowPart;
                    if (highPart != 0)
                        throw new Exception();
                    result[1..].Clear();
                }
                else if (right.Length == 2)
                {
                    var leftValue = ((ulong)left[1] << 32) | left[0];
                    var rightValue = ((ulong)right[1] << 32) | right[0];
                    var gcd = BigIntegerCalculatorVer2.Gcd(leftValue, rightValue);
                    var lowPart = (uint)gcd;
                    var highPart = (uint)(gcd >> 32);
                    result[0] = lowPart;
                    result[1] = highPart;
                    result[2..].Clear();
                }
                else
                {
                    var c = left.DataCompareTo(right);
                    if (c > 0)
                    {
                        BigIntegerCalculatorVer2.Gcd(left, right, result[..left.Length]);
                        result[left.Length..].Clear();
                    }
                    else if (c < 0)
                    {
                        BigIntegerCalculatorVer2.Gcd(right, left, result[..right.Length]);
                        result[right.Length..].Clear();
                    }
                    else
                    {
                        left.CopyTo(result);
                        result[left.Length..].Clear();
                    }
                }
            }
            else
            {
                if (right.Length == 0)
                {
                    left.CopyTo(result);
                    result[left.Length..].Clear();
                }
                else if (right.Length == 1)
                {
                    result[0] = BigIntegerCalculatorVer2.Gcd(left, right[0]);
                    result[1..].Clear();
                }
                else
                {
                    var c = left.DataCompareTo(right);
                    if (c > 0)
                    {
                        BigIntegerCalculatorVer2.Gcd(left, right, result[..left.Length]);
                        result[left.Length..].Clear();
                    }
                    else if (c < 0)
                    {
                        BigIntegerCalculatorVer2.Gcd(right, left, result[..right.Length]);
                        result[right.Length..].Clear();
                    }
                    else
                    {
                        left.CopyTo(result);
                        result[left.Length..].Clear();
                    }
                }
            }

            var lengthOfResult = result.Length;
            while (lengthOfResult > 0 && result[lengthOfResult - 1] == 0)
                --lengthOfResult;
            return lengthOfResult;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static int CalculateGcdByVer2_1(ReadOnlySpan<uint> left, ReadOnlySpan<uint> right, Span<uint> result)
        {
            result.Clear();
            if (left.Length == 0)
            {
                right.CopyTo(result);
                result[right.Length..].Clear();
            }
            else if (left.Length == 1)
            {
                if (right.Length == 0)
                {
                    result[0] = left[0];
                    if (result.Length > 1)
                        result[1..].Clear();
                }
                else if (right.Length == 1)
                {
                    result[0] = BigIntegerCalculatorVer2_1.Gcd(left[0], right[0]);
                    if (result.Length > 1)
                        result[1..].Clear();
                }
                else if (Environment.Is64BitProcess && right.Length == 2)
                {
                    var rightValue = ((ulong)right[1] << 32) | right[0];
                    var gcd = BigIntegerCalculatorVer2_1.Gcd(left[0], rightValue);
                    var lowPart = (uint)gcd;
                    var highPart = (uint)(gcd >> 32);
                    result[0] = lowPart;
                    if (highPart != 0)
                        throw new Exception();
                    if (result.Length > 1)
                        result[1..].Clear();
                }
                else
                {
                    result[0] = BigIntegerCalculatorVer2_1.Gcd(right, left[0]);
                    if (result.Length > 1)
                        result[1..].Clear();
                }
            }
            else if (Environment.Is64BitProcess && left.Length == 2)
            {
                if (right.Length == 0)
                {
                    result[0] = left[0];
                    result[1] = left[1];
                    if (result.Length > 2)
                        result[2..].Clear();
                }
                else if (right.Length == 1)
                {
                    var leftValue = ((ulong)left[1] << 32) | left[0];
                    var gcd = BigIntegerCalculatorVer2_1.Gcd(leftValue, right[0]);
                    var lowPart = (uint)gcd;
                    var highPart = (uint)(gcd >> 32);
                    result[0] = lowPart;
                    if (highPart != 0)
                        throw new Exception();
                    if (result.Length > 1)
                        result[1..].Clear();
                }
                else if (right.Length == 2)
                {
                    var leftValue = ((ulong)left[1] << 32) | left[0];
                    var rightValue = ((ulong)right[1] << 32) | right[0];
                    var gcd = BigIntegerCalculatorVer2_1.Gcd(leftValue, rightValue);
                    var lowPart = (uint)gcd;
                    var highPart = (uint)(gcd >> 32);
                    result[0] = lowPart;
                    result[1] = highPart;
                    if (result.Length > 2)
                        result[2..].Clear();
                }
                else
                {
                    var c = left.DataCompareTo(right);
                    if (c > 0)
                    {
                        BigIntegerCalculatorVer2_1.Gcd(left, right, result[..left.Length]);
                        result[left.Length..].Clear();
                    }
                    else if (c < 0)
                    {
                        BigIntegerCalculatorVer2_1.Gcd(right, left, result[..right.Length]);
                        result[right.Length..].Clear();
                    }
                    else
                    {
                        left.CopyTo(result);
                        result[left.Length..].Clear();
                    }
                }
            }
            else
            {
                if (right.Length == 0)
                {
                    left.CopyTo(result);
                    result[left.Length..].Clear();
                }
                else if (right.Length == 1)
                {
                    result[0] = BigIntegerCalculatorVer2_1.Gcd(left, right[0]);
                    if (result.Length > 1)
                        result[1..].Clear();
                }
                else
                {
                    var c = left.DataCompareTo(right);
                    if (c > 0)
                    {
                        BigIntegerCalculatorVer2_1.Gcd(left, right, result[..left.Length]);
                        result[left.Length..].Clear();
                    }
                    else if (c < 0)
                    {
                        BigIntegerCalculatorVer2_1.Gcd(right, left, result[..right.Length]);
                        result[right.Length..].Clear();
                    }
                    else
                    {
                        left.CopyTo(result);
                        result[left.Length..].Clear();
                    }
                }
            }

            var lengthOfResult = result.Length;
            while (lengthOfResult > 0 && result[lengthOfResult - 1] == 0)
                --lengthOfResult;
            return lengthOfResult;
        }
    }
}
