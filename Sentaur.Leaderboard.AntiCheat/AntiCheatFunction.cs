using System.Globalization;
using System.Text.Json;
using Google.Cloud.Functions.Framework;
using Google.Cloud.Functions.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

[assembly: FunctionsStartup(typeof(SentryStartup))]

namespace Sentaur.Leaderboard.AntiCheat;

public class Function(ILogger<Function> _logger) : IHttpFunction
{
    // private readonly ILogger _logger;
    private readonly HttpClient _client = new(new SentryHttpMessageHandler());

    public async Task HandleAsync(HttpContext context)
    {
        await using var stream = await _client.GetStreamAsync("https://live.sentry.io/stream");

        using var reader = new StreamReader(stream);
        await context.Response.WriteAsync("""{ "bugs": [""");
        for (var i = 0; i < 100; i++)
        {
            var line = await reader.ReadLineAsync();
            if (line is null) break;
            if (!line.StartsWith("data: ")) continue;
            var o = JsonSerializer.Deserialize<object[]>(line[6..]);
            var bug = (lat: double.Parse(o[0].ToString()), lon: double.Parse(o[1].ToString()!), platform: o[3].ToString());
            var length = Math.Sqrt(bug.lat * bug.lat + bug.lon * bug.lon);
            bug.lat /= length; bug.lon /= length;

            if (i > 0)
            {
                await context.Response.WriteAsync(",");
            }
            await context.Response.WriteAsync("{ \"lat\":");
            await context.Response.WriteAsync(Math.Round(bug.lat, 2).ToString(CultureInfo.InvariantCulture));
            await context.Response.WriteAsync(", \"lon\":");
            await context.Response.WriteAsync(Math.Round(bug.lon, 2).ToString(CultureInfo.InvariantCulture));
            await context.Response.WriteAsync(", \"platform\":\"");
            await context.Response.WriteAsync(bug.platform ?? "unknown");
            await context.Response.WriteAsync("\"}");
            _logger.LogInformation("Line: {line}", line);
        }
        await context.Response.WriteAsync("]}");
    }
}
