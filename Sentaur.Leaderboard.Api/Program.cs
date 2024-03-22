using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Sentaur.Leaderboard;
using Sentaur.Leaderboard.Api;

var builder = WebApplication.CreateBuilder(args);

// Add Sentry
builder.WebHost.UseSentry();

builder.Services
    .AddEndpointsApiExplorer()
    .AddSwaggerGen(o =>
    {
        o.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
        {
            Name = "Authorization",
            Type = SecuritySchemeType.ApiKey,
            Scheme = "Bearer",
            BearerFormat = "JWT",
            In = ParameterLocation.Header,
            Description = "JSON Web Token based security",
        });
        o.AddSecurityRequirement(new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = "Bearer"
                    }
                },
                Array.Empty<string>()
            }
        });
    })
    .AddCors(options =>
    {
        options.AddPolicy("AllowAll",
            policyBuilder =>
            {
                policyBuilder
                    .AllowAnyOrigin()
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .SetPreflightMaxAge(TimeSpan.FromDays(1));
            });
    });

builder.Services
    .AddDbContext<LeaderboardContext>(
        options => options.UseNpgsql(builder.Configuration.GetConnectionString("Leaderboard")))
    .AddAuthorization()
    .AddAuthentication(o =>
    {
        o.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        o.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        o.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(o =>
    {
        o.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateAudience = false,
            ValidateLifetime = false,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]
                    ?? throw new InvalidOperationException("Failed to get 'Jwt:Key'")))
        };
    });

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();
app.UseCors("AllowAll");
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<LeaderboardContext>();
    context.Database.Migrate();
}

app.MapGet("/score", [AllowAnonymous] (LeaderboardContext context, CancellationToken token) =>
{
    return context.ScoreEntries
        .OrderByDescending(p => p.Score)
        .ToListAsync(token);
})
.WithName("scores")
.WithOpenApi();

app.MapPost("/score", [Authorize] async (ScoreEntry scoreEntry, LeaderboardContext context, CancellationToken token) =>
{
    context.ScoreEntries.Add(scoreEntry);
    await context.SaveChangesAsync(token);
});

var tokenHolder = new JwtTokenHolder(builder);

app.MapPost("/token", [AllowAnonymous](User user) =>
{
    if (user.Username?.Equals(builder.Configuration["User:Username"]) is true && user.Password?.Equals(builder.Configuration["User:Password"]) is true)
    {
        return Results.Ok(tokenHolder.Token);
    }

    return Results.Unauthorized();
});

app.MapDelete("/score", [Authorize] async (string name, int score, LeaderboardContext context, CancellationToken token) =>
{
    var entry = await context.ScoreEntries.FirstOrDefaultAsync(p => p.Name == name && p.Score == score, token);
    if (entry is not null)
    {
        context.ScoreEntries.Remove(entry);
        await context.SaveChangesAsync(token);
        return Results.Ok();
    }

    return Results.Problem($"Failed to remove provided entry with name '{name}' and score '{score}'");
});

app.MapGet("/lottery", [AllowAnonymous] async (LeaderboardContext context, CancellationToken token) =>
    {
        var allResults = await LotteryEntries(context, token);
        var winner = Random.Shared.GetItems(allResults.ToArray(), 1);
        return winner;
    })
    .WithName("lottery")
    .WithOpenApi();


app.MapGet("/lottery/entries", LotteryEntries)
    .WithName("lottery-entries")
    .WithOpenApi();

app.MapGet("/throw", (string? text) =>
{
    throw new Exception("Testing exception thrown in endpoint: " + text);
});

app.Run();

async Task<List<ScoreEntry>> LotteryEntries(LeaderboardContext leaderboardContext, CancellationToken cancellationToken)
{
    var scoreEntries = await leaderboardContext.ScoreEntries
        // Exclude Sentry employees
        .Where(p => !p.Email.EndsWith("@sentry.io"))
        .OrderByDescending(s => s.Score)
        // Dedupe (1 entry per player)
        .GroupBy(s => s.Email)
        // Get the entry with highest score of each player
        .Select(g => g.OrderByDescending(p => p.Score).First())
        .ToListAsync(cancellationToken);

    scoreEntries = scoreEntries.OrderByDescending(s => s.Score).ToList();

    var credit = new List<ScoreEntry>();
    for (int i = 0; i < 10; i++)
    {
        var dupe = () => scoreEntries[i] with
        {
            Key = Guid.NewGuid()
        };
        if (i == 0) // 15 entries
        {
            credit.AddRange(Enumerable.Range(0, 14).Select(_ => dupe()));
        }
        if (i == 1) // 10 entries
        {
            credit.AddRange(Enumerable.Range(0, 9).Select(_ => dupe()));
        }
        if (i == 2) // 7 entries
        {
            credit.AddRange(Enumerable.Range(0, 6).Select(_ => dupe()));
        }
        if (i == 3) // 5 entries
        {
            credit.AddRange(Enumerable.Range(0, 4).Select(_ => dupe()));
        }
        if (i is > 3 and < 11) // 2 entries
        {
            credit.AddRange(Enumerable.Range(0, 1).Select(_ => dupe()));
        }
    }

    scoreEntries.AddRange(credit);
    return scoreEntries;
}
