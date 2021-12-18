using Discord;

namespace FuzzoBot.Modules;

using Discord.Interactions;

public class CommonModule : InteractionModuleBase
{
    [SlashCommand("echo", "Repeat the input")]
    public async Task Echo(string echo, [Summary(description: "mention the user")]bool mention = false) => await RespondAsync(echo + (mention ? Context.User.Mention : string.Empty));

    [SlashCommand("ping", "Checks the bot connection")]
    public async Task Ping() => await RespondAsync("pong");
}