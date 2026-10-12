using BenchmarkDotNet.Attributes;

namespace Kexiva.Benchmarks;

[MemoryDiagnoser]
internal static class SmokeBenchmark
{
    [Benchmark]
    public static int Placeholder()
    {
        return 42;
    }
}
