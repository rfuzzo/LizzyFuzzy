using System.Text;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using FuzzoBot.Extensions;
using FuzzoBot.Utility;
using Microsoft.EntityFrameworkCore;
using RedDatabase.Model;

namespace FuzzoBot.Modules;

public class Red4Module : InteractionModuleBase
{
    const int PageSize = 20;
    
    // ReSharper disable InconsistentNaming
    public enum EHashSubCommands
    {
        info
    }
    
    
    // ReSharper restore InconsistentNaming
    
    /// <summary>
    /// 
    /// </summary>
    /// <param name="subCommands"></param>
    /// <param name="hash"></param>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    [SlashCommand("file", "red4 file info")]
    public async Task FileCommand(EHashSubCommands subCommands, string hash)
    {
        if (!ulong.TryParse(hash, out var uhash))
        {
            await RespondAsync("Not a valid hash");
        }
        
        await using var db = new RedDbContext();

        switch (subCommands)
        {
            case EHashSubCommands.info:
                var result = await db.Files.FirstOrDefaultAsync(x => x.RedFileId == uhash);
                if (result is not null)
                {
                    var embed = new EmbedBuilder()
                        .WithTitle($"Info {result.RedFileId.ToString()}")
                        .WithColor(Color.Green)
                        //.WithDescription(desc)
                        .WithCurrentTimestamp();
                    embed.AddField("Name", $"```{result.Name}```");
                    embed.AddField("Archive", $"```{result.Archive}```");
                    if (result.Uses is not null)
                    {
                        //1024.
                        var val = string.Join('\n', result.Uses);
                        if (val.Length > 1010)
                        {
                            val = val[..1010];
                        }
                        embed.AddField("Uses", $"```{val}...```");    
                    }
                    await RespondAsync(embed: embed.Build());
                }
                else
                {
                    await RespondAsync("No file with that hash found");
                }
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(subCommands), subCommands, null);
        }

        
    }
    
    /// <summary>
    /// 
    /// </summary>
    /// <param name="contains"></param>
    /// <param name="archive"></param>
    /// <param name="verbose"></param>
    [SlashCommand("find", "Find red4 files")]
    public async Task FindCommand(
        string contains, 
        EVanillaArchives archive = EVanillaArchives.all, 
        bool verbose = false)
    {
        await DeferAsync();

        var embed = new EmbedBuilder()
            .WithTitle($"Search query: {contains}")
            .WithColor(Color.Blue)
            .WithFooter($"{(int)archive}:{verbose}:1")
            .WithCurrentTimestamp();

        var pageResults = await GetDataBaseEntriesPaginated(contains, archive, 1);
        if (verbose)
        {
            foreach (var file in pageResults)
            {
                embed.AddField(file.Name,  $"hash: `{file.RedFileId.ToString()}`\narchive: `{file.Archive}`");
            }    
        }
        else
        {
            var sb = new StringBuilder();
            foreach (var file in pageResults)
            {
                sb.AppendLine(file.Name);
            } 
            embed.WithDescription($"```{sb.ToString().Clamp(4090)}```");
        }
        
        var componentBuilder = new ComponentBuilder()
            .WithButton("⬅️", "find-previous") //⬅➡⬅➡
            .WithButton("➡️", "find-next")
            .WithButton("❌️", "delete");

        await Context.Interaction.ModifyOriginalResponseAsync(properties =>
        {
            properties.Embed = embed.Build();
            properties.Components = componentBuilder.Build();
        });
        
        //await RespondAsync(embed: embed.Build(), components: componentBuilder.Build());
    }

   

    [ComponentInteraction("delete")]
    public async Task ComponentInteractionDelete()
    {
        if (Context.Interaction is SocketMessageComponent socket)
            await Context.Channel.DeleteMessageAsync(socket.Message);
    }
    
    [ComponentInteraction("find-*")]
    public async Task ComponentInteractionFind(string direction)
    {
        bool forward = direction == "next";

        if (Context.Interaction is not SocketMessageComponent socket)
        {
            return;
        }

        await DeferAsync();
        
        var message = socket.Message;
        var messageEmbed = message.Embeds.First();
        if (messageEmbed.Footer.HasValue)
        {
            var splits = messageEmbed.Footer.Value.Text.Split(':');
            var archive = (EVanillaArchives)int.Parse(splits[0]);
            var verbose = bool.Parse(splits[1]);
            var oldPage = int.Parse(splits[2]);
            var page = oldPage;
            if (forward) page += 1;
            else page = Math.Max(1, page - 1);
            if (oldPage == page) return;
            var contains = messageEmbed.Title[14..];

            var embed = messageEmbed.ToEmbedBuilder();
            embed.WithFooter($"{(int)archive}:{verbose}:{page}");

            var pageResults = await GetDataBaseEntriesPaginated(contains, archive, page);
            if (pageResults.Count == 0)
            {
                return;
            }

            if (verbose)
            {
                embed.Fields.Clear();
                foreach (var file in pageResults)
                {
                    embed.AddField(file.Name, $"hash: `{file.RedFileId.ToString()}`\narchive: `{file.Archive}`");
                }
            }
            else
            {
                var sb = new StringBuilder();
                foreach (var file in pageResults)
                {
                    sb.AppendLine(file.Name);
                }

                embed.WithDescription($"```{sb.ToString().Clamp(4090)}```");
            }

            await Context.Interaction.ModifyOriginalResponseAsync(properties => { properties.Embed = embed.Build(); });

            //await socket.UpdateAsync(properties => { properties.Embed = embed.Build(); });
        }
    }
    
    private static async Task<List<RedFile>> GetDataBaseEntriesPaginated(string contains, EVanillaArchives archive, int page)
    {
        await using var db = new RedDbContext();
        var archiveName = archive.ToString();

        var result = db.Files
            .AsAsyncEnumerable()
            .Where(x =>  x.Name != null && x.Name.Contains(contains));

        if (archive != EVanillaArchives.all)
        {
            result = result
                .Where(x => x.Archive != null && x.Archive.Equals(archiveName));
        }

        return await result
                .Skip(PageSize * (page - 1))
                .Take(PageSize)
                .ToListAsync();
    }
    
}