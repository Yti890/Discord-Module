using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace DiscordModuleBot.API.Services
{
    public class TcpServer : IDisposable
    {
        public const int ReceptionBuffer = 256;
        private readonly Bot bot;
        private TcpListener listener;
        private readonly HashSet<TcpClient> clients = new();
        private bool isDisposed;
        public event EventHandler<ReceivedFullEventArgs> ReceivedFull;
        public event EventHandler<ReceivedPartialEventArgs> ReceivedPartial;
        public event EventHandler<SendingErrorEventArgs> SendingError;
        public event EventHandler<ReceivingErrorEventArgs> ReceivingError;
        public event EventHandler<SentEventArgs> Sent;
        public event EventHandler<ConnectingEventArgs> Connecting;
        public event EventHandler<ConnectingErrorEventArgs> ConnectingError;
        public event EventHandler Connected;
        public event EventHandler<UpdatingConnectionErrorEventArgs> UpdatingConnectionError;
        public event EventHandler<TerminatedEventArgs> Terminated;
        public IPEndPoint EndPoint { get; private set; }
        public JsonSerializerSettings JsonSerializerSettings { get; } = new()
        {
            ContractResolver = new CamelCasePropertyNamesContractResolver(),
            TypeNameHandling = TypeNameHandling.Objects,
        };
        public TcpServer(string ip, ushort port, Bot bot)
        {
            this.bot = bot;
            EndPoint = new IPEndPoint(IPAddress.Parse(ip), port);
            listener = new TcpListener(EndPoint);
        }
        public async Task Start(CancellationToken cancellationToken = default)
        {
            listener.Start();
            OnConnecting(this, new ConnectingEventArgs(EndPoint.Address, (ushort)EndPoint.Port));
            _ = bot.Client.SetStatusAsync(Discord.UserStatus.DoNotDisturb);
            _ = bot.Client.SetActivityAsync(new Discord.Game("Waiting for connection..."));
            try
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    var client = await listener.AcceptTcpClientAsync(cancellationToken);
                    lock (clients) clients.Add(client);
                    OnConnected(this, EventArgs.Empty);
                    _ = HandleClientAsync(client, cancellationToken);
                }
            }
            catch (Exception ex)
            {
                OnUpdatingConnectionError(this, new UpdatingConnectionErrorEventArgs(ex));
            }
            finally
            {
                OnTerminated(this, new TerminatedEventArgs(Task.CompletedTask));
            }
        }
        public async ValueTask SendAsync<T>(T data, CancellationToken cancellationToken = default)
        {
            var json = JsonConvert.SerializeObject(data, JsonSerializerSettings);
            var bytes = Encoding.UTF8.GetBytes(json + '\0');
            List<TcpClient> deadClients = new();
            lock (clients)
            {
                foreach (var client in clients.ToArray())
                {
                    try
                    {
                        client.GetStream().WriteAsync(bytes, 0, bytes.Length, cancellationToken);
                        OnSent(this, new SentEventArgs(json, bytes.Length));
                    }
                    catch (Exception ex)
                    {
                        deadClients.Add(client);
                        OnSendingError(this, new SendingErrorEventArgs(ex));
                    }
                }
                foreach (var dc in deadClients)
                {
                    clients.Remove(dc);
                    dc.Dispose();
                }
            }
        }
        private async Task HandleClientAsync(TcpClient client, CancellationToken cancellationToken)
        {
            var buffer = new byte[ReceptionBuffer];
            var sb = new StringBuilder();
            try
            {
                var stream = client.GetStream();
                while (!cancellationToken.IsCancellationRequested)
                {
                    int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length, cancellationToken);
                    if (bytesRead == 0) break;
                    var text = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                    sb.Append(text);
                    var data = sb.ToString();
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
                                OnReceivedFull(this, new ReceivedFullEventArgs(message, message.Length));
                            }
                            catch
                            {
                                OnReceivedPartial(this, new ReceivedPartialEventArgs(message, message.Length));
                            }
                        }
                    }
                    sb.Clear();
                    sb.Append(data);
                }
            }
            catch (Exception ex)
            {
                OnReceivingError(this, new ReceivingErrorEventArgs(ex));
            }
            finally
            {
                lock (clients) clients.Remove(client);
                client.Dispose();
            }
        }
        public void Close() => Dispose();
        public void Dispose()
        {
            if (isDisposed) return;
            lock (clients)
            {
                foreach (var c in clients) c.Dispose();
                clients.Clear();
            }
            listener.Stop();
            isDisposed = true;
        }
        protected virtual void OnReceivedFull(object sender, ReceivedFullEventArgs ev) => ReceivedFull?.Invoke(sender, ev);
        protected virtual void OnReceivedPartial(object sender, ReceivedPartialEventArgs ev) => ReceivedPartial?.Invoke(sender, ev);
        protected virtual void OnSendingError(object sender, SendingErrorEventArgs ev) => SendingError?.Invoke(sender, ev);
        protected virtual void OnReceivingError(object sender, ReceivingErrorEventArgs ev) => ReceivingError?.Invoke(sender, ev);
        protected virtual void OnSent(object sender, SentEventArgs ev) => Sent?.Invoke(sender, ev);
        protected virtual void OnConnecting(object sender, ConnectingEventArgs ev) => Connecting?.Invoke(sender, ev);
        protected virtual void OnConnectingError(object sender, ConnectingErrorEventArgs ev) => ConnectingError?.Invoke(sender, ev);
        protected virtual void OnConnected(object sender, EventArgs ev) => Connected?.Invoke(sender, ev);
        protected virtual void OnUpdatingConnectionError(object sender, UpdatingConnectionErrorEventArgs ev) => UpdatingConnectionError?.Invoke(sender, ev);
        protected virtual void OnTerminated(object sender, TerminatedEventArgs ev) => Terminated?.Invoke(sender, ev);
    }
    public class ReceivedFullEventArgs : EventArgs
    {
        public string Data { get; }
        public int Bytes { get; }
        public ReceivedFullEventArgs(string data, int bytes)
        {
            Data = data;
            Bytes = bytes;
        }
    }
    public class ReceivedPartialEventArgs : EventArgs
    {
        public string Data { get; }
        public int Bytes { get; }
        public ReceivedPartialEventArgs(string data, int bytes)
        {
            Data = data;
            Bytes = bytes;
        }
    }
    public class SendingErrorEventArgs : EventArgs
    {
        public Exception Exception { get; }
        public SendingErrorEventArgs(Exception exception)
        {
            Exception = exception;
        }
    }
    public class ReceivingErrorEventArgs : EventArgs
    {
        public Exception Exception { get; }
        public ReceivingErrorEventArgs(Exception exception)
        {
            Exception = exception;
        }
    }
    public class SentEventArgs : EventArgs
    {
        public string Json { get; }
        public int Bytes { get; }
        public SentEventArgs(string json, int bytes)
        {
            Json = json;
            Bytes = bytes;
        }
    }
    public class ConnectingEventArgs : EventArgs
    {
        public IPAddress Address { get; }
        public ushort Port { get; }
        public ConnectingEventArgs(IPAddress address, ushort port)
        {
            Address = address;
            Port = port;
        }
    }
    public class ConnectingErrorEventArgs : EventArgs
    {
        public Exception Exception { get; }
        public ConnectingErrorEventArgs(Exception exception)
        {
            Exception = exception;
        }
    }
    public class UpdatingConnectionErrorEventArgs : EventArgs
    {
        public Exception Exception { get; }
        public UpdatingConnectionErrorEventArgs(Exception exception)
        {
            Exception = exception;
        }
    }
    public class TerminatedEventArgs : EventArgs
    {
        public Task Task { get; }
        public TerminatedEventArgs(Task task)
        {
            Task = task;
        }
    }
}