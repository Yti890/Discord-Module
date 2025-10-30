using LabApi.Features.Console;
using LabApi.Features.Wrappers;

namespace Discord_Module.API.Commands
{
    public class PlayerCommandSender : CommandSender
    {
        private readonly Player player;
        public PlayerCommandSender(string senderId, string nickname, ulong permissions, byte kickPower, bool fullPermissions)
        {
            SenderId = senderId;
            Nickname = nickname;
            Permissions = permissions;
            KickPower = kickPower;
            FullPermissions = fullPermissions;
            player = Player.Get(SenderId);
        }
        public override string SenderId { get; }
        public override string Nickname { get; }
        public override ulong Permissions { get; }
        public override byte KickPower { get; }
        public override bool FullPermissions { get; }
        public override void Print(string text) => Logger.Info($"[DISCORD] {text}");
        public override void RaReply(string text, bool success, bool logToConsole, string overrideDisplay) => player.SendConsoleMessage($"DISCORDINTEGRATION#{text}, {success}, {logToConsole}, {overrideDisplay}");
        public override bool Available() => true;
    }
}
