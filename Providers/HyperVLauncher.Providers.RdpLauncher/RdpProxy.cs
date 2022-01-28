using System;
using System.Collections.Generic;

using System.Net;
using System.Net.Sockets;

using System.Threading;
using System.Threading.Tasks;

using HyperVLauncher.Providers.Tracing;

namespace HyperVLauncher.Providers.RdpLauncher
{
    internal class RdpProxy
    {
        public string RemoteAddress { get; }

        public event EventHandler? OnDisconnect;
        
        private readonly Socket _clientSocket;
        private readonly UdpClient _udpListener;
        private readonly CancellationToken _cancellationToken;
        private readonly CancellationToken _socketCancellationToken;
        private readonly CancellationTokenSource _socketCancellationTokenSource;

        private bool _closing;

        private Socket? _serverSocket;
        private UdpClient? _serverUdpSocket;

        private readonly List<Task> _taskList = new();

        public RdpProxy(
            string remoteAddress,
            UdpClient udpListener,
            Socket clientSocket,
            CancellationToken cancellationToken)
        {
            RemoteAddress = remoteAddress;
            
            _udpListener = udpListener;
            _clientSocket = clientSocket;
            _cancellationToken = cancellationToken;

            _socketCancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(_cancellationToken);
            _socketCancellationToken = _socketCancellationTokenSource.Token;
        }

        public async Task Run()
        {
            Tracer.Info($"Starting RDP proxy with remote address {RemoteAddress}...");

            _serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            await _serverSocket.ConnectAsync(IPAddress.Parse(RemoteAddress), 3389, _cancellationToken);

            _serverUdpSocket = new UdpClient();
            _serverUdpSocket.Connect("192.168.86.42", 3389);

            Tracer.Debug($"Remote connection with {RemoteAddress} established.");

            Tracer.Debug($"Starting TCP proxy...");

            _taskList.Add(ProxyTcpSocket(_clientSocket, _serverSocket));
            _taskList.Add(ProxyTcpSocket(_serverSocket, _clientSocket));

            _taskList.Add(ProxyUdpSockets(_udpListener, _serverUdpSocket));
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

            var result = await clientUdpClient.ReceiveAsync(_socketCancellationToken);
            await serverUdpClient.SendAsync(result.Buffer, _socketCancellationToken);

            Tracer.Debug($"Starting UDP proxy...");

            _taskList.Add(ProxyUdpSocket(clientUdpClient, serverUdpClient, null));
            _taskList.Add(ProxyUdpSocket(serverUdpClient, clientUdpClient, result.RemoteEndPoint));
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
                _ = Close();
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

            Tracer.Info($"Closing RDP proxy with remote address {RemoteAddress}...");

            _socketCancellationTokenSource.Cancel();

            try
            {
                await Task.WhenAll(_taskList);
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

            Tracer.Info($"RDP proxy with {RemoteAddress} closed.");

            OnDisconnect?.Invoke(this, EventArgs.Empty);
        }
    }
}
