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

public record class BotTag(string Body);

public record class ModToolInfoField(string Title, string Value);