using System.Collections.Immutable;
using System.Net.Http.Headers;
using System.Text;
using System.Text.RegularExpressions;

namespace TealBot.Modules;

public class RuleModule(ILogger<RuleModule> logger, IHttpClientFactory clientFactory) : InteractionModuleBase<SocketInteractionContext>
{
    private static Dictionary<string, Embed> embedList = new();
    
    
    // i basically stole most of the code for this command from the FRC Discord's Dozer bot. 
    [SlashCommand("rule", "Displays the text of a given rule number or description.", ignoreGroupNames: true)]
    public async Task RuleCommand([Summary("text", "Rule number or description of rule")] string text)
    {
        var rulesClient = new HttpClient();
        rulesClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        
        var regexMatch = Regex.Match(text, @"^(?<letter>[a-zA-Z])(?<number>\d{3})$");

        EmbedBuilder embedBuilder;

        if (regexMatch.Success)
        {
            var letter = regexMatch.Groups["letter"].Value;
            var number = regexMatch.Groups["number"].Value;
            var year = DateTime.Now.Year;
            
            var response = await rulesClient.GetAsync($"https://frctools.com/api/rule?query={letter}{number}");

            if (response.IsSuccessStatusCode)
            {
                dynamic rule = JsonConvert.DeserializeObject(await response.Content.ReadAsStringAsync());
                if (rule["error"] == null)
                {
                    embedBuilder = new EmbedBuilder()
                        .WithUrl($"https://frctools.com/{year}/rule/{letter.ToUpper()}{number}")
                        .WithTitle("Rule " + letter.ToUpper() + number)
                        .WithColor(0, 0, 255)
                        .WithDescription("```" + ((string)rule["textContent"]).Replace("\n", " ") + "```")
                        .WithCurrentTimestamp();

                    await RespondAsync(embed: embedBuilder.Build());
                }
                else
                {
                    embedBuilder = new EmbedBuilder()
                        .WithTitle("**Error**")
                        .WithDescription("No Rule Found")
                        .WithColor(255, 0, 0)
                        .WithCurrentTimestamp();

                    await RespondAsync(embed: embedBuilder.Build());
                }
            }
            else
            {
                embedBuilder = new EmbedBuilder()
                    .WithDescription($"Error: FRC Tools returned a failing status code.\n\n{(int)response.StatusCode}")
                    .WithCurrentTimestamp();
            
                await RespondAsync(embed: embedBuilder.Build());
            }
        }
        else
        {
            HttpContent query = new StringContent($"{{\"query\": \"{text}\"}}", Encoding.UTF8, "application/json");
            var year = DateTime.Now.Year;
            var response = await rulesClient.PostAsync("https://search.grahamsh.com/search", query);

            if (response.IsSuccessStatusCode)
            {
                dynamic rules = JsonConvert.DeserializeObject(await response.Content.ReadAsStringAsync());
                embedList = new();
                
                foreach (var rule in rules["data"])
                {
                    embedList.Add("rule-" + (string)rule["text"], new EmbedBuilder()
                        .WithUrl($"https://frctools.com/{year}/rule/{rule["text"]}")
                        .WithTitle("Rule " + (string)rule["text"])
                        .WithDescription("```" + ((string)rule["textContent"]).Replace("\n"," ") + "```")
                        .WithColor(0, 0, 255)
                        .WithCurrentTimestamp()
                        .Build());
                }

                var menuBuilder = new SelectMenuBuilder()
                    .WithPlaceholder("Select a rule...")
                    .WithCustomId("rule-menu")
                    .WithMinValues(1)
                    .WithMaxValues(1);
                foreach (var embed in embedList)
                {
                    menuBuilder.AddOption(embed.Key, embed.Key);
                }

                var builder = new ComponentBuilder()
                    .WithSelectMenu(menuBuilder);

                

                await RespondAsync("Which rule would you like?", components: builder.Build());

            }
            else
            {
                embedBuilder = new EmbedBuilder()
                    .WithDescription($"Error: GrahamSH Search returned a failing status code.\n\n{(int)response.StatusCode}")
                    .WithCurrentTimestamp();
            
                await RespondAsync(embed: embedBuilder.Build());
            }
        }
    }

    [ComponentInteraction("rule-menu")]
    public async Task RuleMenu(string id, string[] selectedRules)
    {
        await Context.Interaction.DeferAsync(true);

        var interaction = await Context.Interaction.GetOriginalResponseAsync();
        
        await interaction.ModifyAsync(x =>
        {
            x.Embed = embedList[selectedRules[0]]; // get selected rule from menu as message content
            x.Components = null; // remove select menu
            x.Content = null; // remove text from message
        });
    }
}