using System.ComponentModel;
using Discord;

namespace FuzzoBot.Modules;

using Discord.Interactions;

public class CommonModule : InteractionModuleBase
{
    [SlashCommand("emote", "Get emote links")]
    public async Task EchoEmote([Summary(description: "the input to echo")]string echo)
    {
        if (Emote.TryParse(echo, out var emote))
        {
            await RespondAsync(emote.Url);
        }
    }

    [SlashCommand("ping", "Checks the bot connection")]
    public async Task Ping() => await RespondAsync("pong");
    
    
    public enum ModdingTool
    {
        CET,
        Red4Ext,
        Redscript,
        WolvenKit
    }
    [SlashCommand("info", "Send info on the selected modding tool")]
    public async Task Blep(ModdingTool moddingTool)
    {
        switch (moddingTool)
        {
            case ModdingTool.WolvenKit:
            {
                var embed = new EmbedBuilder()
                    .WithTitle(moddingTool.ToString())
                    .WithColor(Color.Blue)
                    .WithAuthor(Context.Client.CurrentUser)
                    .WithUrl("https://github.com/WolvenKit/Wolvenkit")
                    .WithDescription("Mod editor/creator for RedEngine games. The point is to have an all in one tool for creating mods for the games made with the engine. ")
                    //.WithThumbnailUrl("https://github.com/WolvenKit/WolvenKit/blob/master/assets/logo_50px.png")
                    .WithCurrentTimestamp();
                embed.AddField("GitHub repository",
                    "https://github.com/WolvenKit/Wolvenkit");
                embed.AddField("Wiki",
                    "https://wiki.redmodding.org/wolvenkit/");
                embed.AddField("Latest release",
                    "https://github.com/WolvenKit/WolvenKit/releases/latest");   
                embed.AddField("Latest nightly build",
                    "https://github.com/WolvenKit/WolvenKit-nightly-releases/releases/latest");
                await RespondAsync(embed: embed.Build());
                break;
            }
            case ModdingTool.CET:
            {
                var embed = new EmbedBuilder()
                    .WithTitle(moddingTool.ToString())
                    .WithColor(Color.DarkGreen)
                    .WithAuthor(Context.Client.CurrentUser)
                    .WithUrl("https://github.com/yamashi/CyberEngineTweaks")
                    .WithDescription("Cyber Engine Tweaks is a framework giving modders a way to script mods using Lua with access to all the internal scripting features. It also comes with a Dear ImGui to provide GUI for different mods you are using, along with console and TweakDB editor for more advanced usage. It also adds some patches for quality of life, all of which can be enabled/disabled through the settings menu or config files (requires game restart to apply).")
                    //.WithThumbnailUrl("https://github.com/WolvenKit/WolvenKit/blob/master/assets/logo_50px.png")
                    .WithCurrentTimestamp();
                embed.AddField("GitHub repository",
                    "https://github.com/yamashi/CyberEngineTweaks");
                embed.AddField("Wiki",
                    "https://wiki.redmodding.org/cyber-engine-tweaks/");
                embed.AddField("Latest release",
                    "https://github.com/yamashi/CyberEngineTweaks/releases/latest");
                await RespondAsync(embed: embed.Build());
                break;
            }
            case ModdingTool.Red4Ext:
            {
                var embed = new EmbedBuilder()
                    .WithTitle(moddingTool.ToString())
                    .WithColor(Color.DarkRed)
                    .WithAuthor(Context.Client.CurrentUser)
                    .WithUrl("https://github.com/WopsS/RED4ext")
                    .WithDescription("A script extender for REDengine 4 (Cyberpunk 2077). RED4ext is a library that extends REDengine 4. It will allow modders to add new features, modify the game behavior, add new scripting functions or call existing ones in your own plugins.")
                    //.WithThumbnailUrl("https://github.com/WolvenKit/WolvenKit/blob/master/assets/logo_50px.png")
                    .WithCurrentTimestamp();
                embed.AddField("GitHub repository",
                    "https://github.com/WopsS/RED4ext");
                embed.AddField("Wiki",
                    "https://wiki.redmodding.org/red4ext/");
                embed.AddField("Latest release",
                    "https://github.com/WopsS/RED4ext/releases/latest");
                await RespondAsync(embed: embed.Build());
                break;
            }
            case ModdingTool.Redscript:
            {
                var embed = new EmbedBuilder()
                    .WithTitle(moddingTool.ToString())
                    .WithColor(Color.Red)
                    .WithAuthor(Context.Client.CurrentUser)
                    .WithUrl("https://github.com/jac3km4/redscript")
                    .WithDescription(" Compiler/decompiler toolkit for redscript. Toolkit for working with scripts used by REDengine in Cyberpunk 2077. Currently includes a compiler, a decompiler and a disassembler.")
                    .WithThumbnailUrl("https://user-images.githubusercontent.com/11986158/145484796-9bf1f77f-e706-4e15-b46b-c9b949f0086c.png")
                    .WithCurrentTimestamp();
                embed.AddField("GitHub repository",
                    "https://github.com/jac3km4/redscript");
                embed.AddField("Wiki",
                    "https://wiki.redmodding.org/redscript/language/intro");
                embed.AddField("Latest release",
                    "https://github.com/jac3km4/redscript/releases/latest");
                await RespondAsync(embed: embed.Build());
                break;
            }
            default:
                throw new ArgumentOutOfRangeException(nameof(moddingTool), moddingTool, null);
        }

        
    }
    
}