using System;
using System.IO;
using Xunit;

namespace Kexiva.Tests;

/// <summary>
/// Tests that enforce verification policy checks for workflows and coverage.
/// </summary>
public sealed class VerificationPolicyTests
{
    /// <summary>
    /// Verifies that workflow policy rejects inline restore without locked mode.
    /// </summary>
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

    /// <summary>
    /// Verifies that workflow policy rejects block builds without warnings as errors.
    /// </summary>
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

    /// <summary>
    /// Verifies that coverage recognizes short Cobertura source paths and applies thresholds.
    /// </summary>
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

    /// <summary>
    /// Verifies that fuzz and mutation run in the nightly workflow and stay out of the pull request gate.
    /// </summary>
    [Fact]
    public void NightlyWorkflow_RunsFuzzAndMutationOutsideThePrGate()
    {
        string root = FindRepositoryRoot();
        string nightly = File.ReadAllText(Path.Combine(root, ".github", "workflows", "nightly.yml"));
        string verify = File.ReadAllText(Path.Combine(root, "eng", "verify.sh"));

        Assert.Contains("./eng/fuzz.sh", nightly, StringComparison.Ordinal);
        Assert.Contains("./eng/mutate.sh", nightly, StringComparison.Ordinal);
        Assert.DoesNotContain("fuzz.sh", verify, StringComparison.Ordinal);
        Assert.DoesNotContain("mutate.sh", verify, StringComparison.Ordinal);
        Assert.DoesNotContain("stryker", verify, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Verifies that two reports contribute one count for the same source line.
    /// </summary>
    [Fact]
    public void Coverage_MergesTheSameLineAcrossReports()
    {
        using TemporaryDirectory repository = new();
        string sourceDirectory = Path.Combine(repository.Path, "src", "Kexiva");
        string resultsDirectory = Path.Combine(repository.Path, "results");
        Directory.CreateDirectory(sourceDirectory);
        Directory.CreateDirectory(resultsDirectory);
        File.WriteAllText(Path.Combine(sourceDirectory, "Widget.cs"), "namespace Kexiva; public sealed class Widget { public int Value => 1; }");
        WriteNamedCoverageReport(resultsDirectory, "coverage.cobertura.unit.xml", "Kexiva/Widget.cs", "<line number=\"1\" hits=\"1\" condition-coverage=\"0% (0/10)\" />");
        WriteNamedCoverageReport(resultsDirectory, "coverage.cobertura.compatibility.xml", "Kexiva/Widget.cs", "<line number=\"1\" hits=\"1\" condition-coverage=\"80% (8/10)\" />");

        Verify.Program.CheckCoverage(repository.Path, resultsDirectory);
    }

    /// <summary>
    /// Verifies that one report still counts each repeated line.
    /// </summary>
    [Fact]
    public void Coverage_CountsEachRepeatedLineInOneReport()
    {
        using TemporaryDirectory repository = new();
        string sourceDirectory = Path.Combine(repository.Path, "src", "Kexiva");
        string resultsDirectory = Path.Combine(repository.Path, "results");
        Directory.CreateDirectory(sourceDirectory);
        Directory.CreateDirectory(resultsDirectory);
        File.WriteAllText(Path.Combine(sourceDirectory, "Widget.cs"), "namespace Kexiva; public sealed class Widget { public int Value => 1; }");
        WriteCoverageReport(resultsDirectory, "Kexiva/Widget.cs", "<line number=\"1\" hits=\"1\" /><line number=\"1\" hits=\"0\" />");

        InvalidOperationException exception = Assert.Throws<InvalidOperationException>(
            () => Verify.Program.CheckCoverage(repository.Path, resultsDirectory));

        Assert.Contains("Line coverage is below", exception.Message, StringComparison.Ordinal);
    }

    /// <summary>
    /// Verifies that coverage reads a Cobertura report whose name includes a coverlet.MTP timestamp.
    /// </summary>
    [Fact]
    public void Coverage_ReadsStampedCoberturaReport()
    {
        using TemporaryDirectory repository = new();
        string sourceDirectory = Path.Combine(repository.Path, "src", "Kexiva");
        string resultsDirectory = Path.Combine(repository.Path, "results");
        Directory.CreateDirectory(sourceDirectory);
        Directory.CreateDirectory(resultsDirectory);
        File.WriteAllText(Path.Combine(sourceDirectory, "Widget.cs"), "namespace Kexiva; public sealed class Widget { public int Value => 1; }");
        string report = "<coverage><packages><package name=\"Kexiva\"><classes><class filename=\"Kexiva/Widget.cs\"><lines><line number=\"1\" hits=\"1\" /></lines></class></classes></package></packages></coverage>";
        File.WriteAllText(Path.Combine(resultsDirectory, "coverage.cobertura.280326122803612.xml"), report);

        Verify.Program.CheckCoverage(repository.Path, resultsDirectory);
    }

    /// <summary>
    /// Verifies that a source file under build output is not a production source.
    /// </summary>
    /// <param name="directoryName">The build-output directory name.</param>
    [Theory]
    [InlineData("bin")]
    [InlineData("obj")]
    [InlineData("Bin")]
    public void Coverage_IgnoresSourceFileUnderBuildOutput(string directoryName)
    {
        ArgumentNullException.ThrowIfNull(directoryName);
        using TemporaryDirectory repository = new();
        string sourceDirectory = Path.Combine(repository.Path, "src", "Kexiva", directoryName);
        string resultsDirectory = Path.Combine(repository.Path, "results");
        Directory.CreateDirectory(sourceDirectory);
        Directory.CreateDirectory(resultsDirectory);
        File.WriteAllText(Path.Combine(sourceDirectory, "Widget.cs"), "namespace Kexiva; public sealed class Widget { public int Value => 1; }");
        WriteCoverageReport(resultsDirectory, "Kexiva/Widget.cs", "<line number=\"1\" hits=\"0\" />");

        Verify.Program.CheckCoverage(repository.Path, resultsDirectory);
    }

    /// <summary>
    /// Verifies that coverage rejects production sources when report paths do not match inventory.
    /// </summary>
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

    /// <summary>
    /// Verifies that the CI policy accepts a workflow with all four runners.
    /// </summary>
    [Fact]
    public void CiWorkflow_AcceptsAllFourRunners()
    {
        using TemporaryDirectory repository = new();
        WriteCiWorkflow(repository.Path, _allRunners);

        Verify.Program.CheckCiWorkflow(repository.Path);
    }

    /// <summary>
    /// Verifies that the CI policy rejects a workflow that misses any one of the four runners.
    /// </summary>
    /// <param name="missingRunner">The runner line that is removed.</param>
    [Theory]
    [InlineData("os: ubuntu-24.04")]
    [InlineData("os: windows-2025")]
    [InlineData("os: macos-15")]
    [InlineData("os: macos-15-intel")]
    public void CiWorkflow_RejectsMissingRunner(string missingRunner)
    {
        using TemporaryDirectory repository = new();
        WriteCiWorkflow(repository.Path, [.. Array.FindAll(_allRunners, runner => runner != missingRunner)]);

        InvalidOperationException exception = Assert.Throws<InvalidOperationException>(
            () => Verify.Program.CheckCiWorkflow(repository.Path));

        Assert.Contains($"missing '{missingRunner}'", exception.Message, StringComparison.Ordinal);
    }

    /// <summary>
    /// Verifies that the build policy accepts the repository Directory.Build.props.
    /// </summary>
    [Fact]
    public void BuildProperties_AcceptsRepositoryConfiguration()
    {
        using TemporaryDirectory repository = new();
        WriteBuildProperties(repository.Path, _requiredProperties);

        Verify.Program.CheckBuildProperties(repository.Path);
    }

    /// <summary>
    /// Verifies that the build policy rejects a missing or disabled NuGet audit.
    /// </summary>
    /// <param name="nuGetAudit">The NuGetAudit element, or an empty string for none.</param>
    [Theory]
    [InlineData("")]
    [InlineData("<NuGetAudit>false</NuGetAudit>")]
    public void BuildProperties_RejectsMissingOrDisabledNuGetAudit(string nuGetAudit)
    {
        using TemporaryDirectory repository = new();
        WriteBuildProperties(repository.Path, _requiredProperties.Replace("<NuGetAudit>true</NuGetAudit>", nuGetAudit, StringComparison.Ordinal));

        InvalidOperationException exception = Assert.Throws<InvalidOperationException>(
            () => Verify.Program.CheckBuildProperties(repository.Path));

        Assert.Contains("NuGet audit", exception.Message, StringComparison.Ordinal);
    }

    /// <summary>
    /// Verifies that repository policy ignores a generated project under build output.
    /// </summary>
    /// <param name="directoryName">The build-output directory name.</param>
    [Theory]
    [InlineData("bin")]
    [InlineData("obj")]
    [InlineData("Bin")]
    [InlineData("OBJ")]
    [InlineData("BenchmarkDotNet.Artifacts")]
    [InlineData("BenchmarkDotNet.Bin")]
    public void RepositoryPolicy_IgnoresProjectUnderBuildOutput(string directoryName)
    {
        ArgumentNullException.ThrowIfNull(directoryName);
        using TemporaryDirectory repository = new();
        WriteRepositoryPolicyFixture(repository.Path);
        WriteSourceProject(repository.Path, Path.Combine("src", "Widget"));
        WriteGeneratedProject(repository.Path, Path.Combine("nested", "deeper", directoryName, "more", "generated"));

        Verify.Program.CheckRepositoryPolicies(repository.Path);
    }

    /// <summary>
    /// Verifies that a source project outside build output still needs a lock file.
    /// </summary>
    /// <param name="relativeProjectPath">The project path, relative to the repository root.</param>
    [Theory]
    [InlineData("tools/binary/Tool.csproj")]
    [InlineData("src/bin.csproj")]
    public void RepositoryPolicy_RejectsSourceProjectWithoutLockFile(string relativeProjectPath)
    {
        ArgumentNullException.ThrowIfNull(relativeProjectPath);
        using TemporaryDirectory repository = new();
        WriteRepositoryPolicyFixture(repository.Path);
        WriteSourceProject(repository.Path, Path.Combine("src", "Widget"));
        string projectPath = Path.Combine(repository.Path, relativeProjectPath.Replace('/', Path.DirectorySeparatorChar));
        Directory.CreateDirectory(Path.GetDirectoryName(projectPath)!);
        File.WriteAllText(projectPath, "<Project Sdk=\"Microsoft.NET.Sdk\"><ItemGroup><PackageReference Include=\"X\" Version=\"1.0.0\" /></ItemGroup></Project>");

        InvalidOperationException exception = Assert.Throws<InvalidOperationException>(
            () => Verify.Program.CheckRepositoryPolicies(repository.Path));

        Assert.Contains("packages.lock.json", exception.Message, StringComparison.Ordinal);
        Assert.Contains(relativeProjectPath.Replace('/', Path.DirectorySeparatorChar), exception.Message, StringComparison.Ordinal);
    }

    /// <summary>
    /// Verifies that the build policy rejects each missing or weakened ADR 0002 build property.
    /// </summary>
    /// <param name="required">The required element that is replaced.</param>
    /// <param name="replacement">The weaker element, or an empty string for none.</param>
    /// <param name="expectedMessage">Part of the expected failure message.</param>
    [Theory]
    [InlineData("<MSBuildTreatWarningsAsErrors>true</MSBuildTreatWarningsAsErrors>", "", "MSBuild warnings as errors")]
    [InlineData("<EnforceCodeStyleInBuild>true</EnforceCodeStyleInBuild>", "<EnforceCodeStyleInBuild>false</EnforceCodeStyleInBuild>", "code style")]
    [InlineData("<AnalysisMode>All</AnalysisMode>", "<AnalysisMode>Recommended</AnalysisMode>", "AnalysisMode")]
    [InlineData("<NuGetAuditMode>all</NuGetAuditMode>", "<NuGetAuditMode>direct</NuGetAuditMode>", "NuGetAuditMode")]
    [InlineData("<NuGetAuditLevel>low</NuGetAuditLevel>", "<NuGetAuditLevel>high</NuGetAuditLevel>", "NuGetAuditLevel")]
    [InlineData("<RestoreLockedMode Condition=\"'$(CI)' == 'true'\">true</RestoreLockedMode>", "", "RestoreLockedMode")]
    [InlineData("<RestoreLockedMode Condition=\"'$(CI)' == 'true'\">true</RestoreLockedMode>", "<RestoreLockedMode Condition=\"'$(Other)' == 'true'\">true</RestoreLockedMode>", "RestoreLockedMode")]
    [InlineData("<ContinuousIntegrationBuild Condition=\"'$(CI)' == 'true'\">true</ContinuousIntegrationBuild>", "", "ContinuousIntegrationBuild")]
    public void BuildProperties_RejectsMissingOrWeakenedBuildPolicy(string required, string replacement, string expectedMessage)
    {
        using TemporaryDirectory repository = new();
        WriteBuildProperties(repository.Path, _requiredProperties.Replace(required, replacement, StringComparison.Ordinal));

        InvalidOperationException exception = Assert.Throws<InvalidOperationException>(
            () => Verify.Program.CheckBuildProperties(repository.Path));

        Assert.Contains(expectedMessage, exception.Message, StringComparison.Ordinal);
    }

    private static readonly string[] _allRunners = ["os: ubuntu-24.04", "os: windows-2025", "os: macos-15", "os: macos-15-intel"];

    private const string _requiredProperties =
        "<TreatWarningsAsErrors>true</TreatWarningsAsErrors>"
        + "<EnableNETAnalyzers>true</EnableNETAnalyzers>"
        + "<RestorePackagesWithLockFile>true</RestorePackagesWithLockFile>"
        + "<NuGetAudit>true</NuGetAudit>"
        + "<MSBuildTreatWarningsAsErrors>true</MSBuildTreatWarningsAsErrors>"
        + "<EnforceCodeStyleInBuild>true</EnforceCodeStyleInBuild>"
        + "<AnalysisMode>All</AnalysisMode>"
        + "<NuGetAuditMode>all</NuGetAuditMode>"
        + "<NuGetAuditLevel>low</NuGetAuditLevel>"
        + "<RestoreLockedMode Condition=\"'$(CI)' == 'true'\">true</RestoreLockedMode>"
        + "<ContinuousIntegrationBuild Condition=\"'$(CI)' == 'true'\">true</ContinuousIntegrationBuild>";

    private static void WriteCiWorkflow(string root, string[] runners)
    {
        string workflows = Path.Combine(root, ".github", "workflows");
        Directory.CreateDirectory(workflows);
        string matrix = string.Concat(Array.ConvertAll(runners, runner => $"          - {runner}\n"));
        File.WriteAllText(
            Path.Combine(workflows, "ci.yml"),
            "jobs:\n  verify:\n    strategy:\n      matrix:\n        include:\n" + matrix.Replace("- os:", "  os:", StringComparison.Ordinal)
            + "    steps:\n      - name: Verify (bash)\n        run: ./eng/verify.sh\n      - name: Verify (PowerShell)\n        run: ./eng/verify.ps1\n");
    }

    private static void WriteBuildProperties(string root, string properties)
    {
        File.WriteAllText(Path.Combine(root, "Directory.Build.props"), $"<Project><PropertyGroup>{properties}</PropertyGroup></Project>");
    }

    private static void WriteRepositoryPolicyFixture(string root)
    {
        WriteCiWorkflow(root, _allRunners);
        WriteBuildProperties(root, _requiredProperties);
        Directory.CreateDirectory(Path.Combine(root, "eng"));
        File.WriteAllText(
            Path.Combine(root, "eng", "verify.sh"),
            """
            dotnet restore --locked-mode
            dotnet build -c Release --no-restore --warnaserror
            dotnet format --verify-no-changes --severity info --no-restore
            dotnet test tests/Kexiva.Tests/Kexiva.Tests.csproj -c Release --no-build --results-directory "$results_directory" --coverlet --coverlet-output-format cobertura --coverlet-include "[Kexiva]*"
            dotnet test tests/Kexiva.Compatibility.Tests/Kexiva.Compatibility.Tests.csproj -c Release --no-build --results-directory "$results_directory" --coverlet --coverlet-output-format cobertura --coverlet-include "[Kexiva]*"
            dotnet run --project eng/Kexiva.Verify/Kexiva.Verify.csproj -c Release --no-build -- "$results_directory"
            """);
        File.WriteAllText(
            Path.Combine(root, "eng", "verify.ps1"),
            """
            Invoke-DotNet { dotnet restore --locked-mode }
            Invoke-DotNet { dotnet build -c Release --no-restore --warnaserror }
            Invoke-DotNet { dotnet format --verify-no-changes --severity info --no-restore }
            Invoke-DotNet { dotnet test tests/Kexiva.Tests/Kexiva.Tests.csproj -c Release --no-build --results-directory "$resultsDirectory" --coverlet --coverlet-output-format cobertura --coverlet-include "[Kexiva]*" }
            Invoke-DotNet { dotnet test tests/Kexiva.Compatibility.Tests/Kexiva.Compatibility.Tests.csproj -c Release --no-build --results-directory "$resultsDirectory" --coverlet --coverlet-output-format cobertura --coverlet-include "[Kexiva]*" }
            Invoke-DotNet { dotnet run --project eng/Kexiva.Verify/Kexiva.Verify.csproj -c Release --no-build -- "$resultsDirectory" }
            """);
        File.WriteAllText(
            Path.Combine(root, "Directory.Packages.props"),
            """
            <Project>
              <ItemGroup>
                <PackageVersion Include="xunit" Version="2.9.3" />
              </ItemGroup>
            </Project>
            """);
    }

    private static void WriteSourceProject(string root, string relativeDirectory)
    {
        string directory = Path.Combine(root, relativeDirectory);
        Directory.CreateDirectory(directory);
        File.WriteAllText(Path.Combine(directory, "Widget.csproj"), "<Project Sdk=\"Microsoft.NET.Sdk\"></Project>");
        File.WriteAllText(Path.Combine(directory, "packages.lock.json"), "{}");
    }

    private static void WriteGeneratedProject(string root, string relativeDirectory)
    {
        string directory = Path.Combine(root, relativeDirectory);
        Directory.CreateDirectory(directory);
        File.WriteAllText(
            Path.Combine(directory, "BenchmarkDotNet.Autogenerated.csproj"),
            "<Project Sdk=\"Microsoft.NET.Sdk\"><ItemGroup><PackageReference Include=\"BenchmarkDotNet\" Version=\"0.15.8\" /></ItemGroup></Project>");
    }

    private static string FindRepositoryRoot()
    {
        DirectoryInfo? directory = new(AppContext.BaseDirectory);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "Kexiva.slnx")))
        {
            directory = directory.Parent;
        }

        return directory?.FullName ?? throw new DirectoryNotFoundException("Could not find Kexiva.slnx.");
    }

    private static void WriteCoverageReport(string resultsDirectory, string sourcePath, string line)
    {
        WriteNamedCoverageReport(resultsDirectory, "coverage.cobertura.xml", sourcePath, line);
    }

    private static void WriteNamedCoverageReport(string resultsDirectory, string fileName, string sourcePath, string line)
    {
        string report = $"<coverage><packages><package name=\"Kexiva\"><classes><class filename=\"{sourcePath}\"><lines>{line}</lines></class></classes></package></packages></coverage>";
        File.WriteAllText(Path.Combine(resultsDirectory, fileName), report);
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
