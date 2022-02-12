using System;
using System.Collections.Generic;

using System.Threading;
using System.Threading.Tasks;

using NetMQ;
using NetMQ.Sockets;

using Newtonsoft.Json;

using HyperVLauncher.Contracts.Enums;
using HyperVLauncher.Contracts.Models;
using HyperVLauncher.Contracts.Constants;
using HyperVLauncher.Contracts.Interfaces;

using HyperVLauncher.Providers.Tracing;

namespace HyperVLauncher.Providers.Ipc
{
    public class IpcProvider : IIpcProviderAll
    {
        private readonly int _port;

        private readonly PublisherSocket _publisherSocket;

        public IpcProvider(int port)
        {
            _port = port;

            _publisherSocket = CreatePublisherSocket(_port);
        }

        private static SubscriberSocket CreateSubscriberSocket(int port, IList<IpcTopic> topics)
        {
            var subscriberSocket = new SubscriberSocket();
            
            subscriberSocket.Connect($"tcp://127.0.0.1:{port}");
            subscriberSocket.Options.ReceiveHighWatermark = 1000;

            if (topics.Contains(IpcTopic.All))
            {
                subscriberSocket.SubscribeToAnyTopic();
            }
            else
            {
                foreach (var topic in topics)
                {
                    subscriberSocket.Subscribe(topic.ToString());
                }
            }

            return subscriberSocket;
        }

        private static PublisherSocket CreatePublisherSocket(int port)
        {
            var publisherSocket = new PublisherSocket();

            publisherSocket.Bind($"tcp://127.0.0.1:{port}");
            publisherSocket.Options.SendHighWatermark = 1000;

            return publisherSocket;
        }

        public Task RunIpcProxy(CancellationToken cancellationToken)
        {
            cancellationToken.Register(() => NetMQConfig.Cleanup(false));

            var taskList = new List<Task>
            {
                Task.Run(() => ProxyMessages(GeneralConstants.TrayIpcPort, cancellationToken), cancellationToken),
                Task.Run(() => ProxyMessages(GeneralConstants.ConsoleIpcPort, cancellationToken), cancellationToken)
            };

            return Task.WhenAll(taskList);
        }

        public Task SendMessage(IpcTopic topic, IpcMessage ipcMessage)
        {
            try
            {
                var jsonMessage = JsonConvert.SerializeObject(ipcMessage);

                _publisherSocket
                    .SendMoreFrame(topic.ToString())
                    .SendFrame(jsonMessage);
            }
            catch (TimeoutException)
            {
                // Swallow.
            }
            catch (Exception ex)
            {
                Tracer.Debug($"Failed to send tray message command: {ipcMessage.IpcCommand}.", ex);
            }

            return Task.CompletedTask;
        }

        public Task SendReloadSettings()
        {
            var ipcMessage = new IpcMessage()
            {
                IpcCommand = IpcCommand.ReloadSettings
            };

            return SendMessage(IpcTopic.Settings, ipcMessage);
        }

        public Task SendShowMessageNotif(string title, string message)
        {
            var ipcMessage = new IpcMessage()
            {
                IpcCommand = IpcCommand.ShowMessageNotif,
                Data = new ShowMessageNotifData(title, message)
            };

            return SendMessage(IpcTopic.Tray, ipcMessage);
        }

        public Task SendShowShortcutCreatedNotif(string vmId, string shortcutName)
        {
            var ipcMessage = new IpcMessage()
            {
                IpcCommand = IpcCommand.ShowShortcutCreatedNotif,
                Data = new ShortcutCreatedNotifData(vmId, shortcutName)
            };

            return SendMessage(IpcTopic.Tray, ipcMessage);
        }

        public Task SendShowShortcutPromptNotif(string vmId, string vmName)
        {
            var ipcMessage = new IpcMessage()
            {
                IpcCommand = IpcCommand.ShowShortcutPromptNotif,
                Data = new ShortcutPromptNotifData(vmId, vmName)
            };

            return SendMessage(IpcTopic.Tray, ipcMessage);
        }

        public Task SendBringToFront()
        {
            var ipcMessage = new IpcMessage()
            {
                IpcCommand = IpcCommand.BringToFront
            };

            return SendMessage(IpcTopic.LaunchPad, ipcMessage);
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

        public IEnumerable<IpcMessage> ReadMessages(
            IList<IpcTopic> topics,
            CancellationToken cancellationToken)
        {
            using var subscriberSocket = CreateSubscriberSocket(GeneralConstants.MonitorIpcPort, topics);

            cancellationToken.Register(() => NetMQConfig.Cleanup(false));

            do
            {
                IpcMessage? ipcMessage = null;

                try
                {
                    var topic = subscriberSocket.ReceiveFrameString();
                    var jsonMessage = subscriberSocket.ReceiveFrameString();

                    if (jsonMessage is null)
                    {
                        break;
                    }

                    ipcMessage = JsonConvert.DeserializeObject<IpcMessage>(jsonMessage);

                    if (ipcMessage is null)
                    {
                        throw new InvalidOperationException("Invalid IPC message received.");
                    }
                }
                catch (Exception ex)
                {
                    if (!cancellationToken.IsCancellationRequested)
                    {
                        Tracer.Warning($"Failed to proxy message on port {_port}.", ex);
                    }
                }

                if (ipcMessage is not null)
                {
                    yield return ipcMessage;
                }
            }
            while (!cancellationToken.IsCancellationRequested);
        }
    }
}