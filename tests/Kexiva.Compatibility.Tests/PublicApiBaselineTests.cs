using System;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Xunit;

namespace Kexiva.Compatibility.Tests;

public sealed class PublicApiBaselineTests
{
    [Fact]
    public void ProductionProject_TracksNullableShippedAndUnshippedApiFiles()
    {
        string projectDirectory = Path.Combine(FindRepositoryRoot(), "src", "Kexiva");
        XDocument project = XDocument.Load(Path.Combine(projectDirectory, "Kexiva.csproj"));

        Assert.Contains(
            project.Descendants("PackageReference"),
            package => string.Equals((string?)package.Attribute("Include"), "Microsoft.CodeAnalysis.PublicApiAnalyzers", StringComparison.Ordinal));

        string[] apiFiles = ["PublicAPI.Shipped.txt", "PublicAPI.Unshipped.txt"];
        foreach (string apiFile in apiFiles)
        {
            string[] lines = File.ReadAllLines(Path.Combine(projectDirectory, apiFile));
            Assert.Contains("#nullable enable", lines);
        }
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
}
