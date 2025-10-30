namespace DiscordModuleBot.API.ConfigObj
{
    public class ChannelConfig
    {
        public LogChannels Logs { get; set; } = new();
        public List<ulong> TopicInfo { get; set; } = new();
        public List<ulong> CommandChannel { get; set; } = new();
    }
}
