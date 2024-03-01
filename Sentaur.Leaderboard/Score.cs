namespace Sentaur.Leaderboard;

public record Score(string name, string email, TimeSpan duration, int score, DateTimeOffset timestamp);
