using Discord;

namespace FuzzoBot.Models;

public record class ModTool(
    string Title,
    string Description, 
    string Url,
    string Color,
    ulong Author,
    string Wiki,
    string ThumbnailUrl,
    ModToolInfoField[] Fields
);

public record class ModToolInfoField(string Title, string Value);