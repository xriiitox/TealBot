// Possible TODO: make teamevents have a selectmenu to select an event to view data of
// TODO: Statbotics.io api for 

using System.Net.Http.Headers;

namespace TealBot;

public class Commands : InteractionModuleBase<SocketInteractionContext>
{
    private readonly ILogger<Commands> _logger;

    private static HttpClient tbaClient;
    private static HttpClient statClient;

    public Commands(ILogger<Commands> logger, IConfiguration config)
    {
        _logger = logger;
        tbaClient = SetupClient(uri: "https://www.thebluealliance.com/", config["Secrets:TBA"]);
        statClient = SetupClient(uri: "https://api.statbotics.io/", config["Secrets:TBA"]);
    }

    private static HttpClient SetupClient(string uri, string TBA)
    {
        HttpClient client = new();
        client.BaseAddress = new Uri(uri);
        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        client.DefaultRequestHeaders.Add("X-TBA-Auth-Key", TBA);

        return client;
    }
    
    
    [SlashCommand("team", "Returns data about the requested team.")]
    public async Task TeamCommand([Summary("TeamNumber", "The team that you want data of.")] int teamID)
    {

        var response = tbaClient.GetAsync($"api/v3/team/frc{teamID}/simple");

        EmbedBuilder respondEmbed;

        if (response.Result.IsSuccessStatusCode)
        {
            Team? teamData = Team.DeserializeTeamJson(await response.Result.Content.ReadAsStringAsync());
            respondEmbed = new EmbedBuilder()
                .WithColor(new Color(0, 0, 255))
                .WithTitle($"**Team {teamData.team_number}**")
                .WithDescription($"{teamData.nickname}\n\n{teamData.name}\n\n{teamData.city}, {teamData.state_prov}, {teamData.country}")
                .WithCurrentTimestamp();
        }
        else
        {
            respondEmbed = new EmbedBuilder()
                .WithDescription($"Error: The Blue Alliance returned a failing status code.\n\n{response.Result.StatusCode}")
                .WithCurrentTimestamp();

        }
        
        

        await RespondAsync(embed: respondEmbed.Build());
    }

    [SlashCommand("teamrobots", "Returns data about the requested team's robots.")]
    public async Task TeamRobotsCommand([Summary("TeamNumber", "The team that you want data of.")] int teamID)
    {
        var response = tbaClient.GetAsync($"api/v3/team/frc{teamID}/robots");

        EmbedBuilder respondEmbed;
        
        if (response.Result.IsSuccessStatusCode)
        {
            string teamRobots = "";
            var listThing = JsonConvert.DeserializeObject<TeamRobot[]>(await response.Result.Content.ReadAsStringAsync());

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
                .WithDescription($"Error: The Blue Alliance returned a failing status code.\n\n{response.Result.StatusCode}")
                .WithCurrentTimestamp();

        }
        await RespondAsync(embed: respondEmbed.Build());
    }
    
    [SlashCommand("teamevents", "Returns data about all the requested team's events in the requested year.")]
    public async Task TeamEventCommand([Summary("TeamNumber", "The team that you want data of.")] int teamID, [Summary("Year", "The year/season you want the information from (default: current season)")] int year = 2024)
    {
        var response = tbaClient.GetAsync($"api/v3/team/frc{teamID}/events/{year}/simple");

        EmbedBuilder respondEmbed;

        if (response.Result.IsSuccessStatusCode)
        {
            string teamEventsData = "";
            var listThing = JsonConvert.DeserializeObject<Event[]>(await response.Result.Content.ReadAsStringAsync());

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
                .WithDescription($"Error: The Blue Alliance returned a failing status code.\n\n{response.Result.StatusCode}")
                .WithCurrentTimestamp();
            
            await RespondAsync(embed: respondEmbed.Build());
        }
        
        
    }

    [SlashCommand("status","Returns API Status")]
    public async Task StatusCommand()
    {

        var response = tbaClient.GetAsync($"api/v3/status");
        

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

    [SlashCommand("districts", "Returns a list of all FRC districts participating in the specified season.")]
    public async Task DistrictsCommand([Summary("Year", "The year/season you want the information from (default: current season)")] int year = 2024)
    {
        var response = tbaClient.GetAsync($"api/v3/districts/{year}");

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