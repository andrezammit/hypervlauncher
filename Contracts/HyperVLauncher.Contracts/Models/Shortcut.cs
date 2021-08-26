
namespace HyperVLauncher.Contracts.Models
{
    public class Shortcut
    {
        public string Id { get; init; }
        public string Name { get; init; }
        public string VmName { get; set; }

        public Shortcut(
            string id,
            string name,
            string vmName)
        {
            Id = id;
            Name = name;
            VmName = vmName;
        }
    }
}
