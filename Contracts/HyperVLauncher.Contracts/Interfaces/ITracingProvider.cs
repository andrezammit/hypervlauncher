using HyperVLauncher.Contracts.Enums;

namespace HyperVLauncher.Contracts.Interfaces
{
    public interface ITracingProvider
    {
        string Name { get; }
        string FilePath { get; }

        void Info(string message);
        void Debug(string message);
        void Error(string message);
        void Warning(string message);

        void Trace(TraceLevel traceLevel, string message);
    }
}
