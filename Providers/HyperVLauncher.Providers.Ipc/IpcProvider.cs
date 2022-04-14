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

        protected readonly NetMQSocket _publisherSocket;

        public IpcProvider()
        {
            _publisherSocket = CreatePublisherSocket();
        }

        protected static SubscriberSocket CreateSubscriberSocket(int port, IList<IpcTopic> topics)
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

        protected virtual NetMQSocket CreatePublisherSocket()
        {
            var requestSocket = new RequestSocket();

            requestSocket.Connect($"tcp://127.0.0.1:{GeneralConstants.MonitorIpcProxyPort}");
            requestSocket.Options.Linger = TimeSpan.Zero;

            return requestSocket;
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
                        Tracer.Warning($"Failed to read message on port {GeneralConstants.MonitorIpcPort}.", ex);
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