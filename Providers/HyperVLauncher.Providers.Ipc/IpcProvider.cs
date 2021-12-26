using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

using System.Threading;
using System.Threading.Tasks;

using System.IO;
using System.IO.Pipes;

using Newtonsoft.Json;

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
            return new NamedPipeServerStream(pipeName, PipeDirection.InOut);
        }

        private static NamedPipeClientStream CreateClientStream(string pipeName)
        {
            return new NamedPipeClientStream(".", pipeName, PipeDirection.InOut);
        }

        public async Task SendMessage(IpcMessage ipcMessage)
        {
            using var namedPipeStream = CreateClientStream(_pipeName);
            
            await namedPipeStream.ConnectAsync();

            using var streamWriter = new StreamWriter(namedPipeStream)
            {
                AutoFlush = true
            };

            var jsonMessage = JsonConvert.SerializeObject(ipcMessage);

            await streamWriter.WriteAsync(jsonMessage);
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