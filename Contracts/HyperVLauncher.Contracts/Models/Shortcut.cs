using HyperVLauncher.Contracts.Enums;

namespace HyperVLauncher.Contracts.Models
{
    public class Shortcut
    {
        public bool RdpTriggerEnabled { get; set; }

        public int RdpPort { get; set; }

        public string Id { get; init; }
        public string VmId { get; init; }
        public string Name { get; set; }

        public CloseAction CloseAction { get; set; }

        public Shortcut(
            string id,
            string vmId,
            string name,
            bool rdpTriggerEnabled = false,
            int rdpPort = 0,
            CloseAction closeAction = CloseAction.None)
        {
            Id = id;
            VmId = vmId;
            Name = name;
            RdpPort = rdpPort;
            CloseAction = closeAction;
            RdpTriggerEnabled = rdpTriggerEnabled;
        }
    }
}
