using HyperVLauncher.Contracts.Enums;
using System;

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

        private static void Trace(TraceLevel traceLevel, string message)
        {
            if (string.IsNullOrEmpty(_defaultTracerName))
            {
                throw new InvalidOperationException("Default tracer name not set.");
            }

            if (_defaultTracer is null)
            {
                _defaultTracer = new(_defaultTracerName);
            }

            _defaultTracer.Trace(traceLevel, message);
        }

        public static void Debug(string message)
        {
            Trace(TraceLevel.Debug, message);
        }

        public static void Info(string message)
        {
            Trace(TraceLevel.Info, message);
        }

        public static void Warning(string message)
        {
            Trace(TraceLevel.Warning, message);
        }

        public static void Error(string message)
        {
            Trace(TraceLevel.Error, message);
        }
    }
}