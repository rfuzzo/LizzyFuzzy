using Discord;

namespace FuzzoBot.Extensions;

public static class StringExtensions
{
    /// <summary>
    ///     Clamp a string to a specific length
    /// </summary>
    /// <param name="input"></param>
    /// <param name="clamp"></param>
    /// <returns></returns>
    public static string Clamp(this string input, int clamp)
    {
        return input.Length > clamp ? input[..clamp] : input;
    }

    public static Color ToDiscordColor(this string str)
    {
        return str switch
        {
            nameof(Color.Teal) => Color.Teal,
            nameof(Color.DarkTeal) => Color.DarkTeal,
            nameof(Color.Green) => Color.Green,
            nameof(Color.DarkGreen) => Color.DarkGreen,
            nameof(Color.Blue) => Color.Blue,
            nameof(Color.DarkBlue) => Color.DarkBlue,
            nameof(Color.Purple) => Color.Purple,
            nameof(Color.DarkPurple) => Color.DarkPurple,
            nameof(Color.Magenta) => Color.Magenta,
            nameof(Color.DarkMagenta) => Color.DarkMagenta,
            nameof(Color.Gold) => Color.Gold,
            nameof(Color.LightOrange) => Color.LightOrange,
            nameof(Color.Orange) => Color.Orange,
            nameof(Color.DarkOrange) => Color.DarkOrange,
            nameof(Color.Red) => Color.Red,
            nameof(Color.DarkRed) => Color.DarkRed,
            nameof(Color.LightGrey) => Color.LightGrey,
            nameof(Color.LighterGrey) => Color.LighterGrey,
            nameof(Color.DarkGrey) => Color.DarkGrey,
            nameof(Color.DarkerGrey) => Color.DarkerGrey,
            _ => Color.Default
        };
    }
}