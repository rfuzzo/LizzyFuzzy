namespace FuzzoBot.Models
{
public record ModTool(
    string Title,
    string Description,
    string Url,
    string Color,
    ulong Author,
    string Wiki,
    string ThumbnailUrl,
    ModToolInfoField[] Fields
);

public record BotTag(string Body);

public record ModToolInfoField(string Title, string Value);
}