using System.Net;
using Discord;
using Discord.Interactions;
using FuzzoBot.Utility;

namespace FuzzoBot.Modules;

/// <summary>
/// 
/// </summary>
public class ModdingModules : InteractionModuleBase
{
    /// <summary>
    /// 
    /// </summary>
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

    /// <summary>
    /// 
    /// </summary>
    /// <param name="moddingTool"></param>
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
    
    /// <summary>
    /// 
    /// </summary>
    /// <param name="message"></param>
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
}