using System;
using System.Numerics;
using System.Runtime.CompilerServices;
using BenchmarkDotNet.Attributes;

namespace Experiment.CUI
{
    public partial class BenchmarkOfGcd
    {
        [Benchmark]
        [BenchmarkCategory("0008bit")]
        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.AggressiveOptimization)]
        public void Gcd_0008bit_BigInteger()
        {
            var dataSet = _0008bitData;
            for (var index1 = 0; index1 < dataSet.Length - 1; ++index1)
            {
                for (var index2 = index1 + 1; index2 < dataSet.Length; ++index2)
                {
                    var left = dataSet[index1];
                    var right = dataSet[index2];
                    _ = BigIntegerCalculator.Gcd(left, right);
                }
            }
        }

        [Benchmark]
        [BenchmarkCategory("0016bit")]
        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.AggressiveOptimization)]
        public void Gcd_0016bit_BigInteger()
        {
            var dataSet = _0016bitData;
            for (var index1 = 0; index1 < dataSet.Length - 1; ++index1)
            {
                for (var index2 = index1 + 1; index2 < dataSet.Length; ++index2)
                {
                    var left = dataSet[index1];
                    var right = dataSet[index2];
                    _ = BigIntegerCalculator.Gcd(left, right);
                }
            }
        }

        [Benchmark]
        [BenchmarkCategory("0032bit")]
        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.AggressiveOptimization)]
        public void Gcd_0032bit_BigInteger()
        {
            var dataSet = _0032bitData;
            for (var index1 = 0; index1 < dataSet.Length - 1; ++index1)
            {
                for (var index2 = index1 + 1; index2 < dataSet.Length; ++index2)
                {
                    var left = dataSet[index1];
                    var right = dataSet[index2];
                    _ = BigIntegerCalculator.Gcd(left, right);
                }
            }
        }

        [Benchmark]
        [BenchmarkCategory("0064bit")]
        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.AggressiveOptimization)]
        public void Gcd_0064bit_BigInteger()
        {
            var dataSet = _0064bitData;
            Span<uint> result = stackalloc uint[2];
            for (var index1 = 0; index1 < dataSet.Length - 1; ++index1)
            {
                for (var index2 = index1 + 1; index2 < dataSet.Length; ++index2)
                {
                    var left = dataSet[index1].Span;
                    var right = dataSet[index2].Span;
                    BigIntegerCalculator.Gcd(left, right, result);
                }
            }
        }

        [Benchmark]
        [BenchmarkCategory("0128bit")]
        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.AggressiveOptimization)]
        public void Gcd_0128bit_BigInteger()
        {
            var dataSet = _0128bitData;
            Span<uint> result = stackalloc uint[4];
            for (var index1 = 0; index1 < dataSet.Length - 1; ++index1)
            {
                for (var index2 = index1 + 1; index2 < dataSet.Length; ++index2)
                {
                    var left = dataSet[index1].Span;
                    var right = dataSet[index2].Span;
                    BigIntegerCalculator.Gcd(left, right, result);
                }
            }
        }

        [Benchmark]
        [BenchmarkCategory("0256bit")]
        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.AggressiveOptimization)]
        public void Gcd_0256bit_BigInteger()
        {
            var dataSet = _0256bitData;
            Span<uint> result = stackalloc uint[8];
            for (var index1 = 0; index1 < dataSet.Length - 1; ++index1)
            {
                for (var index2 = index1 + 1; index2 < dataSet.Length; ++index2)
                {
                    var left = dataSet[index1].Span;
                    var right = dataSet[index2].Span;
                    BigIntegerCalculator.Gcd(left, right, result);
                }
            }
        }

        [Benchmark]
        [BenchmarkCategory("0512bit")]
        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.AggressiveOptimization)]
        public void Gcd_0512bit_BigInteger()
        {
            var dataSet = _0512bitData;
            Span<uint> result = stackalloc uint[16];
            for (var index1 = 0; index1 < dataSet.Length - 1; ++index1)
            {
                for (var index2 = index1 + 1; index2 < dataSet.Length; ++index2)
                {
                    var left = dataSet[index1].Span;
                    var right = dataSet[index2].Span;
                    BigIntegerCalculator.Gcd(left, right, result);
                }
            }
        }

        [Benchmark]
        [BenchmarkCategory("1024bit")]
        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.AggressiveOptimization)]
        public void Gcd_1024bit_BigInteger()
        {
            var dataSet = _1024bitData;
            Span<uint> result = stackalloc uint[32];
            for (var index1 = 0; index1 < dataSet.Length - 1; ++index1)
            {
                for (var index2 = index1 + 1; index2 < dataSet.Length; ++index2)
                {
                    var left = dataSet[index1].Span;
                    var right = dataSet[index2].Span;
                    BigIntegerCalculator.Gcd(left, right, result);
                }
            }
        }

        [Benchmark]
        [BenchmarkCategory("2048bit")]
        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.AggressiveOptimization)]
        public void Gcd_2048bit_BigInteger()
        {
            var dataSet = _2048bitData;
            Span<uint> result = stackalloc uint[64];
            for (var index1 = 0; index1 < dataSet.Length - 1; ++index1)
            {
                for (var index2 = index1 + 1; index2 < dataSet.Length; ++index2)
                {
                    var left = dataSet[index1].Span;
                    var right = dataSet[index2].Span;
                    BigIntegerCalculator.Gcd(left, right, result);
                }
            }
        }

        [Benchmark]
        [BenchmarkCategory("4096bit")]
        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.AggressiveOptimization)]
        public void Gcd_4096bit_BigInteger()
        {
            var dataSet = _4096bitData;
            Span<uint> result = stackalloc uint[128];
            for (var index1 = 0; index1 < dataSet.Length - 1; ++index1)
            {
                for (var index2 = index1 + 1; index2 < dataSet.Length; ++index2)
                {
                    var left = dataSet[index1].Span;
                    var right = dataSet[index2].Span;
                    BigIntegerCalculator.Gcd(left, right, result);
                }
            }
        }

        [Benchmark]
        [BenchmarkCategory("0008bit")]
        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.AggressiveOptimization)]
        public void Gcd_0008bit_Modified_Ver2_0()
        {
            var dataSet = _0008bitData;
            for (var index1 = 0; index1 < dataSet.Length - 1; ++index1)
            {
                for (var index2 = index1 + 1; index2 < dataSet.Length; ++index2)
                {
                    var left = dataSet[index1];
                    var right = dataSet[index2];
                    _ = BigIntegerCalculatorVer2.Gcd(left, right);
                }
            }
        }

        [Benchmark]
        [BenchmarkCategory("0016bit")]
        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.AggressiveOptimization)]
        public void Gcd_0016bit_Modified_Ver2_0()
        {
            var dataSet = _0016bitData;
            for (var index1 = 0; index1 < dataSet.Length - 1; ++index1)
            {
                for (var index2 = index1 + 1; index2 < dataSet.Length; ++index2)
                {
                    var left = dataSet[index1];
                    var right = dataSet[index2];
                    _ = BigIntegerCalculatorVer2.Gcd(left, right);
                }
            }
        }

        [Benchmark]
        [BenchmarkCategory("0032bit")]
        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.AggressiveOptimization)]
        public void Gcd_0032bit_Modified_Ver2_0()
        {
            var dataSet = _0032bitData;
            for (var index1 = 0; index1 < dataSet.Length - 1; ++index1)
            {
                for (var index2 = index1 + 1; index2 < dataSet.Length; ++index2)
                {
                    var left = dataSet[index1];
                    var right = dataSet[index2];
                    _ = BigIntegerCalculatorVer2.Gcd(left, right);
                }
            }
        }

        [Benchmark]
        [BenchmarkCategory("0064bit")]
        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.AggressiveOptimization)]
        public void Gcd_0064bit_Modified_Ver2_0()
        {
            var dataSet = _0064bitData;
            Span<uint> result = stackalloc uint[2];
            for (var index1 = 0; index1 < dataSet.Length - 1; ++index1)
            {
                for (var index2 = index1 + 1; index2 < dataSet.Length; ++index2)
                {
                    var left = dataSet[index1].Span;
                    var right = dataSet[index2].Span;
                    BigIntegerCalculatorVer2.Gcd(left, right, result);
                }
            }
        }

        [Benchmark]
        [BenchmarkCategory("0128bit")]
        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.AggressiveOptimization)]
        public void Gcd_0128bit_Modified_Ver2_0()
        {
            var dataSet = _0128bitData;
            Span<uint> result = stackalloc uint[4];
            for (var index1 = 0; index1 < dataSet.Length - 1; ++index1)
            {
                for (var index2 = index1 + 1; index2 < dataSet.Length; ++index2)
                {
                    var left = dataSet[index1].Span;
                    var right = dataSet[index2].Span;
                    BigIntegerCalculatorVer2.Gcd(left, right, result);
                }
            }
        }

        [Benchmark]
        [BenchmarkCategory("0256bit")]
        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.AggressiveOptimization)]
        public void Gcd_0256bit_Modified_Ver2_0()
        {
            var dataSet = _0256bitData;
            Span<uint> result = stackalloc uint[8];
            for (var index1 = 0; index1 < dataSet.Length - 1; ++index1)
            {
                for (var index2 = index1 + 1; index2 < dataSet.Length; ++index2)
                {
                    var left = dataSet[index1].Span;
                    var right = dataSet[index2].Span;
                    BigIntegerCalculatorVer2.Gcd(left, right, result);
                }
            }
        }

        [Benchmark]
        [BenchmarkCategory("0512bit")]
        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.AggressiveOptimization)]
        public void Gcd_0512bit_Modified_Ver2_0()
        {
            var dataSet = _0512bitData;
            Span<uint> result = stackalloc uint[16];
            for (var index1 = 0; index1 < dataSet.Length - 1; ++index1)
            {
                for (var index2 = index1 + 1; index2 < dataSet.Length; ++index2)
                {
                    var left = dataSet[index1].Span;
                    var right = dataSet[index2].Span;
                    BigIntegerCalculatorVer2.Gcd(left, right, result);
                }
            }
        }

        [Benchmark]
        [BenchmarkCategory("1024bit")]
        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.AggressiveOptimization)]
        public void Gcd_1024bit_Modified_Ver2_0()
        {
            var dataSet = _1024bitData;
            Span<uint> result = stackalloc uint[32];
            for (var index1 = 0; index1 < dataSet.Length - 1; ++index1)
            {
                for (var index2 = index1 + 1; index2 < dataSet.Length; ++index2)
                {
                    var left = dataSet[index1].Span;
                    var right = dataSet[index2].Span;
                    BigIntegerCalculatorVer2.Gcd(left, right, result);
                }
            }
        }

        [Benchmark]
        [BenchmarkCategory("2048bit")]
        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.AggressiveOptimization)]
        public void Gcd_2048bit_Modified_Ver2_0()
        {
            var dataSet = _2048bitData;
            Span<uint> result = stackalloc uint[64];
            for (var index1 = 0; index1 < dataSet.Length - 1; ++index1)
            {
                for (var index2 = index1 + 1; index2 < dataSet.Length; ++index2)
                {
                    var left = dataSet[index1].Span;
                    var right = dataSet[index2].Span;
                    BigIntegerCalculatorVer2.Gcd(left, right, result);
                }
            }
        }

        [Benchmark]
        [BenchmarkCategory("4096bit")]
        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.AggressiveOptimization)]
        public void Gcd_4096bit_Modified_Ver2_0()
        {
            var dataSet = _4096bitData;
            Span<uint> result = stackalloc uint[128];
            for (var index1 = 0; index1 < dataSet.Length - 1; ++index1)
            {
                for (var index2 = index1 + 1; index2 < dataSet.Length; ++index2)
                {
                    var left = dataSet[index1].Span;
                    var right = dataSet[index2].Span;
                    BigIntegerCalculatorVer2.Gcd(left, right, result);
                }
            }
        }

        [Benchmark]
        [BenchmarkCategory("0008bit")]
        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.AggressiveOptimization)]
        public void Gcd_0008bit_Modified_Ver2_1()
        {
            var dataSet = _0008bitData;
            for (var index1 = 0; index1 < dataSet.Length - 1; ++index1)
            {
                for (var index2 = index1 + 1; index2 < dataSet.Length; ++index2)
                {
                    var left = dataSet[index1];
                    var right = dataSet[index2];
                    _ = BigIntegerCalculatorVer2_1.Gcd(left, right);
                }
            }
        }

        [Benchmark]
        [BenchmarkCategory("0016bit")]
        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.AggressiveOptimization)]
        public void Gcd_0016bit_Modified_Ver2_1()
        {
            var dataSet = _0016bitData;
            for (var index1 = 0; index1 < dataSet.Length - 1; ++index1)
            {
                for (var index2 = index1 + 1; index2 < dataSet.Length; ++index2)
                {
                    var left = dataSet[index1];
                    var right = dataSet[index2];
                    _ = BigIntegerCalculatorVer2_1.Gcd(left, right);
                }
            }
        }

        [Benchmark]
        [BenchmarkCategory("0032bit")]
        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.AggressiveOptimization)]
        public void Gcd_0032bit_Modified_Ver2_1()
        {
            var dataSet = _0032bitData;
            for (var index1 = 0; index1 < dataSet.Length - 1; ++index1)
            {
                for (var index2 = index1 + 1; index2 < dataSet.Length; ++index2)
                {
                    var left = dataSet[index1];
                    var right = dataSet[index2];
                    _ = BigIntegerCalculatorVer2_1.Gcd(left, right);
                }
            }
        }

        [Benchmark]
        [BenchmarkCategory("0064bit")]
        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.AggressiveOptimization)]
        public void Gcd_0064bit_Modified_Ver2_1()
        {
            var dataSet = _0064bitData;
            Span<uint> result = stackalloc uint[2];
            for (var index1 = 0; index1 < dataSet.Length - 1; ++index1)
            {
                for (var index2 = index1 + 1; index2 < dataSet.Length; ++index2)
                {
                    var left = dataSet[index1].Span;
                    var right = dataSet[index2].Span;
                    BigIntegerCalculatorVer2_1.Gcd(left, right, result);
                }
            }
        }

        [Benchmark]
        [BenchmarkCategory("0128bit")]
        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.AggressiveOptimization)]
        public void Gcd_0128bit_Modified_Ver2_1()
        {
            var dataSet = _0128bitData;
            Span<uint> result = stackalloc uint[4];
            for (var index1 = 0; index1 < dataSet.Length - 1; ++index1)
            {
                for (var index2 = index1 + 1; index2 < dataSet.Length; ++index2)
                {
                    var left = dataSet[index1].Span;
                    var right = dataSet[index2].Span;
                    BigIntegerCalculatorVer2_1.Gcd(left, right, result);
                }
            }
        }

        [Benchmark]
        [BenchmarkCategory("0256bit")]
        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.AggressiveOptimization)]
        public void Gcd_0256bit_Modified_Ver2_1()
        {
            var dataSet = _0256bitData;
            Span<uint> result = stackalloc uint[8];
            for (var index1 = 0; index1 < dataSet.Length - 1; ++index1)
            {
                for (var index2 = index1 + 1; index2 < dataSet.Length; ++index2)
                {
                    var left = dataSet[index1].Span;
                    var right = dataSet[index2].Span;
                    BigIntegerCalculatorVer2_1.Gcd(left, right, result);
                }
            }
        }

        [Benchmark]
        [BenchmarkCategory("0512bit")]
        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.AggressiveOptimization)]
        public void Gcd_0512bit_Modified_Ver2_1()
        {
            var dataSet = _0512bitData;
            Span<uint> result = stackalloc uint[16];
            for (var index1 = 0; index1 < dataSet.Length - 1; ++index1)
            {
                for (var index2 = index1 + 1; index2 < dataSet.Length; ++index2)
                {
                    var left = dataSet[index1].Span;
                    var right = dataSet[index2].Span;
                    BigIntegerCalculatorVer2_1.Gcd(left, right, result);
                }
            }
        }

        [Benchmark]
        [BenchmarkCategory("1024bit")]
        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.AggressiveOptimization)]
        public void Gcd_1024bit_Modified_Ver2_1()
        {
            var dataSet = _1024bitData;
            Span<uint> result = stackalloc uint[32];
            for (var index1 = 0; index1 < dataSet.Length - 1; ++index1)
            {
                for (var index2 = index1 + 1; index2 < dataSet.Length; ++index2)
                {
                    var left = dataSet[index1].Span;
                    var right = dataSet[index2].Span;
                    BigIntegerCalculatorVer2_1.Gcd(left, right, result);
                }
            }
        }

        [Benchmark]
        [BenchmarkCategory("2048bit")]
        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.AggressiveOptimization)]
        public void Gcd_2048bit_Modified_Ver2_1()
        {
            var dataSet = _2048bitData;
            Span<uint> result = stackalloc uint[64];
            for (var index1 = 0; index1 < dataSet.Length - 1; ++index1)
            {
                for (var index2 = index1 + 1; index2 < dataSet.Length; ++index2)
                {
                    var left = dataSet[index1].Span;
                    var right = dataSet[index2].Span;
                    BigIntegerCalculatorVer2_1.Gcd(left, right, result);
                }
            }
        }

        [Benchmark]
        [BenchmarkCategory("4096bit")]
        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.AggressiveOptimization)]
        public void Gcd_4096bit_Modified_Ver2_1()
        {
            var dataSet = _4096bitData;
            Span<uint> result = stackalloc uint[128];
            for (var index1 = 0; index1 < dataSet.Length - 1; ++index1)
            {
                for (var index2 = index1 + 1; index2 < dataSet.Length; ++index2)
                {
                    var left = dataSet[index1].Span;
                    var right = dataSet[index2].Span;
                    BigIntegerCalculatorVer2_1.Gcd(left, right, result);
                }
            }
        }
    }
}
