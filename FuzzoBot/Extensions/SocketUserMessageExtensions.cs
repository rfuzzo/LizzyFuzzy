using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;

namespace FuzzoBot.Extensions
{

    public static class SocketUserMessageExtensions
    {
        /// <summary>
        ///     Handles custom commands typed with \command before a message
        /// </summary>
        /// <param name="message"></param>
        /// <param name="rawCommand"></param>
        /// <param name="input"></param>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public static async Task HandleCustomCommand(this SocketUserMessage message, string rawCommand, string input)
        {
            if (Enum.TryParse<CustomCommands>(rawCommand, true, out var command))
            {
                switch (command)
                {
                    case CustomCommands.Md:
                        await message.HandleMarkdownCommand(input);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                await message.DeleteAsync();
                return;
            }

            await message.ReplyAsync($"Custom command \\{rawCommand} not found.", false, null, AllowedMentions.None);
        }

        /// <summary>
        ///     The markdown command converts markdown in to a discord usable markdown syntax
        /// </summary>
        /// <param name="message"></param>
        /// <param name="input"></param>
        private static async Task HandleMarkdownCommand(this SocketUserMessage message, string input)
        {
            var embed = new EmbedBuilder
            {
                //Title = "Markdown(1 + to Discord)",
                Color = Color.Gold
            };
            embed.AddField("Markdown",
                    MarkdownToDiscord(input))
                .WithAuthor(message.Author)
                .WithCurrentTimestamp();

            await message.ReplyAsync(embed: embed.Build());

            string MarkdownToDiscord(string input)
            {
                var lines = input.Split(Environment.NewLine);
                var resultLines = new List<string>();
                for (var i = 0; i < lines.Length; i++)
                {
                    var line = lines[i];

                    if (line.StartsWith("# "))
                        line = $"**{line}**\n==================";
                    else if (line.StartsWith("## "))
                        line = $"**{line}**\n------------------";
                    else if (line.StartsWith("### ")) line = $"**{line}**\n";

                    resultLines.Add(line);
                }

                return string.Join(Environment.NewLine, resultLines);
            }
        }

        private enum CustomCommands
        {
            Md
        }
    }
}