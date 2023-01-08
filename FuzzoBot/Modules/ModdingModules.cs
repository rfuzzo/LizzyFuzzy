using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Discord;
using Discord.Interactions;
using FuzzoBot.Extensions;
using FuzzoBot.Utility;

namespace FuzzoBot.Modules;

/// <summary>
/// </summary>
public class ModdingModules : InteractionModuleBase
{
    

    /// <summary>
    /// </summary>
    [SlashCommand("registermod", "Register a mod.")]
    public async Task RegisterMod(string modName, int nexusId)
    {
        var dict = await ResourceUtil.LoadModsDictAsync();

        // check if tag already in dict
        if (dict.ContainsKey(modName))
        {
            await DeferAsync(true);
            await FollowupAsync(ephemeral: true, text: $"Mod \"{modName}\" already registered.");
            return;
        }

        foreach (var (key, value) in dict)
            if (value == nexusId)
            {
                await DeferAsync(true);
                await FollowupAsync(ephemeral: true, text: $"Mod already registered with tag \"{key}\".");
                return;
            }

        // check response
        var nexusUrl = $"https://www.nexusmods.com/cyberpunk2077/mods/{nexusId}";
        HttpClient client = new();
        var response = await client.GetAsync(new Uri(nexusUrl));

        try
        {
            response.EnsureSuccessStatusCode();
        }
        catch (HttpRequestException ex)
        {
            await DeferAsync(true);
            await FollowupAsync(ephemeral: true, text: $"No mod with id {nexusId} exists on nexus.");

            LoggingProvider.Log(ex);
            return;
        }

        dict.Add(modName, nexusId);
        ResourceUtil.SaveModsDict(dict);

        await DeferAsync();
        await FollowupAsync(ephemeral: false, text: $"Mod registered with tag \"{modName}\".");
    }
    
    /// <summary>
    /// </summary>
    [SlashCommand("mod", "Get the Nexus link to a registered mod.")]
    public async Task Mod(string modName)
    {
        var dict = await ResourceUtil.LoadModsDictAsync();

        // check if tag already in dict
        if (dict.ContainsKey(modName))
        {
            var nexusUrl = $"https://www.nexusmods.com/cyberpunk2077/mods/{dict[modName]}";
            await DeferAsync();
            await FollowupAsync(ephemeral: false, text: $"[{modName}]\n{nexusUrl}");
        }
        else
        {
            await DeferAsync(true);
            await FollowupAsync(ephemeral: true, text: $"No mod with tag \"{modName}\" registered.");
        }
    }

    /// <summary>
    /// </summary>
    [SlashCommand("deletemod", "Delete a registered mod.")]
    public async Task Deletemod(string modName)
    {
        var dict = await ResourceUtil.LoadModsDictAsync();

        // check if tag already in dict
        if (dict.ContainsKey(modName))
        {
            dict.Remove(modName);
            ResourceUtil.SaveModsDict(dict);

            await DeferAsync();
            await FollowupAsync(ephemeral: false, text: $"Mod with tag {modName} removed.");
        }
        else
        {
            await DeferAsync(true);
            await FollowupAsync(ephemeral: true, text: $"No mod with tag {modName} registered.");
        }
    }


    /// <summary>
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

        await DeferAsync();
        await FollowupAsync(embed: embed.Build());
    }

    /// <summary>
    /// </summary>
    /// <param name="moddingTool"></param>
    [SlashCommand("info", "Send info on the selected modding tool")]
    public async Task Info(ModdingTool moddingTool)
    {
        var toolsDict = await ResourceUtil.LoadModToolsDictAsync();

        // try get 
        if (toolsDict.TryGetValue(moddingTool.ToString(), out var tool))
        {
            var user = await Context.Client.GetUserAsync(tool.Author) ?? Context.Client.CurrentUser;
            var thumbnail = !string.IsNullOrEmpty(tool.ThumbnailUrl) ? tool.ThumbnailUrl : user.GetAvatarUrl();

            var embed = new EmbedBuilder()
                .WithTitle(tool.Title)
                .WithColor(tool.Color.ToDiscordColor())
                .WithAuthor(user)
                .WithUrl(tool.Url)
                .WithDescription(tool.Description)
                .WithThumbnailUrl(thumbnail)
                .WithCurrentTimestamp();
            embed.AddField(@"🌐 Url", tool.Url);
            embed.AddField(@"❓ Wiki", tool.Wiki);
            foreach (var (title, value) in tool.Fields) embed.AddField(title, value);

            await DeferAsync();
            await FollowupAsync(embed: embed.Build());
        }
    }

    /// <summary>
    /// </summary>
    /// <param name="message"></param>
    [MessageCommand("Check DDS Format")]
    public async Task CheckDdsFormat(IMessage message)
    {
        IEnumerable<IAttachment> attachments =
            message.Attachments.Where(x => Path.GetExtension(x.Filename).Equals(".dds"));
        foreach (var attachment in attachments)
        {
            using HttpClient client = new();
            var response = await client.GetAsync(attachment.Url);
            if (response.StatusCode == HttpStatusCode.OK)
            {
                await using var stream = await response.Content.ReadAsStreamAsync();
                using BinaryReader br = new(stream);

                if (stream.Length < 148)
                {
                    await DeferAsync(true);
                    await FollowupAsync(ephemeral: true, text: $"{attachment.Filename} - that's not a dds file...");
                    return;
                }

                if (br.ReadInt32() != 0x20534444)
                {
                    await DeferAsync(true);
                    await FollowupAsync(ephemeral: true, text: $"{attachment.Filename} - that's not a dds file...");
                    return;
                }

                // If the DDS_PIXELFORMAT dwFlags is set to DDPF_FOURCC and dwFourCC is set to "DX10" an additional DDS_HEADER_DXT10 structure will be present
                br.ReadBytes(80);
                if (br.ReadInt32() != 0x30315844)
                {
                    await DeferAsync(true);
                    await FollowupAsync(ephemeral: true,
                        text: $"{attachment.Filename} - that's not a dx10 dds file...");
                    return;
                }

                await DeferAsync();

                br.ReadBytes(40);
                var fmt = (DXGI_FORMAT)br.ReadInt32();

                var embed = new EmbedBuilder()
                    .WithTitle(attachment.Filename)
                    .WithColor(Color.Green)
                    .WithDescription($"➡ DDS format: `{fmt}`")
                    //.WithThumbnailUrl()
                    .WithCurrentTimestamp();
                await FollowupAsync(embed: embed.Build());
            }
        }
    }
}