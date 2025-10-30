using DiscordModuleDependency;

namespace DiscordModuleBot.API.ConfigObj
{
    public class LogChannels
    {
        public List<LogChannel>? Commands { get; set; } = new();
        public List<LogChannel>? GameEvents { get; set; } = new();
        public List<LogChannel>? Bans { get; set; } = new();
        public List<LogChannel>? Reports { get; set; } = new();
        public List<LogChannel>? Errors { get; set; } = new();
        public List<LogChannel>? AdminChat { get; set; } = new();

        public IEnumerable<LogChannel> this[ChannelType result]
        {
            get =>
                (result switch
                {
                    ChannelType.Command => Commands,
                    ChannelType.GameEvents => GameEvents,
                    ChannelType.Bans => Bans,
                    ChannelType.Reports => Reports,
                    ChannelType.Errors => Errors,
                    ChannelType.AdminChat => AdminChat,
                    _ => throw new ArgumentOutOfRangeException(nameof(result), result, null)
                })!;
        }
    }
}
