using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.Arm;
using System.Runtime.Intrinsics.Wasm;
using System.Runtime.Intrinsics.X86;
using System.Security.Cryptography;
using System.Xml.Linq;

namespace Experiment.Vector.CUI
{
    internal static class Program
    {
        private static void Main()
        {
            Console.WriteLine($"Vector512.IsHardwareAccelerated={Vector512.IsHardwareAccelerated}");
            Console.WriteLine($"Vector256.IsHardwareAccelerated={Vector256.IsHardwareAccelerated}");
            Console.WriteLine($"Vector128.IsHardwareAccelerated={Vector128.IsHardwareAccelerated}");
            Console.WriteLine($"Vector.IsHardwareAccelerated={System.Numerics.Vector.IsHardwareAccelerated}");
            Console.WriteLine($"Avx512F.IsSupported={Avx512F.IsSupported}");
            Console.WriteLine($"Avx2.IsSupported={Avx2.IsSupported}");
            Console.WriteLine($"Sse2.IsSupported={Sse2.IsSupported}");
            Console.WriteLine($"AdvSimd.IsSupported={AdvSimd.IsSupported}");
            Console.WriteLine($"PackedSimd.IsSupported={PackedSimd.IsSupported}");
            Console.WriteLine($"Vector512<uint>.Count={Vector512<uint>.Count}");
            Console.WriteLine($"Vector256<uint>.Count={Vector256<uint>.Count}");
            Console.WriteLine($"Vector128<uint>.Count={Vector128<uint>.Count}");
            Console.WriteLine($"Vector<uint>.Count={Vector<uint>.Count}");

            var source = new uint[1024 / 32];
            using (var randomNumberGenerator = RandomNumberGenerator.Create())
            {
                var byteBuffer = new byte[source.Length * sizeof(uint)];
                randomNumberGenerator.GetBytes(byteBuffer);
                Unsafe.CopyBlockUnaligned(ref Unsafe.As<uint, byte>(ref source[0]), ref byteBuffer[0], (uint)byteBuffer.Length);
            }

            Console.WriteLine($"source={FormatUInt32Array(source)}");
            System.Diagnostics.Debug.WriteLine($"source={FormatUInt32Array(source)}");

            //ShiftRight8BySse2(source);

            Console.WriteLine($"result={FormatUInt32Array(source)}");
            System.Diagnostics.Debug.WriteLine($"result={FormatUInt32Array(source)}");

            Console.WriteLine("Completed.");
            Console.Beep();
            _ = Console.ReadLine();
        }

#if false
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        private static void ShiftRightTry9(Span<uint> value, int bitCount)
        {
            System.Diagnostics.Debug.Assert(Vector128.IsHardwareAccelerated == true);
            System.Diagnostics.Debug.Assert(Vector128<uint>.Count == 4);
            System.Diagnostics.Debug.Assert(Sse2.IsSupported == true);

            var offset = bitCount >> 5;
            var rightShiftCount = bitCount & 31;
            if (rightShiftCount == 0)
            {
                value[offset..].CopyTo(value[..^offset]);
                value[^offset..].Clear();
            }
            else
            {
                unsafe
                {
                    fixed (uint* buffer = value)
                    {
                        if (Avx512F.IsSupported)
                            OperateVectorByAvx512F(buffer + offset, buffer, value.Length - offset, rightShiftCount);
                        if (Avx2.IsSupported)
                            OperateVectorByAvx2(buffer + offset, buffer, value.Length - offset, rightShiftCount);
                        else if (Sse2.IsSupported)
                            OperateVectorBySse2(buffer + offset, buffer, value.Length - offset, rightShiftCount);
                        else if (AdvSimd.IsSupported)
                            OperateVectorByAdvSimd(buffer + offset, buffer, value.Length - offset, rightShiftCount);
                        else if (PackedSimd.IsSupported)
                            OperateVectorByPackedSimd(buffer + offset, buffer, value.Length - offset, rightShiftCount);
                        else
                            ShiftRightByDefault(buffer + offset, buffer, value.Length - offset, rightShiftCount);


                    }
                }

                value[^offset..].Clear();
            }

            [MethodImpl(MethodImplOptions.AggressiveOptimization)]
            static unsafe void OperateVectorByAvx512F(uint* sp, uint* dp, int length, int rightShiftCount)
            {
                System.Diagnostics.Debug.Assert(Avx512F.IsSupported == true);
                System.Diagnostics.Debug.Assert(length > 0);
                System.Diagnostics.Debug.Assert(rightShiftCount >= 1 && rightShiftCount <= 31);

                var leftShiftCount = (32 - rightShiftCount);
                var count = length - 1;
                switch (rightShiftCount)
                {
                    case 1:
                    {
                        while (count >= Vector512<uint>.Count)
                        {
                            var lowPart = Avx512F.ShiftRightLogical(Avx512F.LoadVector512(sp), (byte)rightShiftCount);
                            var highPart = Avx512F.ShiftLeftLogical(Avx512F.LoadVector512(sp + 1), (byte)leftShiftCount);
                            Avx512F.Store(dp, Avx512F.Or(lowPart, highPart));
                            sp += Vector512<uint>.Count;
                            dp += Vector512<uint>.Count;
                            count -= Vector512<uint>.Count;
                        }

                        break;
                    }

                    // 中略

                    case 31:
                    default:
                    {
                        while (count >= Vector512<uint>.Count)
                        {
                            var lowPart = Avx512F.ShiftRightLogical(Avx512F.LoadVector512(sp), (byte)rightShiftCount);
                            var highPart = Avx512F.ShiftLeftLogical(Avx512F.LoadVector512(sp + 1), l(byte)eftShiftCount);
                            Avx512F.Store(dp, Avx512F.Or(lowPart, highPart));
                            sp += Vector512<uint>.Count;
                            dp += Vector512<uint>.Count;
                            count -= Vector512<uint>.Count;
                        }

                        break;
                    }
                }

                ShiftRightLesserThan16Words(sp, dp, count, rightShiftCount);
            }

            [MethodImpl(MethodImplOptions.AggressiveOptimization)]
            static unsafe void OperateVectorByAvx2(uint* sp, uint* dp, int length, int rightShiftCount)
            {
                System.Diagnostics.Debug.Assert(Avx2.IsSupported == true);
                System.Diagnostics.Debug.Assert(length > 0);
                System.Diagnostics.Debug.Assert(rightShiftCount >= 1 && rightShiftCount <= 31);

                var leftShiftCount = (32 - rightShiftCount);
                var count = length - 1;
                switch (rightShiftCount)
                {
                    case 1:
                    {
                        while (count >= Vector256<uint>.Count)
                        {
                            var lowPart = Avx2.ShiftRightLogical(Avx.LoadVector256(sp), (byte)rightShiftCount);
                            var highPart = Avx2.ShiftLeftLogical(Avx.LoadVector256(sp + 1), (byte)leftShiftCount);
                            Avx.Store(dp, Avx2.Or(lowPart, highPart));
                            sp += Vector256<uint>.Count;
                            dp += Vector256<uint>.Count;
                            count -= Vector256<uint>.Count;
                        }

                        break;
                    }

                    // 中略

                    case 31:
                    default:
                    {
                        while (count >= Vector256<uint>.Count)
                        {
                            var lowPart = Avx2.ShiftRightLogical(Avx.LoadVector256(sp), (byte)rightShiftCount);
                            var highPart = Avx2.ShiftLeftLogical(Avx.LoadVector256(sp + 1), (byte)leftShiftCount);
                            Avx.Store(dp, Avx2.Or(lowPart, highPart));
                            sp += Vector256<uint>.Count;
                            dp += Vector256<uint>.Count;
                            count -= Vector256<uint>.Count;
                        }

                        break;
                    }
                }

                ShiftRightLesserThan8Words(sp, dp, count, rightShiftCount);
            }

            [MethodImpl(MethodImplOptions.AggressiveOptimization)]
            static unsafe void OperateVectorBySse2(uint* sp, uint* dp, int length, int rightShiftCount)
            {
                System.Diagnostics.Debug.Assert(Sse2.IsSupported == true);
                System.Diagnostics.Debug.Assert(length > 0);
                System.Diagnostics.Debug.Assert(rightShiftCount >= 1 && rightShiftCount <= 31);

                var leftShiftCount = (32 - rightShiftCount);
                var count = length - 1;
                switch (rightShiftCount)
                {
                    case 1:
                    {
                        while (count >= Vector128<uint>.Count)
                        {
                            var lowPart = Sse2.ShiftRightLogical(Sse2.LoadVector128(sp), (byte)rightShiftCount);
                            var highPart = Sse2.ShiftLeftLogical(Sse2.LoadVector128(sp + 1), (byte)leftShiftCount);
                            Sse2.Store(dp, Sse2.Or(lowPart, highPart));

                            sp += Vector128<uint>.Count;
                            dp += Vector128<uint>.Count;
                            count -= Vector128<uint>.Count;
                        }

                        break;
                    }

                    // 中略

                    case 31:
                    default:
                    {
                        while (count >= Vector128<uint>.Count)
                        {
                            var lowPart = Sse2.ShiftRightLogical(Sse2.LoadVector128(sp), (byte)rightShiftCount);
                            var highPart = Sse2.ShiftLeftLogical(Sse2.LoadVector128(sp + 1), (byte)leftShiftCount);
                            Sse2.Store(dp, Sse2.Or(lowPart, highPart));

                            sp += Vector128<uint>.Count;
                            dp += Vector128<uint>.Count;
                            count -= Vector128<uint>.Count;
                        }

                        break;
                    }
                }

                ShiftRightLesserThan4Words(sp, dp, count, rightShiftCount);
            }

            [MethodImpl(MethodImplOptions.AggressiveOptimization)]
            static unsafe void OperateVectorByAdvSimd(uint* sp, uint* dp, int length, int rightShiftCount)
            {
                System.Diagnostics.Debug.Assert(AdvSimd.IsSupported == true);
                System.Diagnostics.Debug.Assert(length > 0);
                System.Diagnostics.Debug.Assert(rightShiftCount >= 1 && rightShiftCount <= 31);

                var leftShiftCount = (32 - rightShiftCount);
                var count = length - 1;
                switch (rightShiftCount)
                {
                    case 1:
                    {
                        while (count >= Vector128<uint>.Count)
                        {
                            var lowPart = AdvSimd.ShiftRightLogical(Sse2.LoadVector128(sp), (byte)rightShiftCount);
                            var highPart = AdvSimd.ShiftLeftLogical(Sse2.LoadVector128(sp + 1), (byte)leftShiftCount);
                            AdvSimd.Store(dp, AdvSimd.Or(lowPart, highPart));

                            sp += Vector128<uint>.Count;
                            dp += Vector128<uint>.Count;
                            count -= Vector128<uint>.Count;
                        }

                        break;
                    }

                    // 中略

                    case 31:
                    default:
                    {
                        while (count >= Vector128<uint>.Count)
                        {
                            var lowPart = AdvSimd.ShiftRightLogical(AdvSimd.LoadVector128(sp), (byte)rightShiftCount);
                            var highPart = AdvSimd.ShiftLeftLogical(AdvSimd.LoadVector128(sp + 1), (byte)leftShiftCount);
                            AdvSimd.Store(dp, AdvSimd.Or(lowPart, highPart));

                            sp += Vector128<uint>.Count;
                            dp += Vector128<uint>.Count;
                            count -= Vector128<uint>.Count;
                        }

                        break;
                    }
                }

                ShiftRightLesserThan4Words(sp, dp, count, rightShiftCount);
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
            static unsafe void OperateVectorByPackedSimd(uint* sp, uint* dp, int length, int rightShiftCount)
            {
                System.Diagnostics.Debug.Assert(PackedSimd.IsSupported == true);
                System.Diagnostics.Debug.Assert(length > 0);
                System.Diagnostics.Debug.Assert(rightShiftCount >= 1 && rightShiftCount <= 31);

                var leftShiftCount = (byte)(32 - rightShiftCount);
                var count = length - 1;
                while (count >= Vector128<uint>.Count)
                {
                    var lowPart = PackedSimd.ShiftRightLogical(PackedSimd.LoadVector128(sp), rightShiftCount);
                    var highPart = PackedSimd.ShiftLeft(PackedSimd.LoadVector128(sp + 1), leftShiftCount);
                    PackedSimd.Store(dp, PackedSimd.Or(lowPart, highPart));

                    sp += Vector128<uint>.Count;
                    dp += Vector128<uint>.Count;
                    count -= Vector128<uint>.Count;
                }

                ShiftRightLesserThan4Words(sp, dp, count, rightShiftCount);
            }

            [MethodImpl(MethodImplOptions.AggressiveOptimization)]
            static unsafe void ShiftRightByDefault(uint* sp, uint* dp, int length, int rightShiftCount)
            {
                System.Diagnostics.Debug.Assert(length > 0);
                System.Diagnostics.Debug.Assert(rightShiftCount >= 1 && rightShiftCount <= 31);

                var leftShiftCount = 32 - rightShiftCount;
                var lowBits = *sp++ >> rightShiftCount;
                var count = length - 1;

                // 32 のループから開始

                // 中略

                if (count >= 2)
                {
                    var nextBits1 = (ulong)sp[0] << leftShiftCount;
                    dp[0] = lowBits | (uint)nextBits1;
                    var nextBits2 = (ulong)sp[1] << leftShiftCount;
                    dp[1] = (uint)(nextBits1 >> 32) | (uint)nextBits2;
                    lowBits = (uint)(nextBits2 >> 32);
                    dp += 2;
                    sp += 2;
                    count -= 2;
                }

                if (count > 0)
                {
                    var nextBits = (ulong)*sp++ << leftShiftCount;
                    *dp++ = lowBits | (uint)nextBits;
                    lowBits = (uint)(nextBits >> 32);
                    --count;
                }

                *dp = lowBits;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
            static unsafe void ShiftRightLesserThan16Words(uint* sp, uint* dp, int count, int rightShiftCount)
            {
                System.Diagnostics.Debug.Assert(count >= 0 && count < 16);
                System.Diagnostics.Debug.Assert(rightShiftCount >= 1 && rightShiftCount <= 31);

                var leftShiftCount = 32 - rightShiftCount;
                var lowBits = *sp++ >> rightShiftCount;

                if (count >= 8)
                {
                    // 中略
                }

                if (count >= 4)
                {
                    // 中略
                }

                if (count >= 2)
                {
                    var nextBits1 = (ulong)sp[0] << leftShiftCount;
                    dp[0] = lowBits | (uint)nextBits1;
                    var nextBits2 = (ulong)sp[1] << leftShiftCount;
                    dp[1] = (uint)(nextBits1 >> 32) | (uint)nextBits2;
                    lowBits = (uint)(nextBits2 >> 32);
                    dp += 2;
                    sp += 2;
                    count -= 2;
                }

                if (count > 0)
                {
                    var nextBits = (ulong)*sp++ << leftShiftCount;
                    *dp++ = lowBits | (uint)nextBits;
                    lowBits = (uint)(nextBits >> 32);
                    --count;
                }

                *dp = lowBits;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
            static unsafe void ShiftRightLesserThan8Words(uint* sp, uint* dp, int count, int rightShiftCount)
            {
                System.Diagnostics.Debug.Assert(count >= 0 && count < 8);
                System.Diagnostics.Debug.Assert(rightShiftCount >= 1 && rightShiftCount <= 31);

                var leftShiftCount = 32 - rightShiftCount;
                var lowBits = *sp++ >> rightShiftCount;

                if (count >= 4)
                {
                    // 中略
                }

                if (count >= 2)
                {
                    var nextBits1 = (ulong)sp[0] << leftShiftCount;
                    dp[0] = lowBits | (uint)nextBits1;
                    var nextBits2 = (ulong)sp[1] << leftShiftCount;
                    dp[1] = (uint)(nextBits1 >> 32) | (uint)nextBits2;
                    lowBits = (uint)(nextBits2 >> 32);
                    dp += 2;
                    sp += 2;
                    count -= 2;
                }

                if (count > 0)
                {
                    var nextBits = (ulong)*sp++ << leftShiftCount;
                    *dp++ = lowBits | (uint)nextBits;
                    lowBits = (uint)(nextBits >> 32);
                    --count;
                }

                *dp = lowBits;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
            static unsafe void ShiftRightLesserThan4Words(uint* sp, uint* dp, int count, int rightShiftCount)
            {
                System.Diagnostics.Debug.Assert(count >= 0 && count < 4);
                System.Diagnostics.Debug.Assert(rightShiftCount >= 1 && rightShiftCount <= 31);

                var leftShiftCount = 32 - rightShiftCount;
                var lowBits = *sp++ >> rightShiftCount;

                if (count >= 2)
                {
                    var nextBits1 = (ulong)sp[0] << leftShiftCount;
                    dp[0] = lowBits | (uint)nextBits1;
                    var nextBits2 = (ulong)sp[1] << leftShiftCount;
                    dp[1] = (uint)(nextBits1 >> 32) | (uint)nextBits2;
                    lowBits = (uint)(nextBits2 >> 32);
                    dp += 2;
                    sp += 2;
                    count -= 2;
                }

                if (count > 0)
                {
                    var nextBits = (ulong)*sp++ << leftShiftCount;
                    *dp++ = lowBits | (uint)nextBits;
                    lowBits = (uint)(nextBits >> 32);
                    --count;
                }

                *dp = lowBits;
            }
        }
#endif

        private static string FormatUInt32Array(Span<uint> value)
        {
            var digits = new List<string>();
            unsafe
            {
                fixed (uint* buffer = value)
                {
                    var p = (byte*)(buffer + value.Length);
                    var count = value.Length;
                    while (count-- > 0)
                        digits.Add((--p)->ToString("x2", CultureInfo.InvariantCulture));
                }
            }

            return string.Join("-", digits);
        }

        private static string FormatVector(Vector128<uint> value)
        {
            var digits = new List<string>();
            for (var index = 0; index < Vector128<uint>.Count; ++index)
                digits.Add(value.GetElement(index).ToString("x8", CultureInfo.InvariantCulture));
            return $"[{string.Join(", ", digits)}]"; 
        }

        private static string FormatVector(Vector128<ulong> value)
        {
            var digits = new List<string>();
            for (var index = 0; index < Vector128<ulong>.Count; ++index)
                digits.Add(value.GetElement(index).ToString("x16", CultureInfo.InvariantCulture));
            return $"[{string.Join(", ", digits)}]";
        }

        private static string FormatVector(Vector256<uint> value)
        {
            var digits = new List<string>();
            for (var index = 0; index < Vector256<uint>.Count; ++index)
                digits.Add(value.GetElement(index).ToString("x8", CultureInfo.InvariantCulture));
            return $"[{string.Join(", ", digits)}]";
        }

        private static string FormatVector(Vector256<ulong> value)
        {
            var digits = new List<string>();
            for (var index = 0; index < Vector256<ulong>.Count; ++index)
                digits.Add(value.GetElement(index).ToString("x16", CultureInfo.InvariantCulture));
            return $"[{string.Join(", ", digits)}]";
        }

        private static string FormatVector(Vector512<uint> value)
        {
            var digits = new List<string>();
            for (var index = 0; index < Vector512<uint>.Count; ++index)
                digits.Add(value.GetElement(index).ToString("x8", CultureInfo.InvariantCulture));
            return $"[{string.Join(", ", digits)}]";
        }
    }
}
