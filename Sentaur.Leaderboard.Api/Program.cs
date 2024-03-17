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

app.MapGet("/throw", (string? text) =>
{
    throw new Exception("Testing exception thrown in endpoint: " + text);
});

app.Run();
