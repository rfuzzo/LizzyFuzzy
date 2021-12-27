using System.Reflection;
using System.Text.Json;
using FuzzoBot.Models;

namespace FuzzoBot.Utility;

/// <summary>
///     A helper class for embedded resources
/// </summary>
public static class ResourceUtil
{
    /// <summary>
    ///     Returns Modding Tools Info
    /// </summary>
    /// <returns></returns>
    public static async Task<Dictionary<string, ModTool>> GetModToolsAsync()
    {
        var resourceName = "FuzzoBot.Resources.ModTools.json";
        var assembly = Assembly.GetExecutingAssembly();

        await using var stream = assembly.GetManifestResourceStream(resourceName).NotNull();
        using var reader = new StreamReader(stream);
        var toolsDict = await JsonSerializer.DeserializeAsync<Dictionary<string, ModTool>>(stream);
        ArgumentNullException.ThrowIfNull(toolsDict);
        return toolsDict;
    }
}