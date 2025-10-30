using Discord_Module;
using DiscordModuleDependency;
using HarmonyLib;
using LabApi.Features.Wrappers;
using PlayerRoles;
using RemoteAdmin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;

[HarmonyPatch(typeof(QueryProcessor), "ProcessGameConsoleQuery")]
internal static class ClientCommands
{
    private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        var codes = instructions.ToList();
        var logMethod = typeof(ClientCommands).GetMethod(nameof(LogCommand),
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
        codes.InsertRange(0, new[]
        {
            new CodeInstruction(OpCodes.Ldarg_0),
            new CodeInstruction(OpCodes.Ldarg_1),
            new CodeInstruction(OpCodes.Call, logMethod)
        });

        return codes;
    }
    private static void LogCommand(QueryProcessor processor, string query)
    {
        if (string.IsNullOrWhiteSpace(query))
            return;

        string[] args = query.Trim().Split(new[] { ' ' }, 512, StringSplitOptions.RemoveEmptyEntries);
        if (args.Length == 0 || args[0].StartsWith("$"))
            return;

        var player = Server.Host;
        string senderNickname = PluginStart._lang.DedicatedServer;
        string senderId = PluginStart._lang.DedicatedServer;

        if (processor.TryGetSender(out var sender))
        {
            senderNickname = sender.Nickname ?? senderNickname;
            senderId = sender.SenderId ?? senderNickname;

            if (sender is PlayerCommandSender pcs)
                player = Player.Get(pcs) ?? Server.Host;
        }

        string commandName = args[0];
        string commandArgs = string.Join(" ", args.Skip(1));

        string message = string.Format(
            PluginStart._lang.UsedCommand,
            senderNickname,
            senderId,
            player?.Role.GetTeam() ?? Team.Dead,
            commandName,
            commandArgs);

        _ = PluginStart._client.TransmitAsync(new RemoteClient(ActionType.Log, ChannelType.Command, message));
    }
}