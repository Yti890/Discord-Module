using CommandSystem.Commands.RemoteAdmin.PermissionsManagement.Group;
using Discord_Module.API.Other;
using DiscordModuleDependency;
using LabApi.Events.Arguments.Scp914Events;
using LabApi.Events.Arguments.ServerEvents;
using LabApi.Events.Arguments.WarheadEvents;
using LabApi.Events.CustomHandlers;
using LabApi.Features.Wrappers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static PlayerList;

namespace Discord_Module.Events
{
    internal class MapEvents : CustomEventsHandler
    {
        public static int GeneratorCount;
        public override async void OnWarheadDetonated(WarheadDetonatedEventArgs ev)
        {
            await PluginStart._client.TransmitAsync(new RemoteClient(ActionType.Log, ChannelType.GameEvents, PluginStart._lang.WarheadHasDetonated)).ConfigureAwait(false);
            base.OnWarheadDetonated(ev);
        }
        public override async void OnServerGeneratorActivated(GeneratorActivatedEventArgs ev)
        {
            await PluginStart._client.TransmitAsync(new RemoteClient(ActionType.Log, ChannelType.GameEvents, string.Format(PluginStart._lang.GeneratorFinished, ev.Generator.Room, GeneratorCount))).ConfigureAwait(false);
            base.OnServerGeneratorActivated(ev);
        }
        public override async void OnServerLczDecontaminationStarting(LczDecontaminationStartingEventArgs ev)
        {
            await PluginStart._client.TransmitAsync(new RemoteClient(ActionType.Log, ChannelType.GameEvents, PluginStart._lang.DecontaminationHasBegun)).ConfigureAwait(false);
            base.OnServerLczDecontaminationStarting(ev);
        }
        public override async void OnWarheadStarting(WarheadStartingEventArgs ev)
        {
            if ((ev.Player == null || (ev.Player != null && (!ev.Player.DoNotTrack))))
            {
                object[] vars = ev.Player == null ?
                    new object[] { Warhead.DetonationTime } :
                    new object[] { ev.Player.Nickname, ev.Player.UserId, ev.Player.Role, Warhead.DetonationTime };

                await PluginStart._client.TransmitAsync(new RemoteClient(ActionType.Log, ChannelType.GameEvents, string.Format(ev.Player == null ? PluginStart._lang.WarheadStarted : PluginStart._lang.PlayerWarheadStarted, vars))).ConfigureAwait(false);
            }
            base.OnWarheadStarting(ev);
        }
        public override async void OnWarheadStopping(WarheadStoppingEventArgs ev)
        {
            if ((ev.Player == null || (ev.Player != null && (!ev.Player.DoNotTrack))))
            {
                object[] vars = ev.Player == null ?
                    Array.Empty<object>() :
                    new object[] { ev.Player.Nickname, ev.Player.UserId, ev.Player.Role };

                await PluginStart._client.TransmitAsync(new RemoteClient(ActionType.Log, ChannelType.GameEvents, string.Format(ev.Player == null ? PluginStart._lang.CanceledWarhead : PluginStart._lang.PlayerCanceledWarhead, vars))).ConfigureAwait(false);
            }
            base.OnWarheadStopping(ev);
        }
        public override async void OnScp914ProcessedPickup(Scp914ProcessedPickupEventArgs ev)
        {
            await PluginStart._client.TransmitAsync(new RemoteClient(ActionType.Log, ChannelType.GameEvents, string.Format(PluginStart._lang.Scp914ProcessedItem, ev.Pickup.Type)));
            base.OnScp914ProcessedPickup(ev);
        }
    }
}
