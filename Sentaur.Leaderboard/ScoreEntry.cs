namespace Sentaur.Leaderboard;

public record ScoreEntry(Guid Key, string Name, string Email, TimeSpan Duration, int Score, DateTimeOffset Timestamp);
