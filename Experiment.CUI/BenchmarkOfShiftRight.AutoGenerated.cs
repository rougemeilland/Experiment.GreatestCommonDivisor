using System;
using System.Runtime.CompilerServices;
using BenchmarkDotNet.Attributes;

namespace Experiment.CUI
{
    public partial class BenchmarkOfShiftRight
    {
        [Benchmark]
        [BenchmarkCategory("0064bit")]
        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.AggressiveOptimization)]
        public void ShiftRight_0064bit_Try0()
        {
            Span<uint> buffer = stackalloc uint[2];
            var bitCount = 3;
            BigIntegerCalculatorVer2_1.TryToShiftRight(0, buffer, bitCount);
        }

        [Benchmark]
        [BenchmarkCategory("0096bit")]
        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.AggressiveOptimization)]
        public void ShiftRight_0096bit_Try0()
        {
            Span<uint> buffer = stackalloc uint[3];
            var bitCount = 3;
            BigIntegerCalculatorVer2_1.TryToShiftRight(0, buffer, bitCount);
        }

        [Benchmark]
        [BenchmarkCategory("0128bit")]
        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.AggressiveOptimization)]
        public void ShiftRight_0128bit_Try0()
        {
            Span<uint> buffer = stackalloc uint[4];
            var bitCount = 3;
            BigIntegerCalculatorVer2_1.TryToShiftRight(0, buffer, bitCount);
        }

        [Benchmark]
        [BenchmarkCategory("0160bit")]
        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.AggressiveOptimization)]
        public void ShiftRight_0160bit_Try0()
        {
            Span<uint> buffer = stackalloc uint[5];
            var bitCount = 3;
            BigIntegerCalculatorVer2_1.TryToShiftRight(0, buffer, bitCount);
        }

        [Benchmark]
        [BenchmarkCategory("0192bit")]
        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.AggressiveOptimization)]
        public void ShiftRight_0192bit_Try0()
        {
            Span<uint> buffer = stackalloc uint[6];
            var bitCount = 3;
            BigIntegerCalculatorVer2_1.TryToShiftRight(0, buffer, bitCount);
        }

        [Benchmark]
        [BenchmarkCategory("0224bit")]
        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.AggressiveOptimization)]
        public void ShiftRight_0224bit_Try0()
        {
            Span<uint> buffer = stackalloc uint[7];
            var bitCount = 3;
            BigIntegerCalculatorVer2_1.TryToShiftRight(0, buffer, bitCount);
        }

        [Benchmark]
        [BenchmarkCategory("0256bit")]
        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.AggressiveOptimization)]
        public void ShiftRight_0256bit_Try0()
        {
            Span<uint> buffer = stackalloc uint[8];
            var bitCount = 3;
            BigIntegerCalculatorVer2_1.TryToShiftRight(0, buffer, bitCount);
        }

        [Benchmark]
        [BenchmarkCategory("0512bit")]
        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.AggressiveOptimization)]
        public void ShiftRight_0512bit_Try0()
        {
            Span<uint> buffer = stackalloc uint[16];
            var bitCount = 3;
            BigIntegerCalculatorVer2_1.TryToShiftRight(0, buffer, bitCount);
        }

        [Benchmark]
        [BenchmarkCategory("1024bit")]
        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.AggressiveOptimization)]
        public void ShiftRight_1024bit_Try0()
        {
            Span<uint> buffer = stackalloc uint[32];
            var bitCount = 3;
            BigIntegerCalculatorVer2_1.TryToShiftRight(0, buffer, bitCount);
        }

        [Benchmark]
        [BenchmarkCategory("2048bit")]
        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.AggressiveOptimization)]
        public void ShiftRight_2048bit_Try0()
        {
            Span<uint> buffer = stackalloc uint[64];
            var bitCount = 3;
            BigIntegerCalculatorVer2_1.TryToShiftRight(0, buffer, bitCount);
        }

        [Benchmark]
        [BenchmarkCategory("4096bit")]
        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.AggressiveOptimization)]
        public void ShiftRight_4096bit_Try0()
        {
            Span<uint> buffer = stackalloc uint[128];
            var bitCount = 3;
            BigIntegerCalculatorVer2_1.TryToShiftRight(0, buffer, bitCount);
        }

        [Benchmark]
        [BenchmarkCategory("0064bit")]
        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.AggressiveOptimization)]
        public void ShiftRight_0064bit_Try1()
        {
            Span<uint> buffer = stackalloc uint[2];
            var bitCount = 3;
            BigIntegerCalculatorVer2_1.TryToShiftRight(1, buffer, bitCount);
        }

        [Benchmark]
        [BenchmarkCategory("0096bit")]
        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.AggressiveOptimization)]
        public void ShiftRight_0096bit_Try1()
        {
            Span<uint> buffer = stackalloc uint[3];
            var bitCount = 3;
            BigIntegerCalculatorVer2_1.TryToShiftRight(1, buffer, bitCount);
        }

        [Benchmark]
        [BenchmarkCategory("0128bit")]
        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.AggressiveOptimization)]
        public void ShiftRight_0128bit_Try1()
        {
            Span<uint> buffer = stackalloc uint[4];
            var bitCount = 3;
            BigIntegerCalculatorVer2_1.TryToShiftRight(1, buffer, bitCount);
        }

        [Benchmark]
        [BenchmarkCategory("0160bit")]
        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.AggressiveOptimization)]
        public void ShiftRight_0160bit_Try1()
        {
            Span<uint> buffer = stackalloc uint[5];
            var bitCount = 3;
            BigIntegerCalculatorVer2_1.TryToShiftRight(1, buffer, bitCount);
        }

        [Benchmark]
        [BenchmarkCategory("0192bit")]
        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.AggressiveOptimization)]
        public void ShiftRight_0192bit_Try1()
        {
            Span<uint> buffer = stackalloc uint[6];
            var bitCount = 3;
            BigIntegerCalculatorVer2_1.TryToShiftRight(1, buffer, bitCount);
        }

        [Benchmark]
        [BenchmarkCategory("0224bit")]
        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.AggressiveOptimization)]
        public void ShiftRight_0224bit_Try1()
        {
            Span<uint> buffer = stackalloc uint[7];
            var bitCount = 3;
            BigIntegerCalculatorVer2_1.TryToShiftRight(1, buffer, bitCount);
        }

        [Benchmark]
        [BenchmarkCategory("0256bit")]
        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.AggressiveOptimization)]
        public void ShiftRight_0256bit_Try1()
        {
            Span<uint> buffer = stackalloc uint[8];
            var bitCount = 3;
            BigIntegerCalculatorVer2_1.TryToShiftRight(1, buffer, bitCount);
        }

        [Benchmark]
        [BenchmarkCategory("0512bit")]
        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.AggressiveOptimization)]
        public void ShiftRight_0512bit_Try1()
        {
            Span<uint> buffer = stackalloc uint[16];
            var bitCount = 3;
            BigIntegerCalculatorVer2_1.TryToShiftRight(1, buffer, bitCount);
        }

        [Benchmark]
        [BenchmarkCategory("1024bit")]
        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.AggressiveOptimization)]
        public void ShiftRight_1024bit_Try1()
        {
            Span<uint> buffer = stackalloc uint[32];
            var bitCount = 3;
            BigIntegerCalculatorVer2_1.TryToShiftRight(1, buffer, bitCount);
        }

        [Benchmark]
        [BenchmarkCategory("2048bit")]
        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.AggressiveOptimization)]
        public void ShiftRight_2048bit_Try1()
        {
            Span<uint> buffer = stackalloc uint[64];
            var bitCount = 3;
            BigIntegerCalculatorVer2_1.TryToShiftRight(1, buffer, bitCount);
        }

        [Benchmark]
        [BenchmarkCategory("4096bit")]
        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.AggressiveOptimization)]
        public void ShiftRight_4096bit_Try1()
        {
            Span<uint> buffer = stackalloc uint[128];
            var bitCount = 3;
            BigIntegerCalculatorVer2_1.TryToShiftRight(1, buffer, bitCount);
        }

        [Benchmark]
        [BenchmarkCategory("0064bit")]
        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.AggressiveOptimization)]
        public void ShiftRight_0064bit_Try2()
        {
            Span<uint> buffer = stackalloc uint[2];
            var bitCount = 3;
            BigIntegerCalculatorVer2_1.TryToShiftRight(2, buffer, bitCount);
        }

        [Benchmark]
        [BenchmarkCategory("0096bit")]
        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.AggressiveOptimization)]
        public void ShiftRight_0096bit_Try2()
        {
            Span<uint> buffer = stackalloc uint[3];
            var bitCount = 3;
            BigIntegerCalculatorVer2_1.TryToShiftRight(2, buffer, bitCount);
        }

        [Benchmark]
        [BenchmarkCategory("0128bit")]
        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.AggressiveOptimization)]
        public void ShiftRight_0128bit_Try2()
        {
            Span<uint> buffer = stackalloc uint[4];
            var bitCount = 3;
            BigIntegerCalculatorVer2_1.TryToShiftRight(2, buffer, bitCount);
        }

        [Benchmark]
        [BenchmarkCategory("0160bit")]
        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.AggressiveOptimization)]
        public void ShiftRight_0160bit_Try2()
        {
            Span<uint> buffer = stackalloc uint[5];
            var bitCount = 3;
            BigIntegerCalculatorVer2_1.TryToShiftRight(2, buffer, bitCount);
        }

        [Benchmark]
        [BenchmarkCategory("0192bit")]
        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.AggressiveOptimization)]
        public void ShiftRight_0192bit_Try2()
        {
            Span<uint> buffer = stackalloc uint[6];
            var bitCount = 3;
            BigIntegerCalculatorVer2_1.TryToShiftRight(2, buffer, bitCount);
        }

        [Benchmark]
        [BenchmarkCategory("0224bit")]
        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.AggressiveOptimization)]
        public void ShiftRight_0224bit_Try2()
        {
            Span<uint> buffer = stackalloc uint[7];
            var bitCount = 3;
            BigIntegerCalculatorVer2_1.TryToShiftRight(2, buffer, bitCount);
        }

        [Benchmark]
        [BenchmarkCategory("0256bit")]
        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.AggressiveOptimization)]
        public void ShiftRight_0256bit_Try2()
        {
            Span<uint> buffer = stackalloc uint[8];
            var bitCount = 3;
            BigIntegerCalculatorVer2_1.TryToShiftRight(2, buffer, bitCount);
        }

        [Benchmark]
        [BenchmarkCategory("0512bit")]
        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.AggressiveOptimization)]
        public void ShiftRight_0512bit_Try2()
        {
            Span<uint> buffer = stackalloc uint[16];
            var bitCount = 3;
            BigIntegerCalculatorVer2_1.TryToShiftRight(2, buffer, bitCount);
        }

        [Benchmark]
        [BenchmarkCategory("1024bit")]
        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.AggressiveOptimization)]
        public void ShiftRight_1024bit_Try2()
        {
            Span<uint> buffer = stackalloc uint[32];
            var bitCount = 3;
            BigIntegerCalculatorVer2_1.TryToShiftRight(2, buffer, bitCount);
        }

        [Benchmark]
        [BenchmarkCategory("2048bit")]
        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.AggressiveOptimization)]
        public void ShiftRight_2048bit_Try2()
        {
            Span<uint> buffer = stackalloc uint[64];
            var bitCount = 3;
            BigIntegerCalculatorVer2_1.TryToShiftRight(2, buffer, bitCount);
        }

        [Benchmark]
        [BenchmarkCategory("4096bit")]
        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.AggressiveOptimization)]
        public void ShiftRight_4096bit_Try2()
        {
            Span<uint> buffer = stackalloc uint[128];
            var bitCount = 3;
            BigIntegerCalculatorVer2_1.TryToShiftRight(2, buffer, bitCount);
        }

        [Benchmark]
        [BenchmarkCategory("0064bit")]
        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.AggressiveOptimization)]
        public void ShiftRight_0064bit_Try3()
        {
            Span<uint> buffer = stackalloc uint[2];
            var bitCount = 3;
            BigIntegerCalculatorVer2_1.TryToShiftRight(3, buffer, bitCount);
        }

        [Benchmark]
        [BenchmarkCategory("0096bit")]
        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.AggressiveOptimization)]
        public void ShiftRight_0096bit_Try3()
        {
            Span<uint> buffer = stackalloc uint[3];
            var bitCount = 3;
            BigIntegerCalculatorVer2_1.TryToShiftRight(3, buffer, bitCount);
        }

        [Benchmark]
        [BenchmarkCategory("0128bit")]
        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.AggressiveOptimization)]
        public void ShiftRight_0128bit_Try3()
        {
            Span<uint> buffer = stackalloc uint[4];
            var bitCount = 3;
            BigIntegerCalculatorVer2_1.TryToShiftRight(3, buffer, bitCount);
        }

        [Benchmark]
        [BenchmarkCategory("0160bit")]
        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.AggressiveOptimization)]
        public void ShiftRight_0160bit_Try3()
        {
            Span<uint> buffer = stackalloc uint[5];
            var bitCount = 3;
            BigIntegerCalculatorVer2_1.TryToShiftRight(3, buffer, bitCount);
        }

        [Benchmark]
        [BenchmarkCategory("0192bit")]
        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.AggressiveOptimization)]
        public void ShiftRight_0192bit_Try3()
        {
            Span<uint> buffer = stackalloc uint[6];
            var bitCount = 3;
            BigIntegerCalculatorVer2_1.TryToShiftRight(3, buffer, bitCount);
        }

        [Benchmark]
        [BenchmarkCategory("0224bit")]
        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.AggressiveOptimization)]
        public void ShiftRight_0224bit_Try3()
        {
            Span<uint> buffer = stackalloc uint[7];
            var bitCount = 3;
            BigIntegerCalculatorVer2_1.TryToShiftRight(3, buffer, bitCount);
        }

        [Benchmark]
        [BenchmarkCategory("0256bit")]
        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.AggressiveOptimization)]
        public void ShiftRight_0256bit_Try3()
        {
            Span<uint> buffer = stackalloc uint[8];
            var bitCount = 3;
            BigIntegerCalculatorVer2_1.TryToShiftRight(3, buffer, bitCount);
        }

        [Benchmark]
        [BenchmarkCategory("0512bit")]
        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.AggressiveOptimization)]
        public void ShiftRight_0512bit_Try3()
        {
            Span<uint> buffer = stackalloc uint[16];
            var bitCount = 3;
            BigIntegerCalculatorVer2_1.TryToShiftRight(3, buffer, bitCount);
        }

        [Benchmark]
        [BenchmarkCategory("1024bit")]
        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.AggressiveOptimization)]
        public void ShiftRight_1024bit_Try3()
        {
            Span<uint> buffer = stackalloc uint[32];
            var bitCount = 3;
            BigIntegerCalculatorVer2_1.TryToShiftRight(3, buffer, bitCount);
        }

        [Benchmark]
        [BenchmarkCategory("2048bit")]
        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.AggressiveOptimization)]
        public void ShiftRight_2048bit_Try3()
        {
            Span<uint> buffer = stackalloc uint[64];
            var bitCount = 3;
            BigIntegerCalculatorVer2_1.TryToShiftRight(3, buffer, bitCount);
        }

        [Benchmark]
        [BenchmarkCategory("4096bit")]
        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.AggressiveOptimization)]
        public void ShiftRight_4096bit_Try3()
        {
            Span<uint> buffer = stackalloc uint[128];
            var bitCount = 3;
            BigIntegerCalculatorVer2_1.TryToShiftRight(3, buffer, bitCount);
        }

        [Benchmark]
        [BenchmarkCategory("0064bit")]
        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.AggressiveOptimization)]
        public void ShiftRight_0064bit_Try4()
        {
            Span<uint> buffer = stackalloc uint[2];
            var bitCount = 3;
            BigIntegerCalculatorVer2_1.TryToShiftRight(4, buffer, bitCount);
        }

        [Benchmark]
        [BenchmarkCategory("0096bit")]
        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.AggressiveOptimization)]
        public void ShiftRight_0096bit_Try4()
        {
            Span<uint> buffer = stackalloc uint[3];
            var bitCount = 3;
            BigIntegerCalculatorVer2_1.TryToShiftRight(4, buffer, bitCount);
        }

        [Benchmark]
        [BenchmarkCategory("0128bit")]
        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.AggressiveOptimization)]
        public void ShiftRight_0128bit_Try4()
        {
            Span<uint> buffer = stackalloc uint[4];
            var bitCount = 3;
            BigIntegerCalculatorVer2_1.TryToShiftRight(4, buffer, bitCount);
        }

        [Benchmark]
        [BenchmarkCategory("0160bit")]
        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.AggressiveOptimization)]
        public void ShiftRight_0160bit_Try4()
        {
            Span<uint> buffer = stackalloc uint[5];
            var bitCount = 3;
            BigIntegerCalculatorVer2_1.TryToShiftRight(4, buffer, bitCount);
        }

        [Benchmark]
        [BenchmarkCategory("0192bit")]
        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.AggressiveOptimization)]
        public void ShiftRight_0192bit_Try4()
        {
            Span<uint> buffer = stackalloc uint[6];
            var bitCount = 3;
            BigIntegerCalculatorVer2_1.TryToShiftRight(4, buffer, bitCount);
        }

        [Benchmark]
        [BenchmarkCategory("0224bit")]
        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.AggressiveOptimization)]
        public void ShiftRight_0224bit_Try4()
        {
            Span<uint> buffer = stackalloc uint[7];
            var bitCount = 3;
            BigIntegerCalculatorVer2_1.TryToShiftRight(4, buffer, bitCount);
        }

        [Benchmark]
        [BenchmarkCategory("0256bit")]
        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.AggressiveOptimization)]
        public void ShiftRight_0256bit_Try4()
        {
            Span<uint> buffer = stackalloc uint[8];
            var bitCount = 3;
            BigIntegerCalculatorVer2_1.TryToShiftRight(4, buffer, bitCount);
        }

        [Benchmark]
        [BenchmarkCategory("0512bit")]
        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.AggressiveOptimization)]
        public void ShiftRight_0512bit_Try4()
        {
            Span<uint> buffer = stackalloc uint[16];
            var bitCount = 3;
            BigIntegerCalculatorVer2_1.TryToShiftRight(4, buffer, bitCount);
        }

        [Benchmark]
        [BenchmarkCategory("1024bit")]
        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.AggressiveOptimization)]
        public void ShiftRight_1024bit_Try4()
        {
            Span<uint> buffer = stackalloc uint[32];
            var bitCount = 3;
            BigIntegerCalculatorVer2_1.TryToShiftRight(4, buffer, bitCount);
        }

        [Benchmark]
        [BenchmarkCategory("2048bit")]
        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.AggressiveOptimization)]
        public void ShiftRight_2048bit_Try4()
        {
            Span<uint> buffer = stackalloc uint[64];
            var bitCount = 3;
            BigIntegerCalculatorVer2_1.TryToShiftRight(4, buffer, bitCount);
        }

        [Benchmark]
        [BenchmarkCategory("4096bit")]
        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.AggressiveOptimization)]
        public void ShiftRight_4096bit_Try4()
        {
            Span<uint> buffer = stackalloc uint[128];
            var bitCount = 3;
            BigIntegerCalculatorVer2_1.TryToShiftRight(4, buffer, bitCount);
        }

        [Benchmark]
        [BenchmarkCategory("0064bit")]
        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.AggressiveOptimization)]
        public void ShiftRight_0064bit_Try5()
        {
            Span<uint> buffer = stackalloc uint[2];
            var bitCount = 3;
            BigIntegerCalculatorVer2_1.TryToShiftRight(5, buffer, bitCount);
        }

        [Benchmark]
        [BenchmarkCategory("0096bit")]
        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.AggressiveOptimization)]
        public void ShiftRight_0096bit_Try5()
        {
            Span<uint> buffer = stackalloc uint[3];
            var bitCount = 3;
            BigIntegerCalculatorVer2_1.TryToShiftRight(5, buffer, bitCount);
        }

        [Benchmark]
        [BenchmarkCategory("0128bit")]
        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.AggressiveOptimization)]
        public void ShiftRight_0128bit_Try5()
        {
            Span<uint> buffer = stackalloc uint[4];
            var bitCount = 3;
            BigIntegerCalculatorVer2_1.TryToShiftRight(5, buffer, bitCount);
        }

        [Benchmark]
        [BenchmarkCategory("0160bit")]
        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.AggressiveOptimization)]
        public void ShiftRight_0160bit_Try5()
        {
            Span<uint> buffer = stackalloc uint[5];
            var bitCount = 3;
            BigIntegerCalculatorVer2_1.TryToShiftRight(5, buffer, bitCount);
        }

        [Benchmark]
        [BenchmarkCategory("0192bit")]
        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.AggressiveOptimization)]
        public void ShiftRight_0192bit_Try5()
        {
            Span<uint> buffer = stackalloc uint[6];
            var bitCount = 3;
            BigIntegerCalculatorVer2_1.TryToShiftRight(5, buffer, bitCount);
        }

        [Benchmark]
        [BenchmarkCategory("0224bit")]
        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.AggressiveOptimization)]
        public void ShiftRight_0224bit_Try5()
        {
            Span<uint> buffer = stackalloc uint[7];
            var bitCount = 3;
            BigIntegerCalculatorVer2_1.TryToShiftRight(5, buffer, bitCount);
        }

        [Benchmark]
        [BenchmarkCategory("0256bit")]
        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.AggressiveOptimization)]
        public void ShiftRight_0256bit_Try5()
        {
            Span<uint> buffer = stackalloc uint[8];
            var bitCount = 3;
            BigIntegerCalculatorVer2_1.TryToShiftRight(5, buffer, bitCount);
        }

        [Benchmark]
        [BenchmarkCategory("0512bit")]
        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.AggressiveOptimization)]
        public void ShiftRight_0512bit_Try5()
        {
            Span<uint> buffer = stackalloc uint[16];
            var bitCount = 3;
            BigIntegerCalculatorVer2_1.TryToShiftRight(5, buffer, bitCount);
        }

        [Benchmark]
        [BenchmarkCategory("1024bit")]
        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.AggressiveOptimization)]
        public void ShiftRight_1024bit_Try5()
        {
            Span<uint> buffer = stackalloc uint[32];
            var bitCount = 3;
            BigIntegerCalculatorVer2_1.TryToShiftRight(5, buffer, bitCount);
        }

        [Benchmark]
        [BenchmarkCategory("2048bit")]
        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.AggressiveOptimization)]
        public void ShiftRight_2048bit_Try5()
        {
            Span<uint> buffer = stackalloc uint[64];
            var bitCount = 3;
            BigIntegerCalculatorVer2_1.TryToShiftRight(5, buffer, bitCount);
        }

        [Benchmark]
        [BenchmarkCategory("4096bit")]
        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.AggressiveOptimization)]
        public void ShiftRight_4096bit_Try5()
        {
            Span<uint> buffer = stackalloc uint[128];
            var bitCount = 3;
            BigIntegerCalculatorVer2_1.TryToShiftRight(5, buffer, bitCount);
        }

        [Benchmark]
        [BenchmarkCategory("0064bit")]
        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.AggressiveOptimization)]
        public void ShiftRight_0064bit_Try6()
        {
            Span<uint> buffer = stackalloc uint[2];
            var bitCount = 3;
            BigIntegerCalculatorVer2_1.TryToShiftRight(6, buffer, bitCount);
        }

        [Benchmark]
        [BenchmarkCategory("0096bit")]
        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.AggressiveOptimization)]
        public void ShiftRight_0096bit_Try6()
        {
            Span<uint> buffer = stackalloc uint[3];
            var bitCount = 3;
            BigIntegerCalculatorVer2_1.TryToShiftRight(6, buffer, bitCount);
        }

        [Benchmark]
        [BenchmarkCategory("0128bit")]
        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.AggressiveOptimization)]
        public void ShiftRight_0128bit_Try6()
        {
            Span<uint> buffer = stackalloc uint[4];
            var bitCount = 3;
            BigIntegerCalculatorVer2_1.TryToShiftRight(6, buffer, bitCount);
        }

        [Benchmark]
        [BenchmarkCategory("0160bit")]
        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.AggressiveOptimization)]
        public void ShiftRight_0160bit_Try6()
        {
            Span<uint> buffer = stackalloc uint[5];
            var bitCount = 3;
            BigIntegerCalculatorVer2_1.TryToShiftRight(6, buffer, bitCount);
        }

        [Benchmark]
        [BenchmarkCategory("0192bit")]
        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.AggressiveOptimization)]
        public void ShiftRight_0192bit_Try6()
        {
            Span<uint> buffer = stackalloc uint[6];
            var bitCount = 3;
            BigIntegerCalculatorVer2_1.TryToShiftRight(6, buffer, bitCount);
        }

        [Benchmark]
        [BenchmarkCategory("0224bit")]
        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.AggressiveOptimization)]
        public void ShiftRight_0224bit_Try6()
        {
            Span<uint> buffer = stackalloc uint[7];
            var bitCount = 3;
            BigIntegerCalculatorVer2_1.TryToShiftRight(6, buffer, bitCount);
        }

        [Benchmark]
        [BenchmarkCategory("0256bit")]
        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.AggressiveOptimization)]
        public void ShiftRight_0256bit_Try6()
        {
            Span<uint> buffer = stackalloc uint[8];
            var bitCount = 3;
            BigIntegerCalculatorVer2_1.TryToShiftRight(6, buffer, bitCount);
        }

        [Benchmark]
        [BenchmarkCategory("0512bit")]
        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.AggressiveOptimization)]
        public void ShiftRight_0512bit_Try6()
        {
            Span<uint> buffer = stackalloc uint[16];
            var bitCount = 3;
            BigIntegerCalculatorVer2_1.TryToShiftRight(6, buffer, bitCount);
        }

        [Benchmark]
        [BenchmarkCategory("1024bit")]
        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.AggressiveOptimization)]
        public void ShiftRight_1024bit_Try6()
        {
            Span<uint> buffer = stackalloc uint[32];
            var bitCount = 3;
            BigIntegerCalculatorVer2_1.TryToShiftRight(6, buffer, bitCount);
        }

        [Benchmark]
        [BenchmarkCategory("2048bit")]
        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.AggressiveOptimization)]
        public void ShiftRight_2048bit_Try6()
        {
            Span<uint> buffer = stackalloc uint[64];
            var bitCount = 3;
            BigIntegerCalculatorVer2_1.TryToShiftRight(6, buffer, bitCount);
        }

        [Benchmark]
        [BenchmarkCategory("4096bit")]
        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.AggressiveOptimization)]
        public void ShiftRight_4096bit_Try6()
        {
            Span<uint> buffer = stackalloc uint[128];
            var bitCount = 3;
            BigIntegerCalculatorVer2_1.TryToShiftRight(6, buffer, bitCount);
        }
    }
}
