using System.IO.Compression;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Sentaur.Leaderboard.Web;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// Blazor built-in integration is in a Draft: https://github.com/getsentry/sentry-dotnet/pull/2569/
builder.Logging.AddSentry(o =>
{
    o.Dsn = "https://8f3ccd6a8a8e5ba417de8df962236a7d@o87286.ingest.us.sentry.io/4506888120107008";
    o.EnableTracing = true;

    // System.PlatformNotSupportedException: System.Diagnostics.Process is not supported on this platform.
    o.DetectStartupTime = StartupTimeDetectionMode.Fast;
    // Warning: No response compression supported by HttpClientHandler.
    o.RequestBodyCompressionLevel = CompressionLevel.NoCompression;
});

builder.Services.AddScoped(sp => new HttpClient(
    // Sentry tracing integration:
    new SentryHttpMessageHandler()) { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

await builder.Build().RunAsync();
