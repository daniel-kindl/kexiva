# Benchmark Methodology

## Tools
- BenchmarkDotNet for all measurements.

## Requirements
- Benchmark against equivalent standard .NET operations.
- Document workload and input distributions.
- Measure latency, throughput, and allocations.
- Prevent constant folding and dead-code elimination.
- Include valid, invalid, boundary, and overflow inputs where relevant.
- Record CPU, OS, architecture, .NET version, and benchmark configuration.
- Store reproducible benchmark definitions and human-readable results.
- Track performance changes between versions.

## Smoke Benchmark
A simple placeholder (`SmokeBenchmark`) exists to validate the infrastructure.

## CI
Shared CI runners may compile and execute smoke checks. Significant regression decisions require stable benchmarking hardware.

Do not claim performance improvements without benchmark evidence.
