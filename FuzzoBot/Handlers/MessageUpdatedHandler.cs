using Discord;
using Discord.WebSocket;

namespace FuzzoBot.Handlers;

public static class MessageUpdatedHandler
{
    public static async Task MessageUpdated(
        Cacheable<IMessage, ulong> before, 
        SocketMessage after,
        ISocketMessageChannel channel)
    {
        var message = await before.GetOrDownloadAsync();
        
        Console.WriteLine($"{message} -> {after}");
    }
}