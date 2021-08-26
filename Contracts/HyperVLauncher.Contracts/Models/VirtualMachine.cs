
namespace HyperVLauncher.Contracts.Models
{
    public class VirtualMachine
    {
        public string Id { get; init; }
        public string Name { get; init; }

        public VirtualMachine(
            string id,
            string name)
        {
            Id = id;
            Name = name;
        }
    }
}
