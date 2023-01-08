using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using FuzzoBot.Models;

namespace FuzzoBot.Utility;

/// <summary>
///     A helper class for embedded resources
/// </summary>
public static class ResourceUtil
{
    private static string ModToolsFileName => "ModTools.json";

    private static string TagsFileName => "Tags.json";
    //private static string ModsFileName => "Mods.json";

    private static string GetUserProfile()
    {
        return Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
    }

    /// <summary>
    ///     Returns Modding Tools Info
    /// </summary>
    /// <returns></returns>
    public static Task<Dictionary<string, ModTool>> LoadModToolsDictAsync()
    {
        return LoadResourceDictAsync<string, ModTool>(ModToolsFileName);
    }

    public static Task<Dictionary<string, BotTag>> LoadTagsDictAsync()
    {
        return LoadResourceDictAsync<string, BotTag>(TagsFileName);
    }
    //public static Task<Dictionary<string, ModTool>> LoadModDictAsync() => LoadResourceDictAsync(ModsFileName);

    /// <summary>
    ///     Returns Modding Tools Info
    /// </summary>
    /// <returns></returns>
    private static async Task<Dictionary<T1, T2>> LoadResourceDictAsync<T1, T2>(string fileName) where T1 : class
    {
        var extractPath = Path.Combine(GetUserProfile(), fileName);

        if (File.Exists(extractPath))
        {
            // TODO check version
        }
        else
        {
            // download
            HttpClient _client = new();
            var url =
                $"https://raw.githubusercontent.com/CDPR-Modding-Documentation/Cyberpunk-Modding-Docs/main/bot/{fileName}";
            var response = await _client.GetAsync(new Uri(url));

            try
            {
                response.EnsureSuccessStatusCode();

                // save to file
                await using FileStream fs = new(extractPath, FileMode.Create);
                await response.Content.CopyToAsync(fs);
            }
            catch (HttpRequestException ex)
            {
                await LoggingProvider.Log(ex);
            }
        }

        var stream = File.OpenRead(extractPath).NotNull();
        var toolsDict =
            await JsonSerializer.DeserializeAsync<Dictionary<T1, T2>>(stream,
                new JsonSerializerOptions { WriteIndented = true });

        //ArgumentNullException.ThrowIfNull(toolsDict);
        if (toolsDict is null) throw new ArgumentNullException();
        return toolsDict;
    }

    internal static void Reload()
    {
        File.Delete(Path.Combine(GetUserProfile(), ModToolsFileName));
        File.Delete(Path.Combine(GetUserProfile(), TagsFileName));
    }
}