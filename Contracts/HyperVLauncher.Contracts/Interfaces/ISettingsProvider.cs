using System.Threading.Tasks;

using HyperVLauncher.Contracts.Models;

namespace HyperVLauncher.Contracts.Interfaces
{
    public interface ISettingsProvider
    {
        Task Save();
        Task<AppSettings> Get(bool forceReload = false);
    }
}
