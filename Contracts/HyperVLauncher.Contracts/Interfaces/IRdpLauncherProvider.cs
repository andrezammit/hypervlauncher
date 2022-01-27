using System.Threading.Tasks;

namespace HyperVLauncher.Contracts.Interfaces
{
    public interface IRdpLauncherProvider
    {
        Task StopListeners();
        Task StartListeners();
    }
}
