using DiscordModuleDependency;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace DiscordModuleBot.API.ConfigObj
{
    public class LogChannel
    {
        public ulong Id { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public LogType LogType { get; set; }
    }
}
