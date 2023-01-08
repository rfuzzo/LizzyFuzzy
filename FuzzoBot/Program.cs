using System;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using dotenv.net;
using FuzzoBot.Handlers;
using Microsoft.Extensions.DependencyInjection;

namespace FuzzoBot;

public class Program
{
    public static Task Main(string[] args)
    {
        return new Program().MainAsync();
    }

    private async Task MainAsync()
    {
        // Load environment variables from .env file
        DotEnv.Load();
        var envVars = DotEnv.Read();

        await using var services = ConfigureServices( /*configuration*/);
        var client = services.GetRequiredService<DiscordSocketClient>();
        var commands = services.GetRequiredService<InteractionService>();
        var messageReceivedHandler = services.GetRequiredService<MessageReceivedHandler>();

        client.Log += LoggingProvider.LogAsync;
        commands.Log += LoggingProvider.LogAsync;
        client.Ready += async () =>
        {
#if DEBUG
            await commands.RegisterCommandsToGuildAsync(Constants.Guilds["test"]);
#else
            //await commands.RegisterCommandsToGuildAsync(Constants.Guilds["test"]);
            await commands.RegisterCommandsToGuildAsync(Constants.Guilds["community"]);
            //await commands.RegisterCommandsToGuildAsync(Constants.Guilds["gpm"]);

            //await commands.RegisterCommandsGloballyAsync(true);
#endif

            Console.WriteLine("Bot is connected!");
        };

        await services.GetRequiredService<CommandHandler>().InitializeAsync();

        await client.LoginAsync(TokenType.Bot, envVars["DISCORD_TOKEN"]);
        await client.StartAsync();

        //         client.MessageUpdated += MessageUpdatedHandler.MessageUpdated;
        client.MessageReceived += messageReceivedHandler.OnMessageReceived;


        await Task.Delay(Timeout.Infinite);
    }

    private static ServiceProvider ConfigureServices( /*IConfiguration configuration*/)
    {
        return new ServiceCollection()
            .AddSingleton(_ => new DiscordSocketClient(
                new DiscordSocketConfig
                {
                    LogLevel = LogSeverity.Info,

                    // If you or another service needs to do anything with messages
                    // (eg. checking Reactions, checking the content of edited/deleted messages),
                    // you must set the MessageCacheSize. You may adjust the number as needed.
                    MessageCacheSize = 50,

                    // If your platform doesn't have native WebSockets,
                    // add Discord.Net.Providers.WS4Net from NuGet,
                    // add the `using` at the top, and uncomment this line:
                    //WebSocketProvider = WS4NetProvider.Instance
                    
                    GatewayIntents = GatewayIntents.AllUnprivileged | GatewayIntents.MessageContent,

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