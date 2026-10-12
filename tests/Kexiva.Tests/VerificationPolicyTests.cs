using System;
using System.IO;
using Xunit;

namespace Kexiva.Tests;

public sealed class VerificationPolicyTests
{
    [Fact]
    public void WorkflowPolicy_RejectsInlineRestoreWithoutLockedMode()
    {
        using TemporaryDirectory repository = new();
        string workflows = Path.Combine(repository.Path, ".github", "workflows");
        Directory.CreateDirectory(workflows);
        File.WriteAllText(Path.Combine(workflows, "nightly.yml"), "jobs:\n  test:\n    steps:\n      - run: dotnet restore\n");

        InvalidOperationException exception = Assert.Throws<InvalidOperationException>(
            () => Verify.Program.CheckWorkflowPolicies(repository.Path));

        Assert.Contains("locked dependency restore", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void WorkflowPolicy_RejectsBlockBuildWithoutWarningsAsErrors()
    {
        using TemporaryDirectory repository = new();
        string workflows = Path.Combine(repository.Path, ".github", "workflows");
        Directory.CreateDirectory(workflows);
        File.WriteAllText(Path.Combine(workflows, "release.yml"), "jobs:\n  build:\n    steps:\n      - run: |\n          dotnet build -c Release\n");

        InvalidOperationException exception = Assert.Throws<InvalidOperationException>(
            () => Verify.Program.CheckWorkflowPolicies(repository.Path));

        Assert.Contains("warnings as errors", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void Coverage_RecognizesShortCoberturaSourcePathAndAppliesThreshold()
    {
        using TemporaryDirectory repository = new();
        string sourceDirectory = Path.Combine(repository.Path, "src", "Kexiva");
        string resultsDirectory = Path.Combine(repository.Path, "results");
        Directory.CreateDirectory(sourceDirectory);
        Directory.CreateDirectory(resultsDirectory);
        File.WriteAllText(Path.Combine(sourceDirectory, "Widget.cs"), "namespace Kexiva; public sealed class Widget { public int Value => 1; }");
        WriteCoverageReport(resultsDirectory, "Kexiva/Widget.cs", "<line number=\"1\" hits=\"0\" />");

        InvalidOperationException exception = Assert.Throws<InvalidOperationException>(
            () => Verify.Program.CheckCoverage(repository.Path, resultsDirectory));

        Assert.Contains("Line coverage is below", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void Coverage_RejectsProductionSourceWhenReportPathsDoNotMatchInventory()
    {
        using TemporaryDirectory repository = new();
        string sourceDirectory = Path.Combine(repository.Path, "src", "Kexiva");
        string resultsDirectory = Path.Combine(repository.Path, "results");
        Directory.CreateDirectory(sourceDirectory);
        Directory.CreateDirectory(resultsDirectory);
        File.WriteAllText(Path.Combine(sourceDirectory, "Widget.cs"), "namespace Kexiva; public sealed class Widget { public int Value => 1; }");
        WriteCoverageReport(resultsDirectory, "unrecognized/Other.cs", "<line number=\"1\" hits=\"1\" />");

        InvalidOperationException exception = Assert.Throws<InvalidOperationException>(
            () => Verify.Program.CheckCoverage(repository.Path, resultsDirectory));

        Assert.Contains("do not map to production source files", exception.Message, StringComparison.Ordinal);
    }

    private static void WriteCoverageReport(string resultsDirectory, string sourcePath, string line)
    {
        string report = $"<coverage><packages><package name=\"Kexiva\"><classes><class filename=\"{sourcePath}\"><lines>{line}</lines></class></classes></package></packages></coverage>";
        File.WriteAllText(Path.Combine(resultsDirectory, "coverage.cobertura.xml"), report);
    }

    private sealed class TemporaryDirectory : IDisposable
    {
        public TemporaryDirectory()
        {
            Path = System.IO.Path.Combine(System.IO.Path.GetTempPath(), Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(Path);
        }

        public string Path { get; }

        public void Dispose()
        {
            Directory.Delete(Path, recursive: true);
        }
    }
}
