using System;
using System.Threading.Tasks;
using System.Collections.Generic;

using System.IO;
using System.IO.Pipes;

using Newtonsoft.Json;

using HyperVLauncher.Contracts.Models;

namespace HyperVLauncher.Providers.Ipc
{
    public class IpcProvider
    {
        private readonly PipeStream _namedPipeStream;

        public IpcProvider(string pipeName, bool serverMode)
        {
            if (serverMode)
            {
                _namedPipeStream = CreateServerStream(pipeName);
            }
            else
            {
                _namedPipeStream = CreateClientStream(pipeName);
            }
        }

        private static NamedPipeServerStream CreateServerStream(string pipeName)
        {
            return new NamedPipeServerStream(pipeName, PipeDirection.InOut);
        }

        private static NamedPipeClientStream CreateClientStream(string pipeName)
        {
            return new NamedPipeClientStream(".", pipeName, PipeDirection.InOut);
        }

        public async Task Connect()
        {
            if (_namedPipeStream is not NamedPipeClientStream namedPipeClientStream)
            {
                throw new InvalidOperationException("Client stream not created.");
            }

            await namedPipeClientStream.ConnectAsync();
        }

        public async Task WaitForConnection()
        {
            if (_namedPipeStream is not NamedPipeServerStream namedPipeServerStream)
            {
                throw new InvalidOperationException("Server stream not created.");
            }

            await namedPipeServerStream.WaitForConnectionAsync();
        }

        public async Task SendMessage(IpcMessage ipcMessage)
        {
            using var streamWriter = new StreamWriter(_namedPipeStream)
            {
                AutoFlush = true
            };

            var jsonMessage = JsonConvert.SerializeObject(ipcMessage);

            await streamWriter.WriteAsync(jsonMessage);
        }

        public async IAsyncEnumerable<IpcMessage> ReadMessages()
        {
            using var streamReader = new StreamReader(_namedPipeStream);

            do
            {
                var jsonMessage = await streamReader.ReadLineAsync();

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
            while (true);
        }
    }
}