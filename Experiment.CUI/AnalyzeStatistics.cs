using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Experiment.CUI
{
    internal static class AnalyzeStatistics
    {
        public static void Analyze()
        {
            const int LOOP_COUNT = 1000;
            Span<uint> uBuffer = stackalloc uint[1024 / 32];
            Span<uint> vBuffer = stackalloc uint[1024 / 32];
            Span<uint> resultBuffer = stackalloc uint[1024 / 32];

            using (var randomNumberGenerator = RandomNumberGenerator.Create())
            {
                BigIntegerCalculatorVer2_1.ClearStatistics();
                var progressText = "";
                ReportProgress(0, LOOP_COUNT, ref progressText);
                for (var count = 0; count < LOOP_COUNT; ++count)
                {
                    GenerateData(randomNumberGenerator, uBuffer, vBuffer);
                    CalculateGcdByVer2_1(uBuffer, vBuffer, resultBuffer);
                    ReportProgress(count, LOOP_COUNT, ref progressText);
                }

                ReportProgress(LOOP_COUNT, LOOP_COUNT, ref progressText);
                Console.WriteLine();
            }

            var statistics =
                BigIntegerCalculatorVer2_1.EnumerateStatistics()
                .Where(item => item.leftBitCount > 64 || item.rightBitCount > 64)
                .Select(item => (difference: item.leftBitCount - item.rightBitCount, item.count))
                .GroupBy(item => item.difference)
                .Select(g => (difference: g.Key, count: g.Aggregate(0UL, (sum, item) => sum + item.count)))
                .OrderByDescending(item => item.count)
                .ToArray();
            var counts = new ulong[1024];
            counts.AsSpan().Clear();
            foreach (var (difference, count) in statistics)
            {
                for (var index = difference; index < counts.Length; ++index)
                    counts[index] += count;
            }

            var totalCount = statistics.Aggregate(0UL, (sum, item) => sum + item.count);
            for (var index = 0; index < counts.Length; ++index)
            {
                var count = counts[index];
                if (count < totalCount)
                    Console.WriteLine($"|difference <= {index} bits| {(double)count / totalCount:P2}|");
                if (count >= totalCount)
                    break;
            }

            static void ReportProgress(int count, int totalCount, ref string progressText)
            {
                var newProgressText = $"{(double)count / totalCount:P2}";
                if (progressText != newProgressText)
                {
                    progressText = newProgressText;
                    Console.Write($"AnalyzeStatistics: {progressText}\r");
                }
            }

            static void GenerateData(RandomNumberGenerator generator, Span<uint> left, Span<uint> right)
            {
                var u = GenerateRandomNumber(generator);
                var v = GenerateRandomNumber(generator);

                if (u > v)
                {
                    FromBigIntegerToUInt32Array(u, left);
                    FromBigIntegerToUInt32Array(v, right);
                }
                else
                {
                    FromBigIntegerToUInt32Array(u, right);
                    FromBigIntegerToUInt32Array(v, left);
                }

                static BigInteger GenerateRandomNumber(RandomNumberGenerator generator)
                {
                    Span<byte> tempBuffer = stackalloc byte[1024 / 8 + 1];
                    var minimum = BigInteger.One << (1024 - 32);

                    while (true)
                    {
                        generator.GetBytes(tempBuffer[..^1]);
                        tempBuffer[^1] = 0;
                        var number = new BigInteger(tempBuffer);
                        if (number >= minimum)
                            return number;
                    }
                }

                static void FromBigIntegerToUInt32Array(BigInteger value, Span<uint> buffer)
                {
                    for (var index = 0; index < buffer.Length; ++index)
                    {
                        buffer[index] = (uint)(value & uint.MaxValue);
                        value >>= 32;
                    }

                    if (value.Sign != 0)
                        throw new Exception();
                }
            }

            [MethodImpl(MethodImplOptions.NoInlining)]
            static void CalculateGcdByVer2_1(ReadOnlySpan<uint> left, ReadOnlySpan<uint> right, Span<uint> result)
            {
                BigIntegerCalculatorVer2_1.Gcd(left, right, result);
            }
        }
    }
}
