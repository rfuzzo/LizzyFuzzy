using System.IO.Compression;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using FuzzoBot;
using FuzzoBot.Handlers;
using FuzzoBot.Services;
using Microsoft.Extensions.DependencyInjection;
using dotenv.net;

public class Program
{
    public static Task Main(string[] args)
    {
        return new Program().MainAsync();
    }

    private async Task MainAsync()
    {
        var zipPath = Path.GetFullPath(Path.Combine("Resources", "red.db.zip"));
        var extractPath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        if (!File.Exists(Path.Combine(extractPath, "red.db")))
        {
            Console.WriteLine($"Extracting db to {extractPath} ...");
            ZipFile.ExtractToDirectory(zipPath, extractPath);
            Console.WriteLine("done.");
        }

        // Load environment variables from .env file
        DotEnv.Load();

        using var services = ConfigureServices( /*configuration*/);
        var client = services.GetRequiredService<DiscordSocketClient>();
        var commands = services.GetRequiredService<InteractionService>();
        var messageReceivedHandler = services.GetRequiredService<MessageReceivedHandler>();

        client.Log += LoggingProvider.Log;
        commands.Log += LoggingProvider.Log;
        client.Ready += async () =>
        {
#if DEBUG
            await commands.RegisterCommandsToGuildAsync(Constants.Guilds["test"]);
#else
            //await commands.RegisterCommandsToGuildAsync(Constants.Guilds["test"]);
            await commands.RegisterCommandsToGuildAsync(Constants.Guilds["community"]);
            await commands.RegisterCommandsToGuildAsync(Constants.Guilds["gpm"]);

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
            .AddSingleton<MemoryService>()
            .AddSingleton<MessageReceivedHandler>()
            .BuildServiceProvider();
    }
}
