using Discord_Module.API.Commands;
using Discord_Module.API.Other;
using DiscordModuleDependency;
using LabApi.Features.Console;
using LabApi.Features.Wrappers;
using MEC;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
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
        public const int MaxAccumulatedDataSize = 1024 * 1024;
        private const int ConnectTimeoutMs = 10_000;
        private const int SendTimeoutMs = 10_000;
        private const int ReceiveTimeoutMs = 30_000;

        private bool disposed;
        private volatile bool readyToSend;
        private bool firstrestart = false;
        private CoroutineHandle updateBotStatusCoroutine;
        private CoroutineHandle updateBotTopicCoroutine;
        private TcpClient? Client { get; set; }
        public IPEndPoint Endpoint { get; private set; }
        public bool Connected => Client?.Connected ?? false;
        public TimeSpan RetryInterval { get; private set; }
        public JsonSerializerSettings SerializerSettings { get; } = new JsonSerializerSettings
        {
            ContractResolver = new CamelCasePropertyNamesContractResolver(),
            TypeNameHandling = TypeNameHandling.Objects,
        };

        private readonly object _lock = new object();
        private Task? _listenTask;
        private readonly SemaphoreSlim _sendLock = new SemaphoreSlim(1, 1);

        public TcpClientSCPSL(string ip, ushort port, TimeSpan retryInterval)
            : this(new IPEndPoint(IPAddress.Parse(ip), port), retryInterval)
        {
        }

        public TcpClientSCPSL(IPEndPoint endpoint, TimeSpan retryInterval)
        {
            Endpoint = endpoint;
            RetryInterval = retryInterval;
        }

        ~TcpClientSCPSL() => Dispose(false);

        public async Task Initiate(CancellationTokenSource cancelSource)
        {
            lock (_lock)
            {
                if (disposed)
                    throw new ObjectDisposedException(GetType().FullName);
            }
            await MaintainConnection(cancelSource).ConfigureAwait(false);
        }

        public void Shutdown()
        {
            lock (_lock)
            {
                if (disposed)
                    return;

                Logger.Info("[CON] Shutdown initiated.");

                try
                {
                    Timing.KillCoroutines(updateBotStatusCoroutine);
                    Timing.KillCoroutines(updateBotTopicCoroutine);
                }
                catch { }
                try
                {
                    PluginStart._cts?.Cancel();
                }
                catch { }
                try
                {
                    Client?.Close();
                    Client?.Dispose();
                    Client = null;
                }
                catch { }
                readyToSend = false;
                disposed = true;
                firstrestart = false;

                Logger.Info("[CON] Shutdown complete.");
            }
        }


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
                Logger.Debug($"[CON] Data Sent ({sendBytes.Length} bytes): {jsonData}", PluginStart.Instance.Config.Debug);
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
            lock (_lock)
            {
                if (disposeAll)
                {
                    try
                    {
                        Client?.Close();
                        Client?.Dispose();
                    }
                    catch { }
                    Client = null;

                    try { Timing.KillCoroutines(updateBotStatusCoroutine); } catch { }
                    try { Timing.KillCoroutines(updateBotTopicCoroutine); } catch { }
                }
                disposed = true;
            }
            GC.SuppressFinalize(this);
        }

        private async Task ListenAsync(CancellationTokenSource cancelSource)
        {
            var buffer = new byte[ReceiveBufferSize];
            var sb = new StringBuilder();

            while (!cancelSource.IsCancellationRequested)
            {
                TcpClient? client;
                lock (_lock)
                {
                    client = Client;
                }

                if (client == null || !client.Connected)
                {
                    return;
                }

                NetworkStream? stream;
                try
                {
                    stream = client.GetStream();
                }
                catch (Exception ex)
                {
                    Logger.Warn($"[CON] Failed to get stream in ListenAsync: {ex.Message}");
                    lock (_lock)
                    {
                        client?.Dispose();
                        Client = null;
                        readyToSend = false;
                    }
                    return;
                }

                try
                {
                    using var readCts = CancellationTokenSource.CreateLinkedTokenSource(cancelSource.Token);
                    readCts.CancelAfter(ReceiveTimeoutMs);

                    int readBytes = await stream.ReadAsync(buffer, 0, buffer.Length, readCts.Token).ConfigureAwait(false);
                    if (readBytes <= 0)
                        throw new IOException("Connection closed by remote host.");

                    string incoming = Encoding.UTF8.GetString(buffer, 0, readBytes);
                    sb.Append(incoming);

                    if (sb.Length > MaxAccumulatedDataSize)
                    {
                        Logger.Warn("[CON] Accumulated data exceeded max size; clearing buffer to prevent memory issues.");
                        sb.Clear();
                        continue;
                    }

                    string total = sb.ToString();
                    int processedUpTo = 0;
                    int nullPos;
                    while ((nullPos = total.IndexOf('\0', processedUpTo)) >= 0)
                    {
                        string message = total.Substring(processedUpTo, nullPos - processedUpTo);
                        processedUpTo = nullPos + 1;

                        if (string.IsNullOrWhiteSpace(message))
                            continue;

                        try
                        {
                            RemoteClient cmd = JsonConvert.DeserializeObject<RemoteClient>(message, SerializerSettings);
                            Logger.Debug($"[CON] Handling: {cmd.Action}", PluginStart.Instance.Config.Debug);

                            switch (cmd.Action)
                            {
                                case ActionType.ExecuteCommand:
                                    var command = new GameCommand(cmd.Parameters[0].ToString(), cmd.Parameters[1].ToString(),
                                        new DiscordUser(cmd.Parameters[2].ToString(), cmd.Parameters[3].ToString(), cmd.Parameters[1].ToString()));
                                    command?.Execute();
                                    break;

                                case ActionType.CommandReply:
                                    JsonConvert.DeserializeObject<CommandReply>(cmd.Parameters[0].ToString(), SerializerSettings)?.Answer();
                                    break;

                                case ActionType.AdminMessage:
                                    foreach (var plr in Player.List ?? Enumerable.Empty<Player>())
                                    {
                                        if (plr.HasPermission(PlayerPermissions.AdminChat))
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
                            Logger.Warn($"[CON] Invalid JSON received: {message}. Error: {jsonEx.Message}");
                        }
                        catch (Exception ex)
                        {
                            Logger.Error($"[CON] Error handling remote client: {(PluginStart.Instance.Config.Debug ? ex.ToString() : ex.Message)}");
                        }
                    }

                    if (processedUpTo == 0)
                    {
                        if (sb.Length > MaxAccumulatedDataSize / 2)
                        {
                            sb.Remove(0, sb.Length - (MaxAccumulatedDataSize / 2));
                        }
                    }
                    else
                    {
                        sb.Remove(0, processedUpTo);
                    }
                }
                catch (OperationCanceledException)
                {
                    if (cancelSource.IsCancellationRequested)
                        return;

                    Logger.Warn("[CON] Read timed out; dropping connection.");
                    lock (_lock)
                    {
                        client?.Dispose();
                        Client = null;
                        readyToSend = false;
                    }
                    return;
                }
                catch (Exception ex) when (ex is not OperationCanceledException)
                {
                    Logger.Error($"[CON] Receive error: {ex.Message}");
                    lock (_lock)
                    {
                        client?.Dispose();
                        Client = null;
                        readyToSend = false;
                    }
                    return;
                }
            }
        }

        private async Task MaintainConnection(CancellationTokenSource cancelSource)
        {
            while (!cancelSource.IsCancellationRequested)
            {
                bool isConnected;
                lock (_lock)
                {
                    isConnected = Connected;
                }

                if (isConnected)
                {
                    await Task.Delay(1000, cancelSource.Token).ConfigureAwait(false);
                    continue;
                }

                if (!firstrestart)
                    firstrestart = true;
                else
                    Logger.Warn($"[CON] {PluginStart._lang.AttemptingReconnect}");

                if (!IPAddress.TryParse(PluginStart.Instance.Config.IPAddress, out IPAddress addr))
                {
                    Logger.Error($"[CON] {PluginStart._lang.InvalidIPAddress}, {PluginStart.Instance.Config.IPAddress}");
                    try { await Task.Delay(RetryInterval, cancelSource.Token).ConfigureAwait(false); } catch { }
                    continue;
                }

                lock (_lock)
                {
                    try { Timing.KillCoroutines(updateBotStatusCoroutine); } catch { }
                    try { Timing.KillCoroutines(updateBotTopicCoroutine); } catch { }

                    try
                    {
                        Client?.Close();
                        Client?.Dispose();
                    }
                    catch { }

                    Client = new TcpClient();
                }

                Logger.Warn($"[CON] {string.Format(PluginStart._lang.ConnectingTo, addr, PluginStart.Instance.Config.Port)}");

                try
                {
                    var connectTask = Client.ConnectAsync(addr, PluginStart.Instance.Config.Port);
                    var timeoutTask = Task.Delay(ConnectTimeoutMs, cancelSource.Token);
                    var completed = await Task.WhenAny(connectTask, timeoutTask).ConfigureAwait(false);

                    if (completed != connectTask)
                        throw new SocketException((int)SocketError.TimedOut);

                    lock (_lock)
                    {
                        if (Client == null)
                            throw new IOException("Client disposed during connect.");
                        readyToSend = true;
                    }

                    Logger.Info($"[CON] {string.Format(PluginStart._lang.SuccessfullyConnected, Endpoint?.Address, Endpoint?.Port)}");

                    try
                    {
                        lock (_lock)
                        {
                            try
                            {
                                if (!updateBotStatusCoroutine.IsRunning)
                                    updateBotStatusCoroutine = Timing.RunCoroutine(UpdateBotStots(cancelSource));
                            }
                            catch { updateBotStatusCoroutine = Timing.RunCoroutine(UpdateBotStots(cancelSource)); }

                            try
                            {
                                if (!updateBotTopicCoroutine.IsRunning)
                                    updateBotTopicCoroutine = Timing.RunCoroutine(UpdateBotTopic(cancelSource));
                            }
                            catch { updateBotTopicCoroutine = Timing.RunCoroutine(UpdateBotTopic(cancelSource)); }
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.Warn($"[CON] Failed to start coroutines safely: {ex.Message}");
                    }

                    _ = TransmitAsync(new RemoteClient(ActionType.Log, ChannelType.GameEvents, PluginStart._lang.ServerConnected));

                    lock (_lock)
                    {
                        _listenTask = Task.Run(() => ListenAsync(cancelSource));
                    }
                }
                catch (IOException ioEx)
                {
                    Logger.Error($"[CON] {string.Format(PluginStart._lang.ReceivingDataError, PluginStart.Instance.Config.Debug ? ioEx.ToString() : ioEx.Message)}");
                }
                catch (SocketException sockEx)
                {
                    Logger.Warn($"[CON] {string.Format(PluginStart._lang.ConnectingError, PluginStart.Instance.Config.Debug ? sockEx.ToString() : sockEx.Message)}");
                }
                catch (OperationCanceledException) when (cancelSource.IsCancellationRequested)
                {
                    return;
                }
                catch (Exception ex) when (ex is not OperationCanceledException)
                {
                    Logger.Error($"[CON] {string.Format(PluginStart._lang.UpdatingConnectionError, PluginStart.Instance.Config.Debug ? ex.ToString() : ex.Message)}");
                }
                lock (_lock)
                {
                    readyToSend = true; 
                }
                if (cancelSource.IsCancellationRequested)
                    return;

                if (!Connected || Client == null)
                {
                    Logger.Warn($"[CON] {PluginStart._lang.RetryingConnectionIn} {RetryInterval.TotalSeconds}s...");
                }

                try
                {
                    await Task.Delay(RetryInterval, cancelSource.Token).ConfigureAwait(false);
                }
                catch (TaskCanceledException) { }
                try { await Task.Delay(RetryInterval, cancelSource.Token).ConfigureAwait(false); } catch { }
            }
        }

        private IEnumerator<float> UpdateBotTopic(CancellationTokenSource cancelSource)
        {
            while (!cancelSource.IsCancellationRequested)
            {
                lock (_lock)
                {
                    if (!readyToSend || !Connected)
                    {
                        yield return Timing.WaitForSeconds(15f);
                        continue;
                    }
                }

                try
                {
                    int aliveHumans = Player.List.Count(player => player.IsAlive && player.IsHuman);
                    int aliveScps = Player.List.Count(x => x.IsSCP);
                    string warheadText = Warhead.IsDetonated
                        ? PluginStart._lang.WarheadHasBeenDetonated
                        : Warhead.IsDetonationInProgress
                            ? PluginStart._lang.WarheadIsCountingToDetonation
                            : PluginStart._lang.WarheadHasntBeenDetonated;

                    _ = TransmitAsync(new RemoteClient(ActionType.UpdateChannelActivity,
                        $"{string.Format(PluginStart._lang.PlayersOnline, Server.PlayerCount, Server.MaxPlayers)}. " +
                        $"{string.Format(PluginStart._lang.RoundDuration, Round.Duration)}. " +
                        $"{string.Format(PluginStart._lang.AliveHumans, aliveHumans)}. " +
                        $"{string.Format(PluginStart._lang.AliveScps, aliveScps)}. {warheadText} " +
                        $"IP: {Server.IpAddress}:{Server.Port} TPS: {Server.Tps}"));
                }
                catch (Exception ex)
                {
                    Logger.Error($"[CON] Topic update failed: {ex.Message}");
                }

                yield return Timing.WaitForSeconds(15f);
            }
        }

        private IEnumerator<float> UpdateBotStots(CancellationTokenSource cancelSource)
        {
            while (!cancelSource.IsCancellationRequested)
            {
                lock (_lock)
                {
                    if (!readyToSend || !Connected)
                    {
                        yield return Timing.WaitForSeconds(3f);
                        continue;
                    }
                }

                try
                {
                    int adminscount = Player.List.Count(player => player.HasPermission(PlayerPermissions.AdminChat));
                    _ = TransmitAsync(new RemoteClient(ActionType.UpdateActivity,
                        $"[Players: {Server.PlayerCount}/{Server.MaxPlayers} ; Admins: {adminscount}]"));
                }
                catch (Exception ex)
                {
                    Logger.Warn($"[CON] Status update failed: {ex.Message}");
                }

                yield return Timing.WaitForSeconds(3f);
            }
        }
    }
}