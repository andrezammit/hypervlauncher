using System;
using System.Collections.Generic;

using System.Net;
using System.Net.Sockets;

using System.Threading;
using System.Threading.Tasks;

using HyperVLauncher.Contracts.Models;
using HyperVLauncher.Contracts.Interfaces;

using HyperVLauncher.Providers.Common;
using HyperVLauncher.Providers.Tracing;

namespace HyperVLauncher.Providers.RdpLauncher
{
    public class RdpLauncherProvider : IRdpLauncherProvider
    {
        private readonly IHyperVProvider _hyperVProvider;
        private readonly ITrayIpcProvider _trayIpcProvider;
        private readonly ISettingsProvider _settingsProvider;

        private CancellationTokenSource _tcpListenerCancellationTokenSource;

        private readonly List<RdpProxy> _rdpProxies = new();
        private readonly List<Task> _tcpListenerTasks = new();
        private readonly CancellationTokenSource _cancellationTokenSource = new();

        public RdpLauncherProvider(
            IHyperVProvider hyperVProvider,
            ITrayIpcProvider trayIpcProvider,
            ISettingsProvider settingsProvider)
        {
            _hyperVProvider = hyperVProvider;
            _trayIpcProvider = trayIpcProvider;
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
                if (shortcut.RemoteTriggerEnabled)
                {
                    var tcpListenerTask = StartTcpListener(
                        shortcut,
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
            Shortcut shortcut,
            CancellationToken cancellationToken)
        {
            Tracer.Debug($"Starting TCP listener for shortcut {shortcut.Name} on port {shortcut.ListenPort}...");

            try
            {
                var tcpListener = new TcpListener(IPAddress.Any, shortcut.ListenPort);

                tcpListener.Start();

                await ProcessTcpSockets(
                    shortcut,
                    tcpListener,
                    cancellationToken);

                Tracer.Debug($"Stopping TCP listener for shortcut {shortcut.Name} on port {shortcut.ListenPort}...");

                tcpListener.Stop();
            }
            catch (Exception ex)
            {
                Tracer.Error($"Failed to start listening for shortcut {shortcut.Name} on port {shortcut.ListenPort}.", ex);
            }
        }

        private async Task ProcessTcpSockets(
            Shortcut shortcut,
            TcpListener tcpListener,
            CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    var tcpClient = await tcpListener.AcceptTcpClientAsync(cancellationToken);
                    
                    Tracer.Info($"New socket detected on port {shortcut.ListenPort}. Peer address: {tcpClient.Client.RemoteEndPoint}");

                    _hyperVProvider.StartVirtualMachine(shortcut.VmId);

                    string[]? ipAddresses = null;

                    for (var cnt = 0; cnt < 10; cnt++)
                    {
                        ipAddresses = _hyperVProvider.GetVirtualMachineIpAddresses(shortcut.VmId);

                        if (ipAddresses is not null
                            && ipAddresses.Length > 0)
                        {
                            break;
                        }

                        await Task.Delay(1000, cancellationToken);
                    }

                    if (ipAddresses is null || ipAddresses.Length == 0)
                    {
                        tcpClient.Client.Close();

                        throw new InvalidOperationException("Failed to get virtual machine IP addresses.");
                    }

                    var rdpProxy = new RdpProxy(
                        shortcut,
                        ipAddresses,
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
                    Tracer.Warning($"Exception while listening on RDP proxy port {shortcut.ListenPort}.", ex);
                }
            }
        }

        private void RdpProxy_OnDisconnect(object? sender, EventArgs e)
        {
            try
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

                Tracer.Debug($"RDP proxy connection duration: {rdpProxy.ConnectionDuration}");

                if (rdpProxy.ConnectionDuration > TimeSpan.FromSeconds(2))
                {
                    GenericHelpers.HandleShortcutExitBehaviour(
                            _hyperVProvider,
                            _trayIpcProvider,
                            rdpProxy.Shortcut)
                        .GetAwaiter()
                        .GetResult();
                }
            }
            catch (Exception ex)
            {
                Tracer.Warning("Exception while processing RDP proxy disconnect.", ex);
            }
        }
    }
}