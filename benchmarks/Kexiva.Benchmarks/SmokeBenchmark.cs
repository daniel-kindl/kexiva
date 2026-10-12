using BenchmarkDotNet.Attributes;

namespace Kexiva.Benchmarks;

[MemoryDiagnoser]
public class SmokeBenchmark
{
    [Benchmark]
    public int Placeholder()
    {
        return 42;
    }
}
