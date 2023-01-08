using System;
using System.Threading.Tasks;
using Discord;

namespace FuzzoBot;

public static class LoggingProvider
{
    private static void Log(LogMessage message)
    {
        Console.ForegroundColor = message.Severity switch
        {
            LogSeverity.Critical => ConsoleColor.Red,
            LogSeverity.Error => ConsoleColor.Red,
            LogSeverity.Warning => ConsoleColor.Yellow,
            LogSeverity.Info => ConsoleColor.White,
            LogSeverity.Verbose => ConsoleColor.DarkGray,
            LogSeverity.Debug => ConsoleColor.DarkGray,
            _ => Console.ForegroundColor
        };

        Console.WriteLine(
            $"{DateTime.Now,-19} [{message.Severity,8}] {message.Source}: {message.Message} {message.Exception}");
        Console.ResetColor();
    }

    public static void Log(string message)
    {
        Log(new LogMessage(LogSeverity.Info, "", message));
    }

    public static void Log(Exception exception)
    {
        Log(new LogMessage(LogSeverity.Error, "", exception.Message, exception));
    }

    public static void Log(LogSeverity severity, string message)
    {
        Log(new LogMessage(severity, "", message));
    }

    public static Task LogAsync(LogMessage arg)
    {
        Log(arg);
        return Task.CompletedTask;
    }
}