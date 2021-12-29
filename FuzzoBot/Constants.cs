namespace FuzzoBot;

public static class Constants
{
    public const string rfuzzo = @"<@382111410382962690>";

    public static readonly Dictionary<string, ulong> Guilds = new()
    {
        { "test", 532643730730254337},
        { "community", 717692382849663036 },
        { "other", 705931815109656596 },
        { "amm", 420406569872916480 }
    };

    public class Wiki
    {
        public const string Url = @"https://wiki.redmodding.org/";
        public const string EditorInvite = @"https://app.gitbook.com/invite/-MP5ijqI11FeeX7c8-N8/H70HZBOeUulIpkQnBLK7";
    }

    public static class Emotes
    {
        // debug
        public static string debug_emote = @"<:holyC:695709609742041118>";
        public static string tos = @"<a:peepoTOS:892372867663159316>";
    }
}