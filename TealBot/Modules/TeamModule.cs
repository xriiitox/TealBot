// Possible TODO: make teamevents have a selectmenu to select an event to view data of
// TODO: Statbotics.io api for match data
using TealBot.Objects;

namespace TealBot.Modules;

[Group("team", "Team-specific commands")]
public class TeamModule : InteractionModuleBase<SocketInteractionContext>
{
    private readonly ILogger<TeamModule> _logger;
    private readonly IHttpClientFactory _clientFactory;

    public TeamModule(ILogger<TeamModule> logger, IHttpClientFactory clientFactory)
    {
        _logger = logger;
        _clientFactory = clientFactory;
    }
    
    [SlashCommand("get", "Returns data about the requested team.")]
    public async Task TeamCommand([Summary("TeamNumber", "The team that you want data of.")] int teamID)
    {
        HttpClient tbaClient = _clientFactory.CreateClient("TBA");

        var response = await tbaClient.GetAsync($"api/v3/team/frc{teamID}/simple");

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
    public async Task TeamRobotsCommand([Summary("TeamNumber", "The team that you want data of.")] int teamID)
    {
        var tbaClient = _clientFactory.CreateClient("TBA");
        
        var response = await tbaClient.GetAsync($"api/v3/team/frc{teamID}/robots");

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
                .WithTitle($"**Team {teamID}**")
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
    
    [SlashCommand("events", "Returns data about all the requested team's events in the requested year.")]
    public async Task TeamEventCommand([Summary("TeamNumber", "The team that you want data of.")] int teamID, [Summary("Year", "The year/season you want the information from (default: current season)")] int year = 2024)
    {
        var tbaClient = _clientFactory.CreateClient("TBA");
        
        var response = await tbaClient.GetAsync($"api/v3/team/frc{teamID}/events/{year}/simple");

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
                .WithTitle($"**Events for Team {teamID} in the {year} season**")
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
}