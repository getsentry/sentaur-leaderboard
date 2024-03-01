using Sentaur.Leaderboard;

var builder = WebApplication.CreateBuilder(args);

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

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowAll");

app.UseHttpsRedirection();

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};
app.MapGet("/score", () =>
{
    var forecast =  Enumerable.Range(1, 5).Select(index =>
        new ScoreEntry
        (
            summaries[Random.Shared.Next(summaries.Length)],
            summaries[Random.Shared.Next(summaries.Length)] + "@santry.com",
            TimeSpan.FromMinutes(Random.Shared.Next(1, 7)),
            Random.Shared.Next(1, 10000),
            DateTimeOffset.Now.Add(TimeSpan.FromDays(index))
        ))
        .ToArray();
    return forecast;
})
.WithName("scores")
.WithOpenApi();

app.Run();


