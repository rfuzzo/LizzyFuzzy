using System.Net;
using System.Text;
using Discord;
using Discord.WebSocket;
using FuzzoBot.Extensions;
using HtmlAgilityPack;

namespace FuzzoBot.Handlers;

public class MessageReceivedHandler
{
    private static readonly Dictionary<string, double> DictSpam = new()
    {
        { "@everyone", 0.7 },
        { "airdrop", 0.5 },
        { "nitro", 0.2 },
        { "discord", 0.2 },
        { "free", 0.2 },
        { ".ru", 0.4 },
        { "dicsord", 0.4 },
        { "discorde", 0.4 },
        { "http", 0.1 }
    };

    private readonly DiscordSocketClient _client;

    private readonly Dictionary<string, Emote> _emotes = new();
    private readonly Random _rand;

    public MessageReceivedHandler(DiscordSocketClient client)
    {
        _client = client;
        _rand = new Random();

        {
            if (Emote.TryParse(Constants.Emotes.tos, out var emote)) _emotes.Add("tos", emote);
        }
        {
            if (Emote.TryParse(Constants.Emotes.debug_emote, out var emote)) _emotes.Add("dbg", emote);
        }
    }

    public async Task OnMessageReceived(SocketMessage rawMessage)
    {
        // We don't want the bot to respond to itself or other bots.
        // Ignore system messages, or messages from other bots
        if (rawMessage is not SocketUserMessage message) return;
        if (message.Source != MessageSource.User) return;
        if (rawMessage.Author.Id == _client.CurrentUser.Id || rawMessage.Author.IsBot) return;

        var content = rawMessage.Content;
        if (string.IsNullOrEmpty(content)) return;
        
        // support custom slash commands
        if (content[0] == '\\')
        {
            var idx = content.IndexOf(' ') + 1;
            var command = content[1 .. idx];
            await message.HandleCustomCommand(command, content[idx ..]);
            return;
        }
        
        if (await HandleVandalism()) return;

        // handle templates
        var links = (await HandleLinks(message.Content)).ToList();
        var classes = (await HandleClasses(message.Content)).ToList();
        
        if (links.Count > 0 || classes.Count > 0)
        {
            var embed = new EmbedBuilder()
                .WithUrl("https://nativedb.red4ext.com")
                .WithTitle($"Links:")
                .WithColor(Color.Red)
                .WithCurrentTimestamp();
            
            if (links.Count > 0)
            {
                var description = string.Join('\n', links);
                if (description.Length > 4096) description = description[..4096];
                embed.WithDescription(description);
            }
            
            if (classes.Count > 0)
            {
                foreach (var c in classes)
                {
                    embed.AddField(c.Item1, c.Item2);
                }
            }

            await message.ReplyAsync(embed: embed.Build(), allowedMentions: AllowedMentions.None);
        }


        // evaluate other stuff
        // if (content.Contains("diffuse"))
        //     await message.ReplyAsync("Unfortunately we still haven't found the defuse textures :cry:",
        //         false, null, AllowedMentions.None);
        // else if (content.Contains("leak"))
        //     await message.ReplyAsync(_emotes["tos"].ToString(),
        //         false, null, AllowedMentions.None);
        
        async Task<bool> HandleVandalism()
        {
            double p = 0;
            foreach (var (key, value) in DictSpam)
                if (content.Contains(key))
                    p += DictSpam[key];
            
            // randomize p for show
            var r = _rand.NextDouble() / 10;
            r *= _rand.NextDouble() < 0.5 ? -1 : 1;
            p += r;

            // roles
            var author = message.Author;
            if (author is SocketGuildUser user)
                if (user.Roles.Count == 1)
                    p *= 2;

            var pOut = Math.Round(p * 100, 2);

            switch (p)
            {
                case > 0.9:
                    await message.ReplyAsync(
                        $"This message is with {pOut}% probability a scam. Banning on sight. False positive? pinging {Constants.rfuzzo}");
                    await rawMessage.DeleteAsync();
                    return true;
                case > 0.6:
                    await message.ReplyAsync(
                        $"This message is with {pOut}% probability a scam. pinging {Constants.rfuzzo}");
                    return true;
            }

            return false;
        }
    }

    /// <summary>
    ///     Handles custom links in a message
    /// </summary>
    /// <param name="content"></param>
    private async Task<IEnumerable<string>> HandleLinks(string content)
    {
        // crawl text links
        const char linkStart = '[';
        const char linkEnd = ']';

        var reading = false;
        var lastChar = '\n';
        var current = "";
        
        var links = new List<string>();
        foreach (var c in content)
        {
            if (c == linkEnd && lastChar == linkEnd && reading)
            {
                reading = false;
                links.Add(current[..^1]);
                current = "";
            }
            if (reading)
            {
                current += c;
            }
            if (c == linkStart && lastChar == linkStart && !reading)
            {
                reading = true;
            }
            
            lastChar = c;
        }

        const string root = @"https://nativedb.red4ext.com/";
        using var client = new HttpClient();
        
        var finalLinks = new List<string>();
        foreach (var url in links.Select(link => $"{root}{link}"))
        {
            var response = await client.GetAsync(url);
            if (response.StatusCode == HttpStatusCode.OK)
            {
                finalLinks.Add(url);
            }
        }
        
        return finalLinks;
    }
    
    /// <summary>
    ///     Handles custom class links in a message
    /// </summary>
    /// <param name="content"></param>
    private async Task<IEnumerable<(string,string)>> HandleClasses(string content)
    {
        // crawl text links
        const char classStart = '{';
        const char classEnd = '}';
        
        var reading = false;
        var lastChar = '\n';
        var current = "";

        // crawl text links
        var classes = new List<string>();
        foreach (var c in content)
        {
            if (c == classEnd && lastChar == classEnd && reading)
            {
                reading = false;
                classes.Add(current[..^1]);
                current = "";
            }
            if (reading)
            {
                current += c;
            }
            if (c == classStart && lastChar == classStart && !reading)
            {
                reading = true;
            }
            
            lastChar = c;
        }

        const string root = @"https://nativedb.red4ext.com/";
        using var client = new HttpClient();
        
        var finalClasses = new List<(string,string)>();
        foreach (var url in classes.Select(c => $"{root}{c}"))
        {
            var response = await client.GetAsync(url);
            if (response.StatusCode != HttpStatusCode.OK) continue;
            
            var html = response.Content.ReadAsStringAsync().Result;
            var doc = new HtmlDocument();
            doc.LoadHtml(html);
            var fields = doc.DocumentNode.SelectSingleNode("/html/body/div/div/div/div[2]/main/div[2]");
            var sb = new StringBuilder();
            sb.AppendLine("**Fields**");
            sb.AppendLine("```");
            if (fields.ChildNodes.Count > 0)
            {
                // Fields
                if (fields.ChildNodes.First().InnerText == "Fields")
                {
                    foreach (var childNode in fields.ChildNodes.Skip(1))
                    {
                        sb.AppendLine($"{childNode.LastChild.InnerText} {childNode.FirstChild.InnerText}");
                    }
                }
                // Methods
            }

            sb.AppendLine("```");
            var description = sb.ToString();
            finalClasses.Add((url, description));
        }
        
        return finalClasses;
    }
}