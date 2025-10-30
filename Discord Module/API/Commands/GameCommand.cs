using Discord_Module.API.Other;
using Discord_Module.API.Pathes;

namespace Discord_Module.API.Commands
{
    public class GameCommand
    {
        public GameCommand(string channelId, string content, DiscordUser user)
        {
            ChannelId = channelId;
            Content = content;
            User = user;
            Sender = new BotCommandSender(channelId, user?.Id, user?.Name, user?.Command);
        }
        public string ChannelId { get; }
        public string Content { get; }
        public DiscordUser User { get; }
        public CommandSender Sender { get; }
        public void Execute() => ProcessQueryWrapper.ProcessQuery(Content, Sender);
    }
}
