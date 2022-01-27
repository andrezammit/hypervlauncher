using System;
using System.Collections.Generic;

using System.Net;
using System.Net.Sockets;

using System.Threading;
using System.Threading.Tasks;

using HyperVLauncher.Contracts.Interfaces;

using HyperVLauncher.Providers.Tracing;

namespace HyperVLauncher.Providers.RdpLauncher
{
    public class RdpLauncherProvider : IRdpLauncherProvider
    {
        private readonly IHyperVProvider _hyperVProvider;

        private readonly List<Task> _tcpListenerTasks = new();
        private readonly CancellationTokenSource _cancellationTokenSource = new();

        public RdpLauncherProvider(
            IHyperVProvider hyperVProvider)
        {
            _hyperVProvider = hyperVProvider;
        }

        public Task StartListeners()
        {
            Tracer.Info("Starting listening for RDP connections...");

            var tcpListenerTask = StartTcpListener(3390);
            _tcpListenerTasks.Add(tcpListenerTask);

            return Task.CompletedTask;
        }

        public async Task StopListeners()
        {
            Tracer.Info("Stopping listening for RDP connections...");

            _cancellationTokenSource.Cancel();

            await Task.WhenAll(_tcpListenerTasks);
        }

        public async Task StartTcpListener(int port)
        {
            using var udpClient = new UdpClient(port);
            var tcpListener = new TcpListener(IPAddress.Any, port);

            tcpListener.Start();

            while (!_cancellationTokenSource.IsCancellationRequested)
            {
                var tcpClient = await tcpListener.AcceptTcpClientAsync(_cancellationTokenSource.Token);

                _hyperVProvider.StartVirtualMachine("31DD1747-ACFB-47FD-9434-7AAC6DA3442B");

                var serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                await serverSocket.ConnectAsync(IPAddress.Parse("192.168.86.42"), 3389, _cancellationTokenSource.Token);

                var socketCancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(_cancellationTokenSource.Token);

                var serverUdpSocket = new UdpClient();
                serverUdpSocket.Connect("192.168.86.42", 3389);

                _ = ProxyTcpSocket(tcpClient.Client, serverSocket, socketCancellationTokenSource);
                _ = ProxyTcpSocket(serverSocket, tcpClient.Client, socketCancellationTokenSource);

                _ = ProxyUdpSockets(udpClient, serverUdpSocket, socketCancellationTokenSource);
            }

            tcpListener.Stop();
        }

        private async Task ProxyTcpSocket(Socket receiveSocket, Socket sendSocket, CancellationTokenSource cancellationTokenSource)
        {
            try
            {
                while (!cancellationTokenSource.IsCancellationRequested)
                {
                    var bytesToRead = Math.Min(1024, receiveSocket.Available);
                    var clientBuffer = new byte[bytesToRead];

                    await receiveSocket.ReceiveAsync(clientBuffer, SocketFlags.None, _cancellationTokenSource.Token);
                    await sendSocket.SendAsync(clientBuffer, SocketFlags.None, _cancellationTokenSource.Token);
                }
            }
            catch (Exception ex)
            {
                cancellationTokenSource.Cancel();
            }
        }

        private async Task ProxyUdpSockets(UdpClient clientUdpClient, UdpClient serverUdpClient, CancellationTokenSource cancellationTokenSource)
        {
            var result = await clientUdpClient.ReceiveAsync(_cancellationTokenSource.Token);
            await serverUdpClient.SendAsync(result.Buffer, _cancellationTokenSource.Token);

            _ = ProxyUdpSocket(clientUdpClient, serverUdpClient, null, cancellationTokenSource);
            _ = ProxyUdpSocket(serverUdpClient, clientUdpClient, result.RemoteEndPoint, cancellationTokenSource);
        }

        private async Task ProxyUdpSocket(UdpClient clientUdpClient, UdpClient serverUdpClient, IPEndPoint? remoteEndpoint, CancellationTokenSource cancellationTokenSource)
        {
            while (!cancellationTokenSource.IsCancellationRequested)
            {
                var result = await clientUdpClient.ReceiveAsync(_cancellationTokenSource.Token);

                if (remoteEndpoint is null)
                {
                    await serverUdpClient.SendAsync(result.Buffer, _cancellationTokenSource.Token);
                }
                else
                {
                    await serverUdpClient.SendAsync(result.Buffer, remoteEndpoint, _cancellationTokenSource.Token);
                }
            }
        }
    }
}