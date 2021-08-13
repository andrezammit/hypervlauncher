
namespace HyperVLauncher.Contracts.Models
{
    public class Shortcut
    {
        public string Id { get; init; }
        public string VmName { get; set; }

        public Shortcut(
            string id,
            string vmName)
        {
            Id = id;
            VmName = vmName;
        }
    }
}
