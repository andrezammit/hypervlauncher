using System;
using System.Collections.Generic;

using System.Threading;
using System.Threading.Tasks;

using NetMQ;
using NetMQ.Sockets;

using HyperVLauncher.Contracts.Enums;
using HyperVLauncher.Contracts.Constants;
using HyperVLauncher.Contracts.Interfaces;

using HyperVLauncher.Providers.Tracing;

namespace HyperVLauncher.Providers.Ipc
{
    public class MonitorIpcProvider : IpcProvider, IMonitorIpcProvider
    {
        private readonly int _port;

        public MonitorIpcProvider(int port) 
        {
            _port = port;
        }

        protected override NetMQSocket CreatePublisherSocket()
        {
            var publisherSocket = new PublisherSocket();

            publisherSocket.Bind($"tcp://127.0.0.1:{_port}");
            publisherSocket.Options.SendHighWatermark = 1000;

            return publisherSocket;
        }

        public Task RunIpcProxy(CancellationToken cancellationToken)
        {
            cancellationToken.Register(() => NetMQConfig.Cleanup(false));

            var taskList = new List<Task>
            {
                Task.Run(() => ProxyMessages(GeneralConstants.MonitorIpcProxyPort, cancellationToken), cancellationToken),
            };

            return Task.WhenAll(taskList);
        }

        private void ProxyMessages(
            int port,
            CancellationToken cancellationToken)
        {
            Tracer.Debug($"Starting IPC proxy on port {port}...");

            using var subscriberSocket = CreateSubscriberSocket(port, new List<IpcTopic> { IpcTopic.All });

            do
            {
                try
                {
                    var topic = subscriberSocket.ReceiveFrameString();
                    var jsonMessage = subscriberSocket.ReceiveFrameString();

                    if (jsonMessage is null)
                    {
                        continue;
                    }

                    _publisherSocket
                        .SendMoreFrame(topic)
                        .SendFrame(jsonMessage);
                }
                catch (Exception ex)
                {
                    if (!cancellationToken.IsCancellationRequested)
                    {
                        Tracer.Warning($"Failed to proxy message on port {port}.", ex);
                    }
                }
            }
            while (!cancellationToken.IsCancellationRequested);
        }
    }
}