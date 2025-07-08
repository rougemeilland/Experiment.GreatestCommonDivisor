using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Experiment.Library
{
    public static class ExperimrntOfShift
    {
        [StructLayout(LayoutKind.Explicit, Size = 8)]
        private struct QuadWord
        {
            [FieldOffset(0)]
            public ulong QuadWordValue;

            [FieldOffset(0)]
            public uint LowDoubleWordValue;

            [FieldOffset(4)]
            public uint HighDoubleWordValue;
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.AggressiveOptimization)]
        public static ulong ShiftRight64(UInt128 value) => (ulong)(value >> 64);

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.AggressiveOptimization)]
        public static uint ShiftRight32(ulong value) => (uint)(value >> 32);

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.AggressiveOptimization)]
        public static uint ShiftRight24(uint value) => value >> 24;

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.AggressiveOptimization)]
        public static uint ShiftRight16(uint value) => value >> 16;

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.AggressiveOptimization)]
        public static uint ShiftRight8(uint value) => value >> 8;

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.AggressiveOptimization)]
        public static uint ShiftRight32_2(ulong value) => new QuadWord { QuadWordValue = value }.HighDoubleWordValue;

    }
}
