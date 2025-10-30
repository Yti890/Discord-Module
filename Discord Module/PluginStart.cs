using Discord_Module.API;
using Discord_Module.API.Other;
using Discord_Module.Events;
using HarmonyLib;
using LabApi.Events.CustomHandlers;
using LabApi.Loader.Features.Plugins;
using System;
using System.Reflection;
using System.Threading;

namespace Discord_Module
{
    public class PluginStart : Plugin<Config>
    {
        private Harmony _harmony;
        public override string Name => "Discord_Module";

        public override string Description => "An Experemental Plugin for integrate Discord and SCP:SL";

        public override string Author => "Yti890 or Vivian";

        public override Version Version => Assembly.GetExecutingAssembly().GetName().Version;

        public override Version RequiredApiVersion => new Version(1, 1, 4, 0);
        public static PluginStart Instance;
        public static TcpClientSCPSL _client;
        private CancellationTokenSource _cts;
        public static Language _lang;
        private MapEvents MapEvents;
        private PlayerEvents PlayerEvents;
        private ServerEvents ServerEvents;
        public override void Enable()
        {
            Instance = this;
            _harmony = new Harmony("discord.module.patches");
            _harmony.PatchAll();
            _lang = new Language();
            _lang.Save(Config.code,true);
            _lang.Load(Config.code);
            _cts = new CancellationTokenSource();
            _client = new TcpClientSCPSL(Config.IPAddress, Config.Port, TimeSpan.FromSeconds(5));
            _ = _client.Initiate(_cts);
            PlayerEvents = new PlayerEvents();
            MapEvents = new MapEvents();
            ServerEvents = new ServerEvents();
            CustomHandlersManager.RegisterEventsHandler(MapEvents);
            CustomHandlersManager.RegisterEventsHandler(PlayerEvents);
            CustomHandlersManager.RegisterEventsHandler(ServerEvents);
        }
        public override void Disable()
        {
            Instance = null;
            _client = null;
            CustomHandlersManager.UnregisterEventsHandler(MapEvents);
            CustomHandlersManager.UnregisterEventsHandler(PlayerEvents);
            CustomHandlersManager.UnregisterEventsHandler(ServerEvents);
        }
    }
}