global using Discord;
global using Discord.Interactions;
global using Discord.WebSocket;

global using Microsoft.Extensions.Configuration;
global using Microsoft.Extensions.Logging;

global using Newtonsoft.Json;

using TealBot.Services;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using System.Net.Http.Headers;

using Serilog;
using TealBot;


var builder = new HostApplicationBuilder(args);

builder.Configuration.AddEnvironmentVariables("TealBot_");

var loggerConfig = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File($"logs/log-{DateTime.Now:yy.MM.dd_HH.mm}.log")
    .CreateLogger();

builder.Logging.ClearProviders();
builder.Logging.AddSerilog(loggerConfig, dispose: true);

builder.Services.AddSingleton(new DiscordSocketClient(
    new DiscordSocketConfig
    {
        GatewayIntents = GatewayIntents.AllUnprivileged,
        FormatUsersInBidirectionalUnicode = false,
        // Add GatewayIntents.GuildMembers to the GatewayIntents and change this to true if you want to download all users on startup
        AlwaysDownloadUsers = false,
        LogGatewayIntentWarnings = false,
        LogLevel = LogSeverity.Info
    }));

builder.Services.AddSingleton(x => new InteractionService(x.GetRequiredService<DiscordSocketClient>(), new InteractionServiceConfig()
{
    LogLevel = LogSeverity.Info
}));

builder.Services.AddSingleton<InteractionHandler>();

builder.Services.AddHostedService<DiscordBotService>();

builder.Services.AddHttpClient("TBA", c =>
{
    c.BaseAddress = new Uri("https://www.thebluealliance.com/");
    c.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
    c.DefaultRequestHeaders.Add("X-TBA-Auth-Key", builder.Configuration["Secrets:TBA"]);
});

builder.Services.AddHttpClient("statbotics", c =>
{
    c.BaseAddress = new Uri("https://api.statbotics.io/");
    c.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
});

var app = builder.Build();

await app.RunAsync();