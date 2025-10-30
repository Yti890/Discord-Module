using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using DiscordModuleBot.API.Services;
using DiscordModuleBot.Embeds;
using DiscordModuleDependency;
using System.ComponentModel.Design;
using System.Reflection;

namespace DiscordModuleBot.API.Commands
{
    public class SlashCommandHandler
    {
        private readonly InteractionService _service;
        private readonly DiscordSocketClient _client;
        private readonly ServiceContainer _container;
        private readonly Bot _bot;

        public SlashCommandHandler(InteractionService service, DiscordSocketClient client, Bot bot)
        {
            _service = service;
            _client = client;
            _bot = bot;

            _container = new ServiceContainer();
            _container.AddService(typeof(Bot), bot);
        }

        public async Task InstallCommandsAsync()
        {
            await _service.AddModulesAsync(Assembly.GetEntryAssembly(), _container);

            _client.InteractionCreated += OnInteraction;
            _service.SlashCommandExecuted += OnCommandError;
            _service.ContextCommandExecuted += OnCommandError;
            _service.ComponentCommandExecuted += OnCommandError;
        }

        private async Task OnInteraction(SocketInteraction interaction)
        {
            try
            {
                var context = new SocketInteractionContext(_client, interaction);

                await _service.ExecuteCommandAsync(context, _container);
                await LogService.Info(nameof(OnInteraction),
                    $"{interaction.User.Username} executed an interaction.");
            }
            catch (Exception ex)
            {
                await LogService.Error(nameof(OnInteraction), ex);

                if (interaction.Type == InteractionType.ApplicationCommand)
                {
                    try
                    {
                        await interaction.RespondAsync(
                            embed: await ErrorEmbedService.GetErrorEmbed(ErrorCodes.Unspecified, ex.Message),
                            ephemeral: true);
                    }
                    catch {  }
                }
            }
        }

        private async Task OnCommandError(SlashCommandInfo info, IInteractionContext context, IResult result) => await SafeErrorRespond(context, result.ToString());

        private async Task OnCommandError(ContextCommandInfo info, IInteractionContext context, IResult result) => await SafeErrorRespond(context, result.ToString());

        private async Task OnCommandError(ComponentCommandInfo info, IInteractionContext context, IResult result) => await SafeErrorRespond(context, result.ToString());

        private static async Task SafeErrorRespond(IInteractionContext context, string reason)
        {
            try
            {
                await context.Interaction.RespondAsync(
                    embed: await ErrorEmbedService.GetErrorEmbed(ErrorCodes.Unspecified, reason),
                    ephemeral: true);
            }
            catch { }
        }

        public static ErrorCodes CanRunCommand(IGuildUser user, ushort serverNum, string command)
        {
            if (!Program.Config.ValidCommands.TryGetValue(serverNum, out var cmdGroups)
                || cmdGroups.Count == 0)
                return ErrorCodes.InvalidCommand;

            foreach (var (roleId, commands) in cmdGroups)
            {
                bool match = commands.Contains(command)
                            || commands.Any(c => command.StartsWith(c))
                            || commands.Contains(".*");

                if (!match)
                    continue;

                var role = user.Guild.GetRole(roleId);
                if (role == null)
                    return ErrorCodes.InvalidCommand;

                return user.Hierarchy >= role.Position
                    ? ErrorCodes.None
                    : ErrorCodes.PermissionDenied;
            }

            return ErrorCodes.InvalidCommand;
        }
    }
}
