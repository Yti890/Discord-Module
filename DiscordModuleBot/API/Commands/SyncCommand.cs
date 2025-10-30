using Discord.Interactions;
using Discord.WebSocket;
using DiscordModuleDependency;
using Newtonsoft.Json;

namespace DiscordModuleBot.API.Commands
{
    public class SyncCommand : InteractionModuleBase<SocketInteractionContext>
    {
        private readonly Bot _bot;
        public SyncCommand(Bot bot) => _bot = bot;

        [SlashCommand("sync", "Sync your SteamID64@steam for automatic role synchronization.")]
        public async Task SyncAsync([Summary("steamid", "Your SteamID64@steam")] string steamId)
        {
            if (!steamId.EndsWith("@steam"))
            {
                await RespondAsync("❌ Invalid SteamID format. Make sure it ends with @steam.", ephemeral: true);
                return;
            }

            Program.Users.SteamToDiscord[steamId] = Context.User.Id.ToString();
            File.WriteAllText("DiscordIntegration-users.json",
                JsonConvert.SerializeObject(Program.Users, Formatting.Indented));

            if (!Program.Config.DiscordServerIds.TryGetValue(1, out var guildId))
            {
                await RespondAsync("⚠️ Discord server is not configured properly.", ephemeral: true);
                return;
            }

            var client = Context.Client as DiscordSocketClient;
            var guild = client?.GetGuild(guildId);
            var member = guild?.GetUser(Context.User.Id);

            if (guild == null || member == null)
            {
                await RespondAsync("❌ Could not find you in the configured Discord server.", ephemeral: true);
                return;
            }

            var userRoles = member.Roles
                .Where(r => Program.Config.DiscordAutomaticRoles.ContainsKey(r.Id))
                .Select(r => Program.Config.DiscordAutomaticRoles[r.Id])
                .ToList();

            var highestRole = Program.Config.DiscordAutomaticRoles
                .FirstOrDefault(kv => userRoles.Contains(kv.Value)).Value;

            if (highestRole != null)
            {
                await _bot.Server.SendAsync(new RemoteClient(ActionType.AutomaticRoles, $"/pm setgroup {steamId} {highestRole}"));
                await RespondAsync($"✅ Sync complete. Assigned role: {highestRole}", ephemeral: true);
            }
            else
            {
                await RespondAsync("⚠️ Sync complete, but no matching roles were found.", ephemeral: true);
            }
        }
    }
}
