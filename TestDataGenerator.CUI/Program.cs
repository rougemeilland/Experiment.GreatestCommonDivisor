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
