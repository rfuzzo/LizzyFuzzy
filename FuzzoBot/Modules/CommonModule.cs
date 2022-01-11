using System.Net;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using FuzzoBot.Utility;

namespace FuzzoBot.Modules;

/// <summary>
/// 
/// </summary>
public class CommonModule : InteractionModuleBase
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="user"></param>
    [UserCommand("Get user avatar")]
    public async Task GetAvatarUrl(IUser user)
    {
        await RespondAsync(user.GetAvatarUrl());
    }


    /// <summary>
    /// 
    /// </summary>
    /// <param name="topic"></param>
    [SlashCommand("poll", "Start a poll")]
    public async Task StartPoll([Summary(description: "The topic for the poll")] string topic)
    {
        await DeferAsync();
        //Console.WriteLine($"{RespondAsync("Poll").Result}");
        var emotes = new[]
        {
            new Emoji("👍"),
            new Emoji("👎")
        };
        var botUser = Context.Client.CurrentUser;
        var embed = new EmbedBuilder()
            .WithTitle("Poll")
            .WithColor(Color.Red)
            .WithFooter(footer => { footer.Text = botUser.Username; footer.IconUrl = botUser.GetAvatarUrl(); })
            .WithDescription(topic)
            .WithCurrentTimestamp();
        var pollMessage = await FollowupAsync(embed: embed.Build());
        await pollMessage.AddReactionsAsync(emotes);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="msg"></param>
    [MessageCommand("Get emote url")]
    public async Task GetCustomEmoteUrl(IMessage msg)
    {
        if (Emote.TryParse(msg.Content, out var discordEmote)) await RespondAsync(discordEmote.Url);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="message"></param>
    [MessageCommand("Add custom emote")]
    [RequireUserPermission(GuildPermission.ManageEmojisAndStickers)]
    public async Task AddCustomEmote(IMessage message)
    {
        if (Emote.TryParse(message.Content, out var emote))
        {
            using var client = new HttpClient();
            var response = await client.GetAsync(emote.Url);
            if (response.StatusCode == HttpStatusCode.OK)
            {
                await using var stream = await response.Content.ReadAsStreamAsync();
                var result = await Context.Guild.CreateEmoteAsync(emote.Name, new Image(stream));
                if (result is not null) await RespondAsync($"Added emote to server: {result}");
            }
        }
    }

    // /// <summary>
    // /// 
    // /// </summary>
    // [SlashCommand("ping", "Checks the bot connection")]
    // public async Task Ping()
    // {
    //     await RespondAsync("pong");
    // }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="emote"></param>
    [SlashCommand("emote", "Get emote url")]
    public async Task EchoEmote([Summary(description: "the custom emote")] string emote)
    {
        if (Emote.TryParse(emote, out var discordEmote)) await RespondAsync(discordEmote.Url);
    }
    
    
    
    
    
    /// <summary>
    /// 
    /// </summary>
    [ComponentInteraction("ping-*")]
    public async Task ComponentInteractionPing(string userLabel)
    {
        switch (userLabel)
        {
            case "owner":
                await RespondAsync($"Pinging {Constants.rfuzzo} for review. {Constants.Emotes.smile}");
                break;
        }
    }
}
