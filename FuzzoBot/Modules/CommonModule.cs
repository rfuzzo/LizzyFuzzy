using System.Net;
using Discord;
using Discord.Interactions;
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

    /// <summary>
    /// 
    /// </summary>
    [SlashCommand("ping", "Checks the bot connection")]
    public async Task Ping()
    {
        await RespondAsync("pong");
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="emote"></param>
    [SlashCommand("emote", "Get emote url")]
    public async Task EchoEmote([Summary(description: "the custom emote")] string emote)
    {
        if (Emote.TryParse(emote, out var discordEmote)) await RespondAsync(discordEmote.Url);
    }
    
}