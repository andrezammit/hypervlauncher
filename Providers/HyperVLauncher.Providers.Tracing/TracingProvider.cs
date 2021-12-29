using System;
using System.Diagnostics;
using System.IO;

using HyperVLauncher.Contracts.Enums;
using HyperVLauncher.Contracts.Interfaces;

using TraceLevel = HyperVLauncher.Contracts.Enums.TraceLevel;

namespace HyperVLauncher.Providers.Tracing
{
    public class TracingProvider : ITracingProvider
    {
        public static string? TracingPath { get; private set; }

        public string Name { get; private set; }
        public string FilePath { get; private set; }

        public TracingProvider(string name)
        {
            if (string.IsNullOrEmpty(TracingPath))
            {
                throw new InvalidOperationException("Trace folder not set.");
            }

            if (string.IsNullOrEmpty(name))
            {
                throw new InvalidOperationException("Trace name not set.");
            }

            Name = name;
            FilePath = Path.Combine(TracingPath, $"{Name}.log");

            TraceSystemInfo();
        }

        public static void Init(string tracingPath, string defaultTracerName)
        {
            TracingPath = tracingPath;
            Tracer.Name = defaultTracerName;
        }

        private void TraceSystemInfo()
        {
            Debug("System Information");
            Debug("------------------");
            Debug($"OS: {Environment.OSVersion.VersionString}");
            Debug($".Net version: {Environment.Version}");
            Debug($"Host name: {Helpers.System.GetLocalHostName()}");
            Debug($"IP addresses: {string.Join(", ", Helpers.System.GetLocalIpAddresses())}");
            Debug("------------------");
        }

        public void Debug(string message, Exception? exception = null)
        {
            Trace(TraceLevel.Debug, message, exception);
        }

        public void Info(string message, Exception? exception = null)
        {
            Trace(TraceLevel.Info, message, exception);
        }

        public void Warning(string message, Exception? exception = null)
        {
            Trace(TraceLevel.Warning, message, exception);
        }

        public void Error(string message, Exception? exception = null)
        {
            Trace(TraceLevel.Error, message, exception);
        }

        public void Trace(TraceLevel traceLevel, string message, Exception? exception = null)
        {
            var formattedMessage = $"[{DateTime.UtcNow} - {Environment.ProcessId} - {traceLevel}]\t{message}";

            if (exception is not null)
            {
                formattedMessage += $"\nException: {exception.Message}\n{exception.StackTrace}";

                var innerException = exception.InnerException;

                while (innerException is not null)
                {
                    formattedMessage += $"\nInner Exception: {innerException.Message}\n{innerException.StackTrace}";
                    innerException = innerException.InnerException;
                }
            }

            Console.WriteLine(formattedMessage);

            lock (this)
            {
                using var streamWriter = new StreamWriter(FilePath, true);
                streamWriter.WriteLine(formattedMessage);
            }
        }
    }
}