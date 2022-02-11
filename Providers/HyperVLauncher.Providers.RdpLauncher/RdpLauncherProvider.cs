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
        private readonly ISettingsProvider _settingsProvider;

        private CancellationTokenSource _tcpListenerCancellationTokenSource;

        private readonly List<RdpProxy> _rdpProxies = new();
        private readonly List<Task> _tcpListenerTasks = new();
        private readonly CancellationTokenSource _cancellationTokenSource = new();

        public RdpLauncherProvider(
            IHyperVProvider hyperVProvider,
            ISettingsProvider settingsProvider)
        {
            _hyperVProvider = hyperVProvider;
            _settingsProvider = settingsProvider;

            _tcpListenerCancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(_cancellationTokenSource.Token);
        }

        public Task Start()
        {
            Tracer.Info("Starting listening for RDP connections...");

            return StartListeners();
        }

        public async Task Stop()
        {
            Tracer.Info("Stopping listening for RDP connections...");

            _cancellationTokenSource.Cancel();

            var taskList = new List<Task>();

            lock (_rdpProxies)
            {
                foreach (var rdpProxy in _rdpProxies)
                {
                    taskList.Add(rdpProxy.Close());
                }
            }

            Tracer.Debug($"Waiting for {taskList.Count} RDP proxies to stop...");

            await Task.WhenAll(taskList);

            await StopListeners();
        }

        public async Task RefreshListeners()
        {
            Tracer.Info("Refreshing RDP connection listeners...");

            await StopListeners();
            await StartListeners();
        }

        private async Task StartListeners()
        {
            if (_tcpListenerCancellationTokenSource.IsCancellationRequested)
            {
                _tcpListenerCancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(_cancellationTokenSource.Token);
            }

            var appSettings = await _settingsProvider.Get(true);

            foreach (var shortcut in appSettings.Shortcuts)
            {
                if (shortcut.RdpTriggerEnabled)
                {
                    var tcpListenerTask = StartTcpListener(
                        shortcut.VmId,
                        shortcut.RdpPort,
                        _tcpListenerCancellationTokenSource.Token);

                    _tcpListenerTasks.Add(tcpListenerTask);
                }
            }
        }

        private async Task StopListeners()
        {
            _tcpListenerCancellationTokenSource.Cancel();

            await Task.WhenAll(_tcpListenerTasks);
        }

        public async Task StartTcpListener(
            string vmId, 
            int port,
            CancellationToken cancellationToken)
        {
            Tracer.Debug($"Starting TCP listener for virtual machine {vmId} on port {port}...");

            try
            {
                var tcpListener = new TcpListener(IPAddress.Any, port);

                tcpListener.Start();

                await ProcessTcpSockets(
                    vmId,
                    port,
                    tcpListener,
                    cancellationToken);

                Tracer.Debug($"Stopping TCP listener for virtual machine {vmId} on port {port}...");

                tcpListener.Stop();
            }
            catch (Exception ex)
            {
                Tracer.Error($"Failed to start listening for RDP virtual machine {vmId} connections on port {port}.", ex);
            }
        }

        private async Task ProcessTcpSockets(
            string vmId,
            int port,
            TcpListener tcpListener,
            CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    var tcpClient = await tcpListener.AcceptTcpClientAsync(cancellationToken);
                    
                    Tracer.Info($"New socket detected on port {port}. Peer address: {tcpClient.Client.RemoteEndPoint}");

                    _hyperVProvider.StartVirtualMachine(vmId);

                    string[]? ipAddresses = null;

                    for (var cnt = 0; cnt < 5; cnt++)
                    {
                        ipAddresses = _hyperVProvider.GetVirtualMachineIpAddresses(vmId);

                        if (ipAddresses is not null
                            && ipAddresses.Length > 0)
                        {
                            break;
                        }

                        await Task.Delay(1000, cancellationToken);
                    }

                    if (ipAddresses is null)
                    {
                        throw new InvalidOperationException("Failed to get virtual machine IP addresses.");
                    }

                    var rdpProxy = new RdpProxy(
                        vmId,
                        ipAddresses,
                        port,
                        tcpClient.Client,
                        _cancellationTokenSource.Token);

                    rdpProxy.OnDisconnect += RdpProxy_OnDisconnect;

                    _rdpProxies.Add(rdpProxy);

                    _ = rdpProxy.Run();
                }
                catch (OperationCanceledException)
                {
                    // Swallow.
                }
                catch (Exception ex)
                {
                    Tracer.Warning($"Exception while listening on RDP proxy port {port}.", ex);
                }
            }
        }

        private void RdpProxy_OnDisconnect(object? sender, EventArgs e)
        {
            if (sender is not RdpProxy rdpProxy)
            {
                throw new InvalidOperationException($"Invalid sender type {sender?.GetType().Name}.");
            }

            Tracer.Debug($"Processing RDP proxy ({rdpProxy.ConnectedIpAddress}) disconnect event...");

            lock (_rdpProxies)
            {
                _rdpProxies.Remove(rdpProxy);
            }
        }
    }
}