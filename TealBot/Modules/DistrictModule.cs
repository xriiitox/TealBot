using TealBot.Objects;

namespace TealBot.Modules;

[Group("district", "District-specific commands")]
public class DistrictModule : InteractionModuleBase<SocketInteractionContext>
{
    private readonly ILogger<DistrictModule> _logger;

    public DistrictModule(ILogger<DistrictModule> logger)
    {
        _logger = logger;
    }
    
    [SlashCommand("districts", "Returns a list of all FRC districts participating in the specified season.")]
    public async Task DistrictsCommand([Summary("Year", "The year/season you want the information from (default: current season)")] int year = 2024)
    {
        var response = CommandHelper.tbaClient.GetAsync($"api/v3/districts/{year}");

        EmbedBuilder embedBuilder;

        string stuffString = "";

        if (response.Result.IsSuccessStatusCode)
        {
            District[]? disArray = JsonConvert.DeserializeObject<District[]>(await response.Result.Content.ReadAsStringAsync());

            foreach (District d in disArray)
            {
                stuffString += $"{d.display_name}\nAbbreviation: {d.abbreviation}\n\n";
            }

            embedBuilder = new EmbedBuilder()
                .WithTitle($"**{year} Season Districts**")
                .WithDescription(stuffString)
                .WithColor(0, 0, 255)
                .WithCurrentTimestamp();
        }
        else
        {
            embedBuilder = new EmbedBuilder()
                .WithDescription($"Error: The Blue Alliance returned a failing status code.\n\n{response.Result.StatusCode}")
                .WithCurrentTimestamp();
        }

        await RespondAsync(embed: embedBuilder.Build());
    }
}