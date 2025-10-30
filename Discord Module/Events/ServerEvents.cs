using DiscordModuleDependency;
using LabApi.Events.Arguments.PlayerEvents;
using LabApi.Events.Arguments.ServerEvents;
using LabApi.Events.CustomHandlers;
using LabApi.Features.Wrappers;

namespace Discord_Module.Events
{
    internal class ServerEvents : CustomEventsHandler
    {
        public override async void OnPlayerReportedCheater(PlayerReportedCheaterEventArgs ev)
        {
            await PluginStart._client.TransmitAsync(new RemoteClient(ActionType.Log, ChannelType.Reports, string.Format(PluginStart._lang.CheaterReportFilled, ev.Player.Nickname, ev.Player.UserId, ev.Player.Role, ev.Target.Nickname, ev.Target.UserId, ev.Target.Role, ev.Reason))).ConfigureAwait(false);
            base.OnPlayerReportedCheater(ev);
        }
        public override async void OnPlayerReportedPlayer(PlayerReportedPlayerEventArgs ev)
        {
            await PluginStart._client.TransmitAsync(new RemoteClient(ActionType.Log, ChannelType.Reports, string.Format(PluginStart._lang.CheaterReportFilled, ev.Player.Nickname, ev.Player.UserId, ev.Player.Role, ev.Target.Nickname, ev.Target.UserId, ev.Target.Role, ev.Reason))).ConfigureAwait(false);
            base.OnPlayerReportedPlayer(ev);
        }
        public override async void OnServerWaitingForPlayers()
        {
            await PluginStart._client.TransmitAsync(new RemoteClient(ActionType.Log, ChannelType.GameEvents, PluginStart._lang.WaitingForPlayers)).ConfigureAwait(false);
            base.OnServerWaitingForPlayers();
        }
        public override async void OnServerRoundStarted()
        {
            await PluginStart._client.TransmitAsync(new RemoteClient(ActionType.Log, ChannelType.GameEvents, string.Format(PluginStart._lang.RoundStarting, Player.Count))).ConfigureAwait(false);
            base.OnServerRoundStarted();
        }
        public override async void OnServerRoundEnded(RoundEndedEventArgs ev)
        {
            await PluginStart._client.TransmitAsync(new RemoteClient(ActionType.Log, ChannelType.GameEvents, string.Format(PluginStart._lang.RoundEnded, ev.LeadingTeam, Player.Count, Server.MaxPlayers))).ConfigureAwait(false);
            base.OnServerRoundEnded(ev);
        }
        public override async void OnServerWaveRespawned(WaveRespawnedEventArgs ev)
        {
            await PluginStart._client.TransmitAsync(new RemoteClient(ActionType.Log, ChannelType.GameEvents, string.Format(ev.Wave.Faction == PlayerRoles.Faction.FoundationEnemy ? PluginStart._lang.ChaosInsurgencyHaveSpawned : PluginStart._lang.NineTailedFoxHaveSpawned, ev.Players.Count))).ConfigureAwait(false);
            base.OnServerWaveRespawned(ev);
        }
        public override async void OnServerSendingAdminChat(SendingAdminChatEventArgs ev)
        {
            await PluginStart._client.TransmitAsync(new RemoteClient(ActionType.AdminMessage, $"{Server.ServerListName} {Player.Get(ev.Sender).GroupName} {ev.Sender.Nickname} {ev.Message}")).ConfigureAwait(false);
            base.OnServerSendingAdminChat(ev);
        }
    }
}
