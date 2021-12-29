using System.Collections.Generic;

using System.Net;
using System.Net.Sockets;

namespace HyperVLauncher.Providers.Tracing.Helpers
{
    public static class System
    {
        public static string GetLocalHostName()
        {
            return Dns.GetHostName();
        }

        public static IEnumerable<string> GetLocalIpAddresses()
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());

            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    yield return ip.ToString();
                }
            }
        }
    }
}
