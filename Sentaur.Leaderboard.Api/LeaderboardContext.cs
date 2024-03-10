using Microsoft.EntityFrameworkCore;

namespace Sentaur.Leaderboard.Api;

public class LeaderboardContext : DbContext
{
    public DbSet<ScoreEntry> ScoreEntries { get; set; } = null!;

    private string _dbPath;

    public LeaderboardContext()
    {
        var folder = Environment.SpecialFolder.LocalApplicationData;
        var path = Environment.GetFolderPath(folder);
        _dbPath = Path.Join(path, "scores.db");
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .Entity<ScoreEntry>(
                eb => eb.HasKey(nameof(ScoreEntry.Key)));
        base.OnModelCreating(modelBuilder);
    }

    protected override void OnConfiguring(DbContextOptionsBuilder options)
        => options.UseInMemoryDatabase("InMemoryDb");
    // => options.UseSqlite($"Data Source={_dbPath}");
}
