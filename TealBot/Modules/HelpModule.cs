namespace TealBot.Modules;

[Group("help", "Help commands")]
public class HelpModule(ILogger<HelpModule> logger) : InteractionModuleBase<SocketInteractionContext>
{
    private readonly ILogger _logger = logger;

    [SlashCommand("default", "General command help")]
    public async Task HelpCommand()
    {
        await RespondAsync("I have not implemented this yet sorry");
    }
}