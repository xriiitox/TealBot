using TealBot.Objects;

namespace TealBot.Modules;

[Group("district", "District-specific commands")]
public class DistrictModule(ILogger<DistrictModule> logger, IHttpClientFactory clientFactory, InteractionService interactionService) : InteractionModuleBase<SocketInteractionContext>
{
    private readonly ILogger<DistrictModule> _logger = logger;

    [SlashCommand("list", "Returns a list of all FRC districts participating in the specified season.")]
    public async Task DistrictsCommand([Summary("Year", "The year/season you want the information from (default: current season)")] int year = 2024)
    {
        var tbaClient = clientFactory.CreateClient("TBA");
        
        var response = await tbaClient.GetAsync($"api/v3/districts/{year}");

        EmbedBuilder embedBuilder;

        string stuffString = "";

        if (response.IsSuccessStatusCode)
        {
            District[]? disArray = JsonConvert.DeserializeObject<District[]>(await response.Content.ReadAsStringAsync());

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
                .WithDescription($"Error: The Blue Alliance returned a failing status code.\n\n{(int)response.StatusCode}")
                .WithCurrentTimestamp();
        }

        await RespondAsync(embed: embedBuilder.Build());
    }
    
    [SlashCommand("help", "District command specific help")]
    public async Task DistrictHelpCommand()
    {
        var commandString = "";
        var module =
            interactionService.Modules.FirstOrDefault(m => m.SlashGroupName.Equals("district"));

        EmbedBuilder embedBuilder;

        if (module != null)
        {
            var commands = module.SlashCommands;
            foreach (var command in commands)
            {
                var paramList = "";
                foreach (var parameter in command.Parameters)
                {
                    paramList += parameter.ParameterType.Name + " " + parameter.Name + " ";
                }

                commandString += "Command " + command.Name + (paramList != "" ? " with parameters " : "") + paramList + "\n" + command.Description + "\n\n";
            }
            embedBuilder = new EmbedBuilder()
                .WithTitle($"**Commands in module ``{module.SlashGroupName}``**")
                .WithDescription(commandString)
                .WithColor(0, 0, 255)
                .WithCurrentTimestamp();
            
            await RespondAsync(embed: embedBuilder.Build());
        }
        else
        {
            await RespondAsync(
                "Something has gone wrong, and the module cannot be found.\nPlease contact the bot's developer.");
        }
    }
}