using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace System.Numerics
{
    internal static partial class BigIntegerCalculator
    {
        [System.Diagnostics.Conditional("DEBUG")]
        private static void AssertGcd(ReadOnlySpan<uint> u, ReadOnlySpan<uint> v, BigInteger expected)
        {
            var uValue = ToBigInteger(u);
            var vValue = ToBigInteger(v);
            var actual = BigInteger.GreatestCommonDivisor(uValue, vValue);
            //System.Diagnostics.Debug.WriteLine($"BigIntegerCalculator: Gcd({uValue:N0}, {vValue:N0}): actual = {actual:N0}, expected = {expected:N0}");
            Assert(actual == expected);
        }

        [System.Diagnostics.Conditional("DEBUG")]
        private static void Assert(bool condition, [CallerArgumentExpression(nameof(condition))] string? message = null, [CallerFilePath] string? sourceFilePath = null, [CallerLineNumber] int lineNumber = 0)
            => System.Diagnostics.Debug.Assert(condition, $"{message}: source-file=\"{sourceFilePath}\", line={lineNumber}");

        private static BigInteger ToBigInteger(ReadOnlySpan<uint> value)
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
