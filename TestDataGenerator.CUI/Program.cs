using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Security.Cryptography;

namespace TestDataGenerator.CUI
{
    internal sealed class Program
    {
        private const int _PRIMARY_DATA_COUNT = 8;

        private static void Main()
        {
            using (var writer = new StreamWriter("test-data-for-unit-test.txt"))
            using (var genenerator = RandomNumberGenerator.Create())
            {
                foreach (var value in GenerateUInt128TestDataForUnitTest(genenerator).OrderBy(value => value))
                    writer.WriteLine(value.ToString(System.Globalization.CultureInfo.InvariantCulture));
            }

            using (var writer = new StreamWriter("test-data-for-performance-8bit.txt"))
            using (var genenerator = RandomNumberGenerator.Create())
            {
                foreach (var value in GenerateByteTestDataForPerformance(genenerator).OrderBy(value => value))
                    writer.WriteLine(value.ToString(System.Globalization.CultureInfo.InvariantCulture));
            }

            using (var writer = new StreamWriter("test-data-for-performance-16bit.txt"))
            using (var genenerator = RandomNumberGenerator.Create())
            {
                foreach (var value in GenerateUInt16TestDataForPerformance(genenerator).OrderBy(value => value))
                    writer.WriteLine(value.ToString(System.Globalization.CultureInfo.InvariantCulture));
            }

            using (var writer = new StreamWriter("test-data-for-performance-32bit.txt"))
            using (var genenerator = RandomNumberGenerator.Create())
            {
                foreach (var value in GenerateUInt32TestDataForPerformance(genenerator).OrderBy(value => value))
                    writer.WriteLine(value.ToString(System.Globalization.CultureInfo.InvariantCulture));
            }

            using (var writer = new StreamWriter("test-data-for-performance-64bit.txt"))
            using (var genenerator = RandomNumberGenerator.Create())
            {
                foreach (var value in GenerateUInt64TestDataForPerformance(genenerator).OrderBy(value => value))
                    writer.WriteLine(value.ToString(System.Globalization.CultureInfo.InvariantCulture));
            }

            using (var writer = new StreamWriter("test-data-for-performance-128bit.txt"))
            using (var genenerator = RandomNumberGenerator.Create())
            {
                foreach (var value in GenerateUInt128TestDataForPerformance(genenerator).OrderBy(value => value))
                    writer.WriteLine(value.ToString(System.Globalization.CultureInfo.InvariantCulture));
            }

            using (var writer = new StreamWriter("test-data-for-performance-256bit.txt"))
            using (var genenerator = RandomNumberGenerator.Create())
            {
                foreach (var value in GenerateUInt256TestDataForPerformance(genenerator).OrderBy(value => value))
                    writer.WriteLine(value.ToString(System.Globalization.CultureInfo.InvariantCulture));
            }

            using (var writer = new StreamWriter("test-data-for-performance-512bit.txt"))
            using (var genenerator = RandomNumberGenerator.Create())
            {
                foreach (var value in GenerateUInt512TestDataForPerformance(genenerator).OrderBy(value => value))
                    writer.WriteLine(value.ToString(System.Globalization.CultureInfo.InvariantCulture));
            }

            using (var writer = new StreamWriter("test-data-for-performance-1024bit.txt"))
            using (var genenerator = RandomNumberGenerator.Create())
            {
                foreach (var value in GenerateUInt1024TestDataForPerformance(genenerator).OrderBy(value => value))
                    writer.WriteLine(value.ToString(System.Globalization.CultureInfo.InvariantCulture));
            }

            using (var writer = new StreamWriter("test-data-for-performance-2048bit.txt"))
            using (var genenerator = RandomNumberGenerator.Create())
            {
                foreach (var value in GenerateUInt2048TestDataForPerformance(genenerator).OrderBy(value => value))
                    writer.WriteLine(value.ToString(System.Globalization.CultureInfo.InvariantCulture));
            }

            using (var writer = new StreamWriter("test-data-for-performance-4096bit.txt"))
            using (var genenerator = RandomNumberGenerator.Create())
            {
                foreach (var value in GenerateUInt4096TestDataForPerformance(genenerator).OrderBy(value => value))
                    writer.WriteLine(value.ToString(System.Globalization.CultureInfo.InvariantCulture));
            }

            Console.WriteLine("Completed.");
            Console.Beep();
            _ = Console.ReadLine();
        }

        private static IEnumerable<BigInteger> GenerateUInt128TestDataForUnitTest(RandomNumberGenerator randomNumberGenerator)
        {
            const int BIT_LENGTH = 128;
            const int BYTE_LENGTH = BIT_LENGTH / 8;

            var sourceData = GenerateUInt64TestDataForUnitTest(randomNumberGenerator).ToArray();
            var values = new HashSet<BigInteger>();
            for (var index = 0; index < sourceData.Length; ++index)
                _ = values.Add(sourceData[index]);
            for (var index1 = 0; index1 < sourceData.Length; ++index1)
            {
                var data1 = sourceData[index1];
                if (data1 > 1)
                {
                    for (var index2 = index1; index2 < sourceData.Length; ++index2)
                    {
                        var data2 = sourceData[index2];
                        if (data2 > 1)
                            _ = values.Add(data1 * data2);
                    }
                }
            }

            _ = values.Add(BigInteger.One << BIT_LENGTH);

            var n = values.Count;
            var buffer = new byte[BYTE_LENGTH];
            while (values.Count < n + _PRIMARY_DATA_COUNT)
            {
                randomNumberGenerator.GetBytes(buffer);
                var newValue = BinaryPrimitives.ReadUInt128LittleEndian(buffer);
                if (values.All(value => value == 0 || BigInteger.GreatestCommonDivisor(value, newValue) == BigInteger.One))
                    _ = values.Add(newValue);
            }

            foreach (var value in values)
                yield return value;
        }

        private static IEnumerable<BigInteger> GenerateUInt64TestDataForUnitTest(RandomNumberGenerator randomNumberGenerator)
        {
            const int BIT_LENGTH = 64;
            const int BYTE_LENGTH = BIT_LENGTH / 8;

            var sourceData = GenerateUInt32TestDataForUnitTest(randomNumberGenerator).ToArray();
            var values = new HashSet<BigInteger>();
            for (var index = 0; index < sourceData.Length; ++index)
                _ = values.Add(sourceData[index]);
            for (var index1 = 0; index1 < sourceData.Length; ++index1)
            {
                var data1 = sourceData[index1];
                if (data1 > 1)
                {
                    for (var index2 = index1; index2 < sourceData.Length; ++index2)
                    {
                        var data2 = sourceData[index2];
                        if (data2 > 1)
                            _ = values.Add(data1 * data2);
                    }
                }
            }

            _ = values.Add(BigInteger.One << BIT_LENGTH);

            var n = values.Count;
            var buffer = new byte[BYTE_LENGTH];
            while (values.Count < n + _PRIMARY_DATA_COUNT)
            {
                randomNumberGenerator.GetBytes(buffer);
                var newValue = BinaryPrimitives.ReadUInt64LittleEndian(buffer);
                if (values.All(value => value == 0 || BigInteger.GreatestCommonDivisor(value, newValue) == BigInteger.One))
                    _ = values.Add(newValue);
            }

            foreach (var value in values)
                yield return value;
        }

        private static IEnumerable<BigInteger> GenerateUInt32TestDataForUnitTest(RandomNumberGenerator randomNumberGenerator)
        {
            const int BIT_LENGTH = 32;
            const int BYTE_LENGTH = BIT_LENGTH / 8;

            var values = new HashSet<BigInteger>
            {
                0,
                1,
                2,
                1UL << BIT_LENGTH
            };
            var buffer = new byte[BYTE_LENGTH];
            while (values.Count < _PRIMARY_DATA_COUNT)
            {
                randomNumberGenerator.GetBytes(buffer);
                var newValue = BinaryPrimitives.ReadUInt32LittleEndian(buffer);
                if (values.All(value => value == 0 || BigInteger.GreatestCommonDivisor(value, newValue) == BigInteger.One))
                    _ = values.Add(newValue);
            }

            foreach (var value in values)
                yield return value;
        }

        private static IEnumerable<BigInteger> GenerateUInt4096TestDataForPerformance(RandomNumberGenerator randomNumberGenerator)
        {
            const int BIT_LENGTH = 4096;
            const int BYTE_LENGTH = BIT_LENGTH / 8;
            const int UINT32_BYTE_LENGTH = sizeof(uint);

            var values = new HashSet<BigInteger>();
            var buffer = new byte[BYTE_LENGTH];
            var border = BigInteger.One << (BIT_LENGTH - 32);
            while (values.Count < _PRIMARY_DATA_COUNT)
            {
                randomNumberGenerator.GetBytes(buffer);
                var value = FromByteArrayToBigInteger(buffer);
                if (value >= border)
                    _ = values.Add(value);
            }

            foreach (var value in values)
                yield return value;

            static BigInteger FromByteArrayToBigInteger(ReadOnlySpan<byte> buffer)
            {
                return FromUInt32ArrayToBigInteger(
                [
                    BinaryPrimitives.ReadUInt32LittleEndian(buffer[..UINT32_BYTE_LENGTH]),
                    BinaryPrimitives.ReadUInt32LittleEndian(buffer.Slice(001 * UINT32_BYTE_LENGTH, UINT32_BYTE_LENGTH)),
                    BinaryPrimitives.ReadUInt32LittleEndian(buffer.Slice(002 * UINT32_BYTE_LENGTH, UINT32_BYTE_LENGTH)),
                    BinaryPrimitives.ReadUInt32LittleEndian(buffer.Slice(003 * UINT32_BYTE_LENGTH, UINT32_BYTE_LENGTH)),
                    BinaryPrimitives.ReadUInt32LittleEndian(buffer.Slice(004 * UINT32_BYTE_LENGTH, UINT32_BYTE_LENGTH)),
                    BinaryPrimitives.ReadUInt32LittleEndian(buffer.Slice(005 * UINT32_BYTE_LENGTH, UINT32_BYTE_LENGTH)),
                    BinaryPrimitives.ReadUInt32LittleEndian(buffer.Slice(006 * UINT32_BYTE_LENGTH, UINT32_BYTE_LENGTH)),
                    BinaryPrimitives.ReadUInt32LittleEndian(buffer.Slice(007 * UINT32_BYTE_LENGTH, UINT32_BYTE_LENGTH)),
                    BinaryPrimitives.ReadUInt32LittleEndian(buffer.Slice(008 * UINT32_BYTE_LENGTH, UINT32_BYTE_LENGTH)),
                    BinaryPrimitives.ReadUInt32LittleEndian(buffer.Slice(009 * UINT32_BYTE_LENGTH, UINT32_BYTE_LENGTH)),
                    BinaryPrimitives.ReadUInt32LittleEndian(buffer.Slice(010 * UINT32_BYTE_LENGTH, UINT32_BYTE_LENGTH)),
                    BinaryPrimitives.ReadUInt32LittleEndian(buffer.Slice(011 * UINT32_BYTE_LENGTH, UINT32_BYTE_LENGTH)),
                    BinaryPrimitives.ReadUInt32LittleEndian(buffer.Slice(012 * UINT32_BYTE_LENGTH, UINT32_BYTE_LENGTH)),
                    BinaryPrimitives.ReadUInt32LittleEndian(buffer.Slice(013 * UINT32_BYTE_LENGTH, UINT32_BYTE_LENGTH)),
                    BinaryPrimitives.ReadUInt32LittleEndian(buffer.Slice(014 * UINT32_BYTE_LENGTH, UINT32_BYTE_LENGTH)),
                    BinaryPrimitives.ReadUInt32LittleEndian(buffer.Slice(015 * UINT32_BYTE_LENGTH, UINT32_BYTE_LENGTH)),
                    BinaryPrimitives.ReadUInt32LittleEndian(buffer.Slice(016 * UINT32_BYTE_LENGTH, UINT32_BYTE_LENGTH)),
                    BinaryPrimitives.ReadUInt32LittleEndian(buffer.Slice(017 * UINT32_BYTE_LENGTH, UINT32_BYTE_LENGTH)),
                    BinaryPrimitives.ReadUInt32LittleEndian(buffer.Slice(018 * UINT32_BYTE_LENGTH, UINT32_BYTE_LENGTH)),
                    BinaryPrimitives.ReadUInt32LittleEndian(buffer.Slice(019 * UINT32_BYTE_LENGTH, UINT32_BYTE_LENGTH)),
                    BinaryPrimitives.ReadUInt32LittleEndian(buffer.Slice(020 * UINT32_BYTE_LENGTH, UINT32_BYTE_LENGTH)),
                    BinaryPrimitives.ReadUInt32LittleEndian(buffer.Slice(021 * UINT32_BYTE_LENGTH, UINT32_BYTE_LENGTH)),
                    BinaryPrimitives.ReadUInt32LittleEndian(buffer.Slice(022 * UINT32_BYTE_LENGTH, UINT32_BYTE_LENGTH)),
                    BinaryPrimitives.ReadUInt32LittleEndian(buffer.Slice(023 * UINT32_BYTE_LENGTH, UINT32_BYTE_LENGTH)),
                    BinaryPrimitives.ReadUInt32LittleEndian(buffer.Slice(024 * UINT32_BYTE_LENGTH, UINT32_BYTE_LENGTH)),
                    BinaryPrimitives.ReadUInt32LittleEndian(buffer.Slice(025 * UINT32_BYTE_LENGTH, UINT32_BYTE_LENGTH)),
                    BinaryPrimitives.ReadUInt32LittleEndian(buffer.Slice(026 * UINT32_BYTE_LENGTH, UINT32_BYTE_LENGTH)),
                    BinaryPrimitives.ReadUInt32LittleEndian(buffer.Slice(027 * UINT32_BYTE_LENGTH, UINT32_BYTE_LENGTH)),
                    BinaryPrimitives.ReadUInt32LittleEndian(buffer.Slice(028 * UINT32_BYTE_LENGTH, UINT32_BYTE_LENGTH)),
                    BinaryPrimitives.ReadUInt32LittleEndian(buffer.Slice(029 * UINT32_BYTE_LENGTH, UINT32_BYTE_LENGTH)),
                    BinaryPrimitives.ReadUInt32LittleEndian(buffer.Slice(030 * UINT32_BYTE_LENGTH, UINT32_BYTE_LENGTH)),
                    BinaryPrimitives.ReadUInt32LittleEndian(buffer.Slice(031 * UINT32_BYTE_LENGTH, UINT32_BYTE_LENGTH)),
                    BinaryPrimitives.ReadUInt32LittleEndian(buffer.Slice(032 * UINT32_BYTE_LENGTH, UINT32_BYTE_LENGTH)),
                    BinaryPrimitives.ReadUInt32LittleEndian(buffer.Slice(033 * UINT32_BYTE_LENGTH, UINT32_BYTE_LENGTH)),
                    BinaryPrimitives.ReadUInt32LittleEndian(buffer.Slice(034 * UINT32_BYTE_LENGTH, UINT32_BYTE_LENGTH)),
                    BinaryPrimitives.ReadUInt32LittleEndian(buffer.Slice(035 * UINT32_BYTE_LENGTH, UINT32_BYTE_LENGTH)),
                    BinaryPrimitives.ReadUInt32LittleEndian(buffer.Slice(036 * UINT32_BYTE_LENGTH, UINT32_BYTE_LENGTH)),
                    BinaryPrimitives.ReadUInt32LittleEndian(buffer.Slice(037 * UINT32_BYTE_LENGTH, UINT32_BYTE_LENGTH)),
                    BinaryPrimitives.ReadUInt32LittleEndian(buffer.Slice(038 * UINT32_BYTE_LENGTH, UINT32_BYTE_LENGTH)),
                    BinaryPrimitives.ReadUInt32LittleEndian(buffer.Slice(039 * UINT32_BYTE_LENGTH, UINT32_BYTE_LENGTH)),
                    BinaryPrimitives.ReadUInt32LittleEndian(buffer.Slice(040 * UINT32_BYTE_LENGTH, UINT32_BYTE_LENGTH)),
                    BinaryPrimitives.ReadUInt32LittleEndian(buffer.Slice(041 * UINT32_BYTE_LENGTH, UINT32_BYTE_LENGTH)),
                    BinaryPrimitives.ReadUInt32LittleEndian(buffer.Slice(042 * UINT32_BYTE_LENGTH, UINT32_BYTE_LENGTH)),
                    BinaryPrimitives.ReadUInt32LittleEndian(buffer.Slice(043 * UINT32_BYTE_LENGTH, UINT32_BYTE_LENGTH)),
                    BinaryPrimitives.ReadUInt32LittleEndian(buffer.Slice(044 * UINT32_BYTE_LENGTH, UINT32_BYTE_LENGTH)),
                    BinaryPrimitives.ReadUInt32LittleEndian(buffer.Slice(045 * UINT32_BYTE_LENGTH, UINT32_BYTE_LENGTH)),
                    BinaryPrimitives.ReadUInt32LittleEndian(buffer.Slice(046 * UINT32_BYTE_LENGTH, UINT32_BYTE_LENGTH)),
                    BinaryPrimitives.ReadUInt32LittleEndian(buffer.Slice(047 * UINT32_BYTE_LENGTH, UINT32_BYTE_LENGTH)),
                    BinaryPrimitives.ReadUInt32LittleEndian(buffer.Slice(048 * UINT32_BYTE_LENGTH, UINT32_BYTE_LENGTH)),
                    BinaryPrimitives.ReadUInt32LittleEndian(buffer.Slice(049 * UINT32_BYTE_LENGTH, UINT32_BYTE_LENGTH)),
                    BinaryPrimitives.ReadUInt32LittleEndian(buffer.Slice(050 * UINT32_BYTE_LENGTH, UINT32_BYTE_LENGTH)),
                    BinaryPrimitives.ReadUInt32LittleEndian(buffer.Slice(051 * UINT32_BYTE_LENGTH, UINT32_BYTE_LENGTH)),
                    BinaryPrimitives.ReadUInt32LittleEndian(buffer.Slice(052 * UINT32_BYTE_LENGTH, UINT32_BYTE_LENGTH)),
                    BinaryPrimitives.ReadUInt32LittleEndian(buffer.Slice(053 * UINT32_BYTE_LENGTH, UINT32_BYTE_LENGTH)),
                    BinaryPrimitives.ReadUInt32LittleEndian(buffer.Slice(054 * UINT32_BYTE_LENGTH, UINT32_BYTE_LENGTH)),
                    BinaryPrimitives.ReadUInt32LittleEndian(buffer.Slice(055 * UINT32_BYTE_LENGTH, UINT32_BYTE_LENGTH)),
                    BinaryPrimitives.ReadUInt32LittleEndian(buffer.Slice(056 * UINT32_BYTE_LENGTH, UINT32_BYTE_LENGTH)),
                    BinaryPrimitives.ReadUInt32LittleEndian(buffer.Slice(057 * UINT32_BYTE_LENGTH, UINT32_BYTE_LENGTH)),
                    BinaryPrimitives.ReadUInt32LittleEndian(buffer.Slice(058 * UINT32_BYTE_LENGTH, UINT32_BYTE_LENGTH)),
                    BinaryPrimitives.ReadUInt32LittleEndian(buffer.Slice(059 * UINT32_BYTE_LENGTH, UINT32_BYTE_LENGTH)),
                    BinaryPrimitives.ReadUInt32LittleEndian(buffer.Slice(060 * UINT32_BYTE_LENGTH, UINT32_BYTE_LENGTH)),
                    BinaryPrimitives.ReadUInt32LittleEndian(buffer.Slice(061 * UINT32_BYTE_LENGTH, UINT32_BYTE_LENGTH)),
                    BinaryPrimitives.ReadUInt32LittleEndian(buffer.Slice(062 * UINT32_BYTE_LENGTH, UINT32_BYTE_LENGTH)),
                    BinaryPrimitives.ReadUInt32LittleEndian(buffer.Slice(063 * UINT32_BYTE_LENGTH, UINT32_BYTE_LENGTH)),
                    BinaryPrimitives.ReadUInt32LittleEndian(buffer.Slice(064 * UINT32_BYTE_LENGTH, UINT32_BYTE_LENGTH)),
                    BinaryPrimitives.ReadUInt32LittleEndian(buffer.Slice(065 * UINT32_BYTE_LENGTH, UINT32_BYTE_LENGTH)),
                    BinaryPrimitives.ReadUInt32LittleEndian(buffer.Slice(066 * UINT32_BYTE_LENGTH, UINT32_BYTE_LENGTH)),
                    BinaryPrimitives.ReadUInt32LittleEndian(buffer.Slice(067 * UINT32_BYTE_LENGTH, UINT32_BYTE_LENGTH)),
                    BinaryPrimitives.ReadUInt32LittleEndian(buffer.Slice(068 * UINT32_BYTE_LENGTH, UINT32_BYTE_LENGTH)),
                    BinaryPrimitives.ReadUInt32LittleEndian(buffer.Slice(069 * UINT32_BYTE_LENGTH, UINT32_BYTE_LENGTH)),
                    BinaryPrimitives.ReadUInt32LittleEndian(buffer.Slice(070 * UINT32_BYTE_LENGTH, UINT32_BYTE_LENGTH)),
                    BinaryPrimitives.ReadUInt32LittleEndian(buffer.Slice(071 * UINT32_BYTE_LENGTH, UINT32_BYTE_LENGTH)),
                    BinaryPrimitives.ReadUInt32LittleEndian(buffer.Slice(072 * UINT32_BYTE_LENGTH, UINT32_BYTE_LENGTH)),
                    BinaryPrimitives.ReadUInt32LittleEndian(buffer.Slice(073 * UINT32_BYTE_LENGTH, UINT32_BYTE_LENGTH)),
                    BinaryPrimitives.ReadUInt32LittleEndian(buffer.Slice(074 * UINT32_BYTE_LENGTH, UINT32_BYTE_LENGTH)),
                    BinaryPrimitives.ReadUInt32LittleEndian(buffer.Slice(075 * UINT32_BYTE_LENGTH, UINT32_BYTE_LENGTH)),
                    BinaryPrimitives.ReadUInt32LittleEndian(buffer.Slice(076 * UINT32_BYTE_LENGTH, UINT32_BYTE_LENGTH)),
                    BinaryPrimitives.ReadUInt32LittleEndian(buffer.Slice(077 * UINT32_BYTE_LENGTH, UINT32_BYTE_LENGTH)),
                    BinaryPrimitives.ReadUInt32LittleEndian(buffer.Slice(078 * UINT32_BYTE_LENGTH, UINT32_BYTE_LENGTH)),
                    BinaryPrimitives.ReadUInt32LittleEndian(buffer.Slice(079 * UINT32_BYTE_LENGTH, UINT32_BYTE_LENGTH)),
                    BinaryPrimitives.ReadUInt32LittleEndian(buffer.Slice(080 * UINT32_BYTE_LENGTH, UINT32_BYTE_LENGTH)),
                    BinaryPrimitives.ReadUInt32LittleEndian(buffer.Slice(081 * UINT32_BYTE_LENGTH, UINT32_BYTE_LENGTH)),
                    BinaryPrimitives.ReadUInt32LittleEndian(buffer.Slice(082 * UINT32_BYTE_LENGTH, UINT32_BYTE_LENGTH)),
                    BinaryPrimitives.ReadUInt32LittleEndian(buffer.Slice(083 * UINT32_BYTE_LENGTH, UINT32_BYTE_LENGTH)),
                    BinaryPrimitives.ReadUInt32LittleEndian(buffer.Slice(084 * UINT32_BYTE_LENGTH, UINT32_BYTE_LENGTH)),
                    BinaryPrimitives.ReadUInt32LittleEndian(buffer.Slice(085 * UINT32_BYTE_LENGTH, UINT32_BYTE_LENGTH)),
                    BinaryPrimitives.ReadUInt32LittleEndian(buffer.Slice(086 * UINT32_BYTE_LENGTH, UINT32_BYTE_LENGTH)),
                    BinaryPrimitives.ReadUInt32LittleEndian(buffer.Slice(087 * UINT32_BYTE_LENGTH, UINT32_BYTE_LENGTH)),
                    BinaryPrimitives.ReadUInt32LittleEndian(buffer.Slice(088 * UINT32_BYTE_LENGTH, UINT32_BYTE_LENGTH)),
                    BinaryPrimitives.ReadUInt32LittleEndian(buffer.Slice(089 * UINT32_BYTE_LENGTH, UINT32_BYTE_LENGTH)),
                    BinaryPrimitives.ReadUInt32LittleEndian(buffer.Slice(090 * UINT32_BYTE_LENGTH, UINT32_BYTE_LENGTH)),
                    BinaryPrimitives.ReadUInt32LittleEndian(buffer.Slice(091 * UINT32_BYTE_LENGTH, UINT32_BYTE_LENGTH)),
                    BinaryPrimitives.ReadUInt32LittleEndian(buffer.Slice(092 * UINT32_BYTE_LENGTH, UINT32_BYTE_LENGTH)),
                    BinaryPrimitives.ReadUInt32LittleEndian(buffer.Slice(093 * UINT32_BYTE_LENGTH, UINT32_BYTE_LENGTH)),
                    BinaryPrimitives.ReadUInt32LittleEndian(buffer.Slice(094 * UINT32_BYTE_LENGTH, UINT32_BYTE_LENGTH)),
                    BinaryPrimitives.ReadUInt32LittleEndian(buffer.Slice(095 * UINT32_BYTE_LENGTH, UINT32_BYTE_LENGTH)),
                    BinaryPrimitives.ReadUInt32LittleEndian(buffer.Slice(096 * UINT32_BYTE_LENGTH, UINT32_BYTE_LENGTH)),
                    BinaryPrimitives.ReadUInt32LittleEndian(buffer.Slice(097 * UINT32_BYTE_LENGTH, UINT32_BYTE_LENGTH)),
                    BinaryPrimitives.ReadUInt32LittleEndian(buffer.Slice(098 * UINT32_BYTE_LENGTH, UINT32_BYTE_LENGTH)),
                    BinaryPrimitives.ReadUInt32LittleEndian(buffer.Slice(099 * UINT32_BYTE_LENGTH, UINT32_BYTE_LENGTH)),
                    BinaryPrimitives.ReadUInt32LittleEndian(buffer.Slice(100 * UINT32_BYTE_LENGTH, UINT32_BYTE_LENGTH)),
                    BinaryPrimitives.ReadUInt32LittleEndian(buffer.Slice(101 * UINT32_BYTE_LENGTH, UINT32_BYTE_LENGTH)),
                    BinaryPrimitives.ReadUInt32LittleEndian(buffer.Slice(102 * UINT32_BYTE_LENGTH, UINT32_BYTE_LENGTH)),
                    BinaryPrimitives.ReadUInt32LittleEndian(buffer.Slice(103 * UINT32_BYTE_LENGTH, UINT32_BYTE_LENGTH)),
                    BinaryPrimitives.ReadUInt32LittleEndian(buffer.Slice(104 * UINT32_BYTE_LENGTH, UINT32_BYTE_LENGTH)),
                    BinaryPrimitives.ReadUInt32LittleEndian(buffer.Slice(105 * UINT32_BYTE_LENGTH, UINT32_BYTE_LENGTH)),
                    BinaryPrimitives.ReadUInt32LittleEndian(buffer.Slice(106 * UINT32_BYTE_LENGTH, UINT32_BYTE_LENGTH)),
                    BinaryPrimitives.ReadUInt32LittleEndian(buffer.Slice(107 * UINT32_BYTE_LENGTH, UINT32_BYTE_LENGTH)),
                    BinaryPrimitives.ReadUInt32LittleEndian(buffer.Slice(108 * UINT32_BYTE_LENGTH, UINT32_BYTE_LENGTH)),
                    BinaryPrimitives.ReadUInt32LittleEndian(buffer.Slice(109 * UINT32_BYTE_LENGTH, UINT32_BYTE_LENGTH)),
                    BinaryPrimitives.ReadUInt32LittleEndian(buffer.Slice(110 * UINT32_BYTE_LENGTH, UINT32_BYTE_LENGTH)),
                    BinaryPrimitives.ReadUInt32LittleEndian(buffer.Slice(111 * UINT32_BYTE_LENGTH, UINT32_BYTE_LENGTH)),
                    BinaryPrimitives.ReadUInt32LittleEndian(buffer.Slice(112 * UINT32_BYTE_LENGTH, UINT32_BYTE_LENGTH)),
                    BinaryPrimitives.ReadUInt32LittleEndian(buffer.Slice(113 * UINT32_BYTE_LENGTH, UINT32_BYTE_LENGTH)),
                    BinaryPrimitives.ReadUInt32LittleEndian(buffer.Slice(114 * UINT32_BYTE_LENGTH, UINT32_BYTE_LENGTH)),
                    BinaryPrimitives.ReadUInt32LittleEndian(buffer.Slice(115 * UINT32_BYTE_LENGTH, UINT32_BYTE_LENGTH)),
                    BinaryPrimitives.ReadUInt32LittleEndian(buffer.Slice(116 * UINT32_BYTE_LENGTH, UINT32_BYTE_LENGTH)),
                    BinaryPrimitives.ReadUInt32LittleEndian(buffer.Slice(117 * UINT32_BYTE_LENGTH, UINT32_BYTE_LENGTH)),
                    BinaryPrimitives.ReadUInt32LittleEndian(buffer.Slice(118 * UINT32_BYTE_LENGTH, UINT32_BYTE_LENGTH)),
                    BinaryPrimitives.ReadUInt32LittleEndian(buffer.Slice(119 * UINT32_BYTE_LENGTH, UINT32_BYTE_LENGTH)),
                    BinaryPrimitives.ReadUInt32LittleEndian(buffer.Slice(120 * UINT32_BYTE_LENGTH, UINT32_BYTE_LENGTH)),
                    BinaryPrimitives.ReadUInt32LittleEndian(buffer.Slice(121 * UINT32_BYTE_LENGTH, UINT32_BYTE_LENGTH)),
                    BinaryPrimitives.ReadUInt32LittleEndian(buffer.Slice(122 * UINT32_BYTE_LENGTH, UINT32_BYTE_LENGTH)),
                    BinaryPrimitives.ReadUInt32LittleEndian(buffer.Slice(123 * UINT32_BYTE_LENGTH, UINT32_BYTE_LENGTH)),
                    BinaryPrimitives.ReadUInt32LittleEndian(buffer.Slice(124 * UINT32_BYTE_LENGTH, UINT32_BYTE_LENGTH)),
                    BinaryPrimitives.ReadUInt32LittleEndian(buffer.Slice(125 * UINT32_BYTE_LENGTH, UINT32_BYTE_LENGTH)),
                    BinaryPrimitives.ReadUInt32LittleEndian(buffer.Slice(126 * UINT32_BYTE_LENGTH, UINT32_BYTE_LENGTH)),
                    BinaryPrimitives.ReadUInt32LittleEndian(buffer.Slice(127 * UINT32_BYTE_LENGTH, UINT32_BYTE_LENGTH)),
                ]);
            }
        }

        private static IEnumerable<BigInteger> GenerateUInt2048TestDataForPerformance(RandomNumberGenerator randomNumberGenerator)
        {
            const int BIT_LENGTH = 2048;
            const int BYTE_LENGTH = BIT_LENGTH / 8;
            const int UINT32_BYTE_LENGTH = sizeof(uint);

            var values = new HashSet<BigInteger>();
            var buffer = new byte[BYTE_LENGTH];
            var border = BigInteger.One << (BIT_LENGTH - 32);
            while (values.Count < _PRIMARY_DATA_COUNT)
            {
                randomNumberGenerator.GetBytes(buffer);
                var value = FromByteArrayToBigInteger(buffer);
                if (value >= border)
                    _ = values.Add(value);
            }

            foreach (var value in values)
                yield return value;

            static BigInteger FromByteArrayToBigInteger(ReadOnlySpan<byte> buffer)
            {
                return FromUInt32ArrayToBigInteger(
                [
                    BinaryPrimitives.ReadUInt32LittleEndian(buffer[..UINT32_BYTE_LENGTH]),
                    BinaryPrimitives.ReadUInt32LittleEndian(buffer.Slice(01 * UINT32_BYTE_LENGTH, UINT32_BYTE_LENGTH)),
                    BinaryPrimitives.ReadUInt32LittleEndian(buffer.Slice(02 * UINT32_BYTE_LENGTH, UINT32_BYTE_LENGTH)),
                    BinaryPrimitives.ReadUInt32LittleEndian(buffer.Slice(03 * UINT32_BYTE_LENGTH, UINT32_BYTE_LENGTH)),
                    BinaryPrimitives.ReadUInt32LittleEndian(buffer.Slice(04 * UINT32_BYTE_LENGTH, UINT32_BYTE_LENGTH)),
                    BinaryPrimitives.ReadUInt32LittleEndian(buffer.Slice(05 * UINT32_BYTE_LENGTH, UINT32_BYTE_LENGTH)),
                    BinaryPrimitives.ReadUInt32LittleEndian(buffer.Slice(06 * UINT32_BYTE_LENGTH, UINT32_BYTE_LENGTH)),
                    BinaryPrimitives.ReadUInt32LittleEndian(buffer.Slice(07 * UINT32_BYTE_LENGTH, UINT32_BYTE_LENGTH)),
                    BinaryPrimitives.ReadUInt32LittleEndian(buffer.Slice(08 * UINT32_BYTE_LENGTH, UINT32_BYTE_LENGTH)),
                    BinaryPrimitives.ReadUInt32LittleEndian(buffer.Slice(09 * UINT32_BYTE_LENGTH, UINT32_BYTE_LENGTH)),
                    BinaryPrimitives.ReadUInt32LittleEndian(buffer.Slice(10 * UINT32_BYTE_LENGTH, UINT32_BYTE_LENGTH)),
                    BinaryPrimitives.ReadUInt32LittleEndian(buffer.Slice(11 * UINT32_BYTE_LENGTH, UINT32_BYTE_LENGTH)),
                    BinaryPrimitives.ReadUInt32LittleEndian(buffer.Slice(12 * UINT32_BYTE_LENGTH, UINT32_BYTE_LENGTH)),
                    BinaryPrimitives.ReadUInt32LittleEndian(buffer.Slice(13 * UINT32_BYTE_LENGTH, UINT32_BYTE_LENGTH)),
                    BinaryPrimitives.ReadUInt32LittleEndian(buffer.Slice(14 * UINT32_BYTE_LENGTH, UINT32_BYTE_LENGTH)),
                    BinaryPrimitives.ReadUInt32LittleEndian(buffer.Slice(15 * UINT32_BYTE_LENGTH, UINT32_BYTE_LENGTH)),
                    BinaryPrimitives.ReadUInt32LittleEndian(buffer.Slice(16 * UINT32_BYTE_LENGTH, UINT32_BYTE_LENGTH)),
                    BinaryPrimitives.ReadUInt32LittleEndian(buffer.Slice(17 * UINT32_BYTE_LENGTH, UINT32_BYTE_LENGTH)),
                    BinaryPrimitives.ReadUInt32LittleEndian(buffer.Slice(18 * UINT32_BYTE_LENGTH, UINT32_BYTE_LENGTH)),
                    BinaryPrimitives.ReadUInt32LittleEndian(buffer.Slice(19 * UINT32_BYTE_LENGTH, UINT32_BYTE_LENGTH)),
                    BinaryPrimitives.ReadUInt32LittleEndian(buffer.Slice(20 * UINT32_BYTE_LENGTH, UINT32_BYTE_LENGTH)),
                    BinaryPrimitives.ReadUInt32LittleEndian(buffer.Slice(21 * UINT32_BYTE_LENGTH, UINT32_BYTE_LENGTH)),
                    BinaryPrimitives.ReadUInt32LittleEndian(buffer.Slice(22 * UINT32_BYTE_LENGTH, UINT32_BYTE_LENGTH)),
                    BinaryPrimitives.ReadUInt32LittleEndian(buffer.Slice(23 * UINT32_BYTE_LENGTH, UINT32_BYTE_LENGTH)),
                    BinaryPrimitives.ReadUInt32LittleEndian(buffer.Slice(24 * UINT32_BYTE_LENGTH, UINT32_BYTE_LENGTH)),
                    BinaryPrimitives.ReadUInt32LittleEndian(buffer.Slice(25 * UINT32_BYTE_LENGTH, UINT32_BYTE_LENGTH)),
                    BinaryPrimitives.ReadUInt32LittleEndian(buffer.Slice(26 * UINT32_BYTE_LENGTH, UINT32_BYTE_LENGTH)),
                    BinaryPrimitives.ReadUInt32LittleEndian(buffer.Slice(27 * UINT32_BYTE_LENGTH, UINT32_BYTE_LENGTH)),
                    BinaryPrimitives.ReadUInt32LittleEndian(buffer.Slice(28 * UINT32_BYTE_LENGTH, UINT32_BYTE_LENGTH)),
                    BinaryPrimitives.ReadUInt32LittleEndian(buffer.Slice(29 * UINT32_BYTE_LENGTH, UINT32_BYTE_LENGTH)),
                    BinaryPrimitives.ReadUInt32LittleEndian(buffer.Slice(30 * UINT32_BYTE_LENGTH, UINT32_BYTE_LENGTH)),
                    BinaryPrimitives.ReadUInt32LittleEndian(buffer.Slice(31 * UINT32_BYTE_LENGTH, UINT32_BYTE_LENGTH)),
                    BinaryPrimitives.ReadUInt32LittleEndian(buffer.Slice(32 * UINT32_BYTE_LENGTH, UINT32_BYTE_LENGTH)),
                    BinaryPrimitives.ReadUInt32LittleEndian(buffer.Slice(33 * UINT32_BYTE_LENGTH, UINT32_BYTE_LENGTH)),
                    BinaryPrimitives.ReadUInt32LittleEndian(buffer.Slice(34 * UINT32_BYTE_LENGTH, UINT32_BYTE_LENGTH)),
                    BinaryPrimitives.ReadUInt32LittleEndian(buffer.Slice(35 * UINT32_BYTE_LENGTH, UINT32_BYTE_LENGTH)),
                    BinaryPrimitives.ReadUInt32LittleEndian(buffer.Slice(36 * UINT32_BYTE_LENGTH, UINT32_BYTE_LENGTH)),
                    BinaryPrimitives.ReadUInt32LittleEndian(buffer.Slice(37 * UINT32_BYTE_LENGTH, UINT32_BYTE_LENGTH)),
                    BinaryPrimitives.ReadUInt32LittleEndian(buffer.Slice(38 * UINT32_BYTE_LENGTH, UINT32_BYTE_LENGTH)),
                    BinaryPrimitives.ReadUInt32LittleEndian(buffer.Slice(39 * UINT32_BYTE_LENGTH, UINT32_BYTE_LENGTH)),
                    BinaryPrimitives.ReadUInt32LittleEndian(buffer.Slice(40 * UINT32_BYTE_LENGTH, UINT32_BYTE_LENGTH)),
                    BinaryPrimitives.ReadUInt32LittleEndian(buffer.Slice(41 * UINT32_BYTE_LENGTH, UINT32_BYTE_LENGTH)),
                    BinaryPrimitives.ReadUInt32LittleEndian(buffer.Slice(42 * UINT32_BYTE_LENGTH, UINT32_BYTE_LENGTH)),
                    BinaryPrimitives.ReadUInt32LittleEndian(buffer.Slice(43 * UINT32_BYTE_LENGTH, UINT32_BYTE_LENGTH)),
                    BinaryPrimitives.ReadUInt32LittleEndian(buffer.Slice(44 * UINT32_BYTE_LENGTH, UINT32_BYTE_LENGTH)),
                    BinaryPrimitives.ReadUInt32LittleEndian(buffer.Slice(45 * UINT32_BYTE_LENGTH, UINT32_BYTE_LENGTH)),
                    BinaryPrimitives.ReadUInt32LittleEndian(buffer.Slice(46 * UINT32_BYTE_LENGTH, UINT32_BYTE_LENGTH)),
                    BinaryPrimitives.ReadUInt32LittleEndian(buffer.Slice(47 * UINT32_BYTE_LENGTH, UINT32_BYTE_LENGTH)),
                    BinaryPrimitives.ReadUInt32LittleEndian(buffer.Slice(48 * UINT32_BYTE_LENGTH, UINT32_BYTE_LENGTH)),
                    BinaryPrimitives.ReadUInt32LittleEndian(buffer.Slice(49 * UINT32_BYTE_LENGTH, UINT32_BYTE_LENGTH)),
                    BinaryPrimitives.ReadUInt32LittleEndian(buffer.Slice(50 * UINT32_BYTE_LENGTH, UINT32_BYTE_LENGTH)),
                    BinaryPrimitives.ReadUInt32LittleEndian(buffer.Slice(51 * UINT32_BYTE_LENGTH, UINT32_BYTE_LENGTH)),
                    BinaryPrimitives.ReadUInt32LittleEndian(buffer.Slice(52 * UINT32_BYTE_LENGTH, UINT32_BYTE_LENGTH)),
                    BinaryPrimitives.ReadUInt32LittleEndian(buffer.Slice(53 * UINT32_BYTE_LENGTH, UINT32_BYTE_LENGTH)),
                    BinaryPrimitives.ReadUInt32LittleEndian(buffer.Slice(54 * UINT32_BYTE_LENGTH, UINT32_BYTE_LENGTH)),
                    BinaryPrimitives.ReadUInt32LittleEndian(buffer.Slice(55 * UINT32_BYTE_LENGTH, UINT32_BYTE_LENGTH)),
                    BinaryPrimitives.ReadUInt32LittleEndian(buffer.Slice(56 * UINT32_BYTE_LENGTH, UINT32_BYTE_LENGTH)),
                    BinaryPrimitives.ReadUInt32LittleEndian(buffer.Slice(57 * UINT32_BYTE_LENGTH, UINT32_BYTE_LENGTH)),
                    BinaryPrimitives.ReadUInt32LittleEndian(buffer.Slice(58 * UINT32_BYTE_LENGTH, UINT32_BYTE_LENGTH)),
                    BinaryPrimitives.ReadUInt32LittleEndian(buffer.Slice(59 * UINT32_BYTE_LENGTH, UINT32_BYTE_LENGTH)),
                    BinaryPrimitives.ReadUInt32LittleEndian(buffer.Slice(60 * UINT32_BYTE_LENGTH, UINT32_BYTE_LENGTH)),
                    BinaryPrimitives.ReadUInt32LittleEndian(buffer.Slice(61 * UINT32_BYTE_LENGTH, UINT32_BYTE_LENGTH)),
                    BinaryPrimitives.ReadUInt32LittleEndian(buffer.Slice(62 * UINT32_BYTE_LENGTH, UINT32_BYTE_LENGTH)),
                    BinaryPrimitives.ReadUInt32LittleEndian(buffer.Slice(63 * UINT32_BYTE_LENGTH, UINT32_BYTE_LENGTH)),
                ]);
            }
        }

        private static IEnumerable<BigInteger> GenerateUInt1024TestDataForPerformance(RandomNumberGenerator randomNumberGenerator)
        {
            const int BIT_LENGTH = 1024;
            const int BYTE_LENGTH = BIT_LENGTH / 8;
            const int UINT32_BYTE_LENGTH = sizeof(uint);

            var values = new HashSet<BigInteger>();
            var buffer = new byte[BYTE_LENGTH];
            var border = BigInteger.One << (BIT_LENGTH - 32);
            while (values.Count < _PRIMARY_DATA_COUNT)
            {
                randomNumberGenerator.GetBytes(buffer);
                var value = FromByteArrayToBigInteger(buffer);
                if (value >= border)
                    _ = values.Add(value);
            }

            foreach (var value in values)
                yield return value;

            static BigInteger FromByteArrayToBigInteger(ReadOnlySpan<byte> buffer)
            {
                return FromUInt32ArrayToBigInteger(
                [
                    BinaryPrimitives.ReadUInt32LittleEndian(buffer[..UINT32_BYTE_LENGTH]),
                    BinaryPrimitives.ReadUInt32LittleEndian(buffer.Slice(01 * UINT32_BYTE_LENGTH, UINT32_BYTE_LENGTH)),
                    BinaryPrimitives.ReadUInt32LittleEndian(buffer.Slice(02 * UINT32_BYTE_LENGTH, UINT32_BYTE_LENGTH)),
                    BinaryPrimitives.ReadUInt32LittleEndian(buffer.Slice(03 * UINT32_BYTE_LENGTH, UINT32_BYTE_LENGTH)),
                    BinaryPrimitives.ReadUInt32LittleEndian(buffer.Slice(04 * UINT32_BYTE_LENGTH, UINT32_BYTE_LENGTH)),
                    BinaryPrimitives.ReadUInt32LittleEndian(buffer.Slice(05 * UINT32_BYTE_LENGTH, UINT32_BYTE_LENGTH)),
                    BinaryPrimitives.ReadUInt32LittleEndian(buffer.Slice(06 * UINT32_BYTE_LENGTH, UINT32_BYTE_LENGTH)),
                    BinaryPrimitives.ReadUInt32LittleEndian(buffer.Slice(07 * UINT32_BYTE_LENGTH, UINT32_BYTE_LENGTH)),
                    BinaryPrimitives.ReadUInt32LittleEndian(buffer.Slice(08 * UINT32_BYTE_LENGTH, UINT32_BYTE_LENGTH)),
                    BinaryPrimitives.ReadUInt32LittleEndian(buffer.Slice(09 * UINT32_BYTE_LENGTH, UINT32_BYTE_LENGTH)),
                    BinaryPrimitives.ReadUInt32LittleEndian(buffer.Slice(10 * UINT32_BYTE_LENGTH, UINT32_BYTE_LENGTH)),
                    BinaryPrimitives.ReadUInt32LittleEndian(buffer.Slice(11 * UINT32_BYTE_LENGTH, UINT32_BYTE_LENGTH)),
                    BinaryPrimitives.ReadUInt32LittleEndian(buffer.Slice(12 * UINT32_BYTE_LENGTH, UINT32_BYTE_LENGTH)),
                    BinaryPrimitives.ReadUInt32LittleEndian(buffer.Slice(13 * UINT32_BYTE_LENGTH, UINT32_BYTE_LENGTH)),
                    BinaryPrimitives.ReadUInt32LittleEndian(buffer.Slice(14 * UINT32_BYTE_LENGTH, UINT32_BYTE_LENGTH)),
                    BinaryPrimitives.ReadUInt32LittleEndian(buffer.Slice(15 * UINT32_BYTE_LENGTH, UINT32_BYTE_LENGTH)),
                    BinaryPrimitives.ReadUInt32LittleEndian(buffer.Slice(16 * UINT32_BYTE_LENGTH, UINT32_BYTE_LENGTH)),
                    BinaryPrimitives.ReadUInt32LittleEndian(buffer.Slice(17 * UINT32_BYTE_LENGTH, UINT32_BYTE_LENGTH)),
                    BinaryPrimitives.ReadUInt32LittleEndian(buffer.Slice(18 * UINT32_BYTE_LENGTH, UINT32_BYTE_LENGTH)),
                    BinaryPrimitives.ReadUInt32LittleEndian(buffer.Slice(19 * UINT32_BYTE_LENGTH, UINT32_BYTE_LENGTH)),
                    BinaryPrimitives.ReadUInt32LittleEndian(buffer.Slice(20 * UINT32_BYTE_LENGTH, UINT32_BYTE_LENGTH)),
                    BinaryPrimitives.ReadUInt32LittleEndian(buffer.Slice(21 * UINT32_BYTE_LENGTH, UINT32_BYTE_LENGTH)),
                    BinaryPrimitives.ReadUInt32LittleEndian(buffer.Slice(22 * UINT32_BYTE_LENGTH, UINT32_BYTE_LENGTH)),
                    BinaryPrimitives.ReadUInt32LittleEndian(buffer.Slice(23 * UINT32_BYTE_LENGTH, UINT32_BYTE_LENGTH)),
                    BinaryPrimitives.ReadUInt32LittleEndian(buffer.Slice(24 * UINT32_BYTE_LENGTH, UINT32_BYTE_LENGTH)),
                    BinaryPrimitives.ReadUInt32LittleEndian(buffer.Slice(25 * UINT32_BYTE_LENGTH, UINT32_BYTE_LENGTH)),
                    BinaryPrimitives.ReadUInt32LittleEndian(buffer.Slice(26 * UINT32_BYTE_LENGTH, UINT32_BYTE_LENGTH)),
                    BinaryPrimitives.ReadUInt32LittleEndian(buffer.Slice(27 * UINT32_BYTE_LENGTH, UINT32_BYTE_LENGTH)),
                    BinaryPrimitives.ReadUInt32LittleEndian(buffer.Slice(28 * UINT32_BYTE_LENGTH, UINT32_BYTE_LENGTH)),
                    BinaryPrimitives.ReadUInt32LittleEndian(buffer.Slice(29 * UINT32_BYTE_LENGTH, UINT32_BYTE_LENGTH)),
                    BinaryPrimitives.ReadUInt32LittleEndian(buffer.Slice(30 * UINT32_BYTE_LENGTH, UINT32_BYTE_LENGTH)),
                    BinaryPrimitives.ReadUInt32LittleEndian(buffer.Slice(31 * UINT32_BYTE_LENGTH, UINT32_BYTE_LENGTH)),
                ]);
            }
        }

        private static IEnumerable<BigInteger> GenerateUInt512TestDataForPerformance(RandomNumberGenerator randomNumberGenerator)
        {
            const int BIT_LENGTH = 512;
            const int BYTE_LENGTH = BIT_LENGTH / 8;
            const int UINT32_BYTE_LENGTH = sizeof(uint);

            var values = new HashSet<BigInteger>();
            var buffer = new byte[BYTE_LENGTH];
            var border = BigInteger.One << (BIT_LENGTH - 32);
            while (values.Count < _PRIMARY_DATA_COUNT)
            {
                randomNumberGenerator.GetBytes(buffer);
                var value = FromByteArrayToBigInteger(buffer);
                if (value >= border)
                    _ = values.Add(value);
            }

            foreach (var value in values)
                yield return value;

            static BigInteger FromByteArrayToBigInteger(ReadOnlySpan<byte> buffer)
            {
                return FromUInt32ArrayToBigInteger(
                [
                    BinaryPrimitives.ReadUInt32LittleEndian(buffer[..UINT32_BYTE_LENGTH]),
                    BinaryPrimitives.ReadUInt32LittleEndian(buffer.Slice(01 * UINT32_BYTE_LENGTH, UINT32_BYTE_LENGTH)),
                    BinaryPrimitives.ReadUInt32LittleEndian(buffer.Slice(02 * UINT32_BYTE_LENGTH, UINT32_BYTE_LENGTH)),
                    BinaryPrimitives.ReadUInt32LittleEndian(buffer.Slice(03 * UINT32_BYTE_LENGTH, UINT32_BYTE_LENGTH)),
                    BinaryPrimitives.ReadUInt32LittleEndian(buffer.Slice(04 * UINT32_BYTE_LENGTH, UINT32_BYTE_LENGTH)),
                    BinaryPrimitives.ReadUInt32LittleEndian(buffer.Slice(05 * UINT32_BYTE_LENGTH, UINT32_BYTE_LENGTH)),
                    BinaryPrimitives.ReadUInt32LittleEndian(buffer.Slice(06 * UINT32_BYTE_LENGTH, UINT32_BYTE_LENGTH)),
                    BinaryPrimitives.ReadUInt32LittleEndian(buffer.Slice(07 * UINT32_BYTE_LENGTH, UINT32_BYTE_LENGTH)),
                    BinaryPrimitives.ReadUInt32LittleEndian(buffer.Slice(08 * UINT32_BYTE_LENGTH, UINT32_BYTE_LENGTH)),
                    BinaryPrimitives.ReadUInt32LittleEndian(buffer.Slice(09 * UINT32_BYTE_LENGTH, UINT32_BYTE_LENGTH)),
                    BinaryPrimitives.ReadUInt32LittleEndian(buffer.Slice(10 * UINT32_BYTE_LENGTH, UINT32_BYTE_LENGTH)),
                    BinaryPrimitives.ReadUInt32LittleEndian(buffer.Slice(11 * UINT32_BYTE_LENGTH, UINT32_BYTE_LENGTH)),
                    BinaryPrimitives.ReadUInt32LittleEndian(buffer.Slice(12 * UINT32_BYTE_LENGTH, UINT32_BYTE_LENGTH)),
                    BinaryPrimitives.ReadUInt32LittleEndian(buffer.Slice(13 * UINT32_BYTE_LENGTH, UINT32_BYTE_LENGTH)),
                    BinaryPrimitives.ReadUInt32LittleEndian(buffer.Slice(14 * UINT32_BYTE_LENGTH, UINT32_BYTE_LENGTH)),
                    BinaryPrimitives.ReadUInt32LittleEndian(buffer.Slice(15 * UINT32_BYTE_LENGTH, UINT32_BYTE_LENGTH)),
                ]);
            }
        }

        private static IEnumerable<BigInteger> GenerateUInt256TestDataForPerformance(RandomNumberGenerator randomNumberGenerator)
        {
            const int BIT_LENGTH = 256;
            const int BYTE_LENGTH = BIT_LENGTH / 8;
            const int UINT32_BYTE_LENGTH = sizeof(uint);

            var values = new HashSet<BigInteger>();
            var buffer = new byte[BYTE_LENGTH];
            var border = BigInteger.One << (BIT_LENGTH - 32);
            while (values.Count < _PRIMARY_DATA_COUNT)
            {
                randomNumberGenerator.GetBytes(buffer);
                var value = FromByteArrayToBigInteger(buffer);
                if (value >= border)
                    _ = values.Add(value);
            }

            foreach (var value in values)
                yield return value;

            static BigInteger FromByteArrayToBigInteger(ReadOnlySpan<byte> buffer)
            {
                return FromUInt32ArrayToBigInteger(
                [
                    BinaryPrimitives.ReadUInt32LittleEndian(buffer[..UINT32_BYTE_LENGTH]),
                    BinaryPrimitives.ReadUInt32LittleEndian(buffer.Slice(1 * UINT32_BYTE_LENGTH, UINT32_BYTE_LENGTH)),
                    BinaryPrimitives.ReadUInt32LittleEndian(buffer.Slice(2 * UINT32_BYTE_LENGTH, UINT32_BYTE_LENGTH)),
                    BinaryPrimitives.ReadUInt32LittleEndian(buffer.Slice(3 * UINT32_BYTE_LENGTH, UINT32_BYTE_LENGTH)),
                    BinaryPrimitives.ReadUInt32LittleEndian(buffer.Slice(4 * UINT32_BYTE_LENGTH, UINT32_BYTE_LENGTH)),
                    BinaryPrimitives.ReadUInt32LittleEndian(buffer.Slice(5 * UINT32_BYTE_LENGTH, UINT32_BYTE_LENGTH)),
                    BinaryPrimitives.ReadUInt32LittleEndian(buffer.Slice(6 * UINT32_BYTE_LENGTH, UINT32_BYTE_LENGTH)),
                    BinaryPrimitives.ReadUInt32LittleEndian(buffer.Slice(7 * UINT32_BYTE_LENGTH, UINT32_BYTE_LENGTH)),
                ]);
            }
        }

        private static IEnumerable<UInt128> GenerateUInt128TestDataForPerformance(RandomNumberGenerator randomNumberGenerator)
        {
            const int BIT_LENGTH = 128;
            const int BYTE_LENGTH = BIT_LENGTH / 8;

            var values = new HashSet<UInt128>();
            var buffer = new byte[BYTE_LENGTH];
            while (values.Count < _PRIMARY_DATA_COUNT)
            {
                randomNumberGenerator.GetBytes(buffer);
                var value = BinaryPrimitives.ReadUInt128LittleEndian(buffer);
                if (value >= (1U << 32) * 3)
                    _ = values.Add(value);
            }

            foreach (var value in values)
                yield return value;
        }

        private static IEnumerable<ulong> GenerateUInt64TestDataForPerformance(RandomNumberGenerator randomNumberGenerator)
        {
            const int BIT_LENGTH = 64;
            const int BYTE_LENGTH = BIT_LENGTH / 8;

            var values = new HashSet<ulong>();
            var buffer = new byte[BYTE_LENGTH];
            while (values.Count < _PRIMARY_DATA_COUNT)
            {
                randomNumberGenerator.GetBytes(buffer);
                var value = BinaryPrimitives.ReadUInt64LittleEndian(buffer);
                if (value >= (1U << 32))
                    _ = values.Add(value);
            }

            foreach (var value in values)
                yield return value;
        }

        private static IEnumerable<uint> GenerateUInt32TestDataForPerformance(RandomNumberGenerator randomNumberGenerator)
        {
            const int BIT_LENGTH = 32;
            const int BYTE_LENGTH = BIT_LENGTH / 8;

            var values = new HashSet<uint>();
            var buffer = new byte[BYTE_LENGTH];
            while (values.Count < _PRIMARY_DATA_COUNT)
            {
                randomNumberGenerator.GetBytes(buffer);
                var value = BinaryPrimitives.ReadUInt32LittleEndian(buffer);
                if (value > ushort.MaxValue)
                    _ = values.Add(value);
            }

            foreach (var value in values)
                yield return value;
        }

        private static IEnumerable<ushort> GenerateUInt16TestDataForPerformance(RandomNumberGenerator randomNumberGenerator)
        {
            const int BIT_LENGTH = 16;
            const int BYTE_LENGTH = BIT_LENGTH / 8;

            var values = new HashSet<ushort>();
            var buffer = new byte[BYTE_LENGTH];
            while (values.Count < _PRIMARY_DATA_COUNT)
            {
                randomNumberGenerator.GetBytes(buffer);
                var value = BinaryPrimitives.ReadUInt16LittleEndian(buffer);
                if (value > byte.MaxValue)
                    _ = values.Add(value);
            }

            foreach (var value in values)
                yield return value;
        }

        private static IEnumerable<byte> GenerateByteTestDataForPerformance(RandomNumberGenerator randomNumberGenerator)
        {
            const int BIT_LENGTH = 8;
            const int BYTE_LENGTH = BIT_LENGTH / 8;

            var values = new HashSet<byte>();
            var buffer = new byte[BYTE_LENGTH];
            while (values.Count < _PRIMARY_DATA_COUNT)
            {
                randomNumberGenerator.GetBytes(buffer);
                var value = buffer[0];
                if (value > 0)
                    _ = values.Add(value);
            }

            foreach (var value in values)
                yield return value;
        }

        private static BigInteger FromUInt32ArrayToBigInteger(ReadOnlySpan<uint> value)
        {
            var result = BigInteger.Zero;
            for (var index = value.Length - 1; index >= 0; --index)
            {
                result <<= 32;
                result |= value[index];
            }

            return result;
        }
    }
}
