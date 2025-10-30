using System.ComponentModel;

namespace Discord_Module
{
    public class Config
    {
        [Description("Active Debug for that Experemental Plugin?")]
        public bool Debug { get; set; } = false;

        [Description("The date format that will be used throughout the plugin (es. dd/MM/yy HH:mm:ss or MM/dd/yy HH:mm:ss)")]
        public string DateFormat { get; private set; } = "dd/MM/yy HH:mm:ss";

        [Description("Bot IP address")]
        public string IPAddress { get; private set; } = "127.0.0.1";
        [Description("Bot port")]
        public ushort Port { get; private set; } = 9000;
        [Description("Language code")]
        public string code { get; private set; } = "en";
        [Description("Hide IPAdress or not?")]
        public bool ShouldLogIPAddresses { get; internal set; } = true;
        [Description("Active Log for Intercom Using?")]
        public bool PlayerIntercomSpeaking { get; internal set; } = false;
        [Description("Active Log for Triggiring Tesla?")]
        public bool PlayerTriggeringTesla { get; internal set; } = false;
    }
}