using System;
using System.Buffers.Binary;
using System.Numerics;

namespace Experiment.CUI
{
    internal static class BigIntegerExtensions
    {
        public static ReadOnlyMemory<uint> ToUInt32Array(this BigInteger value)
        {
            if (value < 0)
                throw new Exception();
            var byteArray = value.ToByteArray();
            var lengthOfByteArray = byteArray.Length;
            Array.Resize(ref byteArray, (byteArray.Length + sizeof(uint) - 1) / sizeof(uint) * sizeof(uint));
            if (byteArray.Length % sizeof(uint) != 0)
                throw new Exception();
            if (byteArray.Length < lengthOfByteArray)
                throw new Exception();
            if (byteArray.Length >= lengthOfByteArray + sizeof(uint))
                throw new Exception();
            var uintArray = new uint[byteArray.Length / sizeof(uint)];
            for (var index = 0; index < uintArray.Length; ++index)
                uintArray[index] = BinaryPrimitives.ReadUInt32BigEndian(byteArray.AsSpan(index * sizeof(uint), sizeof(uint)));
            var result = uintArray.AsMemory();
            while (result.Length > 0 && result.Span[^1] == 0)
                result = result[..^1];
            if (result.Length > 0 && result.Span[^1] == 0)
                throw new Exception();
            return result;
        }
    }
}
