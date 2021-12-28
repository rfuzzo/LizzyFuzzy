using System.Net;
using Discord;
using Discord.Interactions;
using FuzzoBot.Utility;

namespace FuzzoBot.Modules;

public class CommonModule : InteractionModuleBase
{
    
    [UserCommand("Get user avatar")]
    public async Task GetAvatarUrl(IUser user)
    {
        await RespondAsync(user.GetAvatarUrl());
    }
    
    
    
    
    [MessageCommand("Get emote url")]
    public async Task GetCustomEmoteUrl(IMessage msg)
    {
        if (Emote.TryParse(msg.Content, out var discordEmote)) await RespondAsync(discordEmote.Url);
    }

    
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
                    await RespondAsync($"Added emote to server: {result}");
                }
            }
        }
    }
    
    [MessageCommand("Check DDS Format")]
    public async Task CheckDdsFormat(IMessage message)
    {
        var attachments = message.Attachments.Where(x => Path.GetExtension(x.Filename).Equals(".dds"));
        foreach (var attachment in attachments)
        {
            using var client = new HttpClient();
            var response = await client.GetAsync(attachment.Url);
            if (response.StatusCode == HttpStatusCode.OK)
            {
                await using var stream = await response.Content.ReadAsStreamAsync();
                using var br = new BinaryReader(stream);
                
                if (stream.Length < 148)
                {
                    await RespondAsync($"{attachment.Filename} - that's not a dds file...");
                    return;
                }
                if (br.ReadInt32() != 0x20534444)
                {
                    await RespondAsync($"{attachment.Filename} - that's not a dds file...");
                    return;
                }
                // If the DDS_PIXELFORMAT dwFlags is set to DDPF_FOURCC and dwFourCC is set to "DX10" an additional DDS_HEADER_DXT10 structure will be present
                br.ReadBytes(80);
                if (br.ReadInt32() != 0x30315844)
                {
                    await RespondAsync($"{attachment.Filename} - that's not a dx10 dds file...");
                    return;
                }
                br.ReadBytes(40);
                var fmt = (DXGI_FORMAT)br.ReadInt32();
                
                var embed = new EmbedBuilder()
                    .WithTitle(attachment.Filename)
                    .WithColor(Color.Green)
                    .WithDescription($"➡ DDS format: `{fmt.ToString()}`")
                    //.WithThumbnailUrl()
                    .WithCurrentTimestamp();
                await RespondAsync(embed: embed.Build());
            }
        }
    }
    
    
    
    
    [SlashCommand("ping", "Checks the bot connection")]
    public async Task Ping() => await RespondAsync("pong");

    [SlashCommand("emote", "Get emote url")]
    public async Task EchoEmote([Discord.Interactions.Summary(description: "the custom emote")] string emote)
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