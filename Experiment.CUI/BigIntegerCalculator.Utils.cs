using System.Diagnostics;

namespace System.Numerics
{
    /// <remarks>
    /// <see href="https://github.com/dotnet/runtime/blob/main/src/libraries/System.Runtime.Numerics/src/System/Numerics/BigIntegerCalculator.AddSub.cs"/>
    /// </remarks>
    [Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0022:���\�b�h�Ɏ��{�̂��g�p����", Justification = "<�ۗ���>")]
    [Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0057:�͈͉��Z�q���g�p", Justification = "<�ۗ���>")]
    [Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0007:�ÖٓI�Ȍ^�̎g�p", Justification = "<�ۗ���>")]
    [Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE2003:�u���b�N�ƌ㑱�̃X�e�[�g�����g�̊Ԃɋ󔒍s���K�v�ł�", Justification = "<�ۗ���>")]
    [Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0047:�s�v�Ȃ������̍폜", Justification = "<�ۗ���>")]
    [Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0056:�C���f�b�N�X���Z�q���g�p", Justification = "<�ۗ���>")]
    [Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE2000:�����̋󔒍s�͎g�p�ł��܂���", Justification = "<�ۗ���>")]
    [Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE2001:���ߍ��݃X�e�[�g�����g�͓Ǝ��̍s�ɔz�u����K�v������܂�", Justification = "<�ۗ���>")]
    internal static partial class BigIntegerCalculator
    {
#if DEBUG
        // Mutable for unit testing...
        internal static
#else
        internal const
#endif
        int StackAllocThreshold = 64;

        public static int Compare(ReadOnlySpan<uint> left, ReadOnlySpan<uint> right)
        {
            Debug.Assert(left.Length <= right.Length || left.Slice(right.Length).ContainsAnyExcept(0u));
            Debug.Assert(left.Length >= right.Length || right.Slice(left.Length).ContainsAnyExcept(0u));

            if (left.Length != right.Length)
                return left.Length < right.Length ? -1 : 1;

            int iv = left.Length;
            while (--iv >= 0 && left[iv] == right[iv]) ;

            if (iv < 0)
                return 0;
            return left[iv] < right[iv] ? -1 : 1;
        }

        private static int CompareActual(ReadOnlySpan<uint> left, ReadOnlySpan<uint> right)
        {
            if (left.Length != right.Length)
            {
                if (left.Length < right.Length)
                {
                    if (ActualLength(right.Slice(left.Length)) > 0)
                        return -1;
                    right = right.Slice(0, left.Length);
                }
                else
                {
                    if (ActualLength(left.Slice(right.Length)) > 0)
                        return +1;
                    left = left.Slice(0, right.Length);
                }
            }
            return Compare(left, right);
        }

        public static int ActualLength(ReadOnlySpan<uint> value)
        {
            // Since we're reusing memory here, the actual length
            // of a given value may be less then the array's length

            int length = value.Length;

            while (length > 0 && value[length - 1] == 0)
                --length;
            return length;
        }

        private static int Reduce(Span<uint> bits, ReadOnlySpan<uint> modulus)
        {
            // Executes a modulo operation using the divide operation.

            if (bits.Length >= modulus.Length)
            {
                DivRem(bits, modulus, default);

                return ActualLength(bits.Slice(0, modulus.Length));
            }
            return bits.Length;
        }

        [Conditional("DEBUG")]
        public static void InitializeForDebug(Span<uint> bits)
        {
            // Reproduce the case where the return value of `stackalloc uint` is not initialized to zero.
            bits.Fill(0xCD);
        }
    }
}
