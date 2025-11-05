using Discord_Module.API;
using Discord_Module.API.Other;
using Discord_Module.Events;
using DiscordModuleDependency;
using HarmonyLib;
using LabApi.Events.CustomHandlers;
using LabApi.Loader.Features.Plugins;
using System;
using System.Reflection;
using System.Threading;
using UnityEngine;

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
        public static CancellationTokenSource _cts;
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
            _lang.Save(Config.code);
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
            Application.logMessageReceived += HandleLog;
        }
        public override void Disable()
        {
            Instance = null;
            _client = null;
            PlayerEvents = null;
            MapEvents = null;
            ServerEvents = null;
            CustomHandlersManager.UnregisterEventsHandler(MapEvents);
            CustomHandlersManager.UnregisterEventsHandler(PlayerEvents);
            CustomHandlersManager.UnregisterEventsHandler(ServerEvents);
            Application.logMessageReceived -= HandleLog;
            _client?.Shutdown();
        }

        void HandleLog(string logString, string stackTrace, UnityEngine.LogType type)
        {
            if (type == UnityEngine.LogType.Error || type == UnityEngine.LogType.Exception)
            {
                LogError($"[LogCatcher] Error: {logString}\nStack: {stackTrace}");
            }
        }

        private static void LogError(string message)
        {
            _ = PluginStart._client.TransmitAsync(new RemoteClient(ActionType.Log, ChannelType.Errors, message));
        }
    }
}