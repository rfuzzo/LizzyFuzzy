using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Discord;
using Discord.WebSocket;

namespace FuzzoBot.Services
{
    public enum EStrikeOutCome
    {
        None,
        Warn,
        Kick,
        Ban
    }

    /// <summary>
    /// This bots memory
    /// </summary>
    public class MemoryService
    {
        private record Infraction(int Strikes, DateTime LastStrike);

        private readonly object _lock = new object();

        private const int MemoryLimit = 10;
        //public readonly Dictionary<string, Emote> Emotes = new();

        private readonly ConcurrentDictionary<ulong, Infraction> _strikes = new(2, 10);

        public MemoryService()
        {
            // {
            //     if (Emote.TryParse(Constants.Emotes.tos, out var emote)) Emotes.Add("tos", emote);
            // }
            // {
            //     if (Emote.TryParse(Constants.Emotes.debug_emote, out var emote)) Emotes.Add("dbg", emote);
            // }
            // {
            //     if (Emote.TryParse(Constants.Emotes.thumbsup, out var emote)) Emotes.Add("thumbsup", emote);
            // }
        }

        public int StrikeUser(SocketUser author, int severity = 1)
        {
            var strikes = 1;
            var authorId = author.Id;
            lock (_lock)
            {
                // update memory
                if (!_strikes.TryGetValue(authorId, out var infraction))
                {
                    _strikes.TryAdd(authorId, new Infraction(strikes, DateTime.Now));
                }
                else
                {
                    strikes = infraction.Strikes + 1;
                    _strikes[authorId] = new Infraction(strikes, DateTime.Now);
                }
            }

            Forget();

            return strikes;
        }

        private void Forget()
        {
            lock (_lock)
            {
                // forgetting
                foreach (var (id, _) in _strikes
                             .Where(x => (x.Value.LastStrike - DateTime.Now).TotalDays > 30))
                {
                    _strikes.TryRemove(id, out _);
                }

                // clamp
                if (_strikes.Count > MemoryLimit)
                {
                    var keys = _strikes
                        .OrderBy(x => x.Value.LastStrike)
                        .Take(MemoryLimit / 2)
                        .Select(x => x.Key)
                        .ToList();
                    for (int i = 0; i < keys.Count; i++)
                    {
                        _strikes.Remove(keys[i], out _);
                    }
                }
            }
        }
    }
}