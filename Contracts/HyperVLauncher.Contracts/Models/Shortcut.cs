
namespace HyperVLauncher.Contracts.Models
{
    public class Shortcut
    {
        public string Id { get; init; }
        public string VmId { get; init; }
        public string Name { get; set; }

        public Shortcut(
            string id,
            string vmId,
            string name)
        {
            Id = id;
            VmId = vmId;
            Name = name;
        }
    }
}
