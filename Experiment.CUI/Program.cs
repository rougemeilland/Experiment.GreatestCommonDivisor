#define OUTPUT_PERFORMANCE_LOG
//#define ANALYZE_PERFORMANCE
#define UNIT_TEST
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;

namespace Experiment.CUI
{
    internal sealed class Program
    {
        private static void Main()
        {
#if UNIT_TEST
            BigIntegerCalculatorVer2.TestPrivateMethods();
            Test();
#endif
#if ANALYZE_PERFORMANCE
            CheckPerformance();
#endif
#if OUTPUT_PERFORMANCE_LOG
            EstimatePerformance();
#endif

            Console.WriteLine("Completed.");
            Console.Beep();
#if UNIT_TEST && !OUTPUT_PERFORMANCE_LOG
            _ = Console.ReadLine();
#endif
        }

        private static void EstimatePerformance()
        {
            var now = DateTime.Now;
            var (timeOfDotNetOn8Bit, timeOfVer2On8Bit) = Benchmark8();
            var (timeOfDotNetOn16bit, timeOfVer2On16bit) = Benchmark16();
            var (timeOfDotNetOn32bit, timeOfVer2On32bit) = Benchmark32();
            var (timeOfDotNetOn64bit, timeOfVer2On64bit) = Benchmark64();
            var (timeOfDotNetOn128bit, timeOfVer2On128bit) = Benchmark128();
            var (timeOfDotNetOn256bit, timeOfVer2On256bit) = Benchmark256();
            var (timeOfDotNetOn512bit, timeOfVer2On512bit) = Benchmark512();
            var (timeOfDotNetOn1024bit, timeOfVer2On1024bit) = Benchmark1024();

            using var outStream = new FileStream("performance-log.csv", FileMode.OpenOrCreate, FileAccess.Write, FileShare.None);
            var created = outStream.Length <= 0;
            outStream.Position = outStream.Length;
            using var writer = new StreamWriter(outStream, Encoding.UTF8);
            if (created)
            {
                writer.WriteLine(
                    string.Join(
                        "\t",
                        [
                            "Date",
                            "Config",
                            "DotNet_8bit",
                            "Ver2_8bit",
                            "Rate_8bit",
                            "DotNet_16bit",
                            "Ver2_16bit",
                            "Rate_16bit",
                            "DotNet_32bit",
                            "Ver2_32bit",
                            "Rate_32bit",
                            "DotNet_64bit",
                            "Ver2_64bit",
                            "Rate_64bit",
                            "DotNet_128bit",
                            "Ver2_128bit",
                            "Rate_128bit",
                            "DotNet_256bit",
                            "Ver2_256bit",
                            "Rate_256bit",
                            "DotNet_512bit",
                            "Ver2_512bit",
                            "Rate_512bit",
                            "DotNet_1024bit",
                            "Ver2_1024bit",
                            "Rate_1024bit",
                        ]));
            }

            writer.WriteLine(
                string.Join(
                    "\t",
                    [
                        now.ToString("O"),
                                BigIntegerCalculatorVer2.GetGcdConfiguration(),

                                timeOfDotNetOn8Bit.TotalSeconds.ToString("F12", CultureInfo.InvariantCulture),
                                timeOfVer2On8Bit.TotalSeconds.ToString("F12", CultureInfo.InvariantCulture),
                                (timeOfVer2On8Bit / timeOfDotNetOn8Bit).ToString("P2", CultureInfo.InvariantCulture),

                                timeOfDotNetOn16bit.TotalSeconds.ToString("F12", CultureInfo.InvariantCulture),
                                timeOfVer2On16bit.TotalSeconds.ToString("F12", CultureInfo.InvariantCulture),
                                (timeOfVer2On16bit / timeOfDotNetOn16bit).ToString("P2", CultureInfo.InvariantCulture),

                                timeOfDotNetOn32bit.TotalSeconds.ToString("F12", CultureInfo.InvariantCulture),
                                timeOfVer2On32bit.TotalSeconds.ToString("F12", CultureInfo.InvariantCulture),
                                (timeOfVer2On32bit / timeOfDotNetOn32bit).ToString("p2", CultureInfo.InvariantCulture),

                                timeOfDotNetOn64bit.TotalSeconds.ToString("F12", CultureInfo.InvariantCulture),
                                timeOfVer2On64bit.TotalSeconds.ToString("F12", CultureInfo.InvariantCulture),
                                (timeOfVer2On64bit / timeOfDotNetOn64bit).ToString("p2", CultureInfo.InvariantCulture),

                                timeOfDotNetOn128bit.TotalSeconds.ToString("F12", CultureInfo.InvariantCulture),
                                timeOfVer2On128bit.TotalSeconds.ToString("F12", CultureInfo.InvariantCulture),
                                (timeOfVer2On128bit / timeOfDotNetOn128bit).ToString("p2", CultureInfo.InvariantCulture),

                                timeOfDotNetOn256bit.TotalSeconds.ToString("F12", CultureInfo.InvariantCulture),
                                timeOfVer2On256bit.TotalSeconds.ToString("F12", CultureInfo.InvariantCulture),
                                (timeOfVer2On256bit / timeOfDotNetOn256bit).ToString("p2", CultureInfo.InvariantCulture),

                                timeOfDotNetOn512bit.TotalSeconds.ToString("F12", CultureInfo.InvariantCulture),
                                timeOfVer2On512bit.TotalSeconds.ToString("F12", CultureInfo.InvariantCulture),
                                (timeOfVer2On512bit / timeOfDotNetOn512bit).ToString("p2", CultureInfo.InvariantCulture),

                                timeOfDotNetOn1024bit.TotalSeconds.ToString("F12", CultureInfo.InvariantCulture),
                                timeOfVer2On1024bit.TotalSeconds.ToString("F12", CultureInfo.InvariantCulture),
                                (timeOfVer2On1024bit / timeOfDotNetOn1024bit).ToString("p2", CultureInfo.InvariantCulture),
                    ]));
        }

        private static void Test()
        {
            var dataSet = EnumerateTestData().ToArray();
            Span<uint> resultBuffer1 = stackalloc uint[32];
            Span<uint> resultBuffer2 = stackalloc uint[32];

            var totalCount = (long)dataSet.Length * (dataSet.Length + 1) / 2;
            var progressCount = 0L;
            var percentageText = $"{(double)progressCount / totalCount:p2}";
            Console.Write($"Test progress: {percentageText}        \r");

            for (var index1 = 0; index1 < dataSet.Length; ++index1)
            {
                var left = dataSet[index1];
                for (var index2 = index1; index2 < dataSet.Length; ++index2)
                {
                    var right = dataSet[index2];
                    if (left.Length > 0 && right.Length > 0)
                    {
                        var expected = resultBuffer1[..CalculateGcdByDotNet(left.Span, right.Span, resultBuffer1)];
                        var actual = resultBuffer2[..CalculateGcdByVer2(left.Span, right.Span, resultBuffer2)];
                        if (!EqualData(actual, expected))
                            throw new Exception($"GCD({FormatArray(left.Span)}, {FormatArray(right.Span)}), actual = {FormatArray(actual)}, expected = {FormatArray(expected)}");
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

            static IEnumerable<ReadOnlyMemory<uint>> EnumerateTestData()
            {
                var baseDirectory = AppContext.BaseDirectory;
                using var reader = new StreamReader(Path.Combine(baseDirectory, "test-data-for-unit-test.txt"));
                while (true)
                {
                    var text = reader.ReadLine();
                    if (text is null)
                        break;
                    yield return FromBigIntegerToUInt32Array(BigInteger.Parse(text, NumberStyles.None, CultureInfo.InvariantCulture));
                }
            }
        }

        private static (TimeSpan timeOfDotNet, TimeSpan timeOfVer2) Benchmark8()
        {
            const int LOOP_COUNT = 50000000;
            var sw = new System.Diagnostics.Stopwatch();
            var swForDotNet = new System.Diagnostics.Stopwatch();
            var swForVer2 = new System.Diagnostics.Stopwatch();
            var dataSet =
                File.ReadAllLines(Path.Combine(AppContext.BaseDirectory, "test-data-for-performance-8bit.txt"))
                .Select(text => byte.Parse(text, NumberStyles.None, CultureInfo.InvariantCulture))
                .ToArray();
            sw.Start();
            foreach (var left in dataSet)
            {
                foreach (var right in dataSet)
                {
                    if (left > 0 || right > 0)
                    {
                        var u = left;
                        var v = right;
                        if (u < v)
                            (u, v) = (v, u);
                        swForDotNet.Start();
                        for (var count = 0; count < LOOP_COUNT; ++count)
                            CalculateGcdByDotNet(u, v);
                        swForDotNet.Stop();

                        swForVer2.Start();
                        for (var count = 0; count < LOOP_COUNT; ++count)
                            CalculateGcdByVer2(u, v);
                        swForVer2.Stop();
                    }
                }
            }

            sw.Stop();
            var totalCount = dataSet.Length * dataSet.Length * LOOP_COUNT;
            var timeOfDotNet = swForDotNet.Elapsed / totalCount;
            var timeOfVe2 = swForVer2.Elapsed / totalCount;
            Console.WriteLine($"Benchmark(8bit): total: {sw.Elapsed.TotalMilliseconds:N6}[msec]");
            Console.WriteLine($"Benchmark(8bit):  .Net: {timeOfDotNet.TotalMilliseconds:N6}[msec]");
            Console.WriteLine($"Benchmark(8bit):  Ver2: {timeOfVe2.TotalMilliseconds:N6}[msec]");
            return (timeOfDotNet, timeOfVe2);

            [MethodImpl(MethodImplOptions.NoInlining)]
            static void CalculateGcdByDotNet(byte left, byte right)
            {
                _ = BigIntegerCalculator.Gcd(left, right);
            }

            [MethodImpl(MethodImplOptions.NoInlining)]
            static void CalculateGcdByVer2(byte left, byte right)
            {
                _ = BigIntegerCalculatorVer2.Gcd(left, right);
            }
        }

        private static (TimeSpan timeOfDotNet, TimeSpan timeOfVer2) Benchmark16()
        {
            const int LOOP_COUNT = 20000000;
            var sw = new System.Diagnostics.Stopwatch();
            var swForDotNet = new System.Diagnostics.Stopwatch();
            var swForVer2 = new System.Diagnostics.Stopwatch();
            var dataSet =
                File.ReadAllLines(Path.Combine(AppContext.BaseDirectory, "test-data-for-performance-16bit.txt"))
                .Select(text => ushort.Parse(text, NumberStyles.None, CultureInfo.InvariantCulture))
                .ToArray();
            sw.Start();
            foreach (var left in dataSet)
            {
                foreach (var right in dataSet)
                {
                    if (left > 0 || right > 0)
                    {
                        var u = left;
                        var v = right;
                        if (u < v)
                            (u, v) = (v, u);
                        swForDotNet.Start();
                        for (var count = 0; count < LOOP_COUNT; ++count)
                            CalculateGcdByDotNet(u, v);
                        swForDotNet.Stop();

                        swForVer2.Start();
                        for (var count = 0; count < LOOP_COUNT; ++count)
                            CalculateGcdByVer2(u, v);
                        swForVer2.Stop();
                    }
                }
            }

            sw.Stop();
            var totalCount = dataSet.Length * dataSet.Length * LOOP_COUNT;
            var timeOfDotNet = swForDotNet.Elapsed / totalCount;
            var timeOfVe2 = swForVer2.Elapsed / totalCount;
            Console.WriteLine($"Benchmark(16bit): total: {sw.Elapsed.TotalMilliseconds:N6}[msec]");
            Console.WriteLine($"Benchmark(16bit):  .Net: {timeOfDotNet.TotalMilliseconds:N6}[msec]");
            Console.WriteLine($"Benchmark(16bit):  Ver2: {timeOfVe2.TotalMilliseconds:N6}[msec]");
            return (timeOfDotNet, timeOfVe2);

            [MethodImpl(MethodImplOptions.NoInlining)]
            static void CalculateGcdByDotNet(ushort left, ushort right)
            {
                _ = BigIntegerCalculator.Gcd(left, right);
            }

            [MethodImpl(MethodImplOptions.NoInlining)]
            static void CalculateGcdByVer2(ushort left, ushort right)
            {
                _ = BigIntegerCalculatorVer2.Gcd(left, right);
            }
        }

        private static (TimeSpan timeOfDotNet, TimeSpan timeOfVer2) Benchmark32()
        {
            const int LOOP_COUNT = 10000000;
            var sw = new System.Diagnostics.Stopwatch();
            var swForDotNet = new System.Diagnostics.Stopwatch();
            var swForVer2 = new System.Diagnostics.Stopwatch();
            var dataSet =
                File.ReadAllLines(Path.Combine(AppContext.BaseDirectory, "test-data-for-performance-32bit.txt"))
                .Select(text => uint.Parse(text, NumberStyles.None, CultureInfo.InvariantCulture))
                .ToArray();
            sw.Start();
            foreach (var left in dataSet)
            {
                foreach (var right in dataSet)
                {
                    if (left > 0 || right > 0)
                    {
                        var u = left;
                        var v = right;
                        if (u < v)
                            (u, v) = (v, u);
                        swForDotNet.Start();
                        for (var count = 0; count < LOOP_COUNT; ++count)
                            CalculateGcdByDotNet(u, v);
                        swForDotNet.Stop();

                        swForVer2.Start();
                        for (var count = 0; count < LOOP_COUNT; ++count)
                            CalculateGcdByVer2(u, v);
                        swForVer2.Stop();
                    }
                }
            }

            sw.Stop();
            var totalCount = dataSet.Length * dataSet.Length * LOOP_COUNT;
            var timeOfDotNet = swForDotNet.Elapsed / totalCount;
            var timeOfVe2 = swForVer2.Elapsed / totalCount;
            Console.WriteLine($"Benchmark(32bit): total: {sw.Elapsed.TotalMilliseconds:N6}[msec]");
            Console.WriteLine($"Benchmark(32bit):  .Net: {timeOfDotNet.TotalMilliseconds:N6}[msec]");
            Console.WriteLine($"Benchmark(32bit):  Ver2: {timeOfVe2.TotalMilliseconds:N6}[msec]");
            return (timeOfDotNet, timeOfVe2);

            [MethodImpl(MethodImplOptions.NoInlining)]
            static void CalculateGcdByDotNet(uint left, uint right)
            {
                _ = BigIntegerCalculator.Gcd(left, right);
            }

            [MethodImpl(MethodImplOptions.NoInlining)]
            static void CalculateGcdByVer2(uint left, uint right)
            {
                _ = BigIntegerCalculatorVer2.Gcd(left, right);
            }
        }

        private static (TimeSpan timeOfDotNet, TimeSpan timeOfVer2) Benchmark64()
        {
            const int LOOP_COUNT = 4000000;
            var sw = new System.Diagnostics.Stopwatch();
            var swForDotNet = new System.Diagnostics.Stopwatch();
            var swForVer2 = new System.Diagnostics.Stopwatch();
            var dataSet =
                File.ReadAllLines(Path.Combine(AppContext.BaseDirectory, "test-data-for-performance-64bit.txt"))
                .Select(text => ulong.Parse(text, NumberStyles.None, CultureInfo.InvariantCulture))
                .ToArray();
            sw.Start();
            foreach (var left in dataSet)
            {
                foreach (var right in dataSet)
                {
                    if (left > 0 || right > 0)
                    {
                        var u = left;
                        var v = right;
                        if (u < v)
                            (u, v) = (v, u);
                        swForDotNet.Start();
                        for (var count = 0; count < LOOP_COUNT; ++count)
                            CalculateGcdByDotNet(u, v);
                        swForDotNet.Stop();

                        swForVer2.Start();
                        for (var count = 0; count < LOOP_COUNT; ++count)
                            CalculateGcdByVer2(u, v);
                        swForVer2.Stop();
                    }
                }
            }

            sw.Stop();
            var totalCount = dataSet.Length * dataSet.Length * LOOP_COUNT;
            var timeOfDotNet = swForDotNet.Elapsed / totalCount;
            var timeOfVe2 = swForVer2.Elapsed / totalCount;
            Console.WriteLine($"Benchmark(64bit): total: {sw.Elapsed.TotalMilliseconds:N6}[msec]");
            Console.WriteLine($"Benchmark(64bit):  .Net: {timeOfDotNet.TotalMilliseconds:N6}[msec]");
            Console.WriteLine($"Benchmark(64bit):  Ver2: {timeOfVe2.TotalMilliseconds:N6}[msec]");
            return (timeOfDotNet, timeOfVe2);

            [MethodImpl(MethodImplOptions.NoInlining)]
            static void CalculateGcdByDotNet(ulong left, ulong right)
            {
                _ = BigIntegerCalculator.Gcd(left, right);
            }

            [MethodImpl(MethodImplOptions.NoInlining)]
            static void CalculateGcdByVer2(ulong left, ulong right)
            {
                _ = BigIntegerCalculatorVer2.Gcd(left, right);
            }
        }

        private static (TimeSpan timeOfDotNet, TimeSpan timeOfVer2) Benchmark128()
        {
            const int LOOP_COUNT = 200000;
            var sw = new System.Diagnostics.Stopwatch();
            var swForDotNet = new System.Diagnostics.Stopwatch();
            var swForVer2 = new System.Diagnostics.Stopwatch();
            var dataSet =
                File.ReadAllLines(Path.Combine(AppContext.BaseDirectory, "test-data-for-performance-128bit.txt"))
                .Select(text => UInt128.Parse(text, NumberStyles.None, CultureInfo.InvariantCulture))
                .ToArray();

            Span<uint> uBuffer = stackalloc uint[4];
            Span<uint> vBuffer = stackalloc uint[4];
            Span<uint> resultBuffer = stackalloc uint[4];

            sw.Start();
            foreach (var left in dataSet)
            {
                foreach (var right in dataSet)
                {
                    if (left > 0 || right > 0)
                    {
                        var u = left;
                        var v = right;
                        if (u < v)
                            (u, v) = (v, u);
                        for (var index = 0; index < uBuffer.Length; ++index)
                            uBuffer[index] = (uint)((u >> (index * 32)) & uint.MaxValue);
                        for (var index = 0; index < vBuffer.Length; ++index)
                            vBuffer[index] = (uint)((v >> (index * 32)) & uint.MaxValue);

                        swForDotNet.Start();
                        for (var count = 0; count < LOOP_COUNT; ++count)
                            CalculateGcdByDotNet(uBuffer, vBuffer, resultBuffer);
                        swForDotNet.Stop();

                        swForVer2.Start();
                        for (var count = 0; count < LOOP_COUNT; ++count)
                            CalculateGcdByVer2(uBuffer, vBuffer, resultBuffer);
                        swForVer2.Stop();
                    }
                }
            }

            sw.Stop();
            var totalCount = dataSet.Length * dataSet.Length * LOOP_COUNT;
            var timeOfDotNet = swForDotNet.Elapsed / totalCount;
            var timeOfVe2 = swForVer2.Elapsed / totalCount;
            Console.WriteLine($"Benchmark(128bit): total: {sw.Elapsed.TotalMilliseconds:N6}[msec]");
            Console.WriteLine($"Benchmark(128bit):  .Net: {timeOfDotNet.TotalMilliseconds:N6}[msec]");
            Console.WriteLine($"Benchmark(128bit):  Ver2: {timeOfVe2.TotalMilliseconds:N6}[msec]");
            return (timeOfDotNet, timeOfVe2);

            [MethodImpl(MethodImplOptions.NoInlining)]
            static void CalculateGcdByDotNet(ReadOnlySpan<uint> left, ReadOnlySpan<uint> right, Span<uint> result)
            {
                BigIntegerCalculator.Gcd(left, right, result);
            }

            [MethodImpl(MethodImplOptions.NoInlining)]
            static void CalculateGcdByVer2(ReadOnlySpan<uint> left, ReadOnlySpan<uint> right, Span<uint> result)
            {
                BigIntegerCalculatorVer2.Gcd(left, right, result);
            }
        }

        private static (TimeSpan timeOfDotNet, TimeSpan timeOfVer2) Benchmark256()
        {
            const int LOOP_COUNT = 100000;
            var sw = new System.Diagnostics.Stopwatch();
            var swForDotNet = new System.Diagnostics.Stopwatch();
            var swForVer2 = new System.Diagnostics.Stopwatch();
            var dataSet =
                File.ReadAllLines(Path.Combine(AppContext.BaseDirectory, "test-data-for-performance-256bit.txt"))
                .Select(text => BigInteger.Parse(text, NumberStyles.None, CultureInfo.InvariantCulture))
                .ToArray();

            Span<uint> uBuffer = stackalloc uint[8];
            Span<uint> vBuffer = stackalloc uint[8];
            Span<uint> resultBuffer = stackalloc uint[8];

            sw.Start();
            foreach (var left in dataSet)
            {
                foreach (var right in dataSet)
                {
                    if (left.Sign > 0 || right.Sign > 0)
                    {
                        var u = left;
                        var v = right;
                        if (u < v)
                            (u, v) = (v, u);
                        for (var index = 0; index < uBuffer.Length; ++index)
                            uBuffer[index] = (uint)((u >> (index * 32)) & uint.MaxValue);
                        for (var index = 0; index < vBuffer.Length; ++index)
                            vBuffer[index] = (uint)((v >> (index * 32)) & uint.MaxValue);

                        swForDotNet.Start();
                        for (var count = 0; count < LOOP_COUNT; ++count)
                            CalculateGcdByDotNet(uBuffer, vBuffer, resultBuffer);
                        swForDotNet.Stop();

                        swForVer2.Start();
                        for (var count = 0; count < LOOP_COUNT; ++count)
                            CalculateGcdByVer2(uBuffer, vBuffer, resultBuffer);
                        swForVer2.Stop();
                    }
                }
            }

            sw.Stop();
            var totalCount = dataSet.Length * dataSet.Length * LOOP_COUNT;
            var timeOfDotNet = swForDotNet.Elapsed / totalCount;
            var timeOfVe2 = swForVer2.Elapsed / totalCount;
            Console.WriteLine($"Benchmark(256bit): total: {sw.Elapsed.TotalMilliseconds:N6}[msec]");
            Console.WriteLine($"Benchmark(256bit):  .Net: {timeOfDotNet.TotalMilliseconds:N6}[msec]");
            Console.WriteLine($"Benchmark(256bit):  Ver2: {timeOfVe2.TotalMilliseconds:N6}[msec]");
            return (timeOfDotNet, timeOfVe2);

            [MethodImpl(MethodImplOptions.NoInlining)]
            static void CalculateGcdByDotNet(ReadOnlySpan<uint> left, ReadOnlySpan<uint> right, Span<uint> result)
            {
                BigIntegerCalculator.Gcd(left, right, result);
            }

            [MethodImpl(MethodImplOptions.NoInlining)]
            static void CalculateGcdByVer2(ReadOnlySpan<uint> left, ReadOnlySpan<uint> right, Span<uint> result)
            {
                BigIntegerCalculatorVer2.Gcd(left, right, result);
            }
        }

        private static (TimeSpan timeOfDotNet, TimeSpan timeOfVer2) Benchmark512()
        {
            const int LOOP_COUNT = 50000;
            var sw = new System.Diagnostics.Stopwatch();
            var swForDotNet = new System.Diagnostics.Stopwatch();
            var swForVer2 = new System.Diagnostics.Stopwatch();
            var dataSet =
                File.ReadAllLines(Path.Combine(AppContext.BaseDirectory, "test-data-for-performance-512bit.txt"))
                .Select(text => BigInteger.Parse(text, NumberStyles.None, CultureInfo.InvariantCulture))
                .ToArray();

            Span<uint> uBuffer = stackalloc uint[16];
            Span<uint> vBuffer = stackalloc uint[16];
            Span<uint> resultBuffer = stackalloc uint[16];

            sw.Start();
            foreach (var left in dataSet)
            {
                foreach (var right in dataSet)
                {
                    if (left.Sign > 0 || right.Sign > 0)
                    {
                        var u = left;
                        var v = right;
                        if (u < v)
                            (u, v) = (v, u);
                        for (var index = 0; index < uBuffer.Length; ++index)
                            uBuffer[index] = (uint)((u >> (index * 32)) & uint.MaxValue);
                        for (var index = 0; index < vBuffer.Length; ++index)
                            vBuffer[index] = (uint)((v >> (index * 32)) & uint.MaxValue);

                        swForDotNet.Start();
                        for (var count = 0; count < LOOP_COUNT; ++count)
                            CalculateGcdByDotNet(uBuffer, vBuffer, resultBuffer);
                        swForDotNet.Stop();

                        swForVer2.Start();
                        for (var count = 0; count < LOOP_COUNT; ++count)
                            CalculateGcdByVer2(uBuffer, vBuffer, resultBuffer);
                        swForVer2.Stop();
                    }
                }
            }

            sw.Stop();
            var totalCount = dataSet.Length * dataSet.Length * LOOP_COUNT;
            var timeOfDotNet = swForDotNet.Elapsed / totalCount;
            var timeOfVe2 = swForVer2.Elapsed / totalCount;
            Console.WriteLine($"Benchmark(512bit): total: {sw.Elapsed.TotalMilliseconds:N6}[msec]");
            Console.WriteLine($"Benchmark(512bit):  .Net: {timeOfDotNet.TotalMilliseconds:N6}[msec]");
            Console.WriteLine($"Benchmark(512bit):  Ver2: {timeOfVe2.TotalMilliseconds:N6}[msec]");
            return (timeOfDotNet, timeOfVe2);

            [MethodImpl(MethodImplOptions.NoInlining)]
            static void CalculateGcdByDotNet(ReadOnlySpan<uint> left, ReadOnlySpan<uint> right, Span<uint> result)
            {
                BigIntegerCalculator.Gcd(left, right, result);
            }

            [MethodImpl(MethodImplOptions.NoInlining)]
            static void CalculateGcdByVer2(ReadOnlySpan<uint> left, ReadOnlySpan<uint> right, Span<uint> result)
            {
                BigIntegerCalculatorVer2.Gcd(left, right, result);
            }
        }

        private static (TimeSpan timeOfDotNet, TimeSpan timeOfVer2) Benchmark1024()
        {
            const int LOOP_COUNT = 20000;
            var sw = new System.Diagnostics.Stopwatch();
            var swForDotNet = new System.Diagnostics.Stopwatch();
            var swForVer2 = new System.Diagnostics.Stopwatch();
            var dataSet =
                File.ReadAllLines(Path.Combine(AppContext.BaseDirectory, "test-data-for-performance-1024bit.txt"))
                .Select(text => BigInteger.Parse(text, NumberStyles.None, CultureInfo.InvariantCulture))
                .ToArray();

            Span<uint> uBuffer = stackalloc uint[32];
            Span<uint> vBuffer = stackalloc uint[32];
            Span<uint> resultBuffer = stackalloc uint[32];

            sw.Start();
            foreach (var left in dataSet)
            {
                foreach (var right in dataSet)
                {
                    if (left.Sign > 0 || right.Sign > 0)
                    {
                        var u = left;
                        var v = right;
                        if (u < v)
                            (u, v) = (v, u);
                        for (var index = 0; index < uBuffer.Length; ++index)
                            uBuffer[index] = (uint)((u >> (index * 32)) & uint.MaxValue);
                        for (var index = 0; index < vBuffer.Length; ++index)
                            vBuffer[index] = (uint)((v >> (index * 32)) & uint.MaxValue);

                        swForDotNet.Start();
                        for (var count = 0; count < LOOP_COUNT; ++count)
                            CalculateGcdByDotNet(uBuffer, vBuffer, resultBuffer);
                        swForDotNet.Stop();

                        swForVer2.Start();
                        for (var count = 0; count < LOOP_COUNT; ++count)
                            CalculateGcdByVer2(uBuffer, vBuffer, resultBuffer);
                        swForVer2.Stop();
                    }
                }
            }

            sw.Stop();
            var totalCount = dataSet.Length * dataSet.Length * LOOP_COUNT;
            var timeOfDotNet = swForDotNet.Elapsed / totalCount;
            var timeOfVe2 = swForVer2.Elapsed / totalCount;
            Console.WriteLine($"Benchmark(1024bit): total: {sw.Elapsed.TotalMilliseconds:N6}[msec]");
            Console.WriteLine($"Benchmark(1024bit):  .Net: {timeOfDotNet.TotalMilliseconds:N6}[msec]");
            Console.WriteLine($"Benchmark(1024bit):  Ver2: {timeOfVe2.TotalMilliseconds:N6}[msec]");
            return (timeOfDotNet, timeOfVe2);

            [MethodImpl(MethodImplOptions.NoInlining)]
            static void CalculateGcdByDotNet(ReadOnlySpan<uint> left, ReadOnlySpan<uint> right, Span<uint> result)
            {
                BigIntegerCalculator.Gcd(left, right, result);
            }

            [MethodImpl(MethodImplOptions.NoInlining)]
            static void CalculateGcdByVer2(ReadOnlySpan<uint> left, ReadOnlySpan<uint> right, Span<uint> result)
            {
                BigIntegerCalculatorVer2.Gcd(left, right, result);
            }
        }

        private static void CheckPerformance()
        {
            const int LOOP_COUNT = 1000000;
            var u = BigInteger.Parse("105730772440763124017630612202234101319161314839922858526453951924172399146116170652599378894093105490149630931568112748386095163244060914841338619002272658212886786698191157266935901111416363224179101007054605014965636947907505540286689075101217669281142712194594033878235159075262206141769345965416687288590", NumberStyles.None, CultureInfo.InvariantCulture);
            var v = BigInteger.Parse("112151584900825819239313445576332394792341091129300610865562156067654777303755016130189581322864131838803336221501924172559438747789196866890503747575369881392254717160905226196930817784620186479645291070051428972529523233144587920346658031220416328118471498020780059966891629079943679030059141108463271278746", NumberStyles.None, CultureInfo.InvariantCulture);

            Span<uint> uBuffer = stackalloc uint[32];
            Span<uint> vBuffer = stackalloc uint[32];
            Span<uint> resultBuffer = stackalloc uint[32];

            if (u < v)
                (u, v) = (v, u);
            for (var index = 0; index < uBuffer.Length; ++index)
                uBuffer[index] = (uint)((u >> (index * 32)) & uint.MaxValue);
            for (var index = 0; index < vBuffer.Length; ++index)
                vBuffer[index] = (uint)((v >> (index * 32)) & uint.MaxValue);

            for (var count = 0; count < LOOP_COUNT; ++count)
                CalculateGcdByVer2(uBuffer, vBuffer, resultBuffer);

            [MethodImpl(MethodImplOptions.NoInlining)]
            static void CalculateGcdByVer2(ReadOnlySpan<uint> left, ReadOnlySpan<uint> right, Span<uint> result)
            {
                BigIntegerCalculatorVer2.Gcd(left, right, result);
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static int CalculateGcdByDotNet(ReadOnlySpan<uint> left, ReadOnlySpan<uint> right, Span<uint> result)
        {
            if (left.Length == 0)
            {
                if (right.Length == 0)
                    throw new Exception();
                else
                    right.CopyTo(result);
            }
            else if (left.Length == 1)
            {
                if (right.Length == 0)
                    result[0] = left[0];
                else if (right.Length == 1)
                    result[0] = BigIntegerCalculator.Gcd(left[0], right[0]);
                else
                    result[0] = BigIntegerCalculator.Gcd(right, left[0]);
            }
            else
            {
                if (right.Length == 0)
                {
                    left.CopyTo(result);
                }
                else if (right.Length == 1)
                {
                    result[0] = BigIntegerCalculator.Gcd(left, right[0]);
                }
                else
                {
                    var c = CompareData(left, right);
                    if (c > 0)
                        BigIntegerCalculator.Gcd(left, right, result[..left.Length]);
                    else if (c < 0)
                        BigIntegerCalculator.Gcd(right, left, result[..right.Length]);
                    else
                        left.CopyTo(result);
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
            if (left.Length == 0)
            {
                right.CopyTo(result);
                if (result.Length > right.Length)
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
                    if (result.Length > 1)
                        result[1..].Clear();
                }
                else
                {
                    result[0] = BigIntegerCalculatorVer2.Gcd(right, left[0]);
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
                    var gcd = BigIntegerCalculatorVer2.Gcd(leftValue, right[0]);
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
                    var gcd = BigIntegerCalculatorVer2.Gcd(leftValue, rightValue);
                    var lowPart = (uint)gcd;
                    var highPart = (uint)(gcd >> 32);
                    result[0] = lowPart;
                    result[1] = highPart;
                    if (result.Length > 2)
                        result[2..].Clear();
                }
                else
                {
                    var c = CompareData(left, right);
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
                    }
                }
            }
            else
            {
                if (right.Length == 0)
                {
                    left.CopyTo(result);
                    if (result.Length > left.Length)
                        result[left.Length..].Clear();
                }
                else if (right.Length == 1)
                {
                    result[0] = BigIntegerCalculatorVer2.Gcd(left, right[0]);
                    if (result.Length > 1)
                        result[1..].Clear();
                }
                else
                {
                    var c = CompareData(left, right);
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
                    }
                }
            }

            var lengthOfResult = result.Length;
            while (lengthOfResult > 0 && result[lengthOfResult - 1] == 0)
                --lengthOfResult;
            return lengthOfResult;
        }

        private static bool EqualData(ReadOnlySpan<uint> data1, ReadOnlySpan<uint> data2)
        {
            while (data1.Length > 0 && data1[^1] == 0)
                data1 = data1[..^1];
            while (data2.Length > 0 && data2[^1] == 0)
                data2 = data2[..^1];
            if (data1.Length != data2.Length)
                return false;
            for (var index = 0; index < data1.Length; ++index)
            {
                if (data1[index] != data2[index])
                    return false;
            }

            return true;
        }

        private static int CompareData(ReadOnlySpan<uint> data1, ReadOnlySpan<uint> data2)
        {
            while (data1.Length > 0 && data1[^1] == 0)
                data1 = data1[..^1];
            while (data2.Length > 0 && data2[^1] == 0)
                data2 = data2[..^1];
            int c;
            if ((c = data1.Length.CompareTo(data2.Length)) != 0)
                return c;
            for (var index = data1.Length - 1; index >= 0; --index)
            {
                if ((c = data1[index].CompareTo(data2[index])) != 0)
                    return c;
            }

            return 0;
        }

        private static BigInteger ToBigInteger(ReadOnlySpan<byte> value)
        {
            var result = BigInteger.Zero;
            for (var index = value.Length - 1; index >= 0; --index)
            {
                result <<= 8;
                result |= value[index];
            }

            return result;
        }

        private static ReadOnlyMemory<uint> FromBigIntegerToUInt32Array(BigInteger value)
        {
            if (value < 0)
                throw new Exception();
            var result = new List<uint>();
            while (value.Sign > 0)
            {
                result.Add((uint)(value & uint.MaxValue));
                value >>= 32;
            }

            return result.ToArray();
        }

        private static string FormatArray(ReadOnlySpan<uint> value)
        {
            var valueTexts = new List<string>();
            for (var index = 0; index < value.Length; ++index)
                valueTexts.Add($"0x{value[index]:x8}");
            return $"[{string.Join(", ", valueTexts)}]";
        }
    }
}
