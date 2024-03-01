namespace Sentaur.Leaderboard.Api;

internal class MockData
{
    public static List<ScoreEntry> MockScores { get; } = new();
    static MockData()
    {
        var summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };
        var mock =  Enumerable.Range(1, 5).Select(index =>
                new ScoreEntry
                (
                    Guid.NewGuid(),
                    summaries[Random.Shared.Next(summaries.Length)],
                    summaries[Random.Shared.Next(summaries.Length)] + "@santry.com",
                    TimeSpan.FromMinutes(Random.Shared.Next(1, 7)),
                    Random.Shared.Next(1, 10000),
                    DateTimeOffset.Now.Add(TimeSpan.FromDays(index))
                ))
            .ToArray();

        MockScores.AddRange(mock);
    }
}
