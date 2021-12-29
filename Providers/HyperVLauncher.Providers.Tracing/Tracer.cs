using System;

using HyperVLauncher.Contracts.Enums;

namespace HyperVLauncher.Providers.Tracing
{
    public static class Tracer
    {
        private static string? _defaultTracerName;
        private static TracingProvider? _defaultTracer;

        public static string? Name 
        { 
            get
            {
                return _defaultTracerName;
            }
            set
            {
                _defaultTracer = null;
                _defaultTracerName = value;
            }
        }

        public static string? FilePath
        { 
            get
            {
                return _defaultTracer?.FilePath;
            }
        }

        private static void Trace(TraceLevel traceLevel, string message, Exception? exception = null)
        {
            if (string.IsNullOrEmpty(_defaultTracerName))
            {
                throw new InvalidOperationException("Default tracer name not set.");
            }

            if (_defaultTracer is null)
            {
                _defaultTracer = new(_defaultTracerName);
            }

            _defaultTracer.Trace(traceLevel, message, exception);
        }

        public static void Debug(string message, Exception? exception = null)
        {
            Trace(TraceLevel.Debug, message, exception);
        }

        public static void Info(string message, Exception? exception = null)
        {
            Trace(TraceLevel.Info, message, exception);
        }

        public static void Warning(string message, Exception? exception = null)
        {
            Trace(TraceLevel.Warning, message, exception);
        }

        public static void Error(string message, Exception? exception = null)
        {
            Trace(TraceLevel.Error, message, exception);
        }
    }
}