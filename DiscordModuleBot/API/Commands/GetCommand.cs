using Discord;
using Discord.Interactions;
using DiscordModuleDependency;

namespace DiscordModuleBot.API.Commands
{
    public class GetCommand : InteractionModuleBase<SocketInteractionContext>
    {
        private readonly Bot _bot;
        public GetCommand(Bot bot) => _bot = bot;

        [SlashCommand("get", "Shows which commands you can execute.")]
        public async Task ShowAvailableCommandsAsync()
        {
            if (!Program.Config.ValidCommands.TryGetValue(_bot.ServerNumber, out var serverCommands))
            {
                await RespondAsync("This server was not found.", ephemeral: true);
                return;
            }

            var available = new List<string>();
            foreach (var (roleId, commands) in serverCommands)
                foreach (var cmd in commands)
                    if (SlashCommandHandler.CanRunCommand((IGuildUser)Context.User, _bot.ServerNumber, cmd) == ErrorCodes.None)
                        available.Add(cmd);

            if (!available.Any())
            {
                await RespondAsync("There are no available commands for you.", ephemeral: true);
                return;
            }

            await RespondAsync("You can use these commands:\n" + string.Join("\n", available), ephemeral: true);
        }
    }
}