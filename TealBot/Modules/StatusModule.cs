namespace TealBot.Modules;

[Group("status", "Get API Status")]
public class StatusModule : InteractionModuleBase<SocketInteractionContext>
{
    private readonly ILogger<StatusModule> _logger;
    private readonly IHttpClientFactory _clientFactory;

    public StatusModule(ILogger<StatusModule> logger, IHttpClientFactory clientFactory)
    {
        _logger = logger;
        _clientFactory = clientFactory;
    }
    
    [SlashCommand("bluealliance","Returns The Blue Alliance's API Status")]
    public async Task BlueAllianceStatusCommand()
    {
        var tbaClient = _clientFactory.CreateClient("TBA");

        var response = await tbaClient.GetAsync($"api/v3/status");

        EmbedBuilder embedBuilder;

        if (response.IsSuccessStatusCode)
        {
            dynamic? data = JsonConvert.DeserializeObject(await response.Content.ReadAsStringAsync());

            embedBuilder = new EmbedBuilder()
                .WithTitle($"**{(int)response.StatusCode} OK**")
                .WithDescription("Is datafeed down? " + ((bool)data.is_datafeed_down ? "yes" : "no")
                                                      + "\n\n" + $"Current Season: {(string)data.current_season}")
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
        var statClient = _clientFactory.CreateClient("statbotics");
        
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