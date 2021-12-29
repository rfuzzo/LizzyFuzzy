using System.Text;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using FuzzoBot.Extensions;
using Microsoft.EntityFrameworkCore;
using RedDatabase.Model;

namespace FuzzoBot.Modules;

public class Red4Module : InteractionModuleBase
{
    // ReSharper disable InconsistentNaming
    public enum EHashSubCommands
    {
        info,
        uses,
        usedby
    }


    // ReSharper restore InconsistentNaming
    private const int PageSize = 20;

    /// <summary>
    /// </summary>
    /// <param name="subCommands"></param>
    /// <param name="hash"></param>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    [SlashCommand("file", "red4 file info")]
    public async Task FileCommand(EHashSubCommands subCommands, string hash)
    {
        if (!ulong.TryParse(hash, out var uhash)) await RespondAsync("Not a valid hash");

        RedFile? file;
        await using (var db = new RedDbContext())
        {
            file = await db.Files
                .FirstOrDefaultAsync(x => x.RedFileId == uhash);
        }
        if (file is null)
        {
            await RespondAsync("No file with that hash found");
            return;
        }
        
        switch (subCommands)
        {
            case EHashSubCommands.info:
            {
                var embed = new EmbedBuilder()
                    .WithTitle(file.Name)
                    .WithColor(Color.Green)
                    .WithCurrentTimestamp();
                embed.AddField("Hash", $"```{file.RedFileId.ToString()}```");
                embed.AddField("Archive", $"```{file.Archive}```");
                if (file.Uses is not null)
                {
                    var val = string.Join('\n', file.Uses);
                    embed.AddField("Uses", $"```{val.Clamp(1010)}```");
                }
                await RespondAsync(embed: embed.Build());
                break;
            }
            case EHashSubCommands.uses:
            {
                await DeferAsync();
                
                var embed = new EmbedBuilder()
                    .WithTitle(file.Name)
                    .WithColor(Color.Green)
                    .WithCurrentTimestamp();
                if (file.Uses is not null)
                {
                    foreach (var use in file.Uses.Take(25))
                    {
                        // get file
                        RedFile? usedFile;
                        await using (var db = new RedDbContext())
                        {
                            usedFile = await db.Files.FirstOrDefaultAsync(x => x.RedFileId == use);
                        }
                        if (usedFile is null)
                        {
                            continue;
                        }
                        embed.AddField(usedFile.Name, $"`{usedFile.RedFileId}`");
                    }
                }
                await Context.Interaction.ModifyOriginalResponseAsync(properties =>
                {
                    properties.Embed = embed.Build();
                    //properties.Components = componentBuilder.Build();
                });
                //await RespondAsync(embed: embed.Build());
                break;
            }
            case EHashSubCommands.usedby:
            {
                await DeferAsync();
                
                var embed = new EmbedBuilder()
                    .WithTitle(file.Name)
                    .WithColor(Color.Green)
                    .WithCurrentTimestamp();
                
                var pageResults = await GetDataBaseEntriesPaginated(file.RedFileId, EVanillaArchives.all, 1);
                if (pageResults.Count == 0) return; 
                
                foreach (var usesFile in pageResults.Take(25))
                    embed.AddField(usesFile.Name, $"hash: `{usesFile.RedFileId.ToString()}`");
                
                await Context.Interaction.ModifyOriginalResponseAsync(properties =>
                {
                    properties.Embed = embed.Build();
                    //properties.Components = componentBuilder.Build();
                });
                //await RespondAsync(embed: embed.Build());
                break;
            }
            default:
                throw new ArgumentOutOfRangeException(nameof(subCommands), subCommands, null);
        }
    }

    /// <summary>
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
                embed.AddField(file.Name, $"hash: `{file.RedFileId.ToString()}`\narchive: `{file.Archive}`");
        }
        else
        {
            var sb = new StringBuilder();
            foreach (var file in pageResults) sb.AppendLine(file.Name);
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
        var forward = direction == "next";

        if (Context.Interaction is not SocketMessageComponent socket) return;

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
            if (pageResults.Count == 0) return;

            if (verbose)
            {
                embed.Fields.Clear();
                foreach (var file in pageResults)
                    embed.AddField(file.Name, $"hash: `{file.RedFileId.ToString()}`\narchive: `{file.Archive}`");
            }
            else
            {
                var sb = new StringBuilder();
                foreach (var file in pageResults) sb.AppendLine(file.Name);

                embed.WithDescription($"```{sb.ToString().Clamp(4090)}```");
            }

            await Context.Interaction.ModifyOriginalResponseAsync(properties => { properties.Embed = embed.Build(); });

            //await socket.UpdateAsync(properties => { properties.Embed = embed.Build(); });
        }
    }

    private static async Task<List<RedFile>> GetDataBaseEntriesPaginated(
        string contains, 
        EVanillaArchives archive,
        int page)
    {
        var archiveName = archive.ToString();
        var ret = new List<RedFile>();
        await using (var db = new RedDbContext())
        {
            var result = db.Files
                .Where(x => x.Name != null && x.Name.Contains(contains));
            if (archive != EVanillaArchives.all)
                result = result
                    .Where(x => x.Archive != null && x.Archive.Equals(archiveName));
            ret = await result
                .Skip(PageSize * (page - 1))
                .Take(PageSize)
                .ToListAsync();
        }
        return ret;
    }
    
    private static async Task<List<RedFile>> GetDataBaseEntriesPaginated(
        ulong parentHash, 
        EVanillaArchives archive,
        int page)
    {
        var archiveName = archive.ToString();
        var ret = new List<RedFile>();
        await using (var db = new RedDbContext())
        {
            var result = db.Files
                .AsAsyncEnumerable()
                .Where(x => x.Uses != null && x.Uses.Contains(parentHash));
            if (archive != EVanillaArchives.all)
                result = result
                    .Where(x => x.Archive != null && x.Archive.Equals(archiveName));
            ret = await result
                .Skip(PageSize * (page - 1))
                .Take(PageSize)
                .ToListAsync();
        }
        return ret;
    }
}