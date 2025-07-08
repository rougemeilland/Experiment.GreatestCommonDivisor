using System;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.Arm;
using System.Runtime.Intrinsics.Wasm;
using System.Runtime.Intrinsics.X86;

namespace Experiment.CUI
{
    internal static partial class BigIntegerCalculatorVer2_1
    {
        #region ShiftRightTry1

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        private static void ShiftRightTry1(Span<uint> value, int bitCount)
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

                Assert(offset > 0);
                Assert(offset <= value.Length);

                value[offset..].CopyTo(value[..^offset]);
                value[^offset..].Clear();
            }
            else
            {
                var leftShiftCount = _BIT_COUNT_PER_UINT32 - rightShiftCount;

                Assert(offset >= 0);
                Assert(offset < value.Length);

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

                var index = 0;
                var sourceIndex = offset;
                var count = value.Length - offset - 1;
                var lowBits = value[sourceIndex++] >> rightShiftCount;
                while (count >= 2)
                {
                    var nextBits1 = (ulong)value[sourceIndex + 0] << leftShiftCount;
                    value[index + 0] = lowBits | (uint)nextBits1;
                    var nextBits2 = (ulong)value[sourceIndex + 1] << leftShiftCount;
                    value[index + 1] = (uint)(nextBits1 >> _BIT_COUNT_PER_UINT32) | (uint)nextBits2;
                    lowBits = (uint)(nextBits2 >> _BIT_COUNT_PER_UINT32);
                    index += 2;
                    sourceIndex += 2;
                    count -= 2;
                }

                if (count > 0)
                {
                    var nextBits = (ulong)value[sourceIndex++] << leftShiftCount;
                    value[index++] = lowBits | (uint)nextBits;
                    lowBits = (uint)(nextBits >> _BIT_COUNT_PER_UINT32);
                    --count;
                }

                Assert(count == 0);
                Assert(index == value.Length - offset - 1);
                Assert(sourceIndex == value.Length);

                value[index] = lowBits;

                value[^offset..].Clear();
            }
        }

        #endregion

        #region ShiftRightTry2

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        private static void ShiftRightTry2(Span<uint> value, int bitCount, HardwareAcceleratorOption accelerator = HardwareAcceleratorOption.ByDefault)
        {
            var offset = bitCount >> _SHIFT_BIT_COUNT_PER_UINT32;
            var rightShiftCount = bitCount & _BIT_COUNT_MASK_FOR_UINT32;
            if (rightShiftCount == 0)
            {
                value[offset..].CopyTo(value[..^offset]);
                value[^offset..].Clear();
                return;
            }

            unsafe
            {
                fixed (uint* buffer = value)
                {
                    if ((accelerator & HardwareAcceleratorOption.UseVector512IfPossible) != HardwareAcceleratorOption.None)
                    {
                        if (Avx512F.IsSupported)
                        {
                            Finish(OperateVectorByAvx512F(buffer + offset, buffer, value.Length - offset, rightShiftCount), buffer, value.Length);
                            return;
                        }
                    }

                    if ((accelerator & HardwareAcceleratorOption.UseVector256IfPossible) != HardwareAcceleratorOption.None)
                    {
                        if (Avx2.IsSupported)
                        {
                            Finish(OperateVectorByAvx2(buffer + offset, buffer, value.Length - offset, rightShiftCount), buffer, value.Length);
                            return;
                        }
                    }

                    if ((accelerator & HardwareAcceleratorOption.UseVector128IfPossible) != HardwareAcceleratorOption.None)
                    {
                        if (Sse2.IsSupported)
                        {
                            Finish(OperateVectorBySse2(buffer + offset, buffer, value.Length - offset, rightShiftCount), buffer, value.Length);
                            return;
                        }
                        else if (AdvSimd.IsSupported)
                        {
                            Finish(OperateVectorByAdvSimd(buffer + offset, buffer, value.Length - offset, rightShiftCount), buffer, value.Length);
                            return;
                        }
                        else if (PackedSimd.IsSupported)
                        {
                            Finish(OperateVectorByPackedSimd(buffer + offset, buffer, value.Length - offset, rightShiftCount), buffer, value.Length);
                            return;
                        }
                    }

                    Finish(ShiftRightByDefault(buffer + offset, buffer, value.Length - offset, rightShiftCount), buffer, value.Length);
                }
            }

            [MethodImpl(MethodImplOptions.AggressiveOptimization)]
            static unsafe byte* OperateVectorByAvx512F(uint* sp, uint* dp, int length, int rightShiftCount)
            {
                Assert(Avx512F.IsSupported == true);
                Assert(length > 0);
                Assert(rightShiftCount is >= 1 and <= 31);

                var leftShiftCount = _BIT_COUNT_PER_UINT32 - rightShiftCount;
                var count = length - 1;
                switch (rightShiftCount)
                {
                    case 1:
                        while (count >= Vector512<uint>.Count)
                        {
                            var lowPart = Avx512F.ShiftRightLogical(Avx512F.LoadVector512(sp), 1);
                            var highPart = Avx512F.ShiftLeftLogical(Avx512F.LoadVector512(sp + 1), 31);
                            Avx512F.Store(dp, Avx512F.Or(lowPart, highPart));
                            sp += Vector512<uint>.Count;
                            dp += Vector512<uint>.Count;
                            count -= Vector512<uint>.Count;
                        }

                        break;
                    case 2:
                        while (count >= Vector512<uint>.Count)
                        {
                            var lowPart = Avx512F.ShiftRightLogical(Avx512F.LoadVector512(sp), 2);
                            var highPart = Avx512F.ShiftLeftLogical(Avx512F.LoadVector512(sp + 1), 30);
                            Avx512F.Store(dp, Avx512F.Or(lowPart, highPart));
                            sp += Vector512<uint>.Count;
                            dp += Vector512<uint>.Count;
                            count -= Vector512<uint>.Count;
                        }

                        break;
                    case 3:
                        while (count >= Vector512<uint>.Count)
                        {
                            var lowPart = Avx512F.ShiftRightLogical(Avx512F.LoadVector512(sp), 3);
                            var highPart = Avx512F.ShiftLeftLogical(Avx512F.LoadVector512(sp + 1), 29);
                            Avx512F.Store(dp, Avx512F.Or(lowPart, highPart));
                            sp += Vector512<uint>.Count;
                            dp += Vector512<uint>.Count;
                            count -= Vector512<uint>.Count;
                        }

                        break;
                    case 4:
                        while (count >= Vector512<uint>.Count)
                        {
                            var lowPart = Avx512F.ShiftRightLogical(Avx512F.LoadVector512(sp), 4);
                            var highPart = Avx512F.ShiftLeftLogical(Avx512F.LoadVector512(sp + 1), 28);
                            Avx512F.Store(dp, Avx512F.Or(lowPart, highPart));
                            sp += Vector512<uint>.Count;
                            dp += Vector512<uint>.Count;
                            count -= Vector512<uint>.Count;
                        }

                        break;
                    case 5:
                        while (count >= Vector512<uint>.Count)
                        {
                            var lowPart = Avx512F.ShiftRightLogical(Avx512F.LoadVector512(sp), 5);
                            var highPart = Avx512F.ShiftLeftLogical(Avx512F.LoadVector512(sp + 1), 27);
                            Avx512F.Store(dp, Avx512F.Or(lowPart, highPart));
                            sp += Vector512<uint>.Count;
                            dp += Vector512<uint>.Count;
                            count -= Vector512<uint>.Count;
                        }

                        break;
                    case 6:
                        while (count >= Vector512<uint>.Count)
                        {
                            var lowPart = Avx512F.ShiftRightLogical(Avx512F.LoadVector512(sp), 6);
                            var highPart = Avx512F.ShiftLeftLogical(Avx512F.LoadVector512(sp + 1), 26);
                            Avx512F.Store(dp, Avx512F.Or(lowPart, highPart));
                            sp += Vector512<uint>.Count;
                            dp += Vector512<uint>.Count;
                            count -= Vector512<uint>.Count;
                        }

                        break;
                    case 7:
                        while (count >= Vector512<uint>.Count)
                        {
                            var lowPart = Avx512F.ShiftRightLogical(Avx512F.LoadVector512(sp), 7);
                            var highPart = Avx512F.ShiftLeftLogical(Avx512F.LoadVector512(sp + 1), 25);
                            Avx512F.Store(dp, Avx512F.Or(lowPart, highPart));
                            sp += Vector512<uint>.Count;
                            dp += Vector512<uint>.Count;
                            count -= Vector512<uint>.Count;
                        }

                        break;
                    case 8:
                        {
                            var byteLength = unchecked((uint)length << 2);
                            Assert(byteLength == length * 4);
                            Unsafe.CopyBlockUnaligned((byte*)dp, (byte*)sp + 1, byteLength - 1);
                            return (byte*)dp + byteLength - 1;
                        }
                    case 9:
                        while (count >= Vector512<uint>.Count)
                        {
                            var lowPart = Avx512F.ShiftRightLogical(Avx512F.LoadVector512(sp), 9);
                            var highPart = Avx512F.ShiftLeftLogical(Avx512F.LoadVector512(sp + 1), 23);
                            Avx512F.Store(dp, Avx512F.Or(lowPart, highPart));
                            sp += Vector512<uint>.Count;
                            dp += Vector512<uint>.Count;
                            count -= Vector512<uint>.Count;
                        }

                        break;
                    case 10:
                        while (count >= Vector512<uint>.Count)
                        {
                            var lowPart = Avx512F.ShiftRightLogical(Avx512F.LoadVector512(sp), 10);
                            var highPart = Avx512F.ShiftLeftLogical(Avx512F.LoadVector512(sp + 1), 22);
                            Avx512F.Store(dp, Avx512F.Or(lowPart, highPart));
                            sp += Vector512<uint>.Count;
                            dp += Vector512<uint>.Count;
                            count -= Vector512<uint>.Count;
                        }

                        break;
                    case 11:
                        while (count >= Vector512<uint>.Count)
                        {
                            var lowPart = Avx512F.ShiftRightLogical(Avx512F.LoadVector512(sp), 11);
                            var highPart = Avx512F.ShiftLeftLogical(Avx512F.LoadVector512(sp + 1), 21);
                            Avx512F.Store(dp, Avx512F.Or(lowPart, highPart));
                            sp += Vector512<uint>.Count;
                            dp += Vector512<uint>.Count;
                            count -= Vector512<uint>.Count;
                        }

                        break;
                    case 12:
                        while (count >= Vector512<uint>.Count)
                        {
                            var lowPart = Avx512F.ShiftRightLogical(Avx512F.LoadVector512(sp), 12);
                            var highPart = Avx512F.ShiftLeftLogical(Avx512F.LoadVector512(sp + 1), 20);
                            Avx512F.Store(dp, Avx512F.Or(lowPart, highPart));
                            sp += Vector512<uint>.Count;
                            dp += Vector512<uint>.Count;
                            count -= Vector512<uint>.Count;
                        }

                        break;
                    case 13:
                        while (count >= Vector512<uint>.Count)
                        {
                            var lowPart = Avx512F.ShiftRightLogical(Avx512F.LoadVector512(sp), 13);
                            var highPart = Avx512F.ShiftLeftLogical(Avx512F.LoadVector512(sp + 1), 19);
                            Avx512F.Store(dp, Avx512F.Or(lowPart, highPart));
                            sp += Vector512<uint>.Count;
                            dp += Vector512<uint>.Count;
                            count -= Vector512<uint>.Count;
                        }

                        break;
                    case 14:
                        while (count >= Vector512<uint>.Count)
                        {
                            var lowPart = Avx512F.ShiftRightLogical(Avx512F.LoadVector512(sp), 14);
                            var highPart = Avx512F.ShiftLeftLogical(Avx512F.LoadVector512(sp + 1), 18);
                            Avx512F.Store(dp, Avx512F.Or(lowPart, highPart));
                            sp += Vector512<uint>.Count;
                            dp += Vector512<uint>.Count;
                            count -= Vector512<uint>.Count;
                        }

                        break;
                    case 15:
                        while (count >= Vector512<uint>.Count)
                        {
                            var lowPart = Avx512F.ShiftRightLogical(Avx512F.LoadVector512(sp), 15);
                            var highPart = Avx512F.ShiftLeftLogical(Avx512F.LoadVector512(sp + 1), 17);
                            Avx512F.Store(dp, Avx512F.Or(lowPart, highPart));
                            sp += Vector512<uint>.Count;
                            dp += Vector512<uint>.Count;
                            count -= Vector512<uint>.Count;
                        }

                        break;
                    case 16:
                        {
                            var byteLength = unchecked((uint)length << 2);
                            Assert(byteLength == length * 4);
                            Unsafe.CopyBlockUnaligned((byte*)dp, (byte*)sp + 2, byteLength - 2);
                            return (byte*)dp + byteLength - 2;
                        }
                    case 17:
                        while (count >= Vector512<uint>.Count)
                        {
                            var lowPart = Avx512F.ShiftRightLogical(Avx512F.LoadVector512(sp), 17);
                            var highPart = Avx512F.ShiftLeftLogical(Avx512F.LoadVector512(sp + 1), 15);
                            Avx512F.Store(dp, Avx512F.Or(lowPart, highPart));
                            sp += Vector512<uint>.Count;
                            dp += Vector512<uint>.Count;
                            count -= Vector512<uint>.Count;
                        }

                        break;
                    case 18:
                        while (count >= Vector512<uint>.Count)
                        {
                            var lowPart = Avx512F.ShiftRightLogical(Avx512F.LoadVector512(sp), 18);
                            var highPart = Avx512F.ShiftLeftLogical(Avx512F.LoadVector512(sp + 1), 14);
                            Avx512F.Store(dp, Avx512F.Or(lowPart, highPart));
                            sp += Vector512<uint>.Count;
                            dp += Vector512<uint>.Count;
                            count -= Vector512<uint>.Count;
                        }

                        break;
                    case 19:
                        while (count >= Vector512<uint>.Count)
                        {
                            var lowPart = Avx512F.ShiftRightLogical(Avx512F.LoadVector512(sp), 19);
                            var highPart = Avx512F.ShiftLeftLogical(Avx512F.LoadVector512(sp + 1), 13);
                            Avx512F.Store(dp, Avx512F.Or(lowPart, highPart));
                            sp += Vector512<uint>.Count;
                            dp += Vector512<uint>.Count;
                            count -= Vector512<uint>.Count;
                        }

                        break;
                    case 20:
                        while (count >= Vector512<uint>.Count)
                        {
                            var lowPart = Avx512F.ShiftRightLogical(Avx512F.LoadVector512(sp), 20);
                            var highPart = Avx512F.ShiftLeftLogical(Avx512F.LoadVector512(sp + 1), 12);
                            Avx512F.Store(dp, Avx512F.Or(lowPart, highPart));
                            sp += Vector512<uint>.Count;
                            dp += Vector512<uint>.Count;
                            count -= Vector512<uint>.Count;
                        }

                        break;
                    case 21:
                        while (count >= Vector512<uint>.Count)
                        {
                            var lowPart = Avx512F.ShiftRightLogical(Avx512F.LoadVector512(sp), 21);
                            var highPart = Avx512F.ShiftLeftLogical(Avx512F.LoadVector512(sp + 1), 11);
                            Avx512F.Store(dp, Avx512F.Or(lowPart, highPart));
                            sp += Vector512<uint>.Count;
                            dp += Vector512<uint>.Count;
                            count -= Vector512<uint>.Count;
                        }

                        break;
                    case 22:
                        while (count >= Vector512<uint>.Count)
                        {
                            var lowPart = Avx512F.ShiftRightLogical(Avx512F.LoadVector512(sp), 22);
                            var highPart = Avx512F.ShiftLeftLogical(Avx512F.LoadVector512(sp + 1), 10);
                            Avx512F.Store(dp, Avx512F.Or(lowPart, highPart));
                            sp += Vector512<uint>.Count;
                            dp += Vector512<uint>.Count;
                            count -= Vector512<uint>.Count;
                        }

                        break;
                    case 23:
                        while (count >= Vector512<uint>.Count)
                        {
                            var lowPart = Avx512F.ShiftRightLogical(Avx512F.LoadVector512(sp), 23);
                            var highPart = Avx512F.ShiftLeftLogical(Avx512F.LoadVector512(sp + 1), 9);
                            Avx512F.Store(dp, Avx512F.Or(lowPart, highPart));
                            sp += Vector512<uint>.Count;
                            dp += Vector512<uint>.Count;
                            count -= Vector512<uint>.Count;
                        }

                        break;
                    case 24:
                        {
                            var byteLength = unchecked((uint)length << 2);
                            Assert(byteLength == length * 4);
                            Unsafe.CopyBlockUnaligned((byte*)dp, (byte*)sp + 3, byteLength - 3);
                            return (byte*)dp + byteLength - 3;
                        }
                    case 25:
                        while (count >= Vector512<uint>.Count)
                        {
                            var lowPart = Avx512F.ShiftRightLogical(Avx512F.LoadVector512(sp), 25);
                            var highPart = Avx512F.ShiftLeftLogical(Avx512F.LoadVector512(sp + 1), 7);
                            Avx512F.Store(dp, Avx512F.Or(lowPart, highPart));
                            sp += Vector512<uint>.Count;
                            dp += Vector512<uint>.Count;
                            count -= Vector512<uint>.Count;
                        }

                        break;
                    case 26:
                        while (count >= Vector512<uint>.Count)
                        {
                            var lowPart = Avx512F.ShiftRightLogical(Avx512F.LoadVector512(sp), 26);
                            var highPart = Avx512F.ShiftLeftLogical(Avx512F.LoadVector512(sp + 1), 6);
                            Avx512F.Store(dp, Avx512F.Or(lowPart, highPart));
                            sp += Vector512<uint>.Count;
                            dp += Vector512<uint>.Count;
                            count -= Vector512<uint>.Count;
                        }

                        break;
                    case 27:
                        while (count >= Vector512<uint>.Count)
                        {
                            var lowPart = Avx512F.ShiftRightLogical(Avx512F.LoadVector512(sp), 27);
                            var highPart = Avx512F.ShiftLeftLogical(Avx512F.LoadVector512(sp + 1), 5);
                            Avx512F.Store(dp, Avx512F.Or(lowPart, highPart));
                            sp += Vector512<uint>.Count;
                            dp += Vector512<uint>.Count;
                            count -= Vector512<uint>.Count;
                        }

                        break;
                    case 28:
                        while (count >= Vector512<uint>.Count)
                        {
                            var lowPart = Avx512F.ShiftRightLogical(Avx512F.LoadVector512(sp), 28);
                            var highPart = Avx512F.ShiftLeftLogical(Avx512F.LoadVector512(sp + 1), 4);
                            Avx512F.Store(dp, Avx512F.Or(lowPart, highPart));
                            sp += Vector512<uint>.Count;
                            dp += Vector512<uint>.Count;
                            count -= Vector512<uint>.Count;
                        }

                        break;
                    case 29:
                        while (count >= Vector512<uint>.Count)
                        {
                            var lowPart = Avx512F.ShiftRightLogical(Avx512F.LoadVector512(sp), 29);
                            var highPart = Avx512F.ShiftLeftLogical(Avx512F.LoadVector512(sp + 1), 3);
                            Avx512F.Store(dp, Avx512F.Or(lowPart, highPart));
                            sp += Vector512<uint>.Count;
                            dp += Vector512<uint>.Count;
                            count -= Vector512<uint>.Count;
                        }

                        break;
                    case 30:
                        while (count >= Vector512<uint>.Count)
                        {
                            var lowPart = Avx512F.ShiftRightLogical(Avx512F.LoadVector512(sp), 30);
                            var highPart = Avx512F.ShiftLeftLogical(Avx512F.LoadVector512(sp + 1), 2);
                            Avx512F.Store(dp, Avx512F.Or(lowPart, highPart));
                            sp += Vector512<uint>.Count;
                            dp += Vector512<uint>.Count;
                            count -= Vector512<uint>.Count;
                        }

                        break;
                    case 31:
                    default:
                        while (count >= Vector512<uint>.Count)
                        {
                            var lowPart = Avx512F.ShiftRightLogical(Avx512F.LoadVector512(sp), 31);
                            var highPart = Avx512F.ShiftLeftLogical(Avx512F.LoadVector512(sp + 1), 1);
                            Avx512F.Store(dp, Avx512F.Or(lowPart, highPart));
                            sp += Vector512<uint>.Count;
                            dp += Vector512<uint>.Count;
                            count -= Vector512<uint>.Count;
                        }

                        break;
                }

                return ShiftRightLesserThan16Words(sp, dp, count, rightShiftCount);
            }

            [MethodImpl(MethodImplOptions.AggressiveOptimization)]
            static unsafe byte* OperateVectorByAvx2(uint* sp, uint* dp, int length, int rightShiftCount)
            {
                Assert(Avx2.IsSupported == true);
                Assert(length > 0);
                Assert(rightShiftCount is >= 1 and <= 31);

                var leftShiftCount = _BIT_COUNT_PER_UINT32 - rightShiftCount;
                var count = length - 1;
                switch (rightShiftCount)
                {
                    case 1:
                        while (count >= Vector256<uint>.Count)
                        {
                            var lowPart = Avx2.ShiftRightLogical(Avx.LoadVector256(sp), 1);
                            var highPart = Avx2.ShiftLeftLogical(Avx.LoadVector256(sp + 1), 31);
                            Avx.Store(dp, Avx2.Or(lowPart, highPart));
                            sp += Vector256<uint>.Count;
                            dp += Vector256<uint>.Count;
                            count -= Vector256<uint>.Count;
                        }

                        break;
                    case 2:
                        while (count >= Vector256<uint>.Count)
                        {
                            var lowPart = Avx2.ShiftRightLogical(Avx.LoadVector256(sp), 2);
                            var highPart = Avx2.ShiftLeftLogical(Avx.LoadVector256(sp + 1), 30);
                            Avx.Store(dp, Avx2.Or(lowPart, highPart));
                            sp += Vector256<uint>.Count;
                            dp += Vector256<uint>.Count;
                            count -= Vector256<uint>.Count;
                        }

                        break;
                    case 3:
                        while (count >= Vector256<uint>.Count)
                        {
                            var lowPart = Avx2.ShiftRightLogical(Avx.LoadVector256(sp), 3);
                            var highPart = Avx2.ShiftLeftLogical(Avx.LoadVector256(sp + 1), 29);
                            Avx.Store(dp, Avx2.Or(lowPart, highPart));
                            sp += Vector256<uint>.Count;
                            dp += Vector256<uint>.Count;
                            count -= Vector256<uint>.Count;
                        }

                        break;
                    case 4:
                        while (count >= Vector256<uint>.Count)
                        {
                            var lowPart = Avx2.ShiftRightLogical(Avx.LoadVector256(sp), 4);
                            var highPart = Avx2.ShiftLeftLogical(Avx.LoadVector256(sp + 1), 28);
                            Avx.Store(dp, Avx2.Or(lowPart, highPart));
                            sp += Vector256<uint>.Count;
                            dp += Vector256<uint>.Count;
                            count -= Vector256<uint>.Count;
                        }

                        break;
                    case 5:
                        while (count >= Vector256<uint>.Count)
                        {
                            var lowPart = Avx2.ShiftRightLogical(Avx.LoadVector256(sp), 5);
                            var highPart = Avx2.ShiftLeftLogical(Avx.LoadVector256(sp + 1), 27);
                            Avx.Store(dp, Avx2.Or(lowPart, highPart));
                            sp += Vector256<uint>.Count;
                            dp += Vector256<uint>.Count;
                            count -= Vector256<uint>.Count;
                        }

                        break;
                    case 6:
                        while (count >= Vector256<uint>.Count)
                        {
                            var lowPart = Avx2.ShiftRightLogical(Avx.LoadVector256(sp), 6);
                            var highPart = Avx2.ShiftLeftLogical(Avx.LoadVector256(sp + 1), 26);
                            Avx.Store(dp, Avx2.Or(lowPart, highPart));
                            sp += Vector256<uint>.Count;
                            dp += Vector256<uint>.Count;
                            count -= Vector256<uint>.Count;
                        }

                        break;
                    case 7:
                        while (count >= Vector256<uint>.Count)
                        {
                            var lowPart = Avx2.ShiftRightLogical(Avx.LoadVector256(sp), 7);
                            var highPart = Avx2.ShiftLeftLogical(Avx.LoadVector256(sp + 1), 25);
                            Avx.Store(dp, Avx2.Or(lowPart, highPart));
                            sp += Vector256<uint>.Count;
                            dp += Vector256<uint>.Count;
                            count -= Vector256<uint>.Count;
                        }

                        break;
                    case 8:
                        {
                            var byteLength = unchecked((uint)length << 2);
                            Assert(byteLength == length * 4);
                            Unsafe.CopyBlockUnaligned((byte*)dp, (byte*)sp + 1, byteLength - 1);
                            return (byte*)dp + byteLength - 1;
                        }
                    case 9:
                        while (count >= Vector256<uint>.Count)
                        {
                            var lowPart = Avx2.ShiftRightLogical(Avx.LoadVector256(sp), 9);
                            var highPart = Avx2.ShiftLeftLogical(Avx.LoadVector256(sp + 1), 23);
                            Avx.Store(dp, Avx2.Or(lowPart, highPart));
                            sp += Vector256<uint>.Count;
                            dp += Vector256<uint>.Count;
                            count -= Vector256<uint>.Count;
                        }

                        break;
                    case 10:
                        while (count >= Vector256<uint>.Count)
                        {
                            var lowPart = Avx2.ShiftRightLogical(Avx.LoadVector256(sp), 10);
                            var highPart = Avx2.ShiftLeftLogical(Avx.LoadVector256(sp + 1), 22);
                            Avx.Store(dp, Avx2.Or(lowPart, highPart));
                            sp += Vector256<uint>.Count;
                            dp += Vector256<uint>.Count;
                            count -= Vector256<uint>.Count;
                        }

                        break;
                    case 11:
                        while (count >= Vector256<uint>.Count)
                        {
                            var lowPart = Avx2.ShiftRightLogical(Avx.LoadVector256(sp), 11);
                            var highPart = Avx2.ShiftLeftLogical(Avx.LoadVector256(sp + 1), 21);
                            Avx.Store(dp, Avx2.Or(lowPart, highPart));
                            sp += Vector256<uint>.Count;
                            dp += Vector256<uint>.Count;
                            count -= Vector256<uint>.Count;
                        }

                        break;
                    case 12:
                        while (count >= Vector256<uint>.Count)
                        {
                            var lowPart = Avx2.ShiftRightLogical(Avx.LoadVector256(sp), 12);
                            var highPart = Avx2.ShiftLeftLogical(Avx.LoadVector256(sp + 1), 20);
                            Avx.Store(dp, Avx2.Or(lowPart, highPart));
                            sp += Vector256<uint>.Count;
                            dp += Vector256<uint>.Count;
                            count -= Vector256<uint>.Count;
                        }

                        break;
                    case 13:
                        while (count >= Vector256<uint>.Count)
                        {
                            var lowPart = Avx2.ShiftRightLogical(Avx.LoadVector256(sp), 13);
                            var highPart = Avx2.ShiftLeftLogical(Avx.LoadVector256(sp + 1), 19);
                            Avx.Store(dp, Avx2.Or(lowPart, highPart));
                            sp += Vector256<uint>.Count;
                            dp += Vector256<uint>.Count;
                            count -= Vector256<uint>.Count;
                        }

                        break;
                    case 14:
                        while (count >= Vector256<uint>.Count)
                        {
                            var lowPart = Avx2.ShiftRightLogical(Avx.LoadVector256(sp), 14);
                            var highPart = Avx2.ShiftLeftLogical(Avx.LoadVector256(sp + 1), 18);
                            Avx.Store(dp, Avx2.Or(lowPart, highPart));
                            sp += Vector256<uint>.Count;
                            dp += Vector256<uint>.Count;
                            count -= Vector256<uint>.Count;
                        }

                        break;
                    case 15:
                        while (count >= Vector256<uint>.Count)
                        {
                            var lowPart = Avx2.ShiftRightLogical(Avx.LoadVector256(sp), 15);
                            var highPart = Avx2.ShiftLeftLogical(Avx.LoadVector256(sp + 1), 17);
                            Avx.Store(dp, Avx2.Or(lowPart, highPart));
                            sp += Vector256<uint>.Count;
                            dp += Vector256<uint>.Count;
                            count -= Vector256<uint>.Count;
                        }

                        break;
                    case 16:
                        {
                            var byteLength = unchecked((uint)length << 2);
                            Assert(byteLength == length * 4);
                            Unsafe.CopyBlockUnaligned((byte*)dp, (byte*)sp + 2, byteLength - 2);
                            return (byte*)dp + byteLength - 2;
                        }
                    case 17:
                        while (count >= Vector256<uint>.Count)
                        {
                            var lowPart = Avx2.ShiftRightLogical(Avx.LoadVector256(sp), 17);
                            var highPart = Avx2.ShiftLeftLogical(Avx.LoadVector256(sp + 1), 15);
                            Avx.Store(dp, Avx2.Or(lowPart, highPart));
                            sp += Vector256<uint>.Count;
                            dp += Vector256<uint>.Count;
                            count -= Vector256<uint>.Count;
                        }

                        break;
                    case 18:
                        while (count >= Vector256<uint>.Count)
                        {
                            var lowPart = Avx2.ShiftRightLogical(Avx.LoadVector256(sp), 18);
                            var highPart = Avx2.ShiftLeftLogical(Avx.LoadVector256(sp + 1), 14);
                            Avx.Store(dp, Avx2.Or(lowPart, highPart));
                            sp += Vector256<uint>.Count;
                            dp += Vector256<uint>.Count;
                            count -= Vector256<uint>.Count;
                        }

                        break;
                    case 19:
                        while (count >= Vector256<uint>.Count)
                        {
                            var lowPart = Avx2.ShiftRightLogical(Avx.LoadVector256(sp), 19);
                            var highPart = Avx2.ShiftLeftLogical(Avx.LoadVector256(sp + 1), 13);
                            Avx.Store(dp, Avx2.Or(lowPart, highPart));
                            sp += Vector256<uint>.Count;
                            dp += Vector256<uint>.Count;
                            count -= Vector256<uint>.Count;
                        }

                        break;
                    case 20:
                        while (count >= Vector256<uint>.Count)
                        {
                            var lowPart = Avx2.ShiftRightLogical(Avx.LoadVector256(sp), 20);
                            var highPart = Avx2.ShiftLeftLogical(Avx.LoadVector256(sp + 1), 12);
                            Avx.Store(dp, Avx2.Or(lowPart, highPart));
                            sp += Vector256<uint>.Count;
                            dp += Vector256<uint>.Count;
                            count -= Vector256<uint>.Count;
                        }

                        break;
                    case 21:
                        while (count >= Vector256<uint>.Count)
                        {
                            var lowPart = Avx2.ShiftRightLogical(Avx.LoadVector256(sp), 21);
                            var highPart = Avx2.ShiftLeftLogical(Avx.LoadVector256(sp + 1), 11);
                            Avx.Store(dp, Avx2.Or(lowPart, highPart));
                            sp += Vector256<uint>.Count;
                            dp += Vector256<uint>.Count;
                            count -= Vector256<uint>.Count;
                        }

                        break;
                    case 22:
                        while (count >= Vector256<uint>.Count)
                        {
                            var lowPart = Avx2.ShiftRightLogical(Avx.LoadVector256(sp), 22);
                            var highPart = Avx2.ShiftLeftLogical(Avx.LoadVector256(sp + 1), 10);
                            Avx.Store(dp, Avx2.Or(lowPart, highPart));
                            sp += Vector256<uint>.Count;
                            dp += Vector256<uint>.Count;
                            count -= Vector256<uint>.Count;
                        }

                        break;
                    case 23:
                        while (count >= Vector256<uint>.Count)
                        {
                            var lowPart = Avx2.ShiftRightLogical(Avx.LoadVector256(sp), 23);
                            var highPart = Avx2.ShiftLeftLogical(Avx.LoadVector256(sp + 1), 9);
                            Avx.Store(dp, Avx2.Or(lowPart, highPart));
                            sp += Vector256<uint>.Count;
                            dp += Vector256<uint>.Count;
                            count -= Vector256<uint>.Count;
                        }

                        break;
                    case 24:
                        {
                            var byteLength = unchecked((uint)length << 2);
                            Assert(byteLength == length * 4);
                            Unsafe.CopyBlockUnaligned((byte*)dp, (byte*)sp + 3, byteLength - 3);
                            return (byte*)dp + byteLength - 3;
                        }
                    case 25:
                        while (count >= Vector256<uint>.Count)
                        {
                            var lowPart = Avx2.ShiftRightLogical(Avx.LoadVector256(sp), 25);
                            var highPart = Avx2.ShiftLeftLogical(Avx.LoadVector256(sp + 1), 7);
                            Avx.Store(dp, Avx2.Or(lowPart, highPart));
                            sp += Vector256<uint>.Count;
                            dp += Vector256<uint>.Count;
                            count -= Vector256<uint>.Count;
                        }

                        break;
                    case 26:
                        while (count >= Vector256<uint>.Count)
                        {
                            var lowPart = Avx2.ShiftRightLogical(Avx.LoadVector256(sp), 26);
                            var highPart = Avx2.ShiftLeftLogical(Avx.LoadVector256(sp + 1), 6);
                            Avx.Store(dp, Avx2.Or(lowPart, highPart));
                            sp += Vector256<uint>.Count;
                            dp += Vector256<uint>.Count;
                            count -= Vector256<uint>.Count;
                        }

                        break;
                    case 27:
                        while (count >= Vector256<uint>.Count)
                        {
                            var lowPart = Avx2.ShiftRightLogical(Avx.LoadVector256(sp), 27);
                            var highPart = Avx2.ShiftLeftLogical(Avx.LoadVector256(sp + 1), 5);
                            Avx.Store(dp, Avx2.Or(lowPart, highPart));
                            sp += Vector256<uint>.Count;
                            dp += Vector256<uint>.Count;
                            count -= Vector256<uint>.Count;
                        }

                        break;
                    case 28:
                        while (count >= Vector256<uint>.Count)
                        {
                            var lowPart = Avx2.ShiftRightLogical(Avx.LoadVector256(sp), 28);
                            var highPart = Avx2.ShiftLeftLogical(Avx.LoadVector256(sp + 1), 4);
                            Avx.Store(dp, Avx2.Or(lowPart, highPart));
                            sp += Vector256<uint>.Count;
                            dp += Vector256<uint>.Count;
                            count -= Vector256<uint>.Count;
                        }

                        break;
                    case 29:
                        while (count >= Vector256<uint>.Count)
                        {
                            var lowPart = Avx2.ShiftRightLogical(Avx.LoadVector256(sp), 29);
                            var highPart = Avx2.ShiftLeftLogical(Avx.LoadVector256(sp + 1), 3);
                            Avx.Store(dp, Avx2.Or(lowPart, highPart));
                            sp += Vector256<uint>.Count;
                            dp += Vector256<uint>.Count;
                            count -= Vector256<uint>.Count;
                        }

                        break;
                    case 30:
                        while (count >= Vector256<uint>.Count)
                        {
                            var lowPart = Avx2.ShiftRightLogical(Avx.LoadVector256(sp), 30);
                            var highPart = Avx2.ShiftLeftLogical(Avx.LoadVector256(sp + 1), 2);
                            Avx.Store(dp, Avx2.Or(lowPart, highPart));
                            sp += Vector256<uint>.Count;
                            dp += Vector256<uint>.Count;
                            count -= Vector256<uint>.Count;
                        }

                        break;
                    case 31:
                    default:
                        while (count >= Vector256<uint>.Count)
                        {
                            var lowPart = Avx2.ShiftRightLogical(Avx.LoadVector256(sp), 31);
                            var highPart = Avx2.ShiftLeftLogical(Avx.LoadVector256(sp + 1), 1);
                            Avx.Store(dp, Avx2.Or(lowPart, highPart));
                            sp += Vector256<uint>.Count;
                            dp += Vector256<uint>.Count;
                            count -= Vector256<uint>.Count;
                        }

                        break;
                }

                return ShiftRightLesserThan8Words(sp, dp, count, rightShiftCount);
            }

            [MethodImpl(MethodImplOptions.AggressiveOptimization)]
            static unsafe byte* OperateVectorBySse2(uint* sp, uint* dp, int length, int rightShiftCount)
            {
                Assert(Sse2.IsSupported == true);
                Assert(length > 0);
                Assert(rightShiftCount is >= 1 and <= 31);

                var count = length - 1;
                switch (rightShiftCount)
                {
                    case 1:
                        while (count >= Vector128<uint>.Count)
                        {
                            var lowPart = Sse2.ShiftRightLogical(Sse2.LoadVector128(sp), 1);
                            var highPart = Sse2.ShiftLeftLogical(Sse2.LoadVector128(sp + 1), 31);
                            Sse2.Store(dp, Sse2.Or(lowPart, highPart));
                            sp += Vector128<uint>.Count;
                            dp += Vector128<uint>.Count;
                            count -= Vector128<uint>.Count;
                        }

                        break;
                    case 2:
                        while (count >= Vector128<uint>.Count)
                        {
                            var lowPart = Sse2.ShiftRightLogical(Sse2.LoadVector128(sp), 2);
                            var highPart = Sse2.ShiftLeftLogical(Sse2.LoadVector128(sp + 1), 30);
                            Sse2.Store(dp, Sse2.Or(lowPart, highPart));
                            sp += Vector128<uint>.Count;
                            dp += Vector128<uint>.Count;
                            count -= Vector128<uint>.Count;
                        }

                        break;
                    case 3:
                        while (count >= Vector128<uint>.Count)
                        {
                            var lowPart = Sse2.ShiftRightLogical(Sse2.LoadVector128(sp), 3);
                            var highPart = Sse2.ShiftLeftLogical(Sse2.LoadVector128(sp + 1), 29);
                            Sse2.Store(dp, Sse2.Or(lowPart, highPart));
                            sp += Vector128<uint>.Count;
                            dp += Vector128<uint>.Count;
                            count -= Vector128<uint>.Count;
                        }

                        break;
                    case 4:
                        while (count >= Vector128<uint>.Count)
                        {
                            var lowPart = Sse2.ShiftRightLogical(Sse2.LoadVector128(sp), 4);
                            var highPart = Sse2.ShiftLeftLogical(Sse2.LoadVector128(sp + 1), 28);
                            Sse2.Store(dp, Sse2.Or(lowPart, highPart));
                            sp += Vector128<uint>.Count;
                            dp += Vector128<uint>.Count;
                            count -= Vector128<uint>.Count;
                        }

                        break;
                    case 5:
                        while (count >= Vector128<uint>.Count)
                        {
                            var lowPart = Sse2.ShiftRightLogical(Sse2.LoadVector128(sp), 5);
                            var highPart = Sse2.ShiftLeftLogical(Sse2.LoadVector128(sp + 1), 27);
                            Sse2.Store(dp, Sse2.Or(lowPart, highPart));
                            sp += Vector128<uint>.Count;
                            dp += Vector128<uint>.Count;
                            count -= Vector128<uint>.Count;
                        }

                        break;
                    case 6:
                        while (count >= Vector128<uint>.Count)
                        {
                            var lowPart = Sse2.ShiftRightLogical(Sse2.LoadVector128(sp), 6);
                            var highPart = Sse2.ShiftLeftLogical(Sse2.LoadVector128(sp + 1), 26);
                            Sse2.Store(dp, Sse2.Or(lowPart, highPart));
                            sp += Vector128<uint>.Count;
                            dp += Vector128<uint>.Count;
                            count -= Vector128<uint>.Count;
                        }

                        break;
                    case 7:
                        while (count >= Vector128<uint>.Count)
                        {
                            var lowPart = Sse2.ShiftRightLogical(Sse2.LoadVector128(sp), 7);
                            var highPart = Sse2.ShiftLeftLogical(Sse2.LoadVector128(sp + 1), 25);
                            Sse2.Store(dp, Sse2.Or(lowPart, highPart));
                            sp += Vector128<uint>.Count;
                            dp += Vector128<uint>.Count;
                            count -= Vector128<uint>.Count;
                        }

                        break;
                    case 8:
                        {
                            var byteLength = unchecked((uint)length << 2);
                            Assert(byteLength == length * 4);
                            Unsafe.CopyBlockUnaligned((byte*)dp, (byte*)sp + 1, byteLength - 1);
                            return (byte*)dp + byteLength - 1;
                        }
                    case 9:
                        while (count >= Vector128<uint>.Count)
                        {
                            var lowPart = Sse2.ShiftRightLogical(Sse2.LoadVector128(sp), 9);
                            var highPart = Sse2.ShiftLeftLogical(Sse2.LoadVector128(sp + 1), 23);
                            Sse2.Store(dp, Sse2.Or(lowPart, highPart));
                            sp += Vector128<uint>.Count;
                            dp += Vector128<uint>.Count;
                            count -= Vector128<uint>.Count;
                        }

                        break;
                    case 10:
                        while (count >= Vector128<uint>.Count)
                        {
                            var lowPart = Sse2.ShiftRightLogical(Sse2.LoadVector128(sp), 10);
                            var highPart = Sse2.ShiftLeftLogical(Sse2.LoadVector128(sp + 1), 22);
                            Sse2.Store(dp, Sse2.Or(lowPart, highPart));
                            sp += Vector128<uint>.Count;
                            dp += Vector128<uint>.Count;
                            count -= Vector128<uint>.Count;
                        }

                        break;
                    case 11:
                        while (count >= Vector128<uint>.Count)
                        {
                            var lowPart = Sse2.ShiftRightLogical(Sse2.LoadVector128(sp), 11);
                            var highPart = Sse2.ShiftLeftLogical(Sse2.LoadVector128(sp + 1), 21);
                            Sse2.Store(dp, Sse2.Or(lowPart, highPart));
                            sp += Vector128<uint>.Count;
                            dp += Vector128<uint>.Count;
                            count -= Vector128<uint>.Count;
                        }

                        break;
                    case 12:
                        while (count >= Vector128<uint>.Count)
                        {
                            var lowPart = Sse2.ShiftRightLogical(Sse2.LoadVector128(sp), 12);
                            var highPart = Sse2.ShiftLeftLogical(Sse2.LoadVector128(sp + 1), 20);
                            Sse2.Store(dp, Sse2.Or(lowPart, highPart));
                            sp += Vector128<uint>.Count;
                            dp += Vector128<uint>.Count;
                            count -= Vector128<uint>.Count;
                        }

                        break;
                    case 13:
                        while (count >= Vector128<uint>.Count)
                        {
                            var lowPart = Sse2.ShiftRightLogical(Sse2.LoadVector128(sp), 13);
                            var highPart = Sse2.ShiftLeftLogical(Sse2.LoadVector128(sp + 1), 19);
                            Sse2.Store(dp, Sse2.Or(lowPart, highPart));
                            sp += Vector128<uint>.Count;
                            dp += Vector128<uint>.Count;
                            count -= Vector128<uint>.Count;
                        }

                        break;
                    case 14:
                        while (count >= Vector128<uint>.Count)
                        {
                            var lowPart = Sse2.ShiftRightLogical(Sse2.LoadVector128(sp), 14);
                            var highPart = Sse2.ShiftLeftLogical(Sse2.LoadVector128(sp + 1), 18);
                            Sse2.Store(dp, Sse2.Or(lowPart, highPart));
                            sp += Vector128<uint>.Count;
                            dp += Vector128<uint>.Count;
                            count -= Vector128<uint>.Count;
                        }

                        break;
                    case 15:
                        while (count >= Vector128<uint>.Count)
                        {
                            var lowPart = Sse2.ShiftRightLogical(Sse2.LoadVector128(sp), 15);
                            var highPart = Sse2.ShiftLeftLogical(Sse2.LoadVector128(sp + 1), 17);
                            Sse2.Store(dp, Sse2.Or(lowPart, highPart));
                            sp += Vector128<uint>.Count;
                            dp += Vector128<uint>.Count;
                            count -= Vector128<uint>.Count;
                        }

                        break;
                    case 16:
                        {
                            var byteLength = unchecked((uint)length << 2);
                            Assert(byteLength == length * 4);
                            Unsafe.CopyBlockUnaligned((byte*)dp, (byte*)sp + 2, byteLength - 2);
                            return (byte*)dp + byteLength - 2;
                        }
                    case 17:
                        while (count >= Vector128<uint>.Count)
                        {
                            var lowPart = Sse2.ShiftRightLogical(Sse2.LoadVector128(sp), 17);
                            var highPart = Sse2.ShiftLeftLogical(Sse2.LoadVector128(sp + 1), 15);
                            Sse2.Store(dp, Sse2.Or(lowPart, highPart));
                            sp += Vector128<uint>.Count;
                            dp += Vector128<uint>.Count;
                            count -= Vector128<uint>.Count;
                        }

                        break;
                    case 18:
                        while (count >= Vector128<uint>.Count)
                        {
                            var lowPart = Sse2.ShiftRightLogical(Sse2.LoadVector128(sp), 18);
                            var highPart = Sse2.ShiftLeftLogical(Sse2.LoadVector128(sp + 1), 14);
                            Sse2.Store(dp, Sse2.Or(lowPart, highPart));
                            sp += Vector128<uint>.Count;
                            dp += Vector128<uint>.Count;
                            count -= Vector128<uint>.Count;
                        }

                        break;
                    case 19:
                        while (count >= Vector128<uint>.Count)
                        {
                            var lowPart = Sse2.ShiftRightLogical(Sse2.LoadVector128(sp), 19);
                            var highPart = Sse2.ShiftLeftLogical(Sse2.LoadVector128(sp + 1), 13);
                            Sse2.Store(dp, Sse2.Or(lowPart, highPart));
                            sp += Vector128<uint>.Count;
                            dp += Vector128<uint>.Count;
                            count -= Vector128<uint>.Count;
                        }

                        break;
                    case 20:
                        while (count >= Vector128<uint>.Count)
                        {
                            var lowPart = Sse2.ShiftRightLogical(Sse2.LoadVector128(sp), 20);
                            var highPart = Sse2.ShiftLeftLogical(Sse2.LoadVector128(sp + 1), 12);
                            Sse2.Store(dp, Sse2.Or(lowPart, highPart));
                            sp += Vector128<uint>.Count;
                            dp += Vector128<uint>.Count;
                            count -= Vector128<uint>.Count;
                        }

                        break;
                    case 21:
                        while (count >= Vector128<uint>.Count)
                        {
                            var lowPart = Sse2.ShiftRightLogical(Sse2.LoadVector128(sp), 21);
                            var highPart = Sse2.ShiftLeftLogical(Sse2.LoadVector128(sp + 1), 11);
                            Sse2.Store(dp, Sse2.Or(lowPart, highPart));
                            sp += Vector128<uint>.Count;
                            dp += Vector128<uint>.Count;
                            count -= Vector128<uint>.Count;
                        }

                        break;
                    case 22:
                        while (count >= Vector128<uint>.Count)
                        {
                            var lowPart = Sse2.ShiftRightLogical(Sse2.LoadVector128(sp), 22);
                            var highPart = Sse2.ShiftLeftLogical(Sse2.LoadVector128(sp + 1), 10);
                            Sse2.Store(dp, Sse2.Or(lowPart, highPart));
                            sp += Vector128<uint>.Count;
                            dp += Vector128<uint>.Count;
                            count -= Vector128<uint>.Count;
                        }

                        break;
                    case 23:
                        while (count >= Vector128<uint>.Count)
                        {
                            var lowPart = Sse2.ShiftRightLogical(Sse2.LoadVector128(sp), 23);
                            var highPart = Sse2.ShiftLeftLogical(Sse2.LoadVector128(sp + 1), 9);
                            Sse2.Store(dp, Sse2.Or(lowPart, highPart));
                            sp += Vector128<uint>.Count;
                            dp += Vector128<uint>.Count;
                            count -= Vector128<uint>.Count;
                        }

                        break;
                    case 24:
                        {
                            var byteLength = unchecked((uint)length << 2);
                            Assert(byteLength == length * 4);
                            Unsafe.CopyBlockUnaligned((byte*)dp, (byte*)sp + 3, byteLength - 3);
                            return (byte*)dp + byteLength - 3;
                        }
                    case 25:
                        while (count >= Vector128<uint>.Count)
                        {
                            var lowPart = Sse2.ShiftRightLogical(Sse2.LoadVector128(sp), 25);
                            var highPart = Sse2.ShiftLeftLogical(Sse2.LoadVector128(sp + 1), 7);
                            Sse2.Store(dp, Sse2.Or(lowPart, highPart));
                            sp += Vector128<uint>.Count;
                            dp += Vector128<uint>.Count;
                            count -= Vector128<uint>.Count;
                        }

                        break;
                    case 26:
                        while (count >= Vector128<uint>.Count)
                        {
                            var lowPart = Sse2.ShiftRightLogical(Sse2.LoadVector128(sp), 26);
                            var highPart = Sse2.ShiftLeftLogical(Sse2.LoadVector128(sp + 1), 6);
                            Sse2.Store(dp, Sse2.Or(lowPart, highPart));
                            sp += Vector128<uint>.Count;
                            dp += Vector128<uint>.Count;
                            count -= Vector128<uint>.Count;
                        }

                        break;
                    case 27:
                        while (count >= Vector128<uint>.Count)
                        {
                            var lowPart = Sse2.ShiftRightLogical(Sse2.LoadVector128(sp), 27);
                            var highPart = Sse2.ShiftLeftLogical(Sse2.LoadVector128(sp + 1), 5);
                            Sse2.Store(dp, Sse2.Or(lowPart, highPart));
                            sp += Vector128<uint>.Count;
                            dp += Vector128<uint>.Count;
                            count -= Vector128<uint>.Count;
                        }

                        break;
                    case 28:
                        while (count >= Vector128<uint>.Count)
                        {
                            var lowPart = Sse2.ShiftRightLogical(Sse2.LoadVector128(sp), 28);
                            var highPart = Sse2.ShiftLeftLogical(Sse2.LoadVector128(sp + 1), 4);
                            Sse2.Store(dp, Sse2.Or(lowPart, highPart));
                            sp += Vector128<uint>.Count;
                            dp += Vector128<uint>.Count;
                            count -= Vector128<uint>.Count;
                        }

                        break;
                    case 29:
                        while (count >= Vector128<uint>.Count)
                        {
                            var lowPart = Sse2.ShiftRightLogical(Sse2.LoadVector128(sp), 29);
                            var highPart = Sse2.ShiftLeftLogical(Sse2.LoadVector128(sp + 1), 3);
                            Sse2.Store(dp, Sse2.Or(lowPart, highPart));
                            sp += Vector128<uint>.Count;
                            dp += Vector128<uint>.Count;
                            count -= Vector128<uint>.Count;
                        }

                        break;
                    case 30:
                        while (count >= Vector128<uint>.Count)
                        {
                            var lowPart = Sse2.ShiftRightLogical(Sse2.LoadVector128(sp), 30);
                            var highPart = Sse2.ShiftLeftLogical(Sse2.LoadVector128(sp + 1), 2);
                            Sse2.Store(dp, Sse2.Or(lowPart, highPart));
                            sp += Vector128<uint>.Count;
                            dp += Vector128<uint>.Count;
                            count -= Vector128<uint>.Count;
                        }

                        break;
                    case 31:
                    default:
                        while (count >= Vector128<uint>.Count)
                        {
                            var lowPart = Sse2.ShiftRightLogical(Sse2.LoadVector128(sp), 31);
                            var highPart = Sse2.ShiftLeftLogical(Sse2.LoadVector128(sp + 1), 1);
                            Sse2.Store(dp, Sse2.Or(lowPart, highPart));
                            sp += Vector128<uint>.Count;
                            dp += Vector128<uint>.Count;
                            count -= Vector128<uint>.Count;
                        }

                        break;
                }

                return ShiftRightLesserThan4Words(sp, dp, count, rightShiftCount);
            }

            [MethodImpl(MethodImplOptions.AggressiveOptimization)]
            static unsafe byte* OperateVectorByAdvSimd(uint* sp, uint* dp, int length, int rightShiftCount)
            {
                Assert(AdvSimd.IsSupported == true);
                Assert(length > 0);
                Assert(rightShiftCount is >= 1 and <= 31);

                var count = length - 1;
                switch (rightShiftCount)
                {
                    case 1:
                        while (count >= Vector128<uint>.Count)
                        {
                            var lowPart = AdvSimd.ShiftRightLogical(AdvSimd.LoadVector128(sp), 1);
                            var highPart = AdvSimd.ShiftLeftLogical(AdvSimd.LoadVector128(sp + 1), 31);
                            AdvSimd.Store(dp, AdvSimd.Or(lowPart, highPart));
                            sp += Vector128<uint>.Count;
                            dp += Vector128<uint>.Count;
                            count -= Vector128<uint>.Count;
                        }

                        break;
                    case 2:
                        while (count >= Vector128<uint>.Count)
                        {
                            var lowPart = AdvSimd.ShiftRightLogical(AdvSimd.LoadVector128(sp), 2);
                            var highPart = AdvSimd.ShiftLeftLogical(AdvSimd.LoadVector128(sp + 1), 30);
                            AdvSimd.Store(dp, AdvSimd.Or(lowPart, highPart));
                            sp += Vector128<uint>.Count;
                            dp += Vector128<uint>.Count;
                            count -= Vector128<uint>.Count;
                        }

                        break;
                    case 3:
                        while (count >= Vector128<uint>.Count)
                        {
                            var lowPart = AdvSimd.ShiftRightLogical(AdvSimd.LoadVector128(sp), 3);
                            var highPart = AdvSimd.ShiftLeftLogical(AdvSimd.LoadVector128(sp + 1), 29);
                            AdvSimd.Store(dp, AdvSimd.Or(lowPart, highPart));
                            sp += Vector128<uint>.Count;
                            dp += Vector128<uint>.Count;
                            count -= Vector128<uint>.Count;
                        }

                        break;
                    case 4:
                        while (count >= Vector128<uint>.Count)
                        {
                            var lowPart = AdvSimd.ShiftRightLogical(AdvSimd.LoadVector128(sp), 4);
                            var highPart = AdvSimd.ShiftLeftLogical(AdvSimd.LoadVector128(sp + 1), 28);
                            AdvSimd.Store(dp, AdvSimd.Or(lowPart, highPart));
                            sp += Vector128<uint>.Count;
                            dp += Vector128<uint>.Count;
                            count -= Vector128<uint>.Count;
                        }

                        break;
                    case 5:
                        while (count >= Vector128<uint>.Count)
                        {
                            var lowPart = AdvSimd.ShiftRightLogical(AdvSimd.LoadVector128(sp), 5);
                            var highPart = AdvSimd.ShiftLeftLogical(AdvSimd.LoadVector128(sp + 1), 27);
                            AdvSimd.Store(dp, AdvSimd.Or(lowPart, highPart));
                            sp += Vector128<uint>.Count;
                            dp += Vector128<uint>.Count;
                            count -= Vector128<uint>.Count;
                        }

                        break;
                    case 6:
                        while (count >= Vector128<uint>.Count)
                        {
                            var lowPart = AdvSimd.ShiftRightLogical(AdvSimd.LoadVector128(sp), 6);
                            var highPart = AdvSimd.ShiftLeftLogical(AdvSimd.LoadVector128(sp + 1), 26);
                            AdvSimd.Store(dp, AdvSimd.Or(lowPart, highPart));
                            sp += Vector128<uint>.Count;
                            dp += Vector128<uint>.Count;
                            count -= Vector128<uint>.Count;
                        }

                        break;
                    case 7:
                        while (count >= Vector128<uint>.Count)
                        {
                            var lowPart = AdvSimd.ShiftRightLogical(AdvSimd.LoadVector128(sp), 7);
                            var highPart = AdvSimd.ShiftLeftLogical(AdvSimd.LoadVector128(sp + 1), 25);
                            AdvSimd.Store(dp, AdvSimd.Or(lowPart, highPart));
                            sp += Vector128<uint>.Count;
                            dp += Vector128<uint>.Count;
                            count -= Vector128<uint>.Count;
                        }

                        break;
                    case 8:
                        {
                            var byteLength = unchecked((uint)length << 2);
                            Assert(byteLength == length * 4);
                            Unsafe.CopyBlockUnaligned((byte*)dp, (byte*)sp + 1, byteLength - 1);
                            return (byte*)dp + byteLength - 1;
                        }
                    case 9:
                        while (count >= Vector128<uint>.Count)
                        {
                            var lowPart = AdvSimd.ShiftRightLogical(AdvSimd.LoadVector128(sp), 9);
                            var highPart = AdvSimd.ShiftLeftLogical(AdvSimd.LoadVector128(sp + 1), 23);
                            AdvSimd.Store(dp, AdvSimd.Or(lowPart, highPart));
                            sp += Vector128<uint>.Count;
                            dp += Vector128<uint>.Count;
                            count -= Vector128<uint>.Count;
                        }

                        break;
                    case 10:
                        while (count >= Vector128<uint>.Count)
                        {
                            var lowPart = AdvSimd.ShiftRightLogical(AdvSimd.LoadVector128(sp), 10);
                            var highPart = AdvSimd.ShiftLeftLogical(AdvSimd.LoadVector128(sp + 1), 22);
                            AdvSimd.Store(dp, AdvSimd.Or(lowPart, highPart));
                            sp += Vector128<uint>.Count;
                            dp += Vector128<uint>.Count;
                            count -= Vector128<uint>.Count;
                        }

                        break;
                    case 11:
                        while (count >= Vector128<uint>.Count)
                        {
                            var lowPart = AdvSimd.ShiftRightLogical(AdvSimd.LoadVector128(sp), 11);
                            var highPart = AdvSimd.ShiftLeftLogical(AdvSimd.LoadVector128(sp + 1), 21);
                            AdvSimd.Store(dp, AdvSimd.Or(lowPart, highPart));
                            sp += Vector128<uint>.Count;
                            dp += Vector128<uint>.Count;
                            count -= Vector128<uint>.Count;
                        }

                        break;
                    case 12:
                        while (count >= Vector128<uint>.Count)
                        {
                            var lowPart = AdvSimd.ShiftRightLogical(AdvSimd.LoadVector128(sp), 12);
                            var highPart = AdvSimd.ShiftLeftLogical(AdvSimd.LoadVector128(sp + 1), 20);
                            AdvSimd.Store(dp, AdvSimd.Or(lowPart, highPart));
                            sp += Vector128<uint>.Count;
                            dp += Vector128<uint>.Count;
                            count -= Vector128<uint>.Count;
                        }

                        break;
                    case 13:
                        while (count >= Vector128<uint>.Count)
                        {
                            var lowPart = AdvSimd.ShiftRightLogical(AdvSimd.LoadVector128(sp), 13);
                            var highPart = AdvSimd.ShiftLeftLogical(AdvSimd.LoadVector128(sp + 1), 19);
                            AdvSimd.Store(dp, AdvSimd.Or(lowPart, highPart));
                            sp += Vector128<uint>.Count;
                            dp += Vector128<uint>.Count;
                            count -= Vector128<uint>.Count;
                        }

                        break;
                    case 14:
                        while (count >= Vector128<uint>.Count)
                        {
                            var lowPart = AdvSimd.ShiftRightLogical(AdvSimd.LoadVector128(sp), 14);
                            var highPart = AdvSimd.ShiftLeftLogical(AdvSimd.LoadVector128(sp + 1), 18);
                            AdvSimd.Store(dp, AdvSimd.Or(lowPart, highPart));
                            sp += Vector128<uint>.Count;
                            dp += Vector128<uint>.Count;
                            count -= Vector128<uint>.Count;
                        }

                        break;
                    case 15:
                        while (count >= Vector128<uint>.Count)
                        {
                            var lowPart = AdvSimd.ShiftRightLogical(AdvSimd.LoadVector128(sp), 15);
                            var highPart = AdvSimd.ShiftLeftLogical(AdvSimd.LoadVector128(sp + 1), 17);
                            AdvSimd.Store(dp, AdvSimd.Or(lowPart, highPart));
                            sp += Vector128<uint>.Count;
                            dp += Vector128<uint>.Count;
                            count -= Vector128<uint>.Count;
                        }

                        break;
                    case 16:
                        {
                            var byteLength = unchecked((uint)length << 2);
                            Assert(byteLength == length * 4);
                            Unsafe.CopyBlockUnaligned((byte*)dp, (byte*)sp + 2, byteLength - 2);
                            return (byte*)dp + byteLength - 2;
                        }
                    case 17:
                        while (count >= Vector128<uint>.Count)
                        {
                            var lowPart = AdvSimd.ShiftRightLogical(AdvSimd.LoadVector128(sp), 17);
                            var highPart = AdvSimd.ShiftLeftLogical(AdvSimd.LoadVector128(sp + 1), 15);
                            AdvSimd.Store(dp, AdvSimd.Or(lowPart, highPart));
                            sp += Vector128<uint>.Count;
                            dp += Vector128<uint>.Count;
                            count -= Vector128<uint>.Count;
                        }

                        break;
                    case 18:
                        while (count >= Vector128<uint>.Count)
                        {
                            var lowPart = AdvSimd.ShiftRightLogical(AdvSimd.LoadVector128(sp), 18);
                            var highPart = AdvSimd.ShiftLeftLogical(AdvSimd.LoadVector128(sp + 1), 14);
                            AdvSimd.Store(dp, AdvSimd.Or(lowPart, highPart));
                            sp += Vector128<uint>.Count;
                            dp += Vector128<uint>.Count;
                            count -= Vector128<uint>.Count;
                        }

                        break;
                    case 19:
                        while (count >= Vector128<uint>.Count)
                        {
                            var lowPart = AdvSimd.ShiftRightLogical(AdvSimd.LoadVector128(sp), 19);
                            var highPart = AdvSimd.ShiftLeftLogical(AdvSimd.LoadVector128(sp + 1), 13);
                            AdvSimd.Store(dp, AdvSimd.Or(lowPart, highPart));
                            sp += Vector128<uint>.Count;
                            dp += Vector128<uint>.Count;
                            count -= Vector128<uint>.Count;
                        }

                        break;
                    case 20:
                        while (count >= Vector128<uint>.Count)
                        {
                            var lowPart = AdvSimd.ShiftRightLogical(AdvSimd.LoadVector128(sp), 20);
                            var highPart = AdvSimd.ShiftLeftLogical(AdvSimd.LoadVector128(sp + 1), 12);
                            AdvSimd.Store(dp, AdvSimd.Or(lowPart, highPart));
                            sp += Vector128<uint>.Count;
                            dp += Vector128<uint>.Count;
                            count -= Vector128<uint>.Count;
                        }

                        break;
                    case 21:
                        while (count >= Vector128<uint>.Count)
                        {
                            var lowPart = AdvSimd.ShiftRightLogical(AdvSimd.LoadVector128(sp), 21);
                            var highPart = AdvSimd.ShiftLeftLogical(AdvSimd.LoadVector128(sp + 1), 11);
                            AdvSimd.Store(dp, AdvSimd.Or(lowPart, highPart));
                            sp += Vector128<uint>.Count;
                            dp += Vector128<uint>.Count;
                            count -= Vector128<uint>.Count;
                        }

                        break;
                    case 22:
                        while (count >= Vector128<uint>.Count)
                        {
                            var lowPart = AdvSimd.ShiftRightLogical(AdvSimd.LoadVector128(sp), 22);
                            var highPart = AdvSimd.ShiftLeftLogical(AdvSimd.LoadVector128(sp + 1), 10);
                            AdvSimd.Store(dp, AdvSimd.Or(lowPart, highPart));
                            sp += Vector128<uint>.Count;
                            dp += Vector128<uint>.Count;
                            count -= Vector128<uint>.Count;
                        }

                        break;
                    case 23:
                        while (count >= Vector128<uint>.Count)
                        {
                            var lowPart = AdvSimd.ShiftRightLogical(AdvSimd.LoadVector128(sp), 23);
                            var highPart = AdvSimd.ShiftLeftLogical(AdvSimd.LoadVector128(sp + 1), 9);
                            AdvSimd.Store(dp, AdvSimd.Or(lowPart, highPart));
                            sp += Vector128<uint>.Count;
                            dp += Vector128<uint>.Count;
                            count -= Vector128<uint>.Count;
                        }

                        break;
                    case 24:
                        {
                            var byteLength = unchecked((uint)length << 2);
                            Assert(byteLength == length * 4);
                            Unsafe.CopyBlockUnaligned((byte*)dp, (byte*)sp + 3, byteLength - 3);
                            return (byte*)dp + byteLength - 3;
                        }
                    case 25:
                        while (count >= Vector128<uint>.Count)
                        {
                            var lowPart = AdvSimd.ShiftRightLogical(AdvSimd.LoadVector128(sp), 25);
                            var highPart = AdvSimd.ShiftLeftLogical(AdvSimd.LoadVector128(sp + 1), 7);
                            AdvSimd.Store(dp, AdvSimd.Or(lowPart, highPart));
                            sp += Vector128<uint>.Count;
                            dp += Vector128<uint>.Count;
                            count -= Vector128<uint>.Count;
                        }

                        break;
                    case 26:
                        while (count >= Vector128<uint>.Count)
                        {
                            var lowPart = AdvSimd.ShiftRightLogical(AdvSimd.LoadVector128(sp), 26);
                            var highPart = AdvSimd.ShiftLeftLogical(AdvSimd.LoadVector128(sp + 1), 6);
                            AdvSimd.Store(dp, AdvSimd.Or(lowPart, highPart));
                            sp += Vector128<uint>.Count;
                            dp += Vector128<uint>.Count;
                            count -= Vector128<uint>.Count;
                        }

                        break;
                    case 27:
                        while (count >= Vector128<uint>.Count)
                        {
                            var lowPart = AdvSimd.ShiftRightLogical(AdvSimd.LoadVector128(sp), 27);
                            var highPart = AdvSimd.ShiftLeftLogical(AdvSimd.LoadVector128(sp + 1), 5);
                            AdvSimd.Store(dp, AdvSimd.Or(lowPart, highPart));
                            sp += Vector128<uint>.Count;
                            dp += Vector128<uint>.Count;
                            count -= Vector128<uint>.Count;
                        }

                        break;
                    case 28:
                        while (count >= Vector128<uint>.Count)
                        {
                            var lowPart = AdvSimd.ShiftRightLogical(AdvSimd.LoadVector128(sp), 28);
                            var highPart = AdvSimd.ShiftLeftLogical(AdvSimd.LoadVector128(sp + 1), 4);
                            AdvSimd.Store(dp, AdvSimd.Or(lowPart, highPart));
                            sp += Vector128<uint>.Count;
                            dp += Vector128<uint>.Count;
                            count -= Vector128<uint>.Count;
                        }

                        break;
                    case 29:
                        while (count >= Vector128<uint>.Count)
                        {
                            var lowPart = AdvSimd.ShiftRightLogical(AdvSimd.LoadVector128(sp), 29);
                            var highPart = AdvSimd.ShiftLeftLogical(AdvSimd.LoadVector128(sp + 1), 3);
                            AdvSimd.Store(dp, AdvSimd.Or(lowPart, highPart));
                            sp += Vector128<uint>.Count;
                            dp += Vector128<uint>.Count;
                            count -= Vector128<uint>.Count;
                        }

                        break;
                    case 30:
                        while (count >= Vector128<uint>.Count)
                        {
                            var lowPart = AdvSimd.ShiftRightLogical(AdvSimd.LoadVector128(sp), 30);
                            var highPart = AdvSimd.ShiftLeftLogical(AdvSimd.LoadVector128(sp + 1), 2);
                            AdvSimd.Store(dp, AdvSimd.Or(lowPart, highPart));
                            sp += Vector128<uint>.Count;
                            dp += Vector128<uint>.Count;
                            count -= Vector128<uint>.Count;
                        }

                        break;
                    case 31:
                    default:
                        while (count >= Vector128<uint>.Count)
                        {
                            var lowPart = AdvSimd.ShiftRightLogical(AdvSimd.LoadVector128(sp), 31);
                            var highPart = AdvSimd.ShiftLeftLogical(AdvSimd.LoadVector128(sp + 1), 1);
                            AdvSimd.Store(dp, AdvSimd.Or(lowPart, highPart));
                            sp += Vector128<uint>.Count;
                            dp += Vector128<uint>.Count;
                            count -= Vector128<uint>.Count;
                        }

                        break;
                }

                return ShiftRightLesserThan4Words(sp, dp, count, rightShiftCount);
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
            static unsafe byte* OperateVectorByPackedSimd(uint* sp, uint* dp, int length, int rightShiftCount)
            {
                Assert(PackedSimd.IsSupported == true);
                Assert(length > 0);
                Assert(rightShiftCount is >= 1 and <= 31);

                var leftShiftCount = _BIT_COUNT_PER_UINT32 - rightShiftCount;
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

                return ShiftRightLesserThan4Words(sp, dp, count, rightShiftCount);
            }

            [MethodImpl(MethodImplOptions.AggressiveOptimization)]
            static unsafe byte* ShiftRightByDefault(uint* sp, uint* dp, int length, int rightShiftCount)
            {
                Assert(length > 0);
                Assert(rightShiftCount is >= 1 and <= 31);

                var leftShiftCount = _BIT_COUNT_PER_UINT32 - rightShiftCount;
                var lowBits = *sp++ >> rightShiftCount;
                var count = length - 1;

                while (count >= 32)
                {
                    var nextBits1 = (ulong)sp[0] << leftShiftCount;
                    dp[0] = lowBits | (uint)nextBits1;
                    var nextBits2 = (ulong)sp[1] << leftShiftCount;
                    dp[1] = (uint)(nextBits1 >> _BIT_COUNT_PER_UINT32) | (uint)nextBits2;
                    nextBits1 = (ulong)sp[2] << leftShiftCount;
                    dp[2] = (uint)(nextBits2 >> _BIT_COUNT_PER_UINT32) | (uint)nextBits1;
                    nextBits2 = (ulong)sp[3] << leftShiftCount;
                    dp[3] = (uint)(nextBits1 >> _BIT_COUNT_PER_UINT32) | (uint)nextBits2;
                    nextBits1 = (ulong)sp[4] << leftShiftCount;
                    dp[4] = (uint)(nextBits2 >> _BIT_COUNT_PER_UINT32) | (uint)nextBits1;
                    nextBits2 = (ulong)sp[5] << leftShiftCount;
                    dp[5] = (uint)(nextBits1 >> _BIT_COUNT_PER_UINT32) | (uint)nextBits2;
                    nextBits1 = (ulong)sp[6] << leftShiftCount;
                    dp[6] = (uint)(nextBits2 >> _BIT_COUNT_PER_UINT32) | (uint)nextBits1;
                    nextBits2 = (ulong)sp[7] << leftShiftCount;
                    dp[7] = (uint)(nextBits1 >> _BIT_COUNT_PER_UINT32) | (uint)nextBits2;
                    nextBits1 = (ulong)sp[8] << leftShiftCount;
                    dp[8] = (uint)(nextBits2 >> _BIT_COUNT_PER_UINT32) | (uint)nextBits1;
                    nextBits2 = (ulong)sp[9] << leftShiftCount;
                    dp[9] = (uint)(nextBits1 >> _BIT_COUNT_PER_UINT32) | (uint)nextBits2;
                    nextBits1 = (ulong)sp[10] << leftShiftCount;
                    dp[10] = (uint)(nextBits2 >> _BIT_COUNT_PER_UINT32) | (uint)nextBits1;
                    nextBits2 = (ulong)sp[11] << leftShiftCount;
                    dp[11] = (uint)(nextBits1 >> _BIT_COUNT_PER_UINT32) | (uint)nextBits2;
                    nextBits1 = (ulong)sp[12] << leftShiftCount;
                    dp[12] = (uint)(nextBits2 >> _BIT_COUNT_PER_UINT32) | (uint)nextBits1;
                    nextBits2 = (ulong)sp[13] << leftShiftCount;
                    dp[13] = (uint)(nextBits1 >> _BIT_COUNT_PER_UINT32) | (uint)nextBits2;
                    nextBits1 = (ulong)sp[14] << leftShiftCount;
                    dp[14] = (uint)(nextBits2 >> _BIT_COUNT_PER_UINT32) | (uint)nextBits1;
                    nextBits2 = (ulong)sp[15] << leftShiftCount;
                    dp[15] = (uint)(nextBits1 >> _BIT_COUNT_PER_UINT32) | (uint)nextBits2;
                    nextBits1 = (ulong)sp[16] << leftShiftCount;
                    dp[16] = (uint)(nextBits2 >> _BIT_COUNT_PER_UINT32) | (uint)nextBits1;
                    nextBits2 = (ulong)sp[17] << leftShiftCount;
                    dp[17] = (uint)(nextBits1 >> _BIT_COUNT_PER_UINT32) | (uint)nextBits2;
                    nextBits1 = (ulong)sp[18] << leftShiftCount;
                    dp[18] = (uint)(nextBits2 >> _BIT_COUNT_PER_UINT32) | (uint)nextBits1;
                    nextBits2 = (ulong)sp[19] << leftShiftCount;
                    dp[19] = (uint)(nextBits1 >> _BIT_COUNT_PER_UINT32) | (uint)nextBits2;
                    nextBits1 = (ulong)sp[20] << leftShiftCount;
                    dp[20] = (uint)(nextBits2 >> _BIT_COUNT_PER_UINT32) | (uint)nextBits1;
                    nextBits2 = (ulong)sp[21] << leftShiftCount;
                    dp[21] = (uint)(nextBits1 >> _BIT_COUNT_PER_UINT32) | (uint)nextBits2;
                    nextBits1 = (ulong)sp[22] << leftShiftCount;
                    dp[22] = (uint)(nextBits2 >> _BIT_COUNT_PER_UINT32) | (uint)nextBits1;
                    nextBits2 = (ulong)sp[23] << leftShiftCount;
                    dp[23] = (uint)(nextBits1 >> _BIT_COUNT_PER_UINT32) | (uint)nextBits2;
                    nextBits1 = (ulong)sp[24] << leftShiftCount;
                    dp[24] = (uint)(nextBits2 >> _BIT_COUNT_PER_UINT32) | (uint)nextBits1;
                    nextBits2 = (ulong)sp[25] << leftShiftCount;
                    dp[25] = (uint)(nextBits1 >> _BIT_COUNT_PER_UINT32) | (uint)nextBits2;
                    nextBits1 = (ulong)sp[26] << leftShiftCount;
                    dp[26] = (uint)(nextBits2 >> _BIT_COUNT_PER_UINT32) | (uint)nextBits1;
                    nextBits2 = (ulong)sp[27] << leftShiftCount;
                    dp[27] = (uint)(nextBits1 >> _BIT_COUNT_PER_UINT32) | (uint)nextBits2;
                    nextBits1 = (ulong)sp[28] << leftShiftCount;
                    dp[28] = (uint)(nextBits2 >> _BIT_COUNT_PER_UINT32) | (uint)nextBits1;
                    nextBits2 = (ulong)sp[29] << leftShiftCount;
                    dp[29] = (uint)(nextBits1 >> _BIT_COUNT_PER_UINT32) | (uint)nextBits2;
                    nextBits1 = (ulong)sp[30] << leftShiftCount;
                    dp[30] = (uint)(nextBits2 >> _BIT_COUNT_PER_UINT32) | (uint)nextBits1;
                    nextBits2 = (ulong)sp[31] << leftShiftCount;
                    dp[31] = (uint)(nextBits1 >> _BIT_COUNT_PER_UINT32) | (uint)nextBits2;
                    lowBits = (uint)(nextBits2 >> _BIT_COUNT_PER_UINT32);
                    dp += 32;
                    sp += 32;
                    count -= 32;
                }

                if (count >= 16)
                {
                    var nextBits1 = (ulong)sp[0] << leftShiftCount;
                    dp[0] = lowBits | (uint)nextBits1;
                    var nextBits2 = (ulong)sp[1] << leftShiftCount;
                    dp[1] = (uint)(nextBits1 >> _BIT_COUNT_PER_UINT32) | (uint)nextBits2;
                    nextBits1 = (ulong)sp[2] << leftShiftCount;
                    dp[2] = (uint)(nextBits2 >> _BIT_COUNT_PER_UINT32) | (uint)nextBits1;
                    nextBits2 = (ulong)sp[3] << leftShiftCount;
                    dp[3] = (uint)(nextBits1 >> _BIT_COUNT_PER_UINT32) | (uint)nextBits2;
                    nextBits1 = (ulong)sp[4] << leftShiftCount;
                    dp[4] = (uint)(nextBits2 >> _BIT_COUNT_PER_UINT32) | (uint)nextBits1;
                    nextBits2 = (ulong)sp[5] << leftShiftCount;
                    dp[5] = (uint)(nextBits1 >> _BIT_COUNT_PER_UINT32) | (uint)nextBits2;
                    nextBits1 = (ulong)sp[6] << leftShiftCount;
                    dp[6] = (uint)(nextBits2 >> _BIT_COUNT_PER_UINT32) | (uint)nextBits1;
                    nextBits2 = (ulong)sp[7] << leftShiftCount;
                    dp[7] = (uint)(nextBits1 >> _BIT_COUNT_PER_UINT32) | (uint)nextBits2;
                    nextBits1 = (ulong)sp[8] << leftShiftCount;
                    dp[8] = (uint)(nextBits2 >> _BIT_COUNT_PER_UINT32) | (uint)nextBits1;
                    nextBits2 = (ulong)sp[9] << leftShiftCount;
                    dp[9] = (uint)(nextBits1 >> _BIT_COUNT_PER_UINT32) | (uint)nextBits2;
                    nextBits1 = (ulong)sp[10] << leftShiftCount;
                    dp[10] = (uint)(nextBits2 >> _BIT_COUNT_PER_UINT32) | (uint)nextBits1;
                    nextBits2 = (ulong)sp[11] << leftShiftCount;
                    dp[11] = (uint)(nextBits1 >> _BIT_COUNT_PER_UINT32) | (uint)nextBits2;
                    nextBits1 = (ulong)sp[12] << leftShiftCount;
                    dp[12] = (uint)(nextBits2 >> _BIT_COUNT_PER_UINT32) | (uint)nextBits1;
                    nextBits2 = (ulong)sp[13] << leftShiftCount;
                    dp[13] = (uint)(nextBits1 >> _BIT_COUNT_PER_UINT32) | (uint)nextBits2;
                    nextBits1 = (ulong)sp[14] << leftShiftCount;
                    dp[14] = (uint)(nextBits2 >> _BIT_COUNT_PER_UINT32) | (uint)nextBits1;
                    nextBits2 = (ulong)sp[15] << leftShiftCount;
                    dp[15] = (uint)(nextBits1 >> _BIT_COUNT_PER_UINT32) | (uint)nextBits2;
                    lowBits = (uint)(nextBits2 >> _BIT_COUNT_PER_UINT32);
                    dp += 16;
                    sp += 16;
                    count -= 16;
                }

                if (count >= 8)
                {
                    var nextBits1 = (ulong)sp[0] << leftShiftCount;
                    dp[0] = lowBits | (uint)nextBits1;
                    var nextBits2 = (ulong)sp[1] << leftShiftCount;
                    dp[1] = (uint)(nextBits1 >> _BIT_COUNT_PER_UINT32) | (uint)nextBits2;
                    nextBits1 = (ulong)sp[2] << leftShiftCount;
                    dp[2] = (uint)(nextBits2 >> _BIT_COUNT_PER_UINT32) | (uint)nextBits1;
                    nextBits2 = (ulong)sp[3] << leftShiftCount;
                    dp[3] = (uint)(nextBits1 >> _BIT_COUNT_PER_UINT32) | (uint)nextBits2;
                    nextBits1 = (ulong)sp[4] << leftShiftCount;
                    dp[4] = (uint)(nextBits2 >> _BIT_COUNT_PER_UINT32) | (uint)nextBits1;
                    nextBits2 = (ulong)sp[5] << leftShiftCount;
                    dp[5] = (uint)(nextBits1 >> _BIT_COUNT_PER_UINT32) | (uint)nextBits2;
                    nextBits1 = (ulong)sp[6] << leftShiftCount;
                    dp[6] = (uint)(nextBits2 >> _BIT_COUNT_PER_UINT32) | (uint)nextBits1;
                    nextBits2 = (ulong)sp[7] << leftShiftCount;
                    dp[7] = (uint)(nextBits1 >> _BIT_COUNT_PER_UINT32) | (uint)nextBits2;
                    lowBits = (uint)(nextBits2 >> _BIT_COUNT_PER_UINT32);
                    dp += 8;
                    sp += 8;
                    count -= 8;
                }

                if (count >= 4)
                {
                    var nextBits1 = (ulong)sp[0] << leftShiftCount;
                    dp[0] = lowBits | (uint)nextBits1;
                    var nextBits2 = (ulong)sp[1] << leftShiftCount;
                    dp[1] = (uint)(nextBits1 >> _BIT_COUNT_PER_UINT32) | (uint)nextBits2;
                    nextBits1 = (ulong)sp[2] << leftShiftCount;
                    dp[2] = (uint)(nextBits2 >> _BIT_COUNT_PER_UINT32) | (uint)nextBits1;
                    nextBits2 = (ulong)sp[3] << leftShiftCount;
                    dp[3] = (uint)(nextBits1 >> _BIT_COUNT_PER_UINT32) | (uint)nextBits2;
                    lowBits = (uint)(nextBits2 >> _BIT_COUNT_PER_UINT32);
                    dp += 4;
                    sp += 4;
                    count -= 4;
                }

                if (count >= 2)
                {
                    var nextBits1 = (ulong)sp[0] << leftShiftCount;
                    dp[0] = lowBits | (uint)nextBits1;
                    var nextBits2 = (ulong)sp[1] << leftShiftCount;
                    dp[1] = (uint)(nextBits1 >> _BIT_COUNT_PER_UINT32) | (uint)nextBits2;
                    lowBits = (uint)(nextBits2 >> _BIT_COUNT_PER_UINT32);
                    dp += 2;
                    sp += 2;
                    count -= 2;
                }

                if (count > 0)
                {
                    var nextBits = (ulong)*sp++ << leftShiftCount;
                    *dp++ = lowBits | (uint)nextBits;
                    lowBits = (uint)(nextBits >> _BIT_COUNT_PER_UINT32);
                    --count;
                }

                *dp++ = lowBits;
                return (byte*)dp;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
            static unsafe byte* ShiftRightLesserThan16Words(uint* sp, uint* dp, int count, int rightShiftCount)
            {
                Assert(count is >= 0 and < 16);
                Assert(rightShiftCount is >= 1 and <= 31);

                var leftShiftCount = _BIT_COUNT_PER_UINT32 - rightShiftCount;
                var lowBits = *sp++ >> rightShiftCount;

                if (count >= 8)
                {
                    var nextBits1 = (ulong)sp[0] << leftShiftCount;
                    dp[0] = lowBits | (uint)nextBits1;
                    var nextBits2 = (ulong)sp[1] << leftShiftCount;
                    dp[1] = (uint)(nextBits1 >> _BIT_COUNT_PER_UINT32) | (uint)nextBits2;
                    nextBits1 = (ulong)sp[2] << leftShiftCount;
                    dp[2] = (uint)(nextBits2 >> _BIT_COUNT_PER_UINT32) | (uint)nextBits1;
                    nextBits2 = (ulong)sp[3] << leftShiftCount;
                    dp[3] = (uint)(nextBits1 >> _BIT_COUNT_PER_UINT32) | (uint)nextBits2;
                    nextBits1 = (ulong)sp[4] << leftShiftCount;
                    dp[4] = (uint)(nextBits2 >> _BIT_COUNT_PER_UINT32) | (uint)nextBits1;
                    nextBits2 = (ulong)sp[5] << leftShiftCount;
                    dp[5] = (uint)(nextBits1 >> _BIT_COUNT_PER_UINT32) | (uint)nextBits2;
                    nextBits1 = (ulong)sp[6] << leftShiftCount;
                    dp[6] = (uint)(nextBits2 >> _BIT_COUNT_PER_UINT32) | (uint)nextBits1;
                    nextBits2 = (ulong)sp[7] << leftShiftCount;
                    dp[7] = (uint)(nextBits1 >> _BIT_COUNT_PER_UINT32) | (uint)nextBits2;
                    lowBits = (uint)(nextBits2 >> _BIT_COUNT_PER_UINT32);
                    dp += 8;
                    sp += 8;
                    count -= 8;
                }

                if (count >= 4)
                {
                    var nextBits1 = (ulong)sp[0] << leftShiftCount;
                    dp[0] = lowBits | (uint)nextBits1;
                    var nextBits2 = (ulong)sp[1] << leftShiftCount;
                    dp[1] = (uint)(nextBits1 >> _BIT_COUNT_PER_UINT32) | (uint)nextBits2;
                    nextBits1 = (ulong)sp[2] << leftShiftCount;
                    dp[2] = (uint)(nextBits2 >> _BIT_COUNT_PER_UINT32) | (uint)nextBits1;
                    nextBits2 = (ulong)sp[3] << leftShiftCount;
                    dp[3] = (uint)(nextBits1 >> _BIT_COUNT_PER_UINT32) | (uint)nextBits2;
                    lowBits = (uint)(nextBits2 >> _BIT_COUNT_PER_UINT32);
                    dp += 4;
                    sp += 4;
                    count -= 4;
                }

                if (count >= 2)
                {
                    var nextBits1 = (ulong)sp[0] << leftShiftCount;
                    dp[0] = lowBits | (uint)nextBits1;
                    var nextBits2 = (ulong)sp[1] << leftShiftCount;
                    dp[1] = (uint)(nextBits1 >> _BIT_COUNT_PER_UINT32) | (uint)nextBits2;
                    lowBits = (uint)(nextBits2 >> _BIT_COUNT_PER_UINT32);
                    dp += 2;
                    sp += 2;
                    count -= 2;
                }

                if (count > 0)
                {
                    var nextBits = (ulong)*sp++ << leftShiftCount;
                    *dp++ = lowBits | (uint)nextBits;
                    lowBits = (uint)(nextBits >> _BIT_COUNT_PER_UINT32);
                    --count;
                }

                *dp++ = lowBits;
                return (byte*)dp;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
            static unsafe byte* ShiftRightLesserThan8Words(uint* sp, uint* dp, int count, int rightShiftCount)
            {
                Assert(count is >= 0 and < 8);
                Assert(rightShiftCount is >= 1 and <= 31);

                var leftShiftCount = _BIT_COUNT_PER_UINT32 - rightShiftCount;
                var lowBits = *sp++ >> rightShiftCount;

                if (count >= 4)
                {
                    var nextBits1 = (ulong)sp[0] << leftShiftCount;
                    dp[0] = lowBits | (uint)nextBits1;
                    var nextBits2 = (ulong)sp[1] << leftShiftCount;
                    dp[1] = (uint)(nextBits1 >> _BIT_COUNT_PER_UINT32) | (uint)nextBits2;
                    nextBits1 = (ulong)sp[2] << leftShiftCount;
                    dp[2] = (uint)(nextBits2 >> _BIT_COUNT_PER_UINT32) | (uint)nextBits1;
                    nextBits2 = (ulong)sp[3] << leftShiftCount;
                    dp[3] = (uint)(nextBits1 >> _BIT_COUNT_PER_UINT32) | (uint)nextBits2;
                    lowBits = (uint)(nextBits2 >> _BIT_COUNT_PER_UINT32);
                    dp += 4;
                    sp += 4;
                    count -= 4;
                }

                if (count >= 2)
                {
                    var nextBits1 = (ulong)sp[0] << leftShiftCount;
                    dp[0] = lowBits | (uint)nextBits1;
                    var nextBits2 = (ulong)sp[1] << leftShiftCount;
                    dp[1] = (uint)(nextBits1 >> _BIT_COUNT_PER_UINT32) | (uint)nextBits2;
                    lowBits = (uint)(nextBits2 >> _BIT_COUNT_PER_UINT32);
                    dp += 2;
                    sp += 2;
                    count -= 2;
                }

                if (count > 0)
                {
                    var nextBits = (ulong)*sp++ << leftShiftCount;
                    *dp++ = lowBits | (uint)nextBits;
                    lowBits = (uint)(nextBits >> _BIT_COUNT_PER_UINT32);
                    --count;
                }

                *dp++ = lowBits;
                return (byte*)dp;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
            static unsafe byte* ShiftRightLesserThan4Words(uint* sp, uint* dp, int count, int rightShiftCount)
            {
                Assert(count is >= 0 and < 4);
                Assert(rightShiftCount is >= 1 and <= 31);

                var leftShiftCount = _BIT_COUNT_PER_UINT32 - rightShiftCount;
                var lowBits = *sp++ >> rightShiftCount;

                if (count >= 2)
                {
                    var nextBits1 = (ulong)sp[0] << leftShiftCount;
                    dp[0] = lowBits | (uint)nextBits1;
                    var nextBits2 = (ulong)sp[1] << leftShiftCount;
                    dp[1] = (uint)(nextBits1 >> _BIT_COUNT_PER_UINT32) | (uint)nextBits2;
                    lowBits = (uint)(nextBits2 >> _BIT_COUNT_PER_UINT32);
                    dp += 2;
                    sp += 2;
                    count -= 2;
                }

                if (count > 0)
                {
                    var nextBits = (ulong)*sp++ << leftShiftCount;
                    *dp++ = lowBits | (uint)nextBits;
                    lowBits = (uint)(nextBits >> _BIT_COUNT_PER_UINT32);
                    --count;
                }

                *dp++ = lowBits;
                return (byte*)dp;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
            static unsafe void Finish(byte* end, uint* buffer, int bufferSize)
            {
                Assert(end <= (byte*)(buffer + bufferSize));
                var remain = (byte*)(buffer + bufferSize) - end;
                NativeMemory.Clear(end, (nuint)remain);
            }
        }

        #endregion

        #region SubtractSelfTry1

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        private static void SubtractSelfTry1(Span<uint> left, ReadOnlySpan<uint> right)
        {
            Assert(left.Length > 0);
            Assert(right.Length > 0);
            Assert(left.Length >= right.Length);

            var borrow = 0L;
            var index = 0;
            var count = right.Length;
            while (count >= 32)
            {
                borrow += (long)left[index + 0] - right[index + 0];
                left[index + 0] = unchecked((uint)borrow);
                borrow >>= _BIT_COUNT_PER_UINT32;
                Assert(borrow is 0 or (-1));
                borrow += (long)left[index + 1] - right[index + 1];
                left[index + 1] = unchecked((uint)borrow);
                borrow >>= _BIT_COUNT_PER_UINT32;
                Assert(borrow is 0 or (-1));
                borrow += (long)left[index + 2] - right[index + 2];
                left[index + 2] = unchecked((uint)borrow);
                borrow >>= _BIT_COUNT_PER_UINT32;
                Assert(borrow is 0 or (-1));
                borrow += (long)left[index + 3] - right[index + 3];
                left[index + 3] = unchecked((uint)borrow);
                borrow >>= _BIT_COUNT_PER_UINT32;
                Assert(borrow is 0 or (-1));
                borrow += (long)left[index + 4] - right[index + 4];
                left[index + 4] = unchecked((uint)borrow);
                borrow >>= _BIT_COUNT_PER_UINT32;
                Assert(borrow is 0 or (-1));
                borrow += (long)left[index + 5] - right[index + 5];
                left[index + 5] = unchecked((uint)borrow);
                borrow >>= _BIT_COUNT_PER_UINT32;
                Assert(borrow is 0 or (-1));
                borrow += (long)left[index + 6] - right[index + 6];
                left[index + 6] = unchecked((uint)borrow);
                borrow >>= _BIT_COUNT_PER_UINT32;
                Assert(borrow is 0 or (-1));
                borrow += (long)left[index + 7] - right[index + 7];
                left[index + 7] = unchecked((uint)borrow);
                borrow >>= _BIT_COUNT_PER_UINT32;
                Assert(borrow is 0 or (-1));
                borrow += (long)left[index + 8] - right[index + 8];
                left[index + 8] = unchecked((uint)borrow);
                borrow >>= _BIT_COUNT_PER_UINT32;
                Assert(borrow is 0 or (-1));
                borrow += (long)left[index + 9] - right[index + 9];
                left[index + 9] = unchecked((uint)borrow);
                borrow >>= _BIT_COUNT_PER_UINT32;
                Assert(borrow is 0 or (-1));
                borrow += (long)left[index + 10] - right[index + 10];
                left[index + 10] = unchecked((uint)borrow);
                borrow >>= _BIT_COUNT_PER_UINT32;
                Assert(borrow is 0 or (-1));
                borrow += (long)left[index + 11] - right[index + 11];
                left[index + 11] = unchecked((uint)borrow);
                borrow >>= _BIT_COUNT_PER_UINT32;
                Assert(borrow is 0 or (-1));
                borrow += (long)left[index + 12] - right[index + 12];
                left[index + 12] = unchecked((uint)borrow);
                borrow >>= _BIT_COUNT_PER_UINT32;
                Assert(borrow is 0 or (-1));
                borrow += (long)left[index + 13] - right[index + 13];
                left[index + 13] = unchecked((uint)borrow);
                borrow >>= _BIT_COUNT_PER_UINT32;
                Assert(borrow is 0 or (-1));
                borrow += (long)left[index + 14] - right[index + 14];
                left[index + 14] = unchecked((uint)borrow);
                borrow >>= _BIT_COUNT_PER_UINT32;
                Assert(borrow is 0 or (-1));
                borrow += (long)left[index + 15] - right[index + 15];
                left[index + 15] = unchecked((uint)borrow);
                borrow >>= _BIT_COUNT_PER_UINT32;
                Assert(borrow is 0 or (-1));
                borrow += (long)left[index + 16] - right[index + 16];
                left[index + 16] = unchecked((uint)borrow);
                borrow >>= _BIT_COUNT_PER_UINT32;
                Assert(borrow is 0 or (-1));
                borrow += (long)left[index + 17] - right[index + 17];
                left[index + 17] = unchecked((uint)borrow);
                borrow >>= _BIT_COUNT_PER_UINT32;
                Assert(borrow is 0 or (-1));
                borrow += (long)left[index + 18] - right[index + 18];
                left[index + 18] = unchecked((uint)borrow);
                borrow >>= _BIT_COUNT_PER_UINT32;
                Assert(borrow is 0 or (-1));
                borrow += (long)left[index + 19] - right[index + 19];
                left[index + 19] = unchecked((uint)borrow);
                borrow >>= _BIT_COUNT_PER_UINT32;
                Assert(borrow is 0 or (-1));
                borrow += (long)left[index + 20] - right[index + 20];
                left[index + 20] = unchecked((uint)borrow);
                borrow >>= _BIT_COUNT_PER_UINT32;
                Assert(borrow is 0 or (-1));
                borrow += (long)left[index + 21] - right[index + 21];
                left[index + 21] = unchecked((uint)borrow);
                borrow >>= _BIT_COUNT_PER_UINT32;
                Assert(borrow is 0 or (-1));
                borrow += (long)left[index + 22] - right[index + 22];
                left[index + 22] = unchecked((uint)borrow);
                borrow >>= _BIT_COUNT_PER_UINT32;
                Assert(borrow is 0 or (-1));
                borrow += (long)left[index + 23] - right[index + 23];
                left[index + 23] = unchecked((uint)borrow);
                borrow >>= _BIT_COUNT_PER_UINT32;
                Assert(borrow is 0 or (-1));
                borrow += (long)left[index + 24] - right[index + 24];
                left[index + 24] = unchecked((uint)borrow);
                borrow >>= _BIT_COUNT_PER_UINT32;
                Assert(borrow is 0 or (-1));
                borrow += (long)left[index + 25] - right[index + 25];
                left[index + 25] = unchecked((uint)borrow);
                borrow >>= _BIT_COUNT_PER_UINT32;
                Assert(borrow is 0 or (-1));
                borrow += (long)left[index + 26] - right[index + 26];
                left[index + 26] = unchecked((uint)borrow);
                borrow >>= _BIT_COUNT_PER_UINT32;
                Assert(borrow is 0 or (-1));
                borrow += (long)left[index + 27] - right[index + 27];
                left[index + 27] = unchecked((uint)borrow);
                borrow >>= _BIT_COUNT_PER_UINT32;
                Assert(borrow is 0 or (-1));
                borrow += (long)left[index + 28] - right[index + 28];
                left[index + 28] = unchecked((uint)borrow);
                borrow >>= _BIT_COUNT_PER_UINT32;
                Assert(borrow is 0 or (-1));
                borrow += (long)left[index + 29] - right[index + 29];
                left[index + 29] = unchecked((uint)borrow);
                borrow >>= _BIT_COUNT_PER_UINT32;
                Assert(borrow is 0 or (-1));
                borrow += (long)left[index + 30] - right[index + 30];
                left[index + 30] = unchecked((uint)borrow);
                borrow >>= _BIT_COUNT_PER_UINT32;
                Assert(borrow is 0 or (-1));
                borrow += (long)left[index + 31] - right[index + 31];
                left[index + 31] = unchecked((uint)borrow);
                borrow >>= _BIT_COUNT_PER_UINT32;
                Assert(borrow is 0 or (-1));
                index += 32;
                count -= 32;
            }

            if (count >= 16)
            {
                borrow += (long)left[index + 0] - right[index + 0];
                left[index + 0] = unchecked((uint)borrow);
                borrow >>= _BIT_COUNT_PER_UINT32;
                Assert(borrow is 0 or (-1));
                borrow += (long)left[index + 1] - right[index + 1];
                left[index + 1] = unchecked((uint)borrow);
                borrow >>= _BIT_COUNT_PER_UINT32;
                Assert(borrow is 0 or (-1));
                borrow += (long)left[index + 2] - right[index + 2];
                left[index + 2] = unchecked((uint)borrow);
                borrow >>= _BIT_COUNT_PER_UINT32;
                Assert(borrow is 0 or (-1));
                borrow += (long)left[index + 3] - right[index + 3];
                left[index + 3] = unchecked((uint)borrow);
                borrow >>= _BIT_COUNT_PER_UINT32;
                Assert(borrow is 0 or (-1));
                borrow += (long)left[index + 4] - right[index + 4];
                left[index + 4] = unchecked((uint)borrow);
                borrow >>= _BIT_COUNT_PER_UINT32;
                Assert(borrow is 0 or (-1));
                borrow += (long)left[index + 5] - right[index + 5];
                left[index + 5] = unchecked((uint)borrow);
                borrow >>= _BIT_COUNT_PER_UINT32;
                Assert(borrow is 0 or (-1));
                borrow += (long)left[index + 6] - right[index + 6];
                left[index + 6] = unchecked((uint)borrow);
                borrow >>= _BIT_COUNT_PER_UINT32;
                Assert(borrow is 0 or (-1));
                borrow += (long)left[index + 7] - right[index + 7];
                left[index + 7] = unchecked((uint)borrow);
                borrow >>= _BIT_COUNT_PER_UINT32;
                Assert(borrow is 0 or (-1));
                borrow += (long)left[index + 8] - right[index + 8];
                left[index + 8] = unchecked((uint)borrow);
                borrow >>= _BIT_COUNT_PER_UINT32;
                Assert(borrow is 0 or (-1));
                borrow += (long)left[index + 9] - right[index + 9];
                left[index + 9] = unchecked((uint)borrow);
                borrow >>= _BIT_COUNT_PER_UINT32;
                Assert(borrow is 0 or (-1));
                borrow += (long)left[index + 10] - right[index + 10];
                left[index + 10] = unchecked((uint)borrow);
                borrow >>= _BIT_COUNT_PER_UINT32;
                Assert(borrow is 0 or (-1));
                borrow += (long)left[index + 11] - right[index + 11];
                left[index + 11] = unchecked((uint)borrow);
                borrow >>= _BIT_COUNT_PER_UINT32;
                Assert(borrow is 0 or (-1));
                borrow += (long)left[index + 12] - right[index + 12];
                left[index + 12] = unchecked((uint)borrow);
                borrow >>= _BIT_COUNT_PER_UINT32;
                Assert(borrow is 0 or (-1));
                borrow += (long)left[index + 13] - right[index + 13];
                left[index + 13] = unchecked((uint)borrow);
                borrow >>= _BIT_COUNT_PER_UINT32;
                Assert(borrow is 0 or (-1));
                borrow += (long)left[index + 14] - right[index + 14];
                left[index + 14] = unchecked((uint)borrow);
                borrow >>= _BIT_COUNT_PER_UINT32;
                Assert(borrow is 0 or (-1));
                borrow += (long)left[index + 15] - right[index + 15];
                left[index + 15] = unchecked((uint)borrow);
                borrow >>= _BIT_COUNT_PER_UINT32;
                Assert(borrow is 0 or (-1));
                index += 16;
                count -= 16;
            }

            if (count >= 8)
            {
                borrow += (long)left[index + 0] - right[index + 0];
                left[index + 0] = unchecked((uint)borrow);
                borrow >>= _BIT_COUNT_PER_UINT32;
                Assert(borrow is 0 or (-1));
                borrow += (long)left[index + 1] - right[index + 1];
                left[index + 1] = unchecked((uint)borrow);
                borrow >>= _BIT_COUNT_PER_UINT32;
                Assert(borrow is 0 or (-1));
                borrow += (long)left[index + 2] - right[index + 2];
                left[index + 2] = unchecked((uint)borrow);
                borrow >>= _BIT_COUNT_PER_UINT32;
                Assert(borrow is 0 or (-1));
                borrow += (long)left[index + 3] - right[index + 3];
                left[index + 3] = unchecked((uint)borrow);
                borrow >>= _BIT_COUNT_PER_UINT32;
                Assert(borrow is 0 or (-1));
                borrow += (long)left[index + 4] - right[index + 4];
                left[index + 4] = unchecked((uint)borrow);
                borrow >>= _BIT_COUNT_PER_UINT32;
                Assert(borrow is 0 or (-1));
                borrow += (long)left[index + 5] - right[index + 5];
                left[index + 5] = unchecked((uint)borrow);
                borrow >>= _BIT_COUNT_PER_UINT32;
                Assert(borrow is 0 or (-1));
                borrow += (long)left[index + 6] - right[index + 6];
                left[index + 6] = unchecked((uint)borrow);
                borrow >>= _BIT_COUNT_PER_UINT32;
                Assert(borrow is 0 or (-1));
                borrow += (long)left[index + 7] - right[index + 7];
                left[index + 7] = unchecked((uint)borrow);
                borrow >>= _BIT_COUNT_PER_UINT32;
                Assert(borrow is 0 or (-1));
                index += 8;
                count -= 8;
            }

            if (count >= 4)
            {
                borrow += (long)left[index + 0] - right[index + 0];
                left[index + 0] = unchecked((uint)borrow);
                borrow >>= _BIT_COUNT_PER_UINT32;
                Assert(borrow is 0 or (-1));
                borrow += (long)left[index + 1] - right[index + 1];
                left[index + 1] = unchecked((uint)borrow);
                borrow >>= _BIT_COUNT_PER_UINT32;
                Assert(borrow is 0 or (-1));
                borrow += (long)left[index + 2] - right[index + 2];
                left[index + 2] = unchecked((uint)borrow);
                borrow >>= _BIT_COUNT_PER_UINT32;
                Assert(borrow is 0 or (-1));
                borrow += (long)left[index + 3] - right[index + 3];
                left[index + 3] = unchecked((uint)borrow);
                borrow >>= _BIT_COUNT_PER_UINT32;
                Assert(borrow is 0 or (-1));
                index += 4;
                count -= 4;
            }

            if (count >= 2)
            {
                borrow += (long)left[index + 0] - right[index + 0];
                left[index + 0] = unchecked((uint)borrow);
                borrow >>= _BIT_COUNT_PER_UINT32;
                Assert(borrow is 0 or (-1));
                borrow += (long)left[index + 1] - right[index + 1];
                left[index + 1] = unchecked((uint)borrow);
                borrow >>= _BIT_COUNT_PER_UINT32;
                Assert(borrow is 0 or (-1));
                index += 2;
                count -= 2;
            }

            if (count > 0)
            {
                borrow += (long)left[index + 0] - right[index + 0];
                left[index + 0] = unchecked((uint)borrow);
                borrow >>= _BIT_COUNT_PER_UINT32;
                Assert(borrow is 0 or (-1));
                index += 1;
                count -= 1;
            }

            Assert(count == 0);
            Assert(index == right.Length);
            Assert(borrow is 0 or (-1));

            if (borrow == 0)
                return;

            Assert(left.Length > right.Length);

            count = left.Length - right.Length;
            while (count >= 32)
            {
                borrow += left[index + 0];
                left[index + 0] = unchecked((uint)borrow);
                borrow >>= _BIT_COUNT_PER_UINT32;
                Assert(borrow is 0 or (-1));
                if (borrow == 0)
                    return;
                borrow += left[index + 1];
                left[index + 1] = unchecked((uint)borrow);
                borrow >>= _BIT_COUNT_PER_UINT32;
                Assert(borrow is 0 or (-1));
                if (borrow == 0)
                    return;
                borrow += left[index + 2];
                left[index + 2] = unchecked((uint)borrow);
                borrow >>= _BIT_COUNT_PER_UINT32;
                Assert(borrow is 0 or (-1));
                if (borrow == 0)
                    return;
                borrow += left[index + 3];
                left[index + 3] = unchecked((uint)borrow);
                borrow >>= _BIT_COUNT_PER_UINT32;
                Assert(borrow is 0 or (-1));
                if (borrow == 0)
                    return;
                borrow += left[index + 4];
                left[index + 4] = unchecked((uint)borrow);
                borrow >>= _BIT_COUNT_PER_UINT32;
                Assert(borrow is 0 or (-1));
                if (borrow == 0)
                    return;
                borrow += left[index + 5];
                left[index + 5] = unchecked((uint)borrow);
                borrow >>= _BIT_COUNT_PER_UINT32;
                Assert(borrow is 0 or (-1));
                if (borrow == 0)
                    return;
                borrow += left[index + 6];
                left[index + 6] = unchecked((uint)borrow);
                borrow >>= _BIT_COUNT_PER_UINT32;
                Assert(borrow is 0 or (-1));
                if (borrow == 0)
                    return;
                borrow += left[index + 7];
                left[index + 7] = unchecked((uint)borrow);
                borrow >>= _BIT_COUNT_PER_UINT32;
                Assert(borrow is 0 or (-1));
                if (borrow == 0)
                    return;
                borrow += left[index + 8];
                left[index + 8] = unchecked((uint)borrow);
                borrow >>= _BIT_COUNT_PER_UINT32;
                Assert(borrow is 0 or (-1));
                if (borrow == 0)
                    return;
                borrow += left[index + 9];
                left[index + 9] = unchecked((uint)borrow);
                borrow >>= _BIT_COUNT_PER_UINT32;
                Assert(borrow is 0 or (-1));
                if (borrow == 0)
                    return;
                borrow += left[index + 10];
                left[index + 10] = unchecked((uint)borrow);
                borrow >>= _BIT_COUNT_PER_UINT32;
                Assert(borrow is 0 or (-1));
                if (borrow == 0)
                    return;
                borrow += left[index + 11];
                left[index + 11] = unchecked((uint)borrow);
                borrow >>= _BIT_COUNT_PER_UINT32;
                Assert(borrow is 0 or (-1));
                if (borrow == 0)
                    return;
                borrow += left[index + 12];
                left[index + 12] = unchecked((uint)borrow);
                borrow >>= _BIT_COUNT_PER_UINT32;
                Assert(borrow is 0 or (-1));
                if (borrow == 0)
                    return;
                borrow += left[index + 13];
                left[index + 13] = unchecked((uint)borrow);
                borrow >>= _BIT_COUNT_PER_UINT32;
                Assert(borrow is 0 or (-1));
                if (borrow == 0)
                    return;
                borrow += left[index + 14];
                left[index + 14] = unchecked((uint)borrow);
                borrow >>= _BIT_COUNT_PER_UINT32;
                Assert(borrow is 0 or (-1));
                if (borrow == 0)
                    return;
                borrow += left[index + 15];
                left[index + 15] = unchecked((uint)borrow);
                borrow >>= _BIT_COUNT_PER_UINT32;
                Assert(borrow is 0 or (-1));
                if (borrow == 0)
                    return;
                borrow += left[index + 16];
                left[index + 16] = unchecked((uint)borrow);
                borrow >>= _BIT_COUNT_PER_UINT32;
                Assert(borrow is 0 or (-1));
                if (borrow == 0)
                    return;
                borrow += left[index + 17];
                left[index + 17] = unchecked((uint)borrow);
                borrow >>= _BIT_COUNT_PER_UINT32;
                Assert(borrow is 0 or (-1));
                if (borrow == 0)
                    return;
                borrow += left[index + 18];
                left[index + 18] = unchecked((uint)borrow);
                borrow >>= _BIT_COUNT_PER_UINT32;
                Assert(borrow is 0 or (-1));
                if (borrow == 0)
                    return;
                borrow += left[index + 19];
                left[index + 19] = unchecked((uint)borrow);
                borrow >>= _BIT_COUNT_PER_UINT32;
                Assert(borrow is 0 or (-1));
                if (borrow == 0)
                    return;
                borrow += left[index + 20];
                left[index + 20] = unchecked((uint)borrow);
                borrow >>= _BIT_COUNT_PER_UINT32;
                Assert(borrow is 0 or (-1));
                if (borrow == 0)
                    return;
                borrow += left[index + 21];
                left[index + 21] = unchecked((uint)borrow);
                borrow >>= _BIT_COUNT_PER_UINT32;
                Assert(borrow is 0 or (-1));
                if (borrow == 0)
                    return;
                borrow += left[index + 22];
                left[index + 22] = unchecked((uint)borrow);
                borrow >>= _BIT_COUNT_PER_UINT32;
                Assert(borrow is 0 or (-1));
                if (borrow == 0)
                    return;
                borrow += left[index + 23];
                left[index + 23] = unchecked((uint)borrow);
                borrow >>= _BIT_COUNT_PER_UINT32;
                Assert(borrow is 0 or (-1));
                if (borrow == 0)
                    return;
                borrow += left[index + 24];
                left[index + 24] = unchecked((uint)borrow);
                borrow >>= _BIT_COUNT_PER_UINT32;
                Assert(borrow is 0 or (-1));
                if (borrow == 0)
                    return;
                borrow += left[index + 25];
                left[index + 25] = unchecked((uint)borrow);
                borrow >>= _BIT_COUNT_PER_UINT32;
                Assert(borrow is 0 or (-1));
                if (borrow == 0)
                    return;
                borrow += left[index + 26];
                left[index + 26] = unchecked((uint)borrow);
                borrow >>= _BIT_COUNT_PER_UINT32;
                Assert(borrow is 0 or (-1));
                if (borrow == 0)
                    return;
                borrow += left[index + 27];
                left[index + 27] = unchecked((uint)borrow);
                borrow >>= _BIT_COUNT_PER_UINT32;
                Assert(borrow is 0 or (-1));
                if (borrow == 0)
                    return;
                borrow += left[index + 28];
                left[index + 28] = unchecked((uint)borrow);
                borrow >>= _BIT_COUNT_PER_UINT32;
                Assert(borrow is 0 or (-1));
                if (borrow == 0)
                    return;
                borrow += left[index + 29];
                left[index + 29] = unchecked((uint)borrow);
                borrow >>= _BIT_COUNT_PER_UINT32;
                Assert(borrow is 0 or (-1));
                if (borrow == 0)
                    return;
                borrow += left[index + 30];
                left[index + 30] = unchecked((uint)borrow);
                borrow >>= _BIT_COUNT_PER_UINT32;
                Assert(borrow is 0 or (-1));
                if (borrow == 0)
                    return;
                borrow += left[index + 31];
                left[index + 31] = unchecked((uint)borrow);
                borrow >>= _BIT_COUNT_PER_UINT32;
                Assert(borrow is 0 or (-1));
                if (borrow == 0)
                    return;
                index += 32;
                count -= 32;
            }

            if (count >= 16)
            {
                borrow += left[index + 0];
                left[index + 0] = unchecked((uint)borrow);
                borrow >>= _BIT_COUNT_PER_UINT32;
                Assert(borrow is 0 or (-1));
                if (borrow == 0)
                    return;
                borrow += left[index + 1];
                left[index + 1] = unchecked((uint)borrow);
                borrow >>= _BIT_COUNT_PER_UINT32;
                Assert(borrow is 0 or (-1));
                if (borrow == 0)
                    return;
                borrow += left[index + 2];
                left[index + 2] = unchecked((uint)borrow);
                borrow >>= _BIT_COUNT_PER_UINT32;
                Assert(borrow is 0 or (-1));
                if (borrow == 0)
                    return;
                borrow += left[index + 3];
                left[index + 3] = unchecked((uint)borrow);
                borrow >>= _BIT_COUNT_PER_UINT32;
                Assert(borrow is 0 or (-1));
                if (borrow == 0)
                    return;
                borrow += left[index + 4];
                left[index + 4] = unchecked((uint)borrow);
                borrow >>= _BIT_COUNT_PER_UINT32;
                Assert(borrow is 0 or (-1));
                if (borrow == 0)
                    return;
                borrow += left[index + 5];
                left[index + 5] = unchecked((uint)borrow);
                borrow >>= _BIT_COUNT_PER_UINT32;
                Assert(borrow is 0 or (-1));
                if (borrow == 0)
                    return;
                borrow += left[index + 6];
                left[index + 6] = unchecked((uint)borrow);
                borrow >>= _BIT_COUNT_PER_UINT32;
                Assert(borrow is 0 or (-1));
                if (borrow == 0)
                    return;
                borrow += left[index + 7];
                left[index + 7] = unchecked((uint)borrow);
                borrow >>= _BIT_COUNT_PER_UINT32;
                Assert(borrow is 0 or (-1));
                if (borrow == 0)
                    return;
                borrow += left[index + 8];
                left[index + 8] = unchecked((uint)borrow);
                borrow >>= _BIT_COUNT_PER_UINT32;
                Assert(borrow is 0 or (-1));
                if (borrow == 0)
                    return;
                borrow += left[index + 9];
                left[index + 9] = unchecked((uint)borrow);
                borrow >>= _BIT_COUNT_PER_UINT32;
                Assert(borrow is 0 or (-1));
                if (borrow == 0)
                    return;
                borrow += left[index + 10];
                left[index + 10] = unchecked((uint)borrow);
                borrow >>= _BIT_COUNT_PER_UINT32;
                Assert(borrow is 0 or (-1));
                if (borrow == 0)
                    return;
                borrow += left[index + 11];
                left[index + 11] = unchecked((uint)borrow);
                borrow >>= _BIT_COUNT_PER_UINT32;
                Assert(borrow is 0 or (-1));
                if (borrow == 0)
                    return;
                borrow += left[index + 12];
                left[index + 12] = unchecked((uint)borrow);
                borrow >>= _BIT_COUNT_PER_UINT32;
                Assert(borrow is 0 or (-1));
                if (borrow == 0)
                    return;
                borrow += left[index + 13];
                left[index + 13] = unchecked((uint)borrow);
                borrow >>= _BIT_COUNT_PER_UINT32;
                Assert(borrow is 0 or (-1));
                if (borrow == 0)
                    return;
                borrow += left[index + 14];
                left[index + 14] = unchecked((uint)borrow);
                borrow >>= _BIT_COUNT_PER_UINT32;
                Assert(borrow is 0 or (-1));
                if (borrow == 0)
                    return;
                borrow += left[index + 15];
                left[index + 15] = unchecked((uint)borrow);
                borrow >>= _BIT_COUNT_PER_UINT32;
                Assert(borrow is 0 or (-1));
                if (borrow == 0)
                    return;
                index += 16;
                count -= 16;
            }

            if (count >= 8)
            {
                borrow += left[index + 0];
                left[index + 0] = unchecked((uint)borrow);
                borrow >>= _BIT_COUNT_PER_UINT32;
                Assert(borrow is 0 or (-1));
                if (borrow == 0)
                    return;
                borrow += left[index + 1];
                left[index + 1] = unchecked((uint)borrow);
                borrow >>= _BIT_COUNT_PER_UINT32;
                Assert(borrow is 0 or (-1));
                if (borrow == 0)
                    return;
                borrow += left[index + 2];
                left[index + 2] = unchecked((uint)borrow);
                borrow >>= _BIT_COUNT_PER_UINT32;
                Assert(borrow is 0 or (-1));
                if (borrow == 0)
                    return;
                borrow += left[index + 3];
                left[index + 3] = unchecked((uint)borrow);
                borrow >>= _BIT_COUNT_PER_UINT32;
                Assert(borrow is 0 or (-1));
                if (borrow == 0)
                    return;
                borrow += left[index + 4];
                left[index + 4] = unchecked((uint)borrow);
                borrow >>= _BIT_COUNT_PER_UINT32;
                Assert(borrow is 0 or (-1));
                if (borrow == 0)
                    return;
                borrow += left[index + 5];
                left[index + 5] = unchecked((uint)borrow);
                borrow >>= _BIT_COUNT_PER_UINT32;
                Assert(borrow is 0 or (-1));
                if (borrow == 0)
                    return;
                borrow += left[index + 6];
                left[index + 6] = unchecked((uint)borrow);
                borrow >>= _BIT_COUNT_PER_UINT32;
                Assert(borrow is 0 or (-1));
                if (borrow == 0)
                    return;
                borrow += left[index + 7];
                left[index + 7] = unchecked((uint)borrow);
                borrow >>= _BIT_COUNT_PER_UINT32;
                Assert(borrow is 0 or (-1));
                if (borrow == 0)
                    return;
                index += 8;
                count -= 8;
            }

            if (count >= 4)
            {
                borrow += left[index + 0];
                left[index + 0] = unchecked((uint)borrow);
                borrow >>= _BIT_COUNT_PER_UINT32;
                Assert(borrow is 0 or (-1));
                if (borrow == 0)
                    return;
                borrow += left[index + 1];
                left[index + 1] = unchecked((uint)borrow);
                borrow >>= _BIT_COUNT_PER_UINT32;
                Assert(borrow is 0 or (-1));
                if (borrow == 0)
                    return;
                borrow += left[index + 2];
                left[index + 2] = unchecked((uint)borrow);
                borrow >>= _BIT_COUNT_PER_UINT32;
                Assert(borrow is 0 or (-1));
                if (borrow == 0)
                    return;
                borrow += left[index + 3];
                left[index + 3] = unchecked((uint)borrow);
                borrow >>= _BIT_COUNT_PER_UINT32;
                Assert(borrow is 0 or (-1));
                if (borrow == 0)
                    return;
                index += 4;
                count -= 4;
            }

            if (count >= 2)
            {
                borrow += left[index + 0];
                left[index + 0] = unchecked((uint)borrow);
                borrow >>= _BIT_COUNT_PER_UINT32;
                Assert(borrow is 0 or (-1));
                if (borrow == 0)
                    return;
                borrow += left[index + 1];
                left[index + 1] = unchecked((uint)borrow);
                borrow >>= _BIT_COUNT_PER_UINT32;
                Assert(borrow is 0 or (-1));
                if (borrow == 0)
                    return;
                index += 2;
                count -= 2;
            }

            if (count > 0)
            {
                borrow += left[index + 0];
                left[index + 0] = unchecked((uint)borrow);
                borrow >>= _BIT_COUNT_PER_UINT32;
                Assert(borrow is 0 or (-1));
                if (borrow == 0)
                    return;
                index += 1;
                count -= 1;
            }

            Assert(count == 0);
            Assert(index == left.Length);
            Assert(borrow == 0);
        }

        #endregion

        #region SubtractSelfTry2

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        private static void SubtractSelfTry2(Span<uint> left, ReadOnlySpan<uint> right)
        {
            Assert(left.Length > 0);
            Assert(right.Length > 0);
            Assert(left.Length >= right.Length);

            unsafe
            {
                fixed (uint* leftBuffer = left)
                fixed (uint* rightBuffer = right)
                {
                    var lp = leftBuffer;
                    var rp = rightBuffer;
                    var count = right.Length;
                    var borrow = 0L;
                    while (count >= 32)
                    {
                        borrow += (long)lp[0] - rp[0];
                        lp[0] = unchecked((uint)borrow);
                        borrow >>= _BIT_COUNT_PER_UINT32;
                        Assert(borrow is 0 or (-1));
                        borrow += (long)lp[1] - rp[1];
                        lp[1] = unchecked((uint)borrow);
                        borrow >>= _BIT_COUNT_PER_UINT32;
                        Assert(borrow is 0 or (-1));
                        borrow += (long)lp[2] - rp[2];
                        lp[2] = unchecked((uint)borrow);
                        borrow >>= _BIT_COUNT_PER_UINT32;
                        Assert(borrow is 0 or (-1));
                        borrow += (long)lp[3] - rp[3];
                        lp[3] = unchecked((uint)borrow);
                        borrow >>= _BIT_COUNT_PER_UINT32;
                        Assert(borrow is 0 or (-1));
                        borrow += (long)lp[4] - rp[4];
                        lp[4] = unchecked((uint)borrow);
                        borrow >>= _BIT_COUNT_PER_UINT32;
                        Assert(borrow is 0 or (-1));
                        borrow += (long)lp[5] - rp[5];
                        lp[5] = unchecked((uint)borrow);
                        borrow >>= _BIT_COUNT_PER_UINT32;
                        Assert(borrow is 0 or (-1));
                        borrow += (long)lp[6] - rp[6];
                        lp[6] = unchecked((uint)borrow);
                        borrow >>= _BIT_COUNT_PER_UINT32;
                        Assert(borrow is 0 or (-1));
                        borrow += (long)lp[7] - rp[7];
                        lp[7] = unchecked((uint)borrow);
                        borrow >>= _BIT_COUNT_PER_UINT32;
                        Assert(borrow is 0 or (-1));
                        borrow += (long)lp[8] - rp[8];
                        lp[8] = unchecked((uint)borrow);
                        borrow >>= _BIT_COUNT_PER_UINT32;
                        Assert(borrow is 0 or (-1));
                        borrow += (long)lp[9] - rp[9];
                        lp[9] = unchecked((uint)borrow);
                        borrow >>= _BIT_COUNT_PER_UINT32;
                        Assert(borrow is 0 or (-1));
                        borrow += (long)lp[10] - rp[10];
                        lp[10] = unchecked((uint)borrow);
                        borrow >>= _BIT_COUNT_PER_UINT32;
                        Assert(borrow is 0 or (-1));
                        borrow += (long)lp[11] - rp[11];
                        lp[11] = unchecked((uint)borrow);
                        borrow >>= _BIT_COUNT_PER_UINT32;
                        Assert(borrow is 0 or (-1));
                        borrow += (long)lp[12] - rp[12];
                        lp[12] = unchecked((uint)borrow);
                        borrow >>= _BIT_COUNT_PER_UINT32;
                        Assert(borrow is 0 or (-1));
                        borrow += (long)lp[13] - rp[13];
                        lp[13] = unchecked((uint)borrow);
                        borrow >>= _BIT_COUNT_PER_UINT32;
                        Assert(borrow is 0 or (-1));
                        borrow += (long)lp[14] - rp[14];
                        lp[14] = unchecked((uint)borrow);
                        borrow >>= _BIT_COUNT_PER_UINT32;
                        Assert(borrow is 0 or (-1));
                        borrow += (long)lp[15] - rp[15];
                        lp[15] = unchecked((uint)borrow);
                        borrow >>= _BIT_COUNT_PER_UINT32;
                        Assert(borrow is 0 or (-1));
                        borrow += (long)lp[16] - rp[16];
                        lp[16] = unchecked((uint)borrow);
                        borrow >>= _BIT_COUNT_PER_UINT32;
                        Assert(borrow is 0 or (-1));
                        borrow += (long)lp[17] - rp[17];
                        lp[17] = unchecked((uint)borrow);
                        borrow >>= _BIT_COUNT_PER_UINT32;
                        Assert(borrow is 0 or (-1));
                        borrow += (long)lp[18] - rp[18];
                        lp[18] = unchecked((uint)borrow);
                        borrow >>= _BIT_COUNT_PER_UINT32;
                        Assert(borrow is 0 or (-1));
                        borrow += (long)lp[19] - rp[19];
                        lp[19] = unchecked((uint)borrow);
                        borrow >>= _BIT_COUNT_PER_UINT32;
                        Assert(borrow is 0 or (-1));
                        borrow += (long)lp[20] - rp[20];
                        lp[20] = unchecked((uint)borrow);
                        borrow >>= _BIT_COUNT_PER_UINT32;
                        Assert(borrow is 0 or (-1));
                        borrow += (long)lp[21] - rp[21];
                        lp[21] = unchecked((uint)borrow);
                        borrow >>= _BIT_COUNT_PER_UINT32;
                        Assert(borrow is 0 or (-1));
                        borrow += (long)lp[22] - rp[22];
                        lp[22] = unchecked((uint)borrow);
                        borrow >>= _BIT_COUNT_PER_UINT32;
                        Assert(borrow is 0 or (-1));
                        borrow += (long)lp[23] - rp[23];
                        lp[23] = unchecked((uint)borrow);
                        borrow >>= _BIT_COUNT_PER_UINT32;
                        Assert(borrow is 0 or (-1));
                        borrow += (long)lp[24] - rp[24];
                        lp[24] = unchecked((uint)borrow);
                        borrow >>= _BIT_COUNT_PER_UINT32;
                        Assert(borrow is 0 or (-1));
                        borrow += (long)lp[25] - rp[25];
                        lp[25] = unchecked((uint)borrow);
                        borrow >>= _BIT_COUNT_PER_UINT32;
                        Assert(borrow is 0 or (-1));
                        borrow += (long)lp[26] - rp[26];
                        lp[26] = unchecked((uint)borrow);
                        borrow >>= _BIT_COUNT_PER_UINT32;
                        Assert(borrow is 0 or (-1));
                        borrow += (long)lp[27] - rp[27];
                        lp[27] = unchecked((uint)borrow);
                        borrow >>= _BIT_COUNT_PER_UINT32;
                        Assert(borrow is 0 or (-1));
                        borrow += (long)lp[28] - rp[28];
                        lp[28] = unchecked((uint)borrow);
                        borrow >>= _BIT_COUNT_PER_UINT32;
                        Assert(borrow is 0 or (-1));
                        borrow += (long)lp[29] - rp[29];
                        lp[29] = unchecked((uint)borrow);
                        borrow >>= _BIT_COUNT_PER_UINT32;
                        Assert(borrow is 0 or (-1));
                        borrow += (long)lp[30] - rp[30];
                        lp[30] = unchecked((uint)borrow);
                        borrow >>= _BIT_COUNT_PER_UINT32;
                        Assert(borrow is 0 or (-1));
                        borrow += (long)lp[31] - rp[31];
                        lp[31] = unchecked((uint)borrow);
                        borrow >>= _BIT_COUNT_PER_UINT32;
                        Assert(borrow is 0 or (-1));
                        lp += 32;
                        rp += 32;
                        count -= 32;
                    }

                    if (count >= 16)
                    {
                        borrow += (long)lp[0] - rp[0];
                        lp[0] = unchecked((uint)borrow);
                        borrow >>= _BIT_COUNT_PER_UINT32;
                        Assert(borrow is 0 or (-1));
                        borrow += (long)lp[1] - rp[1];
                        lp[1] = unchecked((uint)borrow);
                        borrow >>= _BIT_COUNT_PER_UINT32;
                        Assert(borrow is 0 or (-1));
                        borrow += (long)lp[2] - rp[2];
                        lp[2] = unchecked((uint)borrow);
                        borrow >>= _BIT_COUNT_PER_UINT32;
                        Assert(borrow is 0 or (-1));
                        borrow += (long)lp[3] - rp[3];
                        lp[3] = unchecked((uint)borrow);
                        borrow >>= _BIT_COUNT_PER_UINT32;
                        Assert(borrow is 0 or (-1));
                        borrow += (long)lp[4] - rp[4];
                        lp[4] = unchecked((uint)borrow);
                        borrow >>= _BIT_COUNT_PER_UINT32;
                        Assert(borrow is 0 or (-1));
                        borrow += (long)lp[5] - rp[5];
                        lp[5] = unchecked((uint)borrow);
                        borrow >>= _BIT_COUNT_PER_UINT32;
                        Assert(borrow is 0 or (-1));
                        borrow += (long)lp[6] - rp[6];
                        lp[6] = unchecked((uint)borrow);
                        borrow >>= _BIT_COUNT_PER_UINT32;
                        Assert(borrow is 0 or (-1));
                        borrow += (long)lp[7] - rp[7];
                        lp[7] = unchecked((uint)borrow);
                        borrow >>= _BIT_COUNT_PER_UINT32;
                        Assert(borrow is 0 or (-1));
                        borrow += (long)lp[8] - rp[8];
                        lp[8] = unchecked((uint)borrow);
                        borrow >>= _BIT_COUNT_PER_UINT32;
                        Assert(borrow is 0 or (-1));
                        borrow += (long)lp[9] - rp[9];
                        lp[9] = unchecked((uint)borrow);
                        borrow >>= _BIT_COUNT_PER_UINT32;
                        Assert(borrow is 0 or (-1));
                        borrow += (long)lp[10] - rp[10];
                        lp[10] = unchecked((uint)borrow);
                        borrow >>= _BIT_COUNT_PER_UINT32;
                        Assert(borrow is 0 or (-1));
                        borrow += (long)lp[11] - rp[11];
                        lp[11] = unchecked((uint)borrow);
                        borrow >>= _BIT_COUNT_PER_UINT32;
                        Assert(borrow is 0 or (-1));
                        borrow += (long)lp[12] - rp[12];
                        lp[12] = unchecked((uint)borrow);
                        borrow >>= _BIT_COUNT_PER_UINT32;
                        Assert(borrow is 0 or (-1));
                        borrow += (long)lp[13] - rp[13];
                        lp[13] = unchecked((uint)borrow);
                        borrow >>= _BIT_COUNT_PER_UINT32;
                        Assert(borrow is 0 or (-1));
                        borrow += (long)lp[14] - rp[14];
                        lp[14] = unchecked((uint)borrow);
                        borrow >>= _BIT_COUNT_PER_UINT32;
                        Assert(borrow is 0 or (-1));
                        borrow += (long)lp[15] - rp[15];
                        lp[15] = unchecked((uint)borrow);
                        borrow >>= _BIT_COUNT_PER_UINT32;
                        Assert(borrow is 0 or (-1));
                        lp += 16;
                        rp += 16;
                        count -= 16;
                    }

                    if (count >= 8)
                    {
                        borrow += (long)lp[0] - rp[0];
                        lp[0] = unchecked((uint)borrow);
                        borrow >>= _BIT_COUNT_PER_UINT32;
                        Assert(borrow is 0 or (-1));
                        borrow += (long)lp[1] - rp[1];
                        lp[1] = unchecked((uint)borrow);
                        borrow >>= _BIT_COUNT_PER_UINT32;
                        Assert(borrow is 0 or (-1));
                        borrow += (long)lp[2] - rp[2];
                        lp[2] = unchecked((uint)borrow);
                        borrow >>= _BIT_COUNT_PER_UINT32;
                        Assert(borrow is 0 or (-1));
                        borrow += (long)lp[3] - rp[3];
                        lp[3] = unchecked((uint)borrow);
                        borrow >>= _BIT_COUNT_PER_UINT32;
                        Assert(borrow is 0 or (-1));
                        borrow += (long)lp[4] - rp[4];
                        lp[4] = unchecked((uint)borrow);
                        borrow >>= _BIT_COUNT_PER_UINT32;
                        Assert(borrow is 0 or (-1));
                        borrow += (long)lp[5] - rp[5];
                        lp[5] = unchecked((uint)borrow);
                        borrow >>= _BIT_COUNT_PER_UINT32;
                        Assert(borrow is 0 or (-1));
                        borrow += (long)lp[6] - rp[6];
                        lp[6] = unchecked((uint)borrow);
                        borrow >>= _BIT_COUNT_PER_UINT32;
                        Assert(borrow is 0 or (-1));
                        borrow += (long)lp[7] - rp[7];
                        lp[7] = unchecked((uint)borrow);
                        borrow >>= _BIT_COUNT_PER_UINT32;
                        Assert(borrow is 0 or (-1));
                        lp += 8;
                        rp += 8;
                        count -= 8;
                    }

                    if (count >= 4)
                    {
                        borrow += (long)lp[0] - rp[0];
                        lp[0] = unchecked((uint)borrow);
                        borrow >>= _BIT_COUNT_PER_UINT32;
                        Assert(borrow is 0 or (-1));
                        borrow += (long)lp[1] - rp[1];
                        lp[1] = unchecked((uint)borrow);
                        borrow >>= _BIT_COUNT_PER_UINT32;
                        Assert(borrow is 0 or (-1));
                        borrow += (long)lp[2] - rp[2];
                        lp[2] = unchecked((uint)borrow);
                        borrow >>= _BIT_COUNT_PER_UINT32;
                        Assert(borrow is 0 or (-1));
                        borrow += (long)lp[3] - rp[3];
                        lp[3] = unchecked((uint)borrow);
                        borrow >>= _BIT_COUNT_PER_UINT32;
                        Assert(borrow is 0 or (-1));
                        lp += 4;
                        rp += 4;
                        count -= 4;
                    }

                    if (count >= 2)
                    {
                        borrow += (long)lp[0] - rp[0];
                        lp[0] = unchecked((uint)borrow);
                        borrow >>= _BIT_COUNT_PER_UINT32;
                        Assert(borrow is 0 or (-1));
                        borrow += (long)lp[1] - rp[1];
                        lp[1] = unchecked((uint)borrow);
                        borrow >>= _BIT_COUNT_PER_UINT32;
                        Assert(borrow is 0 or (-1));
                        lp += 2;
                        rp += 2;
                        count -= 2;
                    }

                    if (count > 0)
                    {
                        borrow += (long)lp[0] - rp[0];
                        lp[0] = unchecked((uint)borrow);
                        borrow >>= _BIT_COUNT_PER_UINT32;
                        Assert(borrow is 0 or (-1));
                        lp += 1;
                        rp += 1;
                        count -= 1;
                    }

                    Assert(lp == &leftBuffer[right.Length]);
                    Assert(rp == &rightBuffer[right.Length]);
                    Assert(count == 0);
                    Assert(borrow is 0 or (-1));

                    if (borrow == 0)
                        return;

                    Assert(left.Length > right.Length);

                    count = left.Length - right.Length;
                    while (count >= 32)
                    {
                        borrow += lp[0];
                        lp[0] = unchecked((uint)borrow);
                        borrow >>= _BIT_COUNT_PER_UINT32;
                        Assert(borrow is 0 or (-1));
                        if (borrow == 0)
                            return;
                        borrow += lp[1];
                        lp[1] = unchecked((uint)borrow);
                        borrow >>= _BIT_COUNT_PER_UINT32;
                        Assert(borrow is 0 or (-1));
                        if (borrow == 0)
                            return;
                        borrow += lp[2];
                        lp[2] = unchecked((uint)borrow);
                        borrow >>= _BIT_COUNT_PER_UINT32;
                        Assert(borrow is 0 or (-1));
                        if (borrow == 0)
                            return;
                        borrow += lp[3];
                        lp[3] = unchecked((uint)borrow);
                        borrow >>= _BIT_COUNT_PER_UINT32;
                        Assert(borrow is 0 or (-1));
                        if (borrow == 0)
                            return;
                        borrow += lp[4];
                        lp[4] = unchecked((uint)borrow);
                        borrow >>= _BIT_COUNT_PER_UINT32;
                        Assert(borrow is 0 or (-1));
                        if (borrow == 0)
                            return;
                        borrow += lp[5];
                        lp[5] = unchecked((uint)borrow);
                        borrow >>= _BIT_COUNT_PER_UINT32;
                        Assert(borrow is 0 or (-1));
                        if (borrow == 0)
                            return;
                        borrow += lp[6];
                        lp[6] = unchecked((uint)borrow);
                        borrow >>= _BIT_COUNT_PER_UINT32;
                        Assert(borrow is 0 or (-1));
                        if (borrow == 0)
                            return;
                        borrow += lp[7];
                        lp[7] = unchecked((uint)borrow);
                        borrow >>= _BIT_COUNT_PER_UINT32;
                        Assert(borrow is 0 or (-1));
                        if (borrow == 0)
                            return;
                        borrow += lp[8];
                        lp[8] = unchecked((uint)borrow);
                        borrow >>= _BIT_COUNT_PER_UINT32;
                        Assert(borrow is 0 or (-1));
                        if (borrow == 0)
                            return;
                        borrow += lp[9];
                        lp[9] = unchecked((uint)borrow);
                        borrow >>= _BIT_COUNT_PER_UINT32;
                        Assert(borrow is 0 or (-1));
                        if (borrow == 0)
                            return;
                        borrow += lp[10];
                        lp[10] = unchecked((uint)borrow);
                        borrow >>= _BIT_COUNT_PER_UINT32;
                        Assert(borrow is 0 or (-1));
                        if (borrow == 0)
                            return;
                        borrow += lp[11];
                        lp[11] = unchecked((uint)borrow);
                        borrow >>= _BIT_COUNT_PER_UINT32;
                        Assert(borrow is 0 or (-1));
                        if (borrow == 0)
                            return;
                        borrow += lp[12];
                        lp[12] = unchecked((uint)borrow);
                        borrow >>= _BIT_COUNT_PER_UINT32;
                        Assert(borrow is 0 or (-1));
                        if (borrow == 0)
                            return;
                        borrow += lp[13];
                        lp[13] = unchecked((uint)borrow);
                        borrow >>= _BIT_COUNT_PER_UINT32;
                        Assert(borrow is 0 or (-1));
                        if (borrow == 0)
                            return;
                        borrow += lp[14];
                        lp[14] = unchecked((uint)borrow);
                        borrow >>= _BIT_COUNT_PER_UINT32;
                        Assert(borrow is 0 or (-1));
                        if (borrow == 0)
                            return;
                        borrow += lp[15];
                        lp[15] = unchecked((uint)borrow);
                        borrow >>= _BIT_COUNT_PER_UINT32;
                        Assert(borrow is 0 or (-1));
                        if (borrow == 0)
                            return;
                        borrow += lp[16];
                        lp[16] = unchecked((uint)borrow);
                        borrow >>= _BIT_COUNT_PER_UINT32;
                        Assert(borrow is 0 or (-1));
                        if (borrow == 0)
                            return;
                        borrow += lp[17];
                        lp[17] = unchecked((uint)borrow);
                        borrow >>= _BIT_COUNT_PER_UINT32;
                        Assert(borrow is 0 or (-1));
                        if (borrow == 0)
                            return;
                        borrow += lp[18];
                        lp[18] = unchecked((uint)borrow);
                        borrow >>= _BIT_COUNT_PER_UINT32;
                        Assert(borrow is 0 or (-1));
                        if (borrow == 0)
                            return;
                        borrow += lp[19];
                        lp[19] = unchecked((uint)borrow);
                        borrow >>= _BIT_COUNT_PER_UINT32;
                        Assert(borrow is 0 or (-1));
                        if (borrow == 0)
                            return;
                        borrow += lp[20];
                        lp[20] = unchecked((uint)borrow);
                        borrow >>= _BIT_COUNT_PER_UINT32;
                        Assert(borrow is 0 or (-1));
                        if (borrow == 0)
                            return;
                        borrow += lp[21];
                        lp[21] = unchecked((uint)borrow);
                        borrow >>= _BIT_COUNT_PER_UINT32;
                        Assert(borrow is 0 or (-1));
                        if (borrow == 0)
                            return;
                        borrow += lp[22];
                        lp[22] = unchecked((uint)borrow);
                        borrow >>= _BIT_COUNT_PER_UINT32;
                        Assert(borrow is 0 or (-1));
                        if (borrow == 0)
                            return;
                        borrow += lp[23];
                        lp[23] = unchecked((uint)borrow);
                        borrow >>= _BIT_COUNT_PER_UINT32;
                        Assert(borrow is 0 or (-1));
                        if (borrow == 0)
                            return;
                        borrow += lp[24];
                        lp[24] = unchecked((uint)borrow);
                        borrow >>= _BIT_COUNT_PER_UINT32;
                        Assert(borrow is 0 or (-1));
                        if (borrow == 0)
                            return;
                        borrow += lp[25];
                        lp[25] = unchecked((uint)borrow);
                        borrow >>= _BIT_COUNT_PER_UINT32;
                        Assert(borrow is 0 or (-1));
                        if (borrow == 0)
                            return;
                        borrow += lp[26];
                        lp[26] = unchecked((uint)borrow);
                        borrow >>= _BIT_COUNT_PER_UINT32;
                        Assert(borrow is 0 or (-1));
                        if (borrow == 0)
                            return;
                        borrow += lp[27];
                        lp[27] = unchecked((uint)borrow);
                        borrow >>= _BIT_COUNT_PER_UINT32;
                        Assert(borrow is 0 or (-1));
                        if (borrow == 0)
                            return;
                        borrow += lp[28];
                        lp[28] = unchecked((uint)borrow);
                        borrow >>= _BIT_COUNT_PER_UINT32;
                        Assert(borrow is 0 or (-1));
                        if (borrow == 0)
                            return;
                        borrow += lp[29];
                        lp[29] = unchecked((uint)borrow);
                        borrow >>= _BIT_COUNT_PER_UINT32;
                        Assert(borrow is 0 or (-1));
                        if (borrow == 0)
                            return;
                        borrow += lp[30];
                        lp[30] = unchecked((uint)borrow);
                        borrow >>= _BIT_COUNT_PER_UINT32;
                        Assert(borrow is 0 or (-1));
                        if (borrow == 0)
                            return;
                        borrow += lp[31];
                        lp[31] = unchecked((uint)borrow);
                        borrow >>= _BIT_COUNT_PER_UINT32;
                        Assert(borrow is 0 or (-1));
                        if (borrow == 0)
                            return;
                        lp += 32;
                        count -= 32;
                    }

                    if (count >= 16)
                    {
                        borrow += lp[0];
                        lp[0] = unchecked((uint)borrow);
                        borrow >>= _BIT_COUNT_PER_UINT32;
                        Assert(borrow is 0 or (-1));
                        if (borrow == 0)
                            return;
                        borrow += lp[1];
                        lp[1] = unchecked((uint)borrow);
                        borrow >>= _BIT_COUNT_PER_UINT32;
                        Assert(borrow is 0 or (-1));
                        if (borrow == 0)
                            return;
                        borrow += lp[2];
                        lp[2] = unchecked((uint)borrow);
                        borrow >>= _BIT_COUNT_PER_UINT32;
                        Assert(borrow is 0 or (-1));
                        if (borrow == 0)
                            return;
                        borrow += lp[3];
                        lp[3] = unchecked((uint)borrow);
                        borrow >>= _BIT_COUNT_PER_UINT32;
                        Assert(borrow is 0 or (-1));
                        if (borrow == 0)
                            return;
                        borrow += lp[4];
                        lp[4] = unchecked((uint)borrow);
                        borrow >>= _BIT_COUNT_PER_UINT32;
                        Assert(borrow is 0 or (-1));
                        if (borrow == 0)
                            return;
                        borrow += lp[5];
                        lp[5] = unchecked((uint)borrow);
                        borrow >>= _BIT_COUNT_PER_UINT32;
                        Assert(borrow is 0 or (-1));
                        if (borrow == 0)
                            return;
                        borrow += lp[6];
                        lp[6] = unchecked((uint)borrow);
                        borrow >>= _BIT_COUNT_PER_UINT32;
                        Assert(borrow is 0 or (-1));
                        if (borrow == 0)
                            return;
                        borrow += lp[7];
                        lp[7] = unchecked((uint)borrow);
                        borrow >>= _BIT_COUNT_PER_UINT32;
                        Assert(borrow is 0 or (-1));
                        if (borrow == 0)
                            return;
                        borrow += lp[8];
                        lp[8] = unchecked((uint)borrow);
                        borrow >>= _BIT_COUNT_PER_UINT32;
                        Assert(borrow is 0 or (-1));
                        if (borrow == 0)
                            return;
                        borrow += lp[9];
                        lp[9] = unchecked((uint)borrow);
                        borrow >>= _BIT_COUNT_PER_UINT32;
                        Assert(borrow is 0 or (-1));
                        if (borrow == 0)
                            return;
                        borrow += lp[10];
                        lp[10] = unchecked((uint)borrow);
                        borrow >>= _BIT_COUNT_PER_UINT32;
                        Assert(borrow is 0 or (-1));
                        if (borrow == 0)
                            return;
                        borrow += lp[11];
                        lp[11] = unchecked((uint)borrow);
                        borrow >>= _BIT_COUNT_PER_UINT32;
                        Assert(borrow is 0 or (-1));
                        if (borrow == 0)
                            return;
                        borrow += lp[12];
                        lp[12] = unchecked((uint)borrow);
                        borrow >>= _BIT_COUNT_PER_UINT32;
                        Assert(borrow is 0 or (-1));
                        if (borrow == 0)
                            return;
                        borrow += lp[13];
                        lp[13] = unchecked((uint)borrow);
                        borrow >>= _BIT_COUNT_PER_UINT32;
                        Assert(borrow is 0 or (-1));
                        if (borrow == 0)
                            return;
                        borrow += lp[14];
                        lp[14] = unchecked((uint)borrow);
                        borrow >>= _BIT_COUNT_PER_UINT32;
                        Assert(borrow is 0 or (-1));
                        if (borrow == 0)
                            return;
                        borrow += lp[15];
                        lp[15] = unchecked((uint)borrow);
                        borrow >>= _BIT_COUNT_PER_UINT32;
                        Assert(borrow is 0 or (-1));
                        if (borrow == 0)
                            return;
                        lp += 16;
                        count -= 16;
                    }

                    if (count >= 8)
                    {
                        borrow += lp[0];
                        lp[0] = unchecked((uint)borrow);
                        borrow >>= _BIT_COUNT_PER_UINT32;
                        Assert(borrow is 0 or (-1));
                        if (borrow == 0)
                            return;
                        borrow += lp[1];
                        lp[1] = unchecked((uint)borrow);
                        borrow >>= _BIT_COUNT_PER_UINT32;
                        Assert(borrow is 0 or (-1));
                        if (borrow == 0)
                            return;
                        borrow += lp[2];
                        lp[2] = unchecked((uint)borrow);
                        borrow >>= _BIT_COUNT_PER_UINT32;
                        Assert(borrow is 0 or (-1));
                        if (borrow == 0)
                            return;
                        borrow += lp[3];
                        lp[3] = unchecked((uint)borrow);
                        borrow >>= _BIT_COUNT_PER_UINT32;
                        Assert(borrow is 0 or (-1));
                        if (borrow == 0)
                            return;
                        borrow += lp[4];
                        lp[4] = unchecked((uint)borrow);
                        borrow >>= _BIT_COUNT_PER_UINT32;
                        Assert(borrow is 0 or (-1));
                        if (borrow == 0)
                            return;
                        borrow += lp[5];
                        lp[5] = unchecked((uint)borrow);
                        borrow >>= _BIT_COUNT_PER_UINT32;
                        Assert(borrow is 0 or (-1));
                        if (borrow == 0)
                            return;
                        borrow += lp[6];
                        lp[6] = unchecked((uint)borrow);
                        borrow >>= _BIT_COUNT_PER_UINT32;
                        Assert(borrow is 0 or (-1));
                        if (borrow == 0)
                            return;
                        borrow += lp[7];
                        lp[7] = unchecked((uint)borrow);
                        borrow >>= _BIT_COUNT_PER_UINT32;
                        Assert(borrow is 0 or (-1));
                        if (borrow == 0)
                            return;
                        lp += 8;
                        count -= 8;
                    }

                    if (count >= 4)
                    {
                        borrow += lp[0];
                        lp[0] = unchecked((uint)borrow);
                        borrow >>= _BIT_COUNT_PER_UINT32;
                        Assert(borrow is 0 or (-1));
                        if (borrow == 0)
                            return;
                        borrow += lp[1];
                        lp[1] = unchecked((uint)borrow);
                        borrow >>= _BIT_COUNT_PER_UINT32;
                        Assert(borrow is 0 or (-1));
                        if (borrow == 0)
                            return;
                        borrow += lp[2];
                        lp[2] = unchecked((uint)borrow);
                        borrow >>= _BIT_COUNT_PER_UINT32;
                        Assert(borrow is 0 or (-1));
                        if (borrow == 0)
                            return;
                        borrow += lp[3];
                        lp[3] = unchecked((uint)borrow);
                        borrow >>= _BIT_COUNT_PER_UINT32;
                        Assert(borrow is 0 or (-1));
                        if (borrow == 0)
                            return;
                        lp += 4;
                        count -= 4;
                    }

                    if (count >= 2)
                    {
                        borrow += lp[0];
                        lp[0] = unchecked((uint)borrow);
                        borrow >>= _BIT_COUNT_PER_UINT32;
                        Assert(borrow is 0 or (-1));
                        if (borrow == 0)
                            return;
                        borrow += lp[1];
                        lp[1] = unchecked((uint)borrow);
                        borrow >>= _BIT_COUNT_PER_UINT32;
                        Assert(borrow is 0 or (-1));
                        if (borrow == 0)
                            return;
                        lp += 2;
                        count -= 2;
                    }

                    if (count > 0)
                    {
                        borrow += lp[0];
                        lp[0] = unchecked((uint)borrow);
                        borrow >>= _BIT_COUNT_PER_UINT32;
                        Assert(borrow is 0 or (-1));
                        if (borrow == 0)
                            return;
                        lp += 1;
                        count -= 1;
                    }

                    Assert(lp == &leftBuffer[left.Length]);
                    Assert(count == 0);
                    Assert(borrow == 0);
                }
            }
        }

        #endregion
    }
}
