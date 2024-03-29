using System.Threading.Tasks;
using Discord.Interactions;

namespace FuzzoBot.Modules;

/// <summary>
/// </summary>
public class FunModule : InteractionModuleBase
{
    /// <summary>
    /// </summary>
    /// <param name="memeTemplate"></param>
    /// <param name="text1"></param>
    /// <param name="text2"></param>
    /// <param name="text3"></param>
    /// <param name="text4"></param>
    [SlashCommand("meme", "Generate a meme from a memegen template")]
    public async Task GenerateMeme([Summary(description: "Template for the meme")] MemeTemplate memeTemplate,
        [Summary(
            description: "Optional text to fit in the first text location on the meme template (if it exists)")]
        string text1 = "",
        [Summary(
            description: "Optional text to fit in the second text location on the meme template (if it exists)")]
        string text2 = "",
        [Summary(
            description: "Optional text to fit in the third text location on the meme template (if it exists)")]
        string text3 = "",
        [Summary(
            description: "Optional text to fit in the fourth text location on the meme template (if it exists)")]
        string text4 = "")
    {
        await DeferAsync();
        var finalTextString = "";
        string[] texts = { text1, text2, text3, text4 };
        foreach (var text in texts)
            if (text != "")
            {
                var updatedText = text;
                updatedText = updatedText.Replace("_", "__");
                updatedText = updatedText.Replace("-", "--");
                updatedText = updatedText.Replace("?", "~q");
                updatedText = updatedText.Replace("&", "~a");
                updatedText = updatedText.Replace("%", "~p");
                updatedText = updatedText.Replace("#", "~h");
                updatedText = updatedText.Replace("/", "~s");
                updatedText = updatedText.Replace("\\", "~b");
                updatedText = updatedText.Replace("<", "~l");
                updatedText = updatedText.Replace(">", "~g");
                updatedText = updatedText.Replace("\"", "''");
                updatedText = updatedText.Replace(" ", "_");
                finalTextString += $"/{updatedText}";
            }

        await FollowupAsync($"https://api.memegen.link/images/{memeTemplate}{finalTextString}");
    }
}