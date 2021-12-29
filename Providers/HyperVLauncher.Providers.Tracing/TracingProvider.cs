using System;
using System.IO;

using HyperVLauncher.Contracts.Enums;
using HyperVLauncher.Contracts.Interfaces;

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

        public void Debug(string message)
        {
            Trace(TraceLevel.Debug, message);
        }

        public void Info(string message)
        {
            Trace(TraceLevel.Info, message);
        }

        public void Warning(string message)
        {
            Trace(TraceLevel.Warning, message);
        }

        public void Error(string message)
        {
            Trace(TraceLevel.Error, message);
        }

        public void Trace(TraceLevel traceLevel, string message)
        {
            var formattedMessage = $"[{DateTime.UtcNow} - {traceLevel}]\t{message}";

            lock (this)
            {
                using var streamWriter = new StreamWriter(FilePath, true);
                streamWriter.WriteLine(formattedMessage);
            }
        }
    }
}