using System;

using System.Collections.Generic;
using System.Collections.Concurrent;

using System.Net;
using System.Net.Sockets;

using System.Threading;
using System.Threading.Tasks;

using HyperVLauncher.Providers.Tracing;

namespace HyperVLauncher.Providers.RdpLauncher
{
    internal class RdpProxy
    {
        public string VmId { get; private set; }
        public string? ConnectedIpAddress { get; private set; }

        public event EventHandler? OnDisconnect;

        private readonly int _port;
        private readonly Socket _clientSocket;
        private readonly UdpClient _udpListener;
        private readonly string[] _remoteAddresses;

        private readonly List<Task> _proxyTasks = new();

        private readonly CancellationToken _cancellationToken;
        private readonly CancellationToken _socketCancellationToken;
        private readonly CancellationTokenSource _socketCancellationTokenSource;

        private bool _closing;

        private Socket? _serverSocket;
        private UdpClient? _serverUdpSocket;

        private static readonly ConcurrentDictionary<int, UdpClient> _udpListeners = new();

        public RdpProxy(
            string vmId,
            string[] remoteAddresses,
            int port,
            Socket clientSocket,
            CancellationToken cancellationToken)
        {
            VmId = vmId;

            if (_udpListeners.TryGetValue(port, out var currentUdpClient))
            {
                currentUdpClient.Dispose();
            }

            _udpListener = new UdpClient(port);
            _udpListeners[port] = _udpListener;

            _port = port;
            _clientSocket = clientSocket;
            _remoteAddresses = remoteAddresses;
            _cancellationToken = cancellationToken;

            _socketCancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(_cancellationToken);
            _socketCancellationToken = _socketCancellationTokenSource.Token;
        }

        public async Task Run()
        {
            Tracer.Info($"Starting RDP proxy with virtual machine {VmId}...");

            _serverSocket = new Socket(SocketType.Stream, ProtocolType.Tcp);
            
            foreach (var remoteAddress in _remoteAddresses)
            {
                try
                {
                    Tracer.Debug($"Attempting to connect to {remoteAddress}...");

                    var connectCancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(_cancellationToken);

                    connectCancellationTokenSource.CancelAfter(5000);

                    await _serverSocket.ConnectAsync(
                        IPAddress.Parse(remoteAddress), 
                        3389,
                        connectCancellationTokenSource.Token);

                    ConnectedIpAddress = remoteAddress;

                    Tracer.Debug($"Connected to {ConnectedIpAddress}.");

                    break;
                }
                catch (Exception ex)
                {
                    Tracer.Debug($"Failed to connect to {remoteAddress}.", ex);

                    _clientSocket.Close();
                }
            }

            if (ConnectedIpAddress is null)
            {
                _clientSocket.Close();

                throw new InvalidOperationException("Connected IP address is null.");
            }

            _serverUdpSocket = new UdpClient(AddressFamily.InterNetworkV6);
            _serverUdpSocket.Client.DualMode = true;

            _serverUdpSocket.Connect(ConnectedIpAddress, 3389);

            Tracer.Debug($"Starting TCP proxy...");

            _proxyTasks.Add(ProxyTcpSocket(_clientSocket, _serverSocket));
            _proxyTasks.Add(ProxyTcpSocket(_serverSocket, _clientSocket));

            _proxyTasks.Add(ProxyUdpSockets(_udpListener, _serverUdpSocket));
        }

        private async Task ProxyTcpSocket(Socket receiveSocket, Socket sendSocket)
        {
            try
            {
                while (!_socketCancellationTokenSource.IsCancellationRequested)
                {
                    var bytesToRead = Math.Min(1024, receiveSocket.Available);
                    var clientBuffer = new byte[bytesToRead];

                    await receiveSocket.ReceiveAsync(clientBuffer, SocketFlags.None, _socketCancellationToken);
                    await sendSocket.SendAsync(clientBuffer, SocketFlags.None, _socketCancellationToken);
                }
            }
            catch
            {
                _ = Close();
            }
        }

        private async Task ProxyUdpSockets(UdpClient clientUdpClient, UdpClient serverUdpClient)
        {
            Tracer.Debug($"Waiting for the first UDP packet...");

            var udpSocketCancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(_socketCancellationToken);
            udpSocketCancellationTokenSource.CancelAfter(TimeSpan.FromSeconds(10));

            var cancellatonToken = udpSocketCancellationTokenSource.Token;

            try
            {
                var result = await clientUdpClient.ReceiveAsync(cancellatonToken);
                await serverUdpClient.SendAsync(result.Buffer, _socketCancellationToken);

                Tracer.Debug($"Starting UDP proxy...");

                var clientSocketProxy = ProxyUdpSocket(clientUdpClient, serverUdpClient, null);
                var serverSocketProxy = ProxyUdpSocket(serverUdpClient, clientUdpClient, result.RemoteEndPoint);

                _proxyTasks.Add(clientSocketProxy);
                _proxyTasks.Add(serverSocketProxy);

                var udpProxyTasks = new List<Task>()
                {
                    clientSocketProxy,
                    serverSocketProxy
                };

                _ = Task.WhenAny(udpProxyTasks)
                    .ContinueWith((result) => Tracer.Debug($"UDP proxy stopped on port {_port}"));
            }
            catch (OperationCanceledException)
            {
                if (!_socketCancellationToken.IsCancellationRequested && 
                    udpSocketCancellationTokenSource.IsCancellationRequested)
                {
                    Tracer.Debug($"Giving up on UDP proxy on port {_port}.");
                }
            }
        }

        private async Task ProxyUdpSocket(UdpClient clientUdpClient, UdpClient serverUdpClient, IPEndPoint? remoteEndpoint)
        {
            try
            {
                while (!_socketCancellationToken.IsCancellationRequested)
                {
                    var result = await clientUdpClient.ReceiveAsync(_socketCancellationToken);

                    if (remoteEndpoint is null)
                    {
                        await serverUdpClient.SendAsync(result.Buffer, _socketCancellationToken);
                    }
                    else
                    {
                        await serverUdpClient.SendAsync(result.Buffer, remoteEndpoint, _socketCancellationToken);
                    }
                }
            }
            catch
            {
                // Swallow.
            }
        }

        public async Task Close()
        {
            lock (this)
            {
                if (_closing)
                {
                    return;
                }

                _closing = true;
            }

            Tracer.Info($"Closing RDP proxy with virtual machine {VmId}...");

            _socketCancellationTokenSource.Cancel();

            try
            {
                await Task.WhenAll(_proxyTasks);
            }
            catch (Exception ex) when (ex is OperationCanceledException)
            {
                // Swallow.
            }
            catch (Exception ex)
            {
                Tracer.Warning("Exception while closing RDP proxy.", ex);
            }

            _serverSocket?.Dispose();
            _serverUdpSocket?.Dispose();

            _udpListener.Dispose();

            Tracer.Info($"RDP proxy with virtual machine {VmId} closed.");

            OnDisconnect?.Invoke(this, EventArgs.Empty);
        }
    }
}
