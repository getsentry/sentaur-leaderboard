using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Sentaur.Leaderboard.Api;

public class LeaderboardContext(DbContextOptions<LeaderboardContext> options) : DbContext(options)
{
    public DbSet<ScoreEntry> ScoreEntries { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .Entity<ScoreEntry>(
                eb => eb.HasKey(nameof(ScoreEntry.Key)));
        base.OnModelCreating(modelBuilder);
    }

    protected override void OnConfiguring(DbContextOptionsBuilder options)
        => options.UseNpgsql();

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
