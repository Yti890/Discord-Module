using Discord;
using System.Reflection;

namespace DiscordModuleBot.API.Services
{
    internal class EmbedBuilderService
    {
        public static Embed CreateBasicEmbed(string title, string description, Color color)
        {
            var embed = new EmbedBuilder()
                .WithTitle(title)
                .WithDescription(description)
                .WithColor(color)
                .WithCurrentTimestamp()
                .WithFooter($"Discord Module | v{Assembly.GetExecutingAssembly().GetName().Version}")
                .Build();

            return embed;
        }
        public static Task<Embed> CreateBasicEmbedAsync(string title, string description, Color color)
        {
            return Task.FromResult(CreateBasicEmbed(title, description, color));
        }
    }
}
