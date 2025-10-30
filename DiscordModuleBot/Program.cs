using DiscordModuleBot.API.Other;
using DiscordModuleBot.API.Services;
using Newtonsoft.Json;

namespace DiscordModuleBot
{
    public static class Program
    {
        private static Config? _config;
        private static UsersData? _users;

        private static readonly string KCfgFile = "config.json";
        private static readonly string KUsersFile = "users.json";

        private static readonly List<Bot> _bots = new();
        public static Config Config => _config ??= LoadConfig();
        public static UsersData Users => _users ??= LoadUsers();

        public static Random Rng { get; } = new();

        public static async Task Main(string[] args)
        {
            LogService.PrintBanner();
            LogService.EnableDebug = args.Contains("--debug");
            _ = Config;
            _ = Users;
            foreach (var (id, token) in Config.BotTokens)
                _bots.Add(new Bot(id, token));
            AppDomain.CurrentDomain.ProcessExit += (_, _) => OnExit();

            await LogService.Info(nameof(Main), "All bots initialized. Entering KeepAlive loop.");

            await KeepAliveAsync();
        }

        private static Config LoadConfig()
        {
            if (File.Exists(KCfgFile))
            {
                try
                {
                    return JsonConvert.DeserializeObject<Config>(File.ReadAllText(KCfgFile))!;
                }
                catch (Exception ex)
                {
                    LogService.Error(nameof(LoadConfig), $"Failed to load config: {ex}");
                }
            }

            var defaultConfig = Config.Default;
            File.WriteAllText(KCfgFile, JsonConvert.SerializeObject(defaultConfig, Formatting.Indented));
            LogService.Warning(nameof(LoadConfig), "Config file not found. Default config created.");
            return defaultConfig;
        }

        private static UsersData LoadUsers()
        {
            if (File.Exists(KUsersFile))
            {
                try
                {
                    return JsonConvert.DeserializeObject<UsersData>(File.ReadAllText(KUsersFile))!;
                }
                catch (Exception ex)
                {
                    LogService.Error(nameof(LoadUsers), $"Failed to load users: {ex}");
                }
            }

            var defaultUsers = UsersData.Default;
            File.WriteAllText(KUsersFile, JsonConvert.SerializeObject(defaultUsers, Formatting.Indented));
            LogService.Warning(nameof(LoadUsers), "Users file not found. Default users created.");
            return defaultUsers;
        }

        private static void OnExit()
        {
            LogService.Warning(nameof(OnExit), "Shutting down bots...");

            foreach (var bot in _bots)
            {
                try
                {
                    bot.Destroy();
                }
                catch (Exception ex)
                {
                    LogService.Error(nameof(OnExit), $"Failed to destroy bot: {ex}");
                }
            }

            if (Config.Debug)
            {
                LogService.Warning(nameof(OnExit), "Debug mode: waiting 10 seconds before exit...");
                Thread.Sleep(10000);
            }

            LogService.Info(nameof(OnExit), "Shutdown complete.");
        }

        private static async Task KeepAliveAsync()
        {
            await LogService.Debug(nameof(KeepAliveAsync), "Entering infinite KeepAlive loop to keep bots alive.");
            await Task.Delay(Timeout.Infinite);
        }
    }
}