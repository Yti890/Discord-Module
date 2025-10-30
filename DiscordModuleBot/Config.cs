using DiscordModuleBot.API.ConfigObj;
using DiscordModuleDependency;

namespace DiscordModuleBot
{
    public class Config
    {
        public Dictionary<ushort, string> BotTokens { get; set; } = new();
        public Dictionary<ushort, ChannelConfig> Channels { get; set; } = new();
        public Dictionary<ushort, Dictionary<ulong, List<string>>> ValidCommands { get; set; } = new();
        public Dictionary<ushort, ulong> DiscordServerIds { get; set; } = new();
        public Dictionary<ushort, TcpServerConfig> TcpServers { get; set; } = new();
        public Dictionary<ulong, string> DiscordAutomaticRoles { get; set; } = new();
        public int KeepAliveInterval { get; set; }
        public int MessageDelay { get; set; }
        public bool Debug { get; set; }
        public static Config Default => new()
        {
            BotTokens = new Dictionary<ushort, string>
            {
                {
                    1,
                    "bot-token-here"
                }
            },

            Channels = new Dictionary<ushort, ChannelConfig>
            {
                {
                    1,
                    new()
                    {
                        TopicInfo = new List<ulong>
                        {
                            0
                        },
                        CommandChannel = new List<ulong>
                        {
                            0,
                        },
                        Logs = new LogChannels
                        {
                            Commands = new List<LogChannel>
                            {
                                new()
                                {
                                    Id = 0,
                                    LogType = LogType.Embed
                                },
                            },
                            GameEvents = new List<LogChannel>
                            {
                                new()
                                {
                                    Id = 0,
                                    LogType = LogType.Embed
                                },
                            },
                            Bans = new List<LogChannel>
                            {
                                new()
                                {
                                    Id = 0,
                                    LogType = LogType.Embed
                                },
                            },
                            Reports = new List<LogChannel>
                            {
                                new()
                                {
                                    Id = 0,
                                    LogType = LogType.Embed
                                },
                            },
                            Errors = new List<LogChannel>
                            {
                                new()
                                {
                                    Id = 0,
                                    LogType = LogType.Embed
                                },
                            },
                            AdminChat = new List<LogChannel>
                            {
                                new()
                                {
                                    Id = 0
                                },
                            },
                        }
                    }
                }
            },

            ValidCommands = new Dictionary<ushort, Dictionary<ulong, List<string>>>
            {
                {
                    1, new Dictionary<ulong, List<string>>
                    {
                        {
                            0, new List<string>
                            {
                                "di"
                            }
                        }
                    }
                }
            },

            DiscordServerIds = new Dictionary<ushort, ulong>
            {
                {
                    1, 0
                }
            },

            TcpServers = new Dictionary<ushort, TcpServerConfig>
            {
                {
                    1, new TcpServerConfig
                    {
                        Port = 9000,
                        IpAddress = "127.0.0.1"
                    }
                }
            },

            DiscordAutomaticRoles = new Dictionary<ulong, string>
            {
                { 123456789012345678, "owner" },
                { 234567890123456789, "admin" },
                { 345678901234567890, "mod" }
            },

            KeepAliveInterval = 2000,
            MessageDelay = 1000,
            Debug = false,
        }; 
    }
}
