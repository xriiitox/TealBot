
namespace TealBot.Modules;

public class HelpModule(ILogger<HelpModule> logger, InteractionService interactions) : InteractionModuleBase<SocketInteractionContext>
{
    private readonly ILogger<HelpModule> _logger = logger;

    [SlashCommand("help", "General command help", ignoreGroupNames: true)]
    public async Task HelpCommand()
    {
        var modules = "";

        foreach (var module in interactions.Modules)
        {
            if (module.SlashGroupName != null)
            {
                modules += module.SlashGroupName + "\n";
            }
        }

        modules += "rules";
        
        await RespondAsync(embed: new EmbedBuilder()
            .WithTitle("**General Command Help**")
            .WithDescription($"Use the command /{{module}} help for specific help within modules.\n\nList of modules:\n{modules}")
            .WithColor(0,0,255)
            .WithCurrentTimestamp()
            .Build());
    }
}