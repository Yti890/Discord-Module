using Discord;
using Discord.Interactions;
using DiscordModuleBot.API.Services;
using DiscordModuleBot.Embeds;
using DiscordModuleDependency;

namespace DiscordModuleBot.API.Commands
{
    public class SendCommand : InteractionModuleBase<SocketInteractionContext>
    {
        private readonly Bot _bot;
        public SendCommand(Bot bot) => _bot = bot;

        [SlashCommand("send", "Sends a command to the SCP server.")]
        public async Task SendCommandAsync([Summary("command", "The command to send")] string command)
        {
            var canRun = SlashCommandHandler.CanRunCommand((IGuildUser)Context.User, _bot.ServerNumber, command);
            if (canRun != ErrorCodes.None)
            {
                await RespondAsync(embed: await ErrorEmbedService.GetErrorEmbed(canRun), ephemeral: true);
                return;
            }

            try
            {
                await LogService.Debug(nameof(SendCommandAsync), $"Sending {command}");
                await _bot.Server.SendAsync(new RemoteClient(DiscordModuleDependency.ActionType.ExecuteCommand,Context.Channel.Id,command,Context.User.Id,Context.User.Username));
                await RespondAsync("✅ Command sent.", ephemeral: true);
            }
            catch (Exception e)
            {
                await LogService.Error(nameof(SendCommandAsync), e);
                await RespondAsync(embed: await ErrorEmbedService.GetErrorEmbed(ErrorCodes.Unspecified, e.Message));
            }
        }
    }
}
