
namespace HyperVLauncher.Contracts.Models
{
    public class VirtualMachine
    {
        public string Name { get; init; }

        public VirtualMachine(string name)
        {
            Name = name;
        }
    }
}
