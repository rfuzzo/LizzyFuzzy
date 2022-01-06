namespace FuzzoBot;

public static class Constants
{
    public const string rfuzzo = @"<@382111410382962690>";

    public const int KickCount = 3;
    public const int BanCount = 5;

    public static readonly Dictionary<string, ulong> Guilds = new()
    {
        { "test", 532643730730254337},
        { "gpm", 917893578460655677 },
        { "community", 717692382849663036 },
        { "other", 705931815109656596 },
        { "amm", 420406569872916480 }
    };

    public static class Wiki
    {
        public const string Url = @"https://wiki.redmodding.org/";
        public const string EditorInvite = @"https://app.gitbook.com/invite/-MP5ijqI11FeeX7c8-N8/H70HZBOeUulIpkQnBLK7";
    }

    public static class Red4
    {
        public const string NativeDb = @"https://nativedb.red4ext.com/";
        public const string NativeDbClassInfo = @"https://nativedb.red4ext.com/_next/data/";//b_1yh1CbxCyfxzLaTQW6q/{{ name }}.json";
    }
    
    public static class Emotes
    {
        // debug
        public static string debug_emote = @"<:holyC:695709609742041118>";
        
        public static string tos = @"<a:peepoTOS:892372867663159316>";
        public static string smile = @"<:smile:865304332197691433>";
        public static string thumbsup = @"<:peepothumbsup:847820160383713329>";
        public static string loading = @"<:loading:926156909994725406> ";
    }
}