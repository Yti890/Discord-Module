using Discord;
using DiscordModuleBot.API.Services;
using DiscordModuleDependency;
namespace DiscordModuleBot.Embeds
{
    public static class ErrorEmbedService
    {
        private static readonly Dictionary<ErrorCodes, string> ErrorDescriptions = new()
        {
            { ErrorCodes.PermissionDenied, "You are not permitted to run this command.\n{0}" },
            { ErrorCodes.UnableToParseDuration, "{0} is invalid.\nThe duration must be a whole number followed by a single modifier letter of 's', 'm', 'h', 'd', 'w', 'M' or 'y'" },
            { ErrorCodes.SpecifiedUserNotFound, "The specified user ({0}) is not in the server." },
            { ErrorCodes.InternalCommandError, "Joker broke something, ping him.\n{0}" },
            { ErrorCodes.InvalidChannelId, "No channel with the provided ID {0} exists." },
            { ErrorCodes.UnableToParseDate, "The date {0} is invalid." },
            { ErrorCodes.InvalidCommand, "That command is not enabled on this server." },
            { ErrorCodes.FailedToParseTitle, "No valid title was found. Titles for embeds must be encased in \"double-quotes\"" },
            { ErrorCodes.FailedToParseColor, "{0} is not a valid HTML HEX color code." },
            { ErrorCodes.Unspecified, "Unknown error. Please contact the developer." },
            { ErrorCodes.NoRecordForUserFound, "No database records for {0} were found." },
            { ErrorCodes.InvalidMessageId, "No message with ID {0} was found." },
            { ErrorCodes.TriggerLengthExceedsLimit, "Ping triggers are limited to {0} characters in length." }
        };
        private static string GetErrorMessage(ErrorCodes code, string extra = "")
        {
            return $"Code {(int)code}: {code.ToString()} {extra}".TrimEnd(' ');
        }
        private static string GetErrorDescription(ErrorCodes code)
        {
            return ErrorDescriptions.TryGetValue(code, out var desc)
                ? desc
                : ErrorDescriptions[ErrorCodes.Unspecified];
        }
        public static Task<Embed> GetErrorEmbed(ErrorCodes errorCode, string extra = "")
        {
            string description;

            if (!string.IsNullOrEmpty(extra))
                description = string.Format(GetErrorDescription(errorCode), $"\"{extra}\"");
            else
                description = GetErrorDescription(errorCode).Replace("{0}", string.Empty) + "\n**Please report all bugs on Github.**";

            var embed = EmbedBuilderService.CreateBasicEmbed(GetErrorMessage(errorCode), description, Color.Red);
            return Task.FromResult(embed);
        }
    }
}