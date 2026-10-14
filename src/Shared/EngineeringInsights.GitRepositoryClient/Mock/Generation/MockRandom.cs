namespace EngineeringInsights.GitRepositoryClient.Mock.Generation;

internal static class MockRandom
{
    /// <summary>Picks between <paramref name="min"/> and <paramref name="max"/> distinct candidates.</summary>
    public static List<string> Pick(Random random, IReadOnlyList<string> candidates, int min, int max)
    {
        var count = Math.Min(random.Next(min, max + 1), candidates.Count);
        var picked = new List<string>();

        while (picked.Count < count)
        {
            var candidate = candidates[random.Next(candidates.Count)];
            if (!picked.Contains(candidate))
            {
                picked.Add(candidate);
            }
        }

        return picked;
    }

    // The owner cannot always be determined from the repository.
    public static string? OwnerTeam(Random random, string product)
        => random.Next(4) == 0 ? null : $"Team {product}";
}
