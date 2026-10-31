# Kexiva

High-performance .NET library providing optimized alternatives to selected standard .NET data-processing APIs.

**Core philosophy: Same API. Same behavior. Better performance.**

Status: M1, Integer Parsing and Formatting. UInt32, Int32, Int64, and UInt64 expose `TryParse` and `Parse`.

## Build and test

Requires the .NET SDK version in [`global.json`](global.json).

```bash
./eng/verify.sh          # Linux and macOS
pwsh ./eng/verify.ps1    # Windows
```

CI runs the same scripts on Linux x64, Windows x64, macOS ARM64, and macOS x64.

## Documentation

- [AGENTS.md](AGENTS.md): agent entry point. It points to the dkstack guidelines.
- [CONTRIBUTING.md](CONTRIBUTING.md)
- [SECURITY.md](SECURITY.md)
- [Architecture](docs/architecture/README.md)
- [Decisions (ADRs)](docs/decisions/README.md)
- [Integer parsing and formatting](docs/compatibility/integer-parsing-and-formatting.md)
- [Testing](docs/testing/README.md)
- [Benchmarks](docs/benchmarks/README.md)
- [Repository settings](docs/development/repository-settings.md)

## License

MIT. See [LICENSE](LICENSE).
