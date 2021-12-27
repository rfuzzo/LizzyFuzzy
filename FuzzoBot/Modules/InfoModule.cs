using System.Reflection;
using System.Text.Json;
using Discord;
using FuzzoBot.Models;
using FuzzoBot.Utility;

namespace FuzzoBot.Modules;

using Discord.Interactions;

public class CommonModule : InteractionModuleBase
{
    [SlashCommand("emote", "Get emote url")]
    public async Task EchoEmote([Summary(description: "the custom emote")]string emote)
    {
        if (Emote.TryParse(emote, out var discordEmote)) await RespondAsync(discordEmote.Url);
    }
    
    [UserCommand("Avatar")]
    public async Task GetAvatarUrl(IUser user) => await RespondAsync(user.GetAvatarUrl());

    [SlashCommand("ping", "Checks the bot connection")]
    public async Task Ping() => await RespondAsync("pong");
    
    
    public enum ModdingTool
    {
        CET,
        Red4Ext,
        Redscript,
        WolvenKit,
        MlsetupBuilder
    }
    [SlashCommand("info", "Send info on the selected modding tool")]
    public async Task InfoCommand(ModdingTool moddingTool)
    {
        var resourceName = "FuzzoBot.Resources.ModTools.json";
        var assembly = Assembly.GetExecutingAssembly();

        await using var stream = assembly.GetManifestResourceStream(resourceName).NotNull();
        using var reader = new StreamReader(stream);
        var toolsDict = await JsonSerializer.DeserializeAsync<Dictionary<string, ModTool>>(stream);
        ArgumentNullException.ThrowIfNull(toolsDict);
        
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
            foreach (var (title, value) in tool.Fields)
            {
                embed.AddField(title, value);
            }
            await RespondAsync(embed: embed.Build());
        }
    }
    
}