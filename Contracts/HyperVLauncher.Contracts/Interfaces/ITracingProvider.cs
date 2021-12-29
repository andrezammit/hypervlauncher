using System;

using HyperVLauncher.Contracts.Enums;

namespace HyperVLauncher.Contracts.Interfaces
{
    public interface ITracingProvider
    {
        string Name { get; }
        string FilePath { get; }

        void Info(string message, Exception? exception = null);
        void Debug(string message, Exception? exception = null);
        void Error(string message, Exception? exception = null);
        void Warning(string message, Exception? exception = null);

        void Trace(TraceLevel traceLevel, string message, Exception? exception = null);
    }
}
