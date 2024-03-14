using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Sentaur.Leaderboard.Api;

public class LeaderboardContext : DbContext
{
    public DbSet<ScoreEntry> ScoreEntries { get; set; } = null!;

    private string _dbPath;

    public LeaderboardContext(DbContextOptions<LeaderboardContext> options) : base(options)
    {
        var folder = Environment.SpecialFolder.LocalApplicationData;
        var path = Environment.GetFolderPath(folder);
        _dbPath = Path.Join(path, "scores.db");
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .Entity<ScoreEntry>(
                eb =>
                {
                    eb.HasKey(nameof(ScoreEntry.Key));
                });
        base.OnModelCreating(modelBuilder);
    }

    protected override void OnConfiguring(DbContextOptionsBuilder options)
#if !DEBUG
        => options.UseInMemoryDatabase("Leaderboard");
#else
        => options.UseNpgsql();
#endif

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        configurationBuilder
            .Properties<DateTimeOffset>()
            .HaveConversion<DateTimeOffsetConverter>();
    }
}

public class DateTimeOffsetConverter()
    : ValueConverter<DateTimeOffset, DateTimeOffset>(d => d.ToUniversalTime(),
    d => d.ToUniversalTime());
