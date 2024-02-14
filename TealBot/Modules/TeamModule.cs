// TODO: Statbotics.io api for match data

using System.Threading.Channels;
using TealBot.Objects;

namespace TealBot.Modules;

[Group("team", "Team-specific commands")]
public class TeamModule(ILogger<TeamModule> logger, IHttpClientFactory clientFactory, InteractionService interactionService) : InteractionModuleBase<SocketInteractionContext>
{
    private readonly ILogger<TeamModule> _logger = logger;

    private static Dictionary<string, Embed> embedList = new();

    [SlashCommand("get", "Retrieves data about the requested team.")]
    public async Task TeamCommand([Summary("TeamNumber", "The team that you want data of.")] int teamId)
    {
        HttpClient tbaClient = clientFactory.CreateClient("TBA");

        var response = await tbaClient.GetAsync($"api/v3/team/frc{teamId}/simple");

        EmbedBuilder respondEmbed;

        if (response.IsSuccessStatusCode)
        {
            Team? teamData = Team.DeserializeTeamJson(await response.Content.ReadAsStringAsync());
            respondEmbed = new EmbedBuilder()
                .WithColor(new Color(0, 0, 255))
                .WithTitle($"**Team {teamData.team_number}**")
                .WithDescription(
                    $"{teamData.nickname}\n\n{teamData.name}\n\n{teamData.city}, {teamData.state_prov}, {teamData.country}")
                .WithCurrentTimestamp();
        }
        else
        {
            respondEmbed = new EmbedBuilder()
                .WithDescription(
                    $"Error: The Blue Alliance returned a failing status code.\n\n{(int)response.StatusCode}")
                .WithCurrentTimestamp();

        }
        await RespondAsync(embed: respondEmbed.Build());
    }
    
    [SlashCommand("robots", "Returns data about the requested team's robots.")]
    public async Task TeamRobotsCommand([Summary("TeamNumber", "The team that you want data of.")] int teamId)
    {
        var tbaClient = clientFactory.CreateClient("TBA");
        
        var response = await tbaClient.GetAsync($"api/v3/team/frc{teamId}/robots");

        EmbedBuilder respondEmbed;
        
        if (response.IsSuccessStatusCode)
        {
            string teamRobots = "";
            var listThing = JsonConvert.DeserializeObject<TeamRobot[]>(await response.Content.ReadAsStringAsync());

            foreach (TeamRobot robot in listThing)
            {
                teamRobots += robot.year + ": " + robot.robot_name + "\n";
            }
                
            respondEmbed = new EmbedBuilder()
                .WithColor(0, 0, 255)
                .WithTitle($"**Team {teamId}**")
                .WithDescription(teamRobots)
                .WithCurrentTimestamp();
        }
        else
        {
            respondEmbed = new EmbedBuilder()
                .WithDescription($"Error: The Blue Alliance returned a failing status code.\n\n{(int)response.StatusCode}")
                .WithCurrentTimestamp();

        }
        await RespondAsync(embed: respondEmbed.Build());
    }
    
    [SlashCommand("events", "Returns data about the requested team's events in the requested year.")]
    public async Task TeamEventCommand([Summary("TeamNumber", "The team that you want data of.")] int teamId, [Summary("Year", "The year/season you want the information from (default: current season)")] int year = 2024)
    {
        var tbaClient = clientFactory.CreateClient("TBA");
        
        var response = await tbaClient.GetAsync($"api/v3/team/frc{teamId}/events/{year}/simple");

        EmbedBuilder respondEmbed;

        if (response.IsSuccessStatusCode)
        {
            var eventList = JsonConvert.DeserializeObject<Event[]>(await response.Content.ReadAsStringAsync());

            var menuBuilder = new SelectMenuBuilder()
                .WithPlaceholder("Select an event...")
                .WithCustomId("team-events-menu")
                .WithMinValues(1)
                .WithMaxValues(1);
            
            foreach (Event e in eventList)
            {
                menuBuilder.AddOption(e.name, e.key);
            }

            var builder = new ComponentBuilder()
                .WithSelectMenu(menuBuilder);

            await RespondAsync($"**Events for team {teamId} in {year}:**", components: builder.Build());
        }
        else
        {
            respondEmbed = new EmbedBuilder()
                .WithDescription($"Error: The Blue Alliance returned a failing status code.\n\n{(int)response.StatusCode}")
                .WithCurrentTimestamp();
            
            await RespondAsync(embed: respondEmbed.Build());
        }
        
        
    }

    [ComponentInteraction("team-events-menu", ignoreGroupNames: true)]
    public async Task EventsMenu(string id, string[] selectedEvents)
    {
        await Context.Interaction.DeferAsync(true);
        
        var client = clientFactory.CreateClient("TBA");

        var response = await client.GetAsync($"api/v3/event/{selectedEvents[0]}/matches/simple");
        
        var matchList = JsonConvert.DeserializeObject<dynamic[]>(await response.Content.ReadAsStringAsync());
        
        var menuBuilder = new SelectMenuBuilder()
            .WithPlaceholder("Select a match...")
            .WithCustomId("match-select-menu")
            .WithMinValues(1)
            .WithMaxValues(1);

        foreach (dynamic match in matchList)
        {
            if (menuBuilder.Options.Count == 25)
            {
                break;
            } 
            var name = "";
            switch ((string)match["comp_level"])
            {
                case "qm":
                    name += "Qualifier Match " + match["match_number"] + (match["set_number"] > 1 ? "Set " + match["set_number"] : "");
                    break;
                case "sf":
                    name += "Semifinals Match " + match["match_number"] + (match["set_number"] > 1 ? "Set " + match["set_number"] : "");
                    break;
                case "f":
                    name += "Finals Match " + match["match_number"] + (match["set_number"] > 1 ? "Set " + match["set_number"] : "");
                    break;
            }

            menuBuilder.AddOption(name, (string)match["key"]);
        }

        var builder = new ComponentBuilder()
            .WithSelectMenu(menuBuilder);

        var interaction = await Context.Interaction.GetOriginalResponseAsync();
        
        await interaction.ModifyAsync(x =>
        {
            x.Embed = null; 
            x.Components = builder.Build(); // add new select menu
            x.Content = "Which Match?"; 
        });
    }

    [ComponentInteraction("match-select-menu", ignoreGroupNames: true)]
    public async Task MatchSelectMenu(string id, string[] selectedMatch)
    {
        await Context.Interaction.DeferAsync(true);
        
        var client = clientFactory.CreateClient("TBA");

        var response = await client.GetAsync($"api/v3/match/{selectedMatch[0]}/simple");

        dynamic match = JsonConvert.DeserializeObject(await response.Content.ReadAsStringAsync());

        var embedBuilder = new EmbedBuilder()
            .WithColor(0, 0, 255)
            .WithTitle($"**{id} at event {(string)match["event_key"]}**")
            .WithCurrentTimestamp();
        var descString = "Teams on the blue alliance: \n";
        // good lord this is ugly
        foreach (string alliance in new []{"blue", "red"})
        {
            foreach (string team in match["alliances"][alliance]["team_keys"])
            {
                descString += "Team " + string.Concat(team.Where(Char.IsDigit)) + "\n";
            }

            descString += alliance.Equals("blue") ? "\nTeams on the red alliance: \n" : "";
        }

        descString += $"\nBlue Points: {(string)match["alliances"]["blue"]["score"]}";
        descString += $"\nRed Points: {(string)match["alliances"]["red"]["score"]}\n";
        descString += $"\nWinner: {(string)match["winning_alliance"]}";

        embedBuilder.WithDescription(descString);
        
        var interaction = await Context.Interaction.GetOriginalResponseAsync();
        
        await interaction.ModifyAsync(x =>
        {
            x.Embed = embedBuilder.Build(); // create embed with match data
            x.Components = null;
            x.Content = null; 
        });

    }

    [SlashCommand("help", "Team command specific help")]
    public async Task TeamHelpCommand()
    {
        var commandString = "";
        var module =
            interactionService.Modules.FirstOrDefault(m => m.SlashGroupName.Equals("team"));

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