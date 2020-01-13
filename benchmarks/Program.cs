using System;
using System.Runtime.CompilerServices;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using BenchmarkDotNet.Jobs;

namespace MyBenchmarks
{
//  [DisassemblyDiagnoser(printAsm: true, printSource: true)] // !!! use the new diagnoser!!
    public class MyBenchmarks
    {
        int bytes;
        static byte[] array = new byte[160];
        internal static ReadOnlySpan<byte> test => new byte[65064];

        public MyBenchmarks()
        {
        }

        [Params(1, 1000, 123456, ulong.MaxValue)]
        public ulong Input { get; set; }

        [Benchmark (Baseline = true)]
        public bool Old_TryFormatUInt64X()
        {
            return Old.TryFormatUInt64X(Input, 0, false, array.AsSpan(), out bytes);    
        }

        [Benchmark]
        public bool New_TryFormatUInt64X()
        {
            return New.TryFormatUInt64X(Input, 0, false, array.AsSpan(), out bytes);
        }
    }

    public class Program
    {
        public static void Main(string[] args)
        {
            BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(args);
        }
    }
}