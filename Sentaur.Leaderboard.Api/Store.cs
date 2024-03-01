namespace Sentaur.Leaderboard.Api;

internal class Store
{
    private readonly List<ScoreEntry> _scoreEntries = new();

    public Store()
    {
        var summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };
        var mock =  Enumerable.Range(1, 5).Select(index =>
                new ScoreEntry
                (
                    summaries[Random.Shared.Next(summaries.Length)],
                    summaries[Random.Shared.Next(summaries.Length)] + "@santry.com",
                    TimeSpan.FromMinutes(Random.Shared.Next(1, 7)),
                    Random.Shared.Next(1, 10000),
                    DateTimeOffset.Now.Add(TimeSpan.FromDays(index))
                ))
            .ToArray();

        _scoreEntries.AddRange(mock);
    }

    public Task Add(ScoreEntry entry)
    {
        _scoreEntries.Add(entry);
        return Task.CompletedTask;
    }

    public Task<IEnumerable<ScoreEntry>> Get(CancellationToken token)
    {
        return Task.FromResult(_scoreEntries.AsEnumerable());
    }
}
