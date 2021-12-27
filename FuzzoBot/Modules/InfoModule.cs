using Discord;
using Discord.Interactions;
using FuzzoBot.Utility;

namespace FuzzoBot.Modules;

public class CommonModule : InteractionModuleBase
{
    [SlashCommand("emote", "Get emote url")]
    public async Task EchoEmote([Summary(description: "the custom emote")] string emote)
    {
        if (Emote.TryParse(emote, out var discordEmote)) await RespondAsync(discordEmote.Url);
    }

    [SlashCommand("wiki", "Get wiki info")]
    public async Task Wiki()
    {
        var embed = new EmbedBuilder()
            .WithTitle("The Red Modding Wiki")
            .WithColor(Color.Red)
            .WithAuthor(Context.Client.CurrentUser)
            .WithUrl(Constants.Wiki.Url)
            .WithDescription($"Welcome to the modding community wiki at {Constants.Wiki.Url}.\n" +
                             "Please follow the below link to gain editor access:")
            //.WithThumbnailUrl()
            .WithCurrentTimestamp();

        embed.AddField(@"🌐 Invite link", Constants.Wiki.EditorInvite);

        await RespondAsync(embed: embed.Build());
    }

    [UserCommand("Avatar")]
    public async Task GetAvatarUrl(IUser user)
    {
        await RespondAsync(user.GetAvatarUrl());
    }

    [SlashCommand("ping", "Checks the bot connection")]
    public async Task Ping()
    {
        await RespondAsync("pong");
    }

    [SlashCommand("info", "Send info on the selected modding tool")]
    public async Task InfoCommand(ModdingTool moddingTool)
    {
        var toolsDict = await ResourceUtil.GetModToolsAsync();

        // try get 
        if (toolsDict.TryGetValue(moddingTool.ToString(), out var tool))
        {
            var color = Color.Red;
            var user = await Context.Client.GetUserAsync(tool.Author) ?? Context.Client.CurrentUser;
            var thumbnail = !string.IsNullOrEmpty(tool.ThumbnailUrl) ? tool.ThumbnailUrl : user.GetAvatarUrl();

            var embed = new EmbedBuilder()
                .WithTitle(tool.Title)
                .WithColor(color)
                .WithAuthor(user)
                .WithUrl(tool.Url)
                .WithDescription(tool.Description)
                .WithThumbnailUrl(thumbnail)
                .WithCurrentTimestamp();
            embed.AddField(@"🌐 Url", tool.Url);
            embed.AddField(@"❓ Wiki", tool.Wiki);
            foreach (var (title, value) in tool.Fields) embed.AddField(title, value);
            await RespondAsync(embed: embed.Build());
        }
    }
}