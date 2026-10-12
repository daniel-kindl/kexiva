using BenchmarkDotNet.Attributes;

namespace Kexiva.Benchmarks;

[MemoryDiagnoser]
internal sealed class SmokeBenchmark
{
    [Benchmark]
    public static int Placeholder()
    {
        return 42;
    }
}
