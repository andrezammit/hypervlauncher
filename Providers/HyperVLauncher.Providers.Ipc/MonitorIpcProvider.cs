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
        private readonly PublisherSocket _publisherSocket;
             
        public MonitorIpcProvider(int port) 
        {
            _port = port;
            _publisherSocket = CreatePublisherSocket();
        }

        protected PublisherSocket CreatePublisherSocket()
        {
            var publisherSocket = new PublisherSocket();

            publisherSocket.Bind($"tcp://127.0.0.1:{_port}");
            publisherSocket.Options.SendHighWatermark = 1000;

            return publisherSocket;
        }

        private ResponseSocket CreateResponseSocket(int port)
        {
            var responseSocket = new ResponseSocket();
            responseSocket.Bind($"tcp://127.0.0.1:{port}");

            return responseSocket;
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

            using var responseSocket = CreateResponseSocket(port);

            do
            {
                try
                {
                    var topic = responseSocket.ReceiveFrameString();
                    var jsonMessage = responseSocket.ReceiveFrameString();

                    responseSocket.SendFrame("OK");

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