using System;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace Experiment.CUI
{
    internal static class UInt32ArrayExtensions
    {
        public static string ToFormattedString(this Span<uint> value)
            => ((ReadOnlySpan<uint>)value).ToFormattedString();

        public static string ToFormattedString(this ReadOnlySpan<uint> value)
        {
            var valueTexts = new List<string>();
            for (var index = 0; index < value.Length; ++index)
                valueTexts.Add($"0x{value[index]:x8}");
            return $"[{string.Join(", ", valueTexts)}]";
        }

        public static BigInteger ToBigInteger(this Span<uint> value)
            => ((ReadOnlySpan<uint>)value).ToBigInteger();

        public static BigInteger ToBigInteger(this ReadOnlySpan<uint> value)
        {
            var sizeOfValue = value.Length * sizeof(uint);
            Span<byte> buffer = stackalloc byte[sizeOfValue + 1];
            Unsafe.CopyBlockUnaligned(ref buffer[0], ref Unsafe.As<uint, byte>(ref Unsafe.AsRef(in value[0])), (uint)sizeOfValue);
            buffer[sizeOfValue] = 0;
            return new BigInteger(buffer);
        }

        public static bool DataEquals(this Span<uint> left, ReadOnlySpan<uint> right)
            => ((ReadOnlySpan<uint>)left).DataEquals(right);

        public static bool DataEquals(this ReadOnlySpan<uint> left, ReadOnlySpan<uint> right)
        {
            while (left.Length > 0 && left[^1] == 0)
                left = left[..^1];
            while (right.Length > 0 && right[^1] == 0)
                right = right[..^1];
            return left.SequenceEqual(right);
        }

        public static int DataCompareTo(this Span<uint> left, ReadOnlySpan<uint> right)
            => ((ReadOnlySpan<uint>)left).DataCompareTo(right);

        public static int DataCompareTo(this ReadOnlySpan<uint> left, ReadOnlySpan<uint> right)
        {
            while (left.Length > 0 && left[^1] == 0)
                left = left[..^1];
            while (right.Length > 0 && right[^1] == 0)
                right = right[..^1];
            int c;
            if ((c = left.Length.CompareTo(right.Length)) != 0)
                return c;
            for (var index = left.Length - 1; index >= 0; --index)
            {
                if ((c = left[index].CompareTo(right[index])) != 0)
                    return c;
            }

            return 0;
        }
    }
}
