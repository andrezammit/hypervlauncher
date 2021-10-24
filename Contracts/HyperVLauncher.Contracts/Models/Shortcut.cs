using HyperVLauncher.Contracts.Enums;

namespace HyperVLauncher.Contracts.Models
{
    public class Shortcut
    {
        public string Id { get; init; }
        public string VmId { get; init; }
        public string Name { get; set; }

        public CloseAction CloseAction { get; set; }

        public Shortcut(
            string id,
            string vmId,
            string name,
            CloseAction closeAction = CloseAction.None)
        {
            Id = id;
            VmId = vmId;
            Name = name;
            CloseAction = closeAction;
        }
    }
}
