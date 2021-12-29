namespace FuzzoBot.Extensions;

public static class StringExtensions
{
    /// <summary>
    /// Clamp a string to a specific length
    /// </summary>
    /// <param name="input"></param>
    /// <param name="clamp"></param>
    /// <returns></returns>
    public static string Clamp(this string input, int clamp) => input.Length > clamp ? input[..clamp] : input;
}