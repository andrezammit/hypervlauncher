using System.Threading;

namespace HyperVLauncher.Providers.Common
{
    public static class GenericHelpers
    {
        public static bool IsUniqueInstance(string mutexName)
        {
            _ = new Mutex(true, mutexName, out var createdNew);

            return createdNew;
        }
    }
}
