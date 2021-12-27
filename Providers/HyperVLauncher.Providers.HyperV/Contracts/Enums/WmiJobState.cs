using System;

namespace HyperVLauncher.Providers.HyperV.Contracts.Enums
{
    public class WmiJobState
    {
        public const UInt16 New = 2;
        public const UInt16 Starting = 3;
        public const UInt16 Running = 4;
        public const UInt16 Suspended = 5;
        public const UInt16 ShuttingDown = 6;
        public const UInt16 Completed = 7;
        public const UInt16 Terminated = 8;
        public const UInt16 Killed = 9;
        public const UInt16 Exception = 10;
        public const UInt16 Service = 11;
    }
}
