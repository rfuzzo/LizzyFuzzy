using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using FuzzoBot;
using FuzzoBot.Handlers;
using FuzzoBot.Modules;
using Microsoft.Extensions.DependencyInjection;

public class Program
{
    public static Task Main(string[] args)
    {
        return new Program().MainAsync();
    }

    private async Task MainAsync()
    {
        using var services = ConfigureServices( /*configuration*/);
        var client = services.GetRequiredService<DiscordSocketClient>();
        var commands = services.GetRequiredService<InteractionService>();
        var messageReceivedHandler = services.GetRequiredService<MessageReceivedHandler>();

        client.Log += LoggingProvider.Log;
        commands.Log += LoggingProvider.Log;
        client.Ready += async () =>
        {
#if DEBUG
            await commands.RegisterCommandsToGuildAsync(Constants.Guild.test, true);
#else
            await commands.RegisterCommandsToGuildAsync(Constants.Guild.test, true);
            await commands.RegisterCommandsToGuildAsync(Constants.Guild.moddingCmmmunity, true);

            //await commands.RegisterCommandsGloballyAsync(true);
#endif

            Console.WriteLine("Bot is connected!");
        };

        await services.GetRequiredService<CommandHandler>().InitializeAsync();

        await client.LoginAsync(TokenType.Bot, Environment.GetEnvironmentVariable("DISCORD_TOKEN"));
        await client.StartAsync();

//         client.MessageUpdated += MessageUpdatedHandler.MessageUpdated;
        client.MessageReceived += messageReceivedHandler.OnMessageReceived;
        

        await Task.Delay(Timeout.Infinite);
    }

    private static ServiceProvider ConfigureServices( /*IConfiguration configuration*/)
    {
        return new ServiceCollection()
            .AddSingleton(x => new DiscordSocketClient(
                new DiscordSocketConfig
                {
                    LogLevel = LogSeverity.Info,

                    // If you or another service needs to do anything with messages
                    // (eg. checking Reactions, checking the content of edited/deleted messages),
                    // you must set the MessageCacheSize. You may adjust the number as needed.
                    MessageCacheSize = 50

                    // If your platform doesn't have native WebSockets,
                    // add Discord.Net.Providers.WS4Net from NuGet,
                    // add the `using` at the top, and uncomment this line:
                    //WebSocketProvider = WS4NetProvider.Instance
                }))
            .AddSingleton(x => new InteractionService(x.GetRequiredService<DiscordSocketClient>(),
                new InteractionServiceConfig
                {
                    LogLevel = LogSeverity.Info
                }))
            .AddSingleton<CommandHandler>()
            .AddSingleton<MessageReceivedHandler>()
            .BuildServiceProvider();
    }
}