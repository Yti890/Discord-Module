using CommandSystem.Commands.RemoteAdmin.PermissionsManagement.Group;
using Discord_Module.API.Commands;
using Discord_Module.API.Other;
using DiscordModuleDependency;
using LabApi.Features.Console;
using LabApi.Features.Wrappers;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Discord_Module.API
{
    public class TcpClientSCPSL : IDisposable
    {
        public const int ReceiveBufferSize = 4080;
        private bool disposed;
        private bool readyToSend;
        public TcpClientSCPSL(string ip, ushort port, TimeSpan retryInterval)
            : this(new IPEndPoint(IPAddress.Parse(ip), port), retryInterval)
        {
        }

        public TcpClientSCPSL(IPEndPoint endpoint, TimeSpan retryInterval)
        {
            Endpoint = endpoint;
            RetryInterval = retryInterval;
        }
        TcpClientSCPSL() => Dispose(false);
        public TcpClient? Client { get; private set; }
        public IPEndPoint Endpoint { get; private set; }
        public bool Connected => Client?.Connected ?? false;
        public TimeSpan RetryInterval { get; private set; }
        public JsonSerializerSettings SerializerSettings { get; } = new JsonSerializerSettings
        {
            ContractResolver = new CamelCasePropertyNamesContractResolver(),
            TypeNameHandling = TypeNameHandling.Objects,
        };
        public async Task Initiate(CancellationTokenSource cancelSource)
        {
            if (disposed)
                throw new ObjectDisposedException(GetType().FullName);

            await MaintainConnection(cancelSource).ContinueWith(t =>
            {
                if (t.IsFaulted)
                    Logger.Error($"[CON] {string.Format(PluginStart._lang.UpdatingConnectionError, PluginStart.Instance.Config.Debug ? t.Exception.ToString() : t.Exception.Message)}");
                else
                    Logger.Warn($"[CON] {PluginStart._lang.ServerHasBeenTerminated}");

                cancelSource.Cancel();
                cancelSource.Dispose();
                cancelSource = new CancellationTokenSource();
                _ = this.Initiate(cancelSource);
            }).ConfigureAwait(false);
            await UpdateBotStots(cancelSource).ConfigureAwait(false);
            await UpdateBotTopic(cancelSource).ConfigureAwait(false);
        }
        public void Shutdown() => Dispose();
        public async ValueTask TransmitAsync<T>(T data) => await TransmitAsync(data, CancellationToken.None);
        public async ValueTask TransmitAsync<T>(T data, CancellationToken cancelToken)
        {
            try
            {
                if (!readyToSend)
                    return;

                int attempts = 0;
                while (!Connected)
                {
                    readyToSend = false;
                    attempts++;
                    await Task.Delay(500, cancelToken);
                    if (attempts >= 50)
                    {
                        Logger.Warn($"{nameof(TransmitAsync)}: {PluginStart._lang.ConnectingError}");
                        Dispose();
                        return;
                    }
                }

                string jsonData = JsonConvert.SerializeObject(data, SerializerSettings);
                byte[] sendBytes = Encoding.UTF8.GetBytes(jsonData + '\0');
                await Client!.GetStream().WriteAsync(sendBytes, 0, sendBytes.Length, cancelToken);
                Logger.Debug(string.Format("Data Send", jsonData, sendBytes.Length), PluginStart.Instance.Config.Debug);
            }
            catch (Exception ex) when (ex.GetType() != typeof(OperationCanceledException))
            {
                Logger.Error($"[CON] {string.Format(PluginStart._lang.SendingDataError, PluginStart.Instance.Config.Debug ? ex.ToString() : ex.Message)}");
                Client?.Dispose();
            }
        }
        public void Dispose() => Dispose(true);
        protected virtual void Dispose(bool disposeAll)
        {
            if (disposeAll)
            {
                Client?.Dispose();
                Client = null;
                Endpoint = null;
            }
            disposed = true;
            GC.SuppressFinalize(this);
        }

        private async Task ListenAsync(CancellationTokenSource cancelSource)
        {
            StringBuilder accumulatedData = new();
            byte[] buffer = new byte[ReceiveBufferSize];
            while (true)
            {
                try
                {
                    if (Client == null)
                        return;
                    Task<int> readOp = Client.GetStream().ReadAsync(buffer, 0, buffer.Length, cancelSource.Token);
                    await Task.WhenAny(readOp, Task.Delay(10000, cancelSource.Token)).ConfigureAwait(false);
                    cancelSource.Token.ThrowIfCancellationRequested();
                    int readBytes = await readOp;
                    if (readBytes > 0)
                    {
                        string incomingData = Encoding.UTF8.GetString(buffer, 0, readBytes);
                        accumulatedData.Append(incomingData);
                        string data = accumulatedData.ToString();
                        int nullPos;
                        while ((nullPos = data.IndexOf('\0')) >= 0)
                        {
                            var message = data.Substring(0, nullPos);
                            data = data.Substring(nullPos + 1);
                            if (!string.IsNullOrWhiteSpace(message))
                            {
                                try
                                {
                                    JsonConvert.DeserializeObject<object>(message);
                                    Logger.Debug($"[CON] {string.Format(PluginStart._lang.ReceivedData, message, message.Length)}", PluginStart.Instance.Config.Debug);
                                    RemoteClient cmd = JsonConvert.DeserializeObject<RemoteClient>(message, SerializerSettings);
                                    Logger.Debug($"[CON] {string.Format(PluginStart._lang.HandlingRemoteClient, cmd.Action, cmd.Parameters[0], Client?.Client?.RemoteEndPoint)}", PluginStart.Instance.Config.Debug);

                                    switch (cmd.Action)
                                    {
                                        case ActionType.ExecuteCommand:
                                            GameCommand command = new GameCommand(cmd.Parameters[0].ToString(), cmd.Parameters[1].ToString(), new DiscordUser(cmd.Parameters[2].ToString(), cmd.Parameters[3].ToString(), cmd.Parameters[1].ToString()));
                                            command?.Execute();
                                            break;
                                        case ActionType.CommandReply:
                                            JsonConvert.DeserializeObject<CommandReply>(cmd.Parameters[0].ToString(), SerializerSettings)?.Answer();
                                            break;
                                        case ActionType.AdminMessage:
                                            foreach (var plr in Player.List)
                                            {
                                                if (PermissionsHandler.IsPermitted(plr.ReferenceHub.serverRoles.Permissions, PlayerPermissions.AdminChat) == true)
                                                {
                                                    plr.SendConsoleMessage(cmd.Parameters[0].ToString(), UnityEngine.Color.green.ToString());
                                                    plr.SendBroadcast(cmd.Parameters[0].ToString(), 10, global::Broadcast.BroadcastFlags.Normal, false);
                                                }
                                            }
                                            break;
                                        case ActionType.AutomaticRoles:
                                            Server.RunCommand(cmd.Parameters[0].ToString());
                                            break;
                                    }
                                }
                                catch (JsonException jsonEx)
                                {
                                    Logger.Warn($"[CON] Partial or invalid JSON received: {message}. Error: {jsonEx.Message}");
                                }
                                catch (Exception ex)
                                {
                                    Logger.Error($"[CON] {string.Format(PluginStart._lang.HandlingRemoteClientError, PluginStart.Instance.Config.Debug ? ex.ToString() : ex.Message)}");
                                }
                            }
                        }
                        accumulatedData.Clear();
                        accumulatedData.Append(data);
                    }
                    else
                    {
                        throw new IOException("Connection closed by remote host.");
                    }
                }
                catch (Exception ex) when (ex is not OperationCanceledException)
                {
                    Logger.Error($"[CON] Receive error: {ex.Message}");
                    Client?.Dispose();
                    return;
                }
            }
        }
        private async Task MaintainConnection(CancellationTokenSource cancelSource)
        {
            while (!cancelSource.IsCancellationRequested)
            {
                try
                {
                    if (Client is { Connected: true })
                    {
                        await Task.Delay(1000, cancelSource.Token);
                        continue;
                    }

                    Logger.Warn($"[CON] {PluginStart._lang.AttemptingReconnect}");

                    IPAddress addr;
                    if (!IPAddress.TryParse(PluginStart.Instance.Config.IPAddress, out addr))
                    {
                        Logger.Error($"[CON] {PluginStart._lang.InvalidIPAddress}, {PluginStart.Instance.Config.IPAddress}");
                        await Task.Delay(RetryInterval, cancelSource.Token);
                        continue;
                    }

                    ushort p = PluginStart.Instance.Config.Port;
                    Endpoint = new IPEndPoint(addr, p);

                    Client?.Dispose();
                    Client = new TcpClient();
                    Logger.Warn($"[CON] {string.Format(PluginStart._lang.ConnectingTo, addr, p)}");
                    await Client.ConnectAsync(Endpoint.Address, Endpoint.Port);

                    readyToSend = true;
                    Logger.Info($"[CON] {string.Format(PluginStart._lang.SuccessfullyConnected, Endpoint?.Address, Endpoint?.Port)}");
                    await TransmitAsync(new RemoteClient(ActionType.Log, ChannelType.GameEvents, PluginStart._lang.ServerConnected));
                    await ListenAsync(cancelSource);
                }
                catch (IOException ioEx)
                {
                    Logger.Error($"[CON] {string.Format(PluginStart._lang.ReceivingDataError, PluginStart.Instance.Config.Debug ? ioEx.ToString() : ioEx.Message)}");
                }
                catch (SocketException sockEx)
                {
                    Logger.Warn($"[CON] {string.Format(PluginStart._lang.ConnectingError, PluginStart.Instance.Config.Debug ? sockEx.ToString() : sockEx.Message)}");
                }
                catch (Exception ex) when (ex is not OperationCanceledException)
                {
                    Logger.Error($"[CON] {string.Format(PluginStart._lang.UpdatingConnectionError, PluginStart.Instance.Config.Debug ? ex.ToString() : ex.Message)}");
                }

                readyToSend = false;
                Client?.Dispose();
                Logger.Warn($"[CON] {PluginStart._lang.RetryingConnectionIn} {RetryInterval.TotalSeconds}s...");
                await Task.Delay(RetryInterval, cancelSource.Token);
            }
        }
        private async Task UpdateBotTopic(CancellationTokenSource cancelSource)
        {
            while (!cancelSource.IsCancellationRequested)
            {
                try
                {
                    int aliveHumans = Player.List.Count(player => player.IsAlive && player.IsHuman);
                    int aliveScps = Player.List.Count(x => x.IsSCP);

                    string warheadText = Warhead.IsDetonated ? PluginStart._lang.WarheadHasBeenDetonated : Warhead.IsDetonationInProgress ? PluginStart._lang.WarheadIsCountingToDetonation : PluginStart._lang.WarheadHasntBeenDetonated;

                    await TransmitAsync(new RemoteClient(ActionType.UpdateChannelActivity, $"{string.Format(PluginStart._lang.PlayersOnline, Player.Dictionary.Count, Server.MaxPlayers)}. {string.Format(PluginStart._lang.RoundDuration, Round.Duration)}. {string.Format(PluginStart._lang.AliveHumans, aliveHumans)}. {string.Format(PluginStart._lang.AliveScps, aliveScps)}. {warheadText} IP: {Server.IpAddress}:{Server.Port} TPS: {Server.Tps}"), cancelSource.Token);
                }
                catch (Exception exception)
                {
                    Logger.Error(string.Format(PluginStart._lang.CouldNotUpdateChannelTopicError, exception));
                }

                await Task.Delay(TimeSpan.FromSeconds(15), cancelSource.Token);
            }
        }
        private async Task UpdateBotStots(CancellationTokenSource cancelSource)
        {
            while (!cancelSource.IsCancellationRequested)
            {
                try
                {
                    await TransmitAsync(new RemoteClient(ActionType.UpdateActivity, $"{Player.Count}/{Server.MaxPlayers}"), cancelSource.Token);
                    await Task.Delay(TimeSpan.FromSeconds(3), cancelSource.Token);
                }
                catch (Exception)
                {
                    await Task.Delay(TimeSpan.FromSeconds(3), cancelSource.Token);
                }
            }
        }
    }
}