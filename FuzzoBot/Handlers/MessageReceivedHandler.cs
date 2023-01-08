using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
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

    private readonly DiscordSocketClient _discordClient;
    private readonly Random _rand;

    public MessageReceivedHandler(
        DiscordSocketClient discordClient
    )
    {
        _discordClient = discordClient;
        _rand = new Random();
    }

    public async Task OnMessageReceived(SocketMessage rawMessage)
    {
        // We don't want the bot to respond to itself or other bots.
        // Ignore system messages, or messages from other bots
        if (rawMessage is not SocketUserMessage message) return;
        if (message.Source != MessageSource.User) return;
        if (rawMessage.Author.Id == _discordClient.CurrentUser.Id || rawMessage.Author.IsBot) return;

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

        // handle templates
        var links = (await HandleLinks(message.Content)).ToList();
        var classes = (await HandleClasses(message.Content)).ToList();

        if (links.Count > 0 || classes.Count > 0)
        {
            var embed = new EmbedBuilder()
                    //.WithUrl("https://nativedb.red4ext.com")
                    //.WithTitle("Links:")
                    .WithColor(Color.Blue)
                //.WithCurrentTimestamp()
                ;

            if (links.Count > 0)
            {
                var description = string.Join('\n', links);
                if (description.Length > 4096) description = description[..4096];
                embed.WithDescription(description);
                await message.ReplyAsync(embed: embed.Build(), allowedMentions: AllowedMentions.None);
            }

            if (classes.Count > 0)
                foreach (var (url, classInfo) in classes)
                {
                    embed
                        .WithUrl(url)
                        .WithTitle(classInfo.name);
                    // fields
                    var sb = new StringBuilder();
                    sb.AppendLine("**Fields**");
                    if (classInfo?.props != null)
                        sb.AppendLine(classInfo.props.Aggregate("```swift\n",
                            (current, prop) => current + $"var {prop.name} : {prop.type}\n"));
                    sb.AppendLine("```");

                    // methods
                    sb.AppendLine("**Methods**");
                    sb.AppendLine("```swift");
                    if (classInfo?.funcs != null)
                        foreach (var f in classInfo.funcs)
                        {
                            var fReturn = f.@return is not null
                                ? $" : {f.@return.type}"
                                : "";
                            var fParams = f.@params is not null
                                ? string.Join(", ", f.@params.Select(x => $"{x.name} : {x.type}"))
                                : "";

                            sb.AppendLine($"{f.shortName}({fParams}){fReturn}");
                        }

                    var desc = sb.ToString().Clamp(4093);
                    desc += "```";
                    embed.WithDescription(desc);

                    await message.ReplyAsync(embed: embed.Build(), allowedMentions: AllowedMentions.None);
                }
        }


        // evaluate other stuff
        // if (content.Contains("diffuse"))
        //     await message.ReplyAsync("Unfortunately we still haven't found the defuse textures :cry:",
        //         false, null, AllowedMentions.None);
        // else if (content.Contains("leak"))
        //     await message.ReplyAsync(_emotes["tos"].ToString(),
        //         false, null, AllowedMentions.None);
    }

    /// <summary>
    ///     Handles custom links in a message
    /// </summary>
    /// <param name="content"></param>
    private static async Task<IEnumerable<string>> HandleLinks(string content)
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

            if (reading) current += c;
            if (c == linkStart && lastChar == linkStart && !reading) reading = true;

            lastChar = c;
        }

        using var client = new HttpClient();

        var finalLinks = new List<string>();
        foreach (var url in links
                     .Distinct()
                     .Select(link => $"{Constants.Red4.NativeDb}{link}"))
        {
            var response = await client.GetAsync(url);
            if (response.StatusCode == HttpStatusCode.OK) finalLinks.Add(url);
        }

        return finalLinks;
    }

    /// <summary>
    ///     Handles custom class links in a message
    /// </summary>
    /// <param name="content"></param>
    private static async Task<IEnumerable<(string, Data)>> HandleClasses(string content)
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

            if (reading) current += c;
            if (c == classStart && lastChar == classStart && !reading) reading = true;

            lastChar = c;
        }

        return await GetNativeClassInfo(classes.Distinct());
    }

    private static async Task<List<(string, Data)>> GetNativeClassInfo(IEnumerable<string> classes)
    {
        var finalClasses = new List<(string, Data)>();

        using var client = new HttpClient();
        foreach (var className in classes)
        {
            var url = $"{Constants.Red4.NativeDb}{className}";
            var response = await client.GetAsync(url);
            if (response.StatusCode != HttpStatusCode.OK) continue;

            var doc = new HtmlDocument();
            doc.LoadHtml(await response.Content.ReadAsStringAsync());
            var nextData = doc.DocumentNode.SelectSingleNode("//*[@id=\"__NEXT_DATA__\"]");
            var scriptData = JsonSerializer.Deserialize<Root>(nextData.InnerText);

            if (scriptData?.props?.pageProps?.data is null) continue;

            finalClasses.Add((url, scriptData.props.pageProps.data));
        }

        return finalClasses;
    }
}