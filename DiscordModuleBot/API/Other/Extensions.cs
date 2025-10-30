using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace DiscordModuleBot.API.Other
{
    public static class Extensions
    {
        public static string SplitCamelCase(this string str)
        {
            if (string.IsNullOrWhiteSpace(str))
                return str;
            return Regex.Replace(str, "(?<!^)([A-Z])", " $1");
        }
        public static string GetUsername(this SocketGuild guild, ulong userId)
        {
            var user = guild.GetUser(userId);
            return !string.IsNullOrEmpty(user?.Username) ? user.Username : $"Name unavailable ({userId})";
        }
    }
}
