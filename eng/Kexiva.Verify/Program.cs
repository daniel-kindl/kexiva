using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace Kexiva.Verify;

internal static class Program
{
    private const double _minimumLineCoverage = 0.80;
    private const double _minimumBranchCoverage = 0.70;

    private static int Main(string[] args)
    {
        try
        {
            string repositoryRoot = FindRepositoryRoot();
            string resultsDirectory = args.Length == 0
                ? Path.Combine(repositoryRoot, "TestResults")
                : Path.GetFullPath(args[0], repositoryRoot);

            CheckRepositoryPolicies(repositoryRoot);
            CheckCoverage(resultsDirectory);
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

    private static void CheckRepositoryPolicies(string root)
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

        string workflow = File.ReadAllText(Path.Combine(root, ".github", "workflows", "ci.yml"));
        string[] workflowLines = workflow.Split('\n', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
        Require(workflowLines.Contains("os: [ubuntu-latest, windows-latest]", StringComparer.Ordinal), "CI must run on Linux and Windows.");
        Require(workflowLines.Contains("run: ./eng/verify.sh", StringComparer.Ordinal), "CI must invoke eng/verify.sh.");
        Require(workflowLines.Contains("run: ./eng/verify.ps1", StringComparer.Ordinal), "CI must invoke eng/verify.ps1.");

        foreach (string workflowPath in Directory.EnumerateFiles(Path.Combine(root, ".github", "workflows"), "*.yml"))
        {
            foreach (string line in File.ReadLines(workflowPath))
            {
                string command = line.Trim();
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

        XDocument buildProperties = XDocument.Load(Path.Combine(root, "Directory.Build.props"));
        Require(PropertyIsTrue(buildProperties, "TreatWarningsAsErrors"), "Directory.Build.props must treat warnings as errors.");
        Require(PropertyIsTrue(buildProperties, "EnableNETAnalyzers"), "Directory.Build.props must enable .NET analyzers.");
        Require(PropertyIsTrue(buildProperties, "RestorePackagesWithLockFile"), "Directory.Build.props must enable NuGet lock files.");

        XDocument packageProperties = XDocument.Load(Path.Combine(root, "Directory.Packages.props"));
        XElement[] packageVersions = [.. packageProperties.Descendants("PackageVersion")];
        Require(packageVersions.Length > 0, "Directory.Packages.props must define centrally managed package versions.");
        foreach (XElement package in packageVersions)
        {
            string? version = (string?)package.Attribute("Version");
            Require(!string.IsNullOrWhiteSpace(version) && !version.Contains('*', StringComparison.Ordinal), "Package versions must be explicit and fixed.");
        }

        foreach (string projectPath in Directory.EnumerateFiles(root, "*.csproj", SearchOption.AllDirectories))
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

    private static void CheckCoverage(string resultsDirectory)
    {
        string[] reports = Directory.Exists(resultsDirectory)
            ? Directory.GetFiles(resultsDirectory, "coverage.cobertura.xml", SearchOption.AllDirectories)
            : [];
        Require(reports.Length > 0, $"No Cobertura coverage report exists under '{resultsDirectory}'.");

        int totalLines = 0;
        int coveredLines = 0;
        int totalBranches = 0;
        int coveredBranches = 0;
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
                string[] pathParts = sourcePath.Split('/', StringSplitOptions.RemoveEmptyEntries);
                bool isProductionSource = pathParts.Length >= 3
                    && pathParts[^3] == "src"
                    && pathParts[^2] == "Kexiva"
                    && !pathParts.Contains("obj", StringComparer.Ordinal)
                    && !string.Equals(pathParts[^1], "AssemblyInfo.cs", StringComparison.Ordinal);
                if (!isProductionSource)
                {
                    continue;
                }

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

        if (totalLines == 0)
        {
            Console.WriteLine($"Coverage thresholds are not applicable because the production assembly has {totalLines} coverable source lines.");
            return;
        }

        double lineCoverage = (double)coveredLines / totalLines;
        double branchCoverage = totalBranches == 0 ? 0 : (double)coveredBranches / totalBranches;
        string branchSummary = totalBranches == 0
            ? "branches not applicable (no production branches)"
            : $"{branchCoverage:P1} branches ({coveredBranches}/{totalBranches})";
        Console.WriteLine($"Production coverage: {lineCoverage:P1} lines ({coveredLines}/{totalLines}), {branchSummary}.");
        Require(lineCoverage >= _minimumLineCoverage, $"Line coverage is below {_minimumLineCoverage:P0}.");
        Require(totalBranches == 0 || branchCoverage >= _minimumBranchCoverage, $"Branch coverage is below {_minimumBranchCoverage:P0}.");
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
