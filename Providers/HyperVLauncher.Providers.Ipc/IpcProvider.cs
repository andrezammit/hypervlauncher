using System;
using System.Collections.Generic;

using System.Threading;
using System.Threading.Tasks;

using NetMQ;
using NetMQ.Sockets;

using Newtonsoft.Json;

using HyperVLauncher.Contracts.Enums;
using HyperVLauncher.Contracts.Models;
using HyperVLauncher.Providers.Tracing;
using HyperVLauncher.Contracts.Interfaces;

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

        private static SubscriberSocket CreateSubscriberSocket(int port)
        {
            var subscriberSocket = new SubscriberSocket();
            
            subscriberSocket.Connect($"tcp://127.0.0.1:{port}");
            subscriberSocket.Options.ReceiveHighWatermark = 1000;

            subscriberSocket.SubscribeToAnyTopic();

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
            var taskList = new List<Task>
            {
                Task.Run(() => ProxyMessages(8871, cancellationToken), cancellationToken),
                Task.Run(() => ProxyMessages(8872, cancellationToken), cancellationToken)
            };

            return Task.WhenAll(taskList);
        }

        public Task SendMessage(IpcMessage ipcMessage)
        {
            try
            {
                var jsonMessage = JsonConvert.SerializeObject(ipcMessage);

                _publisherSocket.SendFrame(jsonMessage);
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

            return SendMessage(ipcMessage);
        }

        public Task SendShowMessageNotif(string title, string message)
        {
            var ipcMessage = new IpcMessage()
            {
                IpcCommand = IpcCommand.ShowMessageNotif,
                Data = new ShowMessageNotifData(title, message)
            };

            return SendMessage(ipcMessage);
        }

        public Task SendShowShortcutCreatedNotif(string vmId, string shortcutName)
        {
            var ipcMessage = new IpcMessage()
            {
                IpcCommand = IpcCommand.ShowShortcutCreatedNotif,
                Data = new ShortcutCreatedNotifData(vmId, shortcutName)
            };

            return SendMessage(ipcMessage);
        }

        public Task SendShowShortcutPromptNotif(string vmId, string vmName)
        {
            var ipcMessage = new IpcMessage()
            {
                IpcCommand = IpcCommand.ShowShortcutPromptNotif,
                Data = new ShortcutPromptNotifData(vmId, vmName)
            };

            return SendMessage(ipcMessage);
        }

        public Task SendBringToFront()
        {
            var ipcMessage = new IpcMessage()
            {
                IpcCommand = IpcCommand.BringToFront
            };

            return SendMessage(ipcMessage);
        }

        private void ProxyMessages(
            int port,
            CancellationToken cancellationToken)
        {
            using var subscriberSocket = CreateSubscriberSocket(port);

            cancellationToken.Register(() => subscriberSocket.Close());

            do
            {
                try
                {
                    subscriberSocket.Poll();

                    var jsonMessage = subscriberSocket.ReceiveFrameString();

                    if (jsonMessage is null)
                    {
                        continue;
                    }

                    _publisherSocket.SendFrame(jsonMessage);
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
            CancellationToken cancellationToken)
        {
            using var subscriberSocket = CreateSubscriberSocket(8870);

            cancellationToken.Register(() => subscriberSocket.Close());

            do
            {
                IpcMessage? ipcMessage = null;

                try
                {
                    subscriberSocket.Poll();

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