using Discord;
using Discord.Interactions;
using Microsoft.EntityFrameworkCore;
using RedDatabase.Model;

namespace FuzzoBot.Modules;

public class Red4Module : InteractionModuleBase
{
    public enum EHashSubCommands
    {
        info
    }
    
    public enum EVanillaArchives
    {
        all,
        audio_1_general,
        audio_2_soundbanks,
        basegame_1_engine,
        basegame_2_mainmenu,
        basegame_3_nightcity,
        basegame_3_nightcity_gi,
        basegame_3_nightcity_terrain,
        basegame_4_animation,
        basegame_4_appearance,
        basegame_4_gamedata,
        basegame_5_video,
        memoryresident_1_general
    }
    
    
    [SlashCommand("hash", "red4 file info")]
    public async Task HashCommand(EHashSubCommands subCommands, string hash)
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
                        embed.AddField("Uses", $"```{val[..1000]}...```");    
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
    
    [SlashCommand("find", "Find red4 files")]
    public async Task FindCommand(string contains, EVanillaArchives archive = EVanillaArchives.all)
    {
        await using var db = new RedDbContext();

        IAsyncEnumerable<RedFile> result;

        if (archive == EVanillaArchives.all)
        {
            result = db.Files
                .AsAsyncEnumerable()
                .Where(x =>  x.Name != null && x.Name.Contains(contains))
                .Take(10);
        }
        else
        {
            result = db.Files
                .AsAsyncEnumerable()
                .Where(x => x.Archive != null && x.Archive.Equals(archive.ToString()))
                .Where(x =>  x.Name != null && x.Name.Contains(contains))
                .Take(10);
        }

        var embed = new EmbedBuilder()
            .WithTitle($"Search query: {contains}")
            .WithColor(Color.Green)
            .WithDescription("Displaying the first 10 files: ")
            .WithCurrentTimestamp();

        await foreach (var redfile in result)
        {
            embed.AddField(redfile.Name, $"hash: `{redfile.RedFileId.ToString()}`\n" +
                                         $"archive: `{redfile.Archive}`");
        }
        
        await RespondAsync(embed: embed.Build());
    }
}