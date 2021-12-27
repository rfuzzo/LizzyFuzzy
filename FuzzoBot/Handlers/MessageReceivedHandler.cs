using Discord;
using Discord.WebSocket;
using FuzzoBot.Extensions;

namespace FuzzoBot.Handlers;

public class MessageReceivedHandler
{
    private readonly DiscordSocketClient _client;
    private readonly Random _rand;

    private readonly Dictionary<string, Emote> _emotes = new();

    public MessageReceivedHandler(DiscordSocketClient client)
    {
        _client = client;
        _rand = new Random();

        {
            if (Emote.TryParse(Constants.Emotes.tos, out var emote))
            {
                _emotes.Add("tos", emote);
            }    
        }
        {
            if (Emote.TryParse(Constants.Emotes.debug_emote, out var emote))
            {
                _emotes.Add("dbg", emote);
            }    
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
        
        // support custom slash commands
        if (content[0] == '\\')
        {
            var idx = content.IndexOf(' ') + 1;
            var command = content[1 .. idx];
            await message.HandleCustomCommand(command, content[idx ..]);
            return;
        }
        
        double p = 0;
        
        
        if (string.IsNullOrEmpty(content)) return;

        foreach (var (key, value) in dict_spam)
        {
            if (content.Contains(key))
            {
                p += dict_spam[key];
            }
        }    

        // randomize p for show
        var r = _rand.NextDouble() / 10;
        r *= _rand.NextDouble() < 0.5 ? -1 : 1;
        p += r;
        
        // roles
        var author = message.Author;
        if (author is SocketGuildUser user)
        {
            if (user.Roles.Count == 1)
            {
                p *= 2;
            }
        }
    
        // evaluate other stuff
        if (content.Contains("diffuse"))
        {
            await message.ReplyAsync("Unfortunately we still haven't found the defuse textures :cry:",
                false, null, AllowedMentions.None);
        }
        else if (content.Contains("leak"))
        {
            await message.ReplyAsync(_emotes["tos"].ToString(),
                false, null, AllowedMentions.None);
        }
        
        var pOut = Math.Round(p * 100, 2);

        switch (p)
        {
            case > 0.9:
                await message.ReplyAsync(
                    $"This message is with {pOut}% probability a scam. Banning on sight. False positive? pinging {Constants.rfuzzo}");
                await rawMessage.DeleteAsync();
                break;
            case > 0.6:
                await message.ReplyAsync(
                    $"This message is with {pOut}% probability a scam. pinging {Constants.rfuzzo}");
                break;
        }

    }


    private static Dictionary<string, double> dict_spam = new Dictionary<string, double>()
    {
        {"@everyone", 0.7},
        {"airdrop", 0.5},
        {"nitro", 0.2},
        {"discord", 0.2},
        {"free", 0.2},
        {".ru", 0.4},
        {"dicsord", 0.4},
        {"discorde", 0.4},
        {"http", 0.1}
    };

    
}