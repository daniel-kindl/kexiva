using System;
using System.IO;

namespace Kexiva.Fuzz;

/// <summary>
/// Delivers each seed in a corpus, then a fixed number of byte mutations of that seed.
/// </summary>
internal static class SeedCorpusRunner
{
    /// <summary>
    /// Reads every file in <paramref name="corpusDirectory"/> and calls <paramref name="target"/> for the seed and each mutation.
    /// </summary>
    /// <param name="corpusDirectory">Directory that contains the seed files.</param>
    /// <param name="mutationsPerSeed">Number of mutated copies to deliver after each seed. Must be zero or greater.</param>
    /// <param name="target">Receives one buffer per delivery.</param>
    /// <returns>The number of buffers delivered.</returns>
    public static int Deliver(string corpusDirectory, int mutationsPerSeed, Action<ReadOnlyMemory<byte>> target)
    {
        ArgumentNullException.ThrowIfNull(corpusDirectory);
        ArgumentNullException.ThrowIfNull(target);
        ArgumentOutOfRangeException.ThrowIfNegative(mutationsPerSeed);

        string[] seeds = Directory.GetFiles(corpusDirectory);
        Array.Sort(seeds, StringComparer.Ordinal);
        int delivered = 0;
        foreach (string seedPath in seeds)
        {
            byte[] seed = File.ReadAllBytes(seedPath);
            target(seed);
            delivered++;
            for (int index = 0; index < mutationsPerSeed; index++)
            {
                target(Mutate(seed, index));
                delivered++;
            }
        }

        return delivered;
    }

    private static byte[] Mutate(byte[] seed, int index)
    {
        if (seed.Length == 0)
        {
            return [(byte)index];
        }

        byte[] copy = (byte[])seed.Clone();
        int position = index % copy.Length;
        copy[position] = unchecked((byte)(copy[position] + 1 + index));
        return copy;
    }
}
