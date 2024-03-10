using Microsoft.EntityFrameworkCore;
using Sentaur.Leaderboard;
using Sentaur.Leaderboard.Api;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.UseSentry(); // DSN on appsettings.json

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        builder =>
        {
            builder
                .AllowAnyOrigin()
                .AllowAnyMethod()
                .AllowAnyHeader()
                .SetPreflightMaxAge(TimeSpan.FromDays(1));
        });
});

builder.Services.AddDbContextFactory<LeaderboardContext>(
    options =>
        options.UseSqlite());

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowAll");

app.UseHttpsRedirection();

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<LeaderboardContext>();
    context.Database.Migrate();
    if (!context.ScoreEntries.Any())
    {
        context.ScoreEntries.AddRange(MockData.MockScores);
        context.SaveChanges();
    }
}

app.MapGet("/score", (LeaderboardContext context, CancellationToken token) =>
{
    return context.ScoreEntries
        .OrderByDescending(p => p.Score)
        .ToListAsync(token);
})
.WithName("scores")
.WithOpenApi();

app.MapPost("/score", async (ScoreEntry scoreEntry, LeaderboardContext context, CancellationToken token) =>
{
    context.ScoreEntries.Add(scoreEntry);
    await context.SaveChangesAsync(token);
});

app.Run();


