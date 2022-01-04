using HyperVLauncher.Contracts.Enums;

using HyperVLauncher.Providers.HyperV.Contracts.Enums;

namespace HyperVLauncher.Providers.HyperV.Mappers
{
    public static class WmiVmStateMapper
    {
        public static VmState ToVmState(this WmiVmState wmiVmState) => wmiVmState switch
        {
            WmiVmState.Started => VmState.Started,
            WmiVmState.Stopped => VmState.Stopped,
            WmiVmState.Saved => VmState.Saved,

            _ => VmState.Unknown
        };
    }
}
