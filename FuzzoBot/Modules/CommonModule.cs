using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Discord;
using Discord.Interactions;

namespace FuzzoBot.Modules;

/// <summary>
/// </summary>
public class CommonModule : InteractionModuleBase
{
    /// <summary>
    /// </summary>
    /// <param name="user"></param>
    [UserCommand("Get user avatar")]
    public async Task GetAvatarUrl(IUser user)
    {
        await DeferAsync();
        await FollowupAsync(user.GetAvatarUrl());
    }


    /// <summary>
    /// </summary>
    /// <param name="topic"></param>
    [SlashCommand("poll", "Start a poll")]
    public async Task StartPoll([Summary(description: "The topic for the poll")] string topic)
    {
        await DeferAsync();
        var emotes = new[]
        {
            new Emoji("👍"),
            new Emoji("👎")
        };
        var botUser = Context.Client.CurrentUser;
        var embed = new EmbedBuilder()
            .WithTitle("Poll")
            .WithColor(Color.Red)
            .WithFooter(footer =>
            {
                footer.Text = botUser.Username;
                footer.IconUrl = botUser.GetAvatarUrl();
            })
            .WithDescription(topic)
            .WithCurrentTimestamp();
        var pollMessage = await FollowupAsync(embed: embed.Build());
        await pollMessage.AddReactionsAsync(emotes);
    }

    /// <summary>
    /// </summary>
    /// <param name="message"></param>
    [MessageCommand("Get custom emote url")]
    public async Task GetCustomEmoteUrl(IMessage message)
    {
        if (Emote.TryParse(message.Content, out var discordEmote))
        {
            await DeferAsync();
            await FollowupAsync(discordEmote.Url);
        }
        else
        {
            await DeferAsync(true);
            await FollowupAsync(ephemeral: true, text: "Unable to parse the emote.");
        }
    }

    /// <summary>
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
                if (result is not null)
                {
                    await DeferAsync();
                    await FollowupAsync($"Added emote to server: {result}");
                }
                else
                {
                    await DeferAsync(true);
                    await FollowupAsync(ephemeral: true, text: "Unable to create the new emote.");
                }
            }
        }
    }

    /// <summary>
    /// </summary>
    [SlashCommand("ping", "Checks the bot connection")]
    public async Task Ping()
    {
        await RespondAsync("pong");
    }

    
}