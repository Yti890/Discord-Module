using Discord_Module.Events;
using DiscordModuleDependency;

namespace Discord_Module.API.Commands
{
    public class BotCommandSender : CommandSender
    {
        public BotCommandSender(string channelId, string senderId, string nickname, string command)
        {
            ChannelId = channelId;
            SenderId = senderId;
            Nickname = nickname;
            Command = command;
        }

        public string Command { get; }
        public string ChannelId { get; }
        public override string SenderId { get; }
        public override string Nickname { get; }
        public override ulong Permissions { get; } = ServerStatic.PermissionsHandler.FullPerm;
        public override byte KickPower { get; } = byte.MaxValue;
        public override bool FullPermissions { get; } = true;

        public override async void RaReply(string text, bool success, bool logToConsole, string overrideDisplay) => await PluginStart._client.TransmitAsync(new RemoteClient(ActionType.SendMessage, ChannelId, $"{Command}|{text.Substring(text.IndexOf('#') + 1)}", success));

        public override async void Print(string text) => await PluginStart._client.TransmitAsync(new RemoteClient(ActionType.SendMessage, ChannelId, text.Substring(text.IndexOf('#') + 1), true));

        public override bool Available() => true;
    }
}
