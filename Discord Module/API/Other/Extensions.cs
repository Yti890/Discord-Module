using CommandSystem;
using System.Text.RegularExpressions;
using Discord_Module.API.Commands;

namespace Discord_Module.API.Other
{
    public static class Extensions
    {
        public static bool IsValidUserId(this string userId)
        {
            if (string.IsNullOrEmpty(userId)) return false;
            return Regex.IsMatch(userId, @"^(\d{17})@(steam|patreon|northwood)$|^(\d{18})@discord$");
        }
        public static bool IsValidDiscordId(this string discordId)
        {
            return !string.IsNullOrEmpty(discordId) && Regex.IsMatch(discordId, @"^\d{18}$");
        }
        public static bool IsValidDiscordRoleId(this string discordRoleId)
        {
            return discordRoleId.IsValidDiscordId();
        }
        public static CommandSender GetCompatible(this ICommandSender sender)
        {
            return sender is CommandSender cs ? cs.GetCompatible() : null!;
        }
        public static CommandSender GetCompatible(this CommandSender sender)
        {
            if (sender.GetType() != typeof(RemoteAdmin.PlayerCommandSender))
                return sender;

            return new PlayerCommandSender(sender.SenderId, sender.Nickname, sender.Permissions, sender.KickPower, sender.FullPermissions);
        }
    }
}
