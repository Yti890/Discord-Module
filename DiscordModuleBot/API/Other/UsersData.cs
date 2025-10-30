namespace DiscordModuleBot.API.Other
{

    public class UsersData
    {
        public Dictionary<string, string> SteamToDiscord { get; set; } = new();
        public static UsersData Default => new UsersData
        {
            SteamToDiscord = new Dictionary<string, string>()
        };
    }
}
