using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;
using Xunit;

namespace Kexiva.Tests;

public sealed class ArchitectureTests
{
    [Fact]
    public void ProductionAssembly_DoesNotReferenceTestOrBenchmarkAssemblies()
    {
        Assembly productionAssembly = Assembly.Load("Kexiva");
        string[] referencedAssemblies = [.. productionAssembly.GetReferencedAssemblies().Select(assembly => assembly.Name ?? string.Empty)];

        Assert.DoesNotContain(referencedAssemblies, name => name.Contains("Tests", StringComparison.Ordinal));
        Assert.DoesNotContain(referencedAssemblies, name => name.Contains("Benchmarks", StringComparison.Ordinal));
    }

    [Fact]
    public void ProductionProject_DoesNotReferenceOtherProjects()
    {
        string projectPath = Path.Combine(FindRepositoryRoot(), "src", "Kexiva", "Kexiva.csproj");
        XDocument project = XDocument.Load(projectPath);

        Assert.Empty(project.Descendants("ProjectReference"));
    }

    [Fact]
    public void ProductionProject_UsesOnlyPrivateBuildInfrastructure()
    {
        string projectPath = Path.Combine(FindRepositoryRoot(), "src", "Kexiva", "Kexiva.csproj");
        XDocument project = XDocument.Load(projectPath);
        XElement[] packageReferences = [.. project.Descendants("PackageReference")];

        Assert.Equal(
            ["Nerdbank.GitVersioning", "Microsoft.CodeAnalysis.PublicApiAnalyzers"],
            packageReferences.Select(packageReference => (string?)packageReference.Attribute("Include")));
        Assert.All(packageReferences, packageReference =>
        {
            string? packageName = (string?)packageReference.Attribute("Include");
            Assert.True(
                string.Equals(packageName, "Nerdbank.GitVersioning", StringComparison.Ordinal)
                || string.Equals(packageName, "Microsoft.CodeAnalysis.PublicApiAnalyzers", StringComparison.Ordinal));
            Assert.Equal("all", (string?)packageReference.Attribute("PrivateAssets"));
        });
    }

    [Fact]
    public void ExportedTypes_UseTheLibraryNamespace()
    {
        Assembly productionAssembly = Assembly.Load("Kexiva");

        Assert.All(
            productionAssembly.GetExportedTypes(),
            type => Assert.True(
                string.Equals(type.Namespace, "Kexiva", StringComparison.Ordinal)
                || type.Namespace?.StartsWith("Kexiva.", StringComparison.Ordinal) == true));
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
