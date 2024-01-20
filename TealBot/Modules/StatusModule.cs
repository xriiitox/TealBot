namespace TealBot.Modules;

[Group("status", "Get API Status")]
public class StatusModule(ILogger<StatusModule> logger, IHttpClientFactory clientFactory) : InteractionModuleBase<SocketInteractionContext>
{
    private readonly ILogger<StatusModule> _logger = logger;

    [SlashCommand("bluealliance","Returns The Blue Alliance's API Status")]
    public async Task BlueAllianceStatusCommand()
    {
        var tbaClient = clientFactory.CreateClient("TBA");

        var response = await tbaClient.GetAsync($"api/v3/status");

        EmbedBuilder embedBuilder;

        if (response.IsSuccessStatusCode)
        {
            dynamic? data = JsonConvert.DeserializeObject(await response.Content.ReadAsStringAsync());

            embedBuilder = new EmbedBuilder()
                .WithTitle($"**Success {(int)response.StatusCode}**")
                .WithDescription("The Blue Alliance's API appears to be up and running.\n\nIs datafeed down? " + ((bool)data.is_datafeed_down ? "yes" : "no")
                    + "\n\n" + $"Current Season: {(string)data.current_season}")
                .WithColor(0,0,255)
                .WithCurrentTimestamp();
        }
        else
        {
            embedBuilder = new EmbedBuilder()
                .WithTitle("**Error**")
                .WithDescription($"Code {(int)response.StatusCode}")
                .WithCurrentTimestamp();
        }

        await RespondAsync(embed: embedBuilder.Build());
    }

    [SlashCommand("statbotics", "Returns Statbotics' API status")]
    public async Task StatboticsStatusCommand()
    {
        var statClient = clientFactory.CreateClient("statbotics");
        
        var response = await statClient.GetAsync("v2");

        EmbedBuilder responseEmbed;

        if (response.IsSuccessStatusCode)
        {
            responseEmbed = new EmbedBuilder()
                .WithTitle($"**Success {(int)response.StatusCode}**")
                .WithDescription("The Statbotics API appears to be up and running.")
                .WithColor(0, 0, 255)
                .WithCurrentTimestamp();
        }
        else
        {
            responseEmbed = new EmbedBuilder()
                .WithTitle($"**Error {(int)response.StatusCode}**")
                .WithDescription("Statbotics returned a failing status code.")
                .WithCurrentTimestamp();
        }

        await RespondAsync(embed: responseEmbed.Build());
    }
}