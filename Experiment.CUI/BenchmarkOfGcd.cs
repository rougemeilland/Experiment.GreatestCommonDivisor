using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;

namespace Experiment.CUI
{
    [HtmlExporter]
    [ShortRunJob]
    [GroupBenchmarksBy( BenchmarkLogicalGroupRule.ByCategory)]
    public partial class BenchmarkOfGcd
    {
        private static readonly byte[] _0008bitData;
        private static readonly ushort[] _0016bitData;
        private static readonly uint[] _0032bitData;
        private static readonly ReadOnlyMemory<uint>[] _0064bitData;
        private static readonly ReadOnlyMemory<uint>[] _0128bitData;
        private static readonly ReadOnlyMemory<uint>[] _0256bitData;
        private static readonly ReadOnlyMemory<uint>[] _0512bitData;
        private static readonly ReadOnlyMemory<uint>[] _1024bitData;
        private static readonly ReadOnlyMemory<uint>[] _2048bitData;
        private static readonly ReadOnlyMemory<uint>[] _4096bitData;

        static BenchmarkOfGcd()
        {
            var baseDirectory = AppContext.BaseDirectory;
            _0008bitData = [.. File.ReadAllLines(Path.Combine(baseDirectory, "TestData", "test-data-for-performance-8bit.txt")).Select(text => byte.Parse(text, NumberStyles.None, CultureInfo.InvariantCulture)).OrderByDescending(value => value)];
            _0016bitData = [.. File.ReadAllLines(Path.Combine(baseDirectory, "TestData", "test-data-for-performance-16bit.txt")).Select(text => ushort.Parse(text, NumberStyles.None, CultureInfo.InvariantCulture)).OrderByDescending(value => value)];
            _0032bitData = [.. File.ReadAllLines(Path.Combine(baseDirectory, "TestData", "test-data-for-performance-32bit.txt")).Select(text => uint.Parse(text, NumberStyles.None, CultureInfo.InvariantCulture)).OrderByDescending(value => value)];
            _0064bitData = [.. File.ReadAllLines(Path.Combine(baseDirectory, "TestData", "test-data-for-performance-64bit.txt")).Select(text => BigInteger.Parse(text, NumberStyles.None, CultureInfo.InvariantCulture)).OrderByDescending(value => value).Select(value => value.ToUInt32Array())];
            _0128bitData = [.. File.ReadAllLines(Path.Combine(baseDirectory, "TestData", "test-data-for-performance-128bit.txt")).Select(text => BigInteger.Parse(text, NumberStyles.None, CultureInfo.InvariantCulture)).OrderByDescending(value => value).Select(value => value.ToUInt32Array())];
            _0256bitData = [.. File.ReadAllLines(Path.Combine(baseDirectory, "TestData", "test-data-for-performance-256bit.txt")).Select(text => BigInteger.Parse(text, NumberStyles.None, CultureInfo.InvariantCulture)).OrderByDescending(value => value).Select(value => value.ToUInt32Array())];
            _0512bitData = [.. File.ReadAllLines(Path.Combine(baseDirectory, "TestData", "test-data-for-performance-512bit.txt")).Select(text => BigInteger.Parse(text, NumberStyles.None, CultureInfo.InvariantCulture)).OrderByDescending(value => value).Select(value => value.ToUInt32Array())];
            _1024bitData = [.. File.ReadAllLines(Path.Combine(baseDirectory, "TestData", "test-data-for-performance-1024bit.txt")).Select(text => BigInteger.Parse(text, NumberStyles.None, CultureInfo.InvariantCulture)).OrderByDescending(value => value).Select(value => value.ToUInt32Array())];
            _2048bitData = [.. File.ReadAllLines(Path.Combine(baseDirectory, "TestData", "test-data-for-performance-2048bit.txt")).Select(text => BigInteger.Parse(text, NumberStyles.None, CultureInfo.InvariantCulture)).OrderByDescending(value => value).Select(value => value.ToUInt32Array())];
            _4096bitData = [.. File.ReadAllLines(Path.Combine(baseDirectory, "TestData", "test-data-for-performance-4096bit.txt")).Select(text => BigInteger.Parse(text, NumberStyles.None, CultureInfo.InvariantCulture)).OrderByDescending(value => value).Select(value => value.ToUInt32Array())];
        }
    }
}
