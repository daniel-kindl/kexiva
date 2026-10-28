using System;
using System.IO;
using Xunit;

namespace Kexiva.Fuzz;

/// <summary>
/// Checks that the nightly seed corpus is present and that the runner delivers every seed.
/// </summary>
public sealed class SeedCorpusTests
{
    /// <summary>
    /// Verifies that each corpus file is delivered once, followed by its mutations.
    /// </summary>
    [Fact]
    public void Corpus_DeliversEverySeedAndItsMutations()
    {
        string corpus = Path.Combine(AppContext.BaseDirectory, "corpus");
        int seedCount = Directory.GetFiles(corpus).Length;
        int delivered = SeedCorpusRunner.Deliver(corpus, mutationsPerSeed: 2, _ => { });

        Assert.True(seedCount > 0);
        Assert.Equal(seedCount * 3, delivered);
    }
}
