using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using DiscordModuleBot.API.Commands;
using DiscordModuleBot.API.ConfigObj;
using DiscordModuleBot.API.Services;
using DiscordModuleDependency;
using Newtonsoft.Json;
using ChannelType = DiscordModuleDependency.ChannelType;
using ActionType = DiscordModuleDependency.ActionType;

namespace DiscordModuleBot
{
    public class Bot
    {
        public DiscordSocketClient? _client;
        private SocketGuild? _guild;
        private readonly string _token;
        private int _lastCount = -1, _lastTotal;

        public ushort ServerNumber { get; }
        public TcpServer Server;
        public InteractionService InteractionService { get; private set; } = null!;
        public SlashCommandHandler CommandHandler { get; private set; } = null!;
        public Dictionary<LogChannel, string> Messages { get; } = new();

        private readonly DiscordSocketConfig _config = new()
        {
            GatewayIntents = GatewayIntents.Guilds | GatewayIntents.GuildMessages | GatewayIntents.MessageContent | GatewayIntents.GuildMembers
        };

        public DiscordSocketClient Client => _client ??= new DiscordSocketClient(_config);
        public SocketGuild Guild => _guild ??= Client.GetGuild(Program.Config.DiscordServerIds[ServerNumber]);

        public Bot(ushort port, string token)
        {
            ServerNumber = port;
            _token = token;
            _ = InitAsync();
        }

        public void Destroy() => _client?.LogoutAsync();

        private async Task InitAsync()
        {
            if (!ValidateToken()) return;

            SetupDiscord();
            await RegisterCommandsAsync();
            Server = new TcpServer(Program.Config.TcpServers[ServerNumber].IpAddress,
                Program.Config.TcpServers[ServerNumber].Port, this);
            _ = Server.Start();
            Server.ReceivedFull += OnReceived;
            _ = DequeueMessagesAsync();
        }

        private bool ValidateToken()
        {
            try
            {
                TokenUtils.ValidateToken(TokenType.Bot, _token);
                return true;
            }
            catch (Exception e)
            {
                LogService.Error(nameof(ValidateToken), e);
                return false;
            }
        }

        private void SetupDiscord()
        {
            InteractionService = new InteractionService(Client, new InteractionServiceConfig { AutoServiceScopes = false });
            CommandHandler = new SlashCommandHandler(InteractionService, Client, this);

            InteractionService.Log += SendLog;
            Client.Log += SendLog;
            Client.MessageReceived += MessageReceived;
            Client.GuildMemberUpdated += GuildMemberUpdated;
            Client.UserLeft += UserLeft;
        }

        private async Task RegisterCommandsAsync()
        {
            await CommandHandler.InstallCommandsAsync();

            Client.Ready += async () =>
            {
                int count = (await InteractionService.RegisterCommandsToGuildAsync(Guild.Id)).Count;
                await LogService.Debug(nameof(RegisterCommandsAsync), $"{count} slash commands registered.");
            };

            await Client.LoginAsync(TokenType.Bot, _token);
            await Client.StartAsync();
        }

        private Task SendLog(LogMessage msg) => LogService.Info(nameof(SendLog), msg.Message);

        private async void OnReceived(object? sender, ReceivedFullEventArgs ev)
        {
            try
            {
                var command = JsonConvert.DeserializeObject<RemoteClient>(ev.Data)!;
                switch (command.Action)
                {
                    case ActionType.Log:
                        HandleLogCommand(command);
                        break;
                    case ActionType.SendMessage:
                        await HandleSendMessageAsync(command);
                        break;
                    case ActionType.UpdateActivity:
                        await HandleUpdateActivityAsync(command);
                        break;
                    case ActionType.UpdateChannelActivity:
                        await HandleChannelTopicAsync(command);
                        break;
                    case ActionType.AdminMessage:
                        await HandleChannelAdminAsync(command);
                        break;
                }
            }
            catch (Exception e)
            {
                await LogService.Error(nameof(OnReceived), e);
            }
        }
        private Task MessageReceived(SocketMessage message)
        {
            if (message is not SocketUserMessage userMessage || userMessage.Author.IsBot) return Task.CompletedTask;

            if (Program.Config.Channels.TryGetValue(ServerNumber, out var channelConfig))
            {
                if (channelConfig.Logs.AdminChat.Any(c => c.Id == userMessage.Channel.Id))
                {
                    var formatted = $"[<color=blue>DISCORD</color>] {userMessage.Content} ~ {userMessage.Author.GlobalName}";
                    _ = Server.SendAsync(new RemoteClient(ActionType.AdminMessage, formatted));
                }
            }

            return Task.CompletedTask;
        }

        private void HandleLogCommand(RemoteClient command)
        {
            if (!Enum.TryParse(command.Parameters[0].ToString(), true, out ChannelType type)) return;

            foreach (var channel in Program.Config.Channels[ServerNumber].Logs[type])
            {
                if (!Messages.ContainsKey(channel)) Messages[channel] = string.Empty;
                Messages[channel] += $"[{DateTime.Now}] {command.Parameters[1]}\n";
            }
        }

        private async Task HandleSendMessageAsync(RemoteClient command)
        {
            if (!ulong.TryParse(command.Parameters[0].ToString(), out var chanId)) return;

            var split = command.Parameters[1].ToString()!.Split('|');
            bool isSuccess = (bool)command.Parameters[2];
            var embed = EmbedBuilderService.CreateBasicEmbed($"Server {ServerNumber} {split[0]}", split[1], isSuccess ? Color.Green : Color.Red);

            await Guild.GetTextChannel(chanId)?.SendMessageAsync(embed: embed);
        }

        private async Task HandleUpdateActivityAsync(RemoteClient command)
        {
            try
            {
                var split = command.Parameters[0].ToString()!.Split('/');
                int count = int.Parse(split[0]);
                int total = int.Parse(split[1]);

                if (count > 0 && Client.Status != UserStatus.Online)
                    await Client.SetStatusAsync(UserStatus.Online);
                else if (count == 0 && Client.Status != UserStatus.AFK)
                    await Client.SetStatusAsync(UserStatus.AFK);

                if (count != _lastCount || total != _lastTotal)
                {
                    _lastCount = count;
                    _lastTotal = total > 0 ? total : _lastTotal;
                    await Client.SetActivityAsync(new Game($"{_lastCount}/{_lastTotal}"));
                }
            }
            catch (Exception e)
            {
                await LogService.Error(nameof(HandleUpdateActivityAsync), "Error updating bot status");
                await LogService.Debug(nameof(HandleUpdateActivityAsync), e);
            }
        }

        private async Task HandleChannelTopicAsync(RemoteClient command)
        {
            foreach (var channelId in Program.Config.Channels[ServerNumber].TopicInfo)
            {
                var channel = Guild.GetTextChannel(channelId);
                if (channel != null) await channel.ModifyAsync(x => x.Topic = (string)command.Parameters[0]);
            }
        }

        private async Task HandleChannelAdminAsync(RemoteClient command)
        {
            foreach (var channelId in Program.Config.Channels[ServerNumber].Logs.AdminChat)
            {
                var channel = Guild.GetTextChannel(channelId.Id);
                if (channel != null) await channel.SendMessageAsync((string)command.Parameters[0]);
            }
        }

        private async Task DequeueMessagesAsync()
        {
            while (true)
            {
                List<KeyValuePair<LogChannel, string>> toSend;

                lock (Messages)
                {
                    toSend = Messages.ToList();
                    Messages.Clear();
                }

                foreach (var (channel, value) in toSend)
                {
                    try
                    {
                        if (value.Length > 1900)
                        {
                            var parts = value.Split('\n');
                            string chunk = string.Empty;
                            int i = 0;

                            while (chunk.Length < 1900 && i < parts.Length)
                                chunk += parts[i++] + '\n';

                            await SendLogMessage(channel, chunk);
                            Messages[channel] = value.Substring(chunk.Length);
                        }
                        else await SendLogMessage(channel, value);
                    }
                    catch (Exception e)
                    {
                        await LogService.Error(nameof(DequeueMessagesAsync),
                            $"{e.Message}\nLikely invalid ChannelId {channel.Id} or GuildId {Program.Config.DiscordServerIds[ServerNumber]}");
                        await LogService.Debug(nameof(DequeueMessagesAsync), value);
                    }
                }

                await Task.Delay(Program.Config.MessageDelay);
            }
        }

        private Task SendLogMessage(LogChannel channel, string content)
        {
            return channel.LogType switch
            {
                LogType.Embed => Guild.GetTextChannel(channel.Id)?.SendMessageAsync(
                    embed: EmbedBuilderService.CreateBasicEmbed($"Server {ServerNumber} Logs", content, Color.Green)) ?? Task.CompletedTask,
                LogType.Text => Guild.GetTextChannel(channel.Id)?.SendMessageAsync($"[{ServerNumber}]: {content}") ?? Task.CompletedTask,
                _ => throw new ArgumentOutOfRangeException()
            };
        }

        private async Task GuildMemberUpdated(Cacheable<SocketGuildUser, ulong> oldCache, SocketGuildUser newMember)
        {
            string discordId = newMember.Id.ToString();
            var steamEntry = Program.Users.SteamToDiscord.FirstOrDefault(x => x.Value == discordId);
            if (string.IsNullOrEmpty(steamEntry.Key)) return;

            string steamId = steamEntry.Key;
            var oldMember = await oldCache.GetOrDownloadAsync();
            if (oldMember == null) return;

            var oldRoles = oldMember.Roles.Select(r => r.Id).ToHashSet();
            var newRoles = newMember.Roles.Select(r => r.Id).ToHashSet();

            foreach (var (roleId, roleName) in Program.Config.DiscordAutomaticRoles)
            {
                if (!oldRoles.Contains(roleId) && newRoles.Contains(roleId))
                    await Server.SendAsync(new RemoteClient(ActionType.AutomaticRoles, $"/pm setgroup {steamId} {roleName}"));
                if (oldRoles.Contains(roleId) && !newRoles.Contains(roleId))
                    await Server.SendAsync(new RemoteClient(ActionType.AutomaticRoles, $"/pm setgroup {steamId} -1"));
            }
        }

        private async Task UserLeft(SocketGuild guild, SocketUser user)
        {
            string discordId = user.Id.ToString();
            var steamEntry = Program.Users.SteamToDiscord.FirstOrDefault(x => x.Value == discordId);
            if (string.IsNullOrEmpty(steamEntry.Key)) return;

            string steamId = steamEntry.Key;
            await Server.SendAsync(new RemoteClient(ActionType.AutomaticRoles, $"/pm setgroup {steamId} -1"));
        }
    }
}