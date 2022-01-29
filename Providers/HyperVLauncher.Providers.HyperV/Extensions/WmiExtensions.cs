using System.Management;

namespace HyperVLauncher.Providers.HyperV.Extensions
{
    public static class WmiExtensions
    {
        public static ManagementObject? FirstOrDefault(this ManagementObjectCollection managementBaseObjects)
        {
            foreach (ManagementObject managementObject in managementBaseObjects)
            {
                return managementObject;
            }

            return null;
        }
    }
}
