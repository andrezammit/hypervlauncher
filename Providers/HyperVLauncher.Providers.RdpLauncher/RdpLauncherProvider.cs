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

        private readonly List<RdpProxy> _rdpProxies = new();
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
            using var udpListener = new UdpClient(port);
            var tcpListener = new TcpListener(IPAddress.Any, port);

            tcpListener.Start();

            while (!_cancellationTokenSource.IsCancellationRequested)
            {
                var tcpClient = await tcpListener.AcceptTcpClientAsync(_cancellationTokenSource.Token);

                Tracer.Info($"New socket detected on port {port}. Peer address: {tcpClient.Client.RemoteEndPoint}");

                _hyperVProvider.StartVirtualMachine("31DD1747-ACFB-47FD-9434-7AAC6DA3442B");

                var rdpProxy = new RdpProxy(
                    "192.168.86.42",
                    udpListener,
                    tcpClient.Client,
                    _cancellationTokenSource.Token);

                rdpProxy.OnDisconnect += RdpProxy_OnDisconnect;

                _rdpProxies.Add(rdpProxy);
                
                _ = rdpProxy.Run();
            }

            var taskList = new List<Task>();
            
            lock (_rdpProxies)
            {
                foreach (var rdpProxy in _rdpProxies)
                {
                    taskList.Add(rdpProxy.Close());
                }
            }

            await Task.WhenAll(taskList);

            tcpListener.Stop();
        }

        private void RdpProxy_OnDisconnect(object? sender, EventArgs e)
        {
            if (sender is not RdpProxy rdpProxy)
            {
                throw new InvalidOperationException($"Invalid sender type {sender?.GetType().Name}.");
            }

            Tracer.Debug($"Processing RDP proxy ({rdpProxy.RemoteAddress}) disconnect event...");

            lock (_rdpProxies)
            {
                _rdpProxies.Remove(rdpProxy);
            }
        }
    }
}