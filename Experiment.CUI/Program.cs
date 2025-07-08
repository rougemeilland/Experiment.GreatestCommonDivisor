#define OUTPUT_PERFORMANCE_LOG
//#define ANALYZE_PERFORMANCE
//#define ANALYZE_STATISTICS
#define UNIT_TEST
using System;
using BenchmarkDotNet.Running;

#if OUTPUT_PERFORMANCE_LOG && ANALYZE_PERFORMANCE
#error OUTPUT_PERFORMANCE_LOG と ANALYZE_PERFORMANCE を同時に設定することはできません。
#endif

#if OUTPUT_PERFORMANCE_LOG && ANALYZE_STATISTICS
#error OUTPUT_PERFORMANCE_LOG と ANALYZE_STATISTICS を同時に設定することはできません。
#endif

#if ANALYZE_PERFORMANCE && ANALYZE_STATISTICS
#error ANALYZE_PERFORMANCE と ANALYZE_STATISTICS を同時に設定することはできません。
#endif

#if ANALYZE_PERFORMANCE && UNIT_TEST
#error ANALYZE_PERFORMANCE と UNIT_TEST を同時に設定することはできません。
#endif

namespace Experiment.CUI
{
    internal sealed class Program
    {
        private static void Main()
        {
            Console.WriteLine($"System.Numerics.Vector.IsHardwareAccelerated={System.Numerics.Vector.IsHardwareAccelerated}");
            Console.WriteLine($"System.Runtime.Intrinsics.Vector64.IsHardwareAccelerated={System.Runtime.Intrinsics.Vector64.IsHardwareAccelerated}");
            Console.WriteLine($"System.Runtime.Intrinsics.Vector128.IsHardwareAccelerated={System.Runtime.Intrinsics.Vector128.IsHardwareAccelerated}");
            Console.WriteLine($"System.Runtime.Intrinsics.Vector256.IsHardwareAccelerated={System.Runtime.Intrinsics.Vector256.IsHardwareAccelerated}");
            Console.WriteLine($"System.Runtime.Intrinsics.Vector512.IsHardwareAccelerated={System.Runtime.Intrinsics.Vector512.IsHardwareAccelerated}");
            Console.WriteLine($"System.Runtime.Intrinsics.Arm.AdvSimd.IsSupported={System.Runtime.Intrinsics.Arm.AdvSimd.IsSupported}");
            Console.WriteLine($"System.Runtime.Intrinsics.Wasm.PackedSimd.IsSupported={System.Runtime.Intrinsics.Wasm.PackedSimd.IsSupported}");
            Console.WriteLine($"System.Runtime.Intrinsics.X86.Avx2.IsSupported={System.Runtime.Intrinsics.X86.Avx2.IsSupported}");
            Console.WriteLine($"System.Runtime.Intrinsics.X86.Avx512F.IsSupported={System.Runtime.Intrinsics.X86.Avx512F.IsSupported}");
            Console.WriteLine($"System.Runtime.Intrinsics.X86.Sse2.IsSupported={System.Runtime.Intrinsics.X86.Sse2.IsSupported}");
#if UNIT_TEST
            TestBigIntegerCalculator.Test();
#endif
#if ANALYZE_STATISTICS
            AnalyzeStatistics.Analyze();
#endif

#if ANALYZE_PERFORMANCE
            //AnalyzePerformance.AnalyzeGcd();
            AnalyzePerformance.AnalyzeShiftRight();
#endif
#if OUTPUT_PERFORMANCE_LOG
            _ = BenchmarkRunner.Run<BenchmarkOfGcd>();
            Console.WriteLine();
            //_ = BenchmarkRunner.Run<BenchmarkOfShiftRight>();
            //Console.WriteLine();
#endif

            Console.WriteLine("Completed.");
            Console.Beep();
#if UNIT_TEST && !OUTPUT_PERFORMANCE_LOG
            _ = Console.ReadLine();
#endif
        }
    }
}
