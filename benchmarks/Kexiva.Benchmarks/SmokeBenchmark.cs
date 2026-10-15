using System.Diagnostics.CodeAnalysis;
using BenchmarkDotNet.Attributes;

namespace Kexiva.Benchmarks;

/// <summary>
/// Smoke benchmark that proves the BenchmarkDotNet harness builds and runs.
/// BenchmarkDotNet creates benchmark instances by reflection and requires public, unsealed, non-static classes.
/// </summary>
[MemoryDiagnoser]
[SuppressMessage("Maintainability", "CA1515:Consider making public types internal", Justification = "BenchmarkDotNet requires benchmark classes to be public.")]
public class SmokeBenchmark
{
    private readonly int _value = 42;

    /// <summary>
    /// Returns a constant value held by the benchmark instance.
    /// </summary>
    /// <returns>The value.</returns>
    [Benchmark]
    public int Placeholder()
    {
        return _value;
    }
}
