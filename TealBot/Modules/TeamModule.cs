// Possible TODO: make teamevents have a selectmenu to select an event to view data of
// TODO: Statbotics.io api for match data
using TealBot.Objects;

namespace TealBot.Modules;

[Group("team", "Team-specific commands")]
public class TeamModule(ILogger<TeamModule> logger, IHttpClientFactory clientFactory, InteractionService interactionService) : InteractionModuleBase<SocketInteractionContext>
{
    private readonly ILogger<TeamModule> _logger = logger;

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
            string teamEventsData = "";
            var listThing = JsonConvert.DeserializeObject<Event[]>(await response.Content.ReadAsStringAsync());

            foreach (Event e in listThing)
            {
                teamEventsData += $"Key: {e.key}\nName: {e.name}\nDate: {e.start_date} to {e.end_date}\n\n";
            }

            respondEmbed = new EmbedBuilder()
                .WithColor(0, 0, 255)
                .WithTitle($"**Events for Team {teamId} in the {year} season**")
                .WithDescription(teamEventsData)
                .WithCurrentTimestamp();

            await RespondAsync(embed: respondEmbed.Build());
        }
        else
        {
            respondEmbed = new EmbedBuilder()
                .WithDescription($"Error: The Blue Alliance returned a failing status code.\n\n{(int)response.StatusCode}")
                .WithCurrentTimestamp();
            
            await RespondAsync(embed: respondEmbed.Build());
        }
        
        
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