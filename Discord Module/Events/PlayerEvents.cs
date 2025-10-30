using DiscordModuleDependency;
using LabApi.Events.Arguments.PlayerEvents;
using LabApi.Events.Arguments.Scp079Events;
using LabApi.Events.Arguments.Scp106Events;
using LabApi.Events.Arguments.Scp914Events;
using LabApi.Events.CustomHandlers;
using PlayerRoles;
using System;

namespace Discord_Module.Events
{
    public class PlayerEvents : CustomEventsHandler
    {
        public override async void OnPlayerActivatedGenerator(PlayerActivatedGeneratorEventArgs ev)
        {
            if (!ev.Player.DoNotTrack)
                await PluginStart._client.TransmitAsync(new RemoteClient(ActionType.Log, ChannelType.GameEvents, string.Format(PluginStart._lang.GeneratorInserted, ev.Player.Nickname, ev.Player.UserId, ev.Player.Role))).ConfigureAwait(false);
            base.OnPlayerActivatedGenerator(ev);
        }
        public override async void OnPlayerOpeningGenerator(PlayerOpeningGeneratorEventArgs ev)
        {
            if (!ev.Player.DoNotTrack)
                await PluginStart._client.TransmitAsync(new RemoteClient(ActionType.Log, ChannelType.GameEvents, string.Format(PluginStart._lang.GeneratorOpened, ev.Player.Nickname, ev.Player.UserId, ev.Player.Role))).ConfigureAwait(false);
            base.OnPlayerOpeningGenerator(ev);
        }
        public override async void OnPlayerUnlockedGenerator(PlayerUnlockedGeneratorEventArgs ev)
        {
            if (!ev.Player.DoNotTrack)
                await PluginStart._client.TransmitAsync(new RemoteClient(ActionType.Log, ChannelType.GameEvents, string.Format(PluginStart._lang.GeneratorUnlocked, ev.Player.Nickname, ev.Player.UserId, ev.Player.Role))).ConfigureAwait(false);
            base.OnPlayerUnlockedGenerator(ev);
        }
        public async override void OnScp106TeleportingPlayer(Scp106TeleportingPlayerEvent ev)
        {
            if (!ev.Player.DoNotTrack)
                await PluginStart._client.TransmitAsync(new RemoteClient(ActionType.Log, ChannelType.GameEvents, string.Format(PluginStart._lang.Scp106CreatedPortal, ev.Player.Nickname, ev.Player.UserId, ev.Player.Role))).ConfigureAwait(false);
            base.OnScp106TeleportingPlayer(ev);
        }
        public override async void OnPlayerChangedItem(PlayerChangedItemEventArgs ev)
        {
            if (!ev.Player.DoNotTrack)
                await PluginStart._client.TransmitAsync(new RemoteClient(ActionType.Log, ChannelType.GameEvents, string.Format(PluginStart._lang.ItemChanged, ev.Player.Nickname, ev.Player.UserId, ev.Player.CurrentItem.Type, ev.OldItem.Type))).ConfigureAwait(false);
            base.OnPlayerChangedItem(ev);
        }
        public override async void OnScp079GainedExperience(Scp079GainedExperienceEventArgs ev)
        {
            if (!ev.Player.DoNotTrack)
                await PluginStart._client.TransmitAsync(new RemoteClient(ActionType.Log, ChannelType.GameEvents, string.Format(PluginStart._lang.GainedExperience, ev.Player.Nickname, ev.Player.UserId, ev.Player.Role, ev.Amount, ev.Reason))).ConfigureAwait(false);
            base.OnScp079GainedExperience(ev);
        }
        public override async void OnScp079LeveledUp(Scp079LeveledUpEventArgs ev)
        {
            if (!ev.Player.DoNotTrack)
                await PluginStart._client.TransmitAsync(new RemoteClient(ActionType.Log, ChannelType.GameEvents, string.Format(PluginStart._lang.GainedLevel, ev.Player.Nickname, ev.Player.UserId, ev.Player.Role, ev.Tier - 1, ev.Tier))).ConfigureAwait(false);
            base.OnScp079LeveledUp(ev);
        }
        public override async void OnPlayerLeft(PlayerLeftEventArgs ev)
        {
            if (!ev.Player.DoNotTrack)
                await PluginStart._client.TransmitAsync(new RemoteClient(ActionType.Log, ChannelType.GameEvents, string.Format(PluginStart._lang.LeftServer, ev.Player.Nickname, ev.Player.UserId, ev.Player.Role))).ConfigureAwait(false);
            base.OnPlayerLeft(ev);
        }
        public override async void OnPlayerReloadingWeapon(PlayerReloadingWeaponEventArgs ev)
        {
            if (!ev.Player.DoNotTrack)
                await PluginStart._client.TransmitAsync(new RemoteClient(ActionType.Log, ChannelType.GameEvents, string.Format(PluginStart._lang.Reloaded, ev.Player.Nickname, ev.Player.UserId, ev.Player.CurrentItem.Type, ev.Player.Role))).ConfigureAwait(false);
            base.OnPlayerReloadingWeapon(ev);
        }
        public override async void OnPlayerUnlockedWarheadButton(PlayerUnlockedWarheadButtonEventArgs ev)
        {
            if (!ev.Player.DoNotTrack)
                await PluginStart._client.TransmitAsync(new RemoteClient(ActionType.Log, ChannelType.GameEvents, string.Format(PluginStart._lang.AccessedWarhead, ev.Player.Nickname, ev.Player.UserId, ev.Player.Role))).ConfigureAwait(false);
            base.OnPlayerUnlockedWarheadButton(ev);
        }
        public override async void OnPlayerInteractedElevator(PlayerInteractedElevatorEventArgs ev)
        {
            if (!ev.Player.DoNotTrack)
                await PluginStart._client.TransmitAsync(new RemoteClient(ActionType.Log, ChannelType.GameEvents, string.Format(PluginStart._lang.CalledElevator, ev.Player.Nickname, ev.Player.UserId, ev.Player.Role))).ConfigureAwait(false);
            base.OnPlayerInteractedElevator(ev);
        }
        public override async void OnPlayerInteractedLocker(PlayerInteractedLockerEventArgs ev)
        {
            if (!ev.Player.DoNotTrack)
                await PluginStart._client.TransmitAsync(new RemoteClient(ActionType.Log, ChannelType.GameEvents, string.Format(PluginStart._lang.UsedLocker, ev.Player.Nickname, ev.Player.UserId, ev.Player.Role))).ConfigureAwait(false);
            base.OnPlayerInteractedLocker(ev);
        }
        public override async void OnPlayerTriggeredTesla(PlayerTriggeredTeslaEventArgs ev)
        {
            if (PluginStart.Instance.Config.PlayerTriggeringTesla && (!ev.Player.DoNotTrack))
                await PluginStart._client.TransmitAsync(new RemoteClient(ActionType.Log, ChannelType.GameEvents, string.Format(PluginStart._lang.HasTriggeredATeslaGate, ev.Player.Nickname, ev.Player.UserId, ev.Player.Role))).ConfigureAwait(false);
            base.OnPlayerTriggeredTesla(ev);
        }
        public override async void OnPlayerClosingGenerator(PlayerClosingGeneratorEventArgs ev)
        {
            if (!ev.Player.DoNotTrack)
                await PluginStart._client.TransmitAsync(new RemoteClient(ActionType.Log, ChannelType.GameEvents, string.Format(PluginStart._lang.GeneratorClosed, ev.Player.Nickname, ev.Player.UserId, ev.Player.Role))).ConfigureAwait(false);
            base.OnPlayerClosingGenerator(ev);
        }
        public override async void OnPlayerDeactivatedGenerator(PlayerDeactivatedGeneratorEventArgs ev)
        {
            if (!ev.Player.DoNotTrack)
                await PluginStart._client.TransmitAsync(new RemoteClient(ActionType.Log, ChannelType.GameEvents, string.Format(PluginStart._lang.GeneratorEjected, ev.Player.Nickname, ev.Player.UserId, ev.Player.Role))).ConfigureAwait(false);
            base.OnPlayerDeactivatedGenerator(ev);
        }
        public override async void OnPlayerInteractedDoor(PlayerInteractedDoorEventArgs ev)
        {
            if (!ev.Player.DoNotTrack)
                await PluginStart._client.TransmitAsync(new RemoteClient(ActionType.Log, ChannelType.GameEvents, string.Format(ev.Door.IsOpened ? PluginStart._lang.HasClosedADoor : PluginStart._lang.HasOpenedADoor, ev.Player.Nickname, ev.Player.UserId, ev.Player.Role, ev.Door.NameTag))).ConfigureAwait(false);
            base.OnPlayerInteractedDoor(ev);
        }
        public override async void OnScp914Activated(Scp914ActivatedEventArgs ev)
        {
            if (!ev.Player.DoNotTrack)
                await PluginStart._client.TransmitAsync(new RemoteClient(ActionType.Log, ChannelType.GameEvents, string.Format(PluginStart._lang.Scp914HasBeenActivated, ev.Player.Nickname, ev.Player.UserId, ev.Player.Role, ev.KnobSetting))).ConfigureAwait(false);
            base.OnScp914Activated(ev);
        }
        public override async void OnScp914KnobChanged(Scp914KnobChangedEventArgs ev)
        {
            if (!ev.Player.DoNotTrack)
                await PluginStart._client.TransmitAsync(new RemoteClient(ActionType.Log, ChannelType.GameEvents, string.Format(PluginStart._lang.Scp914KnobSettingChanged, ev.Player.Nickname, ev.Player.UserId, ev.Player.Role, ev.KnobSetting))).ConfigureAwait(false);
            base.OnScp914KnobChanged(ev);
        }
        public override async void OnPlayerEnteredPocketDimension(PlayerEnteredPocketDimensionEventArgs ev)
        {
            if (!ev.Player.DoNotTrack)
                await PluginStart._client.TransmitAsync(new RemoteClient(ActionType.Log, ChannelType.GameEvents, string.Format(PluginStart._lang.HasEnteredPocketDimension, ev.Player.Nickname, ev.Player.UserId, ev.Player.Role))).ConfigureAwait(false);
            base.OnPlayerEnteredPocketDimension(ev);
        }
        public override async void OnPlayerEscaped(PlayerEscapedEventArgs ev)
        {
            if (!ev.Player.DoNotTrack)
                await PluginStart._client.TransmitAsync(new RemoteClient(ActionType.Log, ChannelType.GameEvents, string.Format(PluginStart._lang.HasEscapedPocketDimension, ev.Player.Nickname, ev.Player.UserId, ev.Player.Role))).ConfigureAwait(false);
            base.OnPlayerEscaped(ev);
        }
        public override async void OnScp079UsedTesla(Scp079UsedTeslaEventArgs ev)
        {
            if (!ev.Player.DoNotTrack)
                await PluginStart._client.TransmitAsync(new RemoteClient(ActionType.Log, ChannelType.GameEvents, string.Format(PluginStart._lang.HasTriggeredATeslaGate, ev.Player.Nickname, ev.Player.UserId, ev.Player.Role))).ConfigureAwait(false);
            base.OnScp079UsedTesla(ev);
        }
        public override async void OnPlayerHurting(PlayerHurtingEventArgs ev)
        {
            if (ev.Player != null && (ev.Attacker == null || ev.Attacker.Role == ev.Player.Role) && (ev.Attacker == null || (ev.Attacker.DoNotTrack && !ev.Player.DoNotTrack)) && (ev.Attacker != null))
                await PluginStart._client.TransmitAsync(new RemoteClient(ActionType.Log, ChannelType.GameEvents, string.Format(PluginStart._lang.HasDamagedForWith, ev.Attacker != null ? ev.Attacker.Nickname : "Server", (ev.Attacker != null ? ev.Attacker.UserId : ""), ev.Attacker?.Role ?? RoleTypeId.None, ev.Player.Nickname, ev.Player.UserId, ev.Player.Role))).ConfigureAwait(false);

            base.OnPlayerHurting(ev);
        }
        public override async void OnPlayerDeath(PlayerDeathEventArgs ev)
        {
            if (ev.Player != null && (ev.Attacker == null || ev.Attacker.Role == ev.Player.Role) && ((ev.Attacker == null || (!ev.Attacker.DoNotTrack && !ev.Player.DoNotTrack))))
                await PluginStart._client.TransmitAsync(new RemoteClient(ActionType.Log, ChannelType.GameEvents, string.Format(PluginStart._lang.HasKilledWith, ev.Attacker != null ? ev.Attacker.Nickname : "Server", ev.Attacker != null ? ev.Attacker.UserId : string.Empty, ev.Player?.Role ?? RoleTypeId.None, ev.Player.Nickname, ev.Player.UserId, ev.Player.Role))).ConfigureAwait(false);
            base.OnPlayerDeath(ev);
        }
        public override async void OnPlayerThrowingItem(PlayerThrowingItemEventArgs ev)
        {
            if (ev.Player != null && (!ev.Player.DoNotTrack))
                await PluginStart._client.TransmitAsync(new RemoteClient(ActionType.Log, ChannelType.GameEvents, string.Format(PluginStart._lang.ThrewAGrenade, ev.Player.Nickname, ev.Player.UserId, ev.Player.Role, ev.Pickup.Type))).ConfigureAwait(false);
            base.OnPlayerThrowingItem(ev);
        }
        public override async void OnPlayerChangingRole(PlayerChangingRoleEventArgs ev)
        {
            if (ev.Player != null  && (!ev.Player.DoNotTrack ))
                await PluginStart._client.TransmitAsync(new RemoteClient(ActionType.Log, ChannelType.GameEvents, string.Format(PluginStart._lang.ChangedRole, ev.Player.Nickname, ev.Player.UserId, ev.Player.Role, ev.NewRole))).ConfigureAwait(false);
            base.OnPlayerChangingRole(ev);
        }

        public override async void OnPlayerJoined(PlayerJoinedEventArgs ev)
        {
            if (!ev.Player.DoNotTrack)
                await PluginStart._client.TransmitAsync(new RemoteClient(ActionType.Log, ChannelType.GameEvents, string.Format(PluginStart._lang.HasJoinedTheGame, ev.Player.Nickname, ev.Player.UserId, PluginStart.Instance.Config.ShouldLogIPAddresses ? ev.Player.IpAddress : PluginStart._lang.Redacted))).ConfigureAwait(false);
            base.OnPlayerJoined(ev);
        }
        public override async void OnPlayerUncuffed(PlayerUncuffedEventArgs ev)
        {
            if (!ev.Player.DoNotTrack && !ev.Target.DoNotTrack)
                await PluginStart._client.TransmitAsync(new RemoteClient(ActionType.Log, ChannelType.GameEvents, string.Format(PluginStart._lang.HasBeenFreedBy, ev.Target.Nickname, ev.Target.UserId, ev.Target.Role, ev.Player.Nickname, ev.Player.UserId, ev.Player.Role))).ConfigureAwait(false);
            base.OnPlayerUncuffed(ev);
        }
        public override async void OnPlayerCuffed(PlayerCuffedEventArgs ev)
        {
            if (!ev.Player.DoNotTrack && !ev.Target.DoNotTrack)
                await PluginStart._client.TransmitAsync(new RemoteClient(ActionType.Log, ChannelType.GameEvents, string.Format(PluginStart._lang.HasBeenHandcuffedBy, ev.Target.Nickname, ev.Target.UserId, ev.Target.Role, ev.Player.Nickname, ev.Player.UserId, ev.Player.Role))).ConfigureAwait(false);
            base.OnPlayerCuffed(ev);
        }
        public override async void OnPlayerKicked(PlayerKickedEventArgs ev)
        {
            await PluginStart._client.TransmitAsync(new RemoteClient(ActionType.Log, "kicks", string.Format(PluginStart._lang.WasKicked, ev.Player?.Nickname ?? PluginStart._lang.NotAuthenticated, ev.Player?.UserId ?? PluginStart._lang.NotAuthenticated, ev.Reason))).ConfigureAwait(false);
            base.OnPlayerKicked(ev);
        }
        public override async void OnPlayerBanned(PlayerBannedEventArgs ev)
        {
            await PluginStart._client.TransmitAsync(new RemoteClient(ActionType.Log, ChannelType.Bans, string.Format(PluginStart._lang.WasBannedBy, ev.Player.Nickname, ev.Player.PlayerId, ev.Issuer, ev.Reason, new DateTime(ev.Duration).ToString(PluginStart.Instance.Config.DateFormat)))).ConfigureAwait(false);
            base.OnPlayerBanned(ev);
        }
        public override async void OnPlayerUsedIntercom(PlayerUsedIntercomEventArgs ev)
        {
            if (ev.Player != null && PluginStart.Instance.Config.PlayerIntercomSpeaking && (!ev.Player.DoNotTrack))
                await PluginStart._client.TransmitAsync(new RemoteClient(ActionType.Log, ChannelType.GameEvents, string.Format(PluginStart._lang.HasStartedUsingTheIntercom, ev.Player.Nickname, ev.Player.UserId, ev.Player.Role))).ConfigureAwait(false);
            base.OnPlayerUsedIntercom(ev);
        }
        public override async void OnPlayerPickedUpItem(PlayerPickedUpItemEventArgs ev)
        {
            if (!ev.Player.DoNotTrack)
                await PluginStart._client.TransmitAsync(new RemoteClient(ActionType.Log, ChannelType.GameEvents, string.Format(PluginStart._lang.HasPickedUp, ev.Player.Nickname, ev.Player.UserId, ev.Player.Role, ev.Item.Type))).ConfigureAwait(false);
            base.OnPlayerPickedUpItem(ev);
        }
        public override async void OnPlayerDroppedItem(PlayerDroppedItemEventArgs ev)
        {
            if (!ev.Player.DoNotTrack)
                await PluginStart._client.TransmitAsync(new RemoteClient(ActionType.Log, ChannelType.GameEvents, string.Format(PluginStart._lang.HasDropped, ev.Player.Nickname, ev.Player.UserId, ev.Player.Role, ev.Pickup.Type))).ConfigureAwait(false);
            base.OnPlayerDroppedItem(ev);
        }
        public override async void OnPlayerGroupChanged(PlayerGroupChangedEventArgs ev)
        {
            if (ev.Player != null && (!ev.Player.DoNotTrack))
                await PluginStart._client.TransmitAsync(new RemoteClient(ActionType.Log, ChannelType.GameEvents, string.Format(PluginStart._lang.GroupSet, ev.Player.Nickname, ev.Player.UserId, ev.Player.Role, ev.Group?.BadgeText ?? PluginStart._lang.None, ev.Group?.BadgeColor ?? PluginStart._lang.None))).ConfigureAwait(false);
            base.OnPlayerGroupChanged(ev);
        }
    }
}
