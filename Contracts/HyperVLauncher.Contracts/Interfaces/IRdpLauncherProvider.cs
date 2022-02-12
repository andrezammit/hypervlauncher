using System.Threading.Tasks;

namespace HyperVLauncher.Contracts.Interfaces
{
    public interface IRdpLauncherProvider
    {
        Task Stop();
        Task Start();

        Task RefreshListeners();
    }
}
