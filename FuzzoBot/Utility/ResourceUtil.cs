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
    private static string ModsFileName => "Mods.json";
    
    public static Task<Dictionary<string, ModTool>> LoadModToolsDictAsync() => LoadToolsDictAsync(ModToolsFileName);
    public static Task<Dictionary<string, int>> LoadModsDictAsync() => LoadDictAsync<string, int>(ModsFileName);
    public static void SaveModsDict(Dictionary<string, int> dict) => SaveDict(dict, ModsFileName);


    private static void SaveDict(Dictionary<string, int> dict, string fileName)
    {
        var dictPath = Path.GetFullPath(Path.Combine("Resources", fileName));
        Directory.CreateDirectory(Path.GetFullPath(Path.Combine("Resources")));
        File.WriteAllText(dictPath,
            JsonSerializer.Serialize(dict, new JsonSerializerOptions { WriteIndented = true }));
    }

    private static async Task<Dictionary<T1, T2>> LoadDictAsync<T1, T2>(string fileName) where T1 : class
    {
        var dictPath = Path.GetFullPath(Path.Combine("Resources", fileName));

        if (!File.Exists(dictPath))
        {
            return new Dictionary<T1, T2>();
        }

        var stream = File.OpenRead(dictPath).NotNull();
        var dict = await JsonSerializer.DeserializeAsync<Dictionary<T1, T2>>(stream, new JsonSerializerOptions { WriteIndented = true });

        ArgumentNullException.ThrowIfNull(dict);
        return dict;
    }

    /// <summary>
    ///     Returns Modding Tools Info
    /// </summary>
    /// <returns></returns>
    private static async Task<Dictionary<string, ModTool>> LoadToolsDictAsync(string fileName) 
    {
        var extractPath = Path.GetFullPath(Path.Combine("Resources",  fileName));
        if (File.Exists(extractPath))
        {
            return await LoadDictAsync<string, ModTool>(extractPath);
        }

        // download
        HttpClient client = new();
        var url =
            $"https://raw.githubusercontent.com/CDPR-Modding-Documentation/Cyberpunk-Modding-Docs/main/bot/{fileName}";
        var response = await client.GetAsync(new Uri(url));

        try
        {
            response.EnsureSuccessStatusCode();

            // save to file
            await using FileStream fs = new(extractPath, FileMode.Create);
            await response.Content.CopyToAsync(fs);
        }
        catch (HttpRequestException ex)
        {
            LoggingProvider.Log(ex);
        }

        return await LoadDictAsync<string, ModTool>(extractPath);
    }

    
}