using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace Kexiva.Verify;

internal static class Program
{
    private const double _minimumLineCoverage = 0.80;
    private const double _minimumBranchCoverage = 0.70;

    // Generated projects live under these directory names. They are not source projects.
    private static readonly HashSet<string> _buildOutputDirectoryNames = new(StringComparer.OrdinalIgnoreCase)
    {
        "bin",
        "obj",
        "BenchmarkDotNet.Artifacts",
        "BenchmarkDotNet.Bin",
    };

    private static int Main(string[] args)
    {
        try
        {
            string repositoryRoot = FindRepositoryRoot();
            string resultsDirectory = args.Length == 0
                ? Path.Combine(repositoryRoot, "TestResults")
                : Path.GetFullPath(args[0], repositoryRoot);

            CheckRepositoryPolicies(repositoryRoot);
            CheckCoverage(repositoryRoot, resultsDirectory);
            return 0;
        }
        catch (InvalidOperationException exception)
        {
            Console.Error.WriteLine($"Verification failed: {exception.Message}");
            return 1;
        }
        catch (IOException exception)
        {
            Console.Error.WriteLine($"Verification failed: {exception.Message}");
            return 1;
        }
        catch (System.Xml.XmlException exception)
        {
            Console.Error.WriteLine($"Verification failed: {exception.Message}");
            return 1;
        }
        catch (ArgumentException exception)
        {
            Console.Error.WriteLine($"Verification failed: {exception.Message}");
            return 1;
        }
        catch (UnauthorizedAccessException exception)
        {
            Console.Error.WriteLine($"Verification failed: {exception.Message}");
            return 1;
        }
    }

    internal static void CheckRepositoryPolicies(string root)
    {
        string shellScript = File.ReadAllText(Path.Combine(root, "eng", "verify.sh"));
        string windowsScript = File.ReadAllText(Path.Combine(root, "eng", "verify.ps1"));
        string[] expectedCommands =
        [
            "dotnet restore --locked-mode",
            "dotnet build -c Release --no-restore --warnaserror",
            "dotnet format --verify-no-changes --severity info --no-restore",
            "dotnet test tests/Kexiva.Tests/Kexiva.Tests.csproj -c Release --no-build --settings eng/coverage.runsettings --collect:\"XPlat Code Coverage\" --results-directory <results>",
            "dotnet test tests/Kexiva.Compatibility.Tests/Kexiva.Compatibility.Tests.csproj -c Release --no-build",
            "dotnet run --project eng/Kexiva.Verify/Kexiva.Verify.csproj -c Release --no-build -- <results>"
        ];
        string[] shellCommands = ReadShellCommands(shellScript);
        string[] windowsCommands = ReadPowerShellCommands(windowsScript);
        Require(shellCommands.SequenceEqual(expectedCommands), "eng/verify.sh does not execute the required verification command sequence.");
        Require(windowsCommands.SequenceEqual(expectedCommands), "eng/verify.ps1 does not execute the same required verification command sequence.");

        string[] forbiddenMarkers = ["NOT CONFIGURED", "Placeholder", "exit 1", "exit 0"];
        foreach (string marker in forbiddenMarkers)
        {
            Require(!shellScript.Contains(marker, StringComparison.OrdinalIgnoreCase), $"eng/verify.sh contains forbidden marker '{marker}'.");
            Require(!windowsScript.Contains(marker, StringComparison.OrdinalIgnoreCase), $"eng/verify.ps1 contains forbidden marker '{marker}'.");
        }

        CheckCiWorkflow(root);
        CheckWorkflowPolicies(root);
        CheckBuildProperties(root);

        XDocument packageProperties = XDocument.Load(Path.Combine(root, "Directory.Packages.props"));
        XElement[] packageVersions = [.. packageProperties.Descendants("PackageVersion")];
        Require(packageVersions.Length > 0, "Directory.Packages.props must define centrally managed package versions.");
        foreach (XElement package in packageVersions)
        {
            string? version = (string?)package.Attribute("Version");
            Require(!string.IsNullOrWhiteSpace(version) && !version.Contains('*', StringComparison.Ordinal), "Package versions must be explicit and fixed.");
        }

        foreach (string projectPath in EnumerateRepositoryProjects(root))
        {
            XDocument project = XDocument.Load(projectPath);
            string lockFile = Path.Combine(Path.GetDirectoryName(projectPath)!, "packages.lock.json");
            Require(File.Exists(lockFile), $"{Path.GetRelativePath(root, projectPath)} must have a committed packages.lock.json file.");

            foreach (XElement packageReference in project.Descendants("PackageReference"))
            {
                Require(packageReference.Attribute("Version") is null, $"{Path.GetRelativePath(root, projectPath)} must use centrally managed package versions.");
            }
        }
    }

    internal static void CheckCiWorkflow(string root)
    {
        string workflow = File.ReadAllText(Path.Combine(root, ".github", "workflows", "ci.yml"));
        string[] workflowLines = workflow.Split('\n', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
        string[] requiredRunners = ["os: ubuntu-24.04", "os: windows-2025", "os: macos-15", "os: macos-15-intel"];
        foreach (string runner in requiredRunners)
        {
            Require(workflowLines.Contains(runner, StringComparer.Ordinal), $"CI must run on Linux, Windows, macOS ARM64, and macOS x64 (missing '{runner}').");
        }

        Require(workflowLines.Contains("run: ./eng/verify.sh", StringComparer.Ordinal), "CI must invoke eng/verify.sh.");
        Require(workflowLines.Contains("run: ./eng/verify.ps1", StringComparer.Ordinal), "CI must invoke eng/verify.ps1.");
    }

    internal static void CheckBuildProperties(string root)
    {
        XDocument buildProperties = XDocument.Load(Path.Combine(root, "Directory.Build.props"));
        Require(PropertyIsTrue(buildProperties, "TreatWarningsAsErrors"), "Directory.Build.props must treat warnings as errors.");
        Require(PropertyIsTrue(buildProperties, "EnableNETAnalyzers"), "Directory.Build.props must enable .NET analyzers.");
        Require(PropertyIsTrue(buildProperties, "RestorePackagesWithLockFile"), "Directory.Build.props must enable NuGet lock files.");
        Require(PropertyIsTrue(buildProperties, "NuGetAudit"), "Directory.Build.props must enable NuGet audit.");
        Require(PropertyIsTrue(buildProperties, "MSBuildTreatWarningsAsErrors"), "Directory.Build.props must treat MSBuild warnings as errors.");
        Require(PropertyIsTrue(buildProperties, "EnforceCodeStyleInBuild"), "Directory.Build.props must enforce code style in build.");
        Require(PropertyEquals(buildProperties, "AnalysisMode", "All"), "Directory.Build.props must set AnalysisMode to All.");
        Require(PropertyEquals(buildProperties, "NuGetAuditMode", "all"), "Directory.Build.props must set NuGetAuditMode to all.");
        Require(PropertyEquals(buildProperties, "NuGetAuditLevel", "low"), "Directory.Build.props must set NuGetAuditLevel to low.");
        Require(PropertyIsTrueInCi(buildProperties, "RestoreLockedMode"), "Directory.Build.props must enable RestoreLockedMode in CI.");
        Require(PropertyIsTrueInCi(buildProperties, "ContinuousIntegrationBuild"), "Directory.Build.props must enable ContinuousIntegrationBuild in CI.");
    }

    private static bool PropertyEquals(XDocument document, string propertyName, string expected)
    {
        return string.Equals((string?)document.Descendants(propertyName).FirstOrDefault(), expected, StringComparison.OrdinalIgnoreCase);
    }

    private static bool PropertyIsTrueInCi(XDocument document, string propertyName)
    {
        return document.Descendants(propertyName).Any(element =>
            string.Equals((string)element, "true", StringComparison.OrdinalIgnoreCase)
            && (element.Attribute("Condition") is null
                || string.Equals(((string?)element.Attribute("Condition"))?.Replace(" ", string.Empty, StringComparison.Ordinal), "'$(CI)'=='true'", StringComparison.Ordinal)));
    }

    internal static void CheckWorkflowPolicies(string root)
    {
        foreach (string workflowPath in Directory.EnumerateFiles(Path.Combine(root, ".github", "workflows"), "*.yml"))
        {
            foreach (string command in ReadWorkflowCommands(File.ReadAllLines(workflowPath)))
            {
                if (command.StartsWith("dotnet restore", StringComparison.Ordinal))
                {
                    Require(command.Contains("--locked-mode", StringComparison.Ordinal), $"{Path.GetFileName(workflowPath)} must use locked dependency restore.");
                }

                if (command.StartsWith("dotnet build", StringComparison.Ordinal))
                {
                    Require(command.Contains("--warnaserror", StringComparison.Ordinal), $"{Path.GetFileName(workflowPath)} must treat build warnings as errors.");
                }
            }
        }
    }

    private static string[] ReadWorkflowCommands(string[] lines)
    {
        System.Collections.Generic.List<string> commands = [];
        int? blockIndent = null;
        foreach (string line in lines)
        {
            string trimmed = line.TrimStart();
            int indentation = line.Length - trimmed.Length;
            if (blockIndent is int requiredIndent)
            {
                if (trimmed.Length == 0)
                {
                    continue;
                }

                if (indentation > requiredIndent)
                {
                    AddCommands(trimmed, commands);
                    continue;
                }

                blockIndent = null;
            }

            string? runValue = trimmed.StartsWith("- run:", StringComparison.Ordinal)
                ? trimmed[6..].Trim()
                : trimmed.StartsWith("run:", StringComparison.Ordinal)
                    ? trimmed[4..].Trim()
                    : null;
            if (runValue is null)
            {
                continue;
            }

            if (runValue.StartsWith('|') || runValue.StartsWith('>'))
            {
                blockIndent = indentation;
                continue;
            }

            AddCommands(runValue, commands);
        }

        return [.. commands];
    }

    private static void AddCommands(string commandLine, System.Collections.Generic.List<string> commands)
    {
        foreach (string command in commandLine.Split([';', '&'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            if (command.StartsWith("dotnet ", StringComparison.Ordinal))
            {
                commands.Add(command);
            }
        }
    }

    private static string[] ReadShellCommands(string script)
    {
        return [.. script.Split('\n', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
            .Where(line => line.StartsWith("dotnet ", StringComparison.Ordinal))
            .Select(NormalizeCommand)];
    }

    private static string[] ReadPowerShellCommands(string script)
    {
        const string prefix = "Invoke-DotNet { ";
        return [.. script.Split('\n', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
            .Where(line => line.StartsWith(prefix, StringComparison.Ordinal) && line.EndsWith(" }", StringComparison.Ordinal))
            .Select(line => NormalizeCommand(line[prefix.Length..^2]))];
    }

    private static string NormalizeCommand(string command)
    {
        return command
            .Replace("\"$results_directory\"", "<results>", StringComparison.Ordinal)
            .Replace("\"$resultsDirectory\"", "<results>", StringComparison.Ordinal);
    }

    internal static void CheckCoverage(string repositoryRoot, string resultsDirectory)
    {
        string productionDirectory = Path.Combine(repositoryRoot, "src", "Kexiva");
        string[] productionFiles = [.. Directory.EnumerateFiles(productionDirectory, "*.cs", SearchOption.AllDirectories)
            .Where(path => !IsUnderBuildOutput(productionDirectory, path))
            .Where(path => !string.Equals(Path.GetFileName(path), "AssemblyInfo.cs", StringComparison.OrdinalIgnoreCase))
            .Select(path => Path.GetRelativePath(productionDirectory, path).Replace('\\', '/'))];
        string[] reports = Directory.Exists(resultsDirectory)
            ? Directory.GetFiles(resultsDirectory, "coverage.cobertura.xml", SearchOption.AllDirectories)
            : [];
        Require(reports.Length > 0, $"No Cobertura coverage report exists under '{resultsDirectory}'.");

        int totalLines = 0;
        int coveredLines = 0;
        int totalBranches = 0;
        int coveredBranches = 0;
        System.Collections.Generic.HashSet<string> matchedProductionFiles = new(StringComparer.OrdinalIgnoreCase);
        foreach (string report in reports)
        {
            XDocument document = XDocument.Load(report);
            XElement? coverage = document.Root;
            Require(coverage?.Name.LocalName == "coverage", $"'{report}' is not a Cobertura report.");
            Require(
                coverage!.Descendants("package").Any(package => string.Equals((string?)package.Attribute("name"), "Kexiva", StringComparison.Ordinal)),
                $"'{report}' does not contain coverage data for the Kexiva production assembly.");

            foreach (XElement sourceClass in coverage.Descendants("class"))
            {
                string sourcePath = ((string?)sourceClass.Attribute("filename") ?? string.Empty).Replace('\\', '/');
                string? productionFile = productionFiles.FirstOrDefault(file => sourcePath.EndsWith(file, StringComparison.OrdinalIgnoreCase));
                productionFile ??= productionFiles
                    .Where(file => string.Equals(Path.GetFileName(file), Path.GetFileName(sourcePath), StringComparison.OrdinalIgnoreCase))
                    .Take(2)
                    .Count() == 1
                    ? productionFiles.First(file => string.Equals(Path.GetFileName(file), Path.GetFileName(sourcePath), StringComparison.OrdinalIgnoreCase))
                    : null;
                if (productionFile is null)
                {
                    continue;
                }

                matchedProductionFiles.Add(productionFile);

                foreach (XElement sourceLine in sourceClass.Descendants("line"))
                {
                    totalLines++;
                    if (int.TryParse((string?)sourceLine.Attribute("hits"), NumberStyles.Integer, CultureInfo.InvariantCulture, out int hits) && hits > 0)
                    {
                        coveredLines++;
                    }

                    string? conditionCoverage = (string?)sourceLine.Attribute("condition-coverage");
                    if (conditionCoverage is not null)
                    {
                        int open = conditionCoverage.IndexOf('(', StringComparison.Ordinal);
                        int slash = conditionCoverage.IndexOf('/', open + 1);
                        int close = conditionCoverage.IndexOf(')', slash + 1);
                        if (open >= 0 && slash > open && close > slash
                            && int.TryParse(conditionCoverage.AsSpan(open + 1, slash - open - 1), NumberStyles.Integer, CultureInfo.InvariantCulture, out int covered)
                            && int.TryParse(conditionCoverage.AsSpan(slash + 1, close - slash - 1), NumberStyles.Integer, CultureInfo.InvariantCulture, out int total))
                        {
                            coveredBranches += covered;
                            totalBranches += total;
                        }
                    }
                }
            }
        }

        if (productionFiles.Length == 0)
        {
            Console.WriteLine($"Coverage thresholds are not applicable because the production assembly has {totalLines} coverable source lines.");
            return;
        }

        Require(matchedProductionFiles.Count > 0, "Coverage reports do not map to production source files in src/Kexiva.");
        Require(totalLines > 0, "Coverage reports map to production files but contain no coverable source lines.");

        double lineCoverage = (double)coveredLines / totalLines;
        double branchCoverage = totalBranches == 0 ? 0 : (double)coveredBranches / totalBranches;
        string branchSummary = totalBranches == 0
            ? "branches not applicable (no production branches)"
            : $"{branchCoverage:P1} branches ({coveredBranches}/{totalBranches})";
        Console.WriteLine($"Production coverage: {lineCoverage:P1} lines ({coveredLines}/{totalLines}), {branchSummary}.");
        Require(lineCoverage >= _minimumLineCoverage, $"Line coverage is below {_minimumLineCoverage:P0}.");
        Require(totalBranches == 0 || branchCoverage >= _minimumBranchCoverage, $"Branch coverage is below {_minimumBranchCoverage:P0}.");
    }

    private static List<string> EnumerateRepositoryProjects(string root)
    {
        List<string> projects = [];
        CollectRepositoryProjects(new DirectoryInfo(root), projects);
        return projects;
    }

    private static void CollectRepositoryProjects(DirectoryInfo directory, List<string> projects)
    {
        foreach (FileInfo file in directory.EnumerateFiles("*.csproj"))
        {
            projects.Add(file.FullName);
        }

        foreach (DirectoryInfo child in directory.EnumerateDirectories())
        {
            if (IsBuildOutputDirectory(child.Name))
            {
                continue;
            }

            CollectRepositoryProjects(child, projects);
        }
    }

    private static bool IsUnderBuildOutput(string root, string path)
    {
        string relative = Path.GetRelativePath(root, path);
        foreach (string segment in relative.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar))
        {
            if (IsBuildOutputDirectory(segment))
            {
                return true;
            }
        }

        return false;
    }

    private static bool IsBuildOutputDirectory(string name)
    {
        return _buildOutputDirectoryNames.Contains(name);
    }

    private static bool PropertyIsTrue(XDocument document, string propertyName)
    {
        return string.Equals((string?)document.Descendants(propertyName).FirstOrDefault(), "true", StringComparison.OrdinalIgnoreCase);
    }

    private static string FindRepositoryRoot()
    {
        DirectoryInfo? directory = new(Directory.GetCurrentDirectory());
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "Kexiva.slnx")))
        {
            directory = directory.Parent;
        }

        return directory?.FullName ?? throw new DirectoryNotFoundException("Could not find Kexiva.slnx from the current directory.");
    }

    private static void Require(bool condition, string message)
    {
        if (!condition)
        {
            throw new InvalidOperationException(message);
        }
    }
}
