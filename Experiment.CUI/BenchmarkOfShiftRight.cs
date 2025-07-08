using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Numerics;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;

namespace Experiment.CUI
{
    //[ShortRunJob]
    [HtmlExporter]
    [GroupBenchmarksBy(BenchmarkLogicalGroupRule.ByCategory)]
    public partial class BenchmarkOfShiftRight
    {
    }
}
