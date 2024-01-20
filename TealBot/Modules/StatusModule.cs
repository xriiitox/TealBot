namespace TealBot.Modules;

[Group("status", "Get API Status")]
public class StatusModule : InteractionModuleBase<SocketInteractionContext>
{
    private readonly ILogger<StatusModule> _logger;

    public StatusModule(ILogger<StatusModule> logger)
    {
        _logger = logger;
    }
    
    [SlashCommand("status","Returns API Status")]
    public async Task StatusCommand()
    {

        var response = CommandHelper.tbaClient.GetAsync($"api/v3/status");

        EmbedBuilder embedBuilder;

        if (response.Result.IsSuccessStatusCode)
        {
            dynamic? data = JsonConvert.DeserializeObject(await response.Result.Content.ReadAsStringAsync());

            embedBuilder = new EmbedBuilder()
                .WithTitle($"**{(int)response.Result.StatusCode} OK**")
                .WithDescription("Is datafeed down? " + ((bool)data.is_datafeed_down ? "yes" : "no")
                                                      + "\n\n" + $"Current Season: {(string)data.current_season}")
                .WithCurrentTimestamp();
        }
        else
        {
            embedBuilder = new EmbedBuilder()
                .WithTitle("**Error**")
                .WithDescription($"Code {(int)response.Result.StatusCode}")
                .WithCurrentTimestamp();
        }

        await RespondAsync(embed: embedBuilder.Build());
    }
}