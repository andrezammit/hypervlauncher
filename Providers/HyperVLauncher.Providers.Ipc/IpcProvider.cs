using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

using System.Threading;
using System.Threading.Tasks;

using System.IO;
using System.IO.Pipes;

using Newtonsoft.Json;

using HyperVLauncher.Contracts.Enums;
using HyperVLauncher.Contracts.Models;
using HyperVLauncher.Contracts.Interfaces;

namespace HyperVLauncher.Providers.Ipc
{
    public class IpcProvider : IIpcProvider
    {
        private readonly string _pipeName;

        public IpcProvider(string pipeName)
        {
            _pipeName = pipeName;
        }

        private static NamedPipeServerStream CreateServerStream(string pipeName)
        {
            return new NamedPipeServerStream(pipeName, PipeDirection.InOut, 1, PipeTransmissionMode.Byte, PipeOptions.Asynchronous);
        }

        private static NamedPipeClientStream CreateClientStream(string pipeName)
        {
            return new NamedPipeClientStream(".", pipeName, PipeDirection.InOut, PipeOptions.Asynchronous);
        }

        public async Task SendMessage(IpcMessage ipcMessage)
        {
            try
            {
                using var namedPipeStream = CreateClientStream(_pipeName);

                await namedPipeStream.ConnectAsync(TimeSpan.FromSeconds(1).Milliseconds);

                using var streamWriter = new StreamWriter(namedPipeStream)
                {
                    AutoFlush = true
                };

                var jsonMessage = JsonConvert.SerializeObject(ipcMessage);

                await streamWriter.WriteAsync(jsonMessage);
            }
            catch
            {
                // Failed to send tray message.
            }
        }

        public Task SendReloadSettings()
        {
            var ipcMessage = new IpcMessage()
            {
                IpcCommand = IpcCommand.ReloadSettings
            };

            return SendMessage(ipcMessage);
        }

        public Task SendShowTrayMessage(string title, string message)
        {
            var ipcMessage = new IpcMessage()
            {
                IpcCommand = IpcCommand.ShowTrayMessage,
                Data = new TrayMessageData(title, message)
            };

            return SendMessage(ipcMessage);
        }

        public async IAsyncEnumerable<IpcMessage> ReadMessages(
            [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            do
            {
                using var namedPipeServerStream = CreateServerStream(_pipeName);

                await namedPipeServerStream.WaitForConnectionAsync(cancellationToken);

                using var streamReader = new StreamReader(namedPipeServerStream);

                var jsonMessage = await streamReader
                    .ReadLineAsync()
                    .WaitAsync(cancellationToken);

                if (jsonMessage is null)
                {
                    break;
                }

                var ipcMessage = JsonConvert.DeserializeObject<IpcMessage>(jsonMessage);

                if (ipcMessage is null)
                {
                    throw new InvalidOperationException("Invalid IPC message received.");
                }

                yield return ipcMessage;
            }
            while (!cancellationToken.IsCancellationRequested);
        }
    }
}