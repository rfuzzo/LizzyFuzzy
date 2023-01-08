using System;
using System.Threading.Tasks;
using Discord;

namespace FuzzoBot;

public static class LoggingProvider
{
    public static Task Log(LogMessage message)
    {
        switch (message.Severity)
        {
            case LogSeverity.Critical:
            case LogSeverity.Error:
                Console.ForegroundColor = ConsoleColor.Red;
                break;
            case LogSeverity.Warning:
                Console.ForegroundColor = ConsoleColor.Yellow;
                break;
            case LogSeverity.Info:
                Console.ForegroundColor = ConsoleColor.White;
                break;
            case LogSeverity.Verbose:
            case LogSeverity.Debug:
                Console.ForegroundColor = ConsoleColor.DarkGray;
                break;
        }

        Console.WriteLine(
            $"{DateTime.Now,-19} [{message.Severity,8}] {message.Source}: {message.Message} {message.Exception}");
        Console.ResetColor();

        return Task.CompletedTask;
    }

    public static Task Log(string message)
    {
        return Log(new LogMessage(LogSeverity.Info, "", message));
    }

    public static Task Log(Exception exception)
    {
        return Log(new LogMessage(LogSeverity.Error, "", exception.Message, exception));
    }

    public static Task Log(LogSeverity severity, string message)
    {
        return Log(new LogMessage(severity, "", message));
    }
}