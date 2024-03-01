namespace Sentaur.Leaderboard;

public record ScoreEntry(string Name, string Email, TimeSpan Duration, int Score, DateTimeOffset Timestamp);
