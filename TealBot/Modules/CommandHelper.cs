// TODO: Statbotics.io api for match data

using System.Net.Http.Headers;
using TealBot.Objects;

namespace TealBot.Modules;

public class CommandHelper
{
    private readonly ILogger<CommandHelper> _logger;

    public static HttpClient tbaClient;
    public static HttpClient statClient;

    public CommandHelper(ILogger<CommandHelper> logger, IConfiguration config)
    {
        _logger = logger;
        tbaClient = SetupClient(uri: "https://www.thebluealliance.com/", config["Secrets:TBA"]);
        statClient = SetupClient(uri: "https://api.statbotics.io/");
    }

    private static HttpClient SetupClient(string uri, string TBA = "")
    {
        HttpClient client = new();
        client.BaseAddress = new Uri(uri);
        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        client.DefaultRequestHeaders.Add("X-TBA-Auth-Key", TBA);

        return client;
    }
    
    

    

    
}